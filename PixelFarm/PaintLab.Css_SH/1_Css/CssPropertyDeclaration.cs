//BSD, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using System.Text;
namespace LayoutFarm.WebDom
{
    public class CssPropertyDeclaration
    {
        bool _isAutoGen;
        bool _markedAsInherit;
        CssCodeValueExpression _firstValue;
        List<CssCodeValueExpression> _moreValues;
#if DEBUG
        static int dbugTotalId;
        public readonly int dbugId = dbugTotalId++;
#endif
        public CssPropertyDeclaration(string unknownName)
        {
            //convert from name to wellknown property name; 
            this.UnknownRawName = unknownName;
        }
        public CssPropertyDeclaration(WellknownCssPropertyName wellNamePropertyName)
        {

            //convert from name to wellknown property name; 
            this.WellknownPropertyName = wellNamePropertyName;
        }
        public CssPropertyDeclaration(WellknownCssPropertyName wellNamePropertyName, CssCodeValueExpression value)
        {
            //from another 
            this.WellknownPropertyName = wellNamePropertyName;
            _firstValue = value;
            _markedAsInherit = value.IsInherit;
            //auto gen from another prop
            _isAutoGen = true;
        }
        public bool IsExpand { get; set; }
        public string UnknownRawName { get; private set; }

        public void AddValue(CssCodeValueExpression value)
        {
            if (_firstValue == null)
            {
                _markedAsInherit = value.IsInherit;
                _firstValue = value;
            }
            else
            {
                if (_moreValues == null)
                {
                    _moreValues = new List<CssCodeValueExpression>();
                }
                _moreValues.Add(value);
                _markedAsInherit = false;
            }
        }
        public void ReplaceValue(int index, CssCodeValueExpression value)
        {
            if (index == 0)
            {
                _firstValue = value;
            }
            else
            {
                _moreValues[index - 1] = value;
            }
        }

        public WellknownCssPropertyName WellknownPropertyName
        {
            get;
            private set;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.WellknownPropertyName.ToString());
            sb.Append(':');
            CollectValues(sb);
            return sb.ToString();
        }
        void CollectValues(StringBuilder stBuilder)
        {
            if (_firstValue != null)
            {
                stBuilder.Append(_firstValue.ToString());
            }
            if (_moreValues != null)
            {
                int j = _moreValues.Count;
                for (int i = 0; i < j; ++i)
                {
                    CssCodeValueExpression propV = _moreValues[i];
                    stBuilder.Append(propV.ToString());
                    if (i < j - 1)
                    {
                        stBuilder.Append(' ');
                    }
                }
            }
        }
        //
        public bool MarkedAsInherit => _markedAsInherit;
        //
        public int ValueCount
        {
            get
            {
                if (_moreValues == null)
                {
                    if (_firstValue == null)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return _moreValues.Count + 1;
                }
            }
        }


        public CssCodeValueExpression GetPropertyValue(int index)
        {
            switch (index)
            {
                case 0:
                    {
                        return _firstValue;
                    }
                default:
                    {
                        if (_moreValues != null)
                        {
                            return _moreValues[index - 1];
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
            }
        }
    }

    public enum CssValueHint : byte
    {
        Unknown,
        Number,
        HexColor,
        LiteralString,
        Iden,
        Func,
        BinaryExpression,
    }


    public enum CssValueEvaluatedAs : byte
    {
        UnEvaluate,
        Unknown,
        BorderLength,
        Length,
        TranslatedLength,
        Color,
        TranslatedString,
        BorderStyle,
        BorderCollapse,
        WhiteSpace,
        Visibility,
        VerticalAlign,
        TextAlign,
        Overflow,
        TextDecoration,
        WordBreak,
        Position,
        Direction,
        Display,
        Float,
        EmptyCell,
        FontWeight,
        FontStyle,
        FontVariant,
        ListStylePosition,
        ListStyleType,
        BackgroundRepeat,
        BoxSizing,
    }
    public enum CssValueOpName
    {
        Unknown,
        Divide,
    }
}