//MIT, 2017, WinterDev
using System;
using System.Collections.Generic;
using System.IO;
using Typography.OpenFont;
using Typography.TextLayout;
using Typography.TextServices;


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

        TypefaceStore _typefaceStore;
        InstalledFontCollection _installedFontCollection;
        OpenFontStore _openFontStore;
        InstalledFont _installedFont;

        Typeface _selectedTypeface;
        bool _typefaceChanged = false;
        public BasicFontOptions()
        {
           
            FontSizeInPoints = 10;
            this.RenderChoice = RenderChoice.RenderWithTextPrinterAndMiniAgg;
        }
        public RenderChoice RenderChoice
        {
            get;
            set;
        }
        public void LoadFontList()
        {

            _openFontStore = new OpenFontStore();
            _typefaceStore = new TypefaceStore();
            //
            _installedFontCollection = new InstalledFontCollection();
            _typefaceStore.FontCollection = _installedFontCollection;

            //---------- 
            //1. create font collection        
            //2. set some essential handler
            _installedFontCollection.SetFontNameDuplicatedHandler((f1, f2) => FontNameDuplicatedDecision.Skip);
            foreach (string file in Directory.GetFiles("../../../TestFonts", "*.ttf"))
            {
                //eg. this is our custom font folder  
                _installedFontCollection.AddFont(new FontFileStreamProvider(file));
            }
            PositionTech = PositionTechnique.OpenFont;
        }
        public PositionTechnique PositionTech { get; set; }
        public OpenFontStore OpenFontStore
        {
            get { return _openFontStore; }
            set { _openFontStore = value; }
        }
        public Typeface Typeface
        {
            get
            {
                return _selectedTypeface;
            }
        }
        public float FontSizeInPoints { get; set; }
        public Typography.OpenFont.ScriptLang ScriptLang { get; set; }
        public InstalledFont InstalledFont
        {
            get
            {
                return _installedFont;
            }
            set
            {
                _installedFont = value;
                _typefaceChanged = false;
                //
                if (value == null) return;
                var selected_typeface = _typefaceStore.GetTypeface(value);
                if (selected_typeface != this._selectedTypeface)
                {
                    _typefaceChanged = true;
                }
                _selectedTypeface = selected_typeface;
            }
        }
        public IEnumerable<InstalledFont> GetInstalledFontIter()
        {
            foreach (InstalledFont ff in _installedFontCollection.GetInstalledFontIter())
            {
                yield return ff;
            }
        }
        public void InvokeAttachEvents()
        {
            if (TypefaceChanged != null && _typefaceChanged)
            {
                TypefaceChanged(this, new TypefaceChangedEventArgs(_selectedTypeface));
            }
            _typefaceChanged = false;
            //
            //
            UpdateRenderOutput?.Invoke(this, EventArgs.Empty);

        }
    }
}