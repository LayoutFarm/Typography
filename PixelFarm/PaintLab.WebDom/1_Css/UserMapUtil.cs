//BSD, 2014-present, WinterDev

using System;
using System.Collections.Generic;

using LayoutFarm.Css;
using PixelFarm.Drawing;

namespace LayoutFarm.WebDom
{


    public static class UserMapUtil
    {
        static readonly ValueMap<CssDisplay> _cssDisplayMap = new ValueMap<CssDisplay>();
        static readonly ValueMap<CssDirection> _cssDirectionMap = new ValueMap<CssDirection>();
        static readonly ValueMap<CssPosition> _cssPositionMap = new ValueMap<CssPosition>();
        static readonly ValueMap<CssWordBreak> _cssWordBreakMap = new ValueMap<CssWordBreak>();
        static readonly ValueMap<CssTextDecoration> _cssTextDecorationMap = new ValueMap<CssTextDecoration>();
        static readonly ValueMap<CssOverflow> _cssOverFlowMap = new ValueMap<CssOverflow>();
        static readonly ValueMap<CssTextAlign> _cssTextAlignMap = new ValueMap<CssTextAlign>();
        static readonly ValueMap<CssVerticalAlign> _cssVerticalAlignMap = new ValueMap<CssVerticalAlign>();
        static readonly ValueMap<CssVisibility> _cssVisibilityMap = new ValueMap<CssVisibility>();
        static readonly ValueMap<CssWhiteSpace> _cssWhitespaceMap = new ValueMap<CssWhiteSpace>();
        static readonly ValueMap<CssBorderCollapse> _cssCollapseBorderMap = new ValueMap<CssBorderCollapse>();
        static readonly ValueMap<CssBorderStyle> _cssBorderStyleMap = new ValueMap<CssBorderStyle>();
        static readonly ValueMap<CssEmptyCell> _cssEmptyCellMap = new ValueMap<CssEmptyCell>();
        static readonly ValueMap<CssFloat> _cssFloatMap = new ValueMap<CssFloat>();
        static readonly ValueMap<LayoutFarm.Css.CssFontStyle> _cssFontStyleMap = new ValueMap<LayoutFarm.Css.CssFontStyle>();
        static readonly ValueMap<CssFontVariant> _cssFontVariantMap = new ValueMap<CssFontVariant>();
        static readonly ValueMap<CssFontWeight> _cssFontWeightMap = new ValueMap<CssFontWeight>();
        static readonly ValueMap<CssListStylePosition> _cssListStylePositionMap = new ValueMap<CssListStylePosition>();
        static readonly ValueMap<CssListStyleType> _cssListStyleTypeMap = new ValueMap<CssListStyleType>();
        static readonly ValueMap<CssNamedBorderWidth> _cssNamedBorderWidthMap = new ValueMap<CssNamedBorderWidth>();
        static readonly ValueMap<CssBackgroundRepeat> _cssBackgroundRepeatMap = new ValueMap<CssBackgroundRepeat>();
        static readonly ValueMap<LayoutFarm.WebDom.WellknownCssPropertyName> _wellKnownCssPropNameMap = new ValueMap<WebDom.WellknownCssPropertyName>();

        static readonly ValueMap<CssBoxSizing> _cssBoxSizingMap = new ValueMap<CssBoxSizing>();
        static UserMapUtil()
        {
        }

        //-----------------------
        public static bool IsBorderStyle(string value)
        {
            return _cssBorderStyleMap.GetValueFromString(value, CssBorderStyle.Unknown) != CssBorderStyle.Unknown;
        }
        //thin,medium,thick border width
        public static bool IsNamedBorderWidth(string value)
        {
            return _cssNamedBorderWidthMap.GetValueFromString(value, CssNamedBorderWidth.Unknown) != CssNamedBorderWidth.Unknown;
        }
        public static bool IsFontVariant(string value)
        {
            return _cssFontVariantMap.GetValueFromString(value, CssFontVariant.Unknown) != CssFontVariant.Unknown;
        }
        public static bool IsFontStyle(string value)
        {
            return _cssFontStyleMap.GetValueFromString(value, LayoutFarm.Css.CssFontStyle.Unknown) != LayoutFarm.Css.CssFontStyle.Unknown;
        }
        public static bool IsFontWeight(string value)
        {
            return _cssFontWeightMap.GetValueFromString(value, CssFontWeight.Unknown) != CssFontWeight.Unknown;
        }

        //-----------------------

        public static CssBorderCollapse GetBorderCollapse(WebDom.CssCodeValueExpression value)
        {
            return (CssBorderCollapse)EvaluateIntPropertyValueFromString(
               _cssCollapseBorderMap,
               WebDom.CssValueEvaluatedAs.BorderCollapse,
               CssBorderCollapse.Separate,
               value);
        }

