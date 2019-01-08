using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DacLib.Codex
{
    public class Singleton<T> where T : class, new()
    {
        public static T Ins { get { if (_Ins == null) _Ins = new T(); return _Ins; } }
        private static T _Ins;
    }
}
