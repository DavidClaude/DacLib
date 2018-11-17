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

    public class Property
    {
        /// <summary>
        /// 属性当前值
        /// </summary>
        public float val
        {
            get
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
                return MathFunc.Clamp(fin, min, max);
            }
        }

        /// <summary>
        /// 初始值
        /// </summary>
        /// <value>The original.</value>
        public float orig { get; }

        /// <summary>
        /// 最小值
        /// </summary>
        /// <value>The minimum.</value>
        public float min { get; }

        /// <summary>
        /// 最大值
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
        }

        /// <summary>
        /// 添加修改记录
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
        }

        /// <summary>
        /// 删除修改记录
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
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            _constTraces = new Dictionary<string, float>();
            _percTraces = new Dictionary<string, float>();
        }
    }


    public class Indicator
    {
        /// <summary>
        /// 属性(当前)值
        /// </summary>
        /// <value>The value.</value>
        public float val { get; private set; }

        /// <summary>
        /// 初始值
        /// </summary>
        /// <value>The original.</value>
        public float orig { get; }

        /// <summary>
        /// 最小值
        /// </summary>
        /// <value>The minimum.</value>
        public float min { get; }

        /// <summary>
        /// 最大值
        /// </summary>
        /// <value>The max.</value>
        public float max { get; }

        public Indicator(float origArg, float minArg = 0.0f, float maxArg = 100.0f)
        {
            orig = origArg;
            min = minArg;
            max = maxArg;

            Set(orig);
        }

        /// <summary>
        /// 改变指示值
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
        /// 设置指示值
        /// </summary>
        /// <param name="valArg">Value argument.</param>
        public void Set(float valArg)
        {
            val = MathFunc.Clamp(valArg, min, max);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            Set(orig);
        }
    }

    /// <summary>
    /// 返回信息
    /// </summary>
    public struct Ret
    {
        public static Ret ok
        {
            get
            {
                return new Ret(RetLevel.Info, 0, "");
            }
        }

        public RetLevel level { get; }

        public int code { get; }

        public string desc { get; }

        public Ret(RetLevel levelArg, int codeArg, string descArg)
        {
            level = levelArg;
            code = codeArg;
            desc = descArg;
        }
    }
}