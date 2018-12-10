using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

using DacLib.Generic;
using DacLib.Codex;
using FF = DacLib.Generic.FormatFunc;
using MF = DacLib.Generic.MathFunc;
using SF = DacLib.Generic.SystemFunc;

namespace DacLib.Hoxis.Server
{
    public static class HoxisServer
    {
        /// <summary>
        /// Ver.
        /// </summary>
        public const string version = "0.0.0";

        #region ret codes
        public const string ERR_MSG_CFG_UNINITIALIZED = "Configuration file should be initialized first";
        #endregion

        /// <summary>
        /// Hoxis server configuration
        /// </summary>
        public static TomlConfiguration config { get; private set; }

        /// <summary>
        /// Local IP
        /// </summary>
        public static string ip { get; private set; }

        /// <summary>
        /// Local port
        /// </summary>
        public static int port { get; private set; }

        /// <summary>
        /// The max count of user connection
        /// </summary>
        public static int maxConn { get; private set; }

        /// <summary>
        /// Remain count of user connection
        /// </summary>
        public static int remainConn { get { return _userReception.remain; } }

        /// <summary>
        /// Basic direction of application
        /// </summary>
        public static readonly string basicPath = AppDomain.CurrentDomain.BaseDirectory;

        private static Socket _socket;
        private static DebugRecorder _logger;
        private static Thread _acceptThread;
        private static CriticalPreformPool<HoxisUser> _userReception;
        private static Dictionary<string, HoxisCluster> _clusters;

        /// <summary>
        /// Init the configuration, such as the ip, port, socket and arguments
        /// </summary>
        /// <param name="configPath"></param>
        public static void InitConfig(string configPath, out Ret ret)
        {
            // Init and begin log recording
            _logger = new DebugRecorder(FF.StringAppend(basicPath, @"logs\server.log"), out ret);
            if (ret.code != 0) { Console.WriteLine(ret.desc); return; }
            _logger.Begin();
            _logger.LogPattern("David.Claude", version, "Survival Mission");

            // Init config
            string path;
            if (configPath != "") { path = configPath; }
            else { path = FF.StringAppend(basicPath, @"..\..\DacLib\Hoxis\Configs\hoxis_server.toml"); }
            config = new TomlConfiguration(path, out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc,"Init",true); return; }
            _logger.LogInfo("read configuration success", "Init");

