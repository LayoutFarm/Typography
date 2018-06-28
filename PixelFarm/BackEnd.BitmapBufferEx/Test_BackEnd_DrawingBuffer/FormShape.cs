//MIT, 2009-2015, Rene Schulte and WriteableBitmapEx Contributors, https://github.com/teichgraf/WriteableBitmapEx
//
//
//   Project:           WriteableBitmapEx - WriteableBitmap extensions
//   Description:       Collection of extension methods for the WriteableBitmap class.
//
//   Changed by:        $Author: unknown $
//   Changed on:        $Date: 2015-03-05 18:18:24 +0100 (Do, 05 Mrz 2015) $
//   Changed in:        $Revision: 113191 $
//   Project:           $URL: https://writeablebitmapex.svn.codeplex.com/svn/trunk/Source/WriteableBitmapEx/WriteableBitmapBaseExtensions.cs $
//   Id:                $Id: WriteableBitmapBaseExtensions.cs 113191 2015-03-05 17:18:24Z unknown $
//
//
//   Copyright © 2009-2015 Rene Schulte and WriteableBitmapEx Contributors
//
//   This code is open source. Please read the License.txt for details. No worries, we won't sue you! ;)
//


using System;
using System.Collections.Generic;

using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PixelFarm.BitmapBufferEx;
namespace WinFormGdiPlus
{
    public partial class FormShape : Form
    {

        private int shapeCount = 100;
        private static Random rand = new Random();
        private int frameCounter = 0;

        enum SampleName
        {
            DrawStaticShapes,
            DrawEllipses,
            DrawEllipsesFlower,
        }

