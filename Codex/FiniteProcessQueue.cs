using System;
using System.Collections.Generic;
using DacLib.Generic;

namespace DacLib.Codex
{
    /// <summary>
    /// Enable callbacks in threads to enter the main program
    /// Useful for operating Unity objects
    /// </summary>
    public class FiniteProcessQueue<T>
    {
        /// <summary>
        /// Max quantity of affairs in queue
        /// </summary>
        public int capacity { get; }
        
        /// <summary>
        /// The quantity of affairs processed in one round
        /// </summary>
        public short processingQuantity { get; }

        /// <summary>
        /// Event of processing an affair
        /// </summary>
        public event ObjectForVoid_Handler onProcess;

        private Queue<T> _queue;
        //private ActionHandler _handler;
        public FiniteProcessQueue(int capacityArg, short processingQuantityArg)
        {
            capacity = capacityArg;
            processingQuantity = processingQuantityArg;
            _queue = new Queue<T>(capacity);
        }

        /// <summary>
        /// Enqueue an affair
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(T affair)
        {
            lock (_queue) { _queue.Enqueue(affair); }
        }

        /// <summary>
        /// Generally called per frame
        /// </summary>
        /// <param name="handle"></param>
        public void ProcessInRound()
        {
            short i = 0;
            while (i < processingQuantity)
            {
                if (_queue.Count <= 0) break;
                T affair = _queue.Dequeue();
                onProcess(affair);
                i++;
            }
        }
    }
}

