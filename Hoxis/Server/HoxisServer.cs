using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using DacLib.Generic;
using DacLib.Codex;


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

        public static string ip { get; private set; }

        public static int port { get; private set; }

        /// <summary>
        /// Hoxis server basic direction
        /// </summary>
        public static string basicPath { get { return AppDomain.CurrentDomain.BaseDirectory + @"DacLib\Hoxis"; } }

        private static Socket _socket;

        /// <summary>
        /// Init the configuration, such as the ip, port, socket
        /// </summary>
        /// <param name="configPath"></param>
        public static void InitConfig(string configPath = "")
        {
            // Read config file
            Ret ret;
            string path;
            if (configPath != "") { path = configPath; }
            else { path = basicPath + @"\Configs\hoxis_server.toml"; }
            config = new TomlConfiguration(path, out ret);
            if (ret.code != 0) { Console.Write("[error]HoxisServer init: " + ret.desc); return; }
            // Assign ip, port and init the sokcet
            ip = SystemFunc.GetLocalIP(out ret);
            if (ret.code != 0) { Console.Write("[error]HoxisServer init: " + ret.desc); return; }
            port = config.GetInt("socket", "port", out ret);
            if (ret.code != 0) { Console.Write("[error]HoxisServer init: " + ret.desc); return; }
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            Console.WriteLine("Configurations init success, server IP is {0}, port is {1}", ip, port.ToString());
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
                Console.WriteLine("Listen success");
            }
            catch (Exception e) { Console.Write("[error]HoxisServer listen: " + e.Message); }
        }

        /// <summary>
        /// Begin accept socket connection within thread
        /// </summary>
        public static void BeginAccept()
        {
            Thread t = new Thread(() =>
            {
                while (true) {
                    Socket cs = _socket.Accept();
                    Console.Write("New client: " + cs.RemoteEndPoint.ToString());
                }
            });
            t.Start();
            Console.WriteLine("Begin accepting connection...");
        }
    }
}
