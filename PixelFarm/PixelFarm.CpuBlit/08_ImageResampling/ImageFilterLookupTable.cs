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
// Adaptation for high precision colors has been sponsored by 
// Liberty Technology Systems, Inc., visit http://lib-sys.com
//
// Liberty Technology Systems, Inc. is the provider of
// PostScript and PDF technology for software developers.
// 
//----------------------------------------------------------------------------
#define USE_UNSAFE_CODE

namespace PixelFarm.CpuBlit.Imaging
{
    //-----------------------------------------------------ImageFilterLookUpTable
    public class ImageFilterLookUpTable
    {
        double _radius;
        int _diameter;
        int _start;
        int[] _weight_array;
        public static class ImgFilterConst
        {
            public const int SHIFT = 14;                     //----image_filter_shift
            public const int SCALE = 1 << SHIFT; //----image_filter_scale 
            public const int MASK = SCALE - 1;  //----image_filter_mask 
        }

        public static class ImgSubPixConst
        {
            public const int SHIFT = 8;                        //----image_subpixel_shift
            public const int SCALE = 1 << SHIFT; //----image_subpixel_scale 
            public const int MASK = SCALE - 1;   //----image_subpixel_mask 
        }

        void Calculate(Imaging.IImageFilter filter)
        {
            Calculate(filter, true);
        }

        void Calculate(Imaging.IImageFilter filter, bool normalization)
        {
            double r = filter.GetRadius();
            ReallocLut(r);
            int i;
            int pivot = Diameter << (ImgSubPixConst.SHIFT - 1);
            for (i = 0; i < pivot; i++)
            {
                double x = (double)i / (double)ImgSubPixConst.SCALE;
                double y = filter.CalculateWeight(x);
                _weight_array[pivot + i] =
                _weight_array[pivot - i] = AggMath.iround(y * ImgFilterConst.SCALE);
            }
            int end = (Diameter << ImgSubPixConst.SHIFT) - 1;
            _weight_array[0] = _weight_array[end];
            if (normalization)
            {
                Normalize();
            }
        }

        public ImageFilterLookUpTable()
        {
            _weight_array = new int[256];
            _radius = _diameter = _start = 0;
        }

        public ImageFilterLookUpTable(Imaging.IImageFilter filter)
            : this(filter, true)
        {
        }
        public ImageFilterLookUpTable(Imaging.IImageFilter filter, bool normalization)
        {
            _weight_array = new int[256];
            Calculate(filter, normalization);
        }

        //
        public double Radius => _radius;
        public int Diameter => _diameter;
        public int Start => _start;
        public int[] WeightArray => _weight_array;
        //

        //--------------------------------------------------------------------
        // This function normalizes integer values and corrects the rounding 
        // errors. It doesn't do anything with the source floating point values
        // (m_weight_array_dbl), it corrects only integers according to the rule 
        // of 1.0 which means that any sum of pixel weights must be equal to 1.0.
        // So, the filter function must produce a graph of the proper shape.
        //--------------------------------------------------------------------
        public void Normalize()
        {
            int i;
            int flip = 1;
            for (i = 0; i < (int)ImgSubPixConst.SCALE; i++)
            {
                for (; ; )
                {
                    int sum = 0;
                    int j;
                    for (j = 0; j < _diameter; j++)
                    {
                        sum += _weight_array[j * (int)ImgSubPixConst.SCALE + i];
                    }

                    if (sum == (int)ImgFilterConst.SCALE) break;
                    double k = (double)((int)ImgFilterConst.SCALE) / (double)(sum);
                    sum = 0;
                    for (j = 0; j < _diameter; j++)
                    {
                        sum += _weight_array[j * (int)ImgSubPixConst.SCALE + i] =
                            (int)AggMath.iround(_weight_array[j * (int)ImgSubPixConst.SCALE + i] * k);
                    }

                    sum -= (int)ImgFilterConst.SCALE;
                    int inc = (sum > 0) ? -1 : 1;
                    for (j = 0; j < _diameter && sum != 0; j++)
                    {
                        flip ^= 1;
                        int idx = flip != 0 ? _diameter / 2 + j / 2 : _diameter / 2 - j / 2;
                        int v = _weight_array[idx * (int)ImgSubPixConst.SCALE + i];
                        if (v < (int)ImgFilterConst.SCALE)
                        {
                            _weight_array[idx * (int)ImgSubPixConst.SCALE + i] += (int)inc;
                            sum += inc;
                        }
                    }
                }
            }

            int pivot = _diameter << (ImgSubPixConst.SHIFT - 1);
            for (i = 0; i < pivot; i++)
            {
                _weight_array[pivot + i] = _weight_array[pivot - i];
            }
            int end = (Diameter << ImgSubPixConst.SHIFT) - 1;
            _weight_array[0] = _weight_array[end];
        }

        void ReallocLut(double radius)
        {
            _radius = radius;
            _diameter = AggMath.uceil(radius) * 2;
            _start = -(_diameter / 2 - 1);
            int size = _diameter << ImgSubPixConst.SHIFT;
            if (size > _weight_array.Length)
            {
                _weight_array = new int[size];
            }
        }
    }
}