//MIT, 2017-present, WinterDev
using System;
using System.Collections.Generic;
using System.IO;
using Typography.OpenFont;
using Typography.TextLayout;
using Typography.TextServices;
using Typography.FontManagement;

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

        Typography.TextServices.TextServices _textServices;
        public BasicFontOptions()
        {

            FontSizeInPoints = 10;
            this.RenderChoice = RenderChoice.RenderWithTextPrinterAndMiniAgg;
            _textServices = new TextServices();
            _textServices.InstalledFontCollection = new InstalledTypefaceCollection();
        }
        public RenderChoice RenderChoice
        {
            get;
            set;
        }
        public void LoadFontList()
        {
            PositionTech = PositionTechnique.OpenFont;
            ////---------- 
            ////1. create font collection        
            ////2. set some essential handler
            foreach (string file in Directory.GetFiles("../../../TestFonts", "*.*"))
            {
                //eg. this is our custom font folder  
                string ext = Path.GetExtension(file).ToLower();

                switch (ext)
                {
                    case ".ttf":
                    case ".otf":
                        _textServices.InstalledFontCollection.AddFontStreamSource(new Typography.FontManagement.FontFileStreamProvider(file));
                        break;
                }

            }

        }
        public PositionTechnique PositionTech { get; set; }
        //public OpenFontStore OpenFontStore
        //{
        //    get { return _openFontStore; }
        //    set { _openFontStore = value; }
        //}
        public Typeface Typeface
        {
            get
            {
                return _selectedTypeface;
            }
        }
        public float FontSizeInPoints { get; set; }
        public Typography.OpenFont.ScriptLang ScriptLang { get; set; }
        public InstalledTypeface InstalledTypeface
        {
            get
            {
                return _instTypeface;
            }
            set
            {
                _instTypeface = value;
                _typefaceChanged = false;
                //
                if (value == null) return;

                //TODO: review here again
                Typeface selected_typeface = _textServices.GetTypeface(value.FontName, TypefaceStyle.Normal);
                if (selected_typeface != this._selectedTypeface)
                {
                    _typefaceChanged = true;
                }
                _selectedTypeface = selected_typeface;
            }
        }
        public IEnumerable<InstalledTypeface> GetInstalledTypefaceIter()
        {
            foreach (InstalledTypeface ff in _textServices.InstalledFontCollection.GetInstalledFontIter())
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