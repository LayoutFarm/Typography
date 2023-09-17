//BSD, 2014-present, WinterDev 
//ArthurHub, Jose Manuel Menendez Poo

// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using LayoutFarm.Css;

namespace LayoutFarm.WebDom.Parser
{
    /// <summary>
    /// Parse CSS properties values like numbers, urls, etc.
    /// </summary>
    public static class CssValueParser
    {


        /// <summary>
        /// get length in pixel
        /// </summary>
        /// <param name="length"></param>
        /// <param name="hundredPercent"></param>
        /// <param name="box"></param>
        /// <returns></returns>
        public static float ConvertToPx(CssLength length, float hundredPercent, IHasEmHeight box)
        {
            //Return zero if no length specified, zero specified      
            switch (length.UnitOrNames)
            {
                case CssUnitOrNames.EmptyValue:
                    return 0;
                case CssUnitOrNames.Percent:
                    return (length.Number / 100f) * hundredPercent;
                case CssUnitOrNames.Ems:
                    return length.Number * box.GetEmHeight();
                case CssUnitOrNames.Ex:
                    return length.Number * (box.GetEmHeight() / 2);
                case CssUnitOrNames.Pixels:
                    //atodo: check support for hi dpi
                    return length.Number;
                case CssUnitOrNames.Milimeters:
                    return length.Number * 3.779527559f; //3 pixels per millimeter      
                case CssUnitOrNames.Centimeters:
                    return length.Number * 37.795275591f; //37 pixels per centimeter 
                case CssUnitOrNames.Inches:
                    return length.Number * 96f; //96 pixels per inch 
                case CssUnitOrNames.Points:
                    return length.Number * (96f / 72f); // 1 point = 1/72 of inch   
                case CssUnitOrNames.Picas:
                    return length.Number * 16f; // 1 pica = 12 points 
                default:
                    return 0;
            }
        }

        public static float ConvertToPxWithFontAdjust(CssLength length, float hundredPercent, IHasEmHeight box)
        {
            //Return zero if no length specified, zero specified     
            switch (length.UnitOrNames)
            {
                case CssUnitOrNames.EmptyValue:
                    return 0;
                case CssUnitOrNames.Percent:
                    return (length.Number / 100f) * hundredPercent;
                case CssUnitOrNames.Ems:
                    return length.Number * box.GetEmHeight();
                case CssUnitOrNames.Ex:
                    return length.Number * (box.GetEmHeight() / 2);
                case CssUnitOrNames.Pixels:
                    //atodo: check support for hi dpi
                    return length.Number * (72f / 96f); //font adjust
                case CssUnitOrNames.Milimeters:
                    return length.Number * 3.779527559f; //3 pixels per millimeter      
                case CssUnitOrNames.Centimeters:
                    return length.Number * 37.795275591f; //37 pixels per centimeter 
                case CssUnitOrNames.Inches:
                    return length.Number * 96f; //96 pixels per inch 
                case CssUnitOrNames.Points:
                    return length.Number * (96f / 72f); // 1 point = 1/72 of inch   
                case CssUnitOrNames.Picas:
                    return length.Number * 16f; // 1 pica = 12 points 
                default:
                    return 0;
            }
        }
        public static float GetActualBorderWidth(CssLength borderValue, IHasEmHeight b)
        {
            //------------------------------
            //plan: use extended cssunit
            //------------------------------
            switch (borderValue.UnitOrNames)
            {
                case CssUnitOrNames.EmptyValue://as medium 
                case CssUnitOrNames.BorderMedium:
                    return 2f;
                case CssUnitOrNames.BorderThin:
                    return 1f;
                case CssUnitOrNames.BorderThick:
                    return 4f;
                default:
                    return Math.Abs(ConvertToPx(borderValue, 1, b));
            }
        }

