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

        public static int requestTimeoutSec { get; set; }

        public long userID { get; private set; }

        public HoxisConnection connection { get; private set; }

        public HoxisRealtimeStatus realtimeStatus { get; private set; }

        ///// <summary>
        ///// Event of post protocols
        ///// Should be registered by superior HoxisConnection
        ///// </summary>
        //public event ProtocolHandler onPost;

        protected Dictionary<string, ResponseHandler> respTable = new Dictionary<string, ResponseHandler>();

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
                    //SynChannelEntry(proto);
                    break;
                case ProtocolType.Request:
                    // Request check
                    ReqHandle handle = FormatFunc.JsonToObject<ReqHandle>(proto.handle);
                    if (handle.req != proto.action.method) { ResponseError(proto.handle, "request name doesn't match method name"); return; }
                    long ts = handle.ts;
                    int intv = (int)Math.Abs(SystemFunc.GetTimeStamp() - ts);
                    if (intv > requestTimeoutSec) { ResponseError(proto.handle, "request is expired"); return; }
                    // Check ok
                    respTable[proto.action.method](proto.handle, proto.action.args);
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

        public bool Response(string handleArg, HoxisProtocolAction actionArg)
        {
            HoxisProtocol proto = new HoxisProtocol
            {
                type = ProtocolType.Response,
                handle = handleArg,
                err = false,
                rcvr = HoxisProtocolReceiver.undef,
                sndr = HoxisProtocolSender.undef,
                action = actionArg,
                desc = ""
            };
            ProtocolPost(proto);
            return true;
        }

        public bool Response(string handleArg, string methodArg, params KVString[] kvs)
        {
            Dictionary<string, string> argsArg = new Dictionary<string, string>();
            foreach (KVString kv in kvs) { argsArg.Add(kv.key, kv.val); }
            HoxisProtocolAction action = new HoxisProtocolAction
            {
                method = methodArg,
                args = new HoxisProtocolArgs { kv = argsArg },
            };
            return Response(handleArg, action);
        }

        public bool ResponseSuccess(string handleArg, string methodArg) { return Response(handleArg, methodArg, new KVString("code", Consts.RESP_SUCCESS)); }
        public bool ResponseSuccess(string handleArg, string methodArg, params KVString[] kvs)
        {
            Dictionary<string, string> argsArg = new Dictionary<string, string>();
            argsArg.Add("code", Consts.RESP_SUCCESS);
            foreach (KVString kv in kvs) { argsArg.Add(kv.key, kv.val); }
            HoxisProtocolAction action = new HoxisProtocolAction
            {
                method = methodArg,
                args = new HoxisProtocolArgs { kv = argsArg }
            };
            return Response(handleArg, action);
        }

        public bool ResponseError(string handleArg, string descArg)
        {
            HoxisProtocol proto = new HoxisProtocol
            {
                type = ProtocolType.Response,
                handle = handleArg,
                err = true,
                rcvr = HoxisProtocolReceiver.undef,
                sndr = HoxisProtocolSender.undef,
                action = HoxisProtocolAction.undef,
                desc = descArg
            };
            ProtocolPost(proto);
            return true;
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
