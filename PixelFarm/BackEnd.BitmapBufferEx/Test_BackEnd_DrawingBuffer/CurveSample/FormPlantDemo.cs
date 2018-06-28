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
using System.Windows.Forms;

using WinFormGdiPlus.PlantDemo;
using PixelFarm.BitmapBufferEx;

namespace WinFormGdiPlus
{
    public partial class FormPlantDemo : Form
    {
       


        public FormPlantDemo()
        {
            InitializeComponent();
            Init();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            //render!
            using (System.Drawing.Graphics g = this.panel1.CreateGraphics())
            using (System.Drawing.Bitmap bmp1 = new System.Drawing.Bitmap(panel1.Width, panel1.Height))
            using (LockBmp bmplock = bmp1.Lock())
            {
                BitmapBuffer wb = bmplock.CreateNewBitmapBuffer();

                Draw(wb);
                bmplock.WriteAndUnlock();
                //

                g.Clear(System.Drawing.Color.White);
                g.DrawImage(bmp1, 0, 0);
            }


        }

        // private Stopwatch _stopwatch = Stopwatch.StartNew();
        private double _lastTime = 0.0;
        private double _lowestFrameTime = double.MaxValue;

        //public MainWindow()
        //{
        //    InitializeComponent();

        //    CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        //}

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            //double timeNow = _stopwatch.ElapsedMilliseconds;
            //double elapsed = timeNow - _lastTime;
            //_lowestFrameTime = Math.Min(_lowestFrameTime, elapsed);
            //FpsCounter.Text = string.Format("FPS: {0:0.0} / Max: {1:0.0}", 1000.0 / elapsed, 1000.0 / _lowestFrameTime);
            //_lastTime = timeNow;
        }


        private const int PointSize = 10;
        private const int PointSizeHalf = PointSize >> 1;
        private const int PointCount = 3000;




        private List<ControlPoint> points;
        private ControlPoint PickedPoint;
        private Random rand;
        private bool isInDelete;
        private Plant plant;



        public float Tension { get; set; }




        private void Init()
        {
            // Init vars
            rand = new Random();
            points = new List<ControlPoint>();
            isInDelete = false;
            Tension = 0.5f;

            // Init plant
            int vw = (int)this.panel1.Width;
            int vh = (int)this.panel1.Height;

            plant = new Plant(new Vector(vw >> 1, vh), new Vector(1, -1), vw, vh);
            plant.BranchLenMin = (int)(vw * 0.17f);
            plant.BranchLenMax = plant.BranchLenMin + (plant.BranchLenMin >> 1);
            plant.MaxGenerations = 6;
            plant.MaxBranchesPerGeneration = 80;
            plant.BranchPoints.AddRange(new List<BranchPoint>
             {
                new BranchPoint(1f,  40), // 40° Right at 100% of branch
                new BranchPoint(1f,   5), //  5° Right at 100% of branch
                new BranchPoint(1f,  -5), //  5° Left  at 100% of branch
                new BranchPoint(1f, -40), // 40° Left  at 100% of branch
             });
            ReloadRandomPoints();
            //ChkDemoPerf.Content = String.Format("Perf. Demo {0} points", PointCount);
            //CheckDemoPlant_Checked(this, null);
            //this.DataContext = this;

            // Init WriteableBitmap
            //writeableBmp = BitmapFactory.New((int)vw, (int)vh);
            //ImageViewport.Source = writeableBmp;

            //// Start render loop
            //CompositionTarget.Rendering += (s, e) =>
            //{
            //    if (ChkDemoPlant.IsChecked.Value)
            //    {
            //        plant.Grow();
            //        plant.Draw(this.writeableBmp);
            //    }
            //    else if (ChkDemoPerf.IsChecked.Value)
            //    {
            //        AddRandomPoints();
            //        Draw();
            //    }
            //};
        }

        private void ReloadRandomPoints()
        {
            int w = (int)this.panel1.Width;
            int h = (int)this.panel1.Height;

            points.Clear();
            for (int i = 0; i < PointCount; i++)
            {
                points.Add(new ControlPoint(rand.Next(0, w), rand.Next(0, h)));
            }
        }

        private void Draw(BitmapBuffer writeableBmp)
        {
            if (this.points != null)
            {
                ReloadRandomPoints();
                // Wrap updates in a GetContext call, to prevent invalidation and nested locking/unlocking during this block
                using (writeableBmp.GetBitmapContext())
                {
                    writeableBmp.Clear();
                    DrawPoints(writeableBmp);
                    //DrawBeziers(writeableBmp);
                    //  DrawCardinal(writeableBmp);

                    //if (ChkShowPoints.IsChecked.Value)
                    //{
                    //    DrawPoints();
                    //}
                    //if (RBBezier.IsChecked.Value)
                    //{
                    //    DrawBeziers();
                    //}
                    //else if (RBCardinal.IsChecked.Value)
                    //{
                    //    DrawCardinal();
                    //}
                }
            }
        }

        private void DrawPoints(BitmapBuffer writeableBmp)
        {
            foreach (var p in points)
            {
                DrawPoint(writeableBmp, p, Colors.Blue);
            }
            if (PickedPoint != null)
            {
                DrawPoint(writeableBmp, PickedPoint, Colors.Red);
            }
        }

        private void DrawPoint(BitmapBuffer writeableBmp, ControlPoint p, ColorInt color)
        {
            var x1 = p.X - PointSizeHalf;
            var y1 = p.Y - PointSizeHalf;
            var x2 = p.X + PointSizeHalf;
            var y2 = p.Y + PointSizeHalf;
            writeableBmp.DrawRectangle(x1, y1, x2, y2, color);
        }

