using System;
using System.Net;
using System.Net.Sockets;
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

        public static TomlConfiguration config { get; private set; }

        public static string ip { get; private set; }

        public static int port { get; private set; }

        private static Socket _socket;

        public static void InitConfig(string configPath = "")
        {
            // Read config file
            Ret ret;
            string path;
            if (configPath != "") { path = configPath; }
            else { path = HoxisServerConfig.basicPath + "Configs/hoxis_server.toml"; }
            config = new TomlConfiguration(path, out ret);
            if (ret.code != 0) { Console.Write("[error]HoxisServer init: " + ret.desc); return; }
            // Assign ip, port and init the sokcet
            ip = SystemFunc.GetLocalIP(out ret);
            if (ret.code != 0) { Console.Write("[error]HoxisServer init: " + ret.desc); return; }
            port = config.GetInt("socket", "port", out ret);
            if (ret.code != 0) { Console.Write("[error]HoxisServer init: " + ret.desc); return; }
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
        }

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
            }
            catch (Exception e) { Console.Write("[error]HoxisServer listen: " + e.Message); }
        }

    }
}
