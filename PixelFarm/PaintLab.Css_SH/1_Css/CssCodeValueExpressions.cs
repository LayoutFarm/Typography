//BSD, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using System.Text;
using LayoutFarm.Css;
using PixelFarm.Drawing;

namespace LayoutFarm.WebDom
{
    public abstract class CssCodeValueExpression
    {
#if DEBUG
        static int dbugTotalId;
        public readonly int dbugId = dbugTotalId++;
#endif

        public CssCodeValueExpression(CssValueHint hint)
        {
#if DEBUG
            //if (this.dbugId == 111)
            //{

            //}
#endif
            this.Hint = hint;
        }


        CssValueEvaluatedAs _evaluatedAs;
        PixelFarm.Drawing.Color _cachedColor;
        LayoutFarm.Css.CssLength _cachedLength;
        int _cachedInt;
        protected float _number;
        public bool IsInherit
        {
            get;
            internal set;
        }
        public CssValueHint Hint
        {
            get;
            private set;
        }
        //------------------------------------------------------
        public float AsNumber()
        {
            return _number;
        }

        public void SetIntValue(int intValue, CssValueEvaluatedAs evaluatedAs)
        {
            _evaluatedAs = evaluatedAs;
            _cachedInt = intValue;
        }
        public void SetColorValue(PixelFarm.Drawing.Color color)
        {
            _evaluatedAs = CssValueEvaluatedAs.Color;
            _cachedColor = color;
        }
        public void SetCssLength(CssLength len, WebDom.CssValueEvaluatedAs evalAs)
        {
            _cachedLength = len;
            _evaluatedAs = evalAs;
        }

        //
        public CssValueEvaluatedAs EvaluatedAs => _evaluatedAs;
        //
        public Color GetCacheColor() => _cachedColor;
        //
        public CssLength GetCacheCssLength() => _cachedLength;
        //
        public virtual string GetTranslatedStringValue() => this.ToString();
        //
        public int GetCacheIntValue() => _cachedInt;
    }
    public class CssCodeColor : CssCodeValueExpression
    {

        public CssCodeColor(Color color)
            : base(CssValueHint.HexColor)
        {
            ActualColor = color;
            SetColorValue(color);
        }
        public Color ActualColor { get; private set; }
    }



    public class CssCodePrimitiveExpression : CssCodeValueExpression
    {

        readonly string _propertyValue;
        public CssCodePrimitiveExpression(string value, CssValueHint hint)
            : base(hint)
        {
            _propertyValue = value;
            switch (hint)
            {
                case CssValueHint.Iden:
                    {
                        //check value  
                        this.IsInherit = value == "inherit";
                    }
                    break;
                case CssValueHint.Number:
                    {
                        _number = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    break;
            }
        }
        public CssCodePrimitiveExpression(float number)
            : base(CssValueHint.Number)
        {
            //number             
            _number = number;
        }
        //
        public string Unit { get; set; }
        //
        public string Value => _propertyValue;
        //
        public override string ToString()
        {
            switch (this.Hint)
            {
                case CssValueHint.Number:
                    {
                        if (Unit != null)
                        {
                            return _number.ToString() + Unit;
                        }
                        else
                        {
                            return _number.ToString();
                        }
                    }
                default:
                    if (Unit != null)
                    {
                        return Value + Unit;
                    }
                    else
                    {
                        return Value;
                    }
            }
        }
    }



    public class CssCodeFunctionCallExpression : CssCodeValueExpression
    {
        string _evaluatedStringValue;
        bool _isEval;
        List<CssCodeValueExpression> _funcArgs = new List<CssCodeValueExpression>();
        public CssCodeFunctionCallExpression(string funcName)
            : base(CssValueHint.Func)
        {
            this.FunctionName = funcName;
        }
        public string FunctionName
        {
            get;
            private set;
        }
        public void AddFuncArg(CssCodeValueExpression arg)
        {
            _funcArgs.Add(arg);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.FunctionName);
            sb.Append('(');
            int j = _funcArgs.Count;
            for (int i = 0; i < j; ++i)
            {
                sb.Append(_funcArgs[i].ToString());
                if (i < j - 1)
                {
                    sb.Append(',');
                }
            }
            sb.Append(')');
            return sb.ToString();
        }
        public override string GetTranslatedStringValue()
        {
            if (_isEval)
            {
                return _evaluatedStringValue;
            }
            else
            {
                _isEval = true;
                switch (this.FunctionName)
                {
                    case "rgb":
                        {
                            //css color function rgb
                            //each is number 
                            //TODO:  	Defines the intensity of red as an integer between 0 and 255, or as a percentage value between 0% and 100%
                            byte r_value = (byte)_funcArgs[0].AsNumber();
                            byte g_value = (byte)_funcArgs[1].AsNumber();
                            byte b_value = (byte)_funcArgs[2].AsNumber();
                            return _evaluatedStringValue = string.Concat("#",
                                ConvertByteToStringWithPadding(r_value),
                                ConvertByteToStringWithPadding(g_value),
                                ConvertByteToStringWithPadding(b_value));
                        }
                    case "rgba":
                        {
                            byte r_value = (byte)_funcArgs[0].AsNumber();
                            byte g_value = (byte)_funcArgs[1].AsNumber();
                            byte b_value = (byte)_funcArgs[2].AsNumber();
                            //Defines the opacity as a number between 0.0(fully transparent) and 1.0(fully opaque)
                            float a_value_f = _funcArgs[3].AsNumber();
                            if (a_value_f < 0)
                            {
                                a_value_f = 0;
                            }
                            else if (a_value_f > 1)
                            {
                                a_value_f = 1;
                            }
                            byte a_value = (byte)(a_value_f * 255);
                            return _evaluatedStringValue = string.Concat("#",
                                ConvertByteToStringWithPadding(a_value), //*** argb
                                ConvertByteToStringWithPadding(r_value),
                                ConvertByteToStringWithPadding(g_value),
                                ConvertByteToStringWithPadding(b_value)
                                );
                        }
                    //TODO: implement rgba here
                    case "url":
                        {
                            return _evaluatedStringValue = _funcArgs[0].ToString();
                        }
                    default:
                        {
                            return _evaluatedStringValue = this.ToString();
                        }
                }
            }
        }
        static string ConvertByteToStringWithPadding(byte colorByte)
        {
            string hex = colorByte.ToString("X");
            if (hex.Length < 2)
            {
                return "0" + hex;
            }
            else
            {
                return hex;
            }
        }
    }

    public class CssCodeBinaryExpression : CssCodeValueExpression
    {
        public CssCodeBinaryExpression()
            : base(CssValueHint.BinaryExpression)
        {
        }
        public CssValueOpName OpName
        {
            get;
            set;
        }
        public CssCodeValueExpression Left
        {
            get;
            set;
        }
        public CssCodeValueExpression Right
        {
            get;
            set;
        }
        public override string ToString()
        {
            StringBuilder stbuilder = new StringBuilder();
            if (Left != null)
            {
                stbuilder.Append(Left.ToString());
            }
            else
            {
                throw new NotSupportedException();
            }
            switch (this.OpName)
            {
                case CssValueOpName.Unknown:
                    {
                        throw new NotSupportedException();
                    }
                case CssValueOpName.Divide:
                    {
                        stbuilder.Append('/');
                    }
                    break;
            }
            if (Right != null)
            {
                stbuilder.Append(Right.ToString());
            }
            else
            {
                throw new NotSupportedException();
            }
            return stbuilder.ToString();
        }
        public override string GetTranslatedStringValue()
        {
            throw new NotImplementedException();
        }
    }
}