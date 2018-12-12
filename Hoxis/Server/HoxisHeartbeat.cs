using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using DacLib.Generic;

namespace DacLib.Hoxis.Server
{
    public class HoxisHeartbeat
    {
        /// <summary>
        /// Threshold of timeout
        /// </summary>
        public int timeout { get; }

        /// <summary>
        /// Is the heartbeat enable ?
        /// </summary>
        public bool enable { get; private set; }

        /// <summary>
        /// Event of timeout
        /// </summary>
        public event IntForVoid_Handler onTimeout;

        private Thread _thread;
        private int _time = 0;

        public HoxisHeartbeat(int timeoutArg)
        {
            if (timeoutArg <= 0) { timeout = 5000; }
            else timeout = timeoutArg;
            enable = false;
            _thread = new Thread(TimerRun);
        }
        public void Start()
        {
            if (enable) return;
            enable = true;
            _thread.Start();
        }

        public void Refresh() { lock (this) { _time = 0; } }

        public void Stop()
        {
            if (!enable) return;
            enable = false;
        }

        public void Reset()
        {
            Stop();
            _thread = new Thread(TimerRun);
            _time = 0;
        }

        private void TimerRun()
        {
            while (enable)
            {
                Thread.Sleep(200);
                _time += 200;
                if (_time > timeout) { OnTimeout(_time); }
            }
        }
        private void OnTimeout(int time) { if (onTimeout == null) return; onTimeout(time); }
    }
}