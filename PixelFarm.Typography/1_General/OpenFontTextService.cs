//MIT, 2014-present, WinterDev
using System;
using System.Collections.Generic;

using Typography.OpenFont;
using Typography.FontManagement;

using PixelFarm.Drawing;

namespace Typography.Text
{

    public partial class OpenFontTextService
    {


        readonly Dictionary<int, ResolvedFont> _resolvedTypefaceCache = new Dictionary<int, ResolvedFont>(); //similar to TypefaceStore
        //
        public static ScriptLang DefaultScriptLang { get; set; }

        readonly InstalledTypefaceCollection _installedTypefaceCollection;

        public OpenFontTextService(InstalledTypefaceCollection installedTypefaceCollection)
        {
            _installedTypefaceCollection = installedTypefaceCollection;
            TrySetupCurrentScriptLang();
        }
        public OpenFontTextService()
        {

            //default,
            _installedTypefaceCollection = InstalledTypefaceCollection.GetSharedTypefaceCollection(collection =>
            {
                //collection.SetFontNameDuplicatedHandler((f0, f1) => FontNameDuplicatedDecision.Skip);
            });

            TrySetupCurrentScriptLang();
        }
        void TrySetupCurrentScriptLang()
        {
            //set script-lang 
            ScriptLang scLang = DefaultScriptLang;
            //---------------
            //if not default then try guess
            //
            if (scLang.scriptTag == 0 &&
                !TryGetScriptLangFromCurrentThreadCultureInfo(out scLang))
            {
                //TODO: handle error here

                throw new NotSupportedException();
            }
        }


        public void LoadSystemFonts() => _installedTypefaceCollection.LoadSystemFonts();

        public void LoadFontsFromFolder(string folder) => _installedTypefaceCollection.LoadFontsFromFolder(folder);

        public void UpdateUnicodeRanges() => _installedTypefaceCollection.UpdateUnicodeRanges();


        static readonly ScriptLang s_latin = new ScriptLang(ScriptTagDefs.Latin.Tag);
        static bool TryGetScriptLangFromCurrentThreadCultureInfo(out Typography.OpenFont.ScriptLang scLang)
        {
            var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

            if (Typography.TextBreak.IcuData.TryGetFullLanguageNameFromLangCode(
                 currentCulture.TwoLetterISOLanguageName,
                 currentCulture.ThreeLetterISOLanguageName,
                 out string langFullName))
            {
                Typography.OpenFont.ScriptLangInfo scLang1 = Typography.OpenFont.ScriptLangs.GetRegisteredScriptLangFromLanguageName(langFullName);
                if (scLang1 == null)
                {
                    //not found -> use default latin
                    //use default lang
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(langFullName + " :use latin");
#endif
                    scLang = s_latin;
                    return true;
                }
                else
                {
                    scLang = new ScriptLang(scLang1.shortname);// scLang1.GetScriptLang();
                    return true;
                }
            }
            else
            {
                scLang = default;
            }
            return false;
        }

        /// <summary>
        /// get alternative typeface from a given unicode codepoint
        /// </summary>
        /// <param name="codepoint"></param>
        /// <param name="selector"></param>
        /// <param name="found"></param>
        /// <returns></returns>
        public bool TryGetAlternativeTypefaceFromCodepoint(int codepoint, AltTypefaceSelectorBase selector, out Typeface found) => _installedTypefaceCollection.TryGetAlternativeTypefaceFromCodepoint(codepoint, selector, out found);

