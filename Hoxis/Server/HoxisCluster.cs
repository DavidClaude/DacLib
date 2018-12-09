using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DacLib.Generic;
using FF = DacLib.Generic.FormatFunc;

namespace DacLib.Hoxis.Server
{
    public class HoxisCluster
    {
        #region ret codes
        public const byte RET_ENOUGH_USERS = 1;
        public const byte RET_ALL_USERS_LEAVE = 2;
        public const byte RET_USER_EXISTS = 3;
        public const byte RET_NO_USER = 4;
        #endregion

        public static int maxUser { get; set; }

        public string id { get; }

        public int userCount { get { return _users.Count; } }

        public HoxisTeam this[string tid]
        {
            get
            {
                if (_teams == null) return null;
                if (!_teams.ContainsKey(tid)) return null;
                return _teams[tid];
            }
        }

        private List<HoxisUser> _users;
        private Dictionary<string, HoxisTeam> _teams;

        public HoxisCluster(string idArg)
        {
            id = idArg;
            _users = new List<HoxisUser>();
            _teams = new Dictionary<string, HoxisTeam>();
        }

        public void ProtocolBroadcast(HoxisProtocol proto) { foreach (HoxisUser u in _users) { u.ProtocolPost(proto); } }

        public void UserJoin(HoxisUser user, out Ret ret)
        {
            if (_users.Count >= maxUser) { ret = new Ret(LogLevel.Info, RET_ENOUGH_USERS, "Users are enough"); return; }
            if (_users.Contains(user)) { ret = new Ret(LogLevel.Warning, RET_USER_EXISTS, FF.StringFormat("User:{0} already exists", user.userID)); return; }
            _users.Add(user);
            user.superiorCluster = this;
            ret = Ret.ok;
        }

        public void UserLeave(HoxisUser user, out Ret ret)
        {
            if (!_users.Contains(user)) { ret = new Ret(LogLevel.Warning, RET_NO_USER, FF.StringFormat("User:{0} doesn't exist", user.userID)); return; }
            ManageTeam(ManageOperation.Leave, user);
            user.superiorCluster = null;
            _users.Remove(user);
            if (_users.Count == 0) { ret = new Ret(LogLevel.Info, RET_ALL_USERS_LEAVE, "All users have left"); return; }
            ret = Ret.ok;
        }

        public bool ManageTeam(ManageOperation op, HoxisUser sponsor)
        {
            switch (op)
            {
                case ManageOperation.Create:

                    break;
                case ManageOperation.Join:

                    break;
                case ManageOperation.Leave:

                    break;
                case ManageOperation.Destroy:

                    break;
            }
            return true;
        }
    }
}
