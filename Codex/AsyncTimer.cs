using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DacLib.Generic
{
    /// <summary>
    /// Asynchronous timer
    /// Caller should register a callback on onTimeout
    /// </summary>
    public class AsyncTimer
    {
        public int timeout { get; }
        public int time { get; private set; }
        public bool enable { get { if (_thread == null) return false; return _thread.IsAlive; } }
        public event NoneForVoid_Handler onTimeout;
        private Thread _thread;

        public AsyncTimer(int timeoutArg)
        {
            if (timeoutArg <= 0) { timeout = 5000; }
            else timeout = timeoutArg;
        }
        public void Begin()
        {
            if (_thread.IsAlive) return;
            _thread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(200);
                    time += 200;
                    if (time > timeout) { OnTimeout(); }
                }
            });
            _thread.Start();
        }
        public void Refresh() { lock (this) { time = 0; } }
        public void End() { if (_thread.IsAlive) _thread.Abort(); }

        private void OnTimeout() { if (onTimeout == null) return; onTimeout(); }
    }
}