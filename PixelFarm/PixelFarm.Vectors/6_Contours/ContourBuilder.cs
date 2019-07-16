//MIT, 2016-present, WinterDev
using System;
using System.Collections.Generic;

namespace PixelFarm.Contours
{

    //this is PixelFarm version ***
    //render with MiniAgg 

    public interface IContourBuilder
    {
        void MoveTo(float x0, float y0);
        void LineTo(float x1, float y1);
        void Curve3(float x1, float y1, float x2, float y2);
        void Curve4(float x1, float y1, float x2, float y2, float x3, float y3);
        void CloseContour();
        void BeginRead(int contourCount);
        void EndRead();
    }

    public class ContourBuilder : IContourBuilder
    {

        List<Contour> _contours;
        float _curX;
        float _curY;
        float _latestMoveToX;
        float _latestMoveToY;
        Contour _currentCnt;
        ContourPart _latestPart;
        //
        public ContourBuilder()
        {

        }
        public List<Contour> GetContours() => _contours;
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
                _currentCnt.AddPart(_latestPart = new Line(_latestPart, x1, y1));
            }
            else
            {
                _currentCnt.AddPart(_latestPart = new Line(_curX, _curY, x1, y1));
            }
            _curX = x1;
            _curY = y1;
        }


        public void Curve3(float x1, float y1, float x2, float y2)
        {
            if (_latestPart != null)
            {
                _currentCnt.AddPart(_latestPart = new Curve3(
                 _latestPart,
                  x1, y1,
                  x2, y2));
            }
            else
            {
                _currentCnt.AddPart(new Curve3(
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
                _currentCnt.AddPart(_latestPart = new Curve4(
                   _latestPart,
                    x1, y1,
                    x2, y2,
                    x3, y3));
            }
            else
            {
                _currentCnt.AddPart(_latestPart = new Curve4(
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
                    _currentCnt.AddPart(_latestPart = new Line(_latestPart, _latestMoveToX, _latestMoveToY));
                }
                else
                {
                    _currentCnt.AddPart(_latestPart = new Line(_curX, _curY, _latestMoveToX, _latestMoveToY));
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
            _currentCnt = new Contour();
        }
        public void BeginRead(int contourCount)
        {
            //reset all
            _contours = new List<Contour>();
            _latestPart = null;
            _latestMoveToX = _curX = _latestMoveToY = _curY = 0;
            //
            _currentCnt = new Contour();
            //new contour, but not add
        }
        public void EndRead()
        {

        }

    }



}