            // Assign ip, port and init the sokcet
            ip = SF.GetLocalIP(out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Init"); return; }
            port = config.GetInt("server", "port", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Init"); return; }
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            _logger.LogInfo(FF.StringFormat("ip is {0}, port is {1}", ip, port.ToString()), "Init");

            // Init user reception
            maxConn = config.GetInt("server", "max_conn_user", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Init"); return; }
            _userReception = new CriticalPreformPool<HoxisUser>(maxConn);
            _logger.LogInfo(FF.StringFormat("max connections is {0}", maxConn), "Init");

            // Init cluster
            _clusters = new Dictionary<string, HoxisCluster>();
            HoxisCluster.maxUser = config.GetInt("cluster", "max_user", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Init"); return; }
            _logger.LogInfo(FF.StringFormat("max users of cluster is {0}", HoxisCluster.maxUser), "Init");

            // Init team
            HoxisTeam.maxUser = config.GetInt("team", "max_user", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Init"); return; }
            _logger.LogInfo(FF.StringFormat("max users of team is {0}", HoxisTeam.maxUser), "Init");

            // Init user
            HoxisUser.requestTimeoutSec = config.GetInt("user", "request_timeout", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Init"); return; }
            _logger.LogInfo(FF.StringFormat("request timeout is {0} seconds", HoxisUser.requestTimeoutSec), "Init");
            HoxisUser.heartbeatTimeout = config.GetInt("user", "heartbeat_timeout", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Init"); return; }
            _logger.LogInfo(FF.StringFormat("heartbeat timeout is {0} milliseconds", HoxisUser.heartbeatTimeout), "Init");
            HoxisUser.heartbeatInterval = config.GetInt("user", "heartbeat_interval", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Init"); return; }
            _logger.LogInfo(FF.StringFormat("heartbeat interval is {0} milliseconds", HoxisUser.heartbeatInterval), "Init");

            // Init connection
            HoxisConnection.readBufferSize = config.GetInt("conn", "read_buffer_size", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Init"); return; }
            _logger.LogInfo(FF.StringFormat("read buffer size of connection is {0}", HoxisConnection.readBufferSize), "Init");

            _logger.LogInfo("success", "Init", true);
        }

        /// <summary>
        /// Bind and listen
        /// </summary>
        public static void Listen()
        {
            try
            {
                IPAddress addr = IPAddress.Parse(ip);
                IPEndPoint ep = new IPEndPoint(addr, port);
                if (_socket == null) { throw new Exception(ERR_MSG_CFG_UNINITIALIZED); }
                _socket.Bind(ep);
                int count = config.GetInt("socket", "max_client_count");
                _socket.Listen(count);
                _logger.LogInfo("success", "Listen", true);
            }
            catch (Exception e) { _logger.LogError(e.Message, "Listen", true); }
        }

        /// <summary>
        /// Begin accept socket connection within thread
        /// </summary>
        public static void BeginAccept()
        {
            _acceptThread = new Thread(() =>
            {
                while (true)
                {
                    Socket cs = _socket.Accept();
                    _logger.LogInfo(FF.StringAppend( "new client: ",cs.RemoteEndPoint.ToString()), "Accept");
                    Ret ret;
                    HoxisUser user = _userReception.Request(cs, out ret);
                    if (ret.code != 0) { _logger.LogWarning(ret.desc, cs.RemoteEndPoint.ToString()); continue; }
                    _logger.LogInfo("request successful", user.connection.remoteEndPoint);
                }
            });
            _acceptThread.Start();
            _logger.LogInfo("begin...", "Accept", true);
        }

        #region management
        /// <summary>
        /// Get all 
        /// </summary>
        /// <returns></returns>
        public static List<HoxisUser> GetWorkers() { return _userReception.GetOccupiedPreforms(); }

        /// <summary>
        /// Release an user
        /// Generally called when an user quits, reconnects or stops heartbeats
        /// </summary>
        /// <param name="user"></param>
        public static void ReleaseUser(HoxisUser user)
        {
            string rep = user.connection.remoteEndPoint;
            Ret ret;
            _userReception.Release(user, out ret);
            if (ret.code != 0) { _logger.LogWarning(ret.desc, rep); return; }
            _logger.LogWarning("released", rep);
        }

        public static bool ManageCluster(ManageOperation op, HoxisUser sponsor)
        {
            switch (op)
            {
                case ManageOperation.Create:
                    string cid = FF.StringAppend(sponsor.userID.ToString(), "@", SF.GetTimeStamp().ToString());
                    if (_clusters.ContainsKey(cid)) { Console.WriteLine("[error]Create cluster: cluster {0} already exists", cid); return false; }
                    lock (_clusters)
                    {
                        HoxisCluster hc = new HoxisCluster(cid);
                        _clusters.Add(cid, hc);
                        Ret ret;
                        hc.UserJoin(sponsor, out ret);
                        if (ret.code != 0) { Console.WriteLine("[warning]Create cluster: {0}", ret.desc); return false; }
                    }
                    break;
                case ManageOperation.Join:
                    // todo Call matching sdk, get a cluster
                    break;
                case ManageOperation.Leave:

                    break;
                case ManageOperation.Destroy:

                    break;
            }
            return true;
        }

        #endregion

        public static void LogConfig()
        {
            Console.WriteLine("Server IP: {0}", ip);
            Console.WriteLine("Server port: {0}", port.ToString());
            Console.WriteLine("Max connecting user quantity: {0}", maxConn.ToString());
            Console.WriteLine("Max cluster user quantity: {0}", HoxisCluster.maxUser.ToString());
            Console.WriteLine("Max team user quantity: {0}", HoxisTeam.maxUser.ToString());
            Console.WriteLine("Read buffer size: {0}", HoxisConnection.readBufferSize.ToString());
        }

        /// <summary>
        /// Stop the service saftly
        /// </summary>
        public static void Stop()
        {
            _logger.End();
            _acceptThread.Abort();
        }
    }
}
