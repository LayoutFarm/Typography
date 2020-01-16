//MIT, 2017-present, WinterDev
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;

namespace TypographyTest.WinForms
{
    public partial class GlyphNameListUserControl : UserControl
    {
        Typeface _selectedTypeface;
        public event EventHandler GlyphNameChanged;
        List<GlyphNameMapInfo> _allGlyphNameMapList = new List<GlyphNameMapInfo>();


        public GlyphNameListUserControl()
        {
            InitializeComponent();
            this.listBox1.SelectedIndexChanged += (s, e) =>
            {
                if (listBox1.SelectedItem != null)
                {
                    this.textBox1.Text =
                       SelectedGlyphName = ((GlyphNameMapInfo)listBox1.SelectedItem)._glyphNameMap.glyphName;
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

            this.txtHexUnicode.KeyDown += (s, e) =>
            {

                if (e.KeyCode == Keys.Enter)
                {
                    //find user name first
                    string unicode_hexForm = this.txtHexUnicode.Text;
                    int unicode = Convert.ToInt32(unicode_hexForm, 16);
                    ushort glyphIndex = _selectedTypeface.LookupIndex(unicode);
                    //
                    if (glyphIndex > 0)
                    {
                        this.listBox1.SelectedIndex = glyphIndex;
                    }
                };
            };


            this.textBox1.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    //find user name first
                    string userSupplyGlyphName = this.textBox1.Text;
                    if (userSupplyGlyphName == "")
                    {
                        //show all
                        ShowGlyphNameList(_allGlyphNameMapList);
                    }
                    else
                    {
                        //show 
                        Glyph found = _selectedTypeface.GetGlyphByName(userSupplyGlyphName);
                        if (found != null && found.GlyphIndex != 0)
                        {
                            ShowGlyphNameList(_allGlyphNameMapList);

                            int index = 0;
                            bool found1 = false;
                            foreach (GlyphNameMapInfo mapInfo in _allGlyphNameMapList)
                            {
                                if (mapInfo._glyphNameMap.glyphName == userSupplyGlyphName)
                                {
                                    found1 = true;
                                    break;
                                }
                                else
                                {
                                    index++;
                                }
                            }

                            if (found1)
                            {
                                listBox1.SelectedIndex = index;
                            }
                        }
                        else
                        {
                            //not found => find glyph that contains the 'name'
                            int index = 0;
                            string user_upperCase = userSupplyGlyphName.ToUpper();

                            List<GlyphNameMapInfo> similarList = new List<GlyphNameMapInfo>();
                            foreach (GlyphNameMapInfo mapInfo in _allGlyphNameMapList)
                            {
                                if (mapInfo._glyphNameMap.glyphName.ToUpper().Contains(user_upperCase))
                                {
                                    similarList.Add(mapInfo);
                                }
                                index++;
                            }
                            ShowGlyphNameList(similarList);
                        }
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

        class GlyphNameMapInfo
        {
            //this is a helper class
            public readonly GlyphNameMap _glyphNameMap;
            public GlyphNameMapInfo(GlyphNameMap glyphNameMap)
            {
                _glyphNameMap = glyphNameMap;
            }
            public override string ToString()
            {
                return _glyphNameMap.glyphIndex + ": " + _glyphNameMap.glyphName;
            }
        }


        void ListAllGlyphNames(Typeface typeface)
        {
            _allGlyphNameMapList.Clear();

            foreach (GlyphNameMap glyphNameMap in typeface.GetGlyphNameIter())
            {
                var mapInfo = new GlyphNameMapInfo(glyphNameMap);
                _allGlyphNameMapList.Add(mapInfo);
            }

            ShowGlyphNameList(_allGlyphNameMapList);
        }
        void ShowGlyphNameList(List<GlyphNameMapInfo> srcList)
        {
            this.listBox1.Items.Clear();
            foreach (GlyphNameMapInfo mapInfo in srcList)
            {
                listBox1.Items.Add(mapInfo);
            }

        }
    }
}
