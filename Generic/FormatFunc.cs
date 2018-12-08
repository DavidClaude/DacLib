using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;


using C = System.Console;

namespace DacLib.Generic
{
    /// <summary>
    /// Static function library about formats converting
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
        /// String append
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
            string rlt = str;
            foreach (string s in srcStrs) { rlt = rlt.Replace(s, desStr); }
            return rlt;
        }

        public static void ReferStringReplace(ref string str, string desStr, params string[] srcStrs)
        {
            foreach (string s in srcStrs) { str = str.Replace(s, desStr); }
        }

        /// <summary>
        /// Quick generate string with tag like "{0}"
        /// </summary>
        /// <param name="str"></param>
        /// <param name="srcStrs"></param>
        /// <returns></returns>
        public static string StringFormat(string str, params object[] srcStrs)
        {
            // Get the indexes
            string[] tags = RegexGetValue(str, @"\{\d+\}", "{", "}");
            if (tags == null) return str;
            Dictionary<int, string> table = new Dictionary<int, string>();
            Ret ret;
            // Add all tags like "{0}" to table
            foreach (string t in tags)
            {
                int index = StringToInt(t, out ret);
                if (ret.code != 0) return "";
                if (table.ContainsKey(index)) continue;
                table.Add(index, "");
            }
            // Set values of index
            for (int i = 0; i < srcStrs.Length; i++) { if (table.ContainsKey(i)) { table[i] = srcStrs[i].ToString(); } }
            string rlt = str;
            foreach (int i in table.Keys) { rlt = RegexReplace(rlt, @"\{" + i.ToString() + @"\}", table[i]); }
            return rlt;
        }

        /// <summary>
        /// Bytes concat
        /// </summary>
        /// <param name="src"></param>
        /// <param name="bytesArgs"></param>
        /// <returns></returns>
        public static byte[] BytesConcat(byte[] src, params byte[][] bytesArgs)
        {
            byte[] bfin = src;
            foreach (byte[] bs in bytesArgs) { bfin = bfin.Concat(bs).ToArray(); }
            return bfin;
        }

        /// <summary>
        /// Int to bytes array
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static byte[] IntToBytes(int i) { return BitConverter.GetBytes(i); }

        /// <summary>
        /// Bytes array to int
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int BytesToInt(byte[] data, int startIndex = 0) { return BitConverter.ToInt32(data, startIndex); }

        /// <summary>
        /// String to int
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static int StringToInt(string str, out Ret ret)
        {
            int i = 0;
            try { i = int.Parse(str); }
            catch (Exception e) { ret = new Ret(LogLevel.Error, 1, "String:" + str + " with illegal format for int\n" + e.Message); }
            ret = Ret.ok;
            return i;
        }

        /// <summary>
        /// String to unit
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static uint StringToUint(string str, out Ret ret)
        {
            uint i = 0;
            try { i = uint.Parse(str); }
            catch (Exception e) { ret = new Ret(LogLevel.Error, 1, "String:" + str + " with illegal format for uint\n" + e.Message); }
            ret = Ret.ok;
            return i;
        }

        /// <summary>
        /// String to sbyte
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static sbyte StringToSbyte(string str, out Ret ret)
        {
            sbyte i = 0;
            try { i = sbyte.Parse(str); }
            catch (Exception e) { ret = new Ret(LogLevel.Error, 1, "String:" + str + " with illegal format for sbyte\n" + e.Message); }
            ret = Ret.ok;
            return i;
        }

        /// <summary>
        /// String to byte
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static byte StringToByte(string str, out Ret ret)
        {
            byte i = 0;
            try { i = byte.Parse(str); }
            catch (Exception e) { ret = new Ret(LogLevel.Error, 1, "String:" + str + " with illegal format for byte\n" + e.Message); }
            ret = Ret.ok;
            return i;
        }

        /// <summary>
        /// String to short
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static short StringToShort(string str, out Ret ret)
        {
            short i = 0;
            try { i = short.Parse(str); }
            catch (Exception e) { ret = new Ret(LogLevel.Error, 1, "String:" + str + " with illegal format for short\n" + e.Message); }
            ret = Ret.ok;
            return i;
        }

        /// <summary>
        /// String to ushort
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static ushort StringToUshort(string str, out Ret ret)
        {
            ushort i = 0;
            try { i = ushort.Parse(str); }
            catch (Exception e) { ret = new Ret(LogLevel.Error, 1, "String:" + str + " with illegal format for ushort\n" + e.Message); }
            ret = Ret.ok;
            return i;
        }

        /// <summary>
        /// String to long
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static long StringToLong(string str, out Ret ret)
        {
            long i = 0;
            try { i = long.Parse(str); }
            catch (Exception e) { ret = new Ret(LogLevel.Error, 1, "String:" + str + " with illegal format for long\n" + e.Message); }
            ret = Ret.ok;
            return i;
        }

        public static long StringToLong(string str)
        {
            Ret ret;
            return StringToLong(str, out ret);
        }

        /// <summary>
        /// String to ulong
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static ulong StringToUlong(string str, out Ret ret)
        {
            ulong i = 0;
            try { i = ulong.Parse(str); }
            catch (Exception e) { ret = new Ret(LogLevel.Error, 1, "String:" + str + " with illegal format for ulong\n" + e.Message); }
            ret = Ret.ok;
            return i;
        }

        /// <summary>
        /// String to float
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static float StringToFloat(string str, out Ret ret)
        {
            float f = 0;
            try { f = float.Parse(str); }
            catch (Exception e) { ret = new Ret(LogLevel.Error, 1, "String:" + str + " with illegal format for float\n" + e.Message); }
            ret = Ret.ok;
            return f;
        }

        /// <summary>
        /// String to double
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static double StringToDouble(string str, out Ret ret)
        {
            double d = 0;
            try { d = double.Parse(str); }
            catch (Exception e) { ret = new Ret(LogLevel.Error, 1, "String:" + str + " with illegal format for double\n" + e.Message); }
            ret = Ret.ok;
            return d;
        }

        /// <summary>
        /// String to bool
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static bool StringToBool(string str, out Ret ret)
        {
            bool b = false;
            try { b = bool.Parse(str); }
            catch (Exception e) { ret = new Ret(LogLevel.Error, 1, "String:" + str + " with illegal format for bool\n" + e.Message); }
            ret = Ret.ok;
            return b;
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
        public static string JsonAppend(string json, out Ret ret, params KV<string, object>[] kvs)
        {
            Dictionary<string, object> table = JsonToTable(json);
            if (table == null)
            {
                ret = new Ret(LogLevel.Error, 1, "Json:" + json + " with illegal format");
                return "";
            }
            foreach (KV<string, object> kv in kvs) { table.Add(kv.key, kv.val); }
            ret = Ret.ok;
            return ObjectToJson(table);
        }

        public static string JsonAppend(string json, params KV<string, object>[] kvs)
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
        public static bool RegexMatch(string input, string pattern) { return Regex.IsMatch(input, pattern); }

        /// <summary>
        /// Regex match and replace
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <param name="desStr"></param>
        /// <returns></returns>
        public static string RegexReplace(string input, string pattern, string desStr)
        {
            string rlt = input;
            MatchCollection mc = Regex.Matches(input, pattern);
            foreach (Match m in mc) { rlt = StringReplace(rlt, desStr, m.Value); }
            return rlt;
        }

        public static void ReferRegexReplace(ref string input, string pattern, string desStr)
        {
            MatchCollection mc = Regex.Matches(input, pattern);
            foreach (Match m in mc) { ReferStringReplace(ref input, desStr, m.Value); }
        }

        /// <summary>
        /// Get the value with given pattern
        /// Need trim the strings besides value
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <param name="trims"></param>
        /// <returns></returns>
        public static string[] RegexGetValue(string input, string pattern, params string[] trims)
        {
            MatchCollection mc = Regex.Matches(input, pattern);
            if (mc.Count <= 0) return null;
            string[] rlts = new string[mc.Count];
            int index = 0;
            foreach (Match m in mc) { rlts[index] = StringReplace(m.Value, "", trims); index++; }
            return rlts;
        }

        /// <summary>
        /// Encode bytes to string by base64
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Base64EncodeToString(byte[] data) { return Convert.ToBase64String(data); }

        /// <summary>
        /// Encode string to string by base64
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Base64EncodeToString(string str) { return Base64EncodeToString(StringToBytes(str)); }

        /// <summary>
        /// Decode string to bytes by base64
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] Base64DecodeToBytes(string str) { return Convert.FromBase64String(str); }
    }
}