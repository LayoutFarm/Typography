//MIT, 2017, WinterDev
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;


using System.Windows.Forms;
using Typography.Contours;

namespace TypographyTest.WinForms
{
    public partial class GlyphRenderOptionsUserControl : UserControl
    {
        GlyphRenderOptions _renderOptions = new GlyphRenderOptions();
        public GlyphRenderOptionsUserControl()
        {
            InitializeComponent();
        }
        public GlyphRenderOptions Options
        {
            get { return _renderOptions; }
        }
        private void GlyphRenderOptionsUserControl_Load(object sender, EventArgs e)
        {
            if (this.DesignMode) return;
            // 
            SetupControlBoxes();
            SetupHintList();
        }
        void SetupControlBoxes()
        {
            chkBorder.CheckedChanged += (s, e) =>
            {
                _renderOptions.DrawBorder = chkBorder.Checked;
                _renderOptions.InvokeAttachEvents();
            };
            chkFillBackground.CheckedChanged += (s, e) =>
            {
                _renderOptions.FillBackground = chkBorder.Checked;
                _renderOptions.InvokeAttachEvents();

            };
        }
        void SetupHintList()
        {
            //---------- 

            lstHintList.Items.Add(HintTechnique.None);
            lstHintList.Items.Add(HintTechnique.TrueTypeInstruction);
            lstHintList.Items.Add(HintTechnique.TrueTypeInstruction_VerticalOnly);
            lstHintList.Items.Add(HintTechnique.CustomAutoFit);

            lstHintList.SelectedIndex = 0;
            lstHintList.SelectedIndexChanged += (s, e) =>
            {
                _renderOptions.HintTechnique = (HintTechnique)lstHintList.SelectedItem;
                _renderOptions.InvokeAttachEvents();
            };
        }

        void SetupLigature()
        {
            chkGsubEnableLigature.CheckedChanged += (s, e) =>
            {
                _renderOptions.EnableLigature = chkGsubEnableLigature.Checked;
                _renderOptions.InvokeAttachEvents();
            };
        }
    }
}
