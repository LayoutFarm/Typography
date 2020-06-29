//MIT, 2014-present, WinterDev 
//
// System.Drawing.FontStyle.cs
//
// Author: Dennis Hayes (dennish@raytek.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004,2006 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections.Generic;
namespace PixelFarm.Drawing
{
    [Flags]
    public enum FontStyle : byte
    {
        Regular = 0,
        Bold = 1,
        Italic = 1 << 1,
        Underline = 1 << 2,
        Strikeout = 1 << 3
    }
    /// <summary>
    /// user-request font specification
    /// </summary>
    public sealed class RequestFont
    {
        //each platform/canvas has its own representation of this Font 
        //this is just a request for specficic font presentation at a time
        //----- 

        public sealed class OtherChoice
        {
            /// <summary>
            /// primary font size
            /// </summary>
            public Len Size { get; }
            /// <summary>
            /// font's face name
            /// </summary>
            public string Name { get; }
            public FontStyle Style { get; }
            public int FontKey { get; }
            public float SizeInPoints { get; }

            public OtherChoice(string facename, float fontSizeInPts, FontStyle style = FontStyle.Regular)
                : this(facename, Len.Pt(fontSizeInPts), style)
            {
            }
            public OtherChoice(string facename, Len fontSize, FontStyle style = FontStyle.Regular)
            {
                Name = facename; //primary typeface name
                Size = fontSize; //store user font size here 
                Style = style;
                FontKey = CalculateFontKey(facename, SizeInPoints = fontSize.ToPoints(), style);
            }

        }


        public int FontKey { get; }
        public Len Size { get; }
        public string Name { get; }
        public FontStyle Style { get; }
        /// <summary>
        /// emheight in point unit
        /// </summary>
        public float SizeInPoints { get; }

        readonly OtherChoice[] _otherChoices;

        public RequestFont(string facename, float fontSizeInPts, FontStyle style = FontStyle.Regular, OtherChoice[] otherChoices = null)
            : this(facename, Len.Pt(fontSizeInPts), style, otherChoices)
        {
        }
        public RequestFont(string facename, Len fontSize, FontStyle style = FontStyle.Regular, OtherChoice[] otherChoices = null)
        {
            Name = facename; //primary typeface name
            Size = fontSize; //store user font size here 
            Style = style;
            FontKey = CalculateFontKey(facename, SizeInPoints = fontSize.ToPoints(), style);

            _otherChoices = otherChoices;
        }

        public int OtherChoicesCount => (_otherChoices != null) ? _otherChoices.Length : 0;
        public OtherChoice GetOtherChoice(int index) => _otherChoices[index];


        public static int CalculateFontKey(string typefaceName, float fontSizeInPts, FontStyle style)
        {
            return (new InternalFontKey(typefaceName, fontSizeInPts, style)).GetHashCode();
        }



        //------------------ 
        //caching ...

        //preserve 2 field user cache their actual here
        internal ResolvedFontBase _resolvedFont1;
        internal object _resolvedFont2;

        public static void SetResolvedFont1(RequestFont reqFont, ResolvedFontBase resolvedFont)
        {
            reqFont._resolvedFont1 = resolvedFont;
        }
        public static void SetResolvedFont2(RequestFont reqFont, object resolvedFont)
        {
            reqFont._resolvedFont2 = resolvedFont;
        }
        public static T GetResolvedFont1<T>(RequestFont reqFont)
           where T : ResolvedFontBase
        {
            return reqFont._resolvedFont1 as T;
        }
        public static T GetResolvedFont2<T>(RequestFont reqFont)
           where T : class
        {
            return reqFont._resolvedFont2 as T;
        }
       
#if DEBUG
        public override string ToString()
        {
            return Name + "," + SizeInPoints + "," + Style;
        }
#endif
    }


    struct InternalFontKey
    {

        public readonly int FontNameIndex;
        public readonly float FontSize;
        public readonly FontStyle FontStyle;

        public InternalFontKey(string typefaceName, float fontSize, FontStyle fs)
        {
            //font name/ not filename
            this.FontNameIndex = RegisterFontName(typefaceName.ToLower());
            this.FontSize = fontSize;
            this.FontStyle = fs;
        }

        static Dictionary<string, int> s_registerFontNames = new Dictionary<string, int>();

        static InternalFontKey()
        {
            RegisterFontName(""); //blank font name
        }
        static int RegisterFontName(string fontName)
        {
            fontName = fontName.ToUpper();
            if (!s_registerFontNames.TryGetValue(fontName, out int found))
            {
                int nameCrc32 = TinyCRC32Calculator.CalculateCrc32(fontName);
                s_registerFontNames.Add(fontName, nameCrc32);
                return nameCrc32;
            }
            return found;
        }
        public override int GetHashCode()
        {
            return CalculateGetHasCode(this.FontNameIndex, this.FontSize, (int)this.FontStyle);
        }
        static int CalculateGetHasCode(int nameIndex, float fontSize, int fontstyle)
        {
            //modified from https://stackoverflow.com/questions/1646807/quick-and-simple-hash-code-combinations
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + nameIndex.GetHashCode();
                hash = hash * 31 + fontSize.GetHashCode();
                hash = hash * 31 + fontstyle.GetHashCode();
                return hash;
            }
        }
    }

    public abstract class ResolvedFontBase
    {
        public float SizeInPoints { get; protected set; }
        public FontStyle FontStyle { get; protected set; }
        public int FontKey { get; protected set; }
        public float ScaleToPixel { get; protected set; }

        public float WhitespaceWidthF { get; protected set; }
        public int WhitespaceWidth { get; protected set; }

        public float AscentInPixels { get; protected set; }
        public float DescentInPixels { get; protected set; }
        public int LineSpacingInPixels { get; protected set; }
        public float LineGapInPx { get; protected set; }


        public ResolvedFontBase(string name, float sizeInPoints, FontStyle fontStyle, int fontKey)
        {
            Name = name;
            SizeInPoints = sizeInPoints;
            FontStyle = fontStyle;
            FontKey = fontKey;
        }
        public ResolvedFontBase(string name, float sizeInPoints, FontStyle fontStyle)
        {
            SizeInPoints = sizeInPoints;
            FontStyle = fontStyle;
            Name = name;
            FontKey = (new InternalFontKey(name ?? "", sizeInPoints, fontStyle)).GetHashCode();
        }
        public string Name { get; }
    }

 
}
