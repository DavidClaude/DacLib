using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using DacLib.Generic;
using DacLib.Codex;

using C = DacLib.Hoxis.Consts;

namespace DacLib.Hoxis.Client
{
    public class HoxisClient
    {
        #region ret codes
        public const byte RET_CONNECT_EXCEPTION = 1;
        public const byte RET_CLOSE_EXCEPTION = 2;
        public const string ERR_MSG_SOCKET_NOT_UNINITIALIZED = "socket is uninitialized";
        #endregion

        /// <summary>
        /// Singleton
        /// </summary>
        public static HoxisClient Ins { get; private set; }

        /// <summary>
        /// Hoxis client configuration
        /// </summary>
        public TomlConfiguration config { get; private set; }

        /// <summary>
        /// IP of Hoxis server which is used for synchronization
        /// </summary>
        public string serverIP { get; private set; }

        /// <summary>
        /// Port of Hoxis server
        /// </summary>
        public int port { get; private set; }

        /// <summary>
        /// Is the client connected ?
        /// </summary>
        public bool isConnected { get { return _socket.Connected; } }

        /// <summary>
        /// Hoxis client basic direction
        /// </summary>
        public static string basicPath { get { return UnityEngine.Application.dataPath + "/DacLib/Hoxis/"; } }

        public event RetForVoid_Handler onInitError;
        public event NoneForVoid_Handler onConnected;
        public event RetForVoid_Handler onConnectError;
        public event NoneForVoid_Handler onClose;
        public event RetForVoid_Handler onCloseError;
        public event ExceptionHandler onNetworkAnomaly;

        private Socket _socket;
        private HoxisBytesExtractor _extractor;
        private Thread _receiveThread;

        public HoxisClient(bool autoStart)
        {
            if (Ins == null) Ins = this;
            if (autoStart){InitializeConfig();}
        }


        /// <summary>
        /// Init the configuration, such as the ip, port, socket
        /// </summary>
        /// <param name="ret"></param>
        public void InitializeConfig(string configPath = "")
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
            HoxisDirector.affairQueueCapacity = config.GetInt("director", "affair_queue_capacity", out ret);
            if (ret.code != 0) { OnInitError(ret); return; }
            HoxisDirector.affairQueueProcessQuantity = config.GetShort("director", "affair_queue_process_quantity", out ret);
            if (ret.code != 0) { OnInitError(ret); return; }
            HoxisDirector.heartbeatInterval = config.GetFloat("director", "heartbeat_interval", out ret);
            if (ret.code != 0) { OnInitError(ret); return; }
        }

        public void Connect()
        {
            try
            {
                IPAddress addr = IPAddress.Parse(serverIP);
                IPEndPoint ep = new IPEndPoint(addr, port);
                if (_socket == null) { throw new Exception(ERR_MSG_SOCKET_NOT_UNINITIALIZED); }
                _socket.Connect(ep);
                OnConnected();
                LoopReceive();
            }
            catch (Exception e) { OnConnectError(new Ret(LogLevel.Error, RET_CONNECT_EXCEPTION, e.Message)); }
        }

        public void BeginConnnect()
        {
            try
            {
                IPAddress addr = IPAddress.Parse(serverIP);
                IPEndPoint ep = new IPEndPoint(addr, port);
                if (_socket == null) { throw new Exception(ERR_MSG_SOCKET_NOT_UNINITIALIZED); }
                _socket.BeginConnect(ep, ConnectCb, null);
            }
            catch (Exception e) { HoxisDirector.Ins.AffairEntry(C.AFFAIR_CONNECT_ERROR, new Ret(LogLevel.Error, RET_CONNECT_EXCEPTION, e.Message)); }
        }

        private void ConnectCb(IAsyncResult ar)
        {
            try
            {
                _socket.EndConnect(ar);
                HoxisDirector.Ins.AffairEntry(C.AFFAIR_CONNECT, null);
                LoopReceive();
            }
            catch (Exception e) { HoxisDirector.Ins.AffairEntry(C.AFFAIR_CONNECT_ERROR, new Ret(LogLevel.Error, RET_CONNECT_EXCEPTION, e.Message)); }
        }

        /// <summary>
        /// **WITHIN THREAD**
        /// Loop receiving data synchronously
        /// </summary>
        public void LoopReceive()
        {
            _receiveThread = new Thread(() =>
            {
                while (true)
                {
                    try { int len = _socket.Receive(_extractor.readBytes, _extractor.readCount, _extractor.remainCount, SocketFlags.None); _extractor.Extract(len); }
                    catch (SocketException e) { OnNetworkAnomaly(e.ErrorCode, e.Message); break; }
                }
            });
            _receiveThread.Start();
        }

        /// <summary>
        /// Send data synchronously
        /// </summary>
        /// <param name="protoData"></param>
        public void Send(byte[] protoData)
        {
            int len = protoData.Length;
            if (len <= 0) return;
            byte[] header = FormatFunc.IntToBytes(len);
            byte[] data = FormatFunc.BytesConcat(header, protoData);
            try { _socket.Send(data); }
            catch (SocketException e) { OnNetworkAnomaly(e.ErrorCode, e.Message); }
        }

        /// <summary>
        /// Close the socket
        /// </summary>
        public void Close()
        {
            if (_receiveThread != null) _receiveThread.Abort();
            if (_socket == null) return;
            if (!isConnected) return;
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            catch (SocketException e) { OnCloseError(new Ret(LogLevel.Error, RET_CLOSE_EXCEPTION, e.Message)); }
            OnClose();
        }



        #region private functions

        /// <summary>
        /// Callback of extracting protocol data
        /// </summary>
        /// <param name="data"></param>
        private void OnExtract(byte[] data) { HoxisDirector.Ins.ProtocolEntry(data); }

        private void OnInitError(Ret ret) { if (onInitError == null) return; onInitError(ret); }
        private void OnConnected() { if (onConnected == null) return; onConnected(); }
        private void OnConnectError(Ret ret) { if (onConnectError == null) return; onConnectError(ret); }
        private void OnNetworkAnomaly(int code, string message) { if (onNetworkAnomaly == null) return; onNetworkAnomaly(code, message); }
        private void OnClose() { if (onClose == null) return; onClose(); }
        private void OnCloseError(Ret ret) { if (onCloseError == null) return; onCloseError(ret); }
        
        #endregion
    }
}