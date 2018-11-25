using System;
using System.Net;
using System.Net.Sockets;

namespace DacLib.Generic
{
    public static class SystemFunc
    {
        public static string GetLocalIP(out Ret ret)
        {
            try
            {
                string name = Dns.GetHostName();
                IPHostEntry entry = Dns.GetHostByName(name);
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
    }
}
