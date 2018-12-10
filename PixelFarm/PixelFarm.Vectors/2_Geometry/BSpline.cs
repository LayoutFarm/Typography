//BSD, 2014-present, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------
//
// class bspline
//
//----------------------------------------------------------------------------

namespace PixelFarm.CpuBlit.VertexProcessing
{
    //----------------------------------------------------------------bspline
    // A very simple class of Bi-cubic Spline interpolation.
    // First call init(num, x[], y[]) where num - number of source points, 
    // x, y - arrays of X and Y values respectively. Here Y must be a function 
    // of X. It means that all the X-coordinates must be arranged in the ascending
    // order. 
    // Then call get(x) that calculates a value Y for the respective X. 
    // The class supports extrapolation, i.e. you can call get(x) where x is
    // outside the given with init() X-range. Extrapolation is a simple linear 
    // function.
    //------------------------------------------------------------------------
    public sealed class BSpline
    {
        int _max;
        int _num;
        int _xOffset;
        int _yOffset;
        double[] _am = new double[16];
        int _last_idx;
        //------------------------------------------------------------------------
        public BSpline()
        {
            _max = (0);
            _num = (0);
            _xOffset = (0);
            _yOffset = (0);
            _last_idx = -1;
        }

        //------------------------------------------------------------------------
        public BSpline(int num)
        {
            _max = (0);
            _num = (0);
            _xOffset = (0);
            _yOffset = (0);
            _last_idx = -1;
            Init(num);
        }

        //------------------------------------------------------------------------
        public BSpline(int num, double[] x, double[] y)
        {
            _max = (0);
            _num = (0);
            _xOffset = (0);
            _yOffset = (0);
            _last_idx = -1;
            Init(num, x, y);
        }


        //------------------------------------------------------------------------
        void Init(int max)
        {
            if (max > 2 && max > _max)
            {
                _am = new double[max * 3];
                _max = max;
                _xOffset = _max;
                _yOffset = _max * 2;
            }
            _num = 0;
            _last_idx = -1;
        }
        //------------------------------------------------------------------------
        public void AddPoint(double x, double y)
        {
            if (_num < _max)
            {
                _am[_xOffset + _num] = x;
                _am[_yOffset + _num] = y;
                ++_num;
            }
        }


        //------------------------------------------------------------------------
        public void Prepare()
        {
            if (_num > 2)
            {
                int i, k;
                int r;
                int s;
                double h, p, d, f, e;
                for (k = 0; k < _num; k++)
                {
                    _am[k] = 0.0;
                }

                int n1 = 3 * _num;
                double[] al = new double[n1];
                for (k = 0; k < n1; k++)
                {
                    al[k] = 0.0;
                }

                r = _num;
                s = _num * 2;
                n1 = _num - 1;
                d = _am[_xOffset + 1] - _am[_xOffset + 0];
                e = (_am[_yOffset + 1] - _am[_yOffset + 0]) / d;
                for (k = 1; k < n1; k++)
                {
                    h = d;
                    d = _am[_xOffset + k + 1] - _am[_xOffset + k];
                    f = e;
                    e = (_am[_yOffset + k + 1] - _am[_yOffset + k]) / d;
                    al[k] = d / (d + h);
                    al[r + k] = 1.0 - al[k];
                    al[s + k] = 6.0 * (e - f) / (h + d);
                }

                for (k = 1; k < n1; k++)
                {
                    p = 1.0 / (al[r + k] * al[k - 1] + 2.0);
                    al[k] *= -p;
                    al[s + k] = (al[s + k] - al[r + k] * al[s + k - 1]) * p;
                }

                _am[n1] = 0.0;
                al[n1 - 1] = al[s + n1 - 1];
                _am[n1 - 1] = al[n1 - 1];
                for (k = n1 - 2, i = 0; i < _num - 2; i++, k--)
                {
                    al[k] = al[k] * al[k + 1] + al[s + k];
                    _am[k] = al[k];
                }
            }
            _last_idx = -1;
        }



