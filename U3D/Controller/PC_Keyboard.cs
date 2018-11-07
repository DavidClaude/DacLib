using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Generic;

namespace DacLib.U3D.Controller
{
	/// <summary>
	/// 键位设置
	/// 命令模式
	/// </summary>
	public class PC_Keyboard
	{
		private Dictionary<string, KeyCode> _commands;

		public PC_Keyboard ()
		{
			_commands = new Dictionary<string, KeyCode> ();
		}

		/// <summary>
		/// 通过已有Json字符串实例化键位设置
		/// </summary>
		/// <param name="json">Json.</param>
		public PC_Keyboard (string json)
		{
			Load (json);
		}

		/// <summary>
		/// 添加命令项
		/// </summary>
		/// <param name="cmd">Cmd.</param>
		/// <param name="code">Code.</param>
		public void Add (string cmd, KeyCode code)
		{
			if (Contain (cmd))
				return;
			_commands.Add (cmd, code);
		}

		/// <summary>
		/// 是否包含命令项
		/// </summary>
		/// <param name="cmd">Cmd.</param>
		public bool Contain (string cmd)
		{
			return _commands.ContainsKey (cmd);
		}

		/// <summary>
		/// 设置命令项
		/// </summary>
		/// <param name="cmd">Cmd.</param>
		/// <param name="code">Code.</param>
		public void Set (string cmd, KeyCode code)
		{
			if (Contain (cmd)) {
				_commands [cmd] = code;
			}
		}

		/// <summary>
		/// 清空命令项
		/// </summary>
		/// <param name="cmd">Cmd.</param>
		public void Clear (string cmd)
		{
			if (Contain (cmd)) {
				_commands [cmd] = KeyCode.None;
			}
		}

		/// <summary>
		/// 通过按键获取命令名称
		/// 若按键可用，返回""
		/// </summary>
		/// <returns>The to cmd.</returns>
		/// <param name="code">Code.</param>
		public string CodeToCmd (KeyCode code)
		{
			foreach (string cmd in _commands.Keys) {
				if (_commands [cmd] == code) {
					return cmd;
				}
			}
			return "";
		}

		/// <summary>
		/// 检测命令开始
		/// </summary>
		/// <returns><c>true</c>, if cmd start was gotten, <c>false</c> otherwise.</returns>
		/// <param name="cmd">Cmd.</param>
		public bool GetCmdStart (string cmd)
		{
			if (Contain (cmd)) {
				return Input.GetKeyDown (_commands [cmd]);
			}
			return false;
		}

		/// <summary>
		/// 检测命令持续
		/// </summary>
		/// <returns><c>true</c>, if cmd was gotten, <c>false</c> otherwise.</returns>
		/// <param name="cmd">Cmd.</param>
		public bool GetCmd (string cmd)
		{
			if (Contain (cmd)) {
				return Input.GetKey (_commands [cmd]);
			}
			return false;
		}

		/// <summary>
		/// 检测命令结束
		/// </summary>
		/// <returns><c>true</c>, if cmd end was gotten, <c>false</c> otherwise.</returns>
		/// <param name="cmd">Cmd.</param>
		public bool GetCmdEnd (string cmd)
		{
			if (Contain (cmd)) {
				return Input.GetKeyUp (_commands [cmd]);
			}
			return false;
		}

		/// <summary>
		/// 将键位设置转为Json字符串
		/// </summary>
		/// <returns>The json.</returns>
		public string ToJson ()
		{
			return FormatFunc.ObjectToJson (_commands);
		}

		/// <summary>
		/// 读取Json字符串并映射到键位设置
		/// </summary>
		/// <param name="json">Json.</param>
		public void Load (string json)
		{
			_commands = FormatFunc.JsonToObject<Dictionary<string, KeyCode>> (json);
		}
	}
}