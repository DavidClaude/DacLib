using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace DacLib.Generic
{
    /// <summary>
    /// Static functions library for converting format
    /// </summary>
    public static class FormatFunc
    {
        /// <summary>
        /// Bytes to string
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string BytesToString(byte[] data, int index, int count) { return Encoding.UTF8.GetString(data, index, count); }
        public static string BytesToString(byte[] data) { return BytesToString(data, 0, data.Length); }

        /// <summary>
        /// String to bytes
        /// </summary>
        /// <returns>The to bytes.</returns>
        /// <param name="msg">Message.</param>
        public static byte[] StringToBytes(string str) { return Encoding.UTF8.GetBytes(str); }

        /// <summary>
        /// String appended
        /// </summary>
        /// <returns>The append.</returns>
        /// <param name="desStr">DES string.</param>
        /// <param name="srcStr">Source string.</param>
        public static string StringAppend(string str, params string[] strs)
        {
            StringBuilder sb = new StringBuilder(str);
            foreach (string s in strs) { sb.Append(s); }
            return sb.ToString();
        }

        /// <summary>
        /// String split
        /// </summary>
        /// <returns>The split.</returns>
        /// <param name="str">String.</param>
        /// <param name="ch">Ch.</param>
        public static string[] StringSplit(string str, char ch)
        {
            string[] strs = str.Split(ch);
            return strs;
        }

        /// <summary>
        /// String replaced
        /// </summary>
        /// <param name="str"></param>
        /// <param name="desStr"></param>
        /// <param name="srcStrs"></param>
        /// <returns></returns>
        public static string StringReplace(string str, string desStr, params string[] srcStrs)
        {
            string s = str;
            foreach (string ss in srcStrs) { s = s.Replace(ss, desStr); }
            return s;
        }

        /// <summary>
        /// Get the last elem of T type array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T LastOfArray<T>(T[] array) { return array[array.Length - 1]; }

        /// <summary>
        /// Object to binaries
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="path">Path.</param>
        /// <param name="mode">Mode.</param>
        /// <param name="access">Access.</param>
        /// <param name="share">Share.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void ObjectToBinary<T>(T obj, string path, FileMode mode = FileMode.Create, FileAccess access = FileAccess.Write, FileShare share = FileShare.None)
        {
            IFormatter fmtr = new BinaryFormatter();
            Stream s = new FileStream(path, mode, access, share);
            fmtr.Serialize(s, obj);
            s.Close();
        }

        /// <summary>
        /// Binaries to object
        /// </summary>
        /// <returns>The to object.</returns>
        /// <param name="path">Path.</param>
        /// <param name="mode">Mode.</param>
        /// <param name="access">Access.</param>
        /// <param name="share">Share.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T BinaryToObject<T>(string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read)
        {
            IFormatter fmtr = new BinaryFormatter();
            Stream s = new FileStream(path, mode, access, share);
            T obj = (T)fmtr.Deserialize(s);
            s.Close();
            return obj;
        }

        /// <summary>
        /// Object to json string
        /// </summary>
        /// <returns>The to json.</returns>
        /// <param name="obj">Object.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static string ObjectToJson(object obj) { return JsonConvert.SerializeObject(obj); }

        /// <summary>
        /// Json string to object
        /// </summary>
        /// <returns>The to object.</returns>
        /// <param name="jsonStr">Json string.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T JsonToObject<T>(string json, out Ret ret)
        {
            T obj;
            try { obj = JsonConvert.DeserializeObject<T>(json); }
            catch (Exception e)
            {
                ret = new Ret(LogLevel.Error, 1, "Json:" + json + " with illegal format\n" + e.Message);
                return default(T);
            }
            ret = Ret.ok;
            return obj;
        }

        public static T JsonToObject<T>(string json)
        {
            Ret ret;
            return JsonToObject<T>(json, out ret);
        }

        /// <summary>
        /// Json to Dictionary<string,object>
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Dictionary<string, object> JsonToTable(string json)
        {
            if (json == "")
                return new Dictionary<string, object>();
            Dictionary<string, object> table = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return table;
        }

        /// <summary>
        /// Json appended
        /// </summary>
        /// <param name="json"></param>
        /// <param name="kvs"></param>
        /// <returns></returns>
        public static string JsonAppend(string json, out Ret ret, params KV[] kvs)
        {
            Dictionary<string, object> table = JsonToTable(json);
            if (table == null)
            {
                ret = new Ret(LogLevel.Error, 1, "Json:" + json + " with illegal format");
                return "";
            }
            foreach (KV kv in kvs) { table.Add(kv.key, kv.val); }
            ret = Ret.ok;
            return ObjectToJson(table);
        }

        public static string JsonAppend(string json, params KV[] kvs)
        {
            Ret ret;
            return JsonAppend(json, out ret, kvs);
        }

        /// <summary>
        /// Does input match ?
        /// </summary>
        /// <returns><c>true</c>, if match was regexed, <c>false</c> otherwise.</returns>
        /// <param name="input">Input.</param>
        /// <param name="pattern">Pattern.</param>
        public static bool RegexMatch(string input, string pattern)
        {
            Regex reg = new Regex(pattern);
            Match match = reg.Match(input);
            return match.Success;
        }

        /// <summary>
        /// Get string in brackets
        /// The brackets must be string whose length is 2
        /// </summary>
        /// <returns>The string in brackets.</returns>
        /// <param name="input">Input.</param>
        /// <param name="brackets">Brackets, such as "()","[]","{}"</param>
        public static string GetStringInBrackets(string input, string brackets)
        {
            //如果brackets字符个数不为2
            if (brackets.Length != 2)
                return "";
            int len = input.Length;
            //如果input为""
            if (len <= 0)
                return "";
            int beginIndex = -1;
            int endIndex = -1;
            for (int i = 0; i < len; i++)
            {
                if (input[i] == brackets[0]) { beginIndex = i; }
                if (input[i] == brackets[1]) { endIndex = i; }
            }
            //如果没有匹配到左括号或右括号
            if (beginIndex == -1 || endIndex == -1)
                return "";
            //如果左括号索引大于等于右括号索引
            if (beginIndex >= endIndex)
                return "";
            return input.Substring(beginIndex + 1, endIndex - beginIndex - 1);
        }
    }
}