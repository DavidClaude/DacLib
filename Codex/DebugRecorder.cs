using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DacLib.Generic;

using FF = DacLib.Generic.FormatFunc;

namespace DacLib.Codex
{
    public class DebugRecorder
    {
        public const byte RET_OPENORCREATE_FILE_EXCEPTION = 1;

        private FileStream _stream;
        private StreamWriter _writer;

        public DebugRecorder(string path)
        {
            _stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        public void Start(out Ret ret, string pattern)
        {
            if (_stream == null)
            {
                ret = new Ret(LogLevel.Error, RET_OPENORCREATE_FILE_EXCEPTION, "File stream is null, open or create exception");
            }
            _writer = new StreamWriter(_stream);
            ret = Ret.ok;

        }

        public void End()
        {
            _writer.Close();
            _stream.Close();
        }
        public void Flush()
        {
            _writer.Flush();
            _stream.Flush();
        }

        public void Log(string content, LogLevel level)
        {
            //_writer.WriteLine(FF.StringAppend())
        }

        public void LogInfo(string content)
        {

        }

        public void LogWarning(string content)
        {

        }

        public void LogError(string content)
        {

        }

        private void LogPattern(string pattern)
        {

        }
    }
}
