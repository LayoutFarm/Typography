//MIT, 2016-present, WinterDev 
using System;
using System.IO;
using System.Collections.Generic;
using Typography.OpenFont;
using Typography.OpenFont.Tables;
using Typography.TextBreak;

namespace Typography.FontCollection
{

    //=======
    //[PART1]
    public partial class InstalledTypefaceCollection : IInstalledTypefaceProvider
    {

        public class InstalledTypefaceGroup
        {
            readonly InstalledTypeface _first;
            List<InstalledTypeface> _others;
            internal InstalledTypefaceGroup(string fontname, InstalledTypeface first)
            {
                _first = first;
                FontName = fontname;
            }
            internal void AddInstalledTypeface(InstalledTypeface other)
            {
                if (_others == null) { _others = new List<InstalledTypeface>(); }
                _others.Add(other);
            }
            public string FontName { get; }

            internal void CollectCandidateFont(TypefaceStyle style, ushort weight, List<InstalledTypeface> candidates)
            {
                if ((ushort)_first.WeightClass == weight && _first.TypefaceStyle == style)
                {
                    candidates.Add(_first);
                }

                if (_others != null)
                {
                    int j = _others.Count;
                    for (int i = 0; i < j; ++i)
                    {
                        InstalledTypeface inst = _others[i];
                        if ((ushort)inst.WeightClass == weight && inst.TypefaceStyle == style)
                        {
                            candidates.Add(inst);
                        }
                    }
                }
            }

            public int Count => (_others != null) ? _others.Count + 1 : 1;

            public InstalledTypeface GetInstalledTypeface(int index)
            {
                index--;
                if (index == -1)
                {
                    return _first;
                }
                else if (_others != null && index >= 0 && index < _others.Count)
                {
                    return _others[index];
                }
                //out-of-range
                throw new NotSupportedException();
            }

            public IEnumerable<InstalledTypeface> GetMemberIter()
            {
                yield return _first;
                if (_others != null)
                {
                    for (int i = 0; i < _others.Count; ++i)
                    {
                        yield return _others[i];
                    }
                }
            }
#if DEBUG
            public override string ToString()
            {
                return FontName;
            }
#endif
        }


        readonly Dictionary<string, InstalledTypefaceGroup> _regNames = new Dictionary<string, InstalledTypefaceGroup>();

        //others
        readonly Dictionary<string, InstalledTypefaceGroup> _otherNames = new Dictionary<string, InstalledTypefaceGroup>();

        readonly Dictionary<string, InstalledTypeface> _all3 = new Dictionary<string, InstalledTypeface>();

#if DEBUG
        public InstalledTypefaceCollection()
        {

        }
#endif
        bool Register(InstalledTypeface instTypeface)
        {
            //[A] ---------------------------------------
            string register_name = instTypeface.TypographicFamilyName;
            //use typographic name first
            if (register_name == null)
            {
                //switch to font name, this should not be null!
                register_name = instTypeface.FontName;
            }

            string reg_name_only = register_name.ToUpper();

            register_name = reg_name_only + "," + instTypeface.TypefaceStyle + "," + instTypeface.WeightClass; //***   
            bool register_result = false;
            if (_all3.TryGetValue(register_name, out InstalledTypeface found))
            {
                //TODO:
                //we already have this font name
                //(but may be different file
                //we let user to handle it        
                if (_fontNameDuplicatedHandler != null)
                {
                    switch (_fontNameDuplicatedHandler(found, instTypeface))
                    {
                        default:
                            throw new NotSupportedException();
                        case FontNameDuplicatedDecision.Skip:
                            break;
                        case FontNameDuplicatedDecision.Replace:
                            //selectedFontGroup.Replace(register_name, instTypeface);
                            _all3[register_name] = instTypeface;
                            register_result = true;
                            break;
                    }
                }
            }
            else
            {
                _all3.Add(register_name, instTypeface);
                register_result = true;
            }


            if (!register_result) { return false; }//early exit


            //[B]---------------------------------------
            //register other names...



            if (!_regNames.TryGetValue(reg_name_only, out InstalledTypefaceGroup regNameGroup))
            {
                regNameGroup = new InstalledTypefaceGroup(reg_name_only, instTypeface);
                _regNames.Add(reg_name_only, regNameGroup);
            }
            else
            {
                regNameGroup.AddInstalledTypeface(instTypeface);
            }


            string fontName = instTypeface.FontName.ToUpper();
            if (fontName != null && fontName != reg_name_only)
            {
                if (!_otherNames.TryGetValue(fontName, out InstalledTypefaceGroup found2))
                {
                    found2 = new InstalledTypefaceGroup(fontName, instTypeface);
                    _otherNames.Add(fontName, found2);
                }
                else
                {
                    found2.AddInstalledTypeface(instTypeface);
                }
            }

            //-----
            string typographicName = instTypeface.TypographicFamilyName?.ToUpper();
            if (typographicName != null && typographicName != reg_name_only && typographicName != fontName)
            {
                if (!_otherNames.TryGetValue(typographicName, out InstalledTypefaceGroup found2))
                {
                    found2 = new InstalledTypefaceGroup(typographicName, instTypeface);
                    _otherNames.Add(typographicName, found2);
                }
                else
                {
                    found2.AddInstalledTypeface(instTypeface);
                }
            }
            //-----
            string postScriptName = instTypeface.PostScriptName?.ToUpper();
            if (postScriptName != null && postScriptName != fontName && postScriptName != typographicName)
            {
                if (!_otherNames.TryGetValue(postScriptName, out InstalledTypefaceGroup found2))
                {
                    found2 = new InstalledTypefaceGroup(postScriptName, instTypeface);
                    _otherNames.Add(postScriptName, found2);
                }
                else
                {
                    found2.AddInstalledTypeface(instTypeface);
                }
            }


            //classified by its weight 
            GetInstalledTypefaceByWeightClass(instTypeface.WeightClass).Add(instTypeface);


            //register by its path (if available)
            if (instTypeface.FontPath != null &&
             !_installedTypefacesByFilenames.ContainsKey(instTypeface.FontPath)) //beware case-sensitive!
            {
                _installedTypefacesByFilenames.Add(instTypeface.FontPath, instTypeface);
            }

            return true;
        }


