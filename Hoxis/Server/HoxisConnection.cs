using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DacLib.Generic;

namespace DacLib.Hoxis.Server
{
    class HoxisConnection : IReusable
    {
        #region ret codes

        public const byte RET_DISCONNECTED = 8;
        public const string ERR_MSG_DISCONNECTED = "Socket is disconnected";
        #endregion

        /// <summary>
        /// The size of read buffer
        /// Set by HoxisServer when initializing
        /// </summary>
        public static int readBufferSize { get; set; }

        /// <summary>
        /// Is the client connected ?
        /// </summary>
        public bool isConnected { get { return _socket.Connected; } }

        public bool isOccupied { get; set; }

        private Socket _socket;
        private HoxisBytesExtractor _extractor;

        public HoxisConnection()
        {
            _extractor = new HoxisBytesExtractor(readBufferSize);
            _extractor.onBytesExtracted += ExtractCb;
        }

        public void OnRelease()
        {
            _socket = null;
        }

        public void OnRequest(object state)
        {
            _socket = (Socket)state;
            BeginReceive();
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
            }
            catch (Exception e) { Console.WriteLine("[error]Begin receive @{0}: {1}", _socket.RemoteEndPoint.ToString(), e.Message); }
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
            catch (Exception e) { Console.WriteLine("[error]Begin send @{0}: {1}", _socket.RemoteEndPoint.ToString(), e.Message); }
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

        #region private functions

        /// <summary>
        /// Callback of extracting protocol data
        /// </summary>
        /// <param name="data"></param>
        private void ExtractCb(byte[] data)
        {
            // test, will delete
            Console.WriteLine("[test]Received string @{0}: {1}", _socket.RemoteEndPoint.ToString(), FormatFunc.BytesToString(data));
        }
        /// <summary>
        /// Callback of receiving data
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCb(IAsyncResult ar)
        {
            try
            {
                int len = _socket.EndReceive(ar);
                _extractor.Extract(len);
            }
            catch (Exception e)
            {
                _extractor.Init();
                Console.WriteLine("[error]End receive @{0}: {1}", _socket.RemoteEndPoint.ToString(), e.Message);
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
            catch (Exception e) { Console.WriteLine("[error]End send @{0}: {1}", _socket.RemoteEndPoint.ToString(), e.Message); }
        }

        #endregion
    }
}
