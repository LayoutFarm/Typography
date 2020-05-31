//MIT, 2020, Brezza92
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using System.Drawing;
using System.Text;
using System.Windows.Forms;

using PixelFarm.Drawing;
using PixelFarm.CpuBlit;
using LayoutFarm.MathLayout;
using PixelFarm.CpuBlit.VertexProcessing;


using Typography.OpenFont;
using Typography.OpenFont.Tables;
using Typography.Contours;
using Typography.TextLayout;

namespace MathLayout
{
    public partial class Form1 : Form
    {
        MemBitmap _memBmp;
        Graphics _g;
        Bitmap _myBitmap;
        GlyphBoxPainter _painter;

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            _g = this.panel1.CreateGraphics();//
            _memBmp = new MemBitmap(800, 500);
            _myBitmap = new Bitmap(_memBmp.Width, _memBmp.Height);
            _painter = new GlyphBoxPainter();

            AddExamples();
        }
        private void AddExamples()
        {
            string[] files = Directory.GetFiles("Examples", "*.html");
            if (files != null && files.Length > 0)
            {
                exampleBox.Items.AddRange(files);
            }
            exampleBox.SelectedIndex = 0;
        }

        void CopyBitmapToScreen()
        {
            if (_memBmp == null) return;
            //-------
            //copy from mem bitmap to native bitmap
            var bmp_data = _myBitmap.LockBits(new System.Drawing.Rectangle(0, 0, _myBitmap.Width, _myBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var tmpMemPtr = MemBitmap.GetBufferPtr(_memBmp))
            {
                unsafe
                {

                    PixelFarm.Drawing.Internal.MemMx.memcpy((byte*)bmp_data.Scan0, (byte*)tmpMemPtr.Ptr, tmpMemPtr.LengthInBytes);
                }
            }
            _myBitmap.UnlockBits(bmp_data);
            _g.DrawImage(_myBitmap, 0, 0);

        }
        private void button1_Click(object sender, EventArgs e)
        {
            using (Tools.BorrowAggPainter(_memBmp, out AggPainter p))
            {
                p.Clear(PixelFarm.Drawing.Color.White);
                //--------------           


                //create a set box glyph box
                _painter.Painter = p;
                //_painter.Paint(CreateTestBox1());
                //_painter.Paint(CreateTestBox2());
                //_painter.Paint(CreateTestBox3());
                //_painter.Paint(CreateTestBox4());
                _painter.Paint(CreateTestBox5());
                //--------------
                //copy memBmp output to screen

                p.FillRect(0, 0, 5, 5, PixelFarm.Drawing.Color.Red); //this is a reference point
                CopyBitmapToScreen();
            }
        }

        private void Paint(Box box)
        {
            using (Tools.BorrowAggPainter(_memBmp, out AggPainter p))
            {
                p.Clear(PixelFarm.Drawing.Color.White);
                //--------------           


                //create a set box glyph box
                _painter.Painter = p;
                //_painter.Paint(CreateTestBox1());
                //_painter.Paint(CreateTestBox2());
                //_painter.Paint(CreateTestBox3());
                //_painter.Paint(CreateTestBox4());
                _painter.Paint(box);
                //--------------
                //copy memBmp output to screen

                p.FillRect(0, 0, 5, 5, PixelFarm.Drawing.Color.Red); //this is a reference point
                CopyBitmapToScreen();
            }
        }
        void LoadFont()
        {
            if (_latinModernMathFont == null)
            {
                using (FileStream fs = new FileStream("Fonts/latinmodern-math.otf", FileMode.Open))
                //using (FileStream fs = new FileStream("Fonts/Asana-Math.otf", FileMode.Open))
                {
                    OpenFontReader reader = new OpenFontReader();
                    _latinModernMathFont = reader.Read(fs);
                }
            }
        }

        Box CreateTestBox1(string text = null, float fontSize = 20)
        {
            HorizontalStackBox hbox = new HorizontalStackBox();
            //hbox.SetBounds(20, 20, 100, 21);
            // 
            if (text != null)
            {
                LoadFont();
                int length = text.Length;
                char[] ch = text.ToCharArray();
                float font_size_in_Point = fontSize;

                _glyphMeshStore.SetFont(_latinModernMathFont, font_size_in_Point);//20= font size
                _glyphMeshStore.FlipGlyphUpward = true;
                float px_scale = _latinModernMathFont.CalculateScaleToPixelFromPointSize(font_size_in_Point);

                for (int i = 0; i < length; ++i)
                {
                    ushort glyphIndex = _latinModernMathFont.GetGlyphIndex((int)ch[i]);
                    ushort advW = _latinModernMathFont.GetAdvanceWidth((int)ch[i]);//unscale glyph width
                    var t = _latinModernMathFont.GetGlyph(glyphIndex);
                    //now scale it to specific font size

                    int advW_s = (int)System.Math.Round(px_scale * advW);


                    GlyphBox b1 = NewGlyphBox();

                    b1.Character = ch[i];
                    b1.GlyphIndex = glyphIndex;
                    b1.AdvanceWidthScale = advW_s;
                    //b1.SetBounds(0, 0, 10, fontSize*2);

                    if (b1 is VxsGlyphBox vxsGlyphBox)
                    {
                        vxsGlyphBox.GlyphVxs = _glyphMeshStore.GetGlyphMesh(glyphIndex);
                    }


                    hbox.AddChild(b1);
                }//*/
            }
            else
            {
                for (int i = 0; i < 5; ++i)
                {
                    GlyphBox b1 = NewGlyphBox();
                    b1.SetBounds(0, 0, 10, 20);
                    hbox.AddChild(b1);
                }
            }

            hbox.Layout();
            return hbox;
        }
        Box CreateTestBox2()
        {
            VerticalStackBox vbox1 = new VerticalStackBox();
            vbox1.AddChild(CreateTestBox1("Hello"));
            vbox1.AddChild(CreateTestBox1("Hello(3*2)"));
            vbox1.AddChild(CreateTestBox1("World[1+2]", 15));
            vbox1.Layout();
            return vbox1;
        }
        Box CreateTestBox3()
        {
            HorizontalStackBox hbox3 = new HorizontalStackBox();
            hbox3.AddChild(CreateTestBox2());
            hbox3.AddChild(CreateTestBox2());
            hbox3.AddChild(CreateTestBox2());
            hbox3.Layout();
            return hbox3;
        }

        VxsGlyphBox NewGlyphBox() => new VxsGlyphBox();

        Box CreateTestBox4()
        {
            VerticalStackBox vbox1 = new VerticalStackBox();
            vbox1.AddChild(CreateTestBox3());
            vbox1.AddChild(CreateTestBox3());
            vbox1.Layout();


            GlyphBox h_sepBar = NewGlyphBox();
            h_sepBar.SetSize(vbox1.Width, 5);
            vbox1.Insert(1, h_sepBar);
            vbox1.Layout();
            return vbox1;
        }
        Box CreateTestBox4_1()
        {
            VerticalStackBox vbox1 = new VerticalStackBox();
            vbox1.AddChild(CreateTestBox3());
            vbox1.AddChild(CreateTestBox3());
            vbox1.Layout();

            return vbox1;
        }
        Box CreateTestBox5()
        {
            HorizontalStackBox hbox = new HorizontalStackBox();
            hbox.SetLocation(50, 50);
            hbox.AddChild(CreateTestBox4());
            hbox.AddChild(CreateTestBox4_1());
            hbox.Layout();

            float height = hbox.Height;//set height to cover all blog

            GlyphBox openBar = NewGlyphBox();
            openBar.SetSize(5, height);
            hbox.Insert(0, openBar);

            GlyphBox verticalBar2 = NewGlyphBox();
            verticalBar2.SetSize(5, height);
            hbox.AddChild(verticalBar2);//add last
            hbox.Layout();

            return hbox;
        }


        Typeface _latinModernMathFont;

        private void button2_Click(object sender, EventArgs e)
        {
            //EXAMPLE, low-level

            //this show how to render a glyph on screen
            //read font file
            LoadFont();
            //inside a font
            //get some glyph by its name
            //Glyph oneGlyph = _latinModernMathFont.GetGlyphByName("one"); //for get glyph by name

            ushort glyphIndex = _latinModernMathFont.GetGlyphIndex((int)'1');
            Glyph oneGlyph = _latinModernMathFont.GetGlyph(glyphIndex);

            //a glyph contains coordinates of line and curves
            //we transform data inside it to vxs
            //this is done by GlyphContour builder
            GlyphTranslatorToVxs glyphTxToVxs = new GlyphTranslatorToVxs();
            GlyphOutlineBuilder outlineBuilder = new GlyphOutlineBuilder(_latinModernMathFont);
            outlineBuilder.BuildFromGlyph(oneGlyph, 20); //read data into outline builder
            outlineBuilder.ReadShapes(glyphTxToVxs);//translate data inside outline builder to vxs
            using (Tools.BorrowVxs(out var v1, out var v2))
            using (Tools.BorrowAggPainter(_memBmp, out var p))
            {
                glyphTxToVxs.WriteOutput(v1);
                //original v1 is head-down
                Q1RectD bounds = v1.GetBoundingRect(); //with this bounds you also know glyph width/height
                //we want head up, so => flip it
                AffineMat aff = AffineMat.Iden();
                aff.Translate(-bounds.Width / 2, -bounds.Height / 2);
                aff.Scale(1, -1);
                aff.Translate(bounds.Width / 2, bounds.Height / 2);

                aff.TransformToVxs(v1, v2);

                //copy data 
                //now the glyph data is inside v1
                //test paint this glyph
                p.Clear(PixelFarm.Drawing.Color.White);
                p.Fill(v2, PixelFarm.Drawing.Color.Black);
            }

            //-----------
            CopyBitmapToScreen();
        }

        GlyphMeshStore _glyphMeshStore = new GlyphMeshStore();

        private void button3_Click(object sender, EventArgs e)
        {
            //it should be faster if we use 'mesh' cache
            //instead of read-transform it every time like code above(button2_click)

            LoadFont();

            float font_size_in_Point = 20;
            _glyphMeshStore.SetFont(_latinModernMathFont, font_size_in_Point);//20= font size
            _glyphMeshStore.FlipGlyphUpward = true;

            float px_scale = _latinModernMathFont.CalculateScaleToPixelFromPointSize(font_size_in_Point);

            using (Tools.BorrowAggPainter(_memBmp, out var p))
            {

                p.Clear(PixelFarm.Drawing.Color.White);

                float prevX = p.OriginX;
                float prevY = p.OriginY;


                int line_left = 10;
                int line_top = 50;

                p.SetOrigin(line_left, line_top);//*** test

                //draw reference point
                p.FillRect(0, 0, 5, 5, PixelFarm.Drawing.Color.Red);

                char[] test_str = "‽_x‾".ToCharArray();
                int inline_left = 0;
                int inline_top = 0;

                //---------- 
                GlyphLayout glyphLayout = new GlyphLayout();
                glyphLayout.ScriptLang = ScriptLangs.Math;
                glyphLayout.Typeface = _latinModernMathFont;


                //temp fix for some typeface

                glyphLayout.SetGlyphIndexNotFoundHandler((glyph_layout, codepoint, next_codepoint) =>
                {
                    switch (codepoint)
                    {
                        //overline unicode
                        case 8254: return 2246; //overline-combine, this will break into 3 parts in math layout process

                    }
                    return 0;
                });


                //  
                glyphLayout.Layout(test_str, 0, test_str.Length);

                List<UnscaledGlyphPlan> glyphPlans = new List<UnscaledGlyphPlan>();

                foreach (UnscaledGlyphPlan glypyPlan in glyphLayout.GetUnscaledGlyphPlanIter())
                {
                    glyphPlans.Add(glypyPlan);
                }
                //--------
                for (int i = 0; i < glyphPlans.Count; ++i)
                {
                    //ushort glyphIndex = _latinModernMathFont.GetGlyphIndex((int)test_str[i]); 
                    ////do some glyph-substitution 
                    //ushort advW = _latinModernMathFont.GetAdvanceWidth((int)test_str[i]);//unscale glyph width
                    //now scale it to specific font size

                    UnscaledGlyphPlan glyphPlan = glyphPlans[i];
                    int advW_s = (int)System.Math.Round(px_scale * glyphPlan.AdvanceX);
                    VertexStore v1 = _glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex);
                    p.SetOrigin(line_left + inline_left, line_top + inline_top);
                    p.Fill(v1, PixelFarm.Drawing.Color.Black);
                    inline_left += advW_s;//move 
                }
                //restore
                p.SetOrigin(prevX, prevY);
            }

            //-----------
            CopyBitmapToScreen();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            //dom autogen
            DomCreator creator = new DomCreator();
            creator.ReadDomSpec("Tools/DomSpec.xml");

        }

