//MIT, 2017, WinterDev
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace TypographyTest.WinForms
{
    public partial class GlyphContourAnalysisOptionsUserControl : UserControl
    {
        ContourAnalysisOptions _options = new ContourAnalysisOptions();

        public GlyphContourAnalysisOptionsUserControl()
        {
            InitializeComponent();
            this.txtGridSize.KeyDown += TxtGridSize_KeyDown;
        }


        private void TxtGridSize_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int result = 5;//default
                if (int.TryParse(this.txtGridSize.Text, out result))
                {
                    if (result < 5)
                    {
                        _options.GridSize = 5;
                    }
                    else if (result > 800)
                    {
                        _options.GridSize = 800;
                    }
                }

                this.txtGridSize.Text = _options.GridSize.ToString();
                //#if DEBUG
                //                Typography.Contours.GlyphDynamicOutline.dbugGridHeight = _gridSize;
                //#endif
                UpdateRenderOutput();
            }

        }
        private void GlyphContourAnalysisOptionsUserControl_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            SetupControlBoxes();
        }
        public ContourAnalysisOptions Options { get { return _options; } }
        public TreeView DebugTreeView
        {
            get
            {
                return this.treeView1;
            }
        }
        void SetupControlBoxes()
        {
            chkShowGrid.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkShowTess.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkXGridFitting.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkYGridFitting.CheckedChanged += (s, e) => UpdateRenderOutput();
            
            chkLcdTechnique.CheckedChanged += (s, e) => UpdateRenderOutput();

            //----------           
            chkDrawCentroidBone.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkDrawGlyphBone.CheckedChanged += (s, e) => UpdateRenderOutput();

            chkDynamicOutline.CheckedChanged += (s, e) => UpdateRenderOutput();

            chkSetPrinterLayoutForLcdSubPix.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkDrawTriangles.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkDrawRegenerateOutline.CheckedChanged += (s, e) => UpdateRenderOutput();
           
            chkDrawLineHubConn.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkDrawPerpendicularLine.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkDrawGlyphPoint.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkTestGridFit.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkUseHorizontalFitAlign.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkWriteFitOutputToConsole.CheckedChanged += (s, e) => UpdateRenderOutput();


            //edge offset
            lstEdgeOffset.Items.Add(0f);
            lstEdgeOffset.Items.Add(-10f);
            lstEdgeOffset.Items.Add(-8f);
            lstEdgeOffset.Items.Add(-6f);
            lstEdgeOffset.Items.Add(-4f);
            lstEdgeOffset.Items.Add(4f);
            lstEdgeOffset.Items.Add(6f);
            lstEdgeOffset.Items.Add(8f);
            lstEdgeOffset.Items.Add(10f);
            lstEdgeOffset.SelectedIndex = 0;
            lstEdgeOffset.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
        }
        void UpdateRenderOutput()
        {
          
            _options.ShowGrid = chkShowGrid.Checked;
            _options.ShowTess = chkShowTess.Checked;

            _options.XGridFitting = chkXGridFitting.Checked;
            _options.YGridFitting = chkYGridFitting.Checked;


            _options.LcdTechnique = chkLcdTechnique.Checked;
            _options.DrawCentroidBone = chkDrawCentroidBone.Checked;
            _options.DrawGlyphBone = chkDrawGlyphBone.Checked;
            _options.DrawGlyphPoint = chkDrawGlyphPoint.Checked;
            _options.DrawPerpendicularLine = chkDrawPerpendicularLine.Checked;
            _options.DrawTriangles = chkDrawTriangles.Checked;
            _options.DrawLineHubConn = chkDrawLineHubConn.Checked;
            //
            _options.DynamicOutline = chkDynamicOutline.Checked;
            _options.EnableGridFit = chkTestGridFit.Checked;
            _options.UseHorizontalFitAlignment = chkUseHorizontalFitAlign.Checked;
            _options.WriteFitOutputToConsole = chkWriteFitOutputToConsole.Checked;
            _options.DrawRegenerationOutline = chkDrawRegenerateOutline.Checked;

            //
            _options.EdgeOffset = (float)lstEdgeOffset.SelectedItem;


            //
            _options.InvokeAttachEvents();

        }
    }
}
