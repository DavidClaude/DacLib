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

        private static Socket _socket;

        public static void InitConfig(out Ret ret)
        {
            TomlConfiguration cfg = new TomlConfiguration(UnityEngine.Application.dataPath + "/DacLib/Hoxis/Client/client.toml", out ret);
            if (ret.code != 0) { OnInitError(ret.desc); return; }
            serverIP = cfg.GetString("socket", "server_ip", out ret);
            if (ret.code != 0) { OnInitError(ret.desc); return; }
            port = cfg.GetInt("socket", "port", out ret);
            if (ret.code != 0) { OnInitError(ret.desc); return; }
            ret = Ret.ok;
        }

        public static void Connect()
        {
            try
            {
                IPAddress ipAddr = IPAddress.Parse(serverIP);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                IPEndPoint ipEP = new IPEndPoint(ipAddr, port);
                _socket.BeginConnect(ipEP, new AsyncCallback(ConnectCallback), _socket);
            }
            catch (SocketException e) { OnConnectError(e.Message); }
        }

        public static void Send(byte[] data)
        {

        }

        //private static 

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
        private static void ConnectCallback(IAsyncResult ar)
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