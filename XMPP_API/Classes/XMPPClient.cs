﻿using Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XMPP_API.Classes.Events;
using XMPP_API.Classes.Network;
using XMPP_API.Classes.Network.Events;
using XMPP_API.Classes.Network.XML;
using XMPP_API.Classes.Network.XML.Messages;
using XMPP_API.Classes.Network.XML.Messages.XEP_0045;
using XMPP_API.Classes.Network.XML.Messages.XEP_0085;
using XMPP_API.Classes.Network.XML.Messages.XEP_0184;
using XMPP_API.Classes.Network.XML.Messages.XEP_0384;

namespace XMPP_API.Classes
{
    public class XMPPClient : IMessageSender
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        private XMPPConnection2 connection;

        public delegate void ConnectionStateChangedEventHandler(XMPPClient client, ConnectionStateChangedEventArgs args);
        public delegate void NewChatMessageEventHandler(XMPPClient client, NewChatMessageEventArgs args);
        public delegate void NewPresenceEventHandler(XMPPClient client, Events.NewPresenceMessageEventArgs args);
        public delegate void NewChatStateEventHandler(XMPPClient client, NewChatStateEventArgs args);
        public delegate void MessageSendEventHandler(XMPPClient client, MessageSendEventArgs args);
        public delegate void NewBookmarksResultMessageEventHandler(XMPPClient client, NewBookmarksResultMessageEventArgs args);
        public delegate void NewMUCMemberPresenceMessageEventHandler(XMPPClient client, NewMUCMemberPresenceMessageEventArgs args);
        public delegate void NewDeliveryReceiptHandler(XMPPClient client, NewDeliveryReceiptEventArgs args);
        public delegate void OmemoSessionBuildErrorEventHandler(XMPPClient client, OmemoSessionBuildErrorEventArgs args);

        public event NewValidMessageEventHandler NewRoosterMessage;
        public event ConnectionStateChangedEventHandler ConnectionStateChanged;
        public event NewChatMessageEventHandler NewChatMessage;
        public event NewPresenceEventHandler NewPresence;
        public event NewChatStateEventHandler NewChatState;
        public event MessageSendEventHandler MessageSend;
        public event NewMUCMemberPresenceMessageEventHandler NewMUCMemberPresenceMessage;
        public event NewValidMessageEventHandler NewValidMessage;
        public event NewBookmarksResultMessageEventHandler NewBookmarksResultMessage;
        public event NewDeliveryReceiptHandler NewDeliveryReceipt;
        public event OmemoSessionBuildErrorEventHandler OmemoSessionBuildError;

