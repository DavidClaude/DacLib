﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DacLib.Generic;

namespace DacLib.Hoxis.Server
{
    public static class HoxisServer
    {
        //public static void InitConfig(out Ret ret, string configPath = "") {
        //    // Read config file
        //    string path;
        //    if (configPath != "") { path = configPath; }
        //    else { path = HoxisConfigs.basicPath + "Configs/hoxis_server.toml"; }
        //    config = new TomlConfiguration(path, out ret);
        //    if (ret.code != 0) { OnInitError(ret); return; }
        //    // Assign ip, port and init the sokcet
        //    serverIP = config.GetString("socket", "server_ip", out ret);
        //    if (ret.code != 0) { OnInitError(ret); return; }
        //    port = config.GetInt("socket", "port", out ret);
        //    if (ret.code != 0) { OnInitError(ret); return; }
        //    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
        //    // Init the extractor
        //    int size = config.GetInt("socket", "read_buffer_size", out ret);
        //    if (ret.code != 0) { OnInitError(ret); return; }
        //    _extractor = new HoxisBytesExtractor(size);
        //    _extractor.onBytesExtracted += ExtractCb;
        //    ret = Ret.ok;
        //}
    }
}