        public ResolvedFont ResolveFont(RequestFont.Choice choice)
        {
            ResolvedFont resolvedFont = RequestFont.Choice.GetResolvedFont1<ResolvedFont>(choice);
            if (resolvedFont != null) return resolvedFont;

            Typeface typeface;
            if (choice.FromTypefaceFile)
            {
                //this may not be loaded
                //so check if we have that file or not
                typeface = _installedTypefaceCollection.ResolveTypefaceFromFile(choice.UserInputTypefaceFile);
                if (typeface != null)
                {
                    //found
                    //TODO: handle FontStyle ***                    
                    resolvedFont = new ResolvedFont(typeface, choice.SizeInPoints);
                    RequestFont.Choice.SetResolvedFont1(choice, resolvedFont);
                    return resolvedFont;
                }
            }

            //cache level-2 (stored in this openfont service)
            if (_resolvedTypefaceCache.TryGetValue(choice.GetFontKey(), out resolvedFont))
            {
                if (resolvedFont.Typeface == null)
                {
                    //this is 'not found' resovled font
                    //so don't return it
                    return null;
                }
                //----
                //cache to level-1
                RequestFont.Choice.SetResolvedFont1(choice, resolvedFont);
                return resolvedFont;
            }
            //-----
            //when not found
            //find it 
            if ((typeface = _installedTypefaceCollection.ResolveTypeface(choice.Name,
                             PixelFarm.Drawing.FontStyleExtensions.ConvToInstalledFontStyle(choice.Style),
                             choice.WeightClass)) != null)
            {
                //NOT NULL=> found 
                if (!_resolvedTypefaceCache.TryGetValue(choice.GetFontKey(), out resolvedFont))
                {
                    resolvedFont = new ResolvedFont(typeface, choice.SizeInPoints, choice.GetFontKey());

                    //** cache it with otherChoice.GetFontKey()**
                    _resolvedTypefaceCache.Add(choice.GetFontKey(), resolvedFont);
                }
                return resolvedFont;
            }
            return null;
        }
        public ResolvedFont ResolveFont(RequestFont font)
        {
            //cache level-1 (attached inside the request font)
            ResolvedFont resolvedFont = RequestFont.GetResolvedFont1<ResolvedFont>(font);
            if (resolvedFont != null) return resolvedFont;

            Typeface typeface;
            if (font.FromTypefaceFile)
            {
                //this may not be loaded
                //so check if we have that file or not
                typeface = _installedTypefaceCollection.ResolveTypefaceFromFile(font.UserInputTypefaceFile);
                if (typeface != null)
                {
                    //found
                    //TODO: handle FontStyle ***                    
                    resolvedFont = new ResolvedFont(typeface, font.SizeInPoints);
                    RequestFont.SetResolvedFont1(font, resolvedFont);
                    return resolvedFont;
                }
            }

            //cache level-2 (stored in this openfont service)
            if (_resolvedTypefaceCache.TryGetValue(font.FontKey, out resolvedFont))
            {
                if (resolvedFont.Typeface == null)
                {
                    //this is 'not found' resovled font
                    //so don't return it
                    return null;
                }
                //----
                //cache to level-1
                RequestFont.SetResolvedFont1(font, resolvedFont);
                return resolvedFont;
            }
            //-----
            //when not found
            //find it

            if ((typeface = _installedTypefaceCollection.ResolveTypeface(font.Name,
                            PixelFarm.Drawing.FontStyleExtensions.ConvToInstalledFontStyle(font.Style),
                            font.WeightClass)) == null)
            {
                //this come from other choices?
                int otherChoiceCount;
                if ((otherChoiceCount = font.OtherChoicesCount) > 0)
                {
                    for (int i = 0; i < otherChoiceCount; ++i)
                    {
                        resolvedFont = ResolveFont(font.GetOtherChoice(i));
                        if (resolvedFont != null)
                        {
                            RequestFont.SetResolvedFont1(font, resolvedFont);
                            return resolvedFont;
                        }
                    }
                }

                //still not found
                if (typeface == null)
                {

                    //we don't cache it in central service 
                    //open opportunity for another search
                    //_resolvedTypefaceCache.Add(font.FontKey, ResolvedFont.s_empty);
                    return null;
                }
                return null;
            }
            else
            {
                resolvedFont = new ResolvedFont(typeface, font.SizeInPoints, font.FontKey);
                //cache to level2
                _resolvedTypefaceCache.Add(resolvedFont.FontKey, resolvedFont);
                RequestFont.SetResolvedFont1(font, resolvedFont);
                return resolvedFont;
            }
        }


        public TextServiceClient CreateNewServiceClient() => new TextServiceClient(this);

    }

}
