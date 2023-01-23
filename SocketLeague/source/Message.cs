using System.Collections.Generic;

namespace SocketLeague
{
    public enum MsgTypes
    {
        ResetGame = 0,
        AssignID = 1,
        SetPlayer = 2,
        SetBall = 3,
    }

    // Structure of message when sent as byte[]:
    // [ size (int) | type (int) | data (List<byte>) ... ]
}