        private void DrawBeziers(BitmapBuffer writeableBmp)
        {
            if (points.Count > 3)
            {
                writeableBmp.DrawBeziers(GetPointArray(), Colors.Red);
            }
        }

        private void DrawCardinal(BitmapBuffer writeableBmp)
        {
            if (points.Count > 2)
            {
                writeableBmp.DrawCurve(GetPointArray(), Tension, Colors.Red);
            }
        }

        private int[] GetPointArray()
        {
            int[] pts = new int[points.Count * 2];
            for (int i = 0; i < points.Count; i++)
            {
                pts[i * 2] = points[i].X;
                pts[i * 2 + 1] = points[i].Y;
            }
            return pts;
        }

        private ControlPoint GetMousePoint(MouseEventArgs e)
        {
            //temp
            return new ControlPoint();
            //return new ControlPoint(e.GetPosition(ImageViewport));
        }

        private void RemovePickedPointPoint()
        {
            //if (PickedPoint != null)
            //{
            //    points.Remove(PickedPoint);
            //    PickedPoint = null;
            //    isInDelete = true;
            //    Draw();
            //}
        }



        //private void UserControl_Loaded(object sender, RoutedEventArgs e)
        //{

        //}

        //private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    // Only add new control point is [DEL] wasn't pressed
        //    if (!isInDelete && PickedPoint == null)
        //    {
        //        points.Add(GetMousePoint(e));
        //    }
        //    PickedPoint = null;
        //    isInDelete = false;
        //    Draw();
        //}

        //private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    // Pick control point
        //    var mp = GetMousePoint(e);
        //    PickedPoint = (from p in points
        //                   where p.X > mp.X - PointSizeHalf && p.X < mp.X + PointSizeHalf
        //                      && p.Y > mp.Y - PointSizeHalf && p.Y < mp.Y + PointSizeHalf
        //                   select p).FirstOrDefault();
        //    Draw();
        //}

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            //// Move control point
            //if (PickedPoint != null)
            //{
            //    var mp = GetMousePoint(e);
            //    PickedPoint.X = mp.X;
            //    PickedPoint.Y = mp.Y;
            //    Draw();
            //}
        }

        //protected override void OnKeyDown(KeyEventArgs e)
        //{
        //    // Delete selected control point
        //    base.OnKeyDown(e);
        //    if (e.Key == Key.Delete)
        //    {
        //        RemovePickedPointPoint();
        //    }
        //}

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    _lowestFrameTime = double.MaxValue;
        //    // Restart plant
        //    if (plant != null && ChkDemoPlant.IsChecked.Value)
        //    {
        //        plant.Clear();
        //    }

        //    // Remove all comtrol points
        //    else if (this.points != null)
        //    {
        //        this.points.Clear();
        //        Draw();
        //    }
        //}

        //private void BtnSave_Click(object sender, RoutedEventArgs e)
        //{
        //    // Take snapshot
        //    var clone = this.writeableBmp.Clone();

        //    // Save as TGA
        //    SaveFileDialog dialog = new SaveFileDialog { Filter = "TGA Image (*.tga)|*.tga" };
        //    if (dialog.ShowDialog().Value)
        //    {
        //        using (var fileStream = dialog.OpenFile())
        //        {
        //            clone.WriteTga(fileStream);
        //        }
        //    }
        //}

        //private void CheckBox_Checked(object sender, RoutedEventArgs e)
        //{
        //    _lowestFrameTime = double.MaxValue;
        //    // Refresh
        //    Draw();
        //}

        //private void RadioButton_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (SldTension == null)
        //        return;

        //    // Tension only makes sense for cardinal splines
        //    if (RBCardinal != null)
        //    {
        //        if (RBCardinal.IsChecked.Value)
        //        {
        //            SldTension.Opacity = 1;
        //        }
        //        else
        //        {
        //            SldTension.Opacity = 0;
        //        }
        //    }
        //    Draw();
        //}

        //private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    // Set tension text
        //    if (this.TxtTension != null)
        //    {
        //        this.TxtTension.Text = String.Format("Tension: {0:f2}", Tension);
        //        Draw();
        //    }

        //    // Update plant
        //    if (plant != null && ChkDemoPlant.IsChecked.Value)
        //    {
        //        plant.Tension = Tension;
        //        plant.Draw(this.writeableBmp);
        //    }
        //}

        //private void CheckDemoPlant_UnChecked(object sender, RoutedEventArgs e)
        //{
        //    // Show irrelevant controls for plant growth demo
        //    if (SPCurveMode != null && ChkDemoPerf != null && ChkShowPoints != null)
        //    {
        //        SPCurveMode.Opacity = 1;
        //        ChkDemoPerf.Opacity = 1;
        //        ChkShowPoints.Opacity = 1;
        //        TxtUsage.Opacity = 1;
        //        BtnClear.Content = "Clear";
        //        Draw();
        //    }
        //}

        //private void CheckDemoPlant_Checked(object sender, RoutedEventArgs e)
        //{
        //    // Hide irrelevant controls for plant growth demo
        //    if (SPCurveMode != null && ChkDemoPerf != null && ChkShowPoints != null)
        //    {
        //        SPCurveMode.Opacity = 0;
        //        ChkDemoPerf.Opacity = 0;
        //        ChkShowPoints.Opacity = 0;
        //        TxtUsage.Opacity = 0;
        //        BtnClear.Content = "Restart";
        //    }
        //}
    }
}
