//MIT, 2017-present, WinterDev
using System;
using System.Collections.Generic;
using System.IO;
using Typography.OpenFont;
using Typography.TextLayout;
using Typography.Text;
using Typography.FontCollections;
using PixelFarm.Drawing;

namespace TypographyTest
{
    public enum RenderChoice
    {
        RenderWithMiniAgg_SingleGlyph,//for test single glyph 
        RenderWithGdiPlusPath,
        RenderWithTextPrinterAndMiniAgg,
        RenderWithMsdfGen, //rendering with multi-channel signed distance field img
        RenderWithSdfGen//not support sdfgen
    }


    public class BasicFontOptions
    {
        public event EventHandler UpdateRenderOutput;
        public event EventHandler<TypefaceChangedEventArgs> TypefaceChanged;

        InstalledTypeface _instTypeface;

        Typeface _selectedTypeface;
        bool _typefaceChanged = false;


        InstalledTypefaceCollection _installedTypefaces;

        public BasicFontOptions()
        {
            SelectedTypefaceStyle = TypefaceStyle.Regular;
            FontSizeInPoints = 10;
            this.RenderChoice = RenderChoice.RenderWithTextPrinterAndMiniAgg;

            _installedTypefaces = new InstalledTypefaceCollection();
            _installedTypefaces.SetFontNameDuplicatedHandler(
                (f0, f1) => FontNameDuplicatedDecision.Skip);
            _installedTypefaces.SetDefaultFontNotFoundHandler();
        }
        public RenderChoice RenderChoice { get; set; }

        public bool EnableMultiTypefaces { get; set; }

        public void LoadFontList()
        {
            PositionTech = PositionTechnique.OpenFont;
            ////---------- 
            ////1. create font collection        
            ////2. set some essential handler

            int index = 0;

            foreach (string file in Directory.GetFiles("../../../TestFonts", "*.*"))
            {
                //eg. this is our custom font folder  

#if DEBUG
                //if (index == 16)
                //{

                //}
#endif

                string ext = Path.GetExtension(file).ToLower();
                try
                {
                    switch (ext)
                    {
                        case ".woff2":
                        case ".woff":
                        case ".ttc":
                        case ".otc":
                        case ".ttf":
                        case ".otf":
                            _installedTypefaces.AddFontStreamSource(new FontFileStreamProvider(file));
                            break;
                    }

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(index + " " + file + " , err=>" + ex.Message);
                }
                index++;

            }

        }
        public PositionTechnique PositionTech { get; set; }
        public Typeface Typeface => _selectedTypeface;

        public float FontSizeInPoints { get; set; }


        bool _scLangChanged;
        ScriptLang _scLang;
        public ScriptLang ScriptLang
        {
            get => _scLang;
            set
            {
                if (_scLang.scriptTag != value.scriptTag ||
                    _scLang.sysLangTag != value.sysLangTag)
                {
                    _scLangChanged = true;
                    _scLang = value;
                }
            }
        }
        public TypefaceStyle SelectedTypefaceStyle { get; set; }

        public InstalledTypeface InstalledTypeface
        {
            get => _instTypeface;
            set
            {
                _instTypeface = value;
                _typefaceChanged = false;
                //
                if (value == null) return;

                //TODO: review here again
                SelectedTypefaceStyle = _instTypeface.TypefaceStyle;
                Typeface selected_typeface = _installedTypefaces.ResolveTypeface(value.FontName, _instTypeface.TypefaceStyle, (ushort)RequestFontWeight.Normal);
                if (selected_typeface != _selectedTypeface)
                {
                    _typefaceChanged = true;
                }
                _selectedTypeface = selected_typeface;
            }
        }
        public IEnumerable<InstalledTypeface> GetInstalledTypefaceIter() => _installedTypefaces.GetInstalledFontIter();
        public InstalledTypefaceCollection InstallTypefaceCollection => _installedTypefaces;

        public void InvokeAttachEvents()
        {
            if (TypefaceChanged != null && (_typefaceChanged || _scLangChanged))
            {
                TypefaceChanged(this, new TypefaceChangedEventArgs(_selectedTypeface));
            }
            _typefaceChanged = false;
            _scLangChanged = false;
            //
            //
            UpdateRenderOutput?.Invoke(this, EventArgs.Empty);
        }
    }
}