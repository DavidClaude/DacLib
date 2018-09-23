using System;

namespace DacLib.U3D.Common
{
	public delegate void TimeUpHandler ();

	public class GameTimer
	{
		/// <summary>
		/// GameTimer管理器
		/// </summary>
		public static GameTimer_Manager gt_manager;

		/// <summary>
		/// 快捷使用Timer
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="duration">Duration.</param>
		/// <param name="handler">Handler.</param>
		public static void New (string name, float duration, TimeUpHandler handler)
		{
			if (gt_manager == null) {
				UnityEngine.GameObject go = new UnityEngine.GameObject ("GameTimer Manager");
				gt_manager = go.AddComponent<GameTimer_Manager> ();
			}
			gt_manager.AddTimer (name, duration, handler);
		}

		/// <summary>
		/// 手动卸载管理器
		/// 当决定不再使用时手动调用，优化内存
		/// </summary>
		public static void Uninstall ()
		{
			UnityEngine.MonoBehaviour.DestroyImmediate (gt_manager.gameObject);
			gt_manager = null;
		}
	}

	public class Timer 
	{
		public string name;
		public float duration;
		public float remain;
		public TimeUpHandler handler;
		public Timer (string nameArg, float durationArg, TimeUpHandler handlerArg)
		{
			name = nameArg;
			duration = durationArg;
			handler = handlerArg;
			remain = duration;
		}
	}
}