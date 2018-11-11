using System;
using System.IO;
using System.Collections.Generic;
using DacLib.Generic;

namespace DacLib.Codex
{
    public class TomlConfiguration
    {
        public const int RET_NOT_TOML = 1;
        public const int RET_NO_FILE = 2;
        public const int RET_NO_SECTION = 3;
        public const int RET_NO_KEY = 4;

        private Dictionary<string, Dictionary<string, string>> _config;

        public TomlConfiguration(string path)
        {
            Ret ret;
            ReadFile(path, out ret);
        }

        public TomlConfiguration(string path, out Ret ret)
        {
            ReadFile(path, out ret);
        }

        public void ReadFile(string path, out Ret ret)
        {
            _config = new Dictionary<string, Dictionary<string, string>>();
            //是否为.toml文件
            string[] strs = path.Split('.');
            if (strs[strs.Length - 1] != "toml")
            {
                ret = new Ret(RetLevel.Error, RET_NOT_TOML, "File:" + path + " isn't .toml");
            }
            //文件是否存在
            if (!File.Exists(path))
            {
                ret = new Ret(RetLevel.Error, RET_NO_FILE, "File:" + path + " doesn't exist");
            }
            //将.toml读入config
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
        /// 获取字符串配置
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="section">Section.</param>
        /// <param name="key">Key.</param>
        public string GetString(string section, string key)
        {
            Ret ret;
            return GetString(section, key, out ret);
        }

        public string GetString(string section, string key, out Ret ret)
        {
            if (!_config.ContainsKey(section))
            {
                ret = new Ret(RetLevel.Error, RET_NO_SECTION, "Section:" + section + " doesn't exist");
                return "";
            }
            if (!_config[section].ContainsKey(key))
            {
                ret = new Ret(RetLevel.Error, RET_NO_KEY, "Key:" + section + " doesn't exist");
                return "";
            }
            ret = Ret.ok;
            return _config[section][key];
        }

        /// <summary>
        /// 获取整型配置
        /// </summary>
        /// <returns>The int.</returns>
        /// <param name="section">Section.</param>
        /// <param name="key">Key.</param>
        /// <param name="ret">Ret.</param>
        public int GetInt(string section, string key)
        {
            Ret ret;
            return GetInt(section, key, out ret);
        }

        public int GetInt(string section, string key, out Ret ret)
        {
            if (!_config.ContainsKey(section))
            {
                ret = new Ret(RetLevel.Error, RET_NO_SECTION, "Section:" + section + " doesn't exist");
                return 0;
            }
            if (!_config[section].ContainsKey(key))
            {
                ret = new Ret(RetLevel.Error, RET_NO_KEY, "Key:" + section + " doesn't exist");
                return 0;
            }
            ret = Ret.ok;
            return int.Parse(_config[section][key]);
        }

        /// <summary>
        /// 获取浮点型配置
        /// </summary>
        /// <returns>The float.</returns>
        /// <param name="section">Section.</param>
        /// <param name="key">Key.</param>
        /// <param name="ret">Ret.</param>
        public float GetFloat(string section, string key)
        {
            Ret ret;
            return GetFloat(section, key, out ret);
        }

        public float GetFloat(string section, string key, out Ret ret)
        {
            if (!_config.ContainsKey(section))
            {
                ret = new Ret(RetLevel.Error, RET_NO_SECTION, "Section:" + section + " doesn't exist");
                return 0f;
            }
            if (!_config[section].ContainsKey(key))
            {
                ret = new Ret(RetLevel.Error, RET_NO_KEY, "Key:" + section + " doesn't exist");
                return 0f;
            }
            ret = Ret.ok;
            return float.Parse(_config[section][key]);
        }

        /// <summary>
        /// 获取布尔型配置
        /// </summary>
        /// <returns><c>true</c>, if bool was gotten, <c>false</c> otherwise.</returns>
        /// <param name="section">Section.</param>
        /// <param name="key">Key.</param>
        /// <param name="ret">Ret.</param>
        public bool GetBool(string section, string key)
        {
            Ret ret;
            return GetBool(section, key, out ret);
        }

        public bool GetBool(string section, string key, out Ret ret)
        {
            if (!_config.ContainsKey(section))
            {
                ret = new Ret(RetLevel.Error, RET_NO_SECTION, "Section:" + section + " doesn't exist");
                return false;
            }
            if (!_config[section].ContainsKey(key))
            {
                ret = new Ret(RetLevel.Error, RET_NO_KEY, "Key:" + section + " doesn't exist");
                return false;
            }
            ret = Ret.ok;
            return _config[section][key] == "true" ? true : false;
        }

        /// <summary>
        /// 获取所有section
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
        /// 获取某section所有key
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
        /// 获取某section所有value
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