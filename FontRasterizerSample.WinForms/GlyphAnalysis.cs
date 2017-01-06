//MIT, 2016,  WinterDev
using System;
using System.Collections.Generic;
using NOpenType;
using PixelFarm.Agg.VertexSource;

namespace PixelFarm.Agg
{
    //this is PixelFarm version ***
    //render with MiniAgg


    public class GlyphContourBuilder
    {
        float curX;
        float curY;
        float latestMoveToX;
        float latestMoveToY;
        GlyphContour currentCnt;
        public GlyphContourBuilder()
        {

            Reset();
        }
        public void MoveTo(float x, float y)
        {
            this.latestMoveToX = this.curX = x;
            this.latestMoveToY = this.curY = y;
        }
        public void LineTo(float x, float y)
        {
            currentCnt.AddPart(new GlyphLine(curX, curY, x, y));
            this.curX = x;
            this.curY = y;

        }
        public void CloseFigure()
        {
            currentCnt.AddPart(new GlyphLine(curX, curY, latestMoveToX, latestMoveToY));
            this.curX = latestMoveToX;
            this.curY = latestMoveToY;
        }
        public void Reset()
        {
            currentCnt = new GlyphContour();
            this.latestMoveToX = this.curX = this.latestMoveToY = this.curY = 0;

        }
        public void Curve3(float p2x, float p2y, float x, float y)
        {
            currentCnt.AddPart(new GlyphCurve3(
                curX, curY,
                p2x, p2y,
                x, y));

            this.curX = x;
            this.curY = y;
        }
        public void Curve4(float p2x, float p2y, float p3x, float p3y, float x, float y)
        {
            currentCnt.AddPart(new GlyphCurve4(
                curX, curY,
                p2x, p2y,
                p3x, p3y,
                x, y));
            this.curX = x;
            this.curY = y;
        }
        public GlyphContour CurrentContour
        {
            get
            {
                return currentCnt;
            }

        }
    }
    public class GlyphContour
    {
        internal List<GlyphPart> parts = new List<GlyphPart>();
        public void AddPart(GlyphPart part)
        {
            parts.Add(part);
        }
    }

    public enum GlyphPartKind
    {
        Unknown,
        Line,
        Curve3,
        Curve4
    }

    public abstract class GlyphPart
    {
        public abstract GlyphPartKind Kind { get; }
    }
    public class GlyphLine : GlyphPart
    {
        internal float x0;
        internal float y0;
        internal float x1;
        internal float y1;
        public GlyphLine(float x0, float y0, float x1, float y1)
        {
            this.x0 = x0;
            this.y0 = y0;
            this.x1 = x1;
            this.y1 = y1;
        }
        public override GlyphPartKind Kind { get { return GlyphPartKind.Line; } }
#if DEBUG
        public override string ToString()
        {
            return "L(" + x0 + "," + y0 + "), (" + x1 + "," + y1 + ")";
        }
#endif
    }
    public class GlyphCurve3 : GlyphPart
    {
        internal float x0, y0, p2x, p2y, x, y;
        public GlyphCurve3(float x0, float y0, float p2x, float p2y, float x, float y)
        {
            this.x0 = x0;
            this.y0 = y0;
            this.p2x = p2x;
            this.p2y = p2y;
            this.x = x;
            this.y = y;
        }
        public override GlyphPartKind Kind { get { return GlyphPartKind.Curve3; } }
#if DEBUG
        public override string ToString()
        {
            return "C3(" + x0 + "," + y0 + "), (" + p2x + "," + p2y + "),(" + x + "," + y + ")";
        }
#endif
    }
    public class GlyphCurve4 : GlyphPart
    {
        internal float x0, y0, p2x, p2y, p3x, p3y, x, y;
        public GlyphCurve4(float x0, float y0, float p2x, float p2y,
            float p3x, float p3y,
            float x, float y)
        {
            this.x0 = x0;
            this.y0 = y0;
            this.p2x = p2x;
            this.p2y = p2y;
            this.p3x = p3x;
            this.p3y = p3y;
            this.x = x;
            this.y = y;
        }
        public override GlyphPartKind Kind { get { return GlyphPartKind.Curve4; } }
#if DEBUG
        public override string ToString()
        {
            return "C4(" + x0 + "," + y0 + "), (" + p2x + "," + p2y + "),(" + p3x + "," + p3y + "), (" + x + "," + y + ")";
        }
#endif

    }


}