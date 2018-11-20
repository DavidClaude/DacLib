using System;

namespace DacLib.Generic
{
    /// <summary>
    /// 数学相关的静态方法库
    /// </summary>
    public static class MathFunc
    {

        /// <summary>
        /// 获取不定参数最小值
        /// </summary>
        /// <param name="vals">Vals.</param>
        public static float Min(params float[] vals)
        {
            float m = float.PositiveInfinity;
            int len = vals.Length;
            for (int i = 0; i < len; i++) { if (vals[i] < m) { m = vals[i]; } }
            return m;
        }

        /// <summary>
        /// 获取不定参数最大值
        /// </summary>
        /// <param name="vals">Vals.</param>
        public static float Max(params float[] vals)
        {
            float m = float.NegativeInfinity;
            int len = vals.Length;
            for (int i = 0; i < len; i++) { if (vals[i] > m) { m = vals[i]; } }
            return m;
        }

        /// <summary>
        /// 限制取值范围
        /// </summary>
        /// <param name="val">Value.</param>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Max.</param>
        public static float Clamp(float val, float min, float max)
        {
            float v = val;
            if (val < min) { v = min; }
            else if (val > max) { v = max; }
            return v;
        }

        /// <summary>
        /// 将角度值限制在[0-360)之间
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static float ClampAngle(float val, float min = 0f, float max = 360f)
        {
            if (max - min != 360f)
                return 0f;
            float angle = val;
            if (angle >= max) { angle -= 360f; }
            else if (angle < min) { angle += 360f; }
            else { return angle; }
            return ClampAngle(angle, min, max);
        }
    }
}