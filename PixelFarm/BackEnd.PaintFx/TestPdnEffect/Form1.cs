//MIT, 2018-present, WinterDev

using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;
using System.Text;
using System.Windows.Forms;
 
using PixelFarm.PaintFx;
using PixelFarm.PaintFx.Effects;

namespace TestPdnEffect
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            rollControl1.ValueChanged += (s, e) => UpdateRotateZoomParameters();
            chkTile.CheckedChanged += (s, e) => UpdateRotateZoomParameters();
            chkKeepBackground.CheckedChanged += (s, e) => UpdateRotateZoomParameters();
            trkZoom.ValueChanged += (s, e) => UpdateRotateZoomParameters();
        }

        RotateZoomEffectConfigToken _rotateZoomConfigToken;


        void UpdateRotateZoomParameters()
        {
            StringBuilder stbuilder = new StringBuilder();
            stbuilder.AppendLine("roll-angle: " + rollControl1.Angle);
            stbuilder.AppendLine("roll-direction: " + rollControl1.RollDirection);
            stbuilder.AppendLine("roll-amount: " + rollControl1.RollAmount);
            this.textBox1.Text = stbuilder.ToString();
            //---------------------
            var token = new RotateZoomEffectConfigToken(true, 0, 0, 0, 1.0f, PixelFarm.Drawing.Point.Empty, false, false);

            double angle = rollControl1.RollDirection * Math.PI / 180;
            double dist = rollControl1.RollAmount;

            if (double.IsNaN(angle))
            {
                angle = 0;
                dist = 0;
            }

            int trackBackZoomValue = trkZoom.Value;//trackBarZoom.Value
            token.Offset = new PixelFarm.Drawing.PointF(0, 0);// panControl.Position;
            token.PreRotateZ = (float)(angle);
            token.PostRotateZ = (float)(-angle - rollControl1.Angle * Math.PI / 180);
            token.Tilt = (float)Math.Asin(dist / 90);
            token.SourceAsBackground = chkKeepBackground.Checked;//*** keepBackgroundCheckBox.Checked;
            token.Tile = chkTile.Checked;
            token.Zoom = (float)Math.Pow(2.0, (trackBackZoomValue - 512) / 128.0);
            _rotateZoomConfigToken = token;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //create surface from memory 
            //on 32 argb format


            Bitmap bmp = new Bitmap("lion1.png");

            var bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int w = bmp.Width;
            int h = bmp.Height;
            int stride = bmpData.Stride;
            int bufferLen = w * h;
            int[] srcBmpBuffer = new int[bufferLen];
            int[] destBmpBuffer = new int[bufferLen];

            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, srcBmpBuffer, 0, srcBmpBuffer.Length);
            bmp.UnlockBits(bmpData);
            //
            unsafe
            {
                fixed (int* srcBmpH = &srcBmpBuffer[0])
                fixed (int* destBmpH = &destBmpBuffer[0])
                {
                    MemHolder srcMemHolder = new MemHolder((IntPtr)srcBmpH, bufferLen);
                    Surface srcSurface = new Surface(stride, w, h, srcMemHolder);

                    MemHolder destMemHolder = new MemHolder((IntPtr)destBmpH, bufferLen);
                    Surface destSurface = new Surface(stride, w, h, destMemHolder);

                    //
                    //apply some filter
                    //


                    //1. test embose renderer
                    EmbossRenderer emboss = new EmbossRenderer();
                    emboss.SetParameters(30);
                    emboss.Render(srcSurface, destSurface, new PixelFarm.Drawing.Rectangle[]{
                            new PixelFarm.Drawing.Rectangle(0,0,w,h)
                        }, 0, 1);

                    //2. test sharpen renderer
                    //SharpenRenderer sharpen = new SharpenRenderer();
                    //sharpen.Amount = 2;
                    //sharpen.Render(srcSurface, destSurface, new PixelFarm.Drawing.Rectangle[]{
                    //        new PixelFarm.Drawing.Rectangle(0,0,w,h)
                    //    }, 0, 0);

                }
            }

            //save to output
            Bitmap outputBmp = new Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bmpData2 = outputBmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Runtime.InteropServices.Marshal.Copy(destBmpBuffer, 0, bmpData2.Scan0, destBmpBuffer.Length);
            outputBmp.UnlockBits(bmpData2);

            this.pictureBox2.Image = outputBmp;
            this.pictureBox1.Image = bmp;


            //process the image
            //then copy to bitmap 
            //


        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateRotateZoomParameters(); //
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //create surface from memory 
            //on 32 argb format


            Bitmap bmp = new Bitmap("lion1.png");
            var bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int w = bmp.Width;
            int h = bmp.Height;
            int stride = bmpData.Stride;
            int bufferLen = w * h;
            int[] srcBmpBuffer = new int[bufferLen];
            int[] destBmpBuffer = new int[bufferLen];

            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, srcBmpBuffer, 0, srcBmpBuffer.Length);
            bmp.UnlockBits(bmpData);
            //
            unsafe
            {
                fixed (int* srcBmpH = &srcBmpBuffer[0])
                fixed (int* destBmpH = &destBmpBuffer[0])
                {
                    MemHolder srcMemHolder = new MemHolder((IntPtr)srcBmpH, bufferLen);
                    Surface srcSurface = new Surface(stride, w, h, srcMemHolder);

                    MemHolder destMemHolder = new MemHolder((IntPtr)destBmpH, bufferLen);
                    Surface destSurface = new Surface(stride, w, h, destMemHolder);

                    //effect
                    RotateZoomEffect eff = new RotateZoomEffect();
                    eff.SelectionBounds = new PixelFarm.Drawing.Rectangle(0, 0, w, h);
                    eff.Parameters = _rotateZoomConfigToken;
                    eff.Render(srcSurface, destSurface, new PixelFarm.Drawing.Rectangle[]{
                            new PixelFarm.Drawing.Rectangle(0,0,w,h)
                        }, 0, 1);

                }
            }

            //save to output
            Bitmap outputBmp = new Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bmpData2 = outputBmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Runtime.InteropServices.Marshal.Copy(destBmpBuffer, 0, bmpData2.Scan0, destBmpBuffer.Length);
            outputBmp.UnlockBits(bmpData2);

            this.pictureBox2.Image = outputBmp;
            this.pictureBox1.Image = bmp;


            //process the image
            //then copy to bitmap 
            //

        }
    }
}
