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
        

        readonly InstalledTypefaceGroup _regular, _italic;
        readonly List<InstalledTypefaceGroup> _allGroups = new List<InstalledTypefaceGroup>();
         
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

//            if (use_typographicFontFam &&
//                instTypeface.FontName != instTypeface.TypographicFamilyName &&
//                instTypeface.TypefaceStyle == TypefaceStyle.Regular)
//            {
//                //in this case, the code above register the typeface with TypographicFamilyName
//                //so we register this typeface with original name too
//                if (_otherFontNames.ContainsKey(instTypeface.FontName.ToUpper()))
//                {
//#if DEBUG
//                    System.Diagnostics.Debug.WriteLine("duplicated font name?:" + instTypeface.FontName.ToUpper());
//#endif
//                }
//                else
//                {
//                    _otherFontNames.Add(instTypeface.FontName.ToUpper(), instTypeface);
//                }
//            }


//            if (instTypeface.PostScriptName != null)
//            {
//                string postScriptName = instTypeface.PostScriptName.ToUpper();
//                if (!_postScriptNames.ContainsKey(postScriptName))
//                {
//                    _postScriptNames.Add(postScriptName, instTypeface);
//                }
//                else
//                {
//#if DEBUG
//                    System.Diagnostics.Debug.WriteLine("duplicated postscriptname:" + postScriptName);
//#endif
//                }
//            }


            GetInstalledTypefaceByWeightClass(instTypeface.WeightClass).Add(instTypeface);


            if (register_result && instTypeface.FontPath != null &&
                !_installedTypefacesByFilenames.ContainsKey(instTypeface.FontPath)) //beware case-sensitive!
            {
                _installedTypefacesByFilenames.Add(instTypeface.FontPath, instTypeface);
            }
            return register_result;
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
        
    }


}