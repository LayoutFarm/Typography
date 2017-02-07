/* Poly2Tri
 * Copyright (c) 2009-2010, Poly2Tri Contributors
 * http://code.google.com/p/poly2tri/
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * * Redistributions of source code must retain the above copyright notice,
 *   this list of conditions and the following disclaimer.
 * * Redistributions in binary form must reproduce the above copyright notice,
 *   this list of conditions and the following disclaimer in the documentation
 *   and/or other materials provided with the distribution.
 * * Neither the name of Poly2Tri nor the names of its contributors may be
 *   used to endorse or promote products derived from this software without specific
 *   prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Poly2Tri;

namespace SwfTest
{
    [System.ComponentModel.DesignerCategory("")]
    class TestForm : Form
    {
        List<Polygon> Polygons;

        int i;

        Timer rotation;

        public TestForm()
        {
            ClientSize = new Size(1000, 1000);
            DoubleBuffered = true;
            Text = "Just a test";
            Visible = true;

            Polygons = ExampleData.Polygons.ToList();
            foreach (var poly in Polygons) try
                {
                    P2T.Triangulate(poly);
                }
                catch (Exception) { }

            rotation = new Timer()
                {
                    Enabled = true
                ,
                    Interval = 500
                };
            rotation.Tick += (o, e) =>
            {
                i = (i + 1) % Polygons.Count;
                Invalidate();
            };

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            float xmin = float.MaxValue, xmax = float.MinValue;
            float ymin = float.MaxValue, ymax = float.MinValue;

            foreach (var point in Polygons[i].Points)
            {
                xmin = Math.Min(xmin, point.Xf);
                xmax = Math.Max(xmax, point.Xf);
                ymin = Math.Min(ymin, point.Yf);
                ymax = Math.Max(ymax, point.Yf);
            }

            if (xmin < xmax && ymin < ymax)
            {
                var fx = e.Graphics;
                float zoom = 0.8f * Math.Min(ClientSize.Width / (xmax - xmin), ClientSize.Height / (ymax - ymin));
                fx.TranslateTransform(ClientSize.Width / 2, ClientSize.Height / 2); // center coordinate system on screen center
                fx.ScaleTransform(zoom, -zoom);
                fx.TranslateTransform(-(xmax + xmin) / 2, -(ymax + ymin) / 2); // center image

                using (var pen = new Pen(Color.Green, 1.0f / zoom))
                {
                    foreach (var tri in Polygons[i].Triangles)
                    {
                        fx.DrawPolygon(pen, new PointF[]
					        { new PointF(tri.P0.Xf,tri.P0.Yf)
					        , new PointF(tri.P1.Xf,tri.P1.Yf)
					        , new PointF(tri.P2.Xf,tri.P2.Yf)
					        });
                    }
                }
                fx.ResetTransform();
                fx.DrawImage(ExampleData.Logo256x256, ClientSize.Width - 256 - 10, 10, 256, 256);
            }

            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            Invalidate();
            base.OnResize(e);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TestForm());
        }
    }
}