        VxsMathBoxTreeBuilder boxHelper = new VxsMathBoxTreeBuilder();
        Typeface _mathTypeface;
        private void button4_Click(object sender, EventArgs e)
        {

            //test read document
            MathMLReader reader = new MathMLReader();
            //reader.Read("Examples/p28.html");
            if (exampleBox.Items.Count > 0)
            {
                string file = exampleBox.SelectedItem.ToString();
                reader.Read(file);
            }
            else
            {
                reader.Read("Examples/p3314_2.html");
            }
            List<math> resultNodes = reader.ResultMathNodes;

            //test render 1st node
            if (resultNodes.Count > 0)
            {
                boxHelper.FontSize = 40;
                //boxHelper.FontFile = "Fonts/Asana-Math.otf";
                if (_mathTypeface == null)
                {
                    string mathFont = "Fonts/latinmodern-math.otf";
                    using (FileStream fs = new FileStream(mathFont, FileMode.Open))
                    {
                        OpenFontReader fontReader = new OpenFontReader();
                        _mathTypeface = fontReader.Read(fs);
                    }
                }
                boxHelper.MathTypeface = _mathTypeface;



                math m = resultNodes[0];
                //Box box = boxHelper.CreateMathBox(m);
                Box box = boxHelper.CreateMathBoxs(resultNodes);
                box.SetLocation(10, 10);
                box.Layout();
                //present this box in the canvas
                Paint(box);
            }
        }
        void AssignGlyphVxs(Box box, float fontSize = 20)
        {
            if (box is GlyphBox glyphBox)
            {
                AssignGlyphVxs(glyphBox, fontSize);
            }
            else if (box is HorizontalStackBox horizontal)
            {
                int length = horizontal.ChildCount;
                for (int i = 0; i < length; i++)
                {
                    AssignGlyphVxs(horizontal.GetChild(i), fontSize);
                }
            }
            else if (box is VerticalStackBox vertical)
            {
                int length = vertical.ChildCount;
                for (int i = 0; i < length; i++)
                {
                    AssignGlyphVxs(vertical.GetChild(i), fontSize);
                }
            }
        }
        void AssignGlyphVxs(GlyphBox glyphBox, float fontSize = 20)
        {
            LoadFont();
            float font_size_in_Point = fontSize;
            char ch = glyphBox.Character;

            _glyphMeshStore.SetFont(_latinModernMathFont, font_size_in_Point);//20= font size
            _glyphMeshStore.FlipGlyphUpward = true;
            float px_scale = _latinModernMathFont.CalculateScaleToPixelFromPointSize(font_size_in_Point);

            ushort glyphIndex = _latinModernMathFont.GetGlyphIndex((int)ch);
            ushort advW = _latinModernMathFont.GetAdvanceWidth((int)ch);//unscale glyph width
            //now scale it to specific font size

            int advW_s = (int)System.Math.Round(px_scale * advW);

            glyphBox.GlyphIndex = glyphIndex;
            glyphBox.AdvanceWidthScale = advW_s;

            if (glyphBox is VxsGlyphBox vxsGlyphBox)
            {
                vxsGlyphBox.GlyphVxs = _glyphMeshStore.GetGlyphMesh(glyphIndex);
            }


        }
        Box CreateMathBox(MathNode node)
        {
            //create box foreach node
            //TODO: most box are glyph box + its text content
            //except some boxes are Horizontal (eg. mrow) or some box are vertical (...)
            //this should be config from DomSpec.xml or Autogen code             
            Box result = null;
            switch (node.Name)
            {
                default:
                    {
                        //text span box 
                        if (node.Text == null)
                        {
                            return null;
                        }
                        char[] text_buff = node.Text.ToCharArray();
                        if (text_buff.Length == 0)
                        {
                            //????
                            return null;
                        }
                        else if (text_buff.Length > 1)
                        {
                            HorizontalStackBox textSpan = new HorizontalStackBox();
                            textSpan.MathNode = node;
                            for (int i = 0; i < text_buff.Length; ++i)
                            {
                                GlyphBox glyphBox = NewGlyphBox();
                                glyphBox.Character = text_buff[i];
                                textSpan.AddChild(glyphBox);
                            }
                            //return textSpan;
                            result = textSpan;
                        }
                        else
                        {
                            //len=1
                            GlyphBox glyphBox = NewGlyphBox();
                            glyphBox.MathNode = node;
                            glyphBox.Character = text_buff[0];
                            //return glyphBox;
                            result = glyphBox;
                        }
                    }
                    break;
                case "math":
                case "mrow":
                case "msub":
                case "msup":
                    {
                        HorizontalStackBox hbox = new HorizontalStackBox();
                        hbox.MathNode = node;
                        //
                        int child_count = node.ChildCount;
                        for (int i = 0; i < child_count; ++i)
                        {
                            Box childBox = CreateMathBox(node.GetNode(i));
                            if (childBox != null)
                            {
                                hbox.AddChild(childBox);
                            }
                        }
                        //return hbox;
                        result = hbox;
                    }
                    break;
                case "mfrac":
                case "munder":
                    {
                        VerticalStackBox vbox = new VerticalStackBox();
                        vbox.MathNode = node;

                        int child_count = node.ChildCount;
                        for (int i = 0; i < child_count; ++i)
                        {
                            Box childBox = CreateMathBox(node.GetNode(i));
                            if (childBox != null)
                            {
                                vbox.AddChild(childBox);
                            }
                        }
                        //return hbox;
                        result = vbox;
                    }
                    break;
                case "mover":
                    {
                        VerticalStackBox vbox = new VerticalStackBox();
                        vbox.MathNode = node;

                        int child_count = node.ChildCount;
                        if (child_count != 2)//expect 2
                        {
                            return null;
                        }
                        Box baseBox = CreateMathBox(node.GetNode(0));
                        Box overBox = CreateMathBox(node.GetNode(1));
                        vbox.AddChild(overBox);
                        vbox.AddChild(baseBox);
                        //return hbox;
                        result = vbox;
                    }
                    break;
            }
            if (result != null)
            {
                AssignGlyphVxs(result);
            }
            return result;
        }

        private void nextExampleBtn_Click(object sender, EventArgs e)
        {
            if (exampleBox.SelectedIndex == exampleBox.Items.Count - 1)
            {
                exampleBox.SelectedIndex = 0;
            }
            else
            {
                exampleBox.SelectedIndex++;
            }
            button4_Click(null, null);
        }

        private void opsAutogenBtn_Click(object sender, EventArgs e)
        {
            //dom autogen
            //OperatorInfoDictionary reader = new OperatorInfoDictionary();
            //reader.Read("only_ops_table.html");
            OperatorTableCreator autogen = new OperatorTableCreator();
            autogen.AutogenFrom("Tools\\only_ops_table.html");
        }
    }
}