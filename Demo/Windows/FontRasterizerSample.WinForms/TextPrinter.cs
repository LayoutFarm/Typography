//MIT, 2016-2017, WinterDev

using System.Collections.Generic;
using Typography.OpenFont;
using PixelFarm.Drawing.Fonts;
using Typography.TextLayout;

namespace SampleWinForms
{

    class TextPrinter
    {
        GlyphLayout _glyphLayout = new GlyphLayout();

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

        public void Print(Typeface typeface, float size, string str, List<GlyphPlan> glyphPlanBuffer)
        {
            Print(typeface, size, str.ToCharArray(), glyphPlanBuffer);
        }

        List<ushort> inputGlyphs = new List<ushort>(); //not thread safe***
        public void Print(Typeface typeface, float size, char[] str, List<GlyphPlan> glyphPlanBuffer)
        {
            //1. layout
            _glyphLayout.Layout(typeface, size, str, glyphPlanBuffer);


            var glyphPathBuilder = new GlyphPathBuilderVxs(typeface);
            int j = glyphPlanBuffer.Count;

            for (int i = 0; i < j; ++i)
            {

                GlyphPlan glyphPlan = glyphPlanBuffer[i];
                //-----------------------------------
                //check if we static vxs/bmp for this glyph
                //if not, create and cache
                //-----------------------------------  
                glyphPathBuilder.BuildFromGlyphIndex(glyphPlan.glyphIndex, size);
                //-----------------------------------  
                glyphPlan.vxs = glyphPathBuilder.GetVxs();
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