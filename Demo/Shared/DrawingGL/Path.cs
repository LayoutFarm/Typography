//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;
using Typography.OpenFont;
namespace DrawingGL
{
    enum PathPointKind : byte
    {
        Point,
        CurveControl,
        CloseFigure,
    }
    struct PathPoint
    {
        public readonly float x;
        public readonly float y;
        public readonly PathPointKind kind;
        public PathPoint(float x, float y, PathPointKind k)
        {
            this.x = x;
            this.y = y;
            this.kind = k;
        }

#if DEBUG
        public override string ToString()
        {
            return "(" + x + "," + y + ")" + ((kind == PathPointKind.Point) ? " p " : "c");
        }
#endif
    }
    interface IWritablePath
    {
        void CloseFigure();
        /// <summary>
        /// add curve4 from latest point (x0,y0)
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void BezireTo(float x1, float y1, float x2, float y2, float x3, float y3);
        void LineTo(float x1, float y1);
        void MoveTo(float x0, float y0);
    }

    class WritablePath : IWritablePath
    {
        //record all cmd 
        internal List<PathPoint> _points = new List<PathPoint>();

        float _latestX;
        float _latestY;
        float _lastMoveX;
        float _lastMoveY;
        bool _addMoveTo;

        public WritablePath()
        {

        }
        public void MoveTo(float x0, float y0)
        {
            _latestX = _lastMoveX = x0;
            _latestY = _lastMoveY = y0;
            _addMoveTo = true;

            //_points.Add(new PathPoint(_latestX = x0, _latestY = y0, PathPointKind.Point));
        }
        public void BezireTo(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            if (_addMoveTo)
            {
                _points.Add(new PathPoint(_latestX, _latestY, PathPointKind.Point));
                _addMoveTo = false;
            }
            _points.Add(new PathPoint(x1, y1, PathPointKind.CurveControl));
            _points.Add(new PathPoint(x2, y2, PathPointKind.CurveControl));
            _points.Add(new PathPoint(_latestX = x3, _latestY = y3, PathPointKind.Point));
        }
        public void CloseFigure()
        {
            if (_lastMoveX != _latestX ||
                _lastMoveY != _latestY)
            {
                _points.Add(new PathPoint(_lastMoveX, _lastMoveY, PathPointKind.Point));
            }
            _lastMoveX = _latestX;
            _lastMoveY = _latestY;

            //add curve
            _points.Add(new PathPoint(_lastMoveX, _lastMoveY, PathPointKind.CloseFigure));

        }
        public void LineTo(float x1, float y1)
        {
            if (_addMoveTo)
            {
                _points.Add(new PathPoint(_latestX, _latestY, PathPointKind.Point));
                _addMoveTo = false;
            }
            _points.Add(new PathPoint(_latestX = x1, _latestY = y1, PathPointKind.Point));
        }
        //-------------------- 
    }

    public struct GlyphRun
    {
        //glyph run contains...
        //1.
        Typography.TextLayout.GlyphPlan glyphPlan; //10 bytes        
        public float[] tessData; //4
        public ushort nTessElements;//2
        internal GlyphRun(Typography.TextLayout.GlyphPlan glyphPlan, float[] tessData, ushort nTessElements)
        {
            this.glyphPlan = glyphPlan;
            this.tessData = tessData;
            this.nTessElements = nTessElements;

        }
        public float OffsetX
        {
            get { return glyphPlan.ExactX; }
        }
        public float OffsetY
        {
            get { return glyphPlan.ExactY; }
        }

    }
    public class TextRun
    {
        //each text run has TextFormat information

        internal List<GlyphRun> _glyphs = new List<GlyphRun>();
        internal Typeface typeface;
        internal float sizeInPoints;

        public TextRun()
        {

        }
        public void AddGlyph(GlyphRun glyph)
        {
            _glyphs.Add(glyph);
        }
        public float CalculateToPixelScaleFromPointSize(float sizeInPoint)
        {
            return typeface.CalculateScaleToPixelFromPointSize(sizeInPoint);
        }

    }
}