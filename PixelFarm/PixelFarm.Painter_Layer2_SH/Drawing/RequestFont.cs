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
    public enum OldFontStyle : byte
    {
        Regular = 0,
        Bold = 1,
        Italic = 1 << 1,
        Underline = 1 << 2,
        Strikeout = 1 << 3,
        Others = 1 << 4
    }






    public enum RequestFontWeight
    {
        Custom = 0, //my extension

        //https://docs.microsoft.com/en-us/typography/opentype/spec/os2

        Thin = 100,
        ExtraLight = 200,
        UltraLight = 200, //= ExtraLight
        Light = 300,
        Normal = 400,
        Medium = 500,
        SemiBold = 600,
        DemiBold = 600, //= SemiBold
        Bold = 700,
        ExtraBold = 800,
        UltraBold = 800,//= ExtraBold
        Black = 900,
        Heavy = 900,//=Black
    }
    public enum RequestFontWidthClass
    {
        Custom = 0,//my extension

        //https://docs.microsoft.com/en-us/typography/opentype/spec/os2

        UltraCondensed = 1,
        ExtraCondensed = 2,
        Condensed = 3,
        SemiCondensed = 4,
        Normal = 5,
        Medium = 5,//=Normal
        SemiExpanded = 6,
        Expanded = 7,
        ExtraExpanded = 8,
        UltraExpanded = 9
    }

    /// <summary>
    /// user-request font specification
    /// </summary>
    public sealed class RequestFont
    {

        //each platform/canvas has its own representation of this Font 
        //this is just a request for specficic font presentation at a time
        //----- 
        public sealed class Choice
        {
            /// <summary>
            /// primary font size
            /// </summary>
            public Len Size { get; }
            public float SizeInPoints { get; }

            /// <summary>
            /// font's face name
            /// </summary>
            public string Name { get; private set; }
            public OldFontStyle Style { get; private set; }

            public bool FromTypefaceFile { get; private set; }
            public string UserInputTypefaceFile { get; private set; }

            public Choice(string facename, float fontSizeInPts, OldFontStyle style = OldFontStyle.Regular)
                : this(facename, Len.Pt(fontSizeInPts), style)
            {
            }
            public Choice(string facename, Len fontSize, OldFontStyle style = OldFontStyle.Regular)
            {
                Name = facename; //primary typeface name
                Size = fontSize; //store user font size here 
                SizeInPoints = fontSize.ToPoints();
                Style = style;
            }
            private Choice(Len fontSize)
            {
                Size = fontSize; //store user font size here 
                SizeInPoints = fontSize.ToPoints();
            }

            int _fontKey;
            public int GetFontKey() => (_fontKey != 0) ? _fontKey : (_fontKey = CalculateFontKey(Name, SizeInPoints, Style));


            /// <summary>
            /// create req font+ specific typeface path
            /// </summary>
            /// <param name="path">path to typeface file</param>
            /// <returns></returns>
            public static Choice FromFile(string path, Len len, Choice[] otherChoices = null)
            {
                //the system will search for the typeface file and try loading it 
                Choice otherChoice = new Choice(len);
                otherChoice.FromTypefaceFile = true;
                otherChoice.UserInputTypefaceFile = path;
                //path to typeface file may be relative path
                return otherChoice;
            }


            internal object _resolvedFont1;
            internal object _resolvedFont2;

            public static void SetResolvedFont1(Choice ch, object resolvedFont)
            {
                ch._resolvedFont1 = resolvedFont;
            }
            public static void SetResolvedFont2(Choice ch, object resolvedFont)
            {
                ch._resolvedFont2 = resolvedFont;
            }
            public static T GetResolvedFont1<T>(Choice ch)
                where T : class
            {
                return ch._resolvedFont1 as T;
            }
            public static T GetResolvedFont2<T>(Choice ch)
               where T : class
            {
                return ch._resolvedFont2 as T;
            }

        }

        /// <summary>
        /// emheight in point unit
        /// </summary>
        public float SizeInPoints { get; }
        public Len Size { get; }

        public string Name { get; private set; }
        public OldFontStyle Style { get; private set; }

        public bool FromTypefaceFile { get; private set; }
        public string UserInputTypefaceFile { get; private set; }

        List<Choice> _otherChoices;

        public RequestFont(string fontFamily, float fontSizeInPts)
            : this(fontFamily, Len.Pt(fontSizeInPts))
        {

        }

        public RequestFont(string fontFamily, Len fontSize)
        {
            //ctor of the RequestFont supports CSS's style font-family
            //font-family: Red/Black, sans-serif;

            //font-family: "Lucida" Grande, sans-serif
            //font-family: Ahem!, sans-serif
            //font-family: test@foo, sans-serif
            //font-family: #POUND, sans-serif
            //font-family: Hawaii 5-0, sans-serif

            //*** the first one will be primary font
            //and the other will be our choice

            //see https://www.w3.org/TR/css-fonts-3/ 

            Size = fontSize; //store user font size here 
            SizeInPoints = fontSize.ToPoints();

            //parse the font family name
            //TODO: use CSS parse code?
            string[] splitedNames = fontFamily.Split(',');

#if DEBUG
            if (splitedNames.Length == 0) { throw new NotSupportedException(); }
#endif

            Name = splitedNames[0].Trim(); //case sensitive***
            if (splitedNames.Length > 1)
            {
                _otherChoices = new List<Choice>();
                for (int i = 1; i < splitedNames.Length; ++i)
                {
                    string name = splitedNames[i].Trim();
                    if (name.Length == 0) { continue; }
                    _otherChoices.Add(new Choice(splitedNames[i], fontSize));
                }
            }
        }

        private RequestFont(Len fontSize)
        {
            Size = fontSize; //store user font size here 
            SizeInPoints = fontSize.ToPoints();
        }



        public ushort WeightClass { get; set; } //Typograghy Weight class
        public ushort WidthClass { get; set; } = 5; //Typography Width-Class

        public void AddOtherChoices(IEnumerable<Choice> choices)
        {
            if (choices == null) return;

            //
            if (_otherChoices == null)
            {
                _otherChoices = new List<Choice>();
            }

            foreach (Choice ch in choices)
            {
                _otherChoices.Add(ch);
            }
        }

        public int OtherChoicesCount => (_otherChoices != null) ? _otherChoices.Count : 0;
        public Choice GetOtherChoice(int index) => _otherChoices[index];

        public static int CalculateFontKey(string typefaceName, float fontSizeInPts, OldFontStyle style)
        {
            return InternalFontKey.CalculateGetHasCode(
                InternalFontKey.RegisterFontName(typefaceName),
                fontSizeInPts,
                style.GetHashCode());
        }
        public static int CalculateFontKey(int typefaceFontKey, float fontSizeInPts, OldFontStyle style)
        {
            return InternalFontKey.CalculateGetHasCode(
                typefaceFontKey,
                fontSizeInPts,
                style.GetHashCode());
        }


        int _fontKey;
        public int FontKey => (_fontKey != 0) ? _fontKey : (_fontKey = CalculateFontKey(Name, SizeInPoints, Style));

        //------------------ 
        //caching ...

        //preserve 2 field user cache their actual here
        internal object _resolvedFont1;
        internal object _resolvedFont2;

        public static void SetResolvedFont1(RequestFont reqFont, object resolvedFont)
        {
            reqFont._resolvedFont1 = resolvedFont;
        }
        public static void SetResolvedFont2(RequestFont reqFont, object resolvedFont)
        {
            reqFont._resolvedFont2 = resolvedFont;
        }
        public static T GetResolvedFont1<T>(RequestFont reqFont)
            where T : class
        {
            return reqFont._resolvedFont1 as T;
        }
        public static T GetResolvedFont2<T>(RequestFont reqFont)
           where T : class
        {
            return reqFont._resolvedFont2 as T;
        }


        //------------------ 
        /// <summary>
        /// create req font+ specific typeface path
        /// </summary>
        /// <param name="path">path to typeface file</param>
        /// <returns></returns>
        public static RequestFont FromFile(string path, Len len)
        {
            //the system will search for the typeface file and try loading it 
            RequestFont reqFont = new RequestFont(len);
            reqFont.FromTypefaceFile = true;
            reqFont.UserInputTypefaceFile = path;

            //path to typeface file may be relative path
            return reqFont;
        }
        public static RequestFont FromFile(string typefacePath, float sizeInPoints) => FromFile(typefacePath, Len.Pt(sizeInPoints));

        //------------------ 


#if DEBUG
        public override string ToString()
        {
            return Name + "," + SizeInPoints + "," + Style;
        }
#endif
    }


    static class InternalFontKey
    {

        //only typeface name
        static readonly Dictionary<string, int> s_registerFontNames = new Dictionary<string, int>();

        static InternalFontKey()
        {
            RegisterFontName(""); //blank font name
        }
        public static int RegisterFontName(string fontName)
        {
            fontName = fontName.ToUpper();//***
            if (!s_registerFontNames.TryGetValue(fontName, out int found))
            {
                int nameCrc32 = TinyCRC32Calculator.CalculateCrc32(fontName);
                s_registerFontNames.Add(fontName, nameCrc32);
                return nameCrc32;
            }
            return found;
        }
        public static int CalculateGetHasCode(int typefaceKey, float fontSize, int fontstyle)
        {
            //modified from https://stackoverflow.com/questions/1646807/quick-and-simple-hash-code-combinations
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + typefaceKey.GetHashCode();
                hash = hash * 31 + fontSize.GetHashCode();
                hash = hash * 31 + fontstyle.GetHashCode();
                return hash;
            }
        }
    }

    public readonly struct TextBufferSpan
    {
        public readonly int start;
        public readonly int len;

        readonly char[] _rawString;

        public TextBufferSpan(char[] rawCharBuffer)
        {
            _rawString = rawCharBuffer;
            this.len = rawCharBuffer.Length;
            this.start = 0;
        }
        public TextBufferSpan(char[] rawCharBuffer, int start, int len)
        {
            this.start = start;
            this.len = len;
            _rawString = rawCharBuffer;
        }

        public override string ToString()
        {
            return start + ":" + len;
        }


        public char[] GetRawCharBuffer() => _rawString;
    }

    public struct TextSpanMeasureResult
    {
        public int[] outputXAdvances;
        public int outputTotalW;
        public ushort lineHeight;

        public bool hasSomeExtraOffsetY;
        public short minOffsetY;
        public short maxOffsetY;
    }
     
}


