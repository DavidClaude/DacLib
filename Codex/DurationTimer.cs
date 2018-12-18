using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DacLib.Generic;

namespace DacLib.Codex
{
    /// <summary>
    /// Timer for measuring time span 
    /// </summary>
    public class DurationTimer : IInitializable 
    {
        public static readonly DurationTimer Ins = new DurationTimer();

        public bool isActive { get; private set; }

        private DateTime _startDT;
        private DateTime _stopDT;

        public DurationTimer()
        {
            isActive = false;
            _startDT = DateTime.MinValue;
            _stopDT = DateTime.MinValue;
        }
        public void Start()
        {
            if (isActive) return;
            _startDT = DateTime.Now;
            isActive = true;
        }
        public void Stop()
        {
            if (!isActive) return;
            _stopDT = DateTime.Now;
            isActive = false;
        }
        public void Initialize()
        {
            isActive = false;
            _startDT = DateTime.MinValue;
            _stopDT = DateTime.MinValue;
        }
        public TimeSpan GetDuration() { return _stopDT - _startDT; }
        public TimeSpan Tag() { return DateTime.Now - _startDT; }

        
    }
}