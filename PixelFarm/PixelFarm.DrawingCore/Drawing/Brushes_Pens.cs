//MIT, 2014-present, WinterDev

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
        PolygonGradient,
        Texture
    }

    public sealed class SolidBrush : Brush
    {
        public SolidBrush()
        {
            //default
            this.Color = Color.Transparent;
        }
        public SolidBrush(Color color)
        {
            this.Color = color;
        }
        public Color Color { get; }
        public override BrushKind BrushKind => BrushKind.Solid;
        public override object InnerBrush { get; set; }
        public override void Dispose()
        {
        }
    }

    public sealed class TextureBrush : Brush
    {
        public TextureBrush(Image textureImage)
        {
            TextureImage = textureImage;
        }
        public override BrushKind BrushKind => BrushKind.Texture;

        public Image TextureImage { get; }
        public override object InnerBrush { get; set; }
        public override void Dispose()
        {
        }
    }


    public abstract class GeometryGradientBrush : Brush
    {
        object _innerBrush;
        public PixelFarm.CpuBlit.VertexProcessing.ICoordTransformer CoordTransformer { get; set; }
        public override object InnerBrush
        {
            get => _innerBrush;
            set => _innerBrush = value;
        }
        public override void Dispose()
        {
        }
    }


    public enum GradientOffsetUnit : byte
    {
        Pixel,
        Ratio,//0-1
    }
    public struct ColorStop
    {
        public readonly float Offset; //relative offset from center of circular gradient
        public readonly GradientOffsetUnit OffsetUnit;
        public readonly Color Color; //color at stop point

        public ColorStop(float offset, GradientOffsetUnit unit, Color color)
        {
#if DEBUG
            if (offset < 0 || offset > 1)
            {
                System.Diagnostics.Debugger.Break();
            }
#endif

            if (offset < 0)
            {
                offset = 0;
            }
            else if (offset > 1)
            {
                offset = 1;
            }

            Offset = offset;
            OffsetUnit = unit;
            Color = color;
        }
        public ColorStop(float offset, Color color)
        {
#if DEBUG
            if (offset < 0 || offset > 1)
            {
                System.Diagnostics.Debugger.Break();
            }
#endif

            if (offset < 0)
            {
                offset = 0;
            }
            else if (offset > 1)
            {
                offset = 1;
            }

            Offset = offset;
            OffsetUnit = GradientOffsetUnit.Ratio;
            Color = color;
        }
        public static readonly ColorStop Empty = new ColorStop();
    }

    /// <summary>
    ///  It determines how a shape is filled beyond the defined edges of the gradient.
    /// </summary>
    public enum SpreadMethod : byte
    {
        //pad
        //The final color of the gradient fills the shape beyond the gradient's edges.
        Pad,

        //reflect
        //The gradient repeats in reverse beyond its edges.
        Reflect,

        //repeat
        //The gradient repeats in the original order beyond its edges.
        Repeat
    }

    public sealed class RadialGradientBrush : GeometryGradientBrush
    {
        public RadialGradientBrush(PointF start, PointF end, Color c1, Color c2)
            : this(start, end, new ColorStop[]
            {
                new ColorStop(0, GradientOffsetUnit.Ratio,c1),
                new ColorStop(1, GradientOffsetUnit.Ratio,c2),
            })
        {
        }
        public RadialGradientBrush(PointF start, float r, Color c1, Color c2)
            : this(start, new PointF(start.X + r, start.Y), new ColorStop[]
            {
                new ColorStop(0, GradientOffsetUnit.Ratio,c1),
                new ColorStop(1, GradientOffsetUnit.Ratio,c2),
            })
        {
        }
        public RadialGradientBrush(PointF start, float r, ColorStop[] stops)
            : this(start, new PointF(start.X + r, start.Y), stops)
        {
        }
        public RadialGradientBrush(PointF start, PointF end, ColorStop[] stops)
        {
            StartPoint = start;
            EndPoint = end;

            //must have at least 2 stops
            if (stops.Length < 2)
            {
                return;
            }
            //---------------

            IsValid = true;
            ColorStops = stops;
        }
        public override BrushKind BrushKind => BrushKind.CircularGraident;
        public SpreadMethod SpreadMethod { get; set; }
        public PointF StartPoint { get; }
        public PointF EndPoint { get; }
        public ColorStop[] ColorStops { get; }
        public bool IsValid { get; }

        public double Length => System.Math.Sqrt(
                                    (EndPoint.Y - StartPoint.Y) * (EndPoint.Y - StartPoint.Y) +
                                    (EndPoint.X - StartPoint.X) * (EndPoint.X - StartPoint.X)
                                    );
    }

    public sealed class LinearGradientBrush : GeometryGradientBrush
    {
        public LinearGradientBrush(PointF start, PointF end, Color c1, Color c2)
        {
            StartPoint = start;
            EndPoint = end;
            IsValid = true;
            ColorStops = new ColorStop[]
            {
                new ColorStop(0, GradientOffsetUnit.Ratio,c1),
                new ColorStop(1, GradientOffsetUnit.Ratio,c2),
            };
        }
        public LinearGradientBrush(PointF start, PointF end, ColorStop[] stops)
        {
            StartPoint = start;
            EndPoint = end;

            //must have at least 2 stops
            if (stops.Length < 2)
            {
                return;
            }
            //---------------

            IsValid = true;
            ColorStops = stops;
        }
        public override BrushKind BrushKind => BrushKind.LinearGradient;
        public SpreadMethod SpreadMethod { get; set; }
        public PointF StartPoint { get; }
        public PointF EndPoint { get; }
        public ColorStop[] ColorStops { get; }
        public bool IsValid { get; }
        //
        public double Length => System.Math.Sqrt(
                                    (EndPoint.Y - StartPoint.Y) * (EndPoint.Y - StartPoint.Y) +
                                    (EndPoint.X - StartPoint.X) * (EndPoint.X - StartPoint.X)
                                    );
        public double Angle => System.Math.Atan2(EndPoint.Y - StartPoint.Y, EndPoint.X - StartPoint.X);
    }


    public sealed class PolygonGradientBrush : GeometryGradientBrush
    {
        /// <summary>
        /// vertex contains (x,y and color)
        /// </summary>
        public struct ColorVertex2d
        {
            public readonly float X;
            public readonly float Y;
            public readonly Color C;
            public ColorVertex2d(float x, float y, Color c)
            {
                X = x;
                Y = y;
                C = c;
            }
        }

        public PolygonGradientBrush(ColorVertex2d[] initVertices)
        {
            //start at least 3 vertices
            if (initVertices.Length < 2) throw new NotSupportedException();
            Vertices.AddRange(initVertices);
        }

        public List<ColorVertex2d> Vertices { get; } = new List<ColorVertex2d>();
        public override BrushKind BrushKind => BrushKind.PolygonGradient;
        public override object InnerBrush { get; set; }
        public override void Dispose()
        {
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

        readonly Brush _brush;
        Color _strokeColor;
        public Pen(Color color)
        {
            Width = 1;
            _strokeColor = color;
            _brush = new SolidBrush(color);
        }
        public Pen(Color color, float width)
        {
            Width = width;
            _strokeColor = color;
            _brush = new SolidBrush(color);
        }
        public Pen(Brush brush)
        {
            Width = 1;//default
            _brush = brush;
            if (brush is SolidBrush solidBrush)
            {
                _strokeColor = solidBrush.Color;
            }
            else
            {
                _strokeColor = Color.Black;
            }

            //TODO: review here
        }
        public override Brush Brush => _brush;
        public Color StrokeColor => _strokeColor;
        public override float[] DashPattern { get; set; }
        public override object InnerPen { get; set; }
        public override float Width { get; set; }
        public override DashStyle DashStyle { get; set; }
        public Color Color => _strokeColor;

        public override void Dispose()
        {
        }
    }
}