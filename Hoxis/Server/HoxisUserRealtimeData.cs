using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DacLib.Generic;
using DacLib.Hoxis.Server;

namespace DacLib.Hoxis
{
    public struct HoxisAgentData
    {
        public static readonly HoxisAgentData undef = new HoxisAgentData
        {
            agentType = AgentType.None,
        };

        public AgentType agentType;
    }
}
