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
using C = DacLib.Hoxis.Consts;

namespace DacLib.Hoxis.Server
{
    public class HoxisServer
    {
        /// <summary>
        /// Singleton
        /// </summary>
        public static HoxisServer Ins { get; private set; }

        /// <summary>
        /// Project served
        /// </summary>
        public string project { get; }

        /// <summary>
        /// Project version
        /// </summary>
        public string version { get; }

        /// <summary>
        /// Hoxis server configuration
        /// </summary>
        public TomlConfiguration config { get; private set; }

        /// <summary>
        /// Local IP
        /// </summary>
        public string localIP { get; private set; }

        /// <summary>
        /// Local port
        /// </summary>
        public int port { get; private set; }

        /// <summary>
        /// The max count of user connection
        /// </summary>
        public int maxConn { get; private set; }

        /// <summary>
        /// Remain count of connections
        /// </summary>
        public int remainConn { get { return _connReception.remain; } }

        public int affairQueueCapacity { get; private set; }
        public short affairQueueProcessQuantity { get; private set; }
        public int affairQueueProcessInterval { get; private set; }
        public int heartbeatUpdateInterval { get; private set; }

        /// <summary>
        /// Basic direction of application
        /// </summary>
        public static readonly string basicPath = AppDomain.CurrentDomain.BaseDirectory;

        private Socket _socket;
        private CriticalPreformPool<HoxisConnection> _connReception;
        private FiniteProcessQueue<KV<int, object>> _affairQueue;
        private Dictionary<string, HoxisCluster> _clusters;
        private Thread _acceptThread;
        private Thread _affairThread;
        private Thread _heartbeatThread;
        private DebugRecorder _logger;

        public HoxisServer(string projectArg, string versionArg, bool autoStart = false)
        {
            if (Ins == null) Ins = this;

            Ret ret;
            project = projectArg;
            version = versionArg;

            // Init and begin log recording
            _logger = new DebugRecorder(FF.StringAppend(basicPath, @"logs\server.log"), out ret);
            if (ret.code != 0) { Quit(); }
            _logger.Begin();
            _logger.LogTitle("David.Claude", project, version);

            // Auto start
            if (autoStart)
            {
                InitializeConfig(out ret);
                if (ret.code != 0) { Quit(); }
                Listen();
                BeginAccept();
                BeginProcess();
                BeginHeartbeatUpdate();
            }
        }