        public static string ToCssStringValue(this CssDisplay value)
        {
            return _cssDisplayMap.GetStringFromValue(value);
        }
        //-----------------------
        static int EvaluateIntPropertyValueFromString<T>(ValueMap<T> map,
            WebDom.CssValueEvaluatedAs
            evalAs,
            T defaultValue,
            WebDom.CssCodeValueExpression value)
            where T : struct
        {
            if (value.EvaluatedAs != evalAs)
            {
                T knownValue = map.GetValueFromString(value.GetTranslatedStringValue(), defaultValue);
                int result = Convert.ToInt32(knownValue);
                value.SetIntValue(result, evalAs);
                return (int)result;
            }
            else
            {
                return value.GetCacheIntValue();
            }
        }
        //-----------------------
        public static CssDisplay GetDisplayType(WebDom.CssCodeValueExpression value)
        {
            return (CssDisplay)EvaluateIntPropertyValueFromString(
                _cssDisplayMap,
                WebDom.CssValueEvaluatedAs.Display,
                CssDisplay.Inline,
                value);
        }
        public static CssBackgroundRepeat GetBackgroundRepeat(WebDom.CssCodeValueExpression value)
        {
            return (CssBackgroundRepeat)EvaluateIntPropertyValueFromString(
                _cssBackgroundRepeatMap,
                WebDom.CssValueEvaluatedAs.BackgroundRepeat,
                CssBackgroundRepeat.Repeat,
                value);
        }
        //----------------------
        public static string ToCssStringValue(this CssDirection value)
        {
            return _cssDirectionMap.GetStringFromValue(value);
        }
        public static CssDirection GetCssDirection(WebDom.CssCodeValueExpression value)
        {
            return (CssDirection)EvaluateIntPropertyValueFromString(
             _cssDirectionMap,
             WebDom.CssValueEvaluatedAs.Direction,
             CssDirection.Ltl,
             value);
        }
        //----------------------
        public static CssPosition GetCssPosition(WebDom.CssCodeValueExpression value)
        {
            return (CssPosition)EvaluateIntPropertyValueFromString(
             _cssPositionMap,
             WebDom.CssValueEvaluatedAs.Position,
             CssPosition.Static,
             value);
        }
        public static CssWordBreak GetWordBreak(WebDom.CssCodeValueExpression value)
        {
            return (CssWordBreak)EvaluateIntPropertyValueFromString(
             _cssWordBreakMap,
             WebDom.CssValueEvaluatedAs.WordBreak,
             CssWordBreak.Normal,
             value);
        }
        public static CssTextDecoration GetTextDecoration(WebDom.CssCodeValueExpression value)
        {
            return (CssTextDecoration)EvaluateIntPropertyValueFromString(
                _cssTextDecorationMap,
                WebDom.CssValueEvaluatedAs.TextDecoration,
                CssTextDecoration.NotAssign,
                value);
        }
        public static CssOverflow GetOverflow(WebDom.CssCodeValueExpression value)
        {
            return (CssOverflow)EvaluateIntPropertyValueFromString(
               _cssOverFlowMap,
               WebDom.CssValueEvaluatedAs.Overflow,
               CssOverflow.Visible,
               value);
        }
        public static CssTextAlign GetTextAlign(WebDom.CssCodeValueExpression value)
        {
            return (CssTextAlign)EvaluateIntPropertyValueFromString(
                _cssTextAlignMap,
                WebDom.CssValueEvaluatedAs.TextAlign,
                CssTextAlign.NotAssign,
                value);
        }
        public static CssBoxSizing GetBoxSizing(WebDom.CssCodeValueExpression value)
        {
            return (CssBoxSizing)EvaluateIntPropertyValueFromString(
                _cssBoxSizingMap,
                WebDom.CssValueEvaluatedAs.BoxSizing,
                CssBoxSizing.ContentBox,//default
                value);
        }
        public static CssVerticalAlign GetVerticalAlign(WebDom.CssCodeValueExpression value)
        {
            return (CssVerticalAlign)EvaluateIntPropertyValueFromString(
            _cssVerticalAlignMap,
            WebDom.CssValueEvaluatedAs.VerticalAlign,
            CssVerticalAlign.Baseline,
            value);
        }


        public static CssVisibility GetVisibility(WebDom.CssCodeValueExpression value)
        {
            return (CssVisibility)EvaluateIntPropertyValueFromString(
            _cssVisibilityMap,
            WebDom.CssValueEvaluatedAs.Visibility,
            CssVisibility.Visible,
            value);
        }
        public static CssWhiteSpace GetWhitespace(WebDom.CssCodeValueExpression value)
        {
            return (CssWhiteSpace)EvaluateIntPropertyValueFromString(
            _cssWhitespaceMap,
            WebDom.CssValueEvaluatedAs.WhiteSpace,
            CssWhiteSpace.Normal,
            value);
        }
        public static CssBorderStyle GetBorderStyle(WebDom.CssCodeValueExpression value)
        {
            return (CssBorderStyle)EvaluateIntPropertyValueFromString(
                   _cssBorderStyleMap,
                   WebDom.CssValueEvaluatedAs.BorderStyle,
                   CssBorderStyle.None,
                   value);
        }

        public static CssUnitOrNames GetCssUnit(string u)
        {
            switch (u)
            {
                case CssConstants.Em:

                    return CssUnitOrNames.Ems;
                case CssConstants.Ex:
                    return CssUnitOrNames.Ex;
                case CssConstants.Px:
                    return CssUnitOrNames.Pixels;
                case CssConstants.Mm:
                    return CssUnitOrNames.Milimeters;
                case CssConstants.Cm:
                    return CssUnitOrNames.Centimeters;
                case CssConstants.In:
                    return CssUnitOrNames.Inches;
                case CssConstants.Pt:
                    return CssUnitOrNames.Points;
                case CssConstants.Pc:
                    return CssUnitOrNames.Picas;
                case "%":
                    return CssUnitOrNames.Percent;
                default:
                    return CssUnitOrNames.Unknown;
            }
        }

        public static CssEmptyCell GetEmptyCell(WebDom.CssCodeValueExpression value)
        {
            return (CssEmptyCell)EvaluateIntPropertyValueFromString(
               _cssEmptyCellMap,
               WebDom.CssValueEvaluatedAs.EmptyCell,
               CssEmptyCell.Show, value);
        }

        public static string ToCssStringValue(this CssEmptyCell value)
        {
            return _cssEmptyCellMap.GetStringFromValue(value);
        }
        public static string ToCssStringValue(this CssTextAlign value)
        {
            return _cssTextAlignMap.GetStringFromValue(value);
        }
        public static string ToCssStringValue(this CssTextDecoration value)
        {
            return _cssTextDecorationMap.GetStringFromValue(value);
        }
        public static string ToCssStringValue(this CssWordBreak value)
        {
            return _cssWordBreakMap.GetStringFromValue(value);
        }
        public static string ToCssStringValue(this CssWhiteSpace value)
        {
            return _cssWhitespaceMap.GetStringFromValue(value);
        }
        public static string ToCssStringValue(this CssVisibility value)
        {
            return _cssVisibilityMap.GetStringFromValue(value);
        }
        public static string ToCssStringValue(this CssVerticalAlign value)
        {
            return _cssVerticalAlignMap.GetStringFromValue(value);
        }
        public static string ToCssStringValue(this CssPosition value)
        {
            return _cssPositionMap.GetStringFromValue(value);
        }
        //public static CssLength SetLineHeight(this CssBox box, CssLength len)
        //{
        //    //2014,2015,
        //    //from www.w3c.org/wiki/Css/Properties/line-height

        //    //line height in <percentage> : 
        //    //The computed value if the property is percentage multiplied by the 
        //    //element's computed font size. 
        //    return CssLength.MakePixelLength(
        //         CssValueParser.ConvertToPx(len, box.GetEmHeight(), box));
        //}

        public static LayoutFarm.WebDom.WellknownCssPropertyName GetWellKnownPropName(string propertyName)
        {
            return _wellKnownCssPropNameMap.GetValueFromString(propertyName, WebDom.WellknownCssPropertyName.Unknown);
        }
        public static CssFloat GetFloat(WebDom.CssCodeValueExpression value)
        {
            return (CssFloat)EvaluateIntPropertyValueFromString(
                _cssFloatMap,
                WebDom.CssValueEvaluatedAs.Float,
                CssFloat.None,
                value);
        }

