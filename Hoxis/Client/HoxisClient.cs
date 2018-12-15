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
        public const byte RET_CONNECT_EXCEPTION = 1;
        public const byte RET_CLOSE_EXCEPTION = 2;
        public const string ERR_MSG_SOCKET_NOT_UNINITIALIZED = "socket is uninitialized";
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
        public static string basicPath { get { return UnityEngine.Application.dataPath + "/DacLib/Hoxis/"; } }

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
        /// **WITHIN THREAD**
        /// Event of connecting error
        /// </summary>
        public static event RetForVoid_Handler onConnectError;

        /// <summary>
        /// **WITHIN THREAD**
        /// Event of closing error
        /// </summary>
        public static event RetForVoid_Handler onCloseError;

        private static Socket _socket;
        private static HoxisBytesExtractor _extractor;
        private static Thread _receiveThread;

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

            // Init extractor
            int size = config.GetInt("socket", "read_buffer_size", out ret);
            if (ret.code != 0) { OnInitError(ret); return; }
            _extractor = new HoxisBytesExtractor(size);
            _extractor.onBytesExtracted += OnExtract;

            // Init HoxisDirector
            HoxisDirector.protocolQueueCapacity = config.GetInt("director", "protocol_queue_capacity", out ret);
            if (ret.code != 0) { OnInitError(ret); return; }
            HoxisDirector.protocolQueueProcessQuantity = config.GetShort("director", "protocol_queue_process_quantity", out ret);
            if (ret.code != 0) { OnInitError(ret); return; }
        }

        public static void Connect()
        {
            try
            {
                IPAddress addr = IPAddress.Parse(serverIP);
                IPEndPoint ep = new IPEndPoint(addr, port);
                if (_socket == null) { throw new Exception(ERR_MSG_SOCKET_NOT_UNINITIALIZED); }
                _socket.Connect(ep);
                OnConnected();
                LoopReceive();
                HoxisDirector.Ins.onPost += Send;
            }
            catch (Exception e) { OnConnectError(new Ret(LogLevel.Error, RET_CONNECT_EXCEPTION, e.Message)); }
        }

        /// <summary>
        /// Loop receiving data synchronously
        /// </summary>
        public static void LoopReceive()
        {
            if (_receiveThread != null) _receiveThread.Abort();
            _receiveThread = new Thread(() => {
                while (true)
                {
                    int len = _socket.Receive(_extractor.readBytes, _extractor.readCount, _extractor.remainCount, SocketFlags.None);
                    _extractor.Extract(len);
                }
            });
            _receiveThread.Start();
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

        public static void Close()
        {
            if (_receiveThread != null) _receiveThread.Abort();
            if (_socket == null) return;
            if (!isConnected) return;
            try
            {
                HoxisDirector.Ins.onPost -= Send;
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            catch (Exception e) { OnCloseError(new Ret(LogLevel.Error, RET_CLOSE_EXCEPTION, e.Message)); }
        }

        #region private functions

        /// <summary>
        /// Callback of extracting protocol data
        /// </summary>
        /// <param name="data"></param>
        private static void OnExtract(byte[] data) { HoxisDirector.Ins.ProtocolEntry(data); }

        private static void OnInitError(Ret ret) { if (onInitError == null) return; onInitError(ret); }
        private static void OnConnected() { if (onConnected == null) return; onConnected(); }
        private static void OnConnectError(Ret ret) { if (onConnectError == null) return; onConnectError(ret); }
        private static void OnCloseError(Ret ret) { if (onCloseError == null) return; onCloseError(ret); }
        #endregion
    }
}