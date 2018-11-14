using System;
using DacLib.Generic;

namespace DacLib.Codex
{
    public class SegmentLevel
    {
        public const int RET_LEVEL_OUT_OF_RANGE = 1;
        public const int RET_COUNT_IS_0 = 2;
        public const int RET_THRESHOLD_IS_LESS_EQUAL_0 = 3;
        public const int RET_THRESHOLD_IS_LESS_EQUAL_PRE = 4;

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
        /// 进度(整体)
        /// </summary>
        public float rate
        {
            get
            {
                return degree / max;
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

        /// <summary>
        /// 最大阈值
        /// </summary>
        public float max
        {
            get
            {
                return _thresholds[count - 1];
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
                ret = new Ret(RetLevel.Error, RET_LEVEL_OUT_OF_RANGE, "Level:" + level + " is out of range");
                return;
            }
            //if (val <= 0)
            //{
            //    ret = new Ret(WARNING_THRESHOLD_IS_LESS_EQUAL_THAN_0, "Threshold:" + val + " is less or equal than 0");
            //    return;
            //}
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
            if (count == 0) {
                ret = new Ret (RetLevel.Error, RET_COUNT_IS_0, "Thresholds count is 0");
                return;
            }
            for (int i = 0; i < count; i++) {
                if (_thresholds[i] <= 0) {
                    ret = new Ret(RetLevel.Warning, RET_THRESHOLD_IS_LESS_EQUAL_0, "Level:" + i + " threshold is less than or equal to 0");
                    return;
                }
                if ( i != 0) {
                    if (_thresholds[i] <= _thresholds[i - 1]) {
                        ret = new Ret(RetLevel.Warning, RET_THRESHOLD_IS_LESS_EQUAL_PRE, "Level:" + i + " threshold is less than or equal to the previous one");
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
            if (degree > max)
            {
                degree = max;
            }
            if (degree < 0)
            {
                degree = 0;
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