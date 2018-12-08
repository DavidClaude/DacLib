using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DacLib.Generic;

using FF = DacLib.Generic.FormatFunc;
using SF = DacLib.Generic.SystemFunc;

namespace DacLib.Codex
{
    public class DebugRecorder
    {
        public const byte RET_STREAM_INIT_ERROR = 1;

        public FileMode fileMode { get; }
        public FileAccess fileAccess { get; }
        public LogWriteMode writeMode { get; }

        private FileStream _stream;
        private StreamWriter _writer;

        public DebugRecorder(string path, out Ret ret, FileMode mode = FileMode.OpenOrCreate, FileAccess access = FileAccess.ReadWrite, LogWriteMode write = LogWriteMode.Append)
        {
            fileMode = mode;
            fileAccess = access;
            writeMode = write;
            try
            {
                _stream = new FileStream(path, fileMode, access);
                if (writeMode == LogWriteMode.Append) _stream.Position = _stream.Length;
                ret = Ret.ok;
            }
            catch (Exception e) { ret = new Ret(LogLevel.Error, RET_STREAM_INIT_ERROR, e.Message); }
        }
        public void Begin(string pattern = "") { _writer = new StreamWriter(_stream); }
        public void Flush() { _stream.Flush(); }
        public void Close() { _writer.Close(); _stream.Close(); }
        public void End() { _writer.WriteLine(""); Flush(); Close(); }

        public void Log(string content, bool console = false)
        {
            _writer.WriteLine(content);
            if (console) Console.WriteLine(content);
        }
        public void LogTag(string content, string tag = "", string speaker = "", bool console = false)
        {
            string s;
            if (speaker == "") s = FF.StringFormat("[{0}]{1}:  {2}", SF.GetDateTime(), tag, content);
            else s = FF.StringFormat("[{0}]{1}:  {2}--->{3}", SF.GetDateTime(), tag, speaker, content);
            _writer.WriteLine(s);
            if (console) Console.WriteLine(s);
        }
        public void LogInfo(string content, string speaker, bool console = false) { LogTag(content, "Info", speaker, console); }
        public void LogWarning(string content, string speaker, bool console = false) { LogTag(content, "Warning", speaker, console); }
        public void LogError(string content, string speaker, bool console = false) { LogTag(content, "Error", speaker, console); }
        public void LogFatal(string content, string speaker, bool console = false) { LogTag(content, "Fatal", speaker, console); End(); }
        public void LogPattern(string copyright, string version, string project)
        {
            Log("=============== Hoxis Server ===============");
            Log("-- Copyright: " + copyright);
            Log("-- Version: " + version);
            Log("-- Project: " + project);
            Log("-- IPv4: " + SF.GetLocalIP());
            Log("-- Platform: " + SF.GetOSVersion());
            Log("-- Time: " + SF.GetDateTime().ToString());
            Log("=================== logs ===================");
        }
    }
}
