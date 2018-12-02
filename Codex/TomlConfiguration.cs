using System;
using System.IO;
using System.Collections.Generic;
using DacLib.Generic;

namespace DacLib.Codex
{
    public class TomlConfiguration
    {
        #region ret codes
        public const byte RET_NOT_TOML = 1;
        public const byte RET_NO_FILE = 2;
        public const byte RET_NO_SECTION = 3;
        public const byte RET_NO_KEY = 4;
        public const byte RET_ILLEGAL_ITEM = 5;
        #endregion

        private Dictionary<string, Dictionary<string, string>> _config;

        public TomlConfiguration(string path)
        {
            Ret ret;
            ReadFile(path, out ret);
        }

        public TomlConfiguration(string path, out Ret ret) { ReadFile(path, out ret); }

        /// <summary>
        /// Read the .toml file into _config
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ret"></param>
        public void ReadFile(string path, out Ret ret)
        {
            _config = new Dictionary<string, Dictionary<string, string>>();
            // Is toml file ?
            string[] paths = FormatFunc.StringSplit(path, '/');
            string[] strs = FormatFunc.StringSplit(FormatFunc.LastOfArray(paths), '.');
            if (FormatFunc.LastOfArray(strs) != "toml")
            {
                ret = new Ret(LogLevel.Error, RET_NOT_TOML, "File:" + path + " isn't .toml");
                return;
            }
            // Does file exist ?
            if (!File.Exists(path))
            {
                ret = new Ret(LogLevel.Error, RET_NO_FILE, "File:" + path + " doesn't exist");
                return;
            }
            // Push toml to config
            string curSec = "";
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string lineTrim = line.Trim();
                if (FormatFunc.RegexMatch(lineTrim, @"^\[.+\]$"))
                {
                    string section = FormatFunc.GetStringInBrackets(lineTrim, "[]");
                    _config.Add(section, new Dictionary<string, string>());
                    curSec = section;
                    continue;
                }
                if (curSec == "")
                    continue;
                string[] kv = lineTrim.Split('=');
                if (kv.Length != 2)
                    continue;
                _config[curSec].Add(kv[0].Trim(), kv[1].Trim());
            }
            ret = Ret.ok;
        }

        /// <summary>
        /// Get config of string type
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public string GetString(string section, string key, out Ret ret)
        {
            if (!ContainItem(section, key, out ret))
                return "";
            ret = Ret.ok;
            return _config[section][key];
        }

        public string GetString(string section, string key)
        {
            Ret ret;
            return GetString(section, key, out ret);
        }

        /// <summary>
        /// Get config of int type
        /// </summary>
        /// <returns>The int.</returns>
        /// <param name="section">Section.</param>
        /// <param name="key">Key.</param>
        /// <param name="ret">Ret.</param>
        public int GetInt(string section, string key, out Ret ret)
        {
            if (!ContainItem(section, key, out ret))
                return 0;
            return FormatFunc.StringToInt(_config[section][key], out ret);
        }

        public int GetInt(string section, string key)
        {
            Ret ret;
            return GetInt(section, key, out ret);
        }

        /// <summary>
        /// Get config of short type
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public short GetShort(string section, string key, out Ret ret)
        {
            if (!ContainItem(section, key, out ret))
                return 0;
            return FormatFunc.StringToShort(_config[section][key], out ret);
        }

        public short GetShort(string section, string key)
        {
            Ret ret;
            return GetShort(section, key, out ret);
        }

        /// <summary>
        /// Get config of long type
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public long GetLong(string section, string key, out Ret ret)
        {
            if (!ContainItem(section, key, out ret))
                return 0;
            return FormatFunc.StringToLong(_config[section][key], out ret);
        }

        public long GetLong(string section, string key)
        {
            Ret ret;
            return GetLong(section, key, out ret);
        }

        /// <summary>
        /// Get config of float type
        /// </summary>
        /// <returns>The float.</returns>
        /// <param name="section">Section.</param>
        /// <param name="key">Key.</param>
        /// <param name="ret">Ret.</param>
        public float GetFloat(string section, string key, out Ret ret)
        {
            if (!ContainItem(section, key, out ret))
                return 0f;
            return FormatFunc.StringToFloat(_config[section][key], out ret);
        }

        public float GetFloat(string section, string key)
        {
            Ret ret;
            return GetFloat(section, key, out ret);
        }

        /// <summary>
        /// Get config of bool type
        /// </summary>
        /// <returns><c>true</c>, if bool was gotten, <c>false</c> otherwise.</returns>
        /// <param name="section">Section.</param>
        /// <param name="key">Key.</param>
        /// <param name="ret">Ret.</param>
        public bool GetBool(string section, string key, out Ret ret)
        {
            if (!ContainItem(section, key, out ret))
                return false;
            return FormatFunc.StringToBool(_config[section][key], out ret);
        }

        public bool GetBool(string section, string key)
        {
            Ret ret;
            return GetBool(section, key, out ret);
        }

        /// <summary>
        /// Does toml contain the given item ?
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public bool ContainItem(string section, string key, out Ret ret)
        {
            if (!_config.ContainsKey(section))
            {
                ret = new Ret(LogLevel.Error, RET_NO_SECTION, "Section:" + section + " doesn't exist");
                return false;
            }
            if (!_config[section].ContainsKey(key))
            {
                ret = new Ret(LogLevel.Error, RET_NO_KEY, "Section:" + section + ", key:" + key + " doesn't exist");
                return false;
            }
            ret = Ret.ok;
            return true;
        }

        public bool ContainItem(string section, string key)
        {
            Ret ret;
            return ContainItem(section, key, out ret);
        }

        /// <summary>
        /// Get all sections
        /// </summary>
        /// <returns>The sections.</returns>
        public string[] GetSections()
        {
            int count = _config.Keys.Count;
            string[] keys = new string[count];
            int index = 0;
            foreach (string sec in _config.Keys)
            {
                keys[index] = sec;
                index++;
            }
            return keys;
        }

        /// <summary>
        /// Get all keys of given section
        /// </summary>
        /// <returns>The section keys.</returns>
        /// <param name="section">Section.</param>
        public string[] GetSectionKeys(string section)
        {
            if (!_config.ContainsKey(section))
                return null;
            int count = _config[section].Keys.Count;
            string[] sections = new string[count];
            int index = 0;
            foreach (string sec in _config[section].Keys)
            {
                sections[index] = sec;
                index++;
            }
            return sections;
        }

        /// <summary>
        /// Get all values of given section
        /// </summary>
        /// <returns>The section values.</returns>
        /// <param name="section">Section.</param>
        public string[] GetSectionValues(string section)
        {
            if (!_config.ContainsKey(section))
                return null;
            int count = _config[section].Values.Count;
            string[] values = new string[count];
            int index = 0;
            foreach (string sec in _config[section].Values)
            {
                values[index] = sec;
                index++;
            }
            return values;
        }
    }
}