using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DacLib.Generic;

namespace DacLib.Codex
{
    class CriticalPreformPool<T> where T : class, IReusable, new()
    {
        #region ret codes
        public const byte RET_NO_UNOCCUPIED_PREFORM = 1;
        #endregion

        public int count { get; }

        private T[] _preforms;
        public CriticalPreformPool(int countArg)
        {
            count = countArg;
            _preforms = new T[count];
        }

        /// <summary>
        /// Request for a preform
        /// Find an unoccupied preform first
        /// </summary>
        /// <param name="state"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public int Request(object state, out Ret ret)
        {
            for (int i = 0; i < count; i++)
            {
                // If null, init and use it
                if (_preforms[i] == null)
                {
                    _preforms[i] = new T();
                    _preforms[i].isOccupied = true;
                    _preforms[i].OnRequest(state);
                    ret = new Ret(LogLevel.Info, 0, "new object");
                    return i;
                }
                // If not occupied, use it
                if (_preforms[i].isOccupied == false)
                {
                    _preforms[i].isOccupied = true;
                    _preforms[i].OnRequest(state);
                    ret = Ret.ok;
                    return i;
                }
            }
            ret = new Ret(LogLevel.Warning, RET_NO_UNOCCUPIED_PREFORM, "No unoccupied preform");
            return -1;
        }

        /// <summary>
        /// Release a preform by object
        /// </summary>
        /// <param name="perform"></param>
        /// <param name="ret"></param>
        public void Release(T perform, out Ret ret)
        {
            int index = GetPerformIndex(perform);
            if (index < 0)
            {
                ret = new Ret(LogLevel.Error, 1, "The perform given is not in pool");
                return;
            }
            _preforms[index].OnRelease();
            _preforms[index].isOccupied = false;
            ret = Ret.ok;
        }

        public int GetPerformIndex(T perform)
        {
            for (int i = 0; i < count; i++) { if (_preforms[i] == perform) return i; }
            return -1;
        }
    }
}
