//MIT, 2014-2017, WinterDev

using System;
using System.Collections.Generic;
namespace PixelFarm.Drawing
{
    public abstract class Brush : System.IDisposable
    {
        public abstract BrushKind BrushKind { get; }
        public abstract void Dispose();
        public abstract object InnerBrush { get; set; }
    }

    public enum BrushKind
    {
        Solid,
        LinearGradient,
        CircularGraident,
        GeometryGradient,
        Texture
    }

    public sealed class SolidBrush : Brush
    {
        object innerBrush;
        public SolidBrush()
        {
            //default
            this.Color = Color.Transparent;
        }
        public SolidBrush(Color color)
        {
            this.Color = color;
        }
        public Color Color { get; set; }
        public override BrushKind BrushKind
        {
            get { return BrushKind.Solid; }
        }
        public override object InnerBrush
        {
            get
            {
                return this.innerBrush;
            }
            set
            {
                this.innerBrush = value;
            }
        }
        public override void Dispose()
        {
        }
    }

    public sealed class TextureBrush : Brush
    {
        object innerBrush;
        Image textureImage;
        public TextureBrush(Image textureImage)
        {
            this.textureImage = textureImage;
        }
        public override BrushKind BrushKind
        {
            get { return BrushKind.Texture; }
        }
        public Image TextureImage
        {
            get { return this.textureImage; }
        }


        public override object InnerBrush
        {
            get
            {
                return this.innerBrush;
            }
            set
            {
                this.innerBrush = value;
            }
        }
        public override void Dispose()
        {
        }
    }


    public abstract class GeometryGraidentBrush : Brush
    {
    }
    public sealed class LinearGradientBrush : GeometryGraidentBrush
    {
        object innerBrush;
        List<Color> stopColors = new List<Color>(2);
        List<PointF> stopPoints = new List<PointF>(2);
        public LinearGradientBrush(PointF stop1, Color c1, PointF stop2, Color c2)
        {
            this.stopColors.Add(c1);
            this.stopColors.Add(c2);
            this.stopPoints.Add(stop1);
            this.stopPoints.Add(stop2);
        }
        public Color Color
        {
            //first stop color
            get { return this.stopColors[0]; }
        }
        public override object InnerBrush
        {
            get
            {
                return this.innerBrush;
            }
            set
            {
                this.innerBrush = value;
            }
        }
        public override BrushKind BrushKind
        {
            get { return BrushKind.LinearGradient; }
        }
        public override void Dispose()
        {
        }
        public List<Color> GetColors()
        {
            return this.stopColors;
        }
        public List<PointF> GetStopPoints()
        {
            return this.stopPoints;
        }