        readonly List<InstalledTypeface> _candidates = new List<InstalledTypeface>();
        public InstalledTypeface GetInstalledTypeface(string fontName, TypefaceStyle wellknownSubFam, ushort weight)
        {

            _candidates.Clear();
            string upper = fontName.Trim().ToUpper();

            if (_regNames.TryGetValue(upper, out InstalledTypefaceGroup found))
            {
                found.CollectCandidateFont(wellknownSubFam, weight, _candidates);

                if (_candidates.Count == 1)
                {
                    //select most proper***
                    //the last one              
                    return _candidates[0];
                }
                else if (_candidates.Count > 1)
                {
                    //TODO: 
                    //more than 1,
                    //TODO: ask user for most proper one

                    return _candidates[_candidates.Count - 1];
                }
            }


            if (_otherNames.TryGetValue(upper, out found))
            {
                found.CollectCandidateFont(wellknownSubFam, weight, _candidates);

                if (_candidates.Count == 1)
                {
                    //select most proper***
                    //the last one              
                    return _candidates[0];
                }
                else if (_candidates.Count > 1)
                {
                    //TODO: 
                    //more than 1,
                    //TODO: ask user for most proper one

                    return _candidates[_candidates.Count - 1];
                }
            }
            return _fontNotFoundHandler?.Invoke(this, upper, wellknownSubFam, weight, null, null);
        }

        public IEnumerable<InstalledTypefaceGroup> GetInstalledTypefaceGroupIter()
        {
            foreach (InstalledTypefaceGroup g in _regNames.Values)
            {
                yield return g;
            }
        }

    }



    //============
    //PART2

    partial class InstalledTypefaceCollection
    {
        static InstalledTypefaceCollection s_intalledTypefaces;
        //----------
        //common weight classes
        internal readonly List<InstalledTypeface> _weight100_Thin = new List<InstalledTypeface>();
        internal readonly List<InstalledTypeface> _weight200_Extralight = new List<InstalledTypeface>(); //Extra-light (Ultra-light)
        internal readonly List<InstalledTypeface> _weight300_Light = new List<InstalledTypeface>();
        internal readonly List<InstalledTypeface> _weight400_Normal = new List<InstalledTypeface>();
        internal readonly List<InstalledTypeface> _weight500_Medium = new List<InstalledTypeface>();
        internal readonly List<InstalledTypeface> _weight600_SemiBold = new List<InstalledTypeface>(); //Semi-bold (Demi-bold)
        internal readonly List<InstalledTypeface> _weight700_Bold = new List<InstalledTypeface>(); //Semi-bold (Demi-bold)
        internal readonly List<InstalledTypeface> _weight800_ExtraBold = new List<InstalledTypeface>(); //Extra-bold (Ultra-bold)
        internal readonly List<InstalledTypeface> _weight900_Black = new List<InstalledTypeface>(); //Black (Heavy)

