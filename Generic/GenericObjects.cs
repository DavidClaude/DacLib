using System;
using System.Collections;
using System.Collections.Generic;

namespace DacLib.Generic
{
    public struct Property:IInitializable
    {
        /// <summary>
        /// Current value
        /// </summary>
        public float value
        {
            get
            {
                float c = 0f;
                foreach (float v in constTraces.Values) { c += v; }
                float p = 0f;
                foreach (float v in percTraces.Values) { p += v; }
                float fin = origin + origin * p + c;
                return MathFunc.Clamp(fin, min, max);
            }
        }

        public float origin;
        public float min;
        public float max;
        public Dictionary<string, float> constTraces;
        public Dictionary<string, float> percTraces;

        public Property(float originArg, float minArg = 0.0f, float maxArg = 100.0f)
        {
            origin = originArg;
            min = minArg;
            max = maxArg;
            constTraces = new Dictionary<string, float>();
            percTraces = new Dictionary<string, float>();
        }

        /// <summary>
        /// Add one trace
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="val">Value.</param>
        /// <param name="mode">Mode.</param>
        public void AddTrace(string name, float valArg, DeltaMode mode = DeltaMode.Const)
        {
            switch (mode)
            {
                case DeltaMode.Const:
                    if (!constTraces.ContainsKey(name)) { constTraces.Add(name, valArg); }
                    break;
                case DeltaMode.Percentage:
                    if (!percTraces.ContainsKey(name)) { percTraces.Add(name, valArg); }
                    break;
            }
        }

        /// <summary>
        /// Remove one trace
        /// </summary>
        /// <param name="name">Name.</param>
        public void RemoveTrace(string name)
        {
            if (constTraces.ContainsKey(name)) { constTraces.Remove(name); }
            if (percTraces.ContainsKey(name)) { percTraces.Remove(name); }
        }

        public void Initialize()
        {
            constTraces.Clear();
            percTraces.Clear();
        }
    }


    public struct Indicator :IInitializable
    {
        /// <summary>
        /// Current value
        /// </summary>
        public float value;
        public float origin;
        public float min;
        public float max;

        public Indicator(float originArg, float minArg = 0.0f, float maxArg = 100.0f)
        {
            origin = originArg;
            min = minArg;
            max = maxArg;
            value = MathFunc.Clamp(origin, min, max);
        }

        
        public void SetDelta(float valArg, DeltaMode mode = DeltaMode.Const)
        {
            float fin = value;
            switch (mode)
            {
                case DeltaMode.Const:
                    fin += valArg;
                    break;
                case DeltaMode.Percentage:
                    fin *= (1 + valArg);
                    break;
            }
            value = MathFunc.Clamp(fin, min, max);
        }

        public void SetValue(float valArg) { value = MathFunc.Clamp(valArg, min, max); }

        public void Initialize() { value = MathFunc.Clamp(origin, min, max); }
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
    public struct KV<TK, TV>
    {
        public KV(TK k, TV v)
        {
            key = k;
            val = v;
        }
        public TK key;
        public TV val;
    }

    /// <summary>
    /// Both string pair of key and value
    /// </summary>
    [Serializable]
    public struct KVString
    {
        public string key;
        public string val;
        public KVString(string k, string v)
        {
            key = k;
            val = v;
        }
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