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
        public const byte RET_PREFORM_GIVEN_NOT_IN_POOL = 2;
        public const byte RET_INDEX_OBJECT_IS_NULL = 3;
        #endregion

        /// <summary>
        /// The most preforms in pool
        /// </summary>
        public int count { get; }

        /// <summary>
        /// Get the count of null or unoccupied preforms
        /// </summary>
        public int remain
        {
            get
            {
                int r = 0;
                foreach (T pf in _preforms)
                {
                    if (pf != null) { if (pf.isOccupied) continue; }
                    r++;
                }
                return r;
            }
        }

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
        /// <param name="preform"></param>
        /// <param name="ret"></param>
        public void Release(T preform, out Ret ret)
        {
            int index = GetPreformIndex(preform);
            if (index < 0)
            {
                ret = new Ret(LogLevel.Error, RET_PREFORM_GIVEN_NOT_IN_POOL, "The preform given is not in pool");
                return;
            }
            if (!_preforms[index].isOccupied) {
                ret = new Ret(LogLevel.Info, 0, "Already released");
                return;
            }
            _preforms[index].OnRelease();
            _preforms[index].isOccupied = false;
            ret = Ret.ok;
        }

        /// <summary>
        /// Release a preform by index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ret"></param>
        public void Release(int index, out Ret ret)
        {
            T preform = GetPreform(index, out ret);
            if (ret.code != 0) { return; }
            if (preform == null) {
                ret = new Ret(LogLevel.Error, RET_INDEX_OBJECT_IS_NULL, "Preform in index:" + index.ToString() + " is null");
                return;
            }
            if (!_preforms[index].isOccupied)
            {
                ret = new Ret(LogLevel.Info, 0, "Already released");
                return;
            }
            _preforms[index].OnRelease();
            _preforms[index].isOccupied = false;
            ret = Ret.ok;
        }

        /// <summary>
        /// Try to get the index of preform given
        /// </summary>
        /// <param name="preform"></param>
        /// <returns></returns>
        public int GetPreformIndex(T preform)
        {
            for (int i = 0; i < count; i++) { if (_preforms[i] == preform) return i; }
            return -1;
        }

        /// <summary>
        /// Get the preform by index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public T GetPreform(int index, out Ret ret)
        {
            if (index < 0 || index > count - 1)
            {
                ret = new Ret(LogLevel.Error, 1, "Index:" + index + " is out of range");
                return null;
            }
            T preform = _preforms[index];
            ret = Ret.ok;
            return preform;
        }

        /// <summary>
        /// TEST
        /// </summary>
        /// <returns></returns>
        public T[] GetAll()
        {
            return _preforms;
        }
    }
}
