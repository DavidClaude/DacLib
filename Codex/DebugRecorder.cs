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
    /// <summary>
    /// Record the logs to native files
    /// </summary>
    public class DebugRecorder
    {
        public const byte RET_STREAM_INIT_ERROR = 1;

        public bool enable { get; private set; }

        private FileStream _stream;
        private StreamWriter _writer;

        public DebugRecorder(string path, out Ret ret, FileMode mode = FileMode.OpenOrCreate, FileAccess access = FileAccess.ReadWrite, LogWriteMode write = LogWriteMode.Append)
        {
            try
            {
                _stream = new FileStream(path, mode, access);
                if (write == LogWriteMode.Append) _stream.Position = _stream.Length;
                ret = Ret.ok;
            }
            catch (Exception e) { ret = new Ret(LogLevel.Error, RET_STREAM_INIT_ERROR, e.Message); }
            enable = false;
        }

        /// <summary>
        /// Get if enable sately
        /// </summary>
        /// <param name="recorder"></param>
        /// <returns></returns>
        public static bool LogEnable(DebugRecorder recorder) { if (recorder == null) return false;return recorder.enable; }

        public void Begin()
        {
            if (enable) return;
            _writer = new StreamWriter(_stream);
            enable = true;
        }
        public void Flush() { _stream.Flush(); }
        public void Dispose() { _writer.Dispose();_stream.Dispose(); }
        public void Close() { _writer.Close(); _stream.Close(); }
        public void End()
        {
            if (!enable) return;
            _writer.WriteLine("");
            Flush();
            Dispose();
            Close();
            enable = false;
        }

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
        public void LogTitle(string copyright, string version, string project)
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
