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
using C = DacLib.Hoxis.Consts;

namespace DacLib.Hoxis.Server
{
    public class HoxisUser : IStatusControllable
    {
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
        public HoxisUserRealtimeData realtimeData;
        public event BytesForVoid_Handler onPost;
        public event ExceptionHandler onNetworkAnomaly; 
        protected Dictionary<string, ResponseHandler> respTable;

        private AsyncTimer _heartbeatMonitor;
        private DebugRecorder _logger;

        public HoxisUser()
        {
            userID = 0;
            connectionState = UserConnectionState.None;
            realtimeData = HoxisUserRealtimeData.undef;
            _heartbeatMonitor = new AsyncTimer(heartbeatTimeout, new NoneForVoid_Handler(() =>
                 {
                     OnNetworkAmomaly(C.CODE_HEARTBEAT_TIMEOUT, "remote socket disconnected exceptionally");
                 })
                );
            respTable = new Dictionary<string, ResponseHandler>();
            respTable.Add("QueryConnectionState", QueryConnectionState);
            respTable.Add("SignIn", SignIn);
            respTable.Add("SignOut", SignOut);
            respTable.Add("Reconnect", Reconnect);
            respTable.Add("RefreshHeartbeat", RefreshHeartbeat);
        }

        #region IStatusControllable
        public void Awake() { _heartbeatMonitor.Begin(); }
        public void Pause() {
            connectionState = UserConnectionState.Disconnected;
            _heartbeatMonitor.End();
            if (DebugRecorder.LogEnable(_logger)) _logger.End();
        }
        public void Continue() {
            Ret ret;
            connectionState = UserConnectionState.Active;
            _heartbeatMonitor.Begin();
            if (!DebugRecorder.LogEnable(_logger))
            {
                _logger = new DebugRecorder(FF.StringAppend(HoxisServer.basicPath, @"logs\users\", NewUserLogName(userID)), out ret);
                if (ret.code != 0) { Console.WriteLine(ret.desc); }
                else { _logger.Begin(); }
            }
        }
        public void Reset()
        {
            userID = 0;
            connectionState = UserConnectionState.None;
            realtimeData = HoxisUserRealtimeData.undef;
            _heartbeatMonitor.End();
        }
        #endregion

        /// <summary>
        /// **WITHIN THREAD**
        /// The entrance of protocol bytes
        /// </summary>
        /// <param name="data"></param>
        public void ProtocolEntry(byte[] data)
        {
            string json = FF.BytesToString(data);
            HoxisProtocol proto = FF.JsonToObject<HoxisProtocol>(json);
            switch (proto.type)
            {
                case ProtocolType.Synchronization:
                    switch (proto.receiver.type)
                    {
                        case ReceiverType.Cluster:
                            if (realtimeData.parentCluster == null) return;
                            realtimeData.parentCluster.ProtocolBroadcast(proto);
                            break;
                        case ReceiverType.Team:
                            if (realtimeData.parentTeam == null) return;
                            realtimeData.parentTeam.ProtocolBroadcast(proto);
                            break;
                        case ReceiverType.User:
                            HoxisUser user = HoxisServer.GetUser(proto.receiver.uid);
                            // todo send
                            break;
                    }
                    break;
                case ProtocolType.Request:
                    // Request check
                    Ret ret;
                    CheckRequest(proto, out ret);
                    if (ret.code != 0)
                    {
                        ResponseError(proto.handle, C.RESP_CHECK_FAILED, ret.desc);
                        return;
                    }
                    // Check ok
                    if (!respTable.ContainsKey(proto.action.method))
                    {
                        if (DebugRecorder.LogEnable(_logger)) _logger.LogError(FF.StringFormat("invalid request: {0}", proto.action.method), "", true);
                        ResponseError(proto.handle, C.RESP_CHECK_FAILED, FF.StringFormat("invalid request: {0}", proto.action.method));
                        return;
                    }
                    respTable[proto.action.method](proto.handle, proto.action.args);
                    break;
                default:
                    if (DebugRecorder.LogEnable(_logger)) _logger.LogError(FF.StringFormat("invalid protocol type: {0}", proto.type), "");
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
                err = C.RESP_SUCCESS,
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
            argsArg.Add("code", C.RESP_SUCCESS);
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
        public bool ResponseError(string handleArg, string errArg, string descArg)
        {
            HoxisProtocol proto = new HoxisProtocol
            {
                type = ProtocolType.Response,
                handle = handleArg,
                err = errArg,
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
            if (handle.req != proto.action.method) { ret = new Ret(LogLevel.Info, 1, "request name doesn't match method name"); return; }
            // Check if expired
            long ts = handle.ts;
            long intv = Math.Abs(SF.GetTimeStamp(TimeUnit.Millisecond) - ts);
            if (intv > requestTTL) { ret = new Ret(LogLevel.Info, 1, "request is expired"); return; }
            ret = Ret.ok;
        }

        ///// <summary>
        ///// **WITHIN THREAD**
        ///// Called when remote socket is closed or heartbeat stopped
        ///// </summary>
        ///// <param name="code"></param>
        ///// <param name="desc"></param>
        //public void ProcessNetworkAnomaly(int code, string desc)
        //{
        //    lock (this)
        //    {
        //        switch (connectionState)
        //        {
        //            case UserConnectionState.None:                              // User hasn't signed in or has been already released, we should release it to avoid repeating being called
                        
        //                break;
        //            case UserConnectionState.Default:
        //                connectionState = UserConnectionState.None;
        //                break;
        //            case UserConnectionState.Active:
        //                connectionState = UserConnectionState.Disconnected;
        //                break;
        //            case UserConnectionState.Disconnected:
        //                // wait for reconnecting
        //                break;
        //        }
        //        if (DebugRecorder.LogEnable(_logger)) { _logger.LogError(FF.StringFormat("network anomaly: code is {0}, message is {1}", code, desc), "", true); }
        //    }
        //}

        /// <summary>
        /// New log name of an user
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static string NewUserLogName(long uid) { return FF.StringAppend(uid.ToString(), "@", SF.GetTimeStamp().ToString(), ".log"); }

        private void OnPost(byte[] data) { if (onPost == null) return; onPost(data); }
        private void OnNetworkAmomaly(int code, string message) { if (onNetworkAnomaly == null) return;onNetworkAnomaly(code, message); }

        #region reflection functions: response

        /// <summary>
        /// Get the connection state if this user has already signed in
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool QueryConnectionState(string handle, HoxisProtocolArgs args)
        {
            long uid = FF.StringToLong(args["uid"]);
            if (uid <= 0) return ResponseError(handle, C.RESP_ILLEGAL_ARGUMENT, FF.StringFormat("illegal argument: {0}", args["uid"]));
            HoxisUser user = HoxisServer.GetUser(uid);
            if (user == null) return Response(handle, "QueryConnectionStateCb", new KVString("code", C.RESP_NO_USER_INFO));
            if (user == this) return Response(handle, "QueryConnectionStateCb", new KVString("code", C.RESP_NO_USER_INFO));
            return ResponseSuccess(handle, "QueryConnectionStateCb", new KVString("state", user.connectionState.ToString()));
        }

        /// <summary>
        /// Fill this HoxisUser with an connected user
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool SignIn(string handle, HoxisProtocolArgs args)
        {
            Ret ret;
            long uid = FF.StringToLong(args["uid"]);
            if (uid <= 0) return ResponseError(handle, C.RESP_ILLEGAL_ARGUMENT, FF.StringFormat("illegal argument: {0}", args["uid"]));
            userID = uid;
            connectionState = UserConnectionState.Default;
            if (DebugRecorder.LogEnable(_logger)) { _logger.LogInfo("sign in", ""); }
            else
            {
                _logger = new DebugRecorder(FF.StringAppend(HoxisServer.basicPath, @"logs\users\", NewUserLogName(uid)), out ret);
                if (ret.code != 0) { Console.WriteLine(ret.desc); }
                else
                {
                    _logger.Begin();
                    _logger.LogInfo("sign in", "");
                }
            }
            return ResponseSuccess(handle, "SignInCb");
        }

        private bool SignOut(string handle, HoxisProtocolArgs args)
        {
            userID = 0;
            connectionState = UserConnectionState.None;
            if (DebugRecorder.LogEnable(_logger)) { _logger.LogInfo("sign out", ""); _logger.End(); }
            return ResponseSuccess(handle, "SignOutCb");
        }

        private bool Reconnect(string handle, HoxisProtocolArgs args)
        {
            long uid = FF.StringToLong(args["uid"]);
            List<HoxisConnection> workers = HoxisServer.GetWorkingConnections();
            foreach (HoxisConnection w in workers)
            {
                // If already signed in, response the state to let user choose if reconnecting
                if (w.user == this) continue;
                if (w.user.userID <= 0) continue;
                if (w.user.userID == uid)
                {
                    userID = w.user.userID;
                    realtimeData = w.user.realtimeData;
                    Continue();
                    if (DebugRecorder.LogEnable(_logger)) _logger.LogInfo("reconnect", "");
                    HoxisServer.AffairEntry(C.AFFAIR_RELEASE_CONNECTION, w);
                    return ResponseSuccess(handle, "ReconnectCb");
                }
            }
            return Response(handle, "ReconnectCb", new KVString("code", C.RESP_NO_USER_INFO));
        }

        /// <summary>
        /// Make sure that the client is connected
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool RefreshHeartbeat(string handle, HoxisProtocolArgs args)
        {
            if (_heartbeatMonitor == null) return ResponseError(handle, C.RESP_HEARTBEAT_UNAVAILABLE, "heartbeat monitor is null");
            if (!_heartbeatMonitor.active) return ResponseError(handle, C.RESP_HEARTBEAT_UNAVAILABLE, "heartbeat monitor is inactive");
            _heartbeatMonitor.Refresh();
            return ResponseSuccess(handle, "RefreshHeartbeatCb");
        }

        ///// <summary>
        ///// Called if client requests for reconnecting
        ///// </summary>
        ///// <param name="handle"></param>
        ///// <param name="args"></param>
        ///// <returns></returns>
        //private bool GetRealtimeStatus(string handle, HoxisProtocolArgs args)
        //{
        //    return false;
        //}

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
