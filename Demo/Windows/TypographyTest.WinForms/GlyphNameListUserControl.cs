//MIT, 2017, WinterDev
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;


using Typography.OpenFont;

namespace TypographyTest.WinForms
{
    public partial class GlyphNameListUserControl : UserControl
    {
        Typeface _selectedTypeface;
        public GlyphNameListUserControl()
        {
            InitializeComponent();
        }

        private void GlyphNameListUserControl_Load(object sender, EventArgs e)
        {

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
           
            typeface.GetGlyphIndexByName("A");
        }
    }
}
