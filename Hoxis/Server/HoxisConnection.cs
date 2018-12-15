using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DacLib.Generic;

namespace DacLib.Hoxis.Server
{
    public class HoxisConnection : IReusable
    {
        #region ret codes
        public const byte RET_DISCONNECTED = 1;
        public const string ERR_MSG_DISCONNECTED = "Socket is disconnected";
        #endregion

        #region reusable
        public int localID { get; set; }
        public bool isOccupied { get; set; }
        #endregion

        /// <summary>
        /// The size of read buffer
        /// </summary>
        public static int readBufferSize { get; set; }

        /// <summary>
        /// Information of client
        /// </summary>
        public string clientIP { get; private set; }

        /// <summary>
        /// Is the client connected ?
        /// </summary>
        public bool isConnected { get { return _socket.Connected; } }

        public HoxisUser user { get; private set; }

        private HoxisBytesExtractor _extractor;
        private Socket _socket;
        private Thread _receiveThread;

        public HoxisConnection()
        {
            _extractor = new HoxisBytesExtractor(readBufferSize);
        }

        public void OnRequest(object state)
        {
            try { _socket = (Socket)state; }
            catch (Exception e) { Console.WriteLine(e.Message); return; }
            clientIP = _socket.RemoteEndPoint.ToString();
            user = new HoxisUser();
            _extractor.onBytesExtracted += user.ProtocolEntry;
            user.onPost += Send;
            LoopReceive();
        }

        public void OnRelease()
        {
            Close();
            clientIP = string.Empty;
            _extractor.onBytesExtracted -= user.ProtocolEntry;
            user.onPost -= Send;
            user = null;
            _extractor.Init();
        }

        /// <summary>
        /// Loop receiving data synchronously
        /// </summary>
        public void LoopReceive()
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
        public void Send(byte[] protoData)
        {
            int len = protoData.Length;
            if (len <= 0) return;
            byte[] header = FormatFunc.IntToBytes(len);
            byte[] data = FormatFunc.BytesConcat(header, protoData);
            _socket.Send(data);
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
            catch (Exception e) { Console.WriteLine("[error]Socket close @{0}: {1}", clientIP, e.Message); }
        }
    }
}
