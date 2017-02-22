//MIT, 2016-2017, WinterDev


using System.Collections.Generic;
using System.IO;

using PixelFarm.Drawing.Fonts;
using Typography.OpenFont;
using Typography.TextLayout;
namespace SampleWinForms
{

    class TextPrinter
    {
        Typeface _currentTypeface;
        GlyphLayout _glyphLayout = new GlyphLayout();
        Dictionary<string, Typeface> _cachedTypefaces = new Dictionary<string, Typeface>();

        string _currentFontFilename = "";

        public TextPrinter()
        {
            //default         
        }
        public ScriptLang ScriptLang
        {
            get
            {
                return _glyphLayout.ScriptLang;
            }
            set
            {
                _glyphLayout.ScriptLang = value;
            }
        }
        public PositionTecnhique PositionTechnique
        {
            get { return _glyphLayout.PositionTechnique; }
            set { _glyphLayout.PositionTechnique = value; }
        }
        public HintTechnique HintTechnique
        {
            get;
            set;
        }
        public bool EnableLigature
        {
            get { return _glyphLayout.EnableLigature; }
            set { this._glyphLayout.EnableLigature = value; }
        }

        public string FontFile
        {
            get { return _currentFontFilename; }
            set
            {
                if (value != _currentFontFilename)
                {
                    //switch to another font                   

                    //store current typeface to cache
                    if (_currentTypeface != null && !_cachedTypefaces.ContainsKey(value))
                    {
                        _cachedTypefaces[_currentFontFilename] = _currentTypeface;
                    }

                    //chkeck if we have this in cache ?
                    _cachedTypefaces.TryGetValue(value, out _currentTypeface);

                }
                this._currentFontFilename = value;
            }
        }
        public void Print(float size, string str, List<GlyphPlan> glyphPlanBuffer)
        {
            if (_currentTypeface == null)
            {
                OpenFontReader reader = new OpenFontReader();
                using (FileStream fs = new FileStream(_currentFontFilename, FileMode.Open))
                {
                    _currentTypeface = reader.Read(fs);
                }
            }
            //-----------
            Print(_currentTypeface, size, str, glyphPlanBuffer);
        }
        public void Print(Typeface typeface, float size, string str, List<GlyphPlan> glyphPlanBuffer)
        {
            Print(typeface, size, str.ToCharArray(), glyphPlanBuffer);
        }

        List<ushort> inputGlyphs = new List<ushort>(); //not thread safe***
        public void Print(Typeface typeface, float size, char[] str, List<GlyphPlan> glyphPlanBuffer)
        {
            //1. layout
            _glyphLayout.Layout(typeface, size, str, glyphPlanBuffer);
            var glyphPathBuilder = new MyGlyphPathBuilder(typeface);
            int j = glyphPlanBuffer.Count;

            float pxScale = typeface.CalculateFromPointToPixelScale(size);
            for (int i = 0; i < j; ++i)
            {

                GlyphPlan glyphPlan = glyphPlanBuffer[i];
                //-----------------------------------
                //check if we static vxs/bmp for this glyph
                //if not, create and cache
                //-----------------------------------  
                glyphPathBuilder.BuildFromGlyphIndex(glyphPlan.glyphIndex, size);
                //-----------------------------------  
                var vxsBuilder = new GlyphPathBuilderVxs();
                glyphPathBuilder.ReadShapes(vxsBuilder);
                glyphPlan.vxs = vxsBuilder.GetVxs(pxScale);
            }

        }



    }

    public enum HintTechnique
    {
        /// <summary>
        /// no hinting
        /// </summary>
        None,
        /// <summary>
        /// truetype instruction
        /// </summary>
        TrueTypeInstruction,
        /// <summary>
        /// truetype instruction vertical only
        /// </summary>
        TrueTypeInstruction_VerticalOnly,
        /// <summary>
        /// custom hint
        /// </summary>
        CustomAutoFit
    }
}