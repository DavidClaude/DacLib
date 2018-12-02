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

    }
    public struct HoxisRealtimeStatusHost
    {

    }
    public struct HoxisRealtimeStatusProxy
    {

    }
}
