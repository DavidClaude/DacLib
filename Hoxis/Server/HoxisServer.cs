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
        /// Hoxis server basic direction
        /// </summary>
        public static string basicPath { get { return AppDomain.CurrentDomain.BaseDirectory + @"\..\..\DacLib\Hoxis"; } }

        private static Socket _socket;
        private static CriticalPreformPool<HoxisUser> _userReception;
        private static Dictionary<string, HoxisCluster> _clusters;

        /// <summary>
        /// Init the configuration, such as the ip, port, socket
        /// </summary>
        /// <param name="configPath"></param>
        public static void InitConfig(string configPath = "")
        {
            Ret ret;
            // Init config
            string path;
            if (configPath != "") { path = configPath; }
            else { path = basicPath + @"\Configs\hoxis_server.toml"; }
            config = new TomlConfiguration(path, out ret);
            if (ret.code != 0) { Console.WriteLine("[error]HoxisServer init: {0}", ret.desc); return; }

            // Assign ip, port and init the sokcet
            ip = SystemFunc.GetLocalIP(out ret);
            if (ret.code != 0) { Console.WriteLine("[error]HoxisServer init: {0}", ret.desc); return; }
            port = config.GetInt("server", "port", out ret);
            if (ret.code != 0) { Console.WriteLine("[error]HoxisServer init: {0}", ret.desc); return; }
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

            // Init user reception
            maxConn = config.GetInt("server", "max_conn_user_quantity", out ret);
            if (ret.code != 0) { Console.WriteLine("[error]HoxisServer init: {0}", ret.desc); return; }
            _userReception = new CriticalPreformPool<HoxisUser>(maxConn);

            // Init cluster
            _clusters = new Dictionary<string, HoxisCluster>();
            HoxisCluster.maxUser = config.GetInt("server", "max_cluster_user_quantity", out ret);
            if (ret.code != 0) { Console.WriteLine("[error]HoxisServer init: {0}", ret.desc); return; }

            // Init team
            HoxisTeam.maxUser = config.GetInt("server", "max_team_user_quantity", out ret);
            if (ret.code != 0) { Console.WriteLine("[error]HoxisServer init: {0}", ret.desc); return; }

            // Init user
            HoxisUser.requestTimeoutSec = config.GetInt("protocol", "request_timeout", out ret);
            if (ret.code != 0) { Console.WriteLine("[error]HoxisServer init: {0}", ret.desc); return; }

            // Init connection
            HoxisConnection.readBufferSize = config.GetInt("conn", "read_buffer_size", out ret);
            if (ret.code != 0) { Console.WriteLine("[error]HoxisServer init: {0}", ret.desc); return; }

            Console.WriteLine("Configurations init success, server IP: {0}, port: {1}", ip, port.ToString());
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
                Console.WriteLine("Listen success, max connection count: {0}", maxConn.ToString());
            }
            catch (Exception e) { Console.Write("[error]HoxisServer listen: {0}", e.Message); }
        }

        /// <summary>
        /// Begin accept socket connection within thread
        /// </summary>
        public static void BeginAccept()
        {
            Thread t = new Thread(() =>
            {
                while (true)
                {
                    Socket cs = _socket.Accept();

                    // LOG instead
                    Console.WriteLine("[test]New client: " + cs.RemoteEndPoint.ToString());

                    Ret ret;
                    HoxisUser user = _userReception.Request(cs, out ret);
                    if (ret.code != 0) { Console.WriteLine("[error]HoxisServer user request: {0}, socekt: {1}", ret.desc, cs.RemoteEndPoint); break; }
                }
            });
            t.Start();
            Console.WriteLine("Begin accepting connection...");
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
            Ret ret;
            _userReception.Release(user, out ret);
            if (ret.code != 0) { Console.WriteLine("[error]HoxisServer user release: {0}, socekt: {1}", ret.desc, user.connection.remoteEndPoint); }
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


    }
}
