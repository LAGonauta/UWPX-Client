﻿namespace XMPP_API.Classes.Network.XML.Messages.Processor
{
    public enum SASLState
    {
        ERROR,
        NO_VALID_MECHANISM,
        DISCONNECTED,
        REQUESTED,
        CHALLENGING,
        CONNECTED
    }
}
