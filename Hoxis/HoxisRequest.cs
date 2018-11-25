using DacLib.Generic;
using DacLib.Codex;
using System.Threading;

namespace DacLib.Hoxis
{
    public class HoxisRequest
    {
        public string handle { get; }
        public int timeout { get; }
        public bool isWaiting { get; private set; }

        private HoxisProtocol _temp;
        private int _timer;

        public HoxisRequest(string handleArg, int timeoutArg, HoxisProtocol tempArg)
        {
            handle = handleArg;
            timeout = timeoutArg;
            isWaiting = false;
            _temp = tempArg;
            _timer = 0;
        }

        public void WaitForResp()
        {
            isWaiting = true;
            Thread t = new Thread(() =>
            {
                while (isWaiting)
                {
                    Thread.Sleep(100);
                    _timer += 100;
                    if (_timer >= timeout) { break; }
                }
            });
        }

        public void Break() { isWaiting = false; }
    }
}

