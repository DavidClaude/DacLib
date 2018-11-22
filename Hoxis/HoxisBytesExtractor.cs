using System;
using DacLib.Generic;
using DacLib.Hoxis.Client;

namespace DacLib.Hoxis
{
    public class HoxisBytesExtractor
    {
        /// <summary>
        /// The byte count of header which indicates the length of data
        /// </summary>
        public const int HEADER_COUNT = 4;

        /// <summary>
        /// The byte count of read buffer
        /// </summary>
        public readonly int readBufferCount;

        /// <summary>
        /// The byte array of read buffer
        /// Set by caller
        /// </summary>
        public byte[] readBytes { get; set; }

        /// <summary>
        /// The byte count of data that has been read
        /// </summary>
        public int readCount { get; private set; }

        /// <summary>
        /// Get the remain of buffer
        /// </summary>
        public int remainCount { get { return readBufferCount - readCount; } }

        public event BytesForVoid_Handler onBytesExtracted;

        private byte[] _headerBytes = new byte[HEADER_COUNT];
        private int _protoLen = 0;

        public HoxisBytesExtractor(int readBufferCountArg)
        {
            readBufferCount = readBufferCountArg;
            readBytes = new byte[readBufferCount];
            readCount = 0;
        }

        /// <summary>
        /// Extract the meaningful protocol data 
        /// </summary>
        /// <param name="count"></param>
        public void Extract(int count)
        {
            readCount += count;
            // If current data is shorter than a header, next data
            if (readCount < HEADER_COUNT) return;
            // Copy the first several bytes to header bytes
            Array.Copy(readBytes, _headerBytes, HEADER_COUNT);
            // Calculate the length value of current protocol
            _protoLen = BitConverter.ToInt32(_headerBytes, 0);
            // Calculate the remain length supposing that current protocol is detached
            int remain = readCount - _protoLen - HEADER_COUNT;
            // The remain which is less than 0 means that more data need be received, next data
            if (remain < 0) return;
            // Copy current protocol data to a new array
            byte[] data = new byte[_protoLen];
            Array.Copy(readBytes, HEADER_COUNT, data, 0, _protoLen);
            // Trigger the event on protocol data extracted
            OnBytesExtracted(data);
            // Detach the extracted data and initialize
            Array.Copy(readBytes, _protoLen + HEADER_COUNT, readBytes, 0, remain);
            Init();
            // Loop above process until no more protocol could be extracted
            if (remain > 0) { Extract(remain); }
        }

        /// <summary>
        /// Initialize
        /// </summary>
        public void Init()
        {
            readCount = 0;
            _protoLen = 0;
        }

        private void OnBytesExtracted(byte[] data)
        {
            if (onBytesExtracted == null)
                return;
            onBytesExtracted(data);
        }
    }
}