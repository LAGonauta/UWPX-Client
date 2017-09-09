﻿using Data_Manager.Classes.DBEntries;
using Data_Manager.Classes.Events;
using Data_Manager.Classes.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using XMPP_API.Classes;
using XMPP_API.Classes.Events;
using XMPP_API.Classes.Network;
using XMPP_API.Classes.Network.XML;
using XMPP_API.Classes.Network.XML.Messages;

namespace Data_Manager.Classes
{
    public class ConnectionHandler
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public static readonly ConnectionHandler INSTANCE = new ConnectionHandler();

        private ArrayList xMPPClients;

        public delegate void NewChatEventHandler(ConnectionHandler handler, NewChatEventArgs args);

        public event NewChatEventHandler NewChat;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Construktoren--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 26/08/2017 Created [Fabian Sauter]
        /// </history>
        public ConnectionHandler()
        {
            loadAllAccounts();
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        public ArrayList getXMPPClients()
        {
            return xMPPClients;
        }

        public XMPPClient getClientForAccount(ServerConnectionConfiguration account)
        {
            foreach (XMPPClient c in xMPPClients)
            {
                if(c.getSeverConnectionConfiguration().Equals(account))
                {
                    return c;
                }
            }
            return null;
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public void connect()
        {
            connectToAllAccounts();
        }

        public void reloadAllAccounts()
        {
            Task.Factory.StartNew(() =>
            {
                closeAllAccount();
                loadAllAccounts();
                connectToAllAccounts();
            });
        }

        public void closeAllAccount()
        {
            List<Task> tasks = new List<Task>();
            foreach (XMPPClient client in xMPPClients)
            {

                tasks.Add(Task.Factory.StartNew(async () => {
                    await client.disconnectAsync();
                }));
            }
            Task.WaitAll(tasks.ToArray());
        }

        #endregion

        #region --Misc Methods (Private)--
        private void loadAllAccounts()
        {
            xMPPClients = new ArrayList();
            foreach (ServerConnectionConfiguration account in UserManager.INSTANCE.getAccounts())
            {
                XMPPClient client = new XMPPClient(account);
                client.NewRoosterMessage += Client_NewRoosterMessage;
                client.ConnectionStateChanged += Client_ConnectionStateChanged;
                client.NewChatMessage += Client_NewChatMessage;
                xMPPClients.Add(client);
            }
        }

        private void connectToAllAccounts()
        {
            foreach (XMPPClient client in xMPPClients)
            {
                if(!client.getSeverConnectionConfiguration().disabled)
                {
                    Task.Factory.StartNew(async () => {
                        await client.connectAsync();
                    });
                }
            }
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        private void Client_NewRoosterMessage(XMPPClient client, XMPP_API.Classes.Network.Events.NewPresenceEventArgs args)
        {
            if(args.getMessage() is RoosterMessage)
            {
                ServerConnectionConfiguration account = client.getSeverConnectionConfiguration();
                RoosterMessage msg = args.getMessage() as RoosterMessage;
                foreach (RosterItem item in msg.getItems())
                {
                    ChatEntry chat = ChatManager.INSTANCE.getChatEntry(item.getJabberId(), account.getIdAndDomain());
                    if(chat != null)
                    {
                        if(item.getSubscription().Equals("remove"))
                        {
                            ChatManager.INSTANCE.removeChatEntry(chat);
                            continue;
                        }
                        chat.name = item.getName();
                        chat.subscription = item.getSubscription();
                        chat.inRooster = true;
                    }
                    else
                    {
                        chat = new ChatEntry()
                        {
                            id = item.getJabberId(),
                            userAccountId = account.getIdAndDomain(),
                            name = item.getName(),
                            subscription = item.getSubscription(),
                            lastActive = DateTime.Now,
                            muted = false,
                            inRooster = true
                        };
                        NewChat?.Invoke(this, new NewChatEventArgs(chat));
                    }
                    ChatManager.INSTANCE.setChatEntry(chat);
                }
            }
        }

        private async void Client_ConnectionStateChanged(XMPPClient client, ConnectionState state)
        {
            if(state == ConnectionState.CONNECTED)
            {
                await client.requestRoosterAsync();
            }
        }

        private void Client_NewChatMessage(XMPPClient client, XMPP_API.Classes.Network.Events.NewChatMessageEventArgs args)
        {
            MessageMessage msg = args.getMessage();
            string pureJabberId = Utils.removeResourceFromJabberid(msg.getFrom());
            ChatEntry chat = ChatManager.INSTANCE.getChatEntry(pureJabberId, client.getSeverConnectionConfiguration().getIdAndDomain());
            if(chat == null)
            {
                chat = new ChatEntry(pureJabberId, Utils.removeResourceFromJabberid(msg.getTo()));
                ChatManager.INSTANCE.setChatEntry(chat);
                NewChat?.Invoke(this, new NewChatEventArgs(chat));
                if(chat == null)
                {
                    // TODO Message to unknown chat
                    return;
                }
            }
            ChatMessageEntry entry = new ChatMessageEntry(msg, chat);
            ChatManager.INSTANCE.setChatMessageEntry(entry);
        }

        #endregion
    }
}