using System;
using DacLib.Generic;

namespace DacLib.Codex
{
    public class SegmentLevel
    {
        #region ret codes
        public const byte RET_LEVEL_OUT_OF_RANGE = 1;
        public const byte RET_COUNT_IS_0 = 2;
        public const byte RET_THRESHOLD_IS_LESS_EQUAL_0 = 3;
        public const byte RET_THRESHOLD_IS_LESS_EQUAL_PRE = 4;
        #endregion

        /// <summary>
        /// Event of level switched
        /// </summary>
        public event IntForVoid_Handler onLevelChange;

        /// <summary>
        /// Value of degree
        /// </summary>
        /// <value>The degree.</value>
        public float degree { get; private set; }

        /// <summary>
        /// Count of levels
        /// </summary>
        public int count { get; }

        /// <summary>
        /// The current level
        /// </summary>
        /// <value>The level.</value>
        public int level
        {
            get
            {
                for (int i = 0; i < count; i++) { if (degree <= _thresholds[i]) return i; }
                return count - 1;
            }
        }

        /// <summary>
        /// Overall progress
        /// </summary>
        public float progress { get { return degree / max; } }

        /// <summary>
        /// Local progress
        /// </summary>
        public float localProgress
        {
            get
            {
                float start = 0;
                if (level > 0) start = _thresholds[level - 1];
                return (degree - start) / (_thresholds[level] - start);
            }
        }

        /// <summary>
        /// Max threshold
        /// </summary>
        public float max { get { return _thresholds[count - 1]; } }

        private float[] _thresholds;
        private int _lastLevel;

        public SegmentLevel(int countArg)
        {
            count = countArg;
            degree = 0;
            _thresholds = new float[count];
            _lastLevel = level;
        }

        /// <summary>
        /// 设置阈值
        /// </summary>
        /// <param name="level">Level.</param>
        /// <param name="val">Value.</param>
        public void SetThreshold(int level, float val, out Ret ret)
        {
            if (level < 0 || level >= count)
            {
                ret = new Ret(LogLevel.Error, RET_LEVEL_OUT_OF_RANGE, "Level:" + level + " is out of range");
                return;
            }
            _thresholds[level] = val;
            ret = Ret.ok;
        }

        /// <summary>
        /// 检查阈值
        /// 阈值数量、是否有0或负值、是否非递增
        /// </summary>
        /// <param name="ret"></param>
        public void CheckThresholds(out Ret ret)
        {
            if (count == 0)
            {
                ret = new Ret(LogLevel.Error, RET_COUNT_IS_0, "Thresholds count is 0");
                return;
            }
            for (int i = 0; i < count; i++)
            {
                if (_thresholds[i] <= 0)
                {
                    ret = new Ret(LogLevel.Warning, RET_THRESHOLD_IS_LESS_EQUAL_0, "Level:" + i + " threshold is less than or equal to 0");
                    return;
                }
                if (i > 0)
                {
                    if (_thresholds[i] <= _thresholds[i - 1])
                    {
                        ret = new Ret(LogLevel.Warning, RET_THRESHOLD_IS_LESS_EQUAL_PRE, "Level:" + i + " threshold is less than or equal to the previous one");
                        return;
                    }
                }
            }
            ret = Ret.ok;
        }

        /// <summary>
        /// 加深程度
        /// </summary>
        /// <param name="val">Value.</param>
        public void Extend(float val)
        {

            degree += val;
            degree = MathFunc.Clamp(degree, 0, max);
            if (level != _lastLevel)
            {
                onLevelChange(level);
                _lastLevel = level;
            }
        }

        /// <summary>
        /// 还原
        /// </summary>
        public void Recover()
        {
            degree = 0;
            if (level != _lastLevel)
            {
                onLevelChange(level);
                _lastLevel = level;
            }
        }
    }
}