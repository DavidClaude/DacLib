using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DacLib.Generic;
using DacLib.Hoxis.Server;

namespace DacLib.Hoxis
{
    public struct HoxisUserRealtimeData
    {
        public static readonly HoxisUserRealtimeData undef = new HoxisUserRealtimeData
        {
            parentCluster = null,
            parentTeam = null,
            hostData = HoxisAgentData.undef,
            proxiesData = new List<HoxisAgentData>()
        };

        public HoxisCluster parentCluster;
        public HoxisTeam parentTeam;
        public HoxisAgentData hostData;
        public List<HoxisAgentData> proxiesData;
    }

    public struct HoxisAgentData
    {
        public static readonly HoxisAgentData undef = new HoxisAgentData
        {
            agentType = AgentType.None,
        };

        public AgentType agentType;
    }
}
