using System;
using System.Collections;
using System.Collections.Generic;

namespace DacLib.Generic
{
    public enum CalcMode
    {
        Const,
        Percentage
    }

    public class Property : IJsonable
    {
        /// <summary>
        /// Current value
        /// </summary>
        public float val { get; private set; }

        /// <summary>
        /// Original value
        /// </summary>
        /// <value>The original.</value>
        public float orig { get; }

        /// <summary>
        /// Minimum value
        /// </summary>
        /// <value>The minimum.</value>
        public float min { get; }

        /// <summary>
        /// Maximum value
        /// </summary>
        /// <value>The max.</value>
        public float max { get; }

        private Dictionary<string, float> _constTraces = new Dictionary<string, float>();
        private Dictionary<string, float> _percTraces = new Dictionary<string, float>();

        public Property(float origArg, float minArg = 0.0f, float maxArg = 100.0f)
        {
            orig = origArg;
            min = minArg;
            max = maxArg;
            OnUpdate();
        }

        /// <summary>
        /// Add one op trace
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="val">Value.</param>
        /// <param name="mode">Mode.</param>
        public void AddTrace(string name, float valArg, CalcMode mode = CalcMode.Const)
        {
            switch (mode)
            {
                case CalcMode.Const:
                    if (!_constTraces.ContainsKey(name))
                    {
                        _constTraces.Add(name, valArg);
                    }
                    break;
                case CalcMode.Percentage:
                    if (!_percTraces.ContainsKey(name))
                    {
                        _percTraces.Add(name, valArg);
                    }
                    break;
            }
            OnUpdate();
        }

        /// <summary>
        /// Remove one op trace
        /// </summary>
        /// <param name="name">Name.</param>
        public void RemoveTrace(string name)
        {
            if (_constTraces.ContainsKey(name))
            {
                _constTraces.Remove(name);
            }
            if (_percTraces.ContainsKey(name))
            {
                _percTraces.Remove(name);
            }
            OnUpdate();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        public void Init()
        {
            _constTraces = new Dictionary<string, float>();
            _percTraces = new Dictionary<string, float>();
            OnUpdate();
        }

        private void OnUpdate()
        {
            float c = 0f;
            foreach (float v in _constTraces.Values)
            {
                c += v;
            }
            float p = 0f;
            foreach (float v in _percTraces.Values)
            {
                p += v;
            }
            float fin = orig + orig * p + c;
            val = MathFunc.Clamp(fin, min, max);
        }

        string IJsonable.ToJson()
        {
            return FormatFunc.JsonAppend("",
                new KV<string,object> { key = "_constTraces", val = _constTraces },
                new KV<string, object> { key = "_percTraces", val = _percTraces }
                );
        }

        void IJsonable.LoadJson(string json)
        {
            Dictionary<string, object> table = FormatFunc.JsonToTable(json);
            _constTraces = FormatFunc.JsonToObject<Dictionary<string, float>>(table["_constTraces"].ToString());
            _percTraces = FormatFunc.JsonToObject<Dictionary<string, float>>(table["_percTraces"].ToString());
            OnUpdate();
        }
    }


    public class Indicator:IJsonable
    {
        /// <summary>
        /// Curent value
        /// </summary>
        /// <value>The value.</value>
        public float val { get; private set; }

        /// <summary>
        /// Original value
        /// </summary>
        /// <value>The original.</value>
        public float orig { get; }

        /// <summary>
        /// Minimum value
        /// </summary>
        /// <value>The minimum.</value>
        public float min { get; }

        /// <summary>
        /// Maximum value
        /// </summary>
        /// <value>The max.</value>
        public float max { get; }

        public Indicator(float origArg, float minArg = 0.0f, float maxArg = 100.0f)
        {
            orig = origArg;
            min = minArg;
            max = maxArg;

            val = MathFunc.Clamp(orig, min, max);
        }

        /// <summary>
        /// Change the value
        /// </summary>
        /// <param name="valArg">Value argument.</param>
        /// <param name="mode">Mode.</param>
        public void Change(float valArg, CalcMode mode = CalcMode.Const)
        {
            float fin = val;
            switch (mode)
            {
                case CalcMode.Const:
                    fin += valArg;
                    break;
                case CalcMode.Percentage:
                    fin *= (1 + valArg);
                    break;
            }
            val = MathFunc.Clamp(fin, min, max);
        }

        /// <summary>
        /// Set the value
        /// </summary>
        /// <param name="valArg">Value argument.</param>
        public void Set(float valArg)
        {
            val = MathFunc.Clamp(valArg, min, max);
        }

        /// <summary>
        /// Initialize
        /// </summary>
        public void Init()
        {
            val = MathFunc.Clamp(orig, min, max);
        }

        string IJsonable.ToJson()
        {
            return FormatFunc.JsonAppend("", new KV<string, object> { key = "val", val = val });
        }

        void IJsonable.LoadJson(string json)
        {
            Dictionary<string, object> table = FormatFunc.JsonToTable(json);
            val = MathFunc.Clamp(float.Parse(table["val"].ToString()), min, max);
        }
    }

    /// <summary>
    /// Generic return information
    /// </summary>
    public struct Ret
    {
        public static readonly Ret ok = new Ret(LogLevel.Info, 0, "");
        public readonly LogLevel level;
        public readonly byte code;
        public readonly string desc;
        public Ret(LogLevel levelArg, byte codeArg, string descArg)
        {
            level = levelArg;
            code = codeArg;
            desc = descArg;
        }
    }

    /// <summary>
    /// Generic pair of key and value
    /// </summary>
    [Serializable]
    public struct KV<TK,TV>
    {
        public TK key;
        public TV val;
    }

    /// <summary>
    /// Both string pair of key and value
    /// </summary>
    [Serializable]
    public struct StringKV
    {
        public string key;
        public string val;
    }

    /// <summary>
    /// Handle used for request
    /// </summary>
    public struct ReqHandle
    {
        public string req;
        public long ts;
    }
}