        public readonly GeneralCommandHelper GENERAL_COMMAND_HELPER;
        public readonly MUCCommandHelper MUC_COMMAND_HELPER;
        public readonly PubSubCommandHelper PUB_SUB_COMMAND_HELPER;
        public readonly OmemoCommandHelper OMEMO_COMMAND_HELPER;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 17/08/2017 Created [Fabian Sauter]
        /// </history>
        public XMPPClient(XMPPAccount account)
        {
            this.GENERAL_COMMAND_HELPER = new GeneralCommandHelper(this);
            this.MUC_COMMAND_HELPER = new MUCCommandHelper(this);
            this.PUB_SUB_COMMAND_HELPER = new PubSubCommandHelper(this);
            this.OMEMO_COMMAND_HELPER = new OmemoCommandHelper(this);
            init(account);
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        public bool isConnected()
        {
            return connection != null && connection.state == ConnectionState.CONNECTED;
        }

        public ConnectionState getConnetionState()
        {
            return connection.state;
        }

        public MessageParserStats getMessageParserStats()
        {
            return connection?.getMessageParserStats();
        }

        public ConnectionError getLastConnectionError()
        {
            return connection.lastConnectionError;
        }

        public OmemoHelper getOmemoHelper()
        {
            return connection.omemoHelper;
        }

        /// <summary>
        /// Sets the given XMPPAccount.
        /// Make sure you call disconnectAsyc() before to prevent memory leaks!
        /// </summary>
        /// <param name="account">The new XMPPAccount.</param>
        public void setAccount(XMPPAccount account)
        {
            // Cleanup old connection:
            if (connection != null)
            {
                switch (connection.state)
                {
                    case ConnectionState.CONNECTING:
                    case ConnectionState.CONNECTED:
                        throw new InvalidOperationException("Unable to set account, if the client is still connecting or connected! state = " + connection.state);
                }
                connection.NewRoosterMessage -= Connection_ConnectionNewRoosterMessage;
                connection.ConnectionStateChanged -= Connection_ConnectionStateChanged;
                connection.NewValidMessage -= Connection_ConnectionNewValidMessage;
                connection.NewPresenceMessage -= Connection_ConnectionNewPresenceMessage;
                connection.MessageSend -= Connection_MessageSend;
                connection.NewBookmarksResultMessage -= Connection_NewBookmarksResultMessage;
                connection.OmemoSessionBuildError -= Connection_OmemoSessionBuildErrorEvent;
            }

            init(account);
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public async Task transferSocketOwnershipAsync()
        {
            //await connection?.transferSocketOwnershipAsync();
        }

        public void connect()
        {
            Logger.Info("Connecting account: " + getXMPPAccount().getIdAndDomain());
            try
            {
                connection.connectAndHold();
            }
            catch (Exception e)
            {
                Logger.Error("Error during connectAsync in XMPPClient!", e);
            }
        }

        public async Task reconnectAsync()
        {
            Logger.Info("Reconnecting account: " + getXMPPAccount().getIdAndDomain());
            await connection.reconnectAsync(true);
        }

        public async Task disconnectAsync()
        {
            Logger.Info("Disconnecting account: " + getXMPPAccount().getIdAndDomain());
            await connection.disconnectAsyncs();
        }

        public void sendOmemoMessage(OmemoMessageMessage msg, string chatJid, string accountJid)
        {
            if (connection.omemoHelper is null)
            {
                OmemoSessionBuildError?.Invoke(this, new OmemoSessionBuildErrorEventArgs(chatJid, Network.XML.Messages.XEP_0384.Signal.Session.OmemoSessionBuildError.KEY_ERROR, new List<OmemoMessageMessage> { msg }));
                Logger.Error("Failed to send OMEMO message - OmemoHelper is null");
            }
            else if (!connection.account.checkOmemoKeys())
            {
                OmemoSessionBuildError?.Invoke(this, new OmemoSessionBuildErrorEventArgs(chatJid, Network.XML.Messages.XEP_0384.Signal.Session.OmemoSessionBuildError.KEY_ERROR, new List<OmemoMessageMessage> { msg }));
                Logger.Error("Failed to send OMEMO message - keys are corrupted");
            }
            else
            {
                connection.omemoHelper.sendOmemoMessage(msg, chatJid, accountJid);
            }
        }

        /// <summary>
        /// Starts a new Task and calls sendAsync(...)
        /// </summary>
        /// <param name="msg">The MessageMessage object that should get send.</param>
        /// <returns>The created Task object.</returns>
        public Task sendMessage(MessageMessage msg)
        {
            return Task.Run(async () => await connection.sendAsync(msg, true, false));
        }

        public async Task<bool> sendAsync(AbstractMessage msg)
        {
            return await sendAsync(msg, false);
        }

        public async Task<bool> sendAsync(AbstractMessage msg, bool cacheIfNotConnected)
        {
            return await connection.sendAsync(msg, cacheIfNotConnected, false);
        }

        public XMPPAccount getXMPPAccount()
        {
            return connection.account;
        }

        /// <summary>
        /// Enables OMEMO encryption for messages for this connection.
        /// Has to be enabled before connecting.
        /// </summary>
        /// <param name="omemoStore">A persistent store for all the OMEMO related data (e.g. device ids and keys).</param>
        /// <returns>Returns true on success.</returns>
        public bool enableOmemo(IOmemoStore omemoStore)
        {
            return connection.enableOmemo(omemoStore);
        }
        #endregion

        #region --Misc Methods (Private)--
        private void init(XMPPAccount account)
        {
            connection = new XMPPConnection2(account);
            connection.NewRoosterMessage += Connection_ConnectionNewRoosterMessage;
            connection.ConnectionStateChanged += Connection_ConnectionStateChanged;
            connection.NewValidMessage += Connection_ConnectionNewValidMessage;
            connection.NewPresenceMessage += Connection_ConnectionNewPresenceMessage;
            connection.MessageSend += Connection_MessageSend;
            connection.NewBookmarksResultMessage += Connection_NewBookmarksResultMessage;
            connection.OmemoSessionBuildError += Connection_OmemoSessionBuildErrorEvent;
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        private void Connection_ConnectionNewRoosterMessage(IMessageSender connection, NewValidMessageEventArgs args)
        {
            NewRoosterMessage?.Invoke(this, args);
        }

        private void Connection_ConnectionStateChanged(AbstractConnection2 connection, ConnectionStateChangedEventArgs args)
        {
            if (args.newState == ConnectionState.CONNECTED)
            {
                Logger.Info("Connected to account: " + getXMPPAccount().getIdAndDomain());
            }
            else if (args.newState == ConnectionState.DISCONNECTED)
            {
                Logger.Info("Disconnected account: " + getXMPPAccount().getIdAndDomain());
            }

            ConnectionStateChanged?.Invoke(this, args);
        }

        private void Connection_ConnectionNewValidMessage(IMessageSender connection, NewValidMessageEventArgs args)
        {
            AbstractMessage msg = args.MESSAGE;
            if (msg is MessageMessage mMsg)
            {
                NewChatMessage?.Invoke(this, new NewChatMessageEventArgs(mMsg));
            }
            else if (msg is ChatStateMessage sMsg)
            {
                NewChatState?.Invoke(this, new NewChatStateEventArgs(sMsg));
            }
            else if (msg is DeliveryReceiptMessage dRMsg)
            {
                NewDeliveryReceipt?.Invoke(this, new NewDeliveryReceiptEventArgs(dRMsg));
            }

            NewValidMessage?.Invoke(this, args);
        }

        private void Connection_ConnectionNewPresenceMessage(IMessageSender connection, NewValidMessageEventArgs args)
        {
            // XEP-0045 (MUC member presence):
            if (args.MESSAGE is MUCMemberPresenceMessage)
            {
                NewMUCMemberPresenceMessage?.Invoke(this, new NewMUCMemberPresenceMessageEventArgs(args.MESSAGE as MUCMemberPresenceMessage));
            }
            else
            {
                NewPresence?.Invoke(this, new NewPresenceMessageEventArgs(args.MESSAGE as PresenceMessage));
            }
        }

        private void Connection_MessageSend(XMPPConnection2 connection, MessageSendEventArgs args)
        {
            MessageSend?.Invoke(this, args);
        }

        private void Connection_NewBookmarksResultMessage(XMPPConnection2 connection, NewBookmarksResultMessageEventArgs args)
        {
            NewBookmarksResultMessage?.Invoke(this, args);
        }

        private void Connection_OmemoSessionBuildErrorEvent(XMPPConnection2 connection, OmemoSessionBuildErrorEventArgs args)
        {
            OmemoSessionBuildError?.Invoke(this, args);
        }

        #endregion
    }
}
