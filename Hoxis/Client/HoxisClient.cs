using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using DacLib.Generic;
using DacLib.Codex;

namespace DacLib.Hoxis.Client
{
    public static class HoxisClient
    {
        #region ret codes
        public const byte RET_CONNECT_BEGIN_EXCEPTION = 1;
        public const byte RET_CONNECT_END_EXCEPTION = 2;
        public const byte RET_RECEIVE_BEGIN_EXCEPTION = 3;
        public const byte RET_RECEIVE_END_EXCEPTION = 4;
        public const byte RET_SEND_BEGIN_EXCEPTION = 5;
        public const byte RET_SEND_END_EXCEPTION = 6;
        public const byte RET_CFG_UNINITIALIZED = 7;
        public const byte RET_DISCONNECTED = 8;
        public const string ERR_MSG_CFG_UNINITIALIZED = "Configuration file should be initialized first";
        public const string ERR_MSG_DISCONNECTED = "Socket is disconnected";
        #endregion

        /// <summary>
        /// Hoxis client configuration
        /// </summary>
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
        public static bool isConnected { get { return _socket.Connected; } }

        /// <summary>
        /// Hoxis client basic direction
        /// </summary>
        public static string basicPath { get { return UnityEngine.Application.dataPath + "/DacLib/Hoxis"; } }

        /// <summary>
        /// Event of bytes extracted
        /// </summary>
        public static event BytesForVoid_Handler onExtract;

        /// <summary>
        /// Event of initializing error
        /// </summary>
        public static event RetForVoid_Handler onInitError;

        /// <summary>
        /// **WITHIN THREAD**
        /// Event of connecting success
        /// </summary>
        public static event NoneForVoid_Handler onConnected;

        /// <summary>
        /// **MAY WITHIN THREAD**
        /// Event of connecting error
        /// </summary>
        public static event RetForVoid_Handler onConnectError;

        /// <summary>
        /// **MAY WITHIN THREAD**
        /// Event of receiving error
        /// </summary>
        public static event RetForVoid_Handler onReceiveError;

        /// <summary>
        /// **MAY WITHIN THREAD**
        /// Event of sending error
        /// </summary>
        public static event RetForVoid_Handler onSendError;

        private static Socket _socket;
        private static HoxisBytesExtractor _extractor;

        /// <summary>
        /// Init the configuration, such as the ip, port, socket
        /// </summary>
        /// <param name="ret"></param>
        public static void InitConfig(string configPath = "")
        {
            Ret ret;
            // Read config file
            string path;
            if (configPath != "") { path = configPath; }
            else { path = basicPath + "/Configs/hoxis_client.toml"; }
            config = new TomlConfiguration(path, out ret);
            if (ret.code != 0) { OnInitError(ret); return; }

            // Assign ip, port and init the sokcet
            serverIP = config.GetString("socket", "server_ip", out ret);
            if (ret.code != 0) { OnInitError(ret); return; }
            port = config.GetInt("socket", "port", out ret);
            if (ret.code != 0) { OnInitError(ret); return; }
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

            // Init the extractor
            int size = config.GetInt("socket", "read_buffer_size", out ret);
            if (ret.code != 0) { OnInitError(ret); return; }
            _extractor = new HoxisBytesExtractor(size);
            _extractor.onBytesExtracted += OnExtract;
        }

        /// <summary>
        /// Connect to server asynchronously
        /// </summary>
        public static void BeginConnect()
        {
            try
            {
                IPAddress addr = IPAddress.Parse(serverIP);
                IPEndPoint ep = new IPEndPoint(addr, port);
                if (_socket == null) { throw new Exception(ERR_MSG_CFG_UNINITIALIZED); }
                _socket.BeginConnect(ep, new AsyncCallback(ConnectCb), null);
            }
            catch (Exception e) { OnConnectError(new Ret(LogLevel.Error, RET_CFG_UNINITIALIZED, e.Message)); }
        }

        /// <summary>
        /// Begin receiving data asynchronously
        /// </summary>
        public static void BeginReceive()
        {
            try
            {
                if (!isConnected) { throw new Exception(ERR_MSG_DISCONNECTED); }
                _socket.BeginReceive(_extractor.readBytes, _extractor.readCount, _extractor.remainCount, SocketFlags.None, new AsyncCallback(ReceiveCb), null);
            }
            catch (Exception e) { OnReceiveError(new Ret(LogLevel.Error, RET_RECEIVE_BEGIN_EXCEPTION, e.Message)); }
        }

        /// <summary>
        /// Begin sending data asynchronously
        /// </summary>
        /// <param name="protoData"></param>
        public static void BeginSend(byte[] protoData)
        {
            int len = protoData.Length;
            if (len <= 0) return;
            byte[] header = FormatFunc.IntToBytes(len);
            byte[] data = FormatFunc.BytesConcat(header, protoData);
            try { _socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCb), null); }
            catch (Exception e) { OnSendError(new Ret(LogLevel.Error, RET_SEND_BEGIN_EXCEPTION, e.Message)); }
        }

        /// <summary>
        /// Send data synchronously
        /// </summary>
        /// <param name="protoData"></param>
        public static void Send(byte[] protoData)
        {
            int len = protoData.Length;
            if (len <= 0) return;
            byte[] header = FormatFunc.IntToBytes(len);
            byte[] data = FormatFunc.BytesConcat(header, protoData);
            _socket.Send(data);
        }

        #region private functions

        /// <summary>
        /// **WITHIN THREAD**
        /// Callback of connecting successfully
        /// </summary>
        /// <param name="ar"></param>
        private static void ConnectCb(IAsyncResult ar)
        {
            try
            {
                _socket.EndConnect(ar);
                OnConnected();
            }
            catch (Exception e) { OnConnectError(new Ret(LogLevel.Error, RET_CONNECT_END_EXCEPTION, e.Message)); }
        }

        /// <summary>
        /// Callback of receiving data
        /// </summary>
        /// <param name="ar"></param>
        private static void ReceiveCb(IAsyncResult ar)
        {
            try
            {
                int len = _socket.EndReceive(ar);
                _extractor.Extract(len);
            }
            catch (Exception e)
            {
                _extractor.Init();
                OnReceiveError(new Ret(LogLevel.Error, RET_RECEIVE_END_EXCEPTION, e.Message));
            }
            finally { BeginReceive(); }
        }

        /// <summary>
        /// Callback of sending data
        /// </summary>
        /// <param name="ar"></param>
        private static void SendCb(IAsyncResult ar)
        {
            try { _socket.EndSend(ar); }
            catch (Exception e) { OnSendError(new Ret(LogLevel.Error, RET_RECEIVE_END_EXCEPTION, e.Message)); }
        }

        /// <summary>
        /// Callback of extracting protocol data
        /// </summary>
        /// <param name="data"></param>
        private static void OnExtract(byte[] data) { onExtract(data); }

        private static void OnInitError(Ret ret) { if (onInitError == null) return; onInitError(ret); }
        private static void OnConnected() { if (onConnected == null) return; onConnected(); }
        private static void OnConnectError(Ret ret) { if (onConnectError == null) return; onConnectError(ret); }
        private static void OnReceiveError(Ret ret) { if (onReceiveError == null) return; onReceiveError(ret); }
        private static void OnSendError(Ret ret) { if (onSendError == null) return; onSendError(ret); }
        #endregion
    }
}