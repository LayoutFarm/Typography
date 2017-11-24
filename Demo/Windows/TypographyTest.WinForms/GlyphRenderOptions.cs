//MIT, 2017, WinterDev
using System;
using Typography.Contours;
namespace TypographyTest
{

    public class GlyphRenderOptions
    {
        public event EventHandler UpdateRenderOutput;


        public GlyphRenderOptions()
        {
            HintTechnique = HintTechnique.None;
            FillBackground = true;
        }
        public HintTechnique HintTechnique
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