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
        public const string ERR_MSG_CFG_UNINITIALIZED = "configuration file should be initialized first";
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
        /// Processing interval of affair queue
        /// </summary>
        public static int affairQueueProcessInterval { get; private set; }

        /// <summary>
        /// The max count of user connection
        /// </summary>
        public static int maxConn { get; private set; }

        /// <summary>
        /// Remain count of connections
        /// </summary>
        public static int remainConn { get { return _connReception.remain; } }

        /// <summary>
        /// Basic direction of application
        /// </summary>
        public static readonly string basicPath = AppDomain.CurrentDomain.BaseDirectory;

        private static Socket _socket;
        private static DebugRecorder _logger;
        private static Thread _acceptThread;
        private static FiniteProcessQueue<KV<string, object>> _affairQueue;
        private static Thread _affairThread;
        private static CriticalPreformPool<HoxisConnection> _connReception;
        private static Dictionary<string, HoxisCluster> _clusters;

        /// <summary>
        /// Init the configuration, such as the ip, port, socket and arguments
        /// </summary>
        /// <param name="configPath"></param>
        public static void InitializeConfig(string project, out Ret ret, string configPath = "")
        {
            // Init and begin log recording
            _logger = new DebugRecorder(FF.StringAppend(basicPath, @"logs\server.log"), out ret);
            if (ret.code != 0) { Console.WriteLine(ret.desc); return; }
            _logger.Begin();
            _logger.LogPattern("David.Claude", version, project);

            // Init config
            string path;
            if (configPath != "") { path = configPath; }
            else { path = FF.StringAppend(basicPath, @"..\..\DacLib\Hoxis\Configs\hoxis_server.toml"); }
            config = new TomlConfiguration(path, out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server", true); return; }
            _logger.LogInfo("read configuration success", "Server");

            // Assign ip, port and init the sokcet
            ip = SF.GetLocalIP(out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server"); return; }
            port = config.GetInt("server", "port", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server"); return; }
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            _logger.LogInfo(FF.StringFormat("ip is {0}, port is {1}", ip, port.ToString()), "Server");

            // Init affair queue
            int affairQueueCapacity = config.GetInt("server", "affair_queue_capacity", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server"); return; }
            short affairQueueProcessQuantity = config.GetShort("server", "affair_queue_process_quantity", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server"); return; }
            affairQueueProcessInterval = config.GetInt("server", "affair_queue_process_interval", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server"); return; }
            _affairQueue = new FiniteProcessQueue<KV<string, object>>(affairQueueCapacity, affairQueueProcessQuantity);
            _affairQueue.onProcess += ProcessAffair;
            _logger.LogInfo(
                FF.StringFormat("affair queue capacity is {0}, processing quantity is {1}, interval is {2}ms",
                affairQueueCapacity,
                affairQueueProcessQuantity,
                affairQueueProcessInterval),
                "Server"
                );

            // Init connection reception
            maxConn = config.GetInt("server", "max_conn", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server"); return; }
            _connReception = new CriticalPreformPool<HoxisConnection>(maxConn);
            _logger.LogInfo(FF.StringFormat("max connections is {0}", maxConn), "Server");

            // Init connection
            HoxisConnection.readBufferSize = config.GetInt("conn", "read_buffer_size", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server"); return; }
            _logger.LogInfo(FF.StringFormat("read buffer size of connection is {0}", HoxisConnection.readBufferSize), "Server");

            // Init cluster
            _clusters = new Dictionary<string, HoxisCluster>();
            HoxisCluster.maxUser = config.GetInt("cluster", "max_user", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server"); return; }
            _logger.LogInfo(FF.StringFormat("max users of cluster is {0}", HoxisCluster.maxUser), "Server");

            // Init team
            HoxisTeam.maxUser = config.GetInt("team", "max_user", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server"); return; }
            _logger.LogInfo(FF.StringFormat("max users of team is {0}", HoxisTeam.maxUser), "Server");

            // Init user
            HoxisUser.requestTTL = config.GetLong("user", "request_ttl", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server"); return; }
            _logger.LogInfo(FF.StringFormat("request time to live is {0}ms", HoxisUser.requestTTL), "Server");
            HoxisUser.heartbeatTimeout = config.GetInt("user", "heartbeat_timeout", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server"); return; }
            _logger.LogInfo(FF.StringFormat("heartbeat timeout is {0}ms", HoxisUser.heartbeatTimeout), "Server");

            _logger.LogInfo("init success", "Server", true);
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
                _logger.LogInfo("listen successful", "Server", true);
            }
            catch (Exception e) { _logger.LogError(e.Message, "Server", true); }
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
                    Socket socket = _socket.Accept();
                    _logger.LogInfo(FF.StringAppend("accept new client: ", socket.RemoteEndPoint.ToString()), "Server");
                    Ret ret;
                    HoxisConnection conn = _connReception.Request(socket, out ret);
                    if (ret.code != 0) { _logger.LogWarning(ret.desc, socket.RemoteEndPoint.ToString()); continue; }
                    _logger.LogInfo("request successful", conn.clientIP);
                }
            });
            _acceptThread.Start();
            _logger.LogInfo("accept begin...", "Server", true);
        }

        /// <summary>
        /// Begin processing affairs with thread
        /// </summary>
        public static void BeginProcessAffair()
        {
            _affairThread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(affairQueueProcessInterval);
                    _affairQueue.ProcessInRound();
                }
            });
            _affairThread.Start();
            _logger.LogInfo("process affairs begin...", "Server", true);
        }

        /// <summary>
        /// Entrance of affairs
        /// </summary>
        /// <param name="affair"></param>
        public static void AffairEntry(KV<string,object> affair){ _affairQueue.Enqueue(affair); }
        public static void AffairEntry(string name, object obj) { AffairEntry(new KV<string, object>(name, obj)); }

        #region management
        /// <summary>
        /// Get all working connections 
        /// </summary>
        /// <returns></returns>
        public static List<HoxisConnection> GetWorkingConnections() { return _connReception.GetWorkers(); }

        /// <summary>
        ///  Get working user by uid
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static HoxisUser GetUser(long uid)
        {
            List<HoxisConnection> workers = GetWorkingConnections();
            foreach (HoxisConnection w in workers) { if (w.user.userID == uid && uid > 0) return w.user; }
            return null;
        }

        /// <summary>
        /// Release an user
        /// Generally called when an user quits, reconnects or stops heartbeats
        /// </summary>
        /// <param name="user"></param>
        public static void ReleaseConnection(HoxisConnection conn)
        {
            Ret ret;
            _connReception.Release(conn, out ret);
            if (ret.code != 0) { _logger.LogWarning(ret.desc, ""); return; }
            _logger.LogInfo("released successful", "Server");
        }

        //public static bool ManageCluster(ManageOperation op, HoxisUser sponsor)
        //{
        //    switch (op)
        //    {
        //        case ManageOperation.Create:
        //            string cid = FF.StringAppend(sponsor.userID.ToString(), "@", SF.GetTimeStamp().ToString());
        //            if (_clusters.ContainsKey(cid)) { Console.WriteLine("[error]Create cluster: cluster {0} already exists", cid); return false; }
        //            lock (_clusters)
        //            {
        //                HoxisCluster hc = new HoxisCluster(cid);
        //                _clusters.Add(cid, hc);
        //                Ret ret;
        //                hc.UserJoin(sponsor, out ret);
        //                if (ret.code != 0) { Console.WriteLine("[warning]Create cluster: {0}", ret.desc); return false; }
        //            }
        //            break;
        //        case ManageOperation.Join:
        //            // todo Call matching sdk, get a cluster
        //            break;
        //        case ManageOperation.Leave:

        //            break;
        //        case ManageOperation.Destroy:

        //            break;
        //    }
        //    return true;
        //}

        #endregion

        /// <summary>
        /// Stop the service saftly
        /// </summary>
        public static void Quit()
        {
            _logger.End();
            _acceptThread.Abort();
            _affairThread.Abort();
        }

        public static void LogConnectionStatus()
        {
            List<HoxisConnection> conns = _connReception.GetWorkers();
            foreach (HoxisConnection c in conns)
            {
                Console.WriteLine("Local ID: {0}\nState: {1}\n", c.localID, c.user.connectionState);
            }
        }

        private static void ProcessAffair(object state)
        {
            KV<string, object> affair = (KV<string, object>)state;
            switch (affair.key)
            {
                case "release connection":
                    HoxisConnection conn = (HoxisConnection)affair.val;
                    ReleaseConnection(conn);
                    break;
                default:
                    Console.WriteLine("Unknown affair: {0}", affair.key);
                    break;
            }
        }
    }
}
