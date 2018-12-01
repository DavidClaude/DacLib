using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DacLib.Hoxis.Server
{
    public class HoxisCluster
    {
        public static int maxUser { get; set; }

        private List<HoxisUser> _users;

        public void Broadcast(HoxisProtocol proto) {

        }
    }
}
