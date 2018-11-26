using System;
using System.Net;
using System.Net.Sockets;

namespace DacLib.Generic
{
    public static class SystemFunc
    {
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
            catch (Exception e){
                ret = new Ret(LogLevel.Error, 2, e.Message);
                return "";
            }
            
        }
        public static string GetLocalIP()
        {
            Ret ret;
            return GetLocalIP(out ret);
        }
    }
}
