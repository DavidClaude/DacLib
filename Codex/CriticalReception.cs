//using System.Collections.Generic;
//using DacLib.Generic;
//using System.Threading;

//namespace DacLib.Codex
//{
//    public class CriticalReception<T> where T : class, INodal
//    {
//        #region ret codes
//        public const byte RET_NO_UNOCCUPIED_DESK = 1;
//        public const byte RET_GUEST_NOT_ON_SERVICE = 2;
//        #endregion

//        /// <summary>
//        /// The most guests served at the same moment
//        /// </summary>
//        public int count { get; }

//        /// <summary>
//        /// The cycle(ms) of service
//        /// </summary>
//        public int cycle { get; }

//        /// <summary>
//        /// Get the count of unoccupied desks
//        /// </summary>
//        public int remain
//        {
//            get
//            {
//                int r = 0;
//                foreach (Desk<T> d in _desks) { if (!d.isOccupied) { r++; } }
//                return r;
//            }
//        }

//        private Desk<T>[] _desks;

//        public CriticalReception(int countArg, int cycleArg = 1000)
//        {
//            count = countArg;
//            if (cycleArg <= 0) { cycle = 1000; }
//            cycle = cycleArg;
//            _desks = new Desk<T>[count];
//            for (int i = 0; i < count; i++) { _desks[i] = new Desk<T>(cycle); }
//        }

//        /// <summary>
//        /// Request a reception desk
//        /// </summary>
//        /// <param name="guest"></param>
//        /// <param name="ret"></param>
//        /// <returns></returns>
//        public int Request(T guest, out Ret ret)
//        {
//            int index = GetUnoccupiedDeskIndex(out ret);
//            if (ret.code != 0)
//                return -1;
//            _desks[index].Accept(guest);
//            return index;
//        }

//        /// <summary>
//        /// Release given reception desk
//        /// </summary>
//        /// <param name="guest"></param>
//        /// <param name="ret"></param>
//        public void Release(T guest, out Ret ret)
//        {
//            foreach (Desk<T> d in _desks)
//            {
//                if (d.guest == guest)
//                {
//                    d.Decline();
//                    ret = Ret.ok;
//                    return;
//                }
//            }
//            ret = new Ret(LogLevel.Warning, RET_GUEST_NOT_ON_SERVICE, "The guest given is not on service");
//        }

//        public void Release(int index, out Ret ret)
//        {
//            if (index < 0 || index > count - 1) {
//                ret = new Ret(LogLevel.Error, 1, "Index:" + index + " is out of range");
//                return;
//            }
//            _desks[index].Decline();
//            ret = Ret.ok;
//        }

//        private int GetUnoccupiedDeskIndex(out Ret ret)
//        {
//            for (int i = 0; i < count; i++)
//            {
//                if (!_desks[i].isOccupied)
//                {
//                    ret = Ret.ok;
//                    return i;
//                }
//            }
//            ret = new Ret(LogLevel.Info, RET_NO_UNOCCUPIED_DESK, "No unoccupied desk");
//            return -1;
//        }

//        public class Desk<TD> where TD : class, IReceivable
//        {
//            /// <summary>
//            /// The object served
//            /// </summary>
//            public TD guest { get; private set; }

//            /// <summary>
//            /// Keep updating ?
//            /// Only used to guests updated
//            /// </summary>
//            public bool rcptOn { get; private set; }

//            /// <summary>
//            /// Is this desk occupied ?
//            /// </summary>
//            public bool isOccupied { get { return guest != null ? true : false; } }

//            /// <summary>
//            /// The cycle(ms) of service
//            /// </summary>
//            public int cycle { get; }

//            public Desk(int cycleArg)
//            {
//                guest = null;
//                rcptOn = false;
//                cycle = cycleArg;
//            }

//            /// <summary>
//            /// Start the service with a guest
//            /// </summary>
//            /// <param name="guestArg"></param>
//            public void Accept(TD guestArg)
//            {
//                if (isOccupied) return;
//                guest = guestArg;
//                guest.OnAccept();
//                if (guest.isUpdated) {
//                    rcptOn = true;
//                    Thread t = new Thread(Serve);
//                    t.Start();
//                }
//            }

//            /// <summary>
//            /// Stop the active service
//            /// </summary>
//            public void Decline()
//            {
//                if (!isOccupied) return;
//                rcptOn = false;
//                guest.OnDecline();
//                guest = null;
//            }

//            private void Serve()
//            {
//                while (true)
//                {
//                    Thread.Sleep(cycle);
//                    if (rcptOn) { lock (guest) { guest.OnService(); } }
//                    else { break; }
//                }
//            }
//        }
//    }
//}

