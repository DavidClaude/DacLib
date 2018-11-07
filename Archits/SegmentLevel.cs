using System;

namespace DacLib.Archits
{
	public class SegmentLevel
	{
		/// <summary>
		/// 级别改变事件
		/// </summary>
		public event DacLib.Utils.IntForVoid_Handler onLevelChange {
			add {
				_onLevelChange += value;
			}
			remove {
				_onLevelChange -= value;
			}
		}
		private event DacLib.Utils.IntForVoid_Handler _onLevelChange;


		/// <summary>
		/// 程度值
		/// </summary>
		/// <value>The degree.</value>
		public float degree { get; private set; }

		/// <summary>
		/// 级别
		/// </summary>
		/// <value>The level.</value>
		public int level {
			get {
				for (int i = 0; i < _thresholds.Length; i++) {
					if (degree <= _thresholds [i])
						return i;
				}
				return _thresholds.Length - 1;
			}
		}

		private float[] _thresholds;
		private int _lastLevel;

		public SegmentLevel (int count)
		{
			degree = 0;
			_thresholds = new float[count];
			_lastLevel = level;
		}

		/// <summary>
		/// 设置阈值
		/// </summary>
		/// <param name="level">Level.</param>
		/// <param name="val">Value.</param>
		public void SetThreshold (int level, float val)
		{
			if (level < 0 || level >= _thresholds.Length)
				return;
			_thresholds [level] = val;
		}

		/// <summary>
		/// 加深程度
		/// </summary>
		/// <param name="val">Value.</param>
		public void Extend (float val)
		{
			degree += val;
			float max = _thresholds [_thresholds.Length - 1];
			if (degree > max) {
				degree = max;
			}
			if (level != _lastLevel) {
				OnLevelChange (level);
				_lastLevel = level;
			}
		}

		/// <summary>
		/// 还原
		/// </summary>
		public void Init ()
		{
			degree = 0;
			if (level != _lastLevel) {
				OnLevelChange (level);
				_lastLevel = level;
			}
		}

		private void OnLevelChange (int level){
			if (_onLevelChange == null)
				return;
			_onLevelChange (level);
		}
	}
}