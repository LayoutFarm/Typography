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
using System.Text;

namespace PixelFarm.Drawing
{

    public interface IFormattedGlyphPlanList
    {

    }
    public enum RequestFontStyle
    {
        //https://www.w3.org/TR/css-fonts-3/#propdef-font-style
        Regular,
        Italic,
        Oblique
    }


    public enum RequestFontWeight : ushort
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

    public enum RequestFontWidthClass : ushort
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


    public abstract class ReqFontSpec
    {
        internal ReqFontSpec()
        {
            StartCodePoint = -1; //default
        }
        /// <summary>
        /// emheight in point unit
        /// </summary>
        public float SizeInPoints { get; internal set; }
        public Len Size { get; internal set; }
        public string Name { get; internal set; }
        public RequestFontStyle Style { get; internal set; } //https://www.w3.org/TR/css-fonts-3/#propdef-font-style 

        public ushort WeightClass { get; internal set; } = (ushort)RequestFontWeight.Normal; //400= regular
        public ushort WidthClass { get; internal set; } = (ushort)RequestFontWidthClass.Normal; //Typography Width-Class and https://www.w3.org/TR/css-fonts-3/#propdef-font-stretch
        //
        public string Src { get; internal set; } //https://www.w3.org/TR/css-fonts-3/#propdef-font-style

        public int StartCodePoint { get; private set; } //https://www.w3.org/TR/css-fonts-3/#descdef-unicode-range
        public int EndCodePoint { get; private set; } //https://www.w3.org/TR/css-fonts-3/#descdef-unicode-range

        public void SetUnicodeRange(int startCodePoint, int endCodePoint)
        {
            StartCodePoint = startCodePoint;
            EndCodePoint = endCodePoint;
        }


        int _runtimeReqKey; //this value depends on the system (string.GetHashCode())

        /// <summary>
        /// get request key
        /// </summary>
        /// <returns></returns>
        public int GetReqKey()
        {
            if (_runtimeReqKey == 0)
            {
                //calculate request key
                if (s_stbuilder == null)
                {
                    s_stbuilder = new StringBuilder();
                }

                //create a string iden for this request font
                s_stbuilder.Length = 0; //clear
                //
                s_stbuilder.Append(Name.ToUpper());
                s_stbuilder.Append(SizeInPoints.ToString("0.00"));
                //

                int hash = 17;
                hash = hash * 31 + s_stbuilder.ToString().GetHashCode();
                hash = hash * 31 + (int)Style;
                hash = hash * 31 + (int)WeightClass;
                hash = hash * 31 + (int)WidthClass;
                hash = hash * 31 + (int)StartCodePoint;
                return _runtimeReqKey = hash * 31 + (int)EndCodePoint;

            }
            return _runtimeReqKey;
        }

        [ThreadStatic]
        static StringBuilder s_stbuilder;
#if DEBUG
        public override string ToString()
        {
            return Name + "," + SizeInPoints + "," + Style;
        }
#endif


        //------------------ 
        //caching ...

        //preserve 2 field user cache their actual here
        internal object _resolvedFont1;
        internal object _resolvedFont2;

        public static void SetResolvedFont1(ReqFontSpec reqFont, object resolvedFont)
        {
            reqFont._resolvedFont1 = resolvedFont;
        }
        public static void SetResolvedFont2(ReqFontSpec reqFont, object resolvedFont)
        {
            reqFont._resolvedFont2 = resolvedFont;
        }

        /// <summary>
        /// get cached resolved-object as specific type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reqFont"></param>
        /// <returns></returns>
        public static T GetResolvedFont1<T>(ReqFontSpec reqFont)
            where T : class
        {
            return reqFont._resolvedFont1 as T;
        }
        /// <summary>
        /// get cached resolved-object as specific type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reqFont"></param>
        /// <returns></returns>
        public static T GetResolvedFont2<T>(ReqFontSpec reqFont)
           where T : class
        {
            return reqFont._resolvedFont2 as T;
        }

    }

    /// <summary>
    /// user-request font specification
    /// </summary>
    public sealed class RequestFont : ReqFontSpec
    {