        public static string ToCssStringValue(this CssFloat cssFloat)
        {
            return _cssFloatMap.GetStringFromValue(cssFloat);
        }
        public static string ToCssStringValue(this CssBorderStyle borderStyle)
        {
            return _cssBorderStyleMap.GetStringFromValue(borderStyle);
        }
        public static string ToCssStringValue(this CssBorderCollapse borderCollapse)
        {
            return _cssCollapseBorderMap.GetStringFromValue(borderCollapse);
        }
        public static string ToCssStringValue(this CssBackgroundRepeat backgrounRepeat)
        {
            return _cssBackgroundRepeatMap.GetStringFromValue(backgrounRepeat);
        }
        public static string ToHexColor(this Color color)
        {
            return string.Concat("#", color.R.ToString("X"), color.G.ToString("X"), color.B.ToString("X"));
        }
        public static string ToCssStringValue(this LayoutFarm.Css.CssFontStyle fontstyle)
        {
            return _cssFontStyleMap.GetStringFromValue(fontstyle);
        }
        public static LayoutFarm.Css.CssFontStyle GetFontStyle(WebDom.CssCodeValueExpression value)
        {
            return (LayoutFarm.Css.CssFontStyle)EvaluateIntPropertyValueFromString(
              _cssFontStyleMap,
              WebDom.CssValueEvaluatedAs.FontStyle,
              LayoutFarm.Css.CssFontStyle.Normal,
              value);
        }
        public static string ToCssStringValue(this CssFontVariant fontVariant)
        {
            return _cssFontVariantMap.GetStringFromValue(fontVariant);
        }
        public static CssFontVariant GetFontVariant(WebDom.CssCodeValueExpression value)
        {
            return (CssFontVariant)EvaluateIntPropertyValueFromString(
              _cssFontVariantMap,
              WebDom.CssValueEvaluatedAs.FontVariant,
              CssFontVariant.Normal,
              value);
        }
        public static string ToFontSizeString(this CssLength length)
        {
            if (length.IsFontSizeName)
            {
                switch (length.UnitOrNames)
                {
                    case CssUnitOrNames.FONTSIZE_MEDIUM:
                        return CssConstants.Medium;
                    case CssUnitOrNames.FONTSIZE_SMALL:
                        return CssConstants.Small;
                    case CssUnitOrNames.FONTSIZE_X_SMALL:
                        return CssConstants.XSmall;
                    case CssUnitOrNames.FONTSIZE_XX_SMALL:
                        return CssConstants.XXSmall;
                    case CssUnitOrNames.FONTSIZE_LARGE:
                        return CssConstants.Large;
                    case CssUnitOrNames.FONTSIZE_X_LARGE:
                        return CssConstants.XLarge;
                    case CssUnitOrNames.FONTSIZE_XX_LARGE:
                        return CssConstants.XXLarge;
                    case CssUnitOrNames.FONTSIZE_LARGER:
                        return CssConstants.Larger;
                    case CssUnitOrNames.FONTSIZE_SMALLER:
                        return CssConstants.Smaller;
                    default:
                        return "";
                }
            }
            else
            {
                return length.ToString();
            }
        }


        public static LayoutFarm.Css.CssLength AsBorderLength(this WebDom.CssCodeValueExpression value)
        {
            if (value.EvaluatedAs != WebDom.CssValueEvaluatedAs.BorderLength)
            {
                switch (value.Hint)
                {
                    case WebDom.CssValueHint.Number:
                        {
                            if (value is WebDom.CssCodePrimitiveExpression)
                            {
                                WebDom.CssCodePrimitiveExpression prim = (WebDom.CssCodePrimitiveExpression)value;
                                CssLength len = new CssLength(value.AsNumber(), GetCssUnit(prim.Unit));
                                value.SetCssLength(len, WebDom.CssValueEvaluatedAs.BorderLength);
                                return len;
                            }
                            else
                            {
                                CssLength len = CssLength.MakePixelLength(value.AsNumber());
                                value.SetCssLength(len, WebDom.CssValueEvaluatedAs.BorderLength);
                                return len;
                            }
                        }
                    default:
                        {
                            CssLength len = MakeBorderLength(value.ToString());
                            value.SetCssLength(len, WebDom.CssValueEvaluatedAs.BorderLength);
                            return len;
                        }
                }
            }
            return value.GetCacheCssLength();
        }


        public static CssLength AsLength(this WebDom.CssCodeValueExpression value)
        {
            if (value.EvaluatedAs != WebDom.CssValueEvaluatedAs.Length)
            {
                //length from number 
                switch (value.Hint)
                {
                    case WebDom.CssValueHint.Number:
                        {
                            if (value is WebDom.CssCodePrimitiveExpression)
                            {
                                WebDom.CssCodePrimitiveExpression prim = (WebDom.CssCodePrimitiveExpression)value;
                                CssLength len = new CssLength(value.AsNumber(), GetCssUnit(prim.Unit));
                                value.SetCssLength(len, WebDom.CssValueEvaluatedAs.Length);
                                return len;
                            }
                            else
                            {
                                CssLength len = CssLength.MakePixelLength(value.AsNumber());
                                value.SetCssLength(len, WebDom.CssValueEvaluatedAs.Length);
                                return len;
                            }
                        }
                    default:
                        {
                            CssLength len = TranslateLength(value.ToString());
                            value.SetCssLength(len, WebDom.CssValueEvaluatedAs.Length);
                            return len;
                        }
                }
            }
            return value.GetCacheCssLength();
        }



        public static CssLength AsTranslatedLength(this WebDom.CssCodeValueExpression value)
        {
            if (value.EvaluatedAs != WebDom.CssValueEvaluatedAs.TranslatedLength)
            {
                switch (value.Hint)
                {
                    case WebDom.CssValueHint.Number:
                        {
                            if (value is WebDom.CssCodePrimitiveExpression)
                            {
                                WebDom.CssCodePrimitiveExpression prim = (WebDom.CssCodePrimitiveExpression)value;
                                CssLength len = new CssLength(value.AsNumber(), GetCssUnit(prim.Unit));
                                if (len.HasError)
                                {
                                    len = CssLength.MakePixelLength(0);
                                }
                                value.SetCssLength(len, WebDom.CssValueEvaluatedAs.TranslatedLength);
                                return len;
                            }
                            else
                            {
                                CssLength len = CssLength.MakePixelLength(value.AsNumber());
                                value.SetCssLength(len, WebDom.CssValueEvaluatedAs.TranslatedLength);
                                return len;
                            }
                        }
                    default:
                        {
                            CssLength len = TranslateLength(value.ToString());
                            value.SetCssLength(len, WebDom.CssValueEvaluatedAs.TranslatedLength);
                            return len;
                        }
                }
            }
            return value.GetCacheCssLength();
        }
        public static Color AsColor(WebDom.CssCodeValueExpression value)
        {
            if (value.EvaluatedAs != WebDom.CssValueEvaluatedAs.Color)
            {
                if (value is WebDom.CssCodeColor)
                {
                    return ((WebDom.CssCodeColor)value).ActualColor;
                }
                else
                {
                    Color actualColor = LayoutFarm.WebDom.Parser.CssValueParser.ParseCssColor(value.GetTranslatedStringValue());
                    value.SetColorValue(actualColor);
                    return actualColor;
                }
            }
            return value.GetCacheColor();
        }