        public FormShape()
        {
            InitializeComponent();
            //setup

        }
        private void FormShape_Load(object sender, EventArgs e)
        {
            this.listBox1.Items.AddRange(
                new object[]
                {
                    SampleName.DrawEllipses,
                    SampleName.DrawEllipsesFlower,
                    SampleName.DrawStaticShapes
                });
            listBox1.SelectedIndex = 0;
            listBox1.SelectedIndexChanged += (s1, e1) => RenderSelectedSample();
        }
        void RenderSelectedSample()
        {
            SampleName sampleName = (SampleName)listBox1.SelectedItem;

            //render!
            using (Graphics g = this.panel1.CreateGraphics())
            using (Bitmap bmp1 = new Bitmap(400, 500))
            using (LockBmp bmplock = bmp1.Lock())
            {
                BitmapBuffer wb = bmplock.CreateNewBitmapBuffer();
                switch (sampleName)
                {
                    case SampleName.DrawEllipses:
                        DrawEllipses(wb);
                        break;
                    case SampleName.DrawEllipsesFlower:
                        DrawEllipsesFlower(wb);
                        break;
                    case SampleName.DrawStaticShapes:
                        DrawStaticShapes(wb);
                        break;
                }

                bmplock.WriteAndUnlock();
                //

                g.Clear(System.Drawing.Color.White);
                g.DrawImage(bmp1, 0, 0);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            RenderSelectedSample();
        }

        /// <summary>
        /// Random color fully opaque
        /// </summary>
        /// <returns></returns>
        private static int GetRandomColor()
        {
            return (int)(0xFF000000 | (uint)rand.Next(0xFFFFFF));
        }

        /// <summary>
        /// Draws circles that decrease in size to build a flower that is animated
        /// </summary>
        private void DrawEllipsesFlower(BitmapBuffer writeableBmp)
        {


            // Init some size vars
            int w = writeableBmp.PixelWidth - 2;
            int h = writeableBmp.PixelHeight - 2;

            // Wrap updates in a GetContext call, to prevent invalidation and nested locking/unlocking during this block
            using (writeableBmp.GetBitmapContext())
            {
                // Increment frame counter
                if (++frameCounter >= int.MaxValue || frameCounter < 1)
                {
                    frameCounter = 1;
                }
                double s = Math.Sin(frameCounter * 0.01);
                if (s < 0)
                {
                    s *= -1;
                }

                // Clear 
                writeableBmp.Clear();

                // Draw center circle
                int xc = w >> 1;
                int yc = h >> 1;
                // Animate base size with sine
                int r0 = (int)((w + h) * 0.07 * s) + 10;

               PixelFarm.BitmapBufferEx.ColorInt color_brown =PixelFarm.BitmapBufferEx.ColorInt.FromArgb(
                    255, System.Drawing.Color.Brown.R, System.Drawing.Color.Brown.G, System.Drawing.Color.Brown.B);

                writeableBmp.DrawEllipseCentered(xc, yc, r0, r0, color_brown);

                // Draw outer circles
                int dec = (int)((w + h) * 0.0045f);
                int r = (int)((w + h) * 0.025f);
                int offset = r0 + r;
                for (int i = 1; i < 6 && r > 1; i++)
                {
                    for (double f = 1; f < 7; f += 0.7)
                    {
                        // Calc postion based on unit circle
                        int xc2 = (int)(Math.Sin(frameCounter * 0.002 * i + f) * offset + xc);
                        int yc2 = (int)(Math.Cos(frameCounter * 0.002 * i + f) * offset + yc);
                        int col = (int)(0xFFFF0000 | (uint)(0x1A * i) << 8 | (uint)(0x20 * f));
                        writeableBmp.DrawEllipseCentered(xc2, yc2, r, r, col);
                    }
                    // Next ring
                    offset += r;
                    r -= dec;
                    offset += r;
                }

                // Invalidates on exit of using block
            }
        }
        /// <summary>
        /// Draws random ellipses
        /// </summary>
        private void DrawEllipses(BitmapBuffer writeableBmp)
        {
            // Init some size vars
            int w = writeableBmp.PixelWidth - 2;
            int h = writeableBmp.PixelHeight - 2;
            int wh = w >> 1;
            int hh = h >> 1;

            // Wrap updates in a GetContext call, to prevent invalidation and nested locking/unlocking during this block
            using (writeableBmp.GetBitmapContext())
            {
                // Clear 
                writeableBmp.Clear();

                // Draw Ellipses
                for (int i = 0; i < shapeCount; i++)
                {
                    writeableBmp.DrawEllipse(rand.Next(wh), rand.Next(hh), rand.Next(wh, w), rand.Next(hh, h),
                                             GetRandomColor());
                }

                // Invalidates on exit of using block
            }
        }
        /// <summary>
        /// Draws the different types of shapes.
        /// </summary>
        private void DrawStaticShapes(BitmapBuffer writeableBmp)
        {
            // Wrap updates in a GetContext call, to prevent invalidation and nested locking/unlocking during this block
            using (writeableBmp.GetBitmapContext())
            {
                // Init some size vars
                int w = writeableBmp.PixelWidth - 2;
                int h = writeableBmp.PixelHeight - 2;
                int w3rd = w / 3;
                int h3rd = h / 3;
                int w6th = w3rd >> 1;
                int h6th = h3rd >> 1;

                // Clear 
                writeableBmp.Clear();

                // Draw some points
                for (int i = 0; i < 200; i++)
                {
                    writeableBmp.SetPixel(rand.Next(w3rd), rand.Next(h3rd), GetRandomColor());
                }

                // Draw Standard shapes
                writeableBmp.DrawLine(rand.Next(w3rd, w3rd * 2), rand.Next(h3rd), rand.Next(w3rd, w3rd * 2), rand.Next(h3rd),
                                      GetRandomColor());
                writeableBmp.DrawTriangle(rand.Next(w3rd * 2, w - w6th), rand.Next(h6th), rand.Next(w3rd * 2, w),
                                          rand.Next(h6th, h3rd), rand.Next(w - w6th, w), rand.Next(h3rd),
                                          GetRandomColor());

                writeableBmp.DrawQuad(rand.Next(0, w6th), rand.Next(h3rd, h3rd + h6th), rand.Next(w6th, w3rd),
                                      rand.Next(h3rd, h3rd + h6th), rand.Next(w6th, w3rd),
                                      rand.Next(h3rd + h6th, 2 * h3rd), rand.Next(0, w6th), rand.Next(h3rd + h6th, 2 * h3rd),
                                      GetRandomColor());
                writeableBmp.DrawRectangle(rand.Next(w3rd, w3rd + w6th), rand.Next(h3rd, h3rd + h6th),
                                           rand.Next(w3rd + w6th, w3rd * 2), rand.Next(h3rd + h6th, 2 * h3rd),
                                           GetRandomColor());

                // Random polyline
                int[] p = new int[rand.Next(7, 10) * 2];
                for (int j = 0; j < p.Length; j += 2)
                {
                    p[j] = rand.Next(w3rd * 2, w);
                    p[j + 1] = rand.Next(h3rd, 2 * h3rd);
                }
                writeableBmp.DrawPolyline(p, GetRandomColor());

                // Random closed polyline
                p = new int[rand.Next(6, 9) * 2];
                for (int j = 0; j < p.Length - 2; j += 2)
                {
                    p[j] = rand.Next(w3rd);
                    p[j + 1] = rand.Next(2 * h3rd, h);
                }
                p[p.Length - 2] = p[0];
                p[p.Length - 1] = p[1];
                writeableBmp.DrawPolyline(p, GetRandomColor());

                // Ellipses
                writeableBmp.DrawEllipse(rand.Next(w3rd, w3rd + w6th), rand.Next(h3rd * 2, h - h6th),
                                         rand.Next(w3rd + w6th, w3rd * 2), rand.Next(h - h6th, h), GetRandomColor());
                writeableBmp.DrawEllipseCentered(w - w6th, h - h6th, w6th >> 1, h6th >> 1, GetRandomColor());


                // Draw Grid
                writeableBmp.DrawLine(0, h3rd, w, h3rd, Colors.Black);
                writeableBmp.DrawLine(0, 2 * h3rd, w, 2 * h3rd, Colors.Black);
                writeableBmp.DrawLine(w3rd, 0, w3rd, h, Colors.Black);
                writeableBmp.DrawLine(2 * w3rd, 0, 2 * w3rd, h, Colors.Black);

                // Invalidates on exit of using block
            }
        }


    }
}
