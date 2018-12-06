using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DacLib.Hoxis.Server
{
    public class HoxisTeam
    {
        public static int maxUser { get; set; }

        public string id { get; }

        public int userCount { get { return _users.Count; } }

        private List<HoxisUser> _users;

        public HoxisTeam(string idArg)
        {
            id = idArg;
            _users = new List<HoxisUser>();
        }

        public void ProtocolBroadcast(HoxisProtocol proto) { foreach (HoxisUser u in _users) { u.ProtocolPost(proto); } }
    }
}
