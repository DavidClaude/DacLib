using System.Collections.Generic;
using DacLib.Generic;
using System.Threading;

namespace DacLib.Codex
{
    public class CriticalReception<T> where T : class, IReceivable
    {
        public class Desk<T> where T : class, IReceivable
        {
            /// <summary>
            /// The object served
            /// </summary>
            public T guest { get; private set; }

            /// <summary>
            /// Is this desk occupied ?
            /// </summary>
            public bool isOccupied { get; private set; }

            /// <summary>
            /// The cycle(ms) of service
            /// </summary>
            public int cycle { get; }

            public Desk(int cycleArg) {
                isOccupied = false;
                cycle = cycleArg;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="guestArg"></param>
            public void StartService(T guestArg)
            {
                isOccupied = true;
                guest = guestArg;
                guest.OnServiceStart();
                Thread thread = new Thread(() =>
                {
                    while (guest != null)
                    {
                        guest.OnService();
                        Thread.Sleep(cycle);
                    }
                });
            }
            public void StopService()
            {
                guest.OnServiceStop();
                guest = null;
                isOccupied = false;
            }
        }

        #region ret codes
        public const byte RET_NO_UNOCCUPIED_DESK = 1;
        #endregion

        public int count { get; }
        public int cycle { get; }

        private Desk<T>[] _desks;

        public CriticalReception(int countArg, int cycleArg = 1000)
        {
            count = countArg;
            if (cycleArg <= 0) { cycle = 1000; }
            cycle = cycleArg;
            _desks = new Desk<T>[count];
        }

        public void Request(T guest, out Ret ret)
        {
            Ret retGetDesk;
            int index = GetUnoccupiedDesk(out retGetDesk);
            if (retGetDesk.code != 0)
            {
                ret = retGetDesk;
                return;
            }
            if (_desks[index] == null)
                _desks[index] = new Desk<T>(cycle);
            _desks[index].StartService(guest);
            ret = Ret.ok;
        }

        public void Release()
        {

        }

        private int GetUnoccupiedDesk(out Ret ret)
        {
            for (int i = 0; i < count; i++)
            {
                if (!_desks[i].isOccupied)
                {
                    ret = Ret.ok;
                    return i;
                }
            }
            ret = new Ret(LogLevel.Info, RET_NO_UNOCCUPIED_DESK, "No unoccupied desk");
            return -1;
        }

    }
}

