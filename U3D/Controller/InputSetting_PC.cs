using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Generic;

namespace DacLib.U3D.Controller
{
    public class WindowsInputKit
    {
        private Dictionary<string, InputLayer> _layers;

        public WindowsInputKit()
        {

        }

        public void AddLayer(string name)
        {

        }

        public void RemoveLayer(string name)
        {

        }
    }

    public class InputLayer
    {
        public const int ERROR_CMD_EXITS = 1;
        public const int ERROR_NO_CMD = 2;
        public const int INFO_AVAILABLE_CMD = 3;

        private Dictionary<string, KeyCode> _commands;

        public InputLayer(string json = "")
        {
            if (json == "")
            {
                _commands = new Dictionary<string, KeyCode>();
            }
            else
            {
                _commands = ToCmds(json);
            }
        }

        /// <summary>
        /// 添加命令项
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="code"></param>
        /// <param name="ret"></param>
        public void Add(string cmd, KeyCode code, out Ret ret)
        {
            if (ContainCmd(cmd))
            {
                ret = new Ret(ERROR_CMD_EXITS, "Cmd:" + cmd + " already exits");
                return;
            }
            _commands.Add(cmd, code);
            ret = Ret.ok;
        }

        /// <summary>
        /// 设置命令项(直接覆盖)
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="code"></param>
        /// <param name="ret"></param>
        public void Set(string cmd, KeyCode code, out Ret ret)
        {
            if (ContainCmd(cmd))
            {
                _commands[cmd] = code;
                ret = Ret.ok;
            }
            ret = new Ret(ERROR_NO_CMD, "Cmd:" + cmd + " doesn't exit");
        }

        /// <summary>
        /// 清除命令项
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ret"></param>
        public void Clear(string cmd, out Ret ret)
        {
            if (ContainCmd(cmd))
            {
                _commands[cmd] = KeyCode.None;
                ret = Ret.ok;
            }
            ret = new Ret(ERROR_NO_CMD, "Cmd:" + cmd + " doesn't exit");
        }

        /// <summary>
        /// 监听命令开始
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public bool GetCmdStart(string cmd)
        {
            if (ContainCmd(cmd))
            {
                return Input.GetKeyDown(_commands[cmd]);
            }
            return false;
        }

        /// <summary>
        /// 监听命令持续
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public bool GetCmding(string cmd)
        {
            if (ContainCmd(cmd))
            {
                return Input.GetKey(_commands[cmd]);
            }
            return false;
        }

        /// <summary>
        /// 监听命令结束
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public bool GetCmdEnd(string cmd)
        {
            if (ContainCmd(cmd))
            {
                return Input.GetKeyUp(_commands[cmd]);
            }
            return false;
        }

        /// <summary>
        /// 是否包含命令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public bool ContainCmd(string cmd)
        {
            return _commands.ContainsKey(cmd);
        }

        /// <summary>
        /// 是否包含键位
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool ContainCode(KeyCode code)
        {
            return _commands.ContainsValue(code);
        }

        /// <summary>
        /// 键位转命令
        /// 若键位未被使用，返回空
        /// </summary>
        /// <param name="code"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public string CodeToCmd(KeyCode code, out Ret ret)
        {
            foreach (string cmd in _commands.Keys)
            {
                if (_commands[cmd] == code)
                {
                    ret = Ret.ok;
                    return cmd;
                }
            }
            ret = new Ret(INFO_AVAILABLE_CMD, "Key:" + code + " is available");
            return "";
        }

        public string ToJson()
        {
            return FormatFunc.ObjectToJson(_commands);
        }

        public Dictionary<string, KeyCode> ToCmds(string json)
        {
            return FormatFunc.JsonToObject<Dictionary<string, KeyCode>>(json);
        }
    }
}