        public static CssFontWeight GetFontWeight(WebDom.CssCodeValueExpression value)
        {
            return (CssFontWeight)EvaluateIntPropertyValueFromString(
                _cssFontWeightMap,
                WebDom.CssValueEvaluatedAs.Float,
                CssFontWeight.NotAssign,
                value);
        }

        public static string ToCssStringValue(this CssFontWeight weight)
        {
            return _cssFontWeightMap.GetStringFromValue(weight);
        }

        public static string ToCssStringValue(this CssListStylePosition listStylePosition)
        {
            return _cssListStylePositionMap.GetStringFromValue(listStylePosition);
        }
        public static CssListStylePosition GetListStylePosition(WebDom.CssCodeValueExpression value)
        {
            return (CssListStylePosition)EvaluateIntPropertyValueFromString(
               _cssListStylePositionMap,
               WebDom.CssValueEvaluatedAs.ListStylePosition,
               CssListStylePosition.Outside,
               value);
        }

        public static string ToCssStringValue(this CssListStyleType listStyleType)
        {
            return _cssListStyleTypeMap.GetStringFromValue(listStyleType);
        }
        public static CssListStyleType GetListStyleType(WebDom.CssCodeValueExpression value)
        {
            return (CssListStyleType)EvaluateIntPropertyValueFromString(
            _cssListStyleTypeMap,
            WebDom.CssValueEvaluatedAs.ListStyleType,
            CssListStyleType.Disc,
            value);
        }




        /// <summary>
        /// Converts an HTML length into a Css length
        /// </summary>
        /// <param name="htmlLength"></param>
        /// <returns></returns>
        public static CssLength TranslateLength(string htmlLength)
        {
            CssLength len = ParseGenericLength(htmlLength);
            if (len.HasError)
            {
                return CssLength.MakePixelLength(0);
            }
            return len;
        }
        internal static CssLength MakeBorderLength(string str)
        {
            switch (str)
            {
                case CssConstants.Medium:
                    return CssLength.Medium;
                case CssConstants.Thick:
                    return CssLength.Thick;
                case CssConstants.Thin:
                    return CssLength.Thin;
            }
            return ParseGenericLength(str);
        }

        public static CssLength ParseGenericLength(string lenValue)
        {
            float parsedNumber = 0f;
            switch (lenValue)
            {
                case null:
                case "":
                case "0":
                    //Return zero if no length specified, zero specified
                    return CssLength.ZeroNoUnit;
                case "auto":
                    return CssLength.AutoLength;
                case "normal":
                    return CssLength.NormalWordOrLine;
            }

            //then parse
            //If percentage, use ParseNumber
            if (lenValue.EndsWith("%"))
            {
                parsedNumber = float.Parse(lenValue.Substring(0, lenValue.Length - 1), System.Globalization.CultureInfo.InvariantCulture);
                return new CssLength(parsedNumber, CssUnitOrNames.Percent);
            }
            //If no units, has error
            if (lenValue.Length < 3)
            {
                if (float.TryParse(lenValue, out parsedNumber))
                {
                    return new CssLength(parsedNumber, CssUnitOrNames.Pixels);
                }
                else
                {
                    return new CssLength(parsedNumber, CssUnitOrNames.Unknown);
                }
            }
            //Get units of the length
            //TODO: Units behave different in paper and in screen! 
            CssUnitOrNames unit = GetCssUnit(lenValue.Substring(lenValue.Length - 2, 2));
            //parse number part
            if (unit == CssUnitOrNames.Unknown)
            {
                //check if all char is number?

                if (!float.TryParse(lenValue,
                    System.Globalization.NumberStyles.Number,
                    System.Globalization.NumberFormatInfo.InvariantInfo, out parsedNumber))
                {
                    //make an error value
                    return new CssLength(0, CssUnitOrNames.Unknown);
                }
                else
                {
                    return new CssLength(parsedNumber, CssUnitOrNames.Pixels);
                }
            }
            else
            {
                string number_part = lenValue.Substring(0, lenValue.Length - 2);
                if (!float.TryParse(number_part,
                    System.Globalization.NumberStyles.Number,
                    System.Globalization.NumberFormatInfo.InvariantInfo, out parsedNumber))
                {
                    //make an error value
                    return new CssLength(0, CssUnitOrNames.Unknown);
                }
                else
                {
                    return new CssLength(parsedNumber, unit);
                }
            }
        }
    }



    //
    // System.Drawing.KnownColors
    //
    // Authors:
    // Gonzalo Paniagua Javier (gonzalo@ximian.com)
    // Peter Dennis Bartok (pbartok@novell.com)
    // Sebastien Pouliot <sebastien@ximian.com>
    //
    // Copyright (C) 2007 Novell, Inc (http://www.novell.com)
    //
    // Permission is hereby granted, free of charge, to any person obtaining
    // a copy of this software and associated documentation files (the
    // "Software"), to deal in the Software without restriction, including
    // without limitation the rights to use, copy, modify, merge, publish,
    // distribute, sublicense, and/or sell copies of the Software, and to
    // permit persons to whom the Software is furnished to do so, subject to
    // the following conditions:
    //
    // The above copyright notice and this permission notice shall be
    // included in all copies or substantial portions of the Software.
    //
    // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    // EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    // MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    // NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
    // LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
    // OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
    // WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    //


