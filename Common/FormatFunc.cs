using System;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace DacLib.Common
{
	/// <summary>
	/// 格式转换相关的静态方法库
	/// </summary>
	public static class FormatFunc
	{
		/// <summary>
		/// 字节流转字符串
		/// </summary>
		/// <returns>The to string.</returns>
		/// <param name="data">Data.</param>
		public static string BytesToString (byte[] data)
		{
			return BytesToString (data, 0, data.Length);
		}

		public static string BytesToString (byte[] data, int index, int count)
		{
			return Encoding.UTF8.GetString (data, index, count);
		}

		/// <summary>
		/// 字符串转字节流
		/// </summary>
		/// <returns>The to bytes.</returns>
		/// <param name="msg">Message.</param>
		public static byte[] StringToBytes (string msg)
		{
			return Encoding.UTF8.GetBytes (msg);
		}

		/// <summary>
		/// 字符串拼接
		/// </summary>
		/// <returns>The append.</returns>
		/// <param name="desStr">DES string.</param>
		/// <param name="srcStr">Source string.</param>
		public static string StringAppend (string desStr, params string[] strs)
		{
			StringBuilder sb = new StringBuilder (desStr);
			int len = strs.Length;
			for (int i = 0; i < len; i++) {
				sb.Append (strs [i]);
			}
			return sb.ToString ();
		}

		/// <summary>
		/// 字符串分隔
		/// </summary>
		/// <returns>The split.</returns>
		/// <param name="str">String.</param>
		/// <param name="ch">Ch.</param>
		public static string[] StringSplit (string str, char ch)
		{
			string[] strs = str.Split (ch);
			return strs;
		}

		/// <summary>
		/// 对象实例转二进制文件
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="path">Path.</param>
		/// <param name="mode">Mode.</param>
		/// <param name="access">Access.</param>
		/// <param name="share">Share.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void ObjectToBinary<T> (T obj, string path, FileMode mode = FileMode.Create, FileAccess access = FileAccess.Write, FileShare share = FileShare.None)
		{
			IFormatter fmtr = new BinaryFormatter ();
			Stream s = new FileStream (path, mode, access, share);
			fmtr.Serialize (s, obj);
			s.Close ();
		}

		/// <summary>
		/// 二进制文件转对象实例
		/// </summary>
		/// <returns>The to object.</returns>
		/// <param name="path">Path.</param>
		/// <param name="mode">Mode.</param>
		/// <param name="access">Access.</param>
		/// <param name="share">Share.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T BinaryToObject<T> (string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read)
		{
			IFormatter fmtr = new BinaryFormatter ();
			Stream s = new FileStream (path, mode, access, share);
			T obj = (T)fmtr.Deserialize (s);
			s.Close ();
			return obj;
		}

		/// <summary>
		/// 对象实例转Json
		/// </summary>
		/// <returns>The to json.</returns>
		/// <param name="obj">Object.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static string ObjectToJson<T> (T obj)
		{
			string s = JsonConvert.SerializeObject (obj);
			return s;
		}

		/// <summary>
		/// Json转对象实例
		/// </summary>
		/// <returns>The to object.</returns>
		/// <param name="jsonStr">Json string.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T JsonToObject<T> (string jsonStr)
		{
			T obj = JsonConvert.DeserializeObject<T> (jsonStr);
			return obj;
		}

//		/// <summary>
//		/// 枚举转字符串
//		/// </summary>
//		/// <returns>The to string.</returns>
//		/// <param name="em">Em.</param>
//		/// <typeparam name="T">The 1st type parameter.</typeparam>
//		public static string EnumToString<T> (T em)
//		{
//			return em.ToString ();
//		}
//
//		/// <summary>
//		/// 字符串转枚举
//		/// </summary>
//		/// <returns>The to enum.</returns>
//		/// <param name="str">String.</param>
//		/// <typeparam name="T">The 1st type parameter.</typeparam>
//		public static T StringToEnum<T> (string str)
//		{
//			T em;
//			try {
//				em = (T)Enum.Parse (typeof(T), str);
//			} catch (Exception e) {
//				
//			}
//			return em;
//		}
//
//		/// <summary>
//		/// 枚举转整型
//		/// </summary>
//		/// <returns>The to int.</returns>
//		/// <param name="em">Em.</param>
//		/// <typeparam name="T">The 1st type parameter.</typeparam>
//		public static int EnumToInt<T> (T em)
//		{
//			return (int)em;
//		}
//
//		/// <summary>
//		/// 整型转枚举
//		/// </summary>
//		/// <returns>The to enum.</returns>
//		/// <param name="i">The index.</param>
//		/// <typeparam name="T">The 1st type parameter.</typeparam>
//		public static T IntToEnum<T> (int i)
//		{
//			T em;
//			if (Enum.IsDefined (typeof(T), i)) {
//				em = (T)i;
//			}
//			return em;
//		}
	}
}