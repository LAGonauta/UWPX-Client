﻿using SQLite;

namespace XMPP_API.Classes.Network.XML.DBEntries
{
    [Table(DBTableConsts.PRE_KEY_TABLE)]
    class PreKeyTable
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        [PrimaryKey]
        public string id { get; set; }
        [NotNull]
        public uint preKeyId { get; set; }
        [NotNull]
        public string accountId { get; set; }
        [NotNull]
        public byte[] preKey { get; set; }

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 08/08/2018 Created [Fabian Sauter]
        /// </history>
        public PreKeyTable()
        {
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public static string generateId(uint preKeyId, string accountId)
        {
            return preKeyId + "_" + accountId;
        }

        #endregion

        #region --Misc Methods (Private)--


        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}