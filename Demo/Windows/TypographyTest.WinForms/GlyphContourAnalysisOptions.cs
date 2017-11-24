//MIT, 2017, WinterDev

using System;
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