//MIT, 2017, WinterDev
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;


using Typography.OpenFont;
using Typography.OpenFont.Extensions;

namespace TypographyTest.WinForms
{
    public partial class GlyphNameListUserControl : UserControl
    {
        Typeface _selectedTypeface;
        public event EventHandler GlyphNameChanged;

        public GlyphNameListUserControl()
        {
            InitializeComponent();
            this.listBox1.SelectedIndexChanged += (s, e) =>
            {
                if (listBox1.SelectedItem != null)
                {
                    this.textBox1.Text =
                       SelectedGlyphName = (string)listBox1.SelectedItem;
                }

                if (chkRenderGlyph.Checked)
                {
                    //render ...
                    if (GlyphNameChanged != null)
                    {
                        GlyphNameChanged(null, EventArgs.Empty);
                    }
                }
            };

            this.textBox1.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    //find user name first
                    string userSupplyGlyphName = this.textBox1.Text;
                    Glyph found = _selectedTypeface.GetGlyphByName(userSupplyGlyphName);
                    if (found != null)
                    {
                        int sel_index = listBox1.FindString(userSupplyGlyphName);
                        listBox1.SelectedIndex = sel_index;
                    }
                }
            };
        }

        private void GlyphNameListUserControl_Load(object sender, EventArgs e)
        {

        }
        public string SelectedGlyphName
        {
            get;
            private set;
        }
        public Typeface Typeface
        {
            get
            {
                return _selectedTypeface;
            }
            set
            {
                _selectedTypeface = value;
                //list all glyph in the type face
                if (value != null)
                {
                    ListAllGlyphNames(value);
                }
            }
        }
        void ListAllGlyphNames(Typeface typeface)
        {
            this.listBox1.Items.Clear();
            foreach (GlyphNameMap glyphNameMap in typeface.GetGlyphNameIter())
            {
                listBox1.Items.Add(glyphNameMap.glyphName);
            }
        }
    }
}
