//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic; 

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
        List<float> allPoints = new List<float>();

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

            allPoints.Add(x);
            allPoints.Add(y);

        }
        public void CloseFigure()
        {
            currentCnt.AddPart(new GlyphLine(curX, curY, latestMoveToX, latestMoveToY));

            allPoints.Add(latestMoveToX);
            allPoints.Add(latestMoveToY);

            this.curX = latestMoveToX;
            this.curY = latestMoveToY;
        }
        
        public void Reset()
        {
            currentCnt = new GlyphContour();
            this.latestMoveToX = this.curX = this.latestMoveToY = this.curY = 0;
            allPoints = new List<float>();
        }
        public void Curve3(float p2x, float p2y, float x, float y)
        {
            currentCnt.AddPart(new GlyphCurve3(
                curX, curY,
                p2x, p2y,
                x, y));

            allPoints.Add(curX);
            allPoints.Add(curY);
            allPoints.Add(p2x);
            allPoints.Add(p2y);
            allPoints.Add(x);
            allPoints.Add(y);

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


            allPoints.Add(curX);
            allPoints.Add(curY);
            allPoints.Add(p2x);
            allPoints.Add(p2y);
            allPoints.Add(p3x);
            allPoints.Add(p3y);
            allPoints.Add(x);
            allPoints.Add(y);


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
        public List<float> GetAllPoints()
        {
            return this.allPoints;
        }
    }

    public class GlyphContour
    {
        internal List<GlyphPart> parts = new List<GlyphPart>();
        internal List<float> allPoints; 
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

    static class GlyphDirectionAnalyzer
    {

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
            //find direction of this line

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