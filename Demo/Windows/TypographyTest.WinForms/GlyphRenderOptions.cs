//MIT, 2017-present, WinterDev
using System;
using Typography.OpenFont;
using Typography.Contours;
namespace TypographyTest
{

    public class GlyphRenderOptions
    {
        public event EventHandler UpdateRenderOutput;


        public GlyphRenderOptions()
        {
            HintTechnique = TrueTypeHintTechnique.None;
            FillBackground = true;
        }
        public TrueTypeHintTechnique HintTechnique
        {
            get; set;
        }
        public bool FillBackground { get; set; }
        public bool DrawBorder { get; set; }

        public bool EnableLigature { get; set; }
        public void InvokeAttachEvents()
        {
            UpdateRenderOutput?.Invoke(this, EventArgs.Empty);
        }
    }
}