        //and others
        internal readonly List<InstalledTypeface> _otherWeightClassTypefaces = new List<InstalledTypeface>();
        //----------
        internal Dictionary<string, InstalledTypeface> _installedTypefacesByFilenames = new Dictionary<string, InstalledTypeface>();


        FontNameDuplicatedHandler _fontNameDuplicatedHandler;
        FontNotFoundHandler _fontNotFoundHandler;

        public void SetFontNameDuplicatedHandler(FontNameDuplicatedHandler handler)
        {
            _fontNameDuplicatedHandler = handler;
        }
        public void SetFontNotFoundHandler(FontNotFoundHandler fontNotFoundHandler)
        {
            _fontNotFoundHandler = fontNotFoundHandler;
        }
        public static InstalledTypefaceCollection GetSharedTypefaceCollection(FirstInitFontCollectionDelegate initdel)
        {
            if (s_intalledTypefaces == null)
            {
                //first time
                s_intalledTypefaces = new InstalledTypefaceCollection();
                initdel(s_intalledTypefaces);
            }
            return s_intalledTypefaces;
        }
        public static void SetAsSharedTypefaceCollection(InstalledTypefaceCollection installedTypefaceCollection) => s_intalledTypefaces = installedTypefaceCollection;

        public static InstalledTypefaceCollection GetSharedTypefaceCollection() => s_intalledTypefaces;
        public bool AddFontStreamSource(IFontStreamSource src)
        {
            //preview data of font
            try
            {
                using (Stream stream = src.ReadFontStream())
                {
                    var reader = new OpenFontReader();
                    PreviewFontInfo previewFont = reader.ReadPreview(stream);
                    if (previewFont == null || string.IsNullOrEmpty(previewFont.Name))
                    {
                        //err!
                        return false;
                    }
                    if (previewFont.IsFontCollection)
                    {
                        int mbCount = previewFont.MemberCount;
                        bool totalResult = true;
                        for (int i = 0; i < mbCount; ++i)
                        {
                            //extract and each members
                            InstalledTypeface instTypeface = AddFontPreview(previewFont.GetMember(i), src.PathName);
                            if (instTypeface == null)
                            {
                                totalResult = false;
                            }

                        }
                        return totalResult;
                    }
                    else
                    {
                        return AddFontPreview(previewFont, src.PathName) != null;
                    }

                }
            }
            catch (IOException)
            {
                //TODO review here again
                return false;
            }
        }
        public InstalledTypeface AddFontPreview(PreviewFontInfo previewFont, string srcPath)
        {

            InstalledTypeface installedTypeface = new InstalledTypeface(
                previewFont,
                srcPath)
            { ActualStreamOffset = previewFont.ActualStreamOffset };


            return Register(installedTypeface) ? installedTypeface : null;
        }
        public IEnumerable<InstalledTypeface> GetInstalledFontIter()
        {
            foreach (InstalledTypeface f in _all3.Values)
            {
                yield return f;
            }
        }
        readonly Dictionary<UnicodeRangeInfo, List<InstalledTypeface>> _registerWithUnicodeRangeDic = new Dictionary<UnicodeRangeInfo, List<InstalledTypeface>>();
        readonly List<InstalledTypeface> _emojiSupportedTypefaces = new List<InstalledTypeface>();
        readonly List<InstalledTypeface> _mathTypefaces = new List<InstalledTypeface>();

        //unicode 13:
        //https://unicode.org/emoji/charts/full-emoji-list.html
        //emoji start at U+1F600 	
        const int UNICODE_EMOJI_START = 0x1F600; //"😁" //first emoji
        const int UNICODE_EMOJI_END = 0x1F64F;

        //https://www.unicode.org/charts/PDF/U1D400.pdf
        const int UNICODE_MATH_ALPHANUM_EXAMPLE = 0x1D400; //1D400–1D7FF;
        List<InstalledTypeface> GetInstalledTypefaceByWeightClass(ushort weightClass)
        {
            switch (weightClass)
            {
                default: return _otherWeightClassTypefaces;
                case 100: return _weight100_Thin;
                case 200: return _weight200_Extralight;
                case 300: return _weight300_Light;
                case 400: return _weight400_Normal;
                case 500: return _weight500_Medium;
                case 600: return _weight600_SemiBold;
                case 700: return _weight700_Bold;
                case 800: return _weight800_ExtraBold;
                case 900: return _weight900_Black;
            }
        }

