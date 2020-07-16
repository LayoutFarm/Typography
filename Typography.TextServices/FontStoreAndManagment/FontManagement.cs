//MIT, 2016-present, WinterDev 
using System;
using System.Collections.Generic;
using System.IO;
using Typography.OpenFont;
using Typography.OpenFont.Tables;
using Typography.TextBreak;

namespace Typography.FontManagement
{



    public partial class InstalledTypefaceCollection : IInstalledTypefaceProvider
    {
        class InstalledTypefaceGroup
        {
            public Dictionary<string, InstalledTypeface> _members = new Dictionary<string, InstalledTypeface>();

            public void AddFont(string registerName, InstalledTypeface installedFont)
            {
                _members.Add(registerName, installedFont);
            }
            public bool TryGetValue(string registerName, out InstalledTypeface found)
            {
                return _members.TryGetValue(registerName, out found);
            }
            public void Replace(string registerName, InstalledTypeface newone)
            {
                _members[registerName] = newone;
            }

#if DEBUG
            public string dbugGroupName;
            public override string ToString()
            {
                return dbugGroupName;
            }
#endif

        }

        /// <summary>
        /// map from font subfam to internal group name
        /// </summary>
        readonly Dictionary<string, InstalledTypefaceGroup> _subFamToFontGroup = new Dictionary<string, InstalledTypefaceGroup>();
        readonly Dictionary<string, bool> _onlyFontNames = new Dictionary<string, bool>();


        readonly InstalledTypefaceGroup _regular, _italic;
        readonly List<InstalledTypefaceGroup> _allGroups = new List<InstalledTypefaceGroup>();

        readonly Dictionary<string, InstalledTypeface> _otherFontNames = new Dictionary<string, InstalledTypeface>();
        readonly Dictionary<string, InstalledTypeface> _postScriptNames = new Dictionary<string, InstalledTypeface>();

        static InstalledTypefaceCollection s_intalledTypefaces;

        public InstalledTypefaceCollection()
        {
            //-----------------------------------------------------
            //init wellknown subfam 
            _regular = CreateCreateNewGroup(TypefaceStyle.Regular, "regular", "normal");
            _italic = CreateCreateNewGroup(TypefaceStyle.Italic, "Italic", "italique");
        }

        InstalledTypefaceGroup CreateCreateNewGroup(TypefaceStyle installedFontStyle, params string[] names)
        {
            //create font group
            var fontGroup = new InstalledTypefaceGroup();
            //single dic may be called by many names            
            foreach (string name in names)
            {
                string upperCaseName = name.ToUpper();
                //register name
                //should not duplicate 
                _subFamToFontGroup.Add(upperCaseName, fontGroup);
            }
            _allGroups.Add(fontGroup);
            return fontGroup;
        }

     


