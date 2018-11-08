using System;
using DacLib.Generic;

namespace DacLib.Codex
{
    public class SegmentLevel
    {
        public const int ERROR_LEVLE_OUT_OF_RANGE = 1;
        public const int ERROR_THRESHOLD_IS_LESS_EQUAL_THAN_ZERO = 2;
        public const int ERROR_THRESHOLD_IS_LESS_THAN_PRE = 3;
        public const int ERROR_THRESHOLD_IS_MORE_THAN_NEXT = 4;

        /// <summary>
        /// 级别改变事件
        /// </summary>
        public event IntForVoid_Handler onLevelChange;

        /// <summary>
        /// 程度值
        /// </summary>
        /// <value>The degree.</value>
        public float degree { get; private set; }

        /// <summary>
        /// 级别数
        /// </summary>
        public int count
        {
            get
            {
                return _thresholds.Length;
            }
        }

        /// <summary>
        /// 级别
        /// </summary>
        /// <value>The level.</value>
        public int level
        {
            get
            {
                for (int i = 0; i < count; i++)
                {
                    if (degree <= _thresholds[i])
                        return i;
                }
                return count - 1;
            }
        }

        /// <summary>
        /// 进度(当前级别中)
        /// </summary>
        public float localRate
        {
            get
            {
                float start = 0;
                if (level > 0)
                    start = _thresholds[level - 1];
                return (degree - start) / (_thresholds[level] - start);
            }
        }

        private float[] _thresholds;
        private int _lastLevel;

        public SegmentLevel(int countArg)
        {
            degree = 0;
            _thresholds = new float[countArg];
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
                ret = new Ret(ERROR_LEVLE_OUT_OF_RANGE, "Level:" + level + " is out of range");
                return;
            }
            if (val <= 0)
            {
                ret = new Ret(ERROR_THRESHOLD_IS_LESS_EQUAL_THAN_ZERO, "Threshold:" + val + " is less or equal than 0");
                return;
            }
            _thresholds[level] = val;
            ret = Ret.ok;
        }

        /// <summary>
        /// 加深程度
        /// </summary>
        /// <param name="val">Value.</param>
        public void Extend(float val)
        {
            if (val == 0)
                return;
            degree += val;
            float max = _thresholds[count-1];
            if (degree > max)
            {
                degree = max;
            }
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