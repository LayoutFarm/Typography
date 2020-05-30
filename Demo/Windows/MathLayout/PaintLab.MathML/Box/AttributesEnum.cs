//MIT, 2020, Brezza92
using System;

namespace LayoutFarm.MathLayout
{
    static class AttributeParser
    {
        public static Alignment ParseAlignment(string align, Alignment defaultAlign = Alignment.Default)
        {
            if (align == null)
            {
                return defaultAlign;
            }
            switch (align)
            {
                default:
                    return defaultAlign;
                case "center":
                    return Alignment.Center;
                case "top":
                    return Alignment.Top;
                case "bottom":
                    return Alignment.Bottom;
                case "left":
                    return Alignment.Left;
                case "right":
                    return Alignment.Right;
            }
        }
        public static int ParseInteger(string integerStr, int defaultInt = 0)
        {
            if (int.TryParse(integerStr, out int result))
            {
                return result;
            }
            else
            {
                return defaultInt;
            }
        }
        public static bool ParseBoolean(string booleanStr, bool defaultValue)
        {
            if (booleanStr == null)
            {
                return defaultValue;
            }
            switch (booleanStr)
            {
                default:
                    return defaultValue;
                case "true":
                    return true;
                case "false":
                    return false;
            }
        }
        public static MathMLNumWithUnit ParseNumberWithUnit(string numstr)
        {
            if (numstr == null)
            {
                return null;
            }

            if (float.TryParse(numstr, out float v))
            {
                return new MathMLNumWithUnit() { Number = v, Unit = MathMLNumUnit.None };
            }
            else if (numstr.EndsWith("%"))
            {
                numstr = numstr.Substring(0, numstr.Length - 1);
                if (float.TryParse(numstr, out v))
                {
                    return new MathMLNumWithUnit() { Number = v, Unit = MathMLNumUnit.Percentage };
                }
                return null;
            }
            else
            {
                string unit = numstr.Substring(numstr.Length - 3);// -1(change length to index) -2(last two character)
                numstr = numstr.Substring(0, numstr.Length - 2);
                if (float.TryParse(numstr, out v))
                {
                    return new MathMLNumWithUnit() { Number = v, Unit = ParseUnit(unit) };
                }
                return null;
            }
        }
        static MathMLNumUnit ParseUnit(string unitStr)
        {
            switch (unitStr)
            {
                default:
                    //return MathMLNumUnit.None;
                    throw new System.NotSupportedException();
                case "em":
                    return MathMLNumUnit.EM;
                case "ex":
                    return MathMLNumUnit.EX;
                case "px":
                    return MathMLNumUnit.Pixels;
                case "in":
                    return MathMLNumUnit.Inches;
                case "cm":
                    return MathMLNumUnit.Centimeters;
                case "mm":
                    return MathMLNumUnit.Millimeters;
                case "pt":
                    return MathMLNumUnit.Points;
                case "pc":
                    return MathMLNumUnit.Picas;
                case "%":
                    return MathMLNumUnit.Percentage;

            }
        }

        //reference code: https://stackoverflow.com/questions/16100/convert-a-string-to-an-enum-in-c-sharp
        public static T ParseEnum<T>(string enumStr, T defaultValue) where T : struct, IConvertible
        {
            if (enumStr == null)
            {
                return defaultValue;
            }
            try
            {
                T result = (T)Enum.Parse(typeof(T), enumStr);
                return result;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }

    public enum MathMLNumUnit
    {
        //reference https://www.w3.org/TR/MathML3/chapter2.xml#type.namedspace
        None,//custom default unit maybe point
        EM,//an em (font-relative unit traditionally used for horizontal lengths)
        EX,//an ex (font-relative unit traditionally used for vertical lengths)
        Pixels,//pixels, or size of a pixel in the current display
        Inches,//inches (1 inch = 2.54 centimeters)
        Centimeters,//centimeters
        Millimeters,//millimeters
        Points,//points (1 point = 1/72 inch)
        Picas,//picas (1 pica = 12 points)
        Percentage,//percentage of the default value
    }

    public enum Alignment
    {
        Default,//TopLeft
        Center,
        Top,
        Bottom,
        Left,
        Right,
    }

    public enum CarryLocation
    {
        //"w" | "nw" | "n" | "ne" | "e" | "se" | "s" | "sw"
        w,
        nw,
        n,
        ne,
        e,
        se,
        s,
        sw,
    }

    public enum CarryCrossout
    {
        //("none" | "updiagonalstrike" | "downdiagonalstrike" | "verticalstrike" | "horizontalstrike")* 
        none,
        updiagonalstrike,
        downdiagonalstrike,
        verticalstrike,
        horizontalstrike,
    }

    public enum EncloseNotation
    {
        longdiv,
        actuarial,
        radical,//deprecated
        box,
        roundedbox,
        circle,
        left,
        right,
        top,
        bottom,
        updiagonalstrike,
        downdiagonalstrike,
        verticalstrike,
        horizontalstrike,
        madruwb,
        updiagonalarrow,
        phasorangle,
    }
}