using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DacLib.U3D.Common
{
	/// <summary>
	/// GameTimer管理器
	/// 主要借助其Update能力更新计时器
	/// </summary>
	public class GameTimer_Manager : MonoBehaviour
	{
		private List<Timer> _timers = new List<Timer> ();
		private List<Timer> _rels = new List<Timer> ();

		void Update ()
		{
			foreach (Timer t in _timers) {
				t.remain -= Time.deltaTime;
				if (t.remain <= 0) {
					t.handler ();
					_rels.Add (t);
				}
			}
			foreach (Timer t in _rels) {
				if (_timers.Contains (t))
					_timers.Remove (t);
			}
			_rels.Clear ();
		}

		public void AddTimer (string name, float duration, TimeUpHandler handler)
		{
			_timers.Add (new Timer (name, duration, handler));
		}

		public void AddTimer (Timer timer)
		{
			_timers.Add (timer);
		}
	}
}