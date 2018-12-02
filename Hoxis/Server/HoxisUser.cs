using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using DacLib.Generic;

namespace DacLib.Hoxis.Server
{
    public class HoxisUser : IReusable
    {
        #region reusable
        public int localID { get; set; }
        public bool isOccupied { get; set; }
        #endregion


        public long userID { get; private set; }

        public HoxisConnection connection { get; private set; }

        public HoxisRealtimeStatus status { get; private set; }

        ///// <summary>
        ///// Event of post protocols
        ///// Should be registered by superior HoxisConnection
        ///// </summary>
        //public event ProtocolHandler onPost;

        protected Dictionary<string, ResponseHandler> respTable = new Dictionary<string, ResponseHandler>();



        public HoxisUser()
        {
            #region register reflection table
            //businessTable.Add("load_user_data", LoadUserData);
            //businessTable.Add("save_user_data", SaveUserData);
            #endregion
        }

        public void OnRequest(object state)
        {
            Socket s = (Socket)state;
            HoxisConnection conn = new HoxisConnection(s);
            TakeOverConnection(conn);
        }

        public void OnRelease()
        {
            userID = -1;
            connection = null;
            status = HoxisRealtimeStatus.undef;
        }

        /// <summary>
        /// **WITHIN THREAD**
        /// The entrance of protocol bytes
        /// </summary>
        /// <param name="data"></param>
        public void ProtocolEntry(byte[] data)
        {
            string json = FormatFunc.BytesToString(data);
            Ret ret;
            HoxisProtocol proto = FormatFunc.JsonToObject<HoxisProtocol>(json, out ret);
            if (ret.code != 0) return;
            switch (proto.type)
            {
                case ProtocolType.Synchronization:
                    //SynChannelEntry(proto);
                    break;
                case ProtocolType.Request:
                    respTable[proto.action.method](proto.action.args, proto.handle);
                    break;
                case ProtocolType.Response:
                    //RespChannelEntry(proto);
                    break;
            }
        }

        public void ProtocolPost(HoxisProtocol proto)
        {
            string json = FormatFunc.ObjectToJson(proto);
            byte[] data = FormatFunc.StringToBytes(json);
            connection.BeginSend(data);
        }

        public void Response(HoxisProtocolAction actionArg, string handleArg)
        {
            HoxisProtocol proto = new HoxisProtocol
            {
                type = ProtocolType.Response,
                handle = handleArg,
                rcvr = HoxisProtocolReceiver.undef,
                sndr = HoxisProtocolSender.undef,
                action = actionArg,
                desc = ""
            };
            ProtocolPost(proto);
        }

        public void HandOverConnection(HoxisUser user)
        {
            connection.onExtract -= ProtocolEntry;
            user.TakeOverConnection(connection);
        }

        public void TakeOverConnection(HoxisConnection conn)
        {
            lock (connection) {
                connection = conn;
                connection.onExtract += ProtocolEntry;
            }
        }

        #region reflection functions: response

        private void SignIn(HoxisProtocolArgs args, string handle)
        {
            long uid = FormatFunc.StringToLong(args.kv["uid"]);
            List<HoxisUser> workers = HoxisServer.GetWorkers();
            foreach (HoxisUser u in workers)
            {
                if (u.userID == uid && uid > 0)
                {
                    HoxisProtocolAction action = new HoxisProtocolAction
                    {
                        method = "SignInCb",
                        args = new HoxisProtocolArgs{
                            kv = new Dictionary<string, string>
                            {
                                //todo
                            }
                        },
                    };
                    HandOverConnection(u);
                    HoxisServer.ReleaseUser(this);
                }
            }
        }

        private void LoadUserData(HoxisProtocolArgs args)
        {
            //解析uid
            long uid = FormatFunc.StringToLong(args.kv["uid"]);

            //访问数据库，获取UserData

            //将UserData转为json

            //将UserData打包成协议
            HoxisProtocol proto = new HoxisProtocol
            {
                type = ProtocolType.Response,
                handle = "",
                rcvr = HoxisProtocolReceiver.undef,
                sndr = HoxisProtocolSender.undef,
                action = new HoxisProtocolAction
                {
                    method = "",
                    args = new HoxisProtocolArgs
                    {
                        kv = new Dictionary<string, string> {
                            {"data","" },
                        }
                    }
                },
                desc = "",
            };
        }

        private void SaveUserData(HoxisProtocolArgs args)
        {

        }




        #endregion
    }
}
