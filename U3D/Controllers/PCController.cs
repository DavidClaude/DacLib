using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Generic;

namespace DacLib.U3D.Controllers
{
    public struct CommandeTable
    {
        public Dictionary<string, KeyCode> commandTable;

        public bool GetCommandOn(string cmd) { return Input.GetKeyDown(commandTable[cmd]); }
        public bool GetCommand(string cmd) { return Input.GetKey(commandTable[cmd]); }
        public bool GetCommandOff(string cmd) { return Input.GetKeyUp(commandTable[cmd]); }

        public bool InUse(KeyCode code)
        {
            foreach (KeyCode kc in commandTable.Values) { if (kc == code) return true; }
            return false;
        }

        public void Set(string cmd, KeyCode code)
        {
            if (!commandTable.ContainsKey(cmd)) return;
            commandTable[cmd] = code;
        }

        public void Clear(string cmd) { Set(cmd, KeyCode.None); }
    }
}
