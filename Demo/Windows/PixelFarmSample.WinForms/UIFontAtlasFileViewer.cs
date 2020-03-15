//MIT, 2020, WinterDev
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using PixelFarm.CpuBlit.BitmapAtlas;

namespace SampleWinForms
{
    public partial class UIFontAtlasFileViewer : UserControl
    {
        string _atlasInfo;
        string _atlasImgFile;
        SimpleBitmapAtlasBuilder _atlasBuilder;
        Bitmap _atlasBmp;
        Graphics _pic1Gfx;

        public UIFontAtlasFileViewer()
        {
            InitializeComponent();
            treeView1.NodeMouseClick += TreeView1_NodeMouseClick;
            treeView1.KeyDown += TreeView1_KeyDown;
        }

        private void TreeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                if (treeView1.SelectedNode.Tag is TempGlyphMap map)
                {
                    ShowGlyphLocation(map);
                }
            }
        }

        private void UIFontAtlasFileViewer_Load(object sender, EventArgs e)
        {

        }

        class TempGlyphMap
        {
            public ushort glyphIndex;
            public AtlasItem glyphMap;
            public override string ToString()
            {
                return glyphIndex + ", (" + glyphMap.Left + "," +
                    glyphMap.Top + "," + glyphMap.Width + "," + glyphMap.Height +
                    "), offset_xy=" + glyphMap.TextureXOffset + "," + glyphMap.TextureYOffset;

            }
        }

        void LoadAtlasImgFile(string atlasImgFile)
        {

            _atlasBmp = new Bitmap(atlasImgFile);
            SimpleUtils.DisposeExistingPictureBoxImage(pictureBox1);
            pictureBox1.Image = _atlasBmp;
        }
        public void LoadFontAtlasFile(string atlasInfo, string atlasImgFile)
        {
            _atlasInfo = atlasInfo;
            _atlasImgFile = atlasImgFile;
            textBox1.Text = atlasImgFile;

            LoadAtlasImgFile(_atlasImgFile);

            //load img
            _atlasBuilder = new SimpleBitmapAtlasBuilder();
            List<SimpleBitmapAtlas> fontAtlasList = null;
            fontAtlasList = _atlasBuilder.LoadAtlasInfo(atlasInfo);


            int count = fontAtlasList.Count;
            treeView1.Nodes.Clear();

            for (int i = 0; i < count; ++i)
            {
                SimpleBitmapAtlas fontAtlas = fontAtlasList[i];

                TreeNode atlasNode = new TreeNode();
                atlasNode.Text = fontAtlas.FontFilename + ", count=" + fontAtlas.GlyphDic.Count;

                treeView1.Nodes.Add(atlasNode);

                List<TempGlyphMap> tmpGlyphMaps = new List<TempGlyphMap>(fontAtlas.GlyphDic.Count);
                foreach (var kv in fontAtlas.GlyphDic)
                {
                    tmpGlyphMaps.Add(new TempGlyphMap { glyphIndex = kv.Key, glyphMap = kv.Value });
                }
                tmpGlyphMaps.Sort((a, b) => a.glyphIndex.CompareTo(b.glyphIndex));

                foreach (TempGlyphMap tmpGlyphMap in tmpGlyphMaps)
                {
                    TreeNode glyphMapNode = new TreeNode();
                    glyphMapNode.Tag = tmpGlyphMap;
                    glyphMapNode.Text = glyphMapNode.Tag.ToString();
                    atlasNode.Nodes.Add(glyphMapNode);
                }
            }
            treeView1.ExpandAll();
        }

        private void TreeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag is TempGlyphMap map)
            {
                ShowGlyphLocation(map);
            }
        }
        void ShowGlyphLocation(TempGlyphMap tmpGlyphMap)
        {
            if (_pic1Gfx == null)
            {
                _pic1Gfx = pictureBox1.CreateGraphics();
            }


            _pic1Gfx.Clear(pictureBox1.BackColor);
            _pic1Gfx.DrawImage(_atlasBmp, 0, 0);
            AtlasItem glyphMap = tmpGlyphMap.glyphMap;
            _pic1Gfx.DrawRectangle(Pens.Red, new Rectangle { X = glyphMap.Left, Y = glyphMap.Top, Width = glyphMap.Width, Height = glyphMap.Height });
        }
    }
}
