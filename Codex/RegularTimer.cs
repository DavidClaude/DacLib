using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DacLib.Generic;

namespace DacLib.Codex
{
    public class RegularTimer
    {
        public float timeout { get; }
        public NoneForVoid_Handler trigger { get; }
        public bool enable { get; private set; }
        public float time { get; private set; }

        public RegularTimer(float timeoutArg, NoneForVoid_Handler triggerArg)
        {
            if (timeoutArg <= 0) { timeout = 2f; }
            else { timeout = timeoutArg; }
            trigger = triggerArg;
            enable = false;
            time = 0f;
        }

        public void Start() { if (enable) return; enable = true; }
        public void Update(float delta)
        {
            if (!enable) return;
            time += delta;
            if (time > timeout) { trigger(); time = 0f; }
        }
        public void Stop() { if (enable) enable = false; }
    }
}
