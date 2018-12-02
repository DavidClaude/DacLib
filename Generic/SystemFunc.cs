using System;
using System.Net;
using System.Net.Sockets;

namespace DacLib.Generic
{
    public static class SystemFunc
    {
        public static long timeStamp
        {
            get
            {
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1);
                return (long)ts.TotalSeconds;
            }
        }


        /// <summary>
        /// Get IPv4 of host 
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static string GetLocalIP(out Ret ret)
        {
            try
            {
                string name = Dns.GetHostName();
                IPHostEntry entry = Dns.GetHostEntry(name);
                foreach (IPAddress ipa in entry.AddressList)
                {
                    if (ipa.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ret = Ret.ok;
                        return ipa.ToString();
                    }
                }
                ret = new Ret(LogLevel.Warning, 1, "No IPv4 by host name: " + name);
                return "";
            }
            catch (Exception e)
            {
                ret = new Ret(LogLevel.Error, 2, e.Message);
                return "";
            }

        }
        public static string GetLocalIP()
        {
            Ret ret;
            return GetLocalIP(out ret);
        }

        /// <summary>
        /// Get current time stamp
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static long GetTimeStamp(string unit = "s")
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1);
            long tsLong = 0;
            switch (unit) {
                case "s":
                    tsLong = (long)ts.TotalSeconds;
                    break;
                case "ms":
                    tsLong = (long)ts.TotalMilliseconds;
                    break;
            }
            return tsLong;
        }
    }
}
