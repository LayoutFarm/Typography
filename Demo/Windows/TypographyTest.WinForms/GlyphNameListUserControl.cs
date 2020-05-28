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
        public event EventHandler GlyphChanged;
        List<GlyphNameMapInfo> _allGlyphNameMapList = new List<GlyphNameMapInfo>();


        public GlyphNameListUserControl()
        {
            InitializeComponent();
            this.listBox1.SelectedIndexChanged += (s, e) =>
            {
                if (listBox1.SelectedItem != null)
                {
                    SelectedGlyphIndex = (ushort)this.listBox1.SelectedIndex;
                    this.textBox1.Text =
                       SelectedGlyphName = ((GlyphNameMapInfo)listBox1.SelectedItem)._glyphNameMap.glyphName;
                }

                if (chkRenderGlyph.Checked)
                {
                    //render ...
                    GlyphChanged?.Invoke(null, EventArgs.Empty);
                }
            };

            this.txtHexUnicode.KeyDown += (s, e) =>
            {

                if (e.KeyCode == Keys.Enter)
                {
                    //find user name first
                    string unicode_hexForm = this.txtHexUnicode.Text;
                    int unicode = Convert.ToInt32(unicode_hexForm, 16);
                    ushort glyphIndex = _selectedTypeface.GetGlyphIndex(unicode);

                    if (glyphIndex > 0)
                    {
                        Glyph foundGlyph = _selectedTypeface.GetGlyph(glyphIndex);

                        //display
                        this.listBox1.SelectedIndex = glyphIndex;
                    }
                };
            };

            lstUnicodes.SelectedIndexChanged += (s, e) =>
            {
                this.txtHexUnicode.Text = (string)lstUnicodes.SelectedItem;
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
        public string SelectedGlyphName { get; private set; }
        public ushort SelectedGlyphIndex { get; private set; }
        public bool RenderByGlyphName => chkRenderByGlyphName.Checked;

        public Typeface Typeface
        {
            get => _selectedTypeface;
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

            lstUnicodes.Items.Clear();
        }
        void ShowGlyphNameList(List<GlyphNameMapInfo> srcList)
        {
            this.listBox1.Items.Clear();

            listBox1.SuspendLayout();
            foreach (GlyphNameMapInfo mapInfo in srcList)
            {
                listBox1.Items.Add(mapInfo);
            }
            listBox1.ResumeLayout();
        }

        void cmdListAllUnicodes_Click(object sender, EventArgs e)
        {
            List<uint> unicodes = new List<uint>();
            _selectedTypeface.CollectUnicode(unicodes);
            //show in list in hex 
            lstUnicodes.Items.Clear();
            foreach (uint u in unicodes)
            {
                lstUnicodes.Items.Add(String.Format("{0:X}", u).ToLower());
            }
        }
    }
}
