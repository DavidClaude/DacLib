using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DacLib.Hoxis
{
    public struct HoxisRealtimeStatus
    {
        public static readonly HoxisRealtimeStatus undef = new HoxisRealtimeStatus {
            state = UserState.None,
        };

        public UserState state;
        public string cluster_id;
        public string team_id;

    }
    public struct HoxisRealtimeStatusAgent
    {
        public AgentType agent_type;
    }
}
