//Apache2, 2017, WinterDev

using System;
using System.IO;
using Typography.OpenFont;
using Typography.TextLayout;

using PixelFarm.Drawing.Fonts;

namespace TypographyTest
{
    public class ContourAnalysisOptions
    {
        public event EventHandler UpdateRenderOutput;

        public ContourAnalysisOptions()
        {
         
            GridSize = 5;
        }
        public void InvokeAttachEvents()
        {
            UpdateRenderOutput?.Invoke(this, EventArgs.Empty);
        }
        //chkXGridFitting.CheckedChanged += (s, e) => UpdateRenderOutput();
        //chkYGridFitting.CheckedChanged += (s, e) => UpdateRenderOutput();
        //chkFillBackground.CheckedChanged += (s, e) => UpdateRenderOutput();
        //chkLcdTechnique.CheckedChanged += (s, e) => UpdateRenderOutput();

        ////----------
        //chkShowTess.CheckedChanged += (s, e) => UpdateRenderOutput();
        //chkDrawCentroidBone.CheckedChanged += (s, e) => UpdateRenderOutput();
        //chkDrawGlyphBone.CheckedChanged += (s, e) => UpdateRenderOutput();
        //chkDynamicOutline.CheckedChanged += (s, e) => UpdateRenderOutput();
        //chkSetPrinterLayoutForLcdSubPix.CheckedChanged += (s, e) => UpdateRenderOutput();
        //chkDrawTriangles.CheckedChanged += (s, e) => UpdateRenderOutput();
        //chkDrawRegenerateOutline.CheckedChanged += (s, e) => UpdateRenderOutput();
        //chkBorder.CheckedChanged += (s, e) => UpdateRenderOutput();
        //chkDrawLineHubConn.CheckedChanged += (s, e) => UpdateRenderOutput();
        //chkDrawPerpendicularLine.CheckedChanged += (s, e) => UpdateRenderOutput();
        //chkDrawGlyphPoint.CheckedChanged += (s, e) => UpdateRenderOutput();
        //chkTestGridFit.CheckedChanged += (s, e) => UpdateRenderOutput();
        //chkUseHorizontalFitAlign.CheckedChanged += (s, e) => UpdateRenderOutput();
        //chkWriteFitOutputToConsole.CheckedChanged += (s, e) => UpdateRenderOutput();

    
        public int GridSize { get; set; }
        public bool ShowGrid { get; set; }
        public bool ShowTess { get; set; }
        public bool ShowTriangle { get; set; }

        
        public bool XGridFitting { get; set; }
        public bool YGridFitting { get; set; }
        public bool LcdTechnique { get; set; }
        //
        public bool DrawCentroidBone { get; set; }
        public bool DrawGlyphBone { get; set; }
        public bool DrawRegenerationOutline { get; set; }
        public bool DrawLineHubConn { get; set; }
        public bool DrawPerpendicularLine { get; set; }
        public bool DrawGlyphPoint { get; set; }
        public bool DrawTriangles { get; set; }
        public bool DynamicOutline { get; set; }

        public bool EnableGridFit { get; set; }
        public bool UseHorizontalFitAlignment { get; set; }
        public bool WriteFitOutputToConsole { get; set; }
        public bool SetupPrinterLayoutForLcdSubPix { get; set; }

        //
        public float EdgeOffset { get; set; }
    }
}