using System;

namespace DacLib.Utils
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
		public static float Min (params float[] vals)
		{
			float m = 0;
			int len = vals.Length;
			for (int i = 0; i < len; i++) {
				if (i == 0) {
					m = vals [i];
				}
				if (vals [i] < m) {
					m = vals [i];
				}
			}
			return m;
		}

		/// <summary>
		/// 获取不定参数最大值
		/// </summary>
		/// <param name="vals">Vals.</param>
		public static float Max (params float[] vals)
		{
			float m = 0;
			int len = vals.Length;
			for (int i = 0; i < len; i++) {
				if (i == 0) {
					m = vals [i];
				}
				if (vals [i] > m) {
					m = vals [i];
				}
			}
			return m;
		}

		/// <summary>
		/// 限制取值范围
		/// </summary>
		/// <param name="val">Value.</param>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		public static float Clamp (float val, float min, float max)
		{
			float v = val;
			if (val < min) {
				v = min;
			} else if (val > max) {
				v = max;
			}
			return v;
		}
	}
}