    enum KnownColor
    {
        Empty,  /* 000 - Empty */
        ActiveBorder,   /* 001 - ActiveBorder */
        ActiveCaption,  /* 002 - ActiveCaption */
        ActiveCaptionText,  /* 003 - ActiveCaptionText */
        AppWorkspace,   /* 004 - AppWorkspace */
        Control,    /* 005 - Control */
        ControlDark,    /* 006 - ControlDark */
        ControlDarkDark,    /* 007 - ControlDarkDark */
        ControlLight,   /* 008 - ControlLight */
        ControlLightLight,  /* 009 - ControlLightLight */
        ControlText,    /* 010 - ControlText */
        Desktop,    /* 011 - Desktop */
        GrayText,   /* 012 - GrayText */
        Highlight,  /* 013 - Highlight */
        HighlightText,  /* 014 - HighlightText */
        HotTrack,   /* 015 - HotTrack */
        InactiveBorder, /* 016 - InactiveBorder */
        InactiveCaption,    /* 017 - InactiveCaption */
        InactiveCaptionText,    /* 018 - InactiveCaptionText */
        Info,   /* 019 - Info */
        InfoText,   /* 020 - InfoText */
        Menu,   /* 021 - Menu */
        MenuText,   /* 022 - MenuText */
        ScrollBar,  /* 023 - ScrollBar */
        Window, /* 024 - Window */
        WindowFrame,    /* 025 - WindowFrame */
        WindowText, /* 026 - WindowText */
        Transparent,    /* 027 - Transparent */
        AliceBlue,  /* 028 - AliceBlue */
        AntiqueWhite,   /* 029 - AntiqueWhite */
        Aqua,   /* 030 - Aqua */
        Aquamarine, /* 031 - Aquamarine */
        Azure,  /* 032 - Azure */
        Beige,  /* 033 - Beige */
        Bisque, /* 034 - Bisque */
        Black,  /* 035 - Black */
        BlanchedAlmond, /* 036 - BlanchedAlmond */
        Blue,   /* 037 - Blue */
        BlueViolet, /* 038 - BlueViolet */
        Brown,  /* 039 - Brown */
        BurlyWood,  /* 040 - BurlyWood */
        CadetBlue,  /* 041 - CadetBlue */
        Chartreuse, /* 042 - Chartreuse */
        Chocolate,  /* 043 - Chocolate */
        Coral,  /* 044 - Coral */
        CornflowerBlue, /* 045 - CornflowerBlue */
        Cornsilk,   /* 046 - Cornsilk */
        Crimson,    /* 047 - Crimson */
        Cyan,   /* 048 - Cyan */
        DarkBlue,   /* 049 - DarkBlue */
        DarkCyan,   /* 050 - DarkCyan */
        DarkGoldenrod,  /* 051 - DarkGoldenrod */
        DarkGray,   /* 052 - DarkGray */
        DarkGreen,  /* 053 - DarkGreen */
        DarkKhaki,  /* 054 - DarkKhaki */
        DarkMagenta,    /* 055 - DarkMagenta */
        DarkOliveGreen, /* 056 - DarkOliveGreen */
        DarkOrange, /* 057 - DarkOrange */
        DarkOrchid, /* 058 - DarkOrchid */
        DarkRed,    /* 059 - DarkRed */
        DarkSalmon, /* 060 - DarkSalmon */
        DarkSeaGreen,   /* 061 - DarkSeaGreen */
        DarkSlateBlue,  /* 062 - DarkSlateBlue */
        DarkSlateGray,  /* 063 - DarkSlateGray */
        DarkTurquoise,  /* 064 - DarkTurquoise */
        DarkViolet, /* 065 - DarkViolet */
        DeepPink,   /* 066 - DeepPink */
        DeepSkyBlue,    /* 067 - DeepSkyBlue */
        DimGray,    /* 068 - DimGray */
        DodgerBlue, /* 069 - DodgerBlue */
        Firebrick,  /* 070 - Firebrick */
        FloralWhite,    /* 071 - FloralWhite */
        ForestGreen,    /* 072 - ForestGreen */
        Fuchsia,    /* 073 - Fuchsia */
        Gainsboro,  /* 074 - Gainsboro */
        GhostWhite, /* 075 - GhostWhite */
        Gold,   /* 076 - Gold */
        Goldenrod,  /* 077 - Goldenrod */
        Gray,   /* 078 - Gray */
        Green,  /* 079 - Green */
        GreenYellow,    /* 080 - GreenYellow */
        Honeydew,   /* 081 - Honeydew */
        HotPink,    /* 082 - HotPink */
        IndianRed,  /* 083 - IndianRed */
        Indigo, /* 084 - Indigo */
        Ivory,  /* 085 - Ivory */
        Khaki,  /* 086 - Khaki */
        Lavender,   /* 087 - Lavender */
        LavenderBlush,  /* 088 - LavenderBlush */
        LawnGreen,  /* 089 - LawnGreen */
        LemonChiffon,   /* 090 - LemonChiffon */
        LightBlue,  /* 091 - LightBlue */
        LightCoral, /* 092 - LightCoral */
        LightCyan,  /* 093 - LightCyan */
        LightGoldenrodYellow,   /* 094 - LightGoldenrodYellow */
        LightGray,  /* 095 - LightGray */
        LightGreen, /* 096 - LightGreen */
        LightPink,  /* 097 - LightPink */
        LightSalmon,    /* 098 - LightSalmon */
        LightSeaGreen,  /* 099 - LightSeaGreen */
        LightSkyBlue,   /* 100 - LightSkyBlue */
        LightSlateGray, /* 101 - LightSlateGray */
        LightSteelBlue, /* 102 - LightSteelBlue */
        LightYellow,    /* 103 - LightYellow */
        Lime,   /* 104 - Lime */
        LimeGreen,  /* 105 - LimeGreen */
        Linen,  /* 106 - Linen */
        Magenta,    /* 107 - Magenta */
        Maroon, /* 108 - Maroon */
        MediumAquamarine,   /* 109 - MediumAquamarine */
        MediumBlue, /* 110 - MediumBlue */
        MediumOrchid,   /* 111 - MediumOrchid */
        MediumPurple,   /* 112 - MediumPurple */
        MediumSeaGreen, /* 113 - MediumSeaGreen */
        MediumSlateBlue,    /* 114 - MediumSlateBlue */
        MediumSpringGreen,  /* 115 - MediumSpringGreen */
        MediumTurquoise,    /* 116 - MediumTurquoise */
        MediumVioletRed,    /* 117 - MediumVioletRed */
        MidnightBlue,   /* 118 - MidnightBlue */
        MintCream,  /* 119 - MintCream */
        MistyRose,  /* 120 - MistyRose */
        Moccasin,   /* 121 - Moccasin */
        NavajoWhite,    /* 122 - NavajoWhite */
        Navy,   /* 123 - Navy */
        OldLace,    /* 124 - OldLace */
        Olive,  /* 125 - Olive */
        OliveDrab,  /* 126 - OliveDrab */
        Orange, /* 127 - Orange */
        OrangeRed,  /* 128 - OrangeRed */
        Orchid, /* 129 - Orchid */
        PaleGoldenrod,  /* 130 - PaleGoldenrod */
        PaleGreen,  /* 131 - PaleGreen */
        PaleTurquoise,  /* 132 - PaleTurquoise */
        PaleVioletRed,  /* 133 - PaleVioletRed */
        PapayaWhip, /* 134 - PapayaWhip */
        PeachPuff,  /* 135 - PeachPuff */
        Peru,   /* 136 - Peru */
        Pink,   /* 137 - Pink */
        Plum,   /* 138 - Plum */
        PowderBlue, /* 139 - PowderBlue */
        Purple, /* 140 - Purple */
        Red,    /* 141 - Red */
        RosyBrown,  /* 142 - RosyBrown */
        RoyalBlue,  /* 143 - RoyalBlue */
        SaddleBrown,    /* 144 - SaddleBrown */
        Salmon, /* 145 - Salmon */
        SandyBrown, /* 146 - SandyBrown */
        SeaGreen,   /* 147 - SeaGreen */
        SeaShell,   /* 148 - SeaShell */
        Sienna, /* 149 - Sienna */
        Silver, /* 150 - Silver */
        SkyBlue,    /* 151 - SkyBlue */
        SlateBlue,  /* 152 - SlateBlue */
        SlateGray,  /* 153 - SlateGray */
        Snow,   /* 154 - Snow */
        SpringGreen,    /* 155 - SpringGreen */
        SteelBlue,  /* 156 - SteelBlue */
        Tan,    /* 157 - Tan */
        Teal,   /* 158 - Teal */
        Thistle,    /* 159 - Thistle */
        Tomato, /* 160 - Tomato */
        Turquoise,  /* 161 - Turquoise */
        Violet, /* 162 - Violet */
        Wheat,  /* 163 - Wheat */
        White,  /* 164 - White */
        WhiteSmoke, /* 165 - WhiteSmoke */
        Yellow, /* 166 - Yellow */
        YellowGreen,    /* 167 - YellowGreen */
        ButtonFace, /* 168 - ButtonFace */
        ButtonHighlight,    /* 169 - ButtonHighlight */
        ButtonShadow,   /* 170 - ButtonShadow */
        GradientActiveCaption,  /* 171 - GradientActiveCaption */
        GradientInactiveCaption,    /* 172 - GradientInactiveCaption */
        MenuBar,    /* 173 - MenuBar */
        MenuHighlight,  /* 174 - MenuHighlight */
    }
    static class KnownColors
    {
        // FindColorMatch relies on the index + 1 == KnowColor match
        static internal uint[] argbValues = new uint[] {
            0x00000000,	/* 000 - Empty */
            0xFFD4D0C8,	/* 001 - ActiveBorder */
            0xFF0054E3,	/* 002 - ActiveCaption */
            0xFFFFFFFF,	/* 003 - ActiveCaptionText */
            0xFF808080,	/* 004 - AppWorkspace */
            0xFFECE9D8,	/* 005 - Control */
            0xFFACA899,	/* 006 - ControlDark */
            0xFF716F64,	/* 007 - ControlDarkDark */
            0xFFF1EFE2,	/* 008 - ControlLight */
            0xFFFFFFFF,	/* 009 - ControlLightLight */
            0xFF000000,	/* 010 - ControlText */
            0xFF004E98,	/* 011 - Desktop */
            0xFFACA899,	/* 012 - GrayText */
            0xFF316AC5,	/* 013 - Highlight */
            0xFFFFFFFF,	/* 014 - HighlightText */
            0xFF000080,	/* 015 - HotTrack */
            0xFFD4D0C8,	/* 016 - InactiveBorder */
            0xFF7A96DF,	/* 017 - InactiveCaption */
            0xFFD8E4F8,	/* 018 - InactiveCaptionText */
            0xFFFFFFE1,	/* 019 - Info */
            0xFF000000,	/* 020 - InfoText */
            0xFFFFFFFF,	/* 021 - Menu */
            0xFF000000,	/* 022 - MenuText */
            0xFFD4D0C8,	/* 023 - ScrollBar */
            0xFFFFFFFF,	/* 024 - Window */
            0xFF000000,	/* 025 - WindowFrame */
            0xFF000000,	/* 026 - WindowText */
            0x00FFFFFF,	/* 027 - Transparent */
            0xFFF0F8FF,	/* 028 - AliceBlue */
            0xFFFAEBD7,	/* 029 - AntiqueWhite */
            0xFF00FFFF,	/* 030 - Aqua */
            0xFF7FFFD4,	/* 031 - Aquamarine */
            0xFFF0FFFF,	/* 032 - Azure */
            0xFFF5F5DC,	/* 033 - Beige */
            0xFFFFE4C4,	/* 034 - Bisque */
            0xFF000000,	/* 035 - Black */
            0xFFFFEBCD,	/* 036 - BlanchedAlmond */
            0xFF0000FF,	/* 037 - Blue */
            0xFF8A2BE2,	/* 038 - BlueViolet */
            0xFFA52A2A,	/* 039 - Brown */
            0xFFDEB887,	/* 040 - BurlyWood */
            0xFF5F9EA0,	/* 041 - CadetBlue */
            0xFF7FFF00,	/* 042 - Chartreuse */
            0xFFD2691E,	/* 043 - Chocolate */
            0xFFFF7F50,	/* 044 - Coral */
            0xFF6495ED,	/* 045 - CornflowerBlue */
            0xFFFFF8DC,	/* 046 - Cornsilk */
            0xFFDC143C,	/* 047 - Crimson */
            0xFF00FFFF,	/* 048 - Cyan */
            0xFF00008B,	/* 049 - DarkBlue */
            0xFF008B8B,	/* 050 - DarkCyan */
            0xFFB8860B,	/* 051 - DarkGoldenrod */
            0xFFA9A9A9,	/* 052 - DarkGray */
            0xFF006400,	/* 053 - DarkGreen */
            0xFFBDB76B,	/* 054 - DarkKhaki */
            0xFF8B008B,	/* 055 - DarkMagenta */
            0xFF556B2F,	/* 056 - DarkOliveGreen */
            0xFFFF8C00,	/* 057 - DarkOrange */
            0xFF9932CC,	/* 058 - DarkOrchid */
            0xFF8B0000,	/* 059 - DarkRed */
            0xFFE9967A,	/* 060 - DarkSalmon */
            0xFF8FBC8B,	/* 061 - DarkSeaGreen */
            0xFF483D8B,	/* 062 - DarkSlateBlue */
            0xFF2F4F4F,	/* 063 - DarkSlateGray */
            0xFF00CED1,	/* 064 - DarkTurquoise */
            0xFF9400D3,	/* 065 - DarkViolet */
            0xFFFF1493,	/* 066 - DeepPink */
            0xFF00BFFF,	/* 067 - DeepSkyBlue */
            0xFF696969,	/* 068 - DimGray */
            0xFF1E90FF,	/* 069 - DodgerBlue */
            0xFFB22222,	/* 070 - Firebrick */
            0xFFFFFAF0,	/* 071 - FloralWhite */
            0xFF228B22,	/* 072 - ForestGreen */
            0xFFFF00FF,	/* 073 - Fuchsia */
            0xFFDCDCDC,	/* 074 - Gainsboro */
            0xFFF8F8FF,	/* 075 - GhostWhite */
            0xFFFFD700,	/* 076 - Gold */
            0xFFDAA520,	/* 077 - Goldenrod */
            0xFF808080,	/* 078 - Gray */
            0xFF008000,	/* 079 - Green */
            0xFFADFF2F,	/* 080 - GreenYellow */
            0xFFF0FFF0,	/* 081 - Honeydew */
            0xFFFF69B4,	/* 082 - HotPink */
            0xFFCD5C5C,	/* 083 - IndianRed */
            0xFF4B0082,	/* 084 - Indigo */
            0xFFFFFFF0,	/* 085 - Ivory */
            0xFFF0E68C,	/* 086 - Khaki */
            0xFFE6E6FA,	/* 087 - Lavender */
            0xFFFFF0F5,	/* 088 - LavenderBlush */
            0xFF7CFC00,	/* 089 - LawnGreen */
            0xFFFFFACD,	/* 090 - LemonChiffon */
            0xFFADD8E6,	/* 091 - LightBlue */
            0xFFF08080,	/* 092 - LightCoral */
            0xFFE0FFFF,	/* 093 - LightCyan */
            0xFFFAFAD2,	/* 094 - LightGoldenrodYellow */
            0xFFD3D3D3,	/* 095 - LightGray */
            0xFF90EE90,	/* 096 - LightGreen */
            0xFFFFB6C1,	/* 097 - LightPink */
            0xFFFFA07A,	/* 098 - LightSalmon */
            0xFF20B2AA,	/* 099 - LightSeaGreen */
            0xFF87CEFA,	/* 100 - LightSkyBlue */
            0xFF778899,	/* 101 - LightSlateGray */
            0xFFB0C4DE,	/* 102 - LightSteelBlue */
            0xFFFFFFE0,	/* 103 - LightYellow */
            0xFF00FF00,	/* 104 - Lime */
            0xFF32CD32,	/* 105 - LimeGreen */
            0xFFFAF0E6,	/* 106 - Linen */
            0xFFFF00FF,	/* 107 - Magenta */
            0xFF800000,	/* 108 - Maroon */
            0xFF66CDAA,	/* 109 - MediumAquamarine */
            0xFF0000CD,	/* 110 - MediumBlue */
            0xFFBA55D3,	/* 111 - MediumOrchid */
            0xFF9370DB,	/* 112 - MediumPurple */
            0xFF3CB371,	/* 113 - MediumSeaGreen */
            0xFF7B68EE,	/* 114 - MediumSlateBlue */
            0xFF00FA9A,	/* 115 - MediumSpringGreen */
            0xFF48D1CC,	/* 116 - MediumTurquoise */
            0xFFC71585,	/* 117 - MediumVioletRed */
            0xFF191970,	/* 118 - MidnightBlue */
            0xFFF5FFFA,	/* 119 - MintCream */
            0xFFFFE4E1,	/* 120 - MistyRose */
            0xFFFFE4B5,	/* 121 - Moccasin */
            0xFFFFDEAD,	/* 122 - NavajoWhite */
            0xFF000080,	/* 123 - Navy */
            0xFFFDF5E6,	/* 124 - OldLace */
            0xFF808000,	/* 125 - Olive */
            0xFF6B8E23,	/* 126 - OliveDrab */
            0xFFFFA500,	/* 127 - Orange */
            0xFFFF4500,	/* 128 - OrangeRed */
            0xFFDA70D6,	/* 129 - Orchid */
            0xFFEEE8AA,	/* 130 - PaleGoldenrod */
            0xFF98FB98,	/* 131 - PaleGreen */
            0xFFAFEEEE,	/* 132 - PaleTurquoise */
            0xFFDB7093,	/* 133 - PaleVioletRed */
            0xFFFFEFD5,	/* 134 - PapayaWhip */
            0xFFFFDAB9,	/* 135 - PeachPuff */
            0xFFCD853F,	/* 136 - Peru */
            0xFFFFC0CB,	/* 137 - Pink */
            0xFFDDA0DD,	/* 138 - Plum */
            0xFFB0E0E6,	/* 139 - PowderBlue */
            0xFF800080,	/* 140 - Purple */
            0xFFFF0000,	/* 141 - Red */
            0xFFBC8F8F,	/* 142 - RosyBrown */
            0xFF4169E1,	/* 143 - RoyalBlue */
            0xFF8B4513,	/* 144 - SaddleBrown */
            0xFFFA8072,	/* 145 - Salmon */
            0xFFF4A460,	/* 146 - SandyBrown */
            0xFF2E8B57,	/* 147 - SeaGreen */
            0xFFFFF5EE,	/* 148 - SeaShell */
            0xFFA0522D,	/* 149 - Sienna */
            0xFFC0C0C0,	/* 150 - Silver */
            0xFF87CEEB,	/* 151 - SkyBlue */
            0xFF6A5ACD,	/* 152 - SlateBlue */
            0xFF708090,	/* 153 - SlateGray */
            0xFFFFFAFA,	/* 154 - Snow */
            0xFF00FF7F,	/* 155 - SpringGreen */
            0xFF4682B4,	/* 156 - SteelBlue */
            0xFFD2B48C,	/* 157 - Tan */
            0xFF008080,	/* 158 - Teal */
            0xFFD8BFD8,	/* 159 - Thistle */
            0xFFFF6347,	/* 160 - Tomato */
            0xFF40E0D0,	/* 161 - Turquoise */
            0xFFEE82EE,	/* 162 - Violet */
            0xFFF5DEB3,	/* 163 - Wheat */
            0xFFFFFFFF,	/* 164 - White */
            0xFFF5F5F5,	/* 165 - WhiteSmoke */
            0xFFFFFF00,	/* 166 - Yellow */
            0xFF9ACD32,	/* 167 - YellowGreen */
            0xFFECE9D8,	/* 168 - ButtonFace */
            0xFFFFFFFF,	/* 169 - ButtonHighlight */
            0xFFACA899,	/* 170 - ButtonShadow */
            0xFF3D95FF,	/* 171 - GradientActiveCaption */
            0xFF9DB9EB,	/* 172 - GradientInactiveCaption */
            0xFFECE9D8,	/* 173 - MenuBar */
            0xFF316AC5,	/* 174 - MenuHighlight */
        };
        static Dictionary<string, Color> colorsByName = new Dictionary<string, Color>();
        static KnownColors()
        {
            int j = argbValues.Length;
            for (short i = 0; i < j; ++i)
            {
                string colorName = GetName(i).ToUpper();
                colorsByName[colorName] = FromKnownColor((KnownColor)i);
            }
            colorsByName["NONE"] = Color.Empty;
        }

