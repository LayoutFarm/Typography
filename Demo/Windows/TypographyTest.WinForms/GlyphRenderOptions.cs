//Apache2, 2017, WinterDev
using System;
using System.Collections.Generic;
using System.IO;
using Typography.OpenFont;
using Typography.TextLayout;

using PixelFarm.Drawing.Fonts;
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