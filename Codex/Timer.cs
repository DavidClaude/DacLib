using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DacLib.Codex
{
    public class Timer
    {
        public static readonly Timer Ins = new Timer();
        private DateTime _startDT;
        private DateTime _stopDT;
        private bool _isRunning = false;

        public void Start()
        {
            if (_isRunning) return;
            _startDT = DateTime.Now;
            _isRunning = true;
        }
        public void Stop()
        {
            if (!_isRunning) return;
            _stopDT = DateTime.Now;
            _isRunning = false;
        }
        public TimeSpan GetDuration() { return _stopDT - _startDT; }
        public TimeSpan Tag() { return DateTime.Now - _startDT; }
    }
}