        public static Color FromKnownColor(string colorName)
        {
            colorName = colorName.ToUpper();
            Color c;
            if (!colorsByName.TryGetValue(colorName.ToUpper(), out c))
            {
                return Color.Black;
            }
            return c;
        }
        public static Color FromKnownColor(KnownColor kc)
        {
            int index = (int)kc;
            if (index < 0 || index > argbValues.Length)
            {
                return Color.Black;
            }
            uint colorValue = argbValues[index];
            return new Color((byte)(colorValue >> 24),
                     (byte)((colorValue >> 16) & 0xFF),
                     (byte)((colorValue >> 8) & 0xFF),
                     (byte)(colorValue & 0xFF));
        }
        public static string GetName(short kc)
        {
            switch (kc)
            {
                case 1: return "ActiveBorder";
                case 2: return "ActiveCaption";
                case 3: return "ActiveCaptionText";
                case 4: return "AppWorkspace";
                case 5: return "Control";
                case 6: return "ControlDark";
                case 7: return "ControlDarkDark";
                case 8: return "ControlLight";
                case 9: return "ControlLightLight";
                case 10: return "ControlText";
                case 11: return "Desktop";
                case 12: return "GrayText";
                case 13: return "Highlight";
                case 14: return "HighlightText";
                case 15: return "HotTrack";
                case 16: return "InactiveBorder";
                case 17: return "InactiveCaption";
                case 18: return "InactiveCaptionText";
                case 19: return "Info";
                case 20: return "InfoText";
                case 21: return "Menu";
                case 22: return "MenuText";
                case 23: return "ScrollBar";
                case 24: return "Window";
                case 25: return "WindowFrame";
                case 26: return "WindowText";
                case 27: return "Transparent";
                case 28: return "AliceBlue";
                case 29: return "AntiqueWhite";
                case 30: return "Aqua";
                case 31: return "Aquamarine";
                case 32: return "Azure";
                case 33: return "Beige";
                case 34: return "Bisque";
                case 35: return "Black";
                case 36: return "BlanchedAlmond";
                case 37: return "Blue";
                case 38: return "BlueViolet";
                case 39: return "Brown";
                case 40: return "BurlyWood";
                case 41: return "CadetBlue";
                case 42: return "Chartreuse";
                case 43: return "Chocolate";
                case 44: return "Coral";
                case 45: return "CornflowerBlue";
                case 46: return "Cornsilk";
                case 47: return "Crimson";
                case 48: return "Cyan";
                case 49: return "DarkBlue";
                case 50: return "DarkCyan";
                case 51: return "DarkGoldenrod";
                case 52: return "DarkGray";
                case 53: return "DarkGreen";
                case 54: return "DarkKhaki";
                case 55: return "DarkMagenta";
                case 56: return "DarkOliveGreen";
                case 57: return "DarkOrange";
                case 58: return "DarkOrchid";
                case 59: return "DarkRed";
                case 60: return "DarkSalmon";
                case 61: return "DarkSeaGreen";
                case 62: return "DarkSlateBlue";
                case 63: return "DarkSlateGray";
                case 64: return "DarkTurquoise";
                case 65: return "DarkViolet";
                case 66: return "DeepPink";
                case 67: return "DeepSkyBlue";
                case 68: return "DimGray";
                case 69: return "DodgerBlue";
                case 70: return "Firebrick";
                case 71: return "FloralWhite";
                case 72: return "ForestGreen";
                case 73: return "Fuchsia";
                case 74: return "Gainsboro";
                case 75: return "GhostWhite";
                case 76: return "Gold";
                case 77: return "Goldenrod";
                case 78: return "Gray";
                case 79: return "Green";
                case 80: return "GreenYellow";
                case 81: return "Honeydew";
                case 82: return "HotPink";
                case 83: return "IndianRed";
                case 84: return "Indigo";
                case 85: return "Ivory";
                case 86: return "Khaki";
                case 87: return "Lavender";
                case 88: return "LavenderBlush";
                case 89: return "LawnGreen";
                case 90: return "LemonChiffon";
                case 91: return "LightBlue";
                case 92: return "LightCoral";
                case 93: return "LightCyan";
                case 94: return "LightGoldenrodYellow";
                case 95: return "LightGray";
                case 96: return "LightGreen";
                case 97: return "LightPink";
                case 98: return "LightSalmon";
                case 99: return "LightSeaGreen";
                case 100: return "LightSkyBlue";
                case 101: return "LightSlateGray";
                case 102: return "LightSteelBlue";
                case 103: return "LightYellow";
                case 104: return "Lime";
                case 105: return "LimeGreen";
                case 106: return "Linen";
                case 107: return "Magenta";
                case 108: return "Maroon";
                case 109: return "MediumAquamarine";
                case 110: return "MediumBlue";
                case 111: return "MediumOrchid";
                case 112: return "MediumPurple";
                case 113: return "MediumSeaGreen";
                case 114: return "MediumSlateBlue";
                case 115: return "MediumSpringGreen";
                case 116: return "MediumTurquoise";
                case 117: return "MediumVioletRed";
                case 118: return "MidnightBlue";
                case 119: return "MintCream";
                case 120: return "MistyRose";
                case 121: return "Moccasin";
                case 122: return "NavajoWhite";
                case 123: return "Navy";
                case 124: return "OldLace";
                case 125: return "Olive";
                case 126: return "OliveDrab";
                case 127: return "Orange";
                case 128: return "OrangeRed";
                case 129: return "Orchid";
                case 130: return "PaleGoldenrod";
                case 131: return "PaleGreen";
                case 132: return "PaleTurquoise";
                case 133: return "PaleVioletRed";
                case 134: return "PapayaWhip";
                case 135: return "PeachPuff";
                case 136: return "Peru";
                case 137: return "Pink";
                case 138: return "Plum";
                case 139: return "PowderBlue";
                case 140: return "Purple";
                case 141: return "Red";
                case 142: return "RosyBrown";
                case 143: return "RoyalBlue";
                case 144: return "SaddleBrown";
                case 145: return "Salmon";
                case 146: return "SandyBrown";
                case 147: return "SeaGreen";
                case 148: return "SeaShell";
                case 149: return "Sienna";
                case 150: return "Silver";
                case 151: return "SkyBlue";
                case 152: return "SlateBlue";
                case 153: return "SlateGray";
                case 154: return "Snow";
                case 155: return "SpringGreen";
                case 156: return "SteelBlue";
                case 157: return "Tan";
                case 158: return "Teal";
                case 159: return "Thistle";
                case 160: return "Tomato";
                case 161: return "Turquoise";
                case 162: return "Violet";
                case 163: return "Wheat";
                case 164: return "White";
                case 165: return "WhiteSmoke";
                case 166: return "Yellow";
                case 167: return "YellowGreen";
                case 168: return "ButtonFace";
                case 169: return "ButtonHighlight";
                case 170: return "ButtonShadow";
                case 171: return "GradientActiveCaption";
                case 172: return "GradientInactiveCaption";
                case 173: return "MenuBar";
                case 174: return "MenuHighlight";
                default: return "";
            }
        }
        public static string GetName(KnownColor kc)
        {
            return GetName((short)kc);
        }


    }



}