//MIT, 2020-present, WinterDev
using System;
using System.Windows.Forms;
using Typography.OpenFont;

namespace SampleWinForms
{
    public partial class UIFontScriptOpt : UserControl
    {
        public UIFontScriptOpt()
        {
            InitializeComponent();

            this.cmbHintTech.Items.Add(Typography.OpenFont.Contours.HintTechnique.None);
            this.cmbHintTech.Items.Add(Typography.OpenFont.Contours.HintTechnique.TrueTypeInstruction);
            this.cmbHintTech.Items.Add(Typography.OpenFont.Contours.HintTechnique.TrueTypeInstruction_VerticalOnly);
            this.cmbHintTech.SelectedIndex = 0;
        }

        private void CustomUIFontScriptOpt_Load(object sender, EventArgs e)
        {

        }
        public bool Selected => chkSelected.Checked;
        public Typography.OpenFont.Contours.HintTechnique HintTechnique => (Typography.OpenFont.Contours.HintTechnique)cmbHintTech.SelectedItem;

        ScriptLang _scLang;
        public ScriptLang ScriptLang
        {
            get => _scLang;
            set
            {
                _scLang = value;
                this.label1.Text = ScriptLang.GetScriptTagString() + ":" + ScriptLang.GetLangTagString();
            }
        }

    }
}
