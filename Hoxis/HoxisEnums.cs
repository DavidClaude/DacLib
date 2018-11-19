using System.Collections;
using System.Collections.Generic;

namespace DacLib.Hoxis {
    public enum ProtocolType {
        None = 0,
        Synchronization = 1,
        Request = 2,
        Response = 3
    }
    public enum ReceiverType {
        None = 0,
        Server = 1,
        MultiPlayers = 2,
        TeamPlayers = 3,
        Player = 4
    }
}


