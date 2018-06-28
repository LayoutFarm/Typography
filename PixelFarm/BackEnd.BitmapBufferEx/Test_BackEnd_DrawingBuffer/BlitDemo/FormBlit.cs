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
using System.ComponentModel;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using PixelFarm.BitmapBufferEx;

 

namespace WinFormGdiPlus
{
    public partial class FormBlit : Form
    {
        Timer timer1;
        public FormBlit()
        {
            InitializeComponent();

         

        }

        private void RollControl1_ValueChanged(object sender, EventArgs e)
        {

            //StringBuilder stbuilder = new StringBuilder();
            //stbuilder.AppendLine("roll-angle: " + rollControl1.Angle);
            //stbuilder.AppendLine("roll-direction: " + rollControl1.RollDirection);
            //stbuilder.AppendLine("roll-amount: " + rollControl1.RollAmount);
            //this.textBox1.Text = stbuilder.ToString();

            //var token = new RotateZoomEffectConfigToken(true, 0, 0, 0, 1.0f, PointF.Empty, false, false);

            //double angle = rollControl1.RollDirection * Math.PI / 180;
            //double dist = rollControl1.RollAmount;

            //if (double.IsNaN(angle))
            //{
            //    angle = 0;
            //    dist = 0;
            //}

            //int trackBackZoomValue = 512;//trackBarZoom.Value
            //token.Offset = new PointF(0, 0);// panControl.Position;
            //token.PreRotateZ = (float)(angle);
            //token.PostRotateZ = (float)(-angle - rollControl1.Angle * Math.PI / 180);
            //token.Tilt = (float)Math.Asin(dist / 90);
            //token.SourceAsBackground = false;//*** keepBackgroundCheckBox.Checked;
            //token.Tile = false;// tileSourceCheckBox.Checked;
            //token.Zoom = (float)Math.Pow(2.0, (trackBackZoomValue - 512) / 128.0);


            ////if (this.angleUpDown.Value != (decimal)this.rollControl.Angle)
            ////{
            ////    this.angleUpDown.Value = (decimal)this.rollControl.Angle;
            ////}

            ////if (this.twistAngleUpDown.Value != -(decimal)this.rollControl.RollDirection)
            ////{
            ////    this.twistAngleUpDown.Value = -(decimal)this.rollControl.RollDirection;
            ////}

            ////if (this.twistRadiusUpDown.Value != (decimal)this.rollControl.RollAmount)
            ////{
            ////    this.twistRadiusUpDown.Value = (decimal)this.rollControl.RollAmount;
            ////}

            ////UpdateUpDowns();
            ////FinishTokenUpdate();


        }

        private void FormBlit_Load(object sender, EventArgs e)
        {
            particleBmp = LoadBitmapAsReadonly("../../FlowerBurst.jpg");
            circleBmp = LoadBitmapAsReadonly("../../circle.png");


            timer1 = new Timer();
            timer1.Interval = 30;
            timer1.Tick += (s1, e1) => this.Invoke(new MethodInvoker(() => UpdateRenderFrame()));
            particleSourceRect = new RectD(0, 0, 64, 64);

            //bmp = BitmapFactory.New(640, 480);
            //bmp.Clear(Colors.Black);
            destBmp = new Bitmap(400, 500);
            g = this.panel1.CreateGraphics();

            emitter = new ParticleEmitter();

            //CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
            this.MouseMove += new MouseEventHandler(MainPage_MouseMove);
        }

        Graphics g;
        Bitmap destBmp;
        void UpdateRenderFrame()
        {
            // Wrap updates in a GetContext call, to prevent invalidation and nested locking/unlocking during this block
            // NOTE: This is not strictly necessary for the SL version as this is a WPF feature, however we include it here for completeness and to show
            // a similar API to WPF 
            //render! 
            using (LockBmp bmplock = destBmp.Lock())
            {
                BitmapBuffer wb = bmplock.CreateNewBitmapBuffer();
                emitter.TargetBitmap = wb;
                emitter.ParticleBitmap = particleBmp;

                wb.Clear(Colors.Black);


                double elapsed = (DateTime.Now - lastUpdate).TotalSeconds;
                lastUpdate = DateTime.Now;
                emitter.Update(elapsed);
                //			bmp.Blit(new Point(100, 150), circleBmp, new Rect(0, 0, 200, 200), Colors.Red, BlendMode.Additive);
                //			bmp.Blit(new Point(160, 55), circleBmp, new Rect(0, 0, 200, 200), Color.FromArgb(255, 0, 255, 0), BlendMode.Additive);
                //			bmp.Blit(new Point(220, 150), circleBmp, new Rect(0, 0, 200, 200), Colors.Blue, BlendMode.Additive);

                //double timeNow = _stopwatch.ElapsedMilliseconds;
                //double elapsedMilliseconds = timeNow - _lastTime;
                //_lowestFrameTime = Math.Min(_lowestFrameTime, elapsedMilliseconds);
                //// FpsCounter.Text = string.Format("FPS: {0:0.0} / Max: {1:0.0}", 1000.0 / elapsedMilliseconds, 1000.0 / _lowestFrameTime);
                //_lastTime = timeNow;

                //
                bmplock.WriteAndUnlock();
                //

                g.Clear(System.Drawing.Color.White);
                g.DrawImage(destBmp, 0, 0);
            }


        }

