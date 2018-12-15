using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using DacLib.Generic;
using DacLib.Codex;

using FF = DacLib.Generic.FormatFunc;
using SF = DacLib.Generic.SystemFunc;

namespace DacLib.Hoxis.Server
{
    public class HoxisUser
    {
        #region ret codes
        public const byte RET_CHECK_ERROR = 1;
        #endregion

        public static long requestTTL { get; set; }
        public static int heartbeatTimeout { get; set; }

        /// <summary>
        /// Unique ID of user
        /// </summary>
        public long userID { get; private set; }

        /// <summary>
        /// State the server keeps which describes the connection
        /// </summary>
        public UserConnectionState connectionState { get; private set; }

        #region realtime data
        public HoxisCluster parentCluster { get; set; }
        public HoxisTeam parentTeam { get; set; }
        public HoxisAgentData hostData { get; private set; }
        public List<HoxisAgentData> proxiesData { get; private set; }
        #endregion

        public event BytesForVoid_Handler onPost;
        //public event IntForVoid_Handler onHeartbeatStop;
        protected Dictionary<string, ResponseHandler> respTable = new Dictionary<string, ResponseHandler>();

        private HoxisHeartbeat _heartbeat;
        private DebugRecorder _logger;

        public HoxisUser()
        {
            userID = 0;
            connectionState = UserConnectionState.None;
            parentCluster = null;
            parentTeam = null;
            hostData = HoxisAgentData.undef;
            proxiesData = new List<HoxisAgentData>();
            respTable.Add("SignIn", SignIn);
            respTable.Add("GetRealtimeStatus", GetRealtimeStatus);
            respTable.Add("LoadUserData", LoadUserData);
            respTable.Add("SaveUserData", SaveUserData);
            _heartbeat = new HoxisHeartbeat(heartbeatTimeout);
            _heartbeat.onTimeout += OnHeartbeatStop;
        }

        /// <summary>
        /// **WITHIN THREAD**
        /// The entrance of protocol bytes
        /// </summary>
        /// <param name="data"></param>
        public void ProtocolEntry(byte[] data)
        {
            string json = FF.BytesToString(data);
            Console.WriteLine(json);
            Ret ret;
            HoxisProtocol proto = FF.JsonToObject<HoxisProtocol>(json, out ret);
            if (ret.code != 0) return;
            switch (proto.type)
            {
                case ProtocolType.Synchronization:
                    switch (proto.receiver.type)
                    {
                        case ReceiverType.Cluster:
                            if (parentCluster == null) return;
                            parentCluster.ProtocolBroadcast(proto);
                            break;
                        case ReceiverType.Team:
                            if (parentTeam == null) return;
                            parentTeam.ProtocolBroadcast(proto);
                            break;
                        case ReceiverType.User:
                            HoxisUser user = HoxisServer.GetUser(proto.receiver.uid);
                            // todo send
                            break;
                    }
                    break;
                case ProtocolType.Request:
                    // Request check
                    Ret retCheck;
                    CheckRequest(proto, out retCheck);
                    if (retCheck.code != 0)
                    {
                        ResponseError(proto.handle, retCheck.desc);
                        _logger.LogError(retCheck.desc, userID.ToString());
                        _logger.End();
                        return;
                    }
                    // Check ok
                    respTable[proto.action.method](proto.handle, proto.action.args);
                    _logger.LogInfo(proto.action.method, userID.ToString());
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
            string json = FF.ObjectToJson(proto);
            byte[] data = FF.StringToBytes(json);
            OnPost(data);
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
            ReqHandle handle = FF.JsonToObject<ReqHandle>(proto.handle, out ret);
            if (ret.code != 0) return;
            // Check if request name matches method name
            if (handle.req != proto.action.method) { ret = new Ret(LogLevel.Info, RET_CHECK_ERROR, "request name doesn't match method name"); return; }
            // Check if expired
            long ts = handle.ts;
            long intv = Math.Abs(SF.GetTimeStamp(TimeUnit.Millisecond) - ts);
            if (intv > requestTTL) { ret = new Ret(LogLevel.Info, RET_CHECK_ERROR, "request is expired"); return; }
            ret = Ret.ok;
        }

        /// <summary>
        /// Reset this HoxisUser to the undefine user
        /// </summary>
        public void SignOut()
        {
            userID = 0;
            connectionState = UserConnectionState.None;
            parentCluster = null;
            parentTeam = null;
            hostData = HoxisAgentData.undef;
            proxiesData = null;
            _heartbeat.Reset();
            _logger.LogInfo("signs out", "");
            _logger.End();
        }

        private void OnHeartbeatStop(int time) {
            connectionState = UserConnectionState.Reconnecting;
            _heartbeat.Reset();
        }

        private void OnPost(byte[] data) { if (onPost == null) return;onPost(data); }

        #region reflection functions: response

        /// <summary>
        /// Make sure that the client is connected
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool RefreshHeartbeat(string handle, HoxisProtocolArgs args)
        {
            if (_heartbeat == null) return ResponseError(handle, "heartbeat is null");
            if (!_heartbeat.enable) return ResponseError(handle, "heartbeat is disable");
            _heartbeat.Refresh();
            return ResponseSuccess(handle, "RefreshHeartbeatCb");
        }

        /// <summary>
        /// Fill this HoxisUser with an connected user
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool SignIn(string handle, HoxisProtocolArgs args)
        {
            long uid = FF.StringToLong(args["uid"]);
            // Reconnect ?
            List<HoxisConnection> workers = HoxisServer.GetWorkingConnections();
            foreach (HoxisConnection w in workers)
            {
                if (w.user.userID == uid && uid > 0)
                {
                    // Set this user by existed user
                    userID = w.user.userID;
                    connectionState = UserConnectionState.Active;
                    parentCluster = w.user.parentCluster;
                    parentTeam = w.user.parentTeam;
                    hostData = w.user.hostData;
                    proxiesData = w.user.proxiesData;
                    _heartbeat.Start();
                    // Release the existed one
                    HoxisServer.ReleaseConnection(w);
                    // Response success
                    return Response(handle, "SignInCb", new KVString("code", Consts.RESP_RECONNECT));
                }
            }
            // New user
            userID = uid;
            connectionState = UserConnectionState.Default;
            _heartbeat.Start();
            Ret ret;
            string name = FF.StringAppend(uid.ToString(), "@", SF.GetTimeStamp().ToString(), ".log");
            _logger = new DebugRecorder(FF.StringAppend(HoxisServer.basicPath, @"logs\users\", name), out ret);
            if (ret.code != 0) { Console.WriteLine(ret.desc); }
            else
            {
                _logger.Begin();
                _logger.LogInfo("signs in", "");
            }
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
            return false;
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
            string json = args["data"];
            //todo 写入数据库
            return ResponseSuccess(handle, "SaveUserDataCb");
        }

        #endregion
    }
}
