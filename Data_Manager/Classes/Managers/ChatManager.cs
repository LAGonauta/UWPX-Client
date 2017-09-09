﻿using Data_Manager.Classes.DBEntries;
using System.Collections.Generic;
using XMPP_API.Classes;

namespace Data_Manager.Classes.Managers
{
    public class ChatManager : AbstractManager
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public static readonly ChatManager INSTANCE = new ChatManager();

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Construktoren--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 29/08/2017 Created [Fabian Sauter]
        /// </history>
        public ChatManager()
        {
            //dropTables();
            //createTables();
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        public bool doesChatExist(XMPPClient client, string chatId)
        {
            List<ChatEntry> chats = dB.Query<ChatEntry>("SELECT * FROM ChatEntry WHERE userAccountId LIKE ? AND id LIKE ?", client.getSeverConnectionConfiguration().getIdAndDomain(), chatId);
            return chats.Count > 0;
        }

        public IList<ChatMessageEntry> getAllChatMessagesForChat(ChatEntry chat)
        {
            return dB.Query<ChatMessageEntry>("SELECT * FROM ChatMessageEntry WHERE chatId LIKE ? ORDER BY date DESC", chat.id + '%');
        }

        public string getLastChatMessageForChat(ChatEntry chat)
        {
            IList<ChatMessageEntry> list = getAllChatMessagesForChat(chat);
            if (list.Count <= 0)
            {
                return "";
            }
            return list[0].message;
        }

        public ChatEntry getChatEntry(string id, string userAccountId)
        {
            IList<ChatEntry> list = dB.Query<ChatEntry>("SELECT * FROM ChatEntry WHERE id LIKE ? AND userAccountId LIKE ?", id, userAccountId);
            if (list.Count < 1)
            {
                return null;
            }
            else
            {
                return list[0];
            }
        }

        public void setChatMessageEntry(ChatMessageEntry message)
        {
            dB.InsertOrReplace(message);
        }

        public void setChatEntry(ChatEntry chat)
        {
            dB.InsertOrReplace(chat);
        }

        public List<ChatEntry> getAllChatsForClient(XMPPClient c)
        {
            return dB.Query<ChatEntry>("SELECT * FROM ChatEntry WHERE userAccountId LIKE ?", c.getSeverConnectionConfiguration().getIdAndDomain());
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public void removeChatEntry(ChatEntry chat)
        {
            dB.Delete(chat);
        }

        #endregion

        #region --Misc Methods (Private)--


        #endregion

        #region --Misc Methods (Protected)--
        protected override void createTables()
        {
            dB.CreateTable<ChatMessageEntry>();
            dB.CreateTable<ChatEntry>();
        }

        protected override void dropTables()
        {
            dB.DropTable<ChatMessageEntry>();
            dB.DropTable<ChatEntry>();
        }

        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}