        BitmapBuffer circleBmp;
        BitmapBuffer particleBmp;
        RectD particleSourceRect;
        ParticleEmitter emitter = new ParticleEmitter();
        DateTime lastUpdate = DateTime.Now;

        private double _lastTime;
        private double _lowestFrameTime;

        static BitmapBuffer LoadBitmapAsReadonly(string path)
        {
            using (Bitmap bmp = new Bitmap(path))
            using (var lockBmp = new LockBmp(bmp))
            {
                return lockBmp.CreateNewBitmapBuffer();
            }
        }
        void MainPage_MouseMove(object sender, MouseEventArgs e)
        {
            //emitter.Center = e.GetPosition(image);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            //start ...
            UpdateRenderFrame();
        }
        public static BitmapBuffer Overlay(BitmapBuffer bmp, BitmapBuffer overlay, PixelFarm.BitmapBufferEx.PointD location)
        {
            BitmapBuffer result = bmp.Clone();
            var size = new PixelFarm.BitmapBufferEx.SizeD(overlay.PixelWidth, overlay.PixelHeight);
            result.Blit(new PixelFarm.BitmapBufferEx.RectD(location, size), overlay,
                new RectD(new PixelFarm.BitmapBufferEx.PointD(0, 0), size),
                BitmapBufferExtensions.BlendMode.Multiply);
            return result;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BitmapBuffer unmodifiedBmp = LoadBitmapAsReadonly("../../02.jpg");
            BitmapBuffer sticker = LoadBitmapAsReadonly("../../01.jpg");

            BitmapBuffer overlayResult = Overlay(unmodifiedBmp, sticker, new PixelFarm.BitmapBufferEx.PointD(10, 10));

            using (LockBmp bmplock = destBmp.Lock())
            {
                BitmapBuffer wb = bmplock.CreateNewBitmapBuffer();
                wb.Clear(Colors.Black);

                wb.Blit(new RectD(0, 0, overlayResult.PixelWidth, overlayResult.PixelHeight),
                        overlayResult,
                        new RectD(0, 0, overlayResult.PixelWidth, overlayResult.PixelHeight));

                bmplock.WriteAndUnlock();

                g.Clear(System.Drawing.Color.White);
                g.DrawImage(destBmp, 0, 0);
            }


        }

        private void button3_Click(object sender, EventArgs e)
        {
            BitmapBuffer unmodifiedBmp = LoadBitmapAsReadonly("../../02.jpg");
            BitmapBuffer cropBmp = unmodifiedBmp.Crop(10, 10, 40, 40);


            using (LockBmp bmplock = destBmp.Lock())
            {
                BitmapBuffer wb = bmplock.CreateNewBitmapBuffer();
                wb.Clear(Colors.White);

                wb.Blit(new RectD(0, 0, cropBmp.PixelWidth, cropBmp.PixelHeight),
                        cropBmp,
                        new RectD(0, 0, cropBmp.PixelWidth, cropBmp.PixelHeight));

                bmplock.WriteAndUnlock();

                g.Clear(System.Drawing.Color.White);
                g.DrawImage(destBmp, 0, 0);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            BitmapBuffer unmodifiedBmp = LoadBitmapAsReadonly("../../02.jpg");
            BitmapBuffer rotateBmp = unmodifiedBmp.Rotate(BitmapBufferExtensions.FastRotateAngle.Rotate270);


            using (LockBmp bmplock = destBmp.Lock())
            {
                BitmapBuffer wb = bmplock.CreateNewBitmapBuffer();
                wb.Clear(Colors.White);

                wb.Blit(new RectD(0, 0, rotateBmp.PixelWidth, rotateBmp.PixelHeight),
                        rotateBmp,
                        new RectD(0, 0, rotateBmp.PixelWidth, rotateBmp.PixelHeight));

                bmplock.WriteAndUnlock();

                g.Clear(System.Drawing.Color.White);
                g.DrawImage(destBmp, 0, 0);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            BitmapBuffer unmodifiedBmp = LoadBitmapAsReadonly("../../02.jpg");
            BitmapBuffer flipImg = unmodifiedBmp.Flip(BitmapBufferExtensions.FlipMode.Horizontal);


            using (LockBmp bmplock = destBmp.Lock())
            {
                BitmapBuffer wb = bmplock.CreateNewBitmapBuffer();
                wb.Clear(Colors.White);

                wb.Blit(new RectD(0, 0, flipImg.PixelWidth, flipImg.PixelHeight),
                        flipImg,
                        new RectD(0, 0, flipImg.PixelWidth, flipImg.PixelHeight));

                bmplock.WriteAndUnlock();

                g.Clear(System.Drawing.Color.White);
                g.DrawImage(destBmp, 0, 0);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            BitmapBuffer unmodifiedBmp = LoadBitmapAsReadonly("../../02.jpg");
            BitmapBuffer flipImg = unmodifiedBmp.RotateFree(20, false);


            using (LockBmp bmplock = destBmp.Lock())
            {
                BitmapBuffer wb = bmplock.CreateNewBitmapBuffer();
                wb.Clear(Colors.White);

                wb.Blit(new RectD(0, 0, flipImg.PixelWidth, flipImg.PixelHeight),
                        flipImg,
                        new RectD(0, 0, flipImg.PixelWidth, flipImg.PixelHeight));

                bmplock.WriteAndUnlock();

                g.Clear(System.Drawing.Color.White);
                g.DrawImage(destBmp, 0, 0);
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            //....



        }
    }
}
