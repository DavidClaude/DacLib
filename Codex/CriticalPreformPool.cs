﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DacLib.Generic;

namespace DacLib.Codex
{
    /// <summary>
    /// Critical object pool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class CriticalPreformPool<T> where T : class, ICritical, new()
    {
        #region ret codes
        public const byte RET_NO_UNOCCUPIED_PREFORM = 1;
        public const byte RET_PREFORM_GIVEN_NOT_IN_POOL = 2;
        public const byte RET_OBJECT_IS_NULL = 3;
        #endregion

        /// <summary>
        /// The most preforms in pool
        /// </summary>
        public int maxCount { get; }

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
                    if (pf == null) continue;
                    if (pf.isOccupied) continue;
                    r++;
                }
                return r;
            }
        }

        private T[] _preforms;

        public CriticalPreformPool(int maxCountArg)
        {
            maxCount = maxCountArg;
            _preforms = new T[maxCount];
        }

        /// <summary>
        /// Request for a preform
        /// Find an unoccupied preform first
        /// </summary>
        /// <param name="state"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public T Request(object state, out Ret ret)
        {
            for (int i = 0; i < maxCount; i++)
            {
                // If null, init and use it
                if (_preforms[i] == null)
                {
                    _preforms[i] = new T();
                    _preforms[i].isOccupied = true;
                    _preforms[i].localID = i;
                    _preforms[i].OnRequest(state);
                    ret = Ret.ok;
                    return _preforms[i];
                }
                // If not occupied, use it
                if (_preforms[i].isOccupied == false)
                {
                    _preforms[i].isOccupied = true;
                    _preforms[i].localID = i;
                    _preforms[i].OnRequest(state);
                    ret = Ret.ok;
                    return _preforms[i];
                }
            }
            ret = new Ret(LogLevel.Warning, RET_NO_UNOCCUPIED_PREFORM, "no unoccupied preform");
            return null;
        }

        /// <summary>
        /// Release a preform by object
        /// </summary>
        /// <param name="preform"></param>
        /// <param name="ret"></param>
        public void Release(T preform, out Ret ret)
        {
            if (preform == null)
            {
                ret = new Ret(LogLevel.Error, RET_OBJECT_IS_NULL, "preform is null");
                return;
            }
            if (!preform.isOccupied)
            {
                ret = new Ret(LogLevel.Info, 0, "already released");
                return;
            }
            preform.OnRelease();
            preform.localID = -1;
            preform.isOccupied = false;
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
            Release(preform, out ret);
        }

        /// <summary>
        /// Get the preform by index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public T GetPreform(int index, out Ret ret)
        {
            if (index < 0 || index > maxCount - 1)
            {
                ret = new Ret(LogLevel.Error, 1, "index:" + index + " is out of range");
                return null;
            }
            T preform = _preforms[index];
            ret = Ret.ok;
            return preform;
        }

        /// <summary>
        /// Get all occupied preforms
        /// </summary>
        /// <returns></returns>
        public List<T> GetWorkers()
        {
            List<T> preforms = new List<T>();
            foreach (T p in _preforms)
            {
                if (p == null) continue;
                if (!p.isOccupied) continue;
                preforms.Add(p);
            }
            return preforms;
        }
    }
}
