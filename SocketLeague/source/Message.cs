using System.Collections.Generic;

namespace SocketLeague
{
    public enum MsgTypes
    {
        // Server messages:
        ResetGame = 0,
        AssignID = 1,

        SetBall = 2,

        // Client messages:
        SetPlayer = 11,
    }

    // Structure of message when sent as byte[]:
    // [ size (int) | type (int) | data (List<byte>) ... ]
}