        bool Register(InstalledTypeface instTypeface)
        {
            InstalledTypefaceGroup selectedFontGroup = null;
            string fontSubFamUpperCaseName = instTypeface.TypographicFontSubFamily;
            bool use_typographicSubFam = true;
            if (fontSubFamUpperCaseName == null)
            {
                //switch to FontSubFamily, this should not be null!
                fontSubFamUpperCaseName = instTypeface.FontSubFamily;
                use_typographicSubFam = false;
            }
            fontSubFamUpperCaseName = fontSubFamUpperCaseName.ToUpper();
            //--------------

            switch (instTypeface.TypefaceStyle)
            {
                default:
                    {

                        if (!_subFamToFontGroup.TryGetValue(fontSubFamUpperCaseName, out selectedFontGroup))
                        {
                            //create new group, we don't known this font group before 
                            //so we add to 'other group' list
                            selectedFontGroup = new InstalledTypefaceGroup();
#if DEBUG
                            selectedFontGroup.dbugGroupName = fontSubFamUpperCaseName;
#endif
                            _subFamToFontGroup.Add(fontSubFamUpperCaseName, selectedFontGroup);
                            _allGroups.Add(selectedFontGroup);

                        }
                    }
                    break;

                case TypefaceStyle.Italic:
                    selectedFontGroup = _italic;
                    break;
                case TypefaceStyle.Regular:
                    {
                        selectedFontGroup = _regular;

                        if (fontSubFamUpperCaseName != "REGULAR" &&
                            !_subFamToFontGroup.TryGetValue(fontSubFamUpperCaseName, out selectedFontGroup))
                        {
                            //create new group, we don't known this font group before 
                            //so we add to 'other group' list
                            selectedFontGroup = new InstalledTypefaceGroup();
#if DEBUG
                            selectedFontGroup.dbugGroupName = fontSubFamUpperCaseName;
#endif
                            _subFamToFontGroup.Add(fontSubFamUpperCaseName, selectedFontGroup);
                            _allGroups.Add(selectedFontGroup);
                        }

                    }
                    break;
            }

            //------------------
            //for font management
            //we use 'typographic family name' if avaliable,            
            string register_name = instTypeface.TypographicFamilyName;
            bool use_typographicFontFam = true;
            if (register_name == null)
            {
                //switch to font name, this should not be null!
                register_name = instTypeface.FontName;
                use_typographicFontFam = false;
            }

            register_name = register_name.ToUpper(); //***  
            bool register_result = false;

            if (selectedFontGroup.TryGetValue(register_name, out InstalledTypeface found))
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
                            selectedFontGroup.Replace(register_name, instTypeface);
                            register_result = true;
                            break;
                    }
                }
            }
            else
            {
                selectedFontGroup.AddFont(register_name, instTypeface);
                register_result = true;
            }

            if (use_typographicFontFam &&
                instTypeface.FontName != instTypeface.TypographicFamilyName &&
                instTypeface.TypefaceStyle == TypefaceStyle.Regular)
            {
                //in this case, the code above register the typeface with TypographicFamilyName
                //so we register this typeface with original name too
                if (_otherFontNames.ContainsKey(instTypeface.FontName.ToUpper()))
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("duplicated font name?:" + instTypeface.FontName.ToUpper());
#endif
                }
                else
                {
                    _otherFontNames.Add(instTypeface.FontName.ToUpper(), instTypeface);
                }
            }


            if (instTypeface.PostScriptName != null)
            {
                string postScriptName = instTypeface.PostScriptName.ToUpper();
                if (!_postScriptNames.ContainsKey(postScriptName))
                {
                    _postScriptNames.Add(postScriptName, instTypeface);
                }
                else
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("duplicated postscriptname:" + postScriptName);
#endif
                }
            }


            GetInstalledTypefaceByWeightClass(instTypeface.WeightClass).Add(instTypeface);


            if (register_result && instTypeface.FontPath != null &&
                !_installedTypefacesByFilenames.ContainsKey(instTypeface.FontPath)) //beware case-sensitive!
            {
                _installedTypefacesByFilenames.Add(instTypeface.FontPath, instTypeface);
            }
            return register_result;
        }

        public InstalledTypeface GetFontByPostScriptName(string postScriptName)
        {
            _postScriptNames.TryGetValue(postScriptName.ToUpper(), out InstalledTypeface found);
            return found;
        }
       
        public InstalledTypeface GetInstalledTypeface(string fontName, string subFamName, ushort weight)
        {
            string upperCaseFontName = fontName.ToUpper();
            string upperCaseSubFamName = subFamName.ToUpper();


            //find font group  
            if (_subFamToFontGroup.TryGetValue(upperCaseSubFamName, out InstalledTypefaceGroup foundFontGroup) &&
                foundFontGroup.TryGetValue(upperCaseFontName, out InstalledTypeface foundInstalledFont))
            {
                return foundInstalledFont;
            }

            //
            if (_otherFontNames.TryGetValue(upperCaseFontName, out foundInstalledFont))
            {
                return foundInstalledFont;
            }
            if (upperCaseSubFamName == "")
            {
                //eg OTHERS...
                foreach (var kv in _subFamToFontGroup)
                {
                    switch (kv.Key)
                    {
                        case "BOLD ITALIC":
                        case "BOLD":
                        case "ITALIC":
                        case "ITALIQUE":
                        case "REGULAR": continue;
                        default:
                            {
                                if (kv.Value.TryGetValue(upperCaseFontName, out foundInstalledFont))
                                {
                                    return foundInstalledFont;
                                }
                            }
                            break;
                    }
                }

            }


            //not found
            if (_fontNotFoundHandler != null)
            {
                throw new NotSupportedException();
                //return _fontNotFoundHandler(this, fontName, subFamName);
            }

            return null; //not found
        }
        public InstalledTypeface GetInstalledTypeface(string fontName, TypefaceStyle wellknownSubFam, ushort weight)
        {
            //not auto resolve
            InstalledTypefaceGroup selectedFontGroup;

            switch (wellknownSubFam)
            {
                default: return null;
                case TypefaceStyle.Regular: selectedFontGroup = _regular; break;
                case TypefaceStyle.Italic: selectedFontGroup = _italic; break;
            }
            if (selectedFontGroup.TryGetValue(fontName.ToUpper(), out InstalledTypeface found))
            {
                return found;
            }
            //------------------------------------------- 
            //not found then ...


            //retry ....
            //if (wellknownSubFam == TypefaceStyle.Bold)
            //{
            //    //try get from Gras?
            //    //eg. tahoma
            //    if (_subFamToFontGroup.TryGetValue("GRAS", out selectedFontGroup))
            //    {

            //        if (selectedFontGroup.TryGetValue(fontName.ToUpper(), out _found))
            //        {
            //            return _found;
            //        }

            //    }
            //}
            //else if (wellknownSubFam == TypefaceStyle.Italic)
            //{
            //    //TODO: simulate oblique (italic) font???
            //    selectedFontGroup = _normal;

            //    if (selectedFontGroup.TryGetValue(fontName.ToUpper(), out _found))
            //    {
            //        return _found;
            //    }
            //}

            if (found == null && _fontNotFoundHandler != null)
            {
                throw new NotSupportedException();
                //return _fontNotFoundHandler(this, fontName, GetSubFam(wellknownSubFam));
            }
            return found;
        }


        internal static string GetSubFam(TypefaceStyle typefaceStyle)
        {
            switch (typefaceStyle)
            {
                case TypefaceStyle.Italic: return "ITALIC";
                case TypefaceStyle.Regular: return "REGULAR";
            }
            return "";
        }


        public IEnumerable<InstalledTypeface> GetInstalledFontIter()
        {
            foreach (InstalledTypefaceGroup fontgroup in _allGroups)
            {
                foreach (InstalledTypeface f in fontgroup._members.Values)
                {
                    yield return f;
                }
            }
        }


        public IEnumerable<string> GetFontNameIter() => _onlyFontNames.Keys;
        public IEnumerable<InstalledTypeface> GetInstalledTypefaceIter(string fontName)
        {
            fontName = fontName.ToUpper();
            foreach (InstalledTypefaceGroup typefaceGroup in _subFamToFontGroup.Values)
            {
                if (typefaceGroup.TryGetValue(fontName, out InstalledTypeface found))
                {
                    yield return found;
                }
            }
        }

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
                                if (instFont.ContainGlyphForUnicode(rng.StarCodepoint))
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