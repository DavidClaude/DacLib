using System;
using System.Collections.Generic;

namespace DacLib.Hoxis
{
    public delegate void HoxisActionHandler(HoxisProtocolAction action);

    /// <summary>
    /// Enable callbacks with HoxisProtocolAction in threads to enter the main procedure
    /// To operate Unity objects
    /// </summary>
    public class HoxisActionQueue
    {
        /// <summary>
        /// Max quantity of actions in queue
        /// </summary>
        public int capacity { get; }
        
        /// <summary>
        /// The quantity of actions processed in one round
        /// </summary>
        public short processingQuantity { get; }

        private Queue<HoxisProtocolAction> _queue;
        private HoxisActionHandler _handler;
        public HoxisActionQueue(int capacityArg, short processingQuantityArg, HoxisActionHandler handlerArg)
        {
            capacity = capacityArg;
            processingQuantity = processingQuantityArg;
            _queue = new Queue<HoxisProtocolAction>(capacity);
            _handler = new HoxisActionHandler(handlerArg);
        }

        /// <summary>
        /// Enqueue an action
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(HoxisProtocolAction action)
        {
            lock (_queue) { _queue.Enqueue(action); }
        }

        /// <summary>
        /// Generally called per frame
        /// </summary>
        /// <param name="handle"></param>
        public void ProcessInRound()
        {
            short q = 0;
            while (q < processingQuantity)
            {
                if (_queue.Count <= 0) break;
                HoxisProtocolAction action = _queue.Dequeue();
                _handler(action);
                q++;
            }
        }
    }
}

