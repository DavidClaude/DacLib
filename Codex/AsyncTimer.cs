using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DacLib.Generic
{
    /// <summary>
    /// Asynchronous timer
    /// Unit: millisecond
    /// </summary>
    public class AsyncTimer
    {
        public int timeout { get; }
        public int time { get; private set; }
        public bool active { get { if (_thread == null) return false; return _thread.IsAlive; } }
        public NoneForVoid_Handler timeoutCallback { get; }
        private Thread _thread;

        public AsyncTimer(int timeoutArg, NoneForVoid_Handler timeoutCallbackArg)
        {
            if (timeoutArg <= 0) timeout = 5000;
            else timeout = timeoutArg;
            timeoutCallback = timeoutCallbackArg;
        }
        public void Begin()
        {
            if (active) return;
            _thread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(200);
                    time += 200;
                    if (time > timeout) { timeoutCallback(); Refresh(); }
                }
            });
            _thread.Start();
        }
        public void Refresh() { lock (this) { time = 0; } }
        public void End() { if (active) _thread.Abort(); }
    }
}