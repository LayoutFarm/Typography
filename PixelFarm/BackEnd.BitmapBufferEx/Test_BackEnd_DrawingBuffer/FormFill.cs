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
using System.Windows.Forms;

using PixelFarm.BitmapBufferEx;
namespace WinFormGdiPlus
{
    public partial class FormFill : Form
    {
        enum SampleName
        {
            StaticShapes,
            DrawShapes,
            DrawFillDemo,
        }

        private static Random rand = new Random();
        public FormFill()
        {
            InitializeComponent();
            this.listBox1.Items.AddRange(
              new object[]
              {
                    SampleName.StaticShapes,
                    SampleName.DrawShapes,
                    SampleName.DrawFillDemo

              });
            listBox1.SelectedIndex = 0;
            listBox1.SelectedIndexChanged += (s1, e1) => RenderSelectedSample();

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

        void RenderSelectedSample()
        {
            SampleName sampleName = (SampleName)listBox1.SelectedItem;
            using (Graphics g = this.panel1.CreateGraphics())
            using (Bitmap bmp1 = new Bitmap(400, 500))
            using (LockBmp bmplock = bmp1.Lock())
            {
                BitmapBuffer wb = bmplock.CreateNewBitmapBuffer();
                switch (sampleName)
                {
                    case SampleName.StaticShapes:
                        DrawStaticShapes(wb);
                        break;
                    case SampleName.DrawShapes:
                        DrawShapes(wb);
                        break;
                    case SampleName.DrawFillDemo:
                        DrawFillDemo(wb);
                        break;
                }

                bmplock.WriteAndUnlock();
                //

                g.Clear(System.Drawing.Color.White);
                g.DrawImage(bmp1, 0, 0);
            }
        }
        /// <summary>
        /// Draws the different types of shapes.
        /// </summary>
        private void DrawStaticShapes(BitmapBuffer writeableBmp)
        {
            // HideShapeCountText();

            // Wrap updates in a GetContext call, to prevent invalidation and nested locking/unlocking during this block
            using (writeableBmp.GetBitmapContext())
            {
                // Init some size vars
                int w = writeableBmp.PixelWidth;
                int h = writeableBmp.PixelHeight;
                int w3 = w / 3;
                int h3 = h / 3;
                int w6 = w3 >> 1;
                int h6 = h3 >> 1;
                int w12 = w6 >> 1;
                int h12 = h6 >> 1;

                // Clear 
                writeableBmp.Clear();

                // Fill closed concave polygon
                var p = new int[]
                            {
                                    w12 >> 1, h12,
                                    w6, h3 - (h12 >> 1),
                                    w3 - (w12 >> 1), h12,
                                    w6 + w12, h12,
                                    w6, h6 + h12,
                                    w12, h12,
                                    w12 >> 1, h12,
                            };
                writeableBmp.FillPolygonsEvenOdd(new[] { p }, GetRandomColor());

                // Fill closed convex polygon
                p = new int[]
                        {
                                w3 + w6, h12 >> 1,
                                w3 + w6 + w12, h12,
                                w3 + w6 + w12, h6 + h12,
                                w3 + w6, h6 + h12 + (h12 >> 1),
                                w3 + w12, h6 + h12,
                                w3 + w12, h12,
                                w3 + w6, h12 >> 1,
                        };
                writeableBmp.FillPolygon(p, GetRandomColor());

                // Fill Triangle + Quad
                writeableBmp.FillTriangle(2 * w3 + w6, h12 >> 1, 2 * w3 + w6 + w12, h6 + h12, 2 * w3 + w12, h6 + h12,
                                          GetRandomColor());
                writeableBmp.FillQuad(w6, h3 + (h12 >> 1), w6 + w12, h3 + h6, w6, h3 + h6 + h12 + (h12 >> 1), w12,
                                      h3 + h6, GetRandomColor());

                // Fill Ellipses
                writeableBmp.FillEllipse(rand.Next(w3, w3 + w6), rand.Next(h3, h3 + h6), rand.Next(w3 + w6, 2 * w3),
                                         rand.Next(h3 + h6, 2 * h3), GetRandomColor());
                writeableBmp.FillEllipseCentered(2 * w3 + w6, h3 + h6, w12, h12, GetRandomColor());

                // Fill closed Cardinal Spline curve
                p = new int[]
                        {
                                w12 >> 1, 2*h3 + h12,
                                w6, h - (h12 >> 1),
                                w3 - (w12 >> 1), 2*h3 + h12,
                                w6 + w12, 2*h3 + h12,
                                w6, 2*h3 + (h12 >> 1),
                                w12, 2*h3 + h12,
                        };
                writeableBmp.FillCurveClosed(p, 0.5f, GetRandomColor());

                // Fill closed Beziér curve
                p = new int[]
                        {
                                w3 + w12, 2*h3 + h6 + h12,
                                w3 + w6 + (w12 >> 1), 2*h3,
                                w3 + w6 + w12 + (w12 >> 1), 2*h3,
                                w3 + w6 + w12, 2*h3 + h6 + h12,
                        };
                writeableBmp.FillBeziers(p, GetRandomColor());

                // Fill Rectangle
                writeableBmp.FillRectangle(rand.Next(2 * w3, 2 * w3 + w6), rand.Next(2 * h3, 2 * h3 + h6),
                                           rand.Next(2 * w3 + w6, w), rand.Next(2 * h3 + h6, h), GetRandomColor());
                // Fill another rectangle with alpha blending
                writeableBmp.FillRectangle(rand.Next(2 * w3, 2 * w3 + w6), rand.Next(2 * h3, 2 * h3 + h6),
                       rand.Next(2 * w3 + w6, w), rand.Next(2 * h3 + h6, h), GetRandomColor(), true);

               PixelFarm.BitmapBufferEx.ColorInt black =PixelFarm.BitmapBufferEx.ColorInt.FromArgb(255, 0, 0, 0);
                // Draw Grid
                writeableBmp.DrawLine(0, h3, w, h3, Colors.Black);
                writeableBmp.DrawLine(0, 2 * h3, w, 2 * h3, Colors.Black);
                writeableBmp.DrawLine(w3, 0, w3, h, Colors.Black);
                writeableBmp.DrawLine(2 * w3, 0, 2 * w3, h, Colors.Black);

                // Invalidates on exit of Using block
            }

        }


        int shapeCount = 100;
        /// <summary>
        /// Draws random shapes.
        /// </summary>
        private void DrawShapes(BitmapBuffer writeableBmp)
        {
            // Wrap updates in a GetContext call, to prevent invalidation and nested locking/unlocking during this block
            using (writeableBmp.GetBitmapContext())
            {
                // Init some size vars
                int w = writeableBmp.PixelWidth - 2;
                int h = writeableBmp.PixelHeight - 2;
                int w2 = w >> 1;
                int h2 = h >> 1;

                // Clear 
                writeableBmp.Clear();

                // Fill Shapes
                for (int i = 0; i < shapeCount; i++)
                {
                    // Random polygon
                    int[] p = new int[rand.Next(5, 10) * 2];
                    for (int j = 0; j < p.Length; j += 2)
                    {
                        p[j] = rand.Next(w);
                        p[j + 1] = rand.Next(h);
                    }
                    writeableBmp.FillPolygon(p, GetRandomColor());
                }

                // Invalidates on exit of Using block
            }
        }


        private class Circle
        {
            public int Color { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public float Radius { get; set; }
            public float Velocity { get; set; }

            public void Update()
            {
                Radius += Velocity;
            }
        }

        private List<Circle> circles = new List<Circle>();
        private float time;
        private const float timeStep = 0.01f;

        private void DrawFillDemo(BitmapBuffer writeableBmp)
        {


            // Wrap updates in a GetContext call, to prevent invalidation and nested locking/unlocking during this block
            using (writeableBmp.GetBitmapContext())
            {
                // Init some size vars
                int w = writeableBmp.PixelWidth - 2;
                int h = writeableBmp.PixelHeight - 2;
                int w2 = w >> 1;
                int h2 = h >> 1;
                int w4 = w2 >> 1;
                int h4 = h2 >> 1;
                int w8 = w4 >> 1;
                int h8 = h4 >> 1;

                // Clear 
                writeableBmp.Clear();

                // Add circles
                const float startTimeFixed = 1;
                const float endTimeFixed = startTimeFixed + timeStep;
                const float startTimeRandom = 3;
                const float endTimeCurve = 9.7f;
                const int intervalRandom = 2;
                const int maxCircles = 30;

                // Spread fixed position and color circles
                if (time > startTimeFixed && time < endTimeFixed)
                {
                    unchecked
                    {
                        circles.Add(new Circle { X = w8, Y = h8, Radius = 10f, Velocity = 1, Color = (int)0xFFC88717 });
                        circles.Add(new Circle
                        { X = w8, Y = h - h8, Radius = 10f, Velocity = 1, Color = (int)0xFFFB522B });
                        circles.Add(new Circle
                        { X = w - w8, Y = h8, Radius = 10f, Velocity = 1, Color = (int)0xFFDB6126 });
                        circles.Add(new Circle
                        { X = w - w8, Y = h - h8, Radius = 10f, Velocity = 1, Color = (int)0xFFFFCE25 });
                    }
                }

                // Spread random position and color circles
                if (time > startTimeRandom && (int)time % intervalRandom == 0)
                {
                    unchecked
                    {
                        circles.Add(new Circle
                        {
                            X = rand.Next(w),
                            Y = rand.Next(h),
                            Radius = 1f,
                            Velocity = rand.Next(1, 5),
                            Color = rand.Next((int)0xFFFF0000, (int)0xFFFFFFFF),
                        });
                    }
                }

                // Render and update circles
                foreach (var circle in circles)
                {
                    var r = (int)circle.Radius;
                    writeableBmp.FillEllipseCentered(circle.X, circle.Y, r, r, circle.Color);
                    circle.Update();
                }

                if (circles.Count > maxCircles)
                {
                    circles.RemoveAt(0);
                }

                // Fill closed Cardinal Spline curve
                if (time < endTimeCurve)
                {
                    var p = new int[]
                                {
                                    w4, h2,
                                    w2, h2 + h4,
                                    w2 + w4, h2,
                                    w2, h4,
                                };
                    writeableBmp.FillCurveClosed(p, (float)Math.Sin(time) * 7, Colors.Black);
                }

                // Update time
                time += timeStep;

                // Invalidates on exit of Using block
            }
        }
    }
}
