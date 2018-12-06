using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DacLib.Generic;
using DacLib.Hoxis.Server;

namespace DacLib.Hoxis
{
    public struct HoxisRealtimeStatus
    {
        public static readonly HoxisRealtimeStatus undef = new HoxisRealtimeStatus {
            state = UserState.None,
            clusterid = string.Empty,
            teamid = string.Empty
        };

        public UserState state;
        public string clusterid;
        public string teamid;

        HoxisRealtimeStatusAgent host;
        HoxisRealtimeStatusAgent[] proxies;
    }
    public struct HoxisRealtimeStatusAgent
    {
        public AgentType agentType;
    }
}
