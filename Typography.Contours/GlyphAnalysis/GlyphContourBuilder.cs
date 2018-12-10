//MIT, 2016-present, WinterDev
using System;
using System.Collections.Generic;

namespace Typography.Contours
{

    //this is PixelFarm version ***
    //render with MiniAgg 

    public class GlyphContourBuilder : OpenFont.IGlyphTranslator
    {

        List<GlyphContour> _contours;
        float _curX;
        float _curY;
        float _latestMoveToX;
        float _latestMoveToY;
        GlyphContour _currentCnt;
        GlyphPart _latestPart;
        //
        public GlyphContourBuilder()
        {

        }
        public List<GlyphContour> GetContours() => _contours;
        public void MoveTo(float x0, float y0)
        {
            _latestMoveToX = _curX = x0;
            _latestMoveToY = _curY = y0;
            _latestPart = null;
            //----------------------------

        }
        public void LineTo(float x1, float y1)
        {
            if (_latestPart != null)
            {
                _currentCnt.AddPart(_latestPart = new GlyphLine(_latestPart, x1, y1));
            }
            else
            {
                _currentCnt.AddPart(_latestPart = new GlyphLine(_curX, _curY, x1, y1));
            }
            _curX = x1;
            _curY = y1;
        }


        public void Curve3(float x1, float y1, float x2, float y2)
        {
            if (_latestPart != null)
            {
                _currentCnt.AddPart(_latestPart = new GlyphCurve3(
                 _latestPart,
                  x1, y1,
                  x2, y2));
            }
            else
            {
                _currentCnt.AddPart(new GlyphCurve3(
                    _curX, _curY,
                    x1, y1,
                    x2, y2));
            }

            _curX = x2;
            _curY = y2;
        }
        public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            if (_latestPart != null)
            {
                _currentCnt.AddPart(_latestPart = new GlyphCurve4(
                   _latestPart,
                    x1, y1,
                    x2, y2,
                    x3, y3));
            }
            else
            {
                _currentCnt.AddPart(_latestPart = new GlyphCurve4(
                   _curX, _curY,
                   x1, y1,
                   x2, y2,
                   x3, y3));
            }
            _curX = x3;
            _curY = y3;
        }

        public void CloseContour()
        {
            if (_curX == _latestMoveToX && _curY == _latestMoveToY)
            {
                //we not need to close 
            }
            else
            {
                if (_latestPart != null)
                {
                    _currentCnt.AddPart(_latestPart = new GlyphLine(_latestPart, _latestMoveToX, _latestMoveToY));
                }
                else
                {
                    _currentCnt.AddPart(_latestPart = new GlyphLine(_curX, _curY, _latestMoveToX, _latestMoveToY));
                }
            }

            _curX = _latestMoveToX;
            _curY = _latestMoveToY;

            if (_currentCnt != null &&
                _currentCnt.parts.Count > 0)
            {
                _contours.Add(_currentCnt);
                _currentCnt = null;
            }
            //
            _currentCnt = new GlyphContour();
        }
        public void BeginRead(int contourCount)
        {
            //reset all
            _contours = new List<GlyphContour>();
            _latestPart = null;
            _latestMoveToX = _curX = _latestMoveToY = _curY = 0;
            //
            _currentCnt = new GlyphContour();
            //new contour, but not add
        }
        public void EndRead()
        {

        }

    }



}