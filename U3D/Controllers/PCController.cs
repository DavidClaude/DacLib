using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Generic;

namespace DacLib.U3D.Controllers
{
    public enum ExecuteMode
    {
        None = 0,
        Hold,
        Toggle
    }

    public class PCControllerSetting
    {
        public Dictionary<string, CommandItem> commandTable { get; private set; }

        public PCControllerSetting()
        {
            commandTable = new Dictionary<string, CommandItem>();
        }

        public bool GetCommandOn(string cmd) { return Input.GetKeyDown(commandTable[cmd].keyCode); }
        public bool GetCommand(string cmd) { return Input.GetKey(commandTable[cmd].keyCode); }
        public bool GetCommandOff(string cmd) { return Input.GetKeyUp(commandTable[cmd].keyCode); }

        /// <summary>
        /// Get the command name by give key code
        /// Get if the given key is occupied
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string GetCommandName(KeyCode code)
        {
            foreach (string cn in commandTable.Keys) { if (commandTable[cn].keyCode == code) return cn; }
            return string.Empty;
        }

        public void AddItem(string name, CommandItem item)
        {
            if (commandTable.ContainsKey(name)) return;
            commandTable.Add(name, item);
        }

        public void SetItem(string name, CommandItem item)
        {
            if (!commandTable.ContainsKey(name)) return;
            commandTable[name] = item;
        }

        public void SetItem(string name, KeyCode code, ExecuteMode mode = ExecuteMode.Hold, string category = "", Dictionary<string,string> descs = null)
        {
            CommandItem item = new CommandItem() { keyCode = code, category = category, funcDescs = descs };
            SetItem(name, item);
        }

        public void SetItem(string name, KeyCode code, ExecuteMode mode = ExecuteMode.Hold, string category = "", params KVString[] descs)
        {
            Dictionary<string, string> descsDict = new Dictionary<string, string>();
            foreach (KVString kv in descs) { descsDict.Add(kv.key, kv.val); }
            SetItem(name, code, mode, category, descsDict);
        }

        public void ClearItem(string name) { SetItem(name, KeyCode.None); }

        public struct CommandItem
        {
            public static readonly CommandItem undef = new CommandItem { keyCode = KeyCode.None, executeMode = ExecuteMode.None, category = string.Empty, funcDescs = null};

            public KeyCode keyCode;
            public ExecuteMode executeMode;
            public string category;
            public Dictionary<string, string> funcDescs;
        }

    }


}