        /// <summary>
        /// Init the configuration, such as the ip, port, socket and arguments
        /// </summary>
        /// <param name="configPath"></param>
        public void InitializeConfig(out Ret ret, string configPath = "")
        {
            // Init config
            string path;
            if (configPath != "") { path = configPath; }
            else { path = FF.StringAppend(basicPath, @"..\..\DacLib\Hoxis\Configs\hoxis_server.toml"); }
            config = new TomlConfiguration(path, out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server", true); return; }
            _logger.LogInfo("read configuration success", "Server");

            // Assign ip, port and init the sokcet
            localIP = SF.GetLocalIP(out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server"); return; }
            port = config.GetInt("server", "port", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server"); return; }
            _logger.LogInfo(FF.StringFormat("ip is {0}, port is {1}", localIP, port.ToString()), "Server");

            // Init connection reception
            maxConn = config.GetInt("server", "max_conn", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server"); return; }
            _connReception = new CriticalPreformPool<HoxisConnection>(maxConn);
            _logger.LogInfo(FF.StringFormat("max connections is {0}", maxConn), "Server");

            // Init affair queue
            affairQueueCapacity = config.GetInt("server", "affair_queue_capacity", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server"); return; }
            _logger.LogInfo(FF.StringFormat("affair queue capacity is {0}", affairQueueCapacity), "Server");
            affairQueueProcessQuantity = config.GetShort("server", "affair_queue_process_quantity", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server"); return; }
            _logger.LogInfo(FF.StringFormat("affair queue process quantity is {0}", affairQueueProcessQuantity), "Server");
            _affairQueue = new FiniteProcessQueue<KV<int, object>>(affairQueueCapacity, affairQueueProcessQuantity);
            _affairQueue.onProcess += ProcessAffair;
            affairQueueProcessInterval = config.GetInt("server", "affair_queue_process_interval", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server"); return; }
            _logger.LogInfo(FF.StringFormat("affair queue process interval is {0}ms", affairQueueProcessInterval), "Server");

            // Init heartbeat update
            heartbeatUpdateInterval = config.GetInt("server", "heartbeat_update_interval", out ret);
            if (ret.code != 0) { _logger.LogFatal(ret.desc, "Server"); return; }
            _logger.LogInfo(FF.StringFormat("heartbeat update interval is {0}ms", heartbeatUpdateInterval), "Server");

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
        public void Listen()
        {
            try
            {
                IPAddress addr = IPAddress.Parse(localIP);
                IPEndPoint ep = new IPEndPoint(addr, port);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                _socket.Bind(ep);
                int count = config.GetInt("socket", "max_client_count");
                _socket.Listen(count);
                _logger.LogInfo("listen success", "Server", true);
            }
            catch (Exception e) { _logger.LogError(e.Message, "Server", true); }
        }

        /// <summary>
        /// Begin accepting socket connection within thread
        /// </summary>
        public void BeginAccept()
        {
            _acceptThread = new Thread(() =>
            {
                while (true)
                {
                    Socket socket = _socket.Accept();
                    _logger.LogInfo(FF.StringAppend("accept new client: ", socket.RemoteEndPoint.ToString()), "Server", true);
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
        /// Begin processing affairs within thread
        /// </summary>
        public void BeginProcess()
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
            _logger.LogInfo("process begin...", "Server", true);
        }

        /// <summary>
        /// Begin heartbeat updating within thread
        /// </summary>
        public void BeginHeartbeatUpdate()
        {
            _heartbeatThread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(heartbeatUpdateInterval);
                    List<HoxisConnection> workers = GetWorkingConnections();
                    lock (workers) { foreach (HoxisConnection w in workers) { w.user.HeartbeartTimerUpdate(heartbeatUpdateInterval); } }
                }
            });
            _heartbeatThread.Start();
            _logger.LogInfo("heartbeat update begin...", "Server", true);
        }

        /// <summary>
        /// Stop the service saftly
        /// </summary>
        public void Quit()
        {
            _logger.End();
            if (_acceptThread.IsAlive) { _acceptThread.Abort(); }
            if (_affairThread.IsAlive) { _affairThread.Abort(); }
            if (_heartbeatThread.IsAlive) { _heartbeatThread.Abort(); }
        }

        /// <summary>
        /// Get all working connections 
        /// </summary>
        /// <returns></returns>
        public List<HoxisConnection> GetWorkingConnections() { return _connReception.GetWorkers(); }

        /// <summary>
        ///  Get working user by uid
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public HoxisUser GetUser(long uid)
        {
            List<HoxisConnection> workers = GetWorkingConnections();
            foreach (HoxisConnection w in workers) { if (w.user.userID == uid && uid > 0) return w.user; }
            return null;
        }

        public void LogConnectionStatus()
        {
            List<HoxisConnection> conns = _connReception.GetWorkers();
            foreach (HoxisConnection c in conns)
            {
                Console.WriteLine("Local ID: {0}\nUser ID: {1}\nState: {2}\n", c.localID, c.user.userID, c.user.connectionState);
            }
        }

        /// <summary>
        /// Entrance of affairs
        /// </summary>
        /// <param name="affair"></param>
        public void AffairEntry(KV<int, object> affair) { lock (_affairQueue) _affairQueue.Enqueue(affair); }
        public void AffairEntry(int code, object state) { AffairEntry(new KV<int, object>(code, state)); }


        //public static void TestRelease(HoxisConnection conn) { Ret ret; lock (conn) _connReception.Release(conn, out ret); }

        #region management

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

        private void ProcessAffair(object state)
        {
            KV<int, object> affair = (KV<int, object>)state;
            switch (affair.key)
            {
                case C.AFFAIR_RELEASE_CONNECTION:
                    Ret ret;
                    HoxisConnection conn = (HoxisConnection)affair.val;
                    lock (conn) { _connReception.Release(conn, out ret); }
                    if (ret.code != 0) { _logger.LogWarning(ret.desc, "Affair"); return; }
                    break;
            }
            _logger.LogInfo(FF.StringFormat("{0} processed", affair.key), "Affair", true);
        }
    }
}