        //each platform/canvas has its own representation of this Font 
        //this is just a request for specficic font presentation at a time
        //----- 
        public sealed class Choice : ReqFontSpec
        {
            public Choice(string fontFamily, float fontSizeInPts, ushort fontWeight = 400, RequestFontStyle cssFontStyle = RequestFontStyle.Regular)
                 : this(fontFamily, Len.Pt(fontSizeInPts), fontWeight, cssFontStyle)
            {

            }

            public Choice(string fontFamily, Len fontSize, ushort fontWeight = 400, RequestFontStyle cssFontStyle = RequestFontStyle.Regular)
            {
                Size = fontSize; //store user font size here 
                SizeInPoints = fontSize.ToPoints();

#if DEBUG
                if (fontFamily.Contains(",")) { throw new NotSupportedException(); }//ONLY 1 name
#endif
                Name = fontFamily.Trim(); //ONLY 1 name
                Style = cssFontStyle;
            }
            internal Choice(Len fontSize)
            {
                Size = fontSize; //store user font size here 
                SizeInPoints = fontSize.ToPoints();
            }
        }

        List<Choice> _otherChoices;

        public RequestFont(string fontFamily, float fontSizeInPts, ushort fontWeight = 400, RequestFontStyle cssFontStyle = RequestFontStyle.Regular)
            : this(fontFamily, Len.Pt(fontSizeInPts), fontWeight, cssFontStyle)
        {
        }

        public RequestFont(string fontFamily, Len fontSize, ushort fontWeight = 400, RequestFontStyle cssFontStyle = RequestFontStyle.Regular)
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



            //<family-name>
            //    The name of a font family of choice such as Helvetica or Verdana in the previous example. 
            //<generic-family>
            //    The following generic family keywords are defined: ‘serif’, ‘sans-serif’, ‘cursive’, ‘fantasy’, and ‘monospace’.
            //    These keywords can be used as a general fallback mechanism when an author's desired font choices are not available.
            //    As keywords, they must not be quoted.
            //    Authors are encouraged to append a generic font family as a last alternative for improved robustness. 

            Size = fontSize; //store user font size here 
            SizeInPoints = fontSize.ToPoints();

            //parse the font family name
            //TODO: use CSS parse code?
            string[] splitedNames = fontFamily.Split(',');

#if DEBUG
            if (splitedNames.Length == 0) { throw new NotSupportedException(); }
#endif

            Name = splitedNames[0].Trim(); //store with case sensitive (as original data)***, but search with case-insensitive
            if (splitedNames.Length > 1)
            {
                _otherChoices = new List<Choice>();
                for (int i = 1; i < splitedNames.Length; ++i)
                {
                    string name = splitedNames[i].Trim(); //store with case sensitive (as original data)***, but search with case-insensitive
                    if (name.Length == 0) { continue; }
                    _otherChoices.Add(new Choice(name, fontSize));
                }
            }

            Style = cssFontStyle;
        }

        private RequestFont(Len fontSize)
        {
            Size = fontSize; //store user font size here 
            SizeInPoints = fontSize.ToPoints();
        }

        public void AddOtherChoices(params Choice[] choices)
        {
            AddOtherChoices((IEnumerable<Choice>)choices);
        }
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


        /// <summary>
        /// create req font+ specific typeface path
        /// </summary>
        /// <param name="path">path to typeface file</param>
        /// <returns></returns>
        public static RequestFont FromFile(string path, Len len) => new RequestFont(len) { Src = path };
        public static RequestFont FromFile(string typefacePath, float sizeInPoints) => FromFile(typefacePath, Len.Pt(sizeInPoints));


        /// <summary>
        /// create req font+ specific typeface path
        /// </summary>
        /// <param name="path">path to typeface file</param>
        /// <returns></returns>
        public static Choice ChoiceFromFile(string path, Len len) => new Choice(len) { Src = path };
        public static Choice ChoiceFromFile(string typefacePath, float sizeInPoints) => ChoiceFromFile(typefacePath, Len.Pt(sizeInPoints));

    }


}