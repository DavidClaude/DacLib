using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DacLib.Generic;

namespace DacLib.Hoxis.Server
{
    public class HoxisConnection
    {
        #region ret codes
        public const byte RET_DISCONNECTED = 1;
        public const string ERR_MSG_DISCONNECTED = "Socket is disconnected";
        #endregion

        /// <summary>
        /// The size of read buffer
        /// Set by HoxisServer when initializing
        /// </summary>
        public static int readBufferSize { get; set; }

        /// <summary>
        /// Information of connection
        /// </summary>
        public string remoteEndPoint { get; private set; }

        /// <summary>
        /// Is the client connected ?
        /// </summary>
        public bool isConnected { get { return _socket.Connected; } }

        /// <summary>
        /// Event of bytes extracted
        /// Observed by superior HoxisUser
        /// </summary>
        public event BytesForVoid_Handler onExtract;

        private Socket _socket;
        private HoxisBytesExtractor _extractor;
        private Thread _receiveThread;

        public HoxisConnection(Socket socketArg)
        {
            remoteEndPoint = socketArg.RemoteEndPoint.ToString();
            _socket = socketArg;
            _extractor = new HoxisBytesExtractor(readBufferSize);
            _extractor.onBytesExtracted += OnExtract;
            LoopReceive();
            //BeginReceive();
        }

        /// <summary>
        /// Begin receiving data asynchronously
        /// </summary>
        public void BeginReceive()
        {
            try
            {
                if (!isConnected) { throw new Exception(ERR_MSG_DISCONNECTED); }
                _socket.BeginReceive(_extractor.readBytes, _extractor.readCount, _extractor.remainCount, SocketFlags.None, new AsyncCallback(ReceiveCb), null);

                Console.WriteLine("BeginReceive called");

            }
            catch (Exception e) { Console.WriteLine("[error]Begin receive @{0}: {1}", remoteEndPoint, e.Message); }
        }

        /// <summary>
        /// Loop receiving data synchronously
        /// </summary>
        public void LoopReceive()
        {
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
        /// Begin sending data asynchronously
        /// </summary>
        /// <param name="protoData"></param>
        public void BeginSend(byte[] protoData)
        {
            int len = protoData.Length;
            if (len <= 0) return;
            byte[] header = FormatFunc.IntToBytes(len);
            byte[] data = FormatFunc.BytesConcat(header, protoData);
            try { _socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCb), null); }
            catch (Exception e) { Console.WriteLine("[error]Begin send @{0}: {1}", remoteEndPoint, e.Message); }
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
            catch (Exception e) { Console.WriteLine("[error]Socket close @{0}: {1}", remoteEndPoint, e.Message); }
        }

        #region private functions

        /// <summary>
        /// Callback of receiving data
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCb(IAsyncResult ar)
        {
            Console.WriteLine("ReceiveCb called");
            try
            {
                int len = _socket.EndReceive(ar);
                _extractor.Extract(len);
            }
            catch (Exception e)
            {
                _extractor.Init();
                Console.WriteLine("[error]End receive @{0}: {1}", remoteEndPoint, e.Message);
            }
            finally { BeginReceive(); }
        }

        /// <summary>
        /// Callback of sending data
        /// </summary>
        /// <param name="ar"></param>
        private void SendCb(IAsyncResult ar)
        {
            try { _socket.EndSend(ar); }
            catch (Exception e) { Console.WriteLine("[error]End send @{0}: {1}", remoteEndPoint, e.Message); }
        }

        private void OnExtract(byte[] data) { if (onExtract == null) return; onExtract(data); }

        #endregion
    }
}
