//MIT, 2016-present, WinterDev 
using System;
using System.Collections.Generic;


namespace Typography.FontManagement
{
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

#if DEBUG
            public override string ToString()
            {
                return FontName;
            }
#endif
        }

         

        readonly Dictionary<string, InstalledTypefaceGroup> _typefaceGroups = new Dictionary<string, InstalledTypefaceGroup>();

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
            register_name = register_name.ToUpper() + "," + instTypeface.TypefaceStyle + "," + instTypeface.WeightClass; //***   
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

            string fontName = instTypeface.FontName.ToUpper();//MUST not be null

            //-----
            if (!_typefaceGroups.TryGetValue(fontName, out InstalledTypefaceGroup found1))
            {
                found1 = new InstalledTypefaceGroup(fontName, instTypeface);
                _typefaceGroups.Add(fontName, found1);
            }
            else
            {
                found1.AddInstalledTypeface(instTypeface);
            }

            //-----
            string typographicName = instTypeface.TypographicFamilyName?.ToUpper();
            if (typographicName != null && typographicName != fontName)
            {
                if (!_typefaceGroups.TryGetValue(typographicName, out InstalledTypefaceGroup found2))
                {
                    found2 = new InstalledTypefaceGroup(typographicName, instTypeface);
                    _typefaceGroups.Add(typographicName, found2);
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
                if (!_typefaceGroups.TryGetValue(postScriptName, out InstalledTypefaceGroup found2))
                {
                    found2 = new InstalledTypefaceGroup(postScriptName, instTypeface);
                    _typefaceGroups.Add(postScriptName, found2);
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
            if (_typefaceGroups.TryGetValue(upper, out InstalledTypefaceGroup found))
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
    }


}