        //------------------------------------------------------------------------
        void Init(int num, double[] x, double[] y)
        {
            if (num > 2)
            {
                Init(num);
                int i;
                for (i = 0; i < num; i++)
                {
                    AddPoint(x[i], y[i]);
                }
                Prepare();
            }
            _last_idx = -1;
        }
        //------------------------------------------------------------------------
        void BSearch(int n, int xOffset, double x0, out int i)
        {
            int j = n - 1;
            int k;
            for (i = 0; (j - i) > 1;)
            {
                k = (i + j) >> 1;
                if (x0 < _am[xOffset + k]) j = k;
                else i = k;
            }
        }



        //------------------------------------------------------------------------
        double Interpolate(double x, int i)
        {
            int j = i + 1;
            double d = _am[_xOffset + i] - _am[_xOffset + j];
            double h = x - _am[_xOffset + j];
            double r = _am[_xOffset + i] - x;
            double p = d * d / 6.0;
            return (_am[j] * r * r * r + _am[i] * h * h * h) / 6.0 / d +
                   ((_am[_yOffset + j] - _am[j] * p) * r + (_am[_yOffset + i] - _am[i] * p) * h) / d;
        }


        //------------------------------------------------------------------------
        double ExtrapolateLeft(double x)
        {
            double d = _am[_xOffset + 1] - _am[_xOffset + 0];
            return (-d * _am[1] / 6 + (_am[_yOffset + 1] - _am[_yOffset + 0]) / d) *
                   (x - _am[_xOffset + 0]) +
                   _am[_yOffset + 0];
        }

        //------------------------------------------------------------------------
        double ExtrapolateRight(double x)
        {
            double d = _am[_xOffset + _num - 1] - _am[_xOffset + _num - 2];
            return (d * _am[_num - 2] / 6 + (_am[_yOffset + _num - 1] - _am[_yOffset + _num - 2]) / d) *
                   (x - _am[_xOffset + _num - 1]) +
                   _am[_yOffset + _num - 1];
        }

        //------------------------------------------------------------------------
        public double Get(double x)
        {
            if (_num > 2)
            {
                int i;
                // Extrapolation on the left
                if (x < _am[_xOffset + 0]) return ExtrapolateLeft(x);
                // Extrapolation on the right
                if (x >= _am[_xOffset + _num - 1]) return ExtrapolateRight(x);
                // Interpolation
                BSearch(_num, _xOffset, x, out i);
                return Interpolate(x, i);
            }
            return 0.0;
        }


        //------------------------------------------------------------------------
        public double GetStateful(double x)
        {
            if (_num > 2)
            {
                // Extrapolation on the left
                if (x < _am[_xOffset + 0]) return ExtrapolateLeft(x);
                // Extrapolation on the right
                if (x >= _am[_xOffset + _num - 1]) return ExtrapolateRight(x);
                if (_last_idx >= 0)
                {
                    // Check if x is not in current range
                    if (x < _am[_xOffset + _last_idx] || x > _am[_xOffset + _last_idx + 1])
                    {
                        // Check if x between next points (most probably)
                        if (_last_idx < _num - 2 &&
                           x >= _am[_xOffset + _last_idx + 1] &&
                           x <= _am[_xOffset + _last_idx + 2])
                        {
                            ++_last_idx;
                        }
                        else
                            if (_last_idx > 0 &&
                               x >= _am[_xOffset + _last_idx - 1] &&
                               x <= _am[_xOffset + _last_idx])
                        {
                            // x is between pevious points
                            --_last_idx;
                        }
                        else
                        {
                            // Else perform full search
                            BSearch(_num, _xOffset, x, out _last_idx);
                        }
                    }
                    return Interpolate(x, _last_idx);
                }
                else
                {
                    // Interpolation
                    BSearch(_num, _xOffset, x, out _last_idx);
                    return Interpolate(x, _last_idx);
                }
            }
            return 0.0;
        }
    }
}
