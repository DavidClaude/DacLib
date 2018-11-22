﻿using System.Collections.Generic;
using DacLib.Generic;
using System.Threading;

namespace DacLib.Codex
{
    public class CriticalReception<T> where T : class, IReceivable
    {
        #region ret codes
        public const byte RET_NO_UNOCCUPIED_DESK = 1;
        public const byte RET_GUEST_NOT_ON_SERVICE = 2;
        #endregion

        /// <summary>
        /// The most guests served at the same moment
        /// </summary>
        public int count { get; }

        /// <summary>
        /// The cycle(ms) of service
        /// </summary>
        public int cycle { get; }

        /// <summary>
        /// Get the remain count of unoccupied desks
        /// </summary>
        public int remain
        {
            get
            {
                int r = 0;
                for (int i = 0; i < count; i++)
                {
                    if (_desks[i].isOccupied) { r++; }
                }
                return r;
            }
        }

        private Desk<T>[] _desks;

        public CriticalReception(int countArg, int cycleArg = 1000)
        {
            count = countArg;
            if (cycleArg <= 0) { cycle = 1000; }
            cycle = cycleArg;
            _desks = new Desk<T>[count];
            for (int i = 0; i < count; i++) { _desks[i] = new Desk<T>(cycle); }
        }

        /// <summary>
        /// Request a reception desk
        /// </summary>
        /// <param name="guest"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public int Request(T guest, out Ret ret)
        {
            int index = GetUnoccupiedDesk(out ret);
            if (ret.code != 0)
                return -1;
            if (_desks[index] == null)
                _desks[index] = new Desk<T>(cycle);
            _desks[index].StartService(guest);
            return index;
        }

        /// <summary>
        /// Release given reception desk
        /// </summary>
        /// <param name="guest"></param>
        /// <param name="ret"></param>
        public void Release(T guest, out Ret ret)
        {
            foreach (Desk<T> d in _desks)
            {
                if (d.guest == guest)
                {
                    d.StopService();
                    ret = Ret.ok;
                    return;
                }
            }
            ret = new Ret(LogLevel.Warning, RET_GUEST_NOT_ON_SERVICE, "The given guest is not on service");
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

        public class Desk<TD> where TD : class, IReceivable
        {
            /// <summary>
            /// The object served
            /// </summary>
            public TD guest { get; private set; }

            /// <summary>
            /// Is this desk occupied ?
            /// </summary>
            public bool isOccupied { get; private set; }

            /// <summary>
            /// The cycle(ms) of service
            /// </summary>
            public int cycle { get; }

            private Thread _serviceThread;

            public Desk(int cycleArg)
            {
                isOccupied = false;
                cycle = cycleArg;
                _serviceThread = new Thread(() =>
                {
                    while (true)
                    {
                        lock (guest) { guest.OnService(); }
                        Thread.Sleep(cycle);
                    }
                });
            }

            /// <summary>
            /// Start the service with a guest
            /// </summary>
            /// <param name="guestArg"></param>
            public void StartService(TD guestArg)
            {
                if (isOccupied)
                    return;
                isOccupied = true;
                guest = guestArg;
                guest.OnServiceStart();
                _serviceThread.Start();
            }

            /// <summary>
            /// Stop the active service
            /// </summary>
            public void StopService()
            {
                if (_serviceThread.IsAlive) { _serviceThread.Abort(); }
                guest.OnServiceStop();
                guest = null;
                isOccupied = false;
            }
        }
    }
}