        /// <summary>
        /// Parses a color value in CSS style; e.g. #ff0000, red, rgb(255,0,0), rgb(100%, 0, 0)
        /// </summary>
        /// <param name="colorValue">color string value to parse</param>
        /// <returns>Color value</returns>
        public static PixelFarm.Drawing.Color ParseCssColor(string colorValue)
        {
            PixelFarm.Drawing.Color color;
            TryGetColor(colorValue, 0, colorValue.Length, out color);
            return color;
        }
        #region Private methods

        /// <summary>
        /// Parses a color value in CSS style; e.g. #ff0000, RED, RGB(255,0,0), RGB(100%, 0, 0)
        /// </summary>
        /// <param name="str">color substring value to parse</param>
        /// <param name="idx">substring start idx </param>
        /// <param name="length">substring length</param>
        /// <param name="color">return the parsed color</param>
        /// <returns>true - valid color, false - otherwise</returns>
        static bool TryGetColor(string str, int idx, int length, out PixelFarm.Drawing.Color color)
        {

            //https://www.w3.org/TR/SVGColor12/
            //1) Three digit hex — #rgb
            //    Each hexadecimal digit, in the range 0 to F, represents one sRGB color component in the order red, green and blue.The digits A to F may be in either uppercase or lowercase. The value of the color component is obtained by replicating digits, so 0 become 00, 1 becomes 11, F becomes FF.This compact syntactical form can represent only 4096 colors.Examples: #000 (i.e. black) #fff (i.e. white) #6CF (i.e. #66CCFF, rgb(102, 204, 255)).
            //2) Six digit hex — #rrggbb
            //    Each pair of hexadecimal digits, in the range 0 to F, represents one sRGB color component in the order red, green and blue.The digits A to F may be in either uppercase or lowercase.This syntactical form, originally introduced by HTML, can represent 16777216 colors.Examples: #9400D3 (i.e. a dark violet), #FFD700 (i.e. a golden color). 
            //3) Integer functional — rgb(rrr, ggg, bbb)
            //    Each integer represents one sRGB color component in the order red, green and blue, separated by a comma and optionally by white space.Each integer is in the range 0 to 255.This syntactical form can represent 16777216 colors.Examples: rgb(233, 150, 122)(i.e.a salmon pink), rgb(255, 165, 0)(i.e.an orange).
            //4) Float functional — rgb(R %, G %, B %)
            //    Each percentage value represents one sRGB color component in the order red, green and blue, separated by a comma and optionally by white space.For colors inside the sRGB gamut, the range of each component is 0.0 % to 100.0 % and an arbitrary number of decimal places may be supplied.Scientific notation is not supported. This syntactical form can represent an arbitrary range of colors, completely covering the sRGB gamut. Color values where one or more components are below 0.0 % or above 100.0 % represent colors outside the sRGB gamut.Examples: rgb(12.375 %, 34.286 %, 28.97 %).
            //5) Color keyword
            //    Originally implemented in HTML browsers and eventually standardized in SVG 1.1, the full list of color keywords and their corresponding sRGB values are given in the SVG 1.1 specification.SVG Tiny 1.2 required only a subset of these, sixteen color keywords. SVG Color requires the full set to be supported.
            try
            {
                if (!string.IsNullOrEmpty(str))
                {
                    if (length > 1 && str[idx] == '#')
                    {
                        return GetColorByHex(str, idx, length, out color);
                    }
                    else if (length > 10 && SubStringEquals(str, idx, 4, "rgb(") && str[length - 1] == ')')
                    {
                        return GetColorByRgb(str, idx, length, out color);
                    }
                    else if (length > 13 && SubStringEquals(str, idx, 5, "rgba(") && str[length - 1] == ')')
                    {
                        return GetColorByRgba(str, idx, length, out color);
                    }
                    else
                    {
                        return GetColorByName(str, idx, length, out color);
                    }
                }
            }
            catch
            {
                //TODO: review here ?????

            }
            color = PixelFarm.Drawing.Color.Black;
            return false;
        }
        /// <summary>
        /// Compare that the substring of <paramref name="str"/> is equal to <paramref name="str"/>
        /// Assume given substring is not empty and all indexes are valid!<br/>
        /// </summary>
        /// <returns>true - equals, false - not equals</returns>
        static bool SubStringEquals(string str, int idx, int length, string str2)
        {
            if (length == str2.Length && idx + length <= str.Length)
            {
                for (int i = 0; i < length; i++)
                {
                    if (Char.ToLowerInvariant(str[idx + i]) != Char.ToLowerInvariant(str2[i]))
                        return false;
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Get color by parsing given hex value color string (#A28B34).
        /// </summary>
        /// <returns>true - valid color, false - otherwise</returns>
        static bool GetColorByHex(string str, int idx, int length, out PixelFarm.Drawing.Color color)
        {

            //from //https://www.w3.org/TR/SVGColor12/
            //1) Three digit hex — #rgb
            //    Each hexadecimal digit, in the range 0 to F, represents one sRGB color component in the order red, green and blue.
            //    The digits A to F may be in either uppercase or lowercase.
            //    The value of the color component is obtained by replicating digits,
            //    so 0 become 00, 1 becomes 11, F becomes FF.
            //    This compact syntactical form can represent only 4096 colors.Examples: #000 (i.e. black) #fff (i.e. white) #6CF (i.e. #66CCFF, rgb(102, 204, 255)).
            //2) Six digit hex — #rrggbb
            //    Each pair of hexadecimal digits, in the range 0 to F, represents one sRGB color component in the order red, green and blue.
            //    The digits A to F may be in either uppercase or lowercase.
            //    This syntactical form, originally introduced by HTML, can represent 16777216 colors.
            //3) 9 digit hex — #rrggbb
            //... more...

            switch (length)
            {
                default:
                    color = PixelFarm.Drawing.Color.Empty;
                    return false;
                case 4:
                    {
                        int r = ParseHexInt(str, idx + 1, 1);
                        r = r * 16 + r;
                        int g = ParseHexInt(str, idx + 2, 1);
                        g = g * 16 + g;
                        int b = ParseHexInt(str, idx + 3, 1);
                        b = b * 16 + b;
                        color = PixelFarm.Drawing.Color.FromArgb(r, g, b);
                        return true;
                    }
                case 7:
                    color = PixelFarm.Drawing.Color.FromArgb(
                        ParseHexInt(str, idx + 1, 2),//r
                        ParseHexInt(str, idx + 3, 2),//g
                        ParseHexInt(str, idx + 5, 2));//b
                    return true;
                case 9:
                    color = PixelFarm.Drawing.Color.FromArgb(
                       ParseHexInt(str, idx + 1, 2),//a
                       ParseHexInt(str, idx + 3, 2),//r
                       ParseHexInt(str, idx + 5, 2),//g
                       ParseHexInt(str, idx + 7, 2));//b
                    return true;

            }
        }

        /// <summary>
        /// Get color by parsing given RGB value color string (RGB(255,180,90))
        /// </summary>
        /// <returns>true - valid color, false - otherwise</returns>
        static bool GetColorByRgb(string str, int idx, int length, out PixelFarm.Drawing.Color color)
        {
            int r = -1;
            int g = -1;
            int b = -1;
            if (length > 10)
            {
                int s = idx + 4;
                r = ParseIntAtIndex(str, ref s);
                if (s < idx + length)
                {
                    g = ParseIntAtIndex(str, ref s);
                }
                if (s < idx + length)
                {
                    b = ParseIntAtIndex(str, ref s);
                }
            }

            if (r > -1 && g > -1 && b > -1)
            {
                color = PixelFarm.Drawing.Color.FromArgb(r, g, b);
                return true;
            }
            color = PixelFarm.Drawing.Color.Empty;
            return false;
        }

        /// <summary>
        /// Get color by parsing given RGBA value color string (RGBA(255,180,90,180))
        /// </summary>
        /// <returns>true - valid color, false - otherwise</returns>
        static bool GetColorByRgba(string str, int idx, int length, out PixelFarm.Drawing.Color color)
        {
            int r = -1;
            int g = -1;
            int b = -1;
            int a = -1;
            if (length > 13)
            {
                int s = idx + 5;
                r = ParseIntAtIndex(str, ref s);
                if (s < idx + length)
                {
                    g = ParseIntAtIndex(str, ref s);
                }
                if (s < idx + length)
                {
                    b = ParseIntAtIndex(str, ref s);
                }
                if (s < idx + length)
                {
                    a = ParseIntAtIndex(str, ref s);
                }
            }

            if (r > -1 && g > -1 && b > -1 && a > -1)
            {
                color = PixelFarm.Drawing.Color.FromArgb(a, r, g, b);
                return true;
            }
            color = PixelFarm.Drawing.Color.Empty;
            return false;
        }

        /// <summary>
        /// Get color by given name, including .NET name.
        /// </summary>
        /// <returns>true - valid color, false - otherwise</returns>
        static bool GetColorByName(string str, int idx, int length, out PixelFarm.Drawing.Color color)
        {
            color = LayoutFarm.WebDom.KnownColors.FromKnownColor(str.Substring(idx, length));
            return color.A > 0;
        }

        /// <summary>
        /// Parse the given decimal number string to positive int value.<br/>
        /// Start at given <paramref name="startIdx"/>, ignore whitespaces and take
        /// as many digits as possible to parse to int.
        /// </summary>
        /// <param name="str">the string to parse</param>
        /// <param name="startIdx">the index to start parsing at</param>
        /// <returns>parsed int or 0</returns>
        static int ParseIntAtIndex(string str, ref int startIdx)
        {
            int len = 0;
            while (char.IsWhiteSpace(str, startIdx))
                startIdx++;
            while (char.IsDigit(str, startIdx + len))
                len++;
            var val = ParseInt(str, startIdx, len);
            startIdx = startIdx + len + 1;
            return val;
        }

        /// <summary>
        /// Parse the given decimal number string to positive int value.
        /// Assume given substring is not empty and all indexes are valid!<br/>
        /// </summary>
        /// <returns>int value, -1 if not valid</returns>
        static int ParseInt(string str, int idx, int length)
        {
            if (length < 1)
                return -1;
            int num = 0;
            for (int i = 0; i < length; i++)
            {
                int c = str[idx + i];
                if (!(c >= 48 && c <= 57))
                    return -1;
                num = num * 10 + c - 48;
            }
            return num;
        }

        /// <summary>
        /// Parse the given hex number string to positive int value.
        /// Assume given substring is not empty and all indexes are valid!<br/>
        /// </summary>
        /// <returns>int value, -1 if not valid</returns>
        static int ParseHexInt(string str, int idx, int length)
        {
            if (length < 1)
                return -1;
            int num = 0;
            for (int i = 0; i < length; i++)
            {
                int c = str[idx + i];
                if (!(c >= 48 && c <= 57) && !(c >= 65 && c <= 70) && !(c >= 97 && c <= 102))
                    return -1;
                num = num * 16 + (c <= 57 ? c - 48 : (10 + c - (c <= 70 ? 65 : 97)));
            }
            return num;
        }

        #endregion

        //---------------------------
        //TODO: review here again
        //a COPY from Typography.OpenFont.Typeface  
        const int pointsPerInch = 72;

        /// <summary>
        /// convert from point-unit value to pixel value
        /// </summary>
        /// <param name="pointSize"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public static float ConvPointsToPixels(float pointSize, int resolution = 96)
        {
            //http://stackoverflow.com/questions/139655/convert-pixels-to-points
            //points = pixels * 72 / 96
            //------------------------------------------------
            //pixels = targetPointSize * 96 /72
            //pixels = targetPointSize * resolution / pointPerInch
            return pointSize * resolution / pointsPerInch;
        }
        public static float ConvPixelsToPoints(float pixelSize, int resolution = 96)
        {
            //http://stackoverflow.com/questions/139655/convert-pixels-to-points
            //points = pixels * 72 / 96
            //------------------------------------------------
            //pixels = targetPointSize * 96 /72
            //pixels = targetPointSize * resolution / pointPerInch
            return pixelSize * pointsPerInch / resolution;
        }
    }
}