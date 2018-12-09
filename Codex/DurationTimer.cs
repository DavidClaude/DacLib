using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DacLib.Codex
{
    public class DurationTimer
    {
        public static readonly DurationTimer Ins = new DurationTimer();

        private DateTime _startDT = DateTime.MinValue;
        private DateTime _stopDT = DateTime.MinValue;
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
        public void Reset()
        {
            _startDT = DateTime.MinValue;
            _stopDT = DateTime.MinValue;
            _isRunning = false;
        }
        public TimeSpan GetDuration() { return _stopDT - _startDT; }
        public TimeSpan Tag() { return DateTime.Now - _startDT; }
    }
}