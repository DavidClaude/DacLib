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
        #region ret codes
        public const byte RET_CHECK_ERROR = 1;
        #endregion


        #region reusable
        public int localID { get; set; }
        public bool isOccupied { get; set; }
        #endregion

        public static int requestTimeoutSec { get; set; }

        public long userID { get; private set; }

        public HoxisConnection connection { get; private set; }

        public HoxisRealtimeStatus realtimeStatus;

        public HoxisCluster superiorCluster {
            get { return _superiorCluster; }
            set {
                _superiorCluster = value;
                if (_superiorCluster == null) { realtimeStatus.cluster = ""; return; }
                realtimeStatus.cluster = _superiorCluster.name;
            }
        }

        public HoxisTeam superiorTeam {
            get { return _superiorTeam; }
            set {
                _superiorTeam = value;
                if (_superiorTeam == null) { realtimeStatus.team = "";return; }
                realtimeStatus.cluster = _superiorTeam.name;
            }
        }

        ///// <summary>
        ///// Event of post protocols
        ///// Should be registered by superior HoxisConnection
        ///// </summary>
        //public event ProtocolHandler onPost;

        protected Dictionary<string, ResponseHandler> respTable = new Dictionary<string, ResponseHandler>();

        private HoxisCluster _superiorCluster;
        private HoxisTeam _superiorTeam;

        public HoxisUser()
        {
            #region register reflection table
            respTable.Add("SignIn", SignIn);
            respTable.Add("GetRealtimeStatus", GetRealtimeStatus);
            respTable.Add("LoadUserData", LoadUserData);
            respTable.Add("SaveUserData", SaveUserData);
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
            realtimeStatus = HoxisRealtimeStatus.undef;
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
                    switch (proto.receiver.type)
                    {
                        case ReceiverType.Cluster:
                            if (superiorCluster == null) return;
                            superiorCluster.ProtocolBroadcast(proto);
                            break;
                        case ReceiverType.Team:
                            if (superiorTeam == null) return;
                            superiorTeam.ProtocolBroadcast(proto);
                            break;
                        case ReceiverType.User:

                            break;
                    }
                    break;
                case ProtocolType.Request:
                    // Request check
                    Ret retCheck;
                    CheckRequest(proto, out retCheck);
                    if (retCheck.code != 0) { ResponseError(proto.handle, retCheck.desc); return; }
                    // Check ok
                    respTable[proto.action.method](proto.handle, proto.action.args);
                    break;
                case ProtocolType.Response:

                    break;
                default:
                    ResponseError(proto.handle, "invalid type of protocol");
                    break;
            }
        }

        /// <summary>
        /// Post a protocol to client
        /// </summary>
        /// <param name="proto"></param>
        public void ProtocolPost(HoxisProtocol proto)
        {
            string json = FormatFunc.ObjectToJson(proto);
            byte[] data = FormatFunc.StringToBytes(json);
            connection.BeginSend(data);
        }

        /// <summary>
        /// Response a custom result, hardly called immediately
        /// </summary>
        /// <param name="handleArg"></param>
        /// <param name="actionArg"></param>
        /// <returns></returns>
        public bool Response(string handleArg, HoxisProtocolAction actionArg)
        {
            HoxisProtocol proto = new HoxisProtocol
            {
                type = ProtocolType.Response,
                handle = handleArg,
                err = false,
                receiver = HoxisProtocolReceiver.undef,
                sender = HoxisProtocolSender.undef,
                action = actionArg,
                desc = ""
            };
            ProtocolPost(proto);
            return true;
        }

        public bool Response(string handleArg, string methodArg, params KVString[] kvs)
        {
            HoxisProtocolAction action = new HoxisProtocolAction(methodArg, new HoxisProtocolArgs(kvs));
            return Response(handleArg, action);
        }

        /// <summary>
        /// Response an successful result with result code and action
        /// </summary>
        /// <param name="handleArg"></param>
        /// <param name="methodArg"></param>
        /// <returns></returns>
        public bool ResponseSuccess(string handleArg, string methodArg) { return Response(handleArg, methodArg, new KVString("code", Consts.RESP_SUCCESS)); }
        public bool ResponseSuccess(string handleArg, string methodArg, params KVString[] kvs)
        {
            Dictionary<string, string> argsArg = new Dictionary<string, string>();
            argsArg.Add("code", Consts.RESP_SUCCESS);
            foreach (KVString kv in kvs) { argsArg.Add(kv.key, kv.val); }
            HoxisProtocolAction action = new HoxisProtocolAction(methodArg, new HoxisProtocolArgs(argsArg));
            return Response(handleArg, action);
        }

        /// <summary>
        /// Response an error with only description
        /// </summary>
        /// <param name="handleArg"></param>
        /// <param name="descArg"></param>
        /// <returns></returns>
        public bool ResponseError(string handleArg, string descArg)
        {
            HoxisProtocol proto = new HoxisProtocol
            {
                type = ProtocolType.Response,
                handle = handleArg,
                err = true,
                receiver = HoxisProtocolReceiver.undef,
                sender = HoxisProtocolSender.undef,
                action = HoxisProtocolAction.undef,
                desc = descArg
            };
            ProtocolPost(proto);
            return true;
        }

        /// <summary>
        /// Check the request name and time stamp
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="ret"></param>
        public void CheckRequest(HoxisProtocol proto, out Ret ret)
        {
            ReqHandle handle = FormatFunc.JsonToObject<ReqHandle>(proto.handle, out ret);
            if (ret.code != 0) return;
            // Check if request name matches method name
            if (handle.req != proto.action.method) { ret = new Ret(LogLevel.Info, RET_CHECK_ERROR, "request name doesn't match method name"); return; }
            // Check if expired
            long ts = handle.ts;
            int intv = (int)Math.Abs(SystemFunc.GetTimeStamp() - ts);
            if (intv > requestTimeoutSec) { ret = new Ret(LogLevel.Info, RET_CHECK_ERROR, "request is expired"); return; }
            ret = Ret.ok;
        }

        public void HandOverConnection(HoxisUser user)
        {
            connection.onExtract -= ProtocolEntry;
            user.TakeOverConnection(connection);
        }

        public void TakeOverConnection(HoxisConnection conn)
        {
            lock (connection)
            {
                connection = conn;
                connection.onExtract += ProtocolEntry;
            }
        }

        #region reflection functions: response

        private bool SignIn(string handle, HoxisProtocolArgs args)
        {
            long uid = FormatFunc.StringToLong(args.kv["uid"]);
            List<HoxisUser> workers = HoxisServer.GetWorkers();
            foreach (HoxisUser u in workers)
            {
                if (u.userID == uid && uid > 0)
                {
                    Response(handle, "SignInCb", new KVString("code", Consts.RESP_RECONNECT));
                    HandOverConnection(u);
                    HoxisServer.ReleaseUser(this);
                    return true;
                }
            }
            userID = uid;
            return ResponseSuccess(handle, "SignInCb");
        }

        /// <summary>
        /// Called if client requests for reconnecting
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool GetRealtimeStatus(string handle, HoxisProtocolArgs args)
        {
            string json = FormatFunc.ObjectToJson(realtimeStatus);
            return ResponseSuccess(handle, "GetRealtimeStatusCb", new KVString("status", json));
        }

        private bool LoadUserData(string handle, HoxisProtocolArgs args)
        {
            //todo 访问数据库，通过userID获取用户数据
            //todo 转为json
            string json = "";
            return ResponseSuccess(handle, "LoadUserDataCb", new KVString("data", json));
        }

        private bool SaveUserData(string handle, HoxisProtocolArgs args)
        {
            string json = args.kv["data"];
            //todo 写入数据库
            return ResponseSuccess(handle, "SaveUserDataCb");
        }




        #endregion
    }
}