        public static LinearGradientBrush CreateLinearGradientBrush(RectangleF rect,
            Color startColor, Color stopColor, float degreeAngle)
        {
            //find radius
            int w = Math.Abs((int)(rect.Right - rect.Left));
            int h = Math.Abs((int)(rect.Bottom - rect.Top));
            int max = Math.Max(w, h);
            float radius = (float)Math.Pow(2 * (max * max), 0.5f);
            //find point1 and point2
            //not implement! 
            bool fromNegativeAngle = false;
            if (degreeAngle < 0)
            {
                fromNegativeAngle = true;
                degreeAngle = -degreeAngle;
            }

            PointF startPoint = new PointF(rect.Left, rect.Top);
            PointF stopPoint = new PointF(rect.Right, rect.Top);
            if (degreeAngle > 360)
            {
            }
            //-------------------------
            if (degreeAngle == 0)
            {
                startPoint = new PointF(rect.Left, rect.Bottom);
                stopPoint = new PointF(rect.Right, rect.Bottom);
            }
            else if (degreeAngle < 90)
            {
                startPoint = new PointF(rect.Left, rect.Bottom);
                var angleRad = DegreesToRadians(degreeAngle);
                stopPoint = new PointF(
                   rect.Left + (float)(Math.Cos(angleRad) * radius),
                   rect.Bottom - (float)(Math.Sin(angleRad) * radius));
            }
            else if (degreeAngle == 90)
            {
                startPoint = new PointF(rect.Left, rect.Bottom);
                stopPoint = new PointF(rect.Left, rect.Top);
            }
            else if (degreeAngle < 180)
            {
                startPoint = new PointF(rect.Right, rect.Bottom);
                var angleRad = DegreesToRadians(degreeAngle);
                var pos = (float)(Math.Cos(angleRad) * radius);
                stopPoint = new PointF(
                   rect.Right + (float)(Math.Cos(angleRad) * radius),
                   rect.Bottom - (float)(Math.Sin(angleRad) * radius));
            }
            else if (degreeAngle == 180)
            {
                startPoint = new PointF(rect.Right, rect.Bottom);
                stopPoint = new PointF(rect.Left, rect.Bottom);
            }
            else if (degreeAngle < 270)
            {
                startPoint = new PointF(rect.Right, rect.Top);
                var angleRad = DegreesToRadians(degreeAngle);
                stopPoint = new PointF(
                   rect.Right - (float)(Math.Cos(angleRad) * radius),
                   rect.Top + (float)(Math.Sin(angleRad) * radius));
            }
            else if (degreeAngle == 270)
            {
                startPoint = new PointF(rect.Left, rect.Top);
                stopPoint = new PointF(rect.Left, rect.Bottom);
            }
            else if (degreeAngle < 360)
            {
                startPoint = new PointF(rect.Left, rect.Top);
                var angleRad = DegreesToRadians(degreeAngle);
                stopPoint = new PointF(
                   rect.Left + (float)(Math.Cos(angleRad) * radius),
                   rect.Top + (float)(Math.Sin(angleRad) * radius));
            }
            else if (degreeAngle == 360)
            {
                startPoint = new PointF(rect.Left, rect.Bottom);
                stopPoint = new PointF(rect.Right, rect.Bottom);
            }

            return new LinearGradientBrush(startPoint, startColor, stopPoint, stopColor);
        }
        const float DEG_TO_RAD = (float)System.Math.PI / 180.0f;
        /// <summary>
        /// Convert degrees to radians
        /// </summary>
        /// <param name="degrees">An angle in degrees</param>
        /// <returns>The angle expressed in radians</returns>
        public static float DegreesToRadians(float degrees)
        {
            return degrees * DEG_TO_RAD;
        }

        /// <summary>
        /// Convert radians to degrees
        /// </summary>
        /// <param name="radians">An angle in radians</param>
        /// <returns>The angle expressed in degrees</returns>
        public static float RadiansToDegrees(float radians)
        {
            const float radToDeg = 180.0f / (float)System.Math.PI;
            return radians * radToDeg;
        }
    }


    public abstract class PenBase : System.IDisposable
    {
        public abstract void Dispose();
        public abstract float[] DashPattern { get; set; }
        public abstract float Width { get; set; }
        public abstract DashStyle DashStyle { get; set; }
        public abstract object InnerPen { get; set; }
        public abstract Brush Brush { get; }
    }
    public sealed class Pen : PenBase
    {
        float[] dashPattern;
        object innerPen;
        DashStyle dashStyle;
        float width = 1;//default 
        Brush brush;
        Color strokeColor;
        public Pen(Color color)
        {
            this.strokeColor = color;
            this.brush = new SolidBrush(color);
        }
        public override Brush Brush
        {
            get { return this.brush; }
        }
        public Pen(Brush brush)
        {
            this.brush = brush;
        }
        public Color StrokeColor
        {
            get { return this.strokeColor; }
        }

        public override float[] DashPattern
        {
            get
            {
                return dashPattern;
            }
            set
            {
                dashPattern = value;
            }
        }
        public override object InnerPen
        {
            get
            {
                return this.innerPen;
            }
            set
            {
                this.innerPen = value;
            }
        }
        public override float Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;
            }
        }
        public override DashStyle DashStyle
        {
            get
            {
                return this.dashStyle;
            }
            set
            {
                this.dashStyle = value;
            }
        }
        public override void Dispose()
        {
        }
    }
}