﻿using Logging;
using XMPP_API.Classes.Network.XML.Messages;
using XMPP_API.Classes.Network.XML.Messages.XEP_0030;
using XMPP_API.Classes.Network.XML.Messages.XEP_0280;

namespace XMPP_API.Classes.Network
{
    class DiscoFeatureHelper
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        private readonly XMPPConnection2 CONNECTION;

        private MessageResponseHelper<IQMessage> discoMessageResponseHelper;
        private MessageResponseHelper<IQMessage> carbonsMessageResponseHelper;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 15/08/2018 Created [Fabian Sauter]
        /// </history>
        public DiscoFeatureHelper(XMPPConnection2 connection)
        {
            this.CONNECTION = connection;
            connection.ConnectionStateChanged += Connection_ConnectionStateChanged;
            this.discoMessageResponseHelper = null;
            this.carbonsMessageResponseHelper = null;
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--
        private void requestDisoInfo()
        {
            CONNECTION.account.CONNECTION_INFO.msgCarbonsState = MessageCarbonsState.DISABLED;
            if (discoMessageResponseHelper != null)
            {
                discoMessageResponseHelper.Dispose();
            }
            discoMessageResponseHelper = new MessageResponseHelper<IQMessage>(CONNECTION, onDiscoMessage, onDiscoTimeout);
            discoMessageResponseHelper.start(new DiscoRequestMessage(CONNECTION.account.getIdDomainAndResource(), CONNECTION.account.user.domain, DiscoType.INFO));
        }

        private bool onDiscoMessage(IQMessage msg)
        {
            if (msg is DiscoResponseMessage disco)
            {
                switch (disco.DISCO_TYPE)
                {
                    case DiscoType.ITEMS:
                        break;

                    case DiscoType.INFO:
                        bool foundCarbons = false;
                        foreach (DiscoFeature f in disco.FEATURES)
                        {
                            if (string.Equals(f.VAR, Consts.XML_XEP_0280_NAMESPACE))
                            {
                                foundCarbons = true;
                                if (CONNECTION.account.connectionConfiguration.disableMessageCarbons)
                                {
                                    CONNECTION.account.CONNECTION_INFO.msgCarbonsState = MessageCarbonsState.DISABLED;
                                }
                                else if (CONNECTION.account.CONNECTION_INFO.msgCarbonsState != MessageCarbonsState.ENABLED)
                                {
                                    Logger.Info("Enabling message carbons for " + CONNECTION.account.getIdAndDomain() + " ...");
                                    carbonsMessageResponseHelper = new MessageResponseHelper<IQMessage>(CONNECTION, onCarbonsMessage, onCarbonsTimeout);
                                    carbonsMessageResponseHelper.start(new CarbonsEnableMessage(CONNECTION.account.getIdDomainAndResource()));
                                }
                                break;
                            }
                        }

                        if (!foundCarbons)
                        {
                            Logger.Warn("Unable to enable message carbons for " + CONNECTION.account.getIdAndDomain() + " - not available.");
                            CONNECTION.account.CONNECTION_INFO.msgCarbonsState = MessageCarbonsState.UNAVAILABLE;
                        }
                        break;
                }
                return true;
            }
            else if (msg is IQErrorMessage errMsg)
            {
                CONNECTION.account.CONNECTION_INFO.msgCarbonsState = MessageCarbonsState.UNAVAILABLE;
                Logger.Error("Failed to request initial server disco for " + CONNECTION.account.getIdAndDomain() + ": " + errMsg.ERROR_OBJ.ToString());
                return true;
            }
            return false;
        }

        private void onDiscoTimeout()
        {
            Logger.Error("Failed to request initial server disco - timeout!");
        }

        private bool onCarbonsMessage(IQMessage msg)
        {
            if (msg is IQErrorMessage errMsg)
            {
                CONNECTION.account.CONNECTION_INFO.msgCarbonsState = MessageCarbonsState.ERROR;
                Logger.Error("Failed to enable message carbons for " + CONNECTION.account.getIdAndDomain() + ": " + errMsg.ERROR_OBJ.ToString());
                return true;
            }
            else if (string.Equals(msg.TYPE, IQMessage.RESULT))
            {
                Logger.Info("Message carbons enabled for " + CONNECTION.account.getIdAndDomain());
                CONNECTION.account.CONNECTION_INFO.msgCarbonsState = MessageCarbonsState.ENABLED;
                return true;
            }
            return false;
        }

        private void onCarbonsTimeout()
        {
            CONNECTION.account.CONNECTION_INFO.msgCarbonsState = MessageCarbonsState.ERROR;
            Logger.Error("Failed to enable message carbons for " + CONNECTION.account.getIdAndDomain() + " - timeout!");
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        private void Connection_ConnectionStateChanged(AbstractConnection2 connection, Events.ConnectionStateChangedEventArgs args)
        {
            if (args.newState == ConnectionState.CONNECTED)
            {
                requestDisoInfo();
            }
            else if (args.newState == ConnectionState.DISCONNECTED)
            {
                // Stop message processors:
                if (discoMessageResponseHelper != null)
                {
                    discoMessageResponseHelper.Dispose();
                    discoMessageResponseHelper = null;
                }
                if (carbonsMessageResponseHelper != null)
                {
                    carbonsMessageResponseHelper.Dispose();
                    carbonsMessageResponseHelper = null;
                }
            }
        }

        #endregion
    }
}