using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using DacLib.Generic;
using DacLib.Codex;

namespace DacLib.Hoxis.Client
{
    public static class HoxisClient
    {
        public static TomlConfiguration config { get; private set; }

        /// <summary>
        /// IP of Hoxis server which is used for synchronization
        /// </summary>
        public static string serverIP { get; private set; }

        /// <summary>
        /// Port of Hoxis server
        /// </summary>
        public static int port { get; private set; }

        /// <summary>
        /// Is the client connected ?
        /// </summary>
        public static bool isConnected
        {
            get { return _socket.Connected; }
        }

        /// <summary>
        /// Event of initializing error
        /// </summary>
        public static event StringForVoid_Handler onInitError;

        /// <summary>
        /// **WITHIN THREAD**
        /// Event of connecting success
        /// </summary>
        public static event NoneForVoid_Handler onConnected;

        /// <summary>
        /// **MAY WITHIN THREAD**
        /// Event of connecting error
        /// </summary>
        public static event StringForVoid_Handler onConnectError;

        private static Socket _socket;

        /// <summary>
        /// Init the configuration, such as the ip, port, socket
        /// </summary>
        /// <param name="ret"></param>
        public static void InitConfig(out Ret ret)
        {
            config = new TomlConfiguration(Configs.hoxisPath + "Configs/hoxisclient.toml", out ret);
            if (ret.code != 0) { OnInitError(ret.desc); return; }
            serverIP = config.GetString("socket", "server_ip", out ret);
            if (ret.code != 0) { OnInitError(ret.desc); return; }
            port = config.GetInt("socket", "port", out ret);
            if (ret.code != 0) { OnInitError(ret.desc); return; }
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            ret = Ret.ok;
        }

        /// <summary>
        /// Connect to server
        /// </summary>
        public static void Connect()
        {
            try
            {
                IPAddress addr = IPAddress.Parse(serverIP);
                IPEndPoint ep = new IPEndPoint(addr, port);
                _socket.BeginConnect(ep, new AsyncCallback(ConnectCb), _socket);
            }
            catch (SocketException e) { OnConnectError(e.Message); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void StartReceive()
        {

        }

        /// <summary>
        /// Send protocol bytes to server
        /// </summary>
        /// <param name="data"></param>
        public static void Send(byte[] data)
        {

        }

        #region private functions

        private static void OnInitError(string msg)
        {
            if (onInitError == null) return;
            OnInitError(msg);
        }

        private static void OnConnected()
        {
            if (onConnected == null) return;
            onConnected();
        }

        private static void OnConnectError(string msg)
        {
            if (onConnectError == null) return;
            onConnectError(msg);
        }

        /// <summary>
        /// **WITHIN THREAD**
        /// Callback function of successful connecting event
        /// </summary>
        /// <param name="ar"></param>
        private static void ConnectCb(IAsyncResult ar)
        {
            Socket s = (Socket)ar.AsyncState;
            try
            {
                s.EndConnect(ar);
                OnConnected();
            }
            catch (SocketException e) { OnConnectError(e.Message); }
        }

        #endregion
    }
}