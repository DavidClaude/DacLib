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
        public bool active { get; private set; }

        private NoneForVoid_Handler _timeoutCb;
        private Thread _thread;

        public AsyncTimer(int timeoutArg)
        {
            if (timeoutArg <= 0) timeout = 5000;
            else timeout = timeoutArg;
            time = 0;
            active = false;
        }
        public void Begin(NoneForVoid_Handler callback, bool autoEnd = false)
        {
            _timeoutCb = callback;
            active = true;
            _thread = new Thread(() =>
            {
                while (active)
                {
                    Thread.Sleep(100);
                    time += 100;
                    if (time > timeout) {
                        _timeoutCb?.Invoke();
                        if (autoEnd) End();
                    }
                }
            });
            _thread.Start();
        }
        public void Refresh() { lock (this) time = 0; }
        public void End()
        {
            lock (this)
            {
                time = 0;
                active = false;
                _timeoutCb = null;
            }
        }
    }
}