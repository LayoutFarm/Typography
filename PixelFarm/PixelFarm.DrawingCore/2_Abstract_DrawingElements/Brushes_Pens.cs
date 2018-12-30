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
        GeometryGradient,
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
        public Color Color { get; set; }
        public override BrushKind BrushKind => BrushKind.Solid;

        public override object InnerBrush { get; set; }
        public override void Dispose()
        {
        }
    }

    public sealed class TextureBrush : Brush
    {

        Image _textureImage;
        public TextureBrush(Image textureImage)
        {
            _textureImage = textureImage;
        }
        public override BrushKind BrushKind => BrushKind.Texture;

        public Image TextureImage => _textureImage;
        public override object InnerBrush { get; set; }
        public override void Dispose()
        {
        }
    }


    public abstract class GeometryGraidentBrush : Brush
    {

    }


    public class LinearGradientPair
    {
        public readonly Color c1;
        public readonly float x1;
        public readonly float y1;


        public readonly Color c2;
        public readonly float x2;
        public readonly float y2;
        public readonly double Distance;
        public readonly double Angle;

        public readonly GradientDirection Direction;
        readonly bool NeedSwap;

        public int steps;

        public LinearGradientPair(PointF stop1, Color c1, PointF stop2, Color c2)
        {
            this.c1 = c1;
            this.c2 = c2;
            this.x1 = stop1.X;
            this.y1 = stop1.Y;
            this.x2 = stop2.X;
            this.y2 = stop2.Y;

            float dx = stop2.X - stop1.X;
            float dy = stop2.Y - stop1.Y;
            NeedSwap = dx < 0;

            if (dx == 0)
            {
                //vertical
                Direction = GradientDirection.Vertical;
                Distance = Math.Abs(dy);
            }
            else if (dy == 0)
            {
                //horizontal
                Direction = GradientDirection.Horizontal;
                Distance = Math.Abs(dx);
            }
            else
            {
                Direction = GradientDirection.Angle;
                Distance = Math.Sqrt(dx * dx + dy * dy);
            }
            Angle = (double)Math.Atan2(dy, dx);
            steps = 256;
        }

        public void GetProperSwapVertices(
            out float x1, out float y1, out Color c1,
            out float x2, out float y2, out Color c2)
        {
            if (NeedSwap)
            {
                x1 = this.x2;
                y1 = this.y2;
                c1 = this.c2;
                //
                x2 = this.x1;
                y2 = this.y1;
                c2 = this.c1;
            }
            else
            {
                x1 = this.x1;
                y1 = this.y1;
                c1 = this.c1;
                //
                x2 = this.x2;
                y2 = this.y2;
                c2 = this.c2;
            }

        }
        public enum GradientDirection : byte
        {
            Vertical,
            Horizontal,
            Angle
        }
    }


    public sealed class CircularGradientBrush : GeometryGraidentBrush
    {
        object _innerBrush;
        LinearGradientPair _firstGradientPair;
        List<LinearGradientPair> _colorPairs;

        PointF _latesStop;
        Color _latestColor;

        public CircularGradientBrush(PointF stop1, Color c1, PointF stop2, Color c2)
        {
            _firstGradientPair = new LinearGradientPair(stop1, c1, stop2, c2);
            _latesStop = stop2;
            _latestColor = c2;
        }
        public void AddMoreColorStop(PointF stop2, Color c2)
        {
            if (_colorPairs == null)
            {
                _colorPairs = new List<LinearGradientPair>();
                _colorPairs.Add(_firstGradientPair);
            }
            var newpair = new LinearGradientPair(_latesStop, _latestColor, stop2, c2);
            _colorPairs.Add(newpair);
            _latesStop = stop2;
            _latestColor = c2;
        }
        //first stop color, todo review here
        public Color Color => _firstGradientPair.c1;

        public int PairCount => (_colorPairs == null) ? 1 : _colorPairs.Count;

        public LinearGradientPair GetFirstPair() => _firstGradientPair;

        public IEnumerable<LinearGradientPair> GetColorPairIter()
        {
            if (_colorPairs == null)
            {
                yield return _firstGradientPair;
            }
            else
            {
                int j = _colorPairs.Count;
                for (int i = 0; i < j; ++i)
                {
                    yield return _colorPairs[i];
                }
            }
        }

        public override object InnerBrush
        {
            get => _innerBrush;

            set => _innerBrush = value;
        }
        public override BrushKind BrushKind => BrushKind.CircularGraident;

        public override void Dispose()
        {
        }

    }

    public sealed class LinearGradientBrush : GeometryGraidentBrush
    {

        LinearGradientPair _firstGradientPair;
        List<LinearGradientPair> _colorPairs;

        PointF _latesStop;
        Color _latestColor;

        public LinearGradientBrush(PointF stop1, Color c1, PointF stop2, Color c2)
        {
            _firstGradientPair = new LinearGradientPair(stop1, c1, stop2, c2);
            _latesStop = stop2;
            _latestColor = c2;
        }
        public void AddMoreColorStop(PointF stop2, Color c2)
        {
            if (_colorPairs == null)
            {
                _colorPairs = new List<LinearGradientPair>();
                _colorPairs.Add(_firstGradientPair);
            }
            var newpair = new LinearGradientPair(_latesStop, _latestColor, stop2, c2);
            _colorPairs.Add(newpair);
            _latesStop = stop2;
            _latestColor = c2;
        }
        //first stop color
        public Color Color => _firstGradientPair.c1;

        public int PairCount => (_colorPairs == null) ? 1 : _colorPairs.Count;

        public LinearGradientPair GetFirstPair() => _firstGradientPair;

        public IEnumerable<LinearGradientPair> GetColorPairIter()
        {
            if (_colorPairs == null)
            {
                yield return _firstGradientPair;
            }
            else
            {
                int j = _colorPairs.Count;
                for (int i = 0; i < j; ++i)
                {
                    yield return _colorPairs[i];
                }
            }
        }

        public override object InnerBrush { get; set; }
        public override BrushKind BrushKind => BrushKind.LinearGradient;

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