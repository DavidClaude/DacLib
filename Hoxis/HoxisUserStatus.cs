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
        };

        public UserState state;
        public string cluster;
        public string team;
    }
    public struct HoxisRealtimeStatusAgent
    {
        public AgentType agent_type;
    }
}
