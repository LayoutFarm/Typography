/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public partial class RollControl : UserControl
    {

        static class Utility
        {
            public static RectangleF RectangleFromCenter(PointF center, float halfSize)
            {
                RectangleF ret = new RectangleF(center.X, center.Y, 0, 0);
                ret.Inflate(halfSize, halfSize);
                return ret;
            }
        }



        private bool tracking = false;
        private Point lastMouseXY;
        private Bitmap renderSurface = null; // used for double-buffering

        public event EventHandler ValueChanged;
        protected virtual void OnValueChanged()
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, EventArgs.Empty);
            }
        }

        public double angle;
        public double Angle
        {
            get
            {
                return angle;
            }

            set
            {
                double v = Math.IEEERemainder(value, 360);

                if (angle != v)
                {
                    angle = v;
                    OnValueChanged();
                    Invalidate();
                }
            }
        }


        //Direction at which to roll the image, in degrees
        protected double rollDirection;
        public double RollDirection
        {
            get
            {
                return rollDirection;
            }

            set
            {
                double v = Math.IEEERemainder(value, 360);

                if (rollDirection != v)
                {
                    rollDirection = v;
                    OnValueChanged();
                    Invalidate();
                }
            }
        }

        //Amount to roll the image, in degrees
        protected double rollAmount;
        public double RollAmount
        {
            get
            {
                return rollAmount;
            }

            set
            {
                double v = Math.IEEERemainder(value, 360);

                if (v >= 90)
                {
                    return;
                }
                if (rollAmount != v)
                {
                    rollAmount = v;
                    OnValueChanged();
                    Invalidate();
                }
            }
        }

        private void DrawToGraphics(Graphics g)
        {
            g.Clear(this.BackColor);

#if DEBUG
            string debug = "";

            try
            {
#endif
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Calculations
                Rectangle ourRect = Rectangle.Inflate(ClientRectangle, -2, -2);
                int diameter = Math.Min(ourRect.Width, ourRect.Height);
                Point center = new Point(ourRect.X + (diameter / 2), ourRect.Y + (diameter / 2));
                float radius1 = ((float)diameter / 3);
                float radius2 = ((float)diameter / 2);
                double theta = -((double)angle * 2 * Math.PI) / 360.0;
                float cos = (float)Math.Cos(theta);
                float sin = (float)Math.Sin(theta);
                float rx = (float)(rollAmount * Math.Cos(rollDirection * Math.PI / 180) / 90);
                float ry = (float)(rollAmount * Math.Sin(rollDirection * Math.PI / 180) / 90);

                float phi = (float)(rx / (ry * ry < 0.99 ? Math.Sqrt(1 - ry * ry) : 1));
                float rho = (float)(ry / (rx * rx < 0.99 ? Math.Sqrt(1 - rx * rx) : 1));

                // Globe
                g.TranslateTransform(center.X, center.Y);

                int divs = 4;
                double angleRadians = angle * Math.PI / 180;

                Pen darkPen;
                darkPen = onSphere ? Pens.Blue : Pens.Black;

                Pen lightPen;
                lightPen = Pens.Gray;

                for (int i = -divs; i < divs; i++)
                {
                    double u = -angleRadians + i * Math.PI / divs;
                    double v = -Math.PI / 2;
                    double ox = Math.Cos(u) * Math.Cos(v);
                    double oy = Math.Sin(u) * Math.Cos(v);
                    double oz = Math.Sin(v);
                    double x;
                    double y;
                    double z;

                    for (int j = -divs * 4; j <= 0; j++)
                    {
                        v = j * Math.PI / (divs * 8);
                        Pen p = (i % 2 == 0) ? lightPen : darkPen;
                        x = Math.Cos(u) * Math.Cos(v);
                        y = Math.Sin(u) * Math.Cos(v);
                        z = Math.Sin(v);
                        Draw3DLine(g, p, rx, -ry, radius1, ox, oy, oz, x, y, z);
                        ox = x;
                        oy = y;
                        oz = z;
                    }
                }

                for (int j = -divs / 2; j <= 0; j++)
                {
                    double v = j * Math.PI / divs;
                    double u = -angleRadians + -Math.PI;
                    double ox = Math.Cos(u) * Math.Cos(v);
                    double oy = Math.Sin(u) * Math.Cos(v);
                    double oz = Math.Sin(v);
                    double x;
                    double y;
                    double z;

                    for (int i = -divs * 6; i <= divs * 6; i++)
                    {
                        u = -angleRadians + i * Math.PI / (divs * 6);
                        double cosv = Math.Cos(v);
                        double sinv = Math.Sin(v);
                        Pen p = (j == 0) ? lightPen : darkPen;

                        x = Math.Cos(u) * Math.Cos(v);
                        y = Math.Sin(u) * Math.Cos(v);
                        z = Math.Sin(v);
                        Draw3DLine(g, p, rx, -ry, radius1, ox, oy, oz, x, y, z);
                        ox = x;
                        oy = y;
                        oz = z;
                    }
                }

                g.ResetTransform();

                // Ring

                // Reference Theta line
                g.DrawLine(SystemPens.ControlDark,
                    center.X + radius1, center.Y,
                    center.X + radius2, center.Y);

                // Draw Theta-chooser ring
                Pen outerRingDark = (Pen)SystemPens.ControlDarkDark.Clone();
                outerRingDark.Width = 2.0f;

                Pen outerRingLight = (Pen)SystemPens.ControlLightLight.Clone();
                outerRingLight.Width = 2.0f;

                g.DrawEllipse(outerRingDark, Utility.RectangleFromCenter(new Point(center.X - 1, center.Y - 1), radius1));
                g.DrawEllipse(outerRingDark, Utility.RectangleFromCenter(new Point(center.X - 1, center.Y - 1), radius2));
                g.DrawEllipse(outerRingLight, Utility.RectangleFromCenter(center, radius2));
                g.DrawEllipse(outerRingLight, Utility.RectangleFromCenter(center, radius1));

                outerRingDark.Dispose();
                outerRingLight.Dispose();

                // Draw actual theta line
                Pen thetaLinePen;

                if (mouseEntered && !onSphere)
                {
                    thetaLinePen = Pens.Blue;
                }
                else
                {
                    thetaLinePen = Pens.Black;
                }

                Pen useMePen = (Pen)thetaLinePen.Clone();
                useMePen.Width = 3.0f;

                g.DrawLine(useMePen,
                    center.X + radius1 * cos, center.Y + radius1 * sin,
                    center.X + radius2 * cos, center.Y + radius2 * sin);

                useMePen.Dispose();
#if DEBUG
            }

            catch
            {
                g.DrawString(debug, new Font("Courier New", 10), SystemBrushes.WindowText, 0, 0);
            }
#endif
        }

        void Draw3DLine(
            Graphics g,
            Pen p,
            double rx,
            double ry,
            double scale,
            double xs,
            double ys,
            double zs,
            double xe,
            double ye,
            double ze)
        {
            double dist = Math.Sqrt(rx * rx + ry * ry);

            if (dist != 0)
            {
                double rAngle = Math.Atan2(ry, rx);
                double sinAngle = Math.Sin(rAngle);
                double cosAngle = Math.Cos(rAngle);

                Transform(sinAngle, cosAngle, dist, Math.Cos(Math.Asin(dist)), ref xs, ref ys, ref zs);
                Transform(sinAngle, cosAngle, dist, Math.Cos(Math.Asin(dist)), ref xe, ref ye, ref ze);
            }

            xs *= scale;
            xe *= scale;
            ys *= scale;
            ye *= scale;

            if (ze < 0.03 && zs < 0.03)
            {
                g.DrawLine(p, (float)xs, (float)ys, (float)xe, (float)ye);
            }
        }

        void Transform(double sinangle, double cosangle, double sinamt, double cosamt, ref double x, ref double y, ref double z)
        {
            double ox = x;
            double oy = y;
            double oz = z;

            x = cosangle * ox - sinangle * oy;
            y = sinangle * ox + cosangle * oy;

            ox = x;
            oy = y;

            x = cosamt * ox - sinamt * oz;
            z = sinamt * ox + cosamt * oz;

            ox = x;

            x = cosangle * ox + sinangle * oy;
            y = -sinangle * ox + cosangle * oy;
        }

        private void CheckRenderSurface()
        {
            if (renderSurface != null && renderSurface.Size != Size)
            {
                renderSurface.Dispose();
                renderSurface = null;
            }

            if (renderSurface == null)
            {
                renderSurface = new Bitmap(Width, Height);

                using (Graphics g = Graphics.FromImage(renderSurface))
                {
                    DrawToGraphics(g);
                }
            }
        }

        private void DoPaint(Graphics g)
        {
            CheckRenderSurface();
            g.DrawImage(renderSurface, ClientRectangle, ClientRectangle, GraphicsUnit.Pixel);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            renderSurface = null;
            DoPaint(e.Graphics);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            DoPaint(pevent.Graphics);
        }

        private bool onSphere = false;
        private double startAngle;
        private double startTheta;
        private PointF startRoll;
        private Point startPt;
        private bool mouseEntered = false;

        protected override void OnMouseEnter(EventArgs e)
        {
            mouseEntered = true;
            onSphere = IsMouseOnSphere(Control.MousePosition.X, Control.MousePosition.Y);
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            mouseEntered = false;
            onSphere = IsMouseOnSphere(Control.MousePosition.X, Control.MousePosition.Y);
            Invalidate();
            base.OnMouseLeave(e);
        }

        private bool IsMouseOnSphere(int x, int y)
        {
            Rectangle ourRect = Rectangle.Inflate(ClientRectangle, -2, -2);
            int diameter = Math.Min(ourRect.Width, ourRect.Height);
            float radius1 = ((float)diameter / 3);
            Point center = new Point(ourRect.X + (diameter / 2), ourRect.Y + (diameter / 2));
            Point dist = new Point(x - center.X, y - center.Y);
            bool returnVal = (Math.Sqrt(dist.X * dist.X + dist.Y * dist.Y) <= radius1);
            return returnVal;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            startPt = new Point(e.X, e.Y);
            base.OnMouseDown(e);

            tracking = true;

            onSphere = IsMouseOnSphere(e.X, e.Y);
            startAngle = angle;
            startTheta = rollDirection;
            startRoll = new PointF(
                (float)(rollAmount * Math.Cos(rollDirection * Math.PI / 180)),
                (float)(rollAmount * Math.Sin(rollDirection * Math.PI / 180)));

            OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            tracking = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!tracking)
            {
                onSphere = IsMouseOnSphere(e.X, e.Y);
                Invalidate();
            }

            Point preLastMouseXY = new Point(e.X, e.Y);
            bool moved = (preLastMouseXY != lastMouseXY);

            lastMouseXY = preLastMouseXY;

            if (tracking && moved)
            {
                Rectangle ourRect = Rectangle.Inflate(ClientRectangle, -2, -2);
                int diameter = Math.Min(ourRect.Width, ourRect.Height);
                Point center = new Point(ourRect.X + (diameter / 2), ourRect.Y + (diameter / 2));

                if (onSphere)
                {
                    int dx = e.X - startPt.X;
                    int dy = e.Y - startPt.Y;
                    float mx = startRoll.X / 89.9f + 3.0f * dx / (diameter - 4);
                    float my = startRoll.Y / 89.9f + 3.0f * dy / (diameter - 4);
                    float dist = (float)Math.Sqrt(mx * mx + my * my);
                    float rad = (float)((dist > 1) ? 1 : dist);

                    if (dist == 0.0f)
                    {
                        mx = 0;
                        my = 0;
                    }
                    else
                    {
                        mx = mx * rad / dist;
                        my = my * rad / dist;
                    }

                    if (0 != (ModifierKeys & Keys.Shift))
                    {
                        if (mx * mx > my * my)
                        {
                            my = 0;
                        }
                        else
                        {
                            mx = 0;
                        }
                    }

                    this.rollDirection = 180 * Math.Atan2(my, mx) / Math.PI;
                    this.rollAmount = 89.94 * Math.Sqrt(mx * mx + my * my);
                    OnValueChanged();
                    Update();
                }
                else
                {
                    int dx = e.X - center.X;
                    int dy = e.Y - center.Y;
                    double theta = Math.Atan2(-dy, dx);

                    if (0 != (ModifierKeys & Keys.Shift))
                    {
                        this.Angle = Math.Round(4 * theta / Math.PI) * 45;
                    }
                    else
                    {
                        this.Angle = (theta * 360) / (2 * Math.PI);
                    }

                    Update();
                }
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            tracking = true;
            OnMouseMove(new MouseEventArgs(MouseButtons.Left, 1, lastMouseXY.X, lastMouseXY.Y, 0));
            tracking = false;
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);
            tracking = true;
            OnMouseMove(new MouseEventArgs(MouseButtons.Left, 1, lastMouseXY.X, lastMouseXY.Y, 0));
            tracking = false;
            rollAmount = 0;
            rollDirection = 0;
        }

        public RollControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.ResizeRedraw = true;
        }




    }
}
