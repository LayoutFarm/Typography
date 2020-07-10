//MIT, 2020-present, WinterDev
using System;
using System.Collections.Generic;

using Typography.OpenFont;
using Typography.TextBreak;
using Typography.FontManagement;

using PixelFarm.Drawing;

namespace Typography.TextServices
{
    public class MyAlternativeTypefaceSelector : AlternativeTypefaceSelector
    {
        readonly Dictionary<string, PreferredTypefaceList> _dics = new Dictionary<string, PreferredTypefaceList>();
        PreferredTypefaceList _emojiPreferList = new PreferredTypefaceList();

#if DEBUG
        public MyAlternativeTypefaceSelector()
        {
        }
#endif

        public void SetPreferredTypefaces(UnicodeRangeInfo unicodeRangeInfo, PreferredTypefaceList typefaceNames)
        {
            _dics[unicodeRangeInfo.Name] = typefaceNames;
        }
        public void SetPreferredTypefaces(UnicodeRangeInfo[] unicodeRangeInfos, PreferredTypefaceList typefaceNames)
        {
            for (int i = 0; i < unicodeRangeInfos.Length; ++i)
            {
                _dics[unicodeRangeInfos[i].Name] = typefaceNames;
            }

        }
        public void SetPerferredEmoji(PreferredTypefaceList typefaceNames)
        {
            _emojiPreferList = typefaceNames;
        }

        public PreferredTypefaceList GetPreferTypefaces(string scriptTag) => _dics.TryGetValue(scriptTag, out PreferredTypefaceList foundList) ? foundList : null;

        RequestFont _reqFont;
        Typography.TextServices.OpenFontTextService _textService;
        public void SetCurrentReqFont(RequestFont reqFont, Typography.TextServices.OpenFontTextService textService)
        {
            _reqFont = reqFont;
            _textService = textService;
        }

        public override SelectedTypeface Select(List<InstalledTypeface> choices, UnicodeRangeInfo unicodeRangeInfo, int hintCodePoint)
        {
            //request font may have hint for typeface 
            if (_reqFont != null)
            {
                for (int i = 0; i < _reqFont.OtherChoicesCount; ++i)
                {
                    RequestFont.Choice choice = _reqFont.GetOtherChoice(i);
                    ResolvedFont resolvedFont = _textService.ResolveFont(choice);
                    //check if resolvedFont support specific unicodeRange info or not 
                    Typeface typeface = resolvedFont.Typeface;
                    ushort codepoint = typeface.GetGlyphIndex(unicodeRangeInfo.StarCodepoint);
                    if (codepoint > 0)
                    {
                        //use this
                        return new SelectedTypeface(typeface);
                    }
                }
            }

            List<PreferTypeface> list = null;
            if (unicodeRangeInfo == Unicode13RangeInfoList.Emoticons)
            {
                list = _emojiPreferList._list;
            }
            else if (_dics.TryGetValue(unicodeRangeInfo.Name, out PreferredTypefaceList foundList))
            {
                list = foundList._list;
            }

            if (list != null)
            {
                int j = list.Count;
                for (int i = 0; i < j; ++i)
                {
                    //select that first one
                    PreferTypeface p = list[i];

                    if (p.InstalledTypeface == null && !p.ResolvedInstalledTypeface)
                    {
                        //find
                        int choice_count = choices.Count;

                        for (int m = 0; m < choice_count; ++m)
                        {
                            InstalledTypeface instTypeface = choices[m];
                            if (p.RequestTypefaceName == instTypeface.FontName)
                            {
                                //TODO: review here again
                                p.InstalledTypeface = instTypeface;

                                break;
                            }
                        }
                        p.ResolvedInstalledTypeface = true;
                    }
                    //-------
                    if (p.InstalledTypeface != null)
                    {
                        return new SelectedTypeface(p.InstalledTypeface);
                    }
                }
            }

            //still not found
            if (choices.Count > 0)
            {
                //choose default
                return new SelectedTypeface(choices[0]);
            }


            return new SelectedTypeface();//empty
        }

    }
    public class PreferTypeface
    {
        public PreferTypeface(string reqTypefaceName) => RequestTypefaceName = reqTypefaceName;
        public string RequestTypefaceName { get; }
        public InstalledTypeface InstalledTypeface { get; internal set; }
        internal bool ResolvedInstalledTypeface { get; set; }
    }
    public class PreferredTypefaceList
    {
        internal List<PreferTypeface> _list = new List<PreferTypeface>();
        public void AddTypefaceName(string typefaceName)
        {
            _list.Add(new PreferTypeface(typefaceName));
        }
    }
}