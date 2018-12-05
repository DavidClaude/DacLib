using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DacLib.Generic;

namespace DacLib.Hoxis.Server
{
    public class HoxisCluster
    {
        public static int maxUser { get; set; }

        public string name { get; }

        public int userCount { get { return _users.Count; } }

        private List<HoxisUser> _users;
        private Dictionary<string, HoxisTeam> _teams;

        public HoxisCluster(string nameArg)
        {
            name = nameArg;
            _users = new List<HoxisUser>();
            _teams = new Dictionary<string, HoxisTeam>();
        }

        public void ProtocolBroadcast(HoxisProtocol proto) { foreach (HoxisUser u in _users) { u.ProtocolPost(proto); } }

        public HoxisTeam GetTeam(string tid)
        {
            if (!_teams.ContainsKey(tid)) return null;
            return _teams[tid];
        }

        public bool ManageTeam(string operation, HoxisUser sponsor)
        {
            switch (operation)
            {
                case "create":
                    //string id = FormatFunc.StringAppend(clusterID, ".", sponsor.userID.ToString());
                    //if (_teams.ContainsKey(id)) { Console.WriteLine("[error]Create team: {0} already exists", id); return false; }
                    //lock (_teams)
                    //{
                    //    _teams.Add(id, new HoxisTeam(id));
                    //    // add this user
                    //}
                    break;
                case "join":
                    break;
                case "leave":
                    break;
                case "destroy":
                    break;
                default:
                    break;
            }
            return true;
        }
    }
}
