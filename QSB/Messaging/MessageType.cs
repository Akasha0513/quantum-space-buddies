﻿using UnityEngine.Networking;

namespace QSB.Messaging
{
    public enum MessageType
    {
        Sector = MsgType.Highest + 1,
        WakeUp = MsgType.Highest + 2,
        AnimTrigger = MsgType.Highest + 3
        // Add other message types here, incrementing the value.
    }
}
