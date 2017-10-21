//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;

namespace Typography.Contours
{

    //this is PixelFarm version ***
    //render with MiniAgg 

    public class GlyphContourBuilder : OpenFont.IGlyphTranslator
    {

        List<GlyphContour> contours;
        float curX;
        float curY;
        float latestMoveToX;
        float latestMoveToY;
        GlyphContour currentCnt;
        GlyphPart _latestPart;
        public GlyphContourBuilder()
        {

        }
        public List<GlyphContour> GetContours()
        {
            return contours;
        }
        public void MoveTo(float x0, float y0)
        {
            this.latestMoveToX = this.curX = x0;
            this.latestMoveToY = this.curY = y0;
            _latestPart = null;
            //----------------------------

        }
        public void LineTo(float x1, float y1)
        {
            if (_latestPart != null)
            {
                currentCnt.AddPart(_latestPart = new GlyphLine(_latestPart, x1, y1));
            }
            else
            {
                currentCnt.AddPart(_latestPart = new GlyphLine(curX, curY, x1, y1));
            }
            this.curX = x1;
            this.curY = y1;
        }


        public void Curve3(float x1, float y1, float x2, float y2)
        {
            if (_latestPart != null)
            {
                currentCnt.AddPart(_latestPart = new GlyphCurve3(
                 _latestPart,
                  x1, y1,
                  x2, y2));
            }
            else
            {
                currentCnt.AddPart(new GlyphCurve3(
                    curX, curY,
                    x1, y1,
                    x2, y2));
            }

            this.curX = x2;
            this.curY = y2;
        }
        public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            if (_latestPart != null)
            {
                currentCnt.AddPart(_latestPart = new GlyphCurve4(
                   _latestPart,
                    x1, y1,
                    x2, y2,
                    x3, y3));
            }
            else
            {
                currentCnt.AddPart(_latestPart = new GlyphCurve4(
                   curX, curY,
                   x1, y1,
                   x2, y2,
                   x3, y3));
            }
            this.curX = x3;
            this.curY = y3;
        }

        public void CloseContour()
        {
            if (curX == latestMoveToX && curY == latestMoveToY)
            {
                //we not need to close 
            }
            else
            {
                if (_latestPart != null)
                {
                    currentCnt.AddPart(_latestPart = new GlyphLine(_latestPart, latestMoveToX, latestMoveToY));
                }
                else
                {
                    currentCnt.AddPart(_latestPart = new GlyphLine(curX, curY, latestMoveToX, latestMoveToY));
                }
            }

            this.curX = latestMoveToX;
            this.curY = latestMoveToY;

            if (currentCnt != null &&
                currentCnt.parts.Count > 0)
            {
                this.contours.Add(currentCnt); 
                currentCnt = null;
            }
            //
            currentCnt = new GlyphContour();
        }
        public void BeginRead(int contourCount)
        {
            //reset all
            contours = new List<GlyphContour>();
            _latestPart = null;
            this.latestMoveToX = this.curX = this.latestMoveToY = this.curY = 0;
            //
            currentCnt = new GlyphContour();
            //new contour, but not add
        }
        public void EndRead()
        {

        }

    }



}