        public IEnumerable<InstalledTypeface> GetInstalledTypefaceIterByWeightClassIter(ushort weightClass)
        {
            return GetInstalledTypefaceByWeightClass(weightClass);
        }

        List<InstalledTypeface> GetExisitingOrCreateNewListForUnicodeRange(UnicodeRangeInfo range)
        {
            if (!_registerWithUnicodeRangeDic.TryGetValue(range, out List<InstalledTypeface> found))
            {
                found = new List<InstalledTypeface>();
                _registerWithUnicodeRangeDic.Add(range, found);
            }
            return found;
        }
        public void UpdateUnicodeRanges()
        {

            _registerWithUnicodeRangeDic.Clear();
            _emojiSupportedTypefaces.Clear();
            _mathTypefaces.Clear();

            foreach (InstalledTypeface instFont in GetInstalledFontIter())
            {
                foreach (BitposAndAssciatedUnicodeRanges bitposAndAssocUnicodeRanges in instFont.GetSupportedUnicodeLangIter())
                {
                    foreach (UnicodeRangeInfo range in bitposAndAssocUnicodeRanges.Ranges)
                    {

                        List<InstalledTypeface> typefaceList = GetExisitingOrCreateNewListForUnicodeRange(range);
                        typefaceList.Add(instFont);
                        //----------------
                        //sub range
                        if (range == BitposAndAssciatedUnicodeRanges.None_Plane_0)
                        {
                            //special search
                            //TODO: review here again
                            foreach (UnicodeRangeInfo rng in Unicode13RangeInfoList.GetNonePlane0Iter())
                            {
                                if (instFont.ContainGlyphForUnicode(rng.StartCodepoint))
                                {
                                    typefaceList = GetExisitingOrCreateNewListForUnicodeRange(rng);
                                    typefaceList.Add(instFont);
                                }
                            }
                            if (instFont.ContainGlyphForUnicode(UNICODE_EMOJI_START))
                            {
                                _emojiSupportedTypefaces.Add(instFont);
                            }
                            if (instFont.ContainGlyphForUnicode(UNICODE_MATH_ALPHANUM_EXAMPLE))
                            {
                                _mathTypefaces.Add(instFont);
                            }
                        }
                    }
                }
            }
            //------
            //select perfer unicode font

        }
        /// <summary>
        /// get alternative typeface from a given unicode codepoint
        /// </summary>
        /// <param name="codepoint"></param>
        /// <param name="selector"></param>
        /// <param name="found"></param>
        /// <returns></returns>
        public bool TryGetAlternativeTypefaceFromCodepoint(int codepoint, AltTypefaceSelectorBase selector, out Typeface selectedTypeface)
        {
            //find a typeface that supported input char c

            List<InstalledTypeface> installedTypefaceList = null;
            if (ScriptLangs.TryGetUnicodeRangeInfo(codepoint, out UnicodeRangeInfo unicodeRangeInfo))
            {
                if (_registerWithUnicodeRangeDic.TryGetValue(unicodeRangeInfo, out List<InstalledTypeface> typefaceList) &&
                    typefaceList.Count > 0)
                {
                    //select a proper typeface                        
                    installedTypefaceList = typefaceList;
                }
            }


            //not found
            if (installedTypefaceList == null && codepoint >= UNICODE_EMOJI_START && codepoint <= UNICODE_EMOJI_END)
            {
                unicodeRangeInfo = Unicode13RangeInfoList.Emoticons;
                if (_emojiSupportedTypefaces.Count > 0)
                {
                    installedTypefaceList = _emojiSupportedTypefaces;
                }
            }
            //-------------
            if (installedTypefaceList != null)
            {
                //select a prefer font 
                if (selector != null)
                {
                    AltTypefaceSelectorBase.SelectedTypeface result = selector.Select(installedTypefaceList, unicodeRangeInfo, codepoint);
                    if (result.InstalledTypeface != null)
                    {
                        selectedTypeface = this.ResolveTypeface(result.InstalledTypeface);
                        return selectedTypeface != null;
                    }
                    else if (result.Typeface != null)
                    {
                        selectedTypeface = result.Typeface;
                        return true;
                    }
                    else
                    {
                        selectedTypeface = null;
                        return false;
                    }
                }
                else if (installedTypefaceList.Count > 0)
                {
                    InstalledTypeface instTypeface = installedTypefaceList[0];//default
                    selectedTypeface = this.ResolveTypeface(installedTypefaceList[0]);
                    return selectedTypeface != null;
                }
            }

            selectedTypeface = null;
            return false;
        }

    }


}