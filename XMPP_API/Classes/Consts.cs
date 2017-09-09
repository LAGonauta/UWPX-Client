﻿
namespace XMPP_API.Classes
{
    class Consts
    {
        public static readonly string XML_TO = " to=";
        public static readonly string XML_FROM = " from=";
        public static readonly string XML_ID = " id=";
        public static readonly string XML_STREAM = " stream";
        public static readonly string XML_VERSION = " version='1.0'";
        public static readonly string XML_LANG = " xml:lang='en'";
        public static readonly string XML_CLIENT = " xmlns='jabber:client'";

        public static readonly string XML_HEADER = "<?xml version='1.0'?>";
        public static readonly string XML_STREAM_START = "<stream:stream";
        public static readonly string XML_STREAM_NAMESPACE = " xmlns:stream='http://etherx.jabber.org/streams'";
        public static readonly string XML_STREAM_CLOSE = "</stream:stream>";
        public static readonly string XML_STREAM_ERROR_START = "<stream:error>";
        public static readonly string XML_STREAM_ERROR_CLOSE = "</stream:error>";
        public static readonly string XML_STREAM_FEATURE_START = "<stream:features>";
        public static readonly string XML_STREAM_FEATURE_CLOSE = "</stream:features>";

        public static readonly string XML_MESSAGE_START = "<message>";
        public static readonly string XML_MESSAGE_CLOSE = "</message>";
        public static readonly string XML_BODY_START = "<body>";
        public static readonly string XML_BODY_CLOSE = "</body>";
        public static readonly string XML_RESOURCE_START = "<resource>";
        public static readonly string XML_RESOURCE_CLOSE = "</resource>";

        public static readonly string XML_STARTTLS = "<starttls xmlns='urn:ietf:params:xml:ns:xmpp-tls'/>";
        public static readonly string XML_PROCEED = "<proceed xmlns='urn:ietf:params:xml:ns:xmpp-tls'/>";

        public static readonly string XML_SASL_CHALLENGE_START = "<sasl:challenge>";
        public static readonly string XML_SASL_CHALLENGE_CLOSE = "<sasl:challenge/>";
        public static readonly string XML_SASL_SUCCESS = "<success xmlns='urn:ietf:params:xml:ns:xmpp-sasl'/>";
        public static readonly string XML_SASL_BINDING_START = "<bind xmlns='urn:ietf:params:xml:ns:xmpp-bind'>";
        public static readonly string XML_SASL_BINDING_CLOSE = "</bind>";

        public static readonly string XML_QUERY_ROOSTER = "<query xmlns='jabber:iq:roster'/>";


        // Error:
        public static readonly string XML_FAILURE = "<failure xmlns='urn:ietf:params:xml:ns:xmpp-tls'/>";
        public static readonly string XML_ERROR = ""; //TODO
    }
}