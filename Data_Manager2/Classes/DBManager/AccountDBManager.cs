﻿using Data_Manager2.Classes.Events;
using Data_Manager2.Classes.DBTables;
using System.Collections.Generic;
using XMPP_API.Classes.Network;
using Windows.Security.Cryptography.Certificates;
using Logging;
using System.Threading;
using Thread_Save_Components.Classes.SQLite;
using Data_Manager2.Classes.DBManager.Omemo;

namespace Data_Manager2.Classes.DBManager
{
    public class AccountDBManager : AbstractDBManager
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public static readonly AccountDBManager INSTANCE = new AccountDBManager();
        private static readonly SemaphoreSlim ADD_ACCOUNT_SEMA = new SemaphoreSlim(1, 1);

        public delegate void AccountChangedHandler(AccountDBManager handler, AccountChangedEventArgs args);

        public event AccountChangedHandler AccountChanged;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 17/11/2017 Created [Fabian Sauter]
        /// </history>
        public AccountDBManager()
        {

        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        /// <summary>
        /// Adds the given XMPPAccount to the db or replaces it, if it already exists.
        /// </summary>
        /// <param name="account">The account which should get inserted or replaced.</param>
        public void setAccount(XMPPAccount account, bool triggerAccountChanged)
        {
            update(new AccountTable(account));
            Vault.storePassword(account);

            saveAccountConnectionConfiguration(account);

            if (account.omemoPreKeys != null)
            {
                OmemoSignalKeyDBManager.INSTANCE.setPreKeys(account.omemoPreKeys, account.getIdAndDomain());
            }
            else
            {
                OmemoSignalKeyDBManager.INSTANCE.deletePreKeys(account.getIdAndDomain());
            }
            if (account.omemoSignedPreKeyPair != null)
            {
                OmemoSignalKeyDBManager.INSTANCE.setSignedPreKey(account.omemoSignedPreKeyId, account.omemoSignedPreKeyPair, account.getIdAndDomain());
            }
            else
            {
                OmemoSignalKeyDBManager.INSTANCE.deleteSignedPreKey(account.omemoSignedPreKeyId, account.getIdAndDomain());
            }

            if (triggerAccountChanged)
            {
                AccountChanged?.Invoke(this, new AccountChangedEventArgs(account, false));
            }
        }

        /// <summary>
        /// Sets the disabled property of account and triggers the AccountChanged event.
        /// </summary>
        /// <param name="account">The XMPPAccount with updated disabled property.</param>
        public void setAccountDisabled(XMPPAccount account)
        {
            dB.Execute("UPDATE " + DBTableConsts.ACCOUNT_TABLE + " SET disabled = ? WHERE id = ?;", account.disabled, account.getIdAndDomain());
            AccountChanged?.Invoke(this, new AccountChangedEventArgs(account, false));
        }

        /// <summary>
        /// Returns the ConnectionOptionsTable matching the given accountId.
        /// </summary>
        /// <param name="accountId">The id of the AccountTable.</param>
        public ConnectionOptionsTable getConnectionOptionsTable(string accountId)
        {
            IList<ConnectionOptionsTable> list = dB.Query<ConnectionOptionsTable>(true, "SELECT * FROM " + DBTableConsts.CONNECTION_OPTIONS_TABLE + " WHERE accountId = ?;", accountId);
            if (list.Count < 1)
            {
                return null;
            }
            else
            {
                return list[0];
            }
        }

        /// <summary>
        /// Returns a list of IgnoredCertificateErrorTable object matching the given accountId.
        /// </summary>
        /// <param name="accountId">The id of the AccountTable.</param>
        public IList<IgnoredCertificateErrorTable> getIgnoredCertificateErrorTables(string accountId)
        {
            return dB.Query<IgnoredCertificateErrorTable>(true, "SELECT * FROM " + DBTableConsts.IGNORED_CERTIFICATE_ERROR_TABLE + " WHERE accountId = ?;", accountId);
        }

        /// <summary>
        /// Returns a list of all accounts from the DB.
        /// </summary>
        private List<AccountTable> getAccounts()
        {
            return dB.Query<AccountTable>(true, "SELECT * FROM " + DBTableConsts.ACCOUNT_TABLE + ";");
        }

        /// <summary>
        /// Returns how many accounts are currently in the DB.
        /// </summary>
        public int getAccountCount()
        {
            return getAccounts().Count;
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        /// <summary>
        /// Deletes the given account.
        /// </summary>
        /// <param name="account">The account to delete.</param>
        /// <param name="deleteAllKeys">Whether to delete all OMEMO keys.</param>
        public void deleteAccount(XMPPAccount account, bool triggerAccountChanged, bool deleteAllKeys)
        {
            dB.Execute("DELETE FROM " + DBTableConsts.ACCOUNT_TABLE + " WHERE id = ?;", account.getIdAndDomain());
            dB.Execute("DELETE FROM " + DBTableConsts.IGNORED_CERTIFICATE_ERROR_TABLE + " WHERE accountId = ?;", account.getIdAndDomain());
            dB.Execute("DELETE FROM " + DBTableConsts.CONNECTION_OPTIONS_TABLE + " WHERE accountId = ?;", account.getIdAndDomain());
            if (deleteAllKeys)
            {
                OmemoDeviceDBManager.INSTANCE.deleteAllForAccount(account.getIdAndDomain());
                OmemoSignalKeyDBManager.INSTANCE.deleteAllForAccount(account.getIdAndDomain());
            }
            else
            {
                OmemoSignalKeyDBManager.INSTANCE.deletePreKeys(account.getIdAndDomain());
                OmemoSignalKeyDBManager.INSTANCE.deleteSignedPreKey(account.omemoSignedPreKeyId, account.getIdAndDomain());
            }
            Vault.deletePassword(account);

            if (triggerAccountChanged)
            {
                AccountChanged?.Invoke(this, new AccountChangedEventArgs(account, true));
            }
        }

        /// <summary>
        /// Returns all XMPPAccounts from the db.
        /// Loads all passwords from their corresponding Vault objects.
        /// </summary>
        /// <returns>A list of XMPPAccount.</returns>
        public IList<XMPPAccount> loadAllAccounts()
        {
            IList<XMPPAccount> results = new List<XMPPAccount>();
            IList<AccountTable> accounts = getAccounts();
            for (int i = 0; i < accounts.Count; i++)
            {
                XMPPAccount acc = accounts[i].toXMPPAccount();
                Vault.loadPassword(acc);
                loadAccountConnectionConfiguration(acc);
                results.Add(acc);
            }
            return results;
        }

        /// <summary>
        /// Deletes the old account and inserts the new account.
        /// </summary>
        /// <param name="oldAccount">The old account, which should get deleted.</param>
        /// <param name="account">The account, that should get inserted.</param>
        public void replaceAccount(XMPPAccount oldAccount, XMPPAccount account)
        {
            ADD_ACCOUNT_SEMA.Wait();
            Logger.Debug("LOCKED replaceAccount()");
            dB.BeginTransaction();
            try
            {
                deleteAccount(oldAccount, true, false);
                setAccount(account, true);
            }
            catch (System.Exception e)
            {
                Logger.Error("Cough an exception in AccountDBManager.replaceAccount()", e);
            }
            finally
            {
                dB.Commit();
                ADD_ACCOUNT_SEMA.Release();
                Logger.Debug("UNLOCKED replaceAccount()");
            }
        }

        /// <summary>
        /// Loads the ConnectionConfiguration of the given XMPPAccount from the db.
        /// </summary>
        /// <param name="account">The XMPPAccount you want to load the ConnectionConfiguration for.</param>
        public void loadAccountConnectionConfiguration(XMPPAccount account)
        {
            // Load general options:
            ConnectionOptionsTable optionsTable = getConnectionOptionsTable(account.getIdAndDomain());
            if (optionsTable != null)
            {
                optionsTable.toConnectionConfiguration(account.connectionConfiguration);
            }

            // Load ignored certificate errors:
            IList<IgnoredCertificateErrorTable> ignoredCertificates = getIgnoredCertificateErrorTables(account.getIdAndDomain());
            if (ignoredCertificates != null)
            {
                foreach (IgnoredCertificateErrorTable i in ignoredCertificates)
                {
                    account.connectionConfiguration.IGNORED_CERTIFICATE_ERRORS.Add(i.certificateError);
                }
            }
        }

        /// <summary>
        /// Saves the ConnectionConfiguration for the given XMPPAccount in the DB.
        /// </summary>
        /// <param name="account">The XMPPAccount containing the ConnectionConfiguration you want to save to the db.</param>
        public void saveAccountConnectionConfiguration(XMPPAccount account)
        {
            // Save general options:
            ConnectionOptionsTable optionsTable = new ConnectionOptionsTable(account.connectionConfiguration)
            {
                accountId = account.getIdAndDomain()
            };
            update(optionsTable);

            // Save ignored certificate errors:
            dB.Execute("DELETE FROM " + DBTableConsts.IGNORED_CERTIFICATE_ERROR_TABLE + " WHERE accountId = ?;", account.getIdAndDomain());
            foreach (ChainValidationResult i in account.connectionConfiguration.IGNORED_CERTIFICATE_ERRORS)
            {
                update(new IgnoredCertificateErrorTable()
                {
                    accountId = account.getIdAndDomain(),
                    certificateError = i,
                    id = IgnoredCertificateErrorTable.generateId(account.getIdAndDomain(), i)
                });
            }
        }

        #endregion

        #region --Misc Methods (Private)--


        #endregion

        #region --Misc Methods (Protected)--
        protected override void createTables()
        {
            dB.CreateTable<AccountTable>();
            dB.CreateTable<ConnectionOptionsTable>();
            dB.CreateTable<IgnoredCertificateErrorTable>();
        }

        protected override void dropTables()
        {
            dB.DropTable<AccountTable>();
            dB.DropTable<ConnectionOptionsTable>();
            dB.DropTable<IgnoredCertificateErrorTable>();
        }

        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
