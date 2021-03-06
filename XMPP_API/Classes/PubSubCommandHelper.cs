﻿using System;
using System.Collections.Generic;
using XMPP_API.Classes.Network.XML.Messages;
using XMPP_API.Classes.Network.XML.Messages.XEP_0004;
using XMPP_API.Classes.Network.XML.Messages.XEP_0060;
using XMPP_API.Classes.Network.XML.Messages.XEP_0402;

namespace XMPP_API.Classes
{
    public class PubSubCommandHelper
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        private readonly XMPPClient CLIENT;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 16/06/2018 Created [Fabian Sauter]
        /// </history>
        public PubSubCommandHelper(XMPPClient client)
        {
            this.CLIENT = client;
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        public string getPubSubServer()
        {
            throw new NotImplementedException();
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        /// <summary>
        /// Sends a RequestBookmarksMessage to request all bookmarks from the server.
        /// https://xmpp.org/extensions/xep-0048.html#storage-pubsub-retrieve
        /// </summary>
        /// <param name="onMessage">The method that should get executed once the helper receives a new valid message (can be null).</param>
        /// <param name="onTimeout">The method that should get executed once the helper timeout gets triggered (can be null).</param>
        /// <returns>Returns a MessageResponseHelper listening for RequestBookmarksMessage answers.</returns>
        public MessageResponseHelper<IQMessage> requestBookmars_xep_0048(MessageResponseHelper<IQMessage>.OnMessageHandler onMessage, MessageResponseHelper<IQMessage>.OnTimeoutHandler onTimeout)
        {
            MessageResponseHelper<IQMessage> helper = new MessageResponseHelper<IQMessage>(CLIENT, onMessage, onTimeout);
            Network.XML.Messages.XEP_0048.RequestBookmarksMessage msg = new Network.XML.Messages.XEP_0048.RequestBookmarksMessage(CLIENT.getXMPPAccount().getIdDomainAndResource());
            helper.start(msg);
            return helper;
        }

        /// <summary>
        /// Sends a SetBookmarksMessage to request all bookmarks from the server.
        /// https://xmpp.org/extensions/xep-0048.html#storage-pubsub-upload
        /// </summary>
        /// <param name="onMessage">The method that should get executed once the helper receives a new valid message (can be null).</param>
        /// <param name="onTimeout">The method that should get executed once the helper timeout gets triggered (can be null).</param>
        /// <returns>Returns a MessageResponseHelper listening for SetBookmarksMessage answers.</returns>
        public MessageResponseHelper<IQMessage> setBookmars_xep_0048(IList<Network.XML.Messages.XEP_0048.ConferenceItem> conferences, MessageResponseHelper<IQMessage>.OnMessageHandler onMessage, MessageResponseHelper<IQMessage>.OnTimeoutHandler onTimeout)
        {
            MessageResponseHelper<IQMessage> helper = new MessageResponseHelper<IQMessage>(CLIENT, onMessage, onTimeout);
            Network.XML.Messages.XEP_0048.SetBookmarksMessage msg = new Network.XML.Messages.XEP_0048.SetBookmarksMessage(CLIENT.getXMPPAccount().getIdDomainAndResource(), conferences);
            helper.start(msg);
            return helper;
        }

        /// <summary>
        /// Sends a RequestBookmarksMessage to request all bookmarks from the server.
        /// https://xmpp.org/extensions/xep-0402.html#retrieving-bookmarks
        /// </summary>
        /// <param name="onMessage">The method that should get executed once the helper receives a new valid message (can be null).</param>
        /// <param name="onTimeout">The method that should get executed once the helper timeout gets triggered (can be null).</param>
        /// <returns>Returns a MessageResponseHelper listening for RequestBookmarksMessage answers.</returns>
        public MessageResponseHelper<IQMessage> requestBookmars_xep_0402(MessageResponseHelper<IQMessage>.OnMessageHandler onMessage, MessageResponseHelper<IQMessage>.OnTimeoutHandler onTimeout)
        {
            MessageResponseHelper<IQMessage> helper = new MessageResponseHelper<IQMessage>(CLIENT, onMessage, onTimeout);
            RequestBookmarksMessage msg = new RequestBookmarksMessage(CLIENT.getXMPPAccount().getIdDomainAndResource());
            helper.start(msg);
            return helper;
        }

        /// <summary>
        /// Sends a AddBookmarksMessage to store the given bookmark on the server.
        /// https://xmpp.org/extensions/xep-0402.html#adding-a-bookmark
        /// </summary>
        /// <param name="conferenceItem">The bookmark that should get stored on the server.</param>
        /// <param name="onMessage">The method that should get executed once the helper receives a new valid message (can be null).</param>
        /// <param name="onTimeout">The method that should get executed once the helper timeout gets triggered (can be null).</param>
        /// <returns>Returns a MessageResponseHelper listening for AddBookmarksMessage answers.</returns>
        public MessageResponseHelper<IQMessage> addBookmark_xep_0402(ConferenceItem conferenceItem, MessageResponseHelper<IQMessage>.OnMessageHandler onMessage, MessageResponseHelper<IQMessage>.OnTimeoutHandler onTimeout)
        {
            MessageResponseHelper<IQMessage> helper = new MessageResponseHelper<IQMessage>(CLIENT, onMessage, onTimeout);
            AddBookmarkMessage msg = new AddBookmarkMessage(CLIENT.getXMPPAccount().getIdDomainAndResource(), conferenceItem);
            helper.start(msg);
            return helper;
        }

        /// <summary>
        /// Sends a RemoveBookmarksMessage to remove the given bookmark from the server.
        /// https://xmpp.org/extensions/xep-0402.html#removing-a-bookmark
        /// </summary>
        /// <param name="conferenceItem">The bookmark that should get removed from the server.</param>
        /// <param name="onMessage">The method that should get executed once the helper receives a new valid message (can be null).</param>
        /// <param name="onTimeout">The method that should get executed once the helper timeout gets triggered (can be null).</param>
        /// <returns>Returns a MessageResponseHelper listening for RemoveBookmarksMessage answers.</returns>
        public MessageResponseHelper<IQMessage> removeBookmark_xep_0402(ConferenceItem conferenceItem, MessageResponseHelper<IQMessage>.OnMessageHandler onMessage, MessageResponseHelper<IQMessage>.OnTimeoutHandler onTimeout)
        {
            MessageResponseHelper<IQMessage> helper = new MessageResponseHelper<IQMessage>(CLIENT, onMessage, onTimeout);
            RemoveBookmarkMessage msg = new RemoveBookmarkMessage(CLIENT.getXMPPAccount().getIdDomainAndResource(), conferenceItem);
            helper.start(msg);
            return helper;
        }

        /// <summary>
        /// Sends a DiscoverNodeMetadataMessage to discover the node metadata.
        /// https://xmpp.org/extensions/xep-0060.html#entity-metadata
        /// </summary>
        /// <param name="to">The target pubsub server (can be null).</param>
        /// <param name="nodeName">The name of the node, the metadata should get queried for.</param>
        /// <param name="onMessage">The method that should get executed once the helper receives a new valid message (can be null).</param>
        /// <param name="onTimeout">The method that should get executed once the helper timeout gets triggered (can be null).</param>
        /// <returns>Returns a MessageResponseHelper listening for DiscoverNodeMetadataMessage answers.</returns>
        public MessageResponseHelper<IQMessage> requestNodeMetadata(string to, string nodeName, MessageResponseHelper<IQMessage>.OnMessageHandler onMessage, MessageResponseHelper<IQMessage>.OnTimeoutHandler onTimeout)
        {
            MessageResponseHelper<IQMessage> helper = new MessageResponseHelper<IQMessage>(CLIENT, onMessage, onTimeout);
            PubSubDiscoverNodeMetadataMessage msg = new PubSubDiscoverNodeMetadataMessage(CLIENT.getXMPPAccount().getIdDomainAndResource(), to, nodeName);
            helper.start(msg);
            return helper;
        }

        /// <summary>
        /// Sends a PubSubSubscribeMessage to subscribe to the given node.
        /// </summary>
        /// <param name="to">The target pubsub server (can be null).</param>
        /// <param name="nodeName">The name of the node, you want to subscribe to.</param>
        /// <param name="onMessage">The method that should get executed once the helper receives a new valid message (can be null).</param>
        /// <param name="onTimeout">The method that should get executed once the helper timeout gets triggered (can be null).</param>
        /// <returns>Returns a MessageResponseHelper listening for PubSubSubscribeMessage answers.</returns>
        public MessageResponseHelper<IQMessage> requestNodeSubscription(string to, string nodeName, MessageResponseHelper<IQMessage>.OnMessageHandler onMessage, MessageResponseHelper<IQMessage>.OnTimeoutHandler onTimeout)
        {
            MessageResponseHelper<IQMessage> helper = new MessageResponseHelper<IQMessage>(CLIENT, onMessage, onTimeout);
            PubSubSubscribeMessage msg = new PubSubSubscribeMessage(CLIENT.getXMPPAccount().getIdDomainAndResource(), CLIENT.getXMPPAccount().getIdAndDomain(), to, nodeName);
            helper.start(msg);
            return helper;
        }

        /// <summary>
        /// Sends a PubSubCreateNodeMessage to create a pubsub node.
        /// https://xmpp.org/extensions/xep-0060.html#owner-create
        /// </summary>
        /// <param name="to">The target pubsub server (can be null).</param>
        /// <param name="nodeName">The name of the node, you want to create.</param>
        /// <param name="onMessage">The method that should get executed once the helper receives a new valid message (can be null).</param>
        /// <param name="onTimeout">The method that should get executed once the helper timeout gets triggered (can be null).</param>
        /// <returns>Returns a MessageResponseHelper listening for PubSubCreateNodeMessage answers.</returns>
        public MessageResponseHelper<IQMessage> createNode(string to, string nodeName, MessageResponseHelper<IQMessage>.OnMessageHandler onMessage, MessageResponseHelper<IQMessage>.OnTimeoutHandler onTimeout)
        {
            MessageResponseHelper<IQMessage> helper = new MessageResponseHelper<IQMessage>(CLIENT, onMessage, onTimeout);
            PubSubCreateNodeMessage msg = new PubSubCreateNodeMessage(CLIENT.getXMPPAccount().getIdDomainAndResource(), to, nodeName);
            helper.start(msg);
            return helper;
        }

        /// <summary>
        /// Sends a PubSubCreateNodeMessage to create a pubsub node.
        /// https://xmpp.org/extensions/xep-0060.html#owner-create-and-configure
        /// </summary>
        /// <param name="to">The target pubsub server (can be null).</param>
        /// <param name="nodeName">The name of the node, you want to create.</param>
        /// <param name="config">The configuration for the node.</param>
        /// <param name="onMessage">The method that should get executed once the helper receives a new valid message (can be null).</param>
        /// <param name="onTimeout">The method that should get executed once the helper timeout gets triggered (can be null).</param>
        /// <returns>Returns a MessageResponseHelper listening for PubSubCreateNodeMessage answers.</returns>
        public MessageResponseHelper<IQMessage> createNode(string to, string nodeName, DataForm config, MessageResponseHelper<IQMessage>.OnMessageHandler onMessage, MessageResponseHelper<IQMessage>.OnTimeoutHandler onTimeout)
        {
            MessageResponseHelper<IQMessage> helper = new MessageResponseHelper<IQMessage>(CLIENT, onMessage, onTimeout);
            PubSubCreateNodeMessage msg = new PubSubCreateNodeMessage(CLIENT.getXMPPAccount().getIdDomainAndResource(), to, nodeName, config);
            helper.start(msg);
            return helper;
        }

        /// <summary>
        /// Sends a PubSubDeleteNodeMessage to delete a pubsub node.
        /// https://xmpp.org/extensions/xep-0060.html#owner-delete
        /// </summary>
        /// <param name="to">The target pubsub server (can be null).</param>
        /// <param name="nodeName">The name of the node, you want to delete.</param>
        /// <param name="onMessage">The method that should get executed once the helper receives a new valid message (can be null).</param>
        /// <param name="onTimeout">The method that should get executed once the helper timeout gets triggered (can be null).</param>
        /// <returns>Returns a MessageResponseHelper listening for PubSubDeleteNodeMessage answers.</returns>
        public MessageResponseHelper<IQMessage> deleteNode(string to, string nodeName, MessageResponseHelper<IQMessage>.OnMessageHandler onMessage, MessageResponseHelper<IQMessage>.OnTimeoutHandler onTimeout)
        {
            MessageResponseHelper<IQMessage> helper = new MessageResponseHelper<IQMessage>(CLIENT, onMessage, onTimeout);
            PubSubDeleteNodeMessage msg = new PubSubDeleteNodeMessage(CLIENT.getXMPPAccount().getIdDomainAndResource(), to, nodeName);
            helper.start(msg);
            return helper;
        }

        /// <summary>
        /// Sends a PubSubRequestSubscriptionsMessage to request all subscriptions from a given server.
        /// https://xmpp.org/extensions/xep-0060.html#entity-subscriptions
        /// </summary>
        /// <param name="to">The target pubsub server.</param>
        /// <param name="onMessage">The method that should get executed once the helper receives a new valid message (can be null).</param>
        /// <param name="onTimeout">The method that should get executed once the helper timeout gets triggered (can be null).</param>
        /// <returns>Returns a MessageResponseHelper listening for PubSubRequestSubscriptionsMessage answers.</returns>
        public MessageResponseHelper<IQMessage> requestSubscriptions(string to, MessageResponseHelper<IQMessage>.OnMessageHandler onMessage, MessageResponseHelper<IQMessage>.OnTimeoutHandler onTimeout)
        {
            MessageResponseHelper<IQMessage> helper = new MessageResponseHelper<IQMessage>(CLIENT, onMessage, onTimeout);
            PubSubRequestSubscriptionsMessage msg = new PubSubRequestSubscriptionsMessage(CLIENT.getXMPPAccount().getIdDomainAndResource(), to);
            helper.start(msg);
            return helper;
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
