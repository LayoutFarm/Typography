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
    /// user request font specification
    /// </summary>
    public sealed class RequestFont
    {
        //each platform/canvas has its own representation of this Font 
        //this is just a request for specficic font presentation at a time
        //----- 

        public RequestFont(string facename, float fontSizeInPts, FontStyle style = FontStyle.Regular)
            : this(facename, Len.Pt(fontSizeInPts), style)
        {
        }
        public RequestFont(string facename, Len fontSize, FontStyle style = FontStyle.Regular)
        {
            //Lang = "en";//default
            Name = facename;
            Size = fontSize; //store user font size here
            //SizeInPoints = fontSizeInPts;
            Style = style;
            float fontSizeInPts = SizeInPoints = fontSize.ToPoints();
            FontKey = CalculateFontKey(facename, fontSizeInPts, style);
        }
        public Len Size { get; private set; }
        //
        public int FontKey { get; private set; }
        /// <summary>
        /// font's face name
        /// </summary>
        public string Name { get; private set; }
        public FontStyle Style { get; private set; }

        /// <summary>
        /// emheight in point unit
        /// </summary>
        public float SizeInPoints { get; private set; }

        public static int CalculateFontKey(string facename, float fontSizeInPts, FontStyle style)
        {
            return (new InternalFontKey(facename, fontSizeInPts, style)).GetHashCode();
        }

        struct InternalFontKey
        {

            public readonly int FontNameIndex;
            public readonly float FontSize;
            public readonly FontStyle FontStyle;

            public InternalFontKey(string fontname, float fontSize, FontStyle fs)
            {
                //font name/ not filename
                this.FontNameIndex = RegisterFontName(fontname.ToLower());
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
                    int nameCrc32 = CRC32Calculator.CalculateCrc32(fontName);
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

        //------------------ 
        //caching ...
        //store latest platform's actual font  as WeakReference
        //access this by PixelFarm.Drawing.Internal.RequestFontCacheAccess
        internal int _platform_id;//resolve by system id
        internal object _latestResolved; //result of the actual font, we store it as weak reference
        internal int _whitespace_width;
        internal int _generalLineSpacingInPx;

        //------------------ 


        //TODO: review here again
        internal float _sizeInPx;
        internal float _descentInPx;
        internal float _ascentInPx;
        internal float _lineGapInPx;

        public float SizeInPixels => _sizeInPx;
        public float DescentInPixels => _descentInPx;
        public float AscentInPixels => _ascentInPx;
        public float LineGapInPixels => _lineGapInPx;
        /// <summary>
        /// already in pixels
        /// </summary>
        public float LineSpacingInPixels => _generalLineSpacingInPx;

    }

    namespace Internal
    {
        public static class RequestFontCacheAccess
        {
            static int s_totalCacheSystemId;
            public static int GetNewCacheSystemId()
            {
                return ++s_totalCacheSystemId;
            }
            public static void ClearCache(RequestFont reqFont)
            {
                reqFont._platform_id = 0;
                reqFont._latestResolved = null;
                reqFont._whitespace_width = reqFont._generalLineSpacingInPx = 0;
            }
            public static void SetActualFont(RequestFont reqFont,
                int platform_id,
                object platformFont)
            {
                //replace 
                reqFont._platform_id = platform_id;
                reqFont._latestResolved = platformFont;
            }
            public static void SetGeneralFontMetricInfo(
               RequestFont reqFont,
               float sizeInPx, float ascentInPx,
               float descentInPx, float lineGapInPx,
               float lineHeight)
            {
                reqFont._sizeInPx = sizeInPx;
                reqFont._ascentInPx = ascentInPx;
                reqFont._descentInPx = descentInPx;
                reqFont._lineGapInPx = lineGapInPx;
                reqFont._generalLineSpacingInPx = (int)Math.Round(lineHeight);
            }

            public static T GetActualFont<T>(RequestFont reqFont, int platform_id)
               where T : class
            {
                if (reqFont._platform_id == platform_id &&
                    reqFont._latestResolved != null)
                {
                    return reqFont._latestResolved as T;
                }
                return null;
            }
            public static int GetWhitespaceWidth(RequestFont reqFont, int platform_id)
            {
                if (reqFont._platform_id == platform_id &&
                    reqFont._latestResolved != null)
                {
                    return reqFont._whitespace_width;
                }
                return 0;
            }
            public static void SetWhitespaceWidth(RequestFont reqFont,
                int platform_id,
                int whitespaceW)
            {
                reqFont._platform_id = platform_id;
                reqFont._whitespace_width = whitespaceW;
            }
            public static int GetLinespaceHeight(RequestFont reqFont, int platform_id)
            {
                if (reqFont._platform_id == platform_id &&
                    reqFont._latestResolved != null)
                {
                    return reqFont._generalLineSpacingInPx;
                }
                return 0;
            }
            public static void SetLineSpaceHeight(RequestFont reqFont,
               int platform_id,
               int height)
            {
                reqFont._platform_id = platform_id;
                reqFont._generalLineSpacingInPx = height;
            }
        }
    }

}
