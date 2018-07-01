//BSD, 2014-present, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
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
// Bilinear 2D transformations
//
//----------------------------------------------------------------------------

using System;
namespace PixelFarm.CpuBlit.VertexProcessing
{
    //==========================================================trans_bilinear
    public sealed partial class Bilinear : ICoordTransformer
    {
        //readonly double[,] m_mtx = new double[4, 2];//row x column
        //4 row, 2 columns

        double rc00, rc01,
               rc10, rc11,
               rc20, rc21,
               rc30, rc31;
        bool m_valid;
        private Bilinear()
        {
        }
        private Bilinear(double[,] result)
        {
            rc00 = result[0, 0];
            rc10 = result[1, 0];
            rc20 = result[2, 0];
            rc30 = result[3, 0];
            rc01 = result[0, 1];
            rc11 = result[1, 1];
            rc21 = result[2, 1];
            rc31 = result[3, 1];
            this.m_valid = true;
        }

        //--------------------------------------------------------------------
        // Set the transformations using two arbitrary quadrangles. 
        public static Bilinear RectToQuad(double srcX1, double srcY1, double srcX2, double srcY2, double[] quad)
        {
            double[] src = new double[8];
            //cartesian coord
            src[0] = srcX1; src[1] = srcY1;//(x1,y1)
            src[2] = srcX2; src[3] = srcY1;//(x2,y1)
            src[4] = srcX2; src[5] = srcY2;//(x2,y2)
            src[6] = srcX1; src[7] = srcY2;//(x2,y2) 
            double[,] result = new double[4, 2];
            if (GenerateMatrixQuadToQuad(src, quad, result))
            {
                return new Bilinear(result);
            }
            else
            {
                return new Bilinear();
            }
        }

        public static Bilinear QuadToRect(double[] srcQuad,
                        double destX1, double destY1,
                        double destX2, double destY2)
        {
            //--------------------------------------------------------------------
            // Set the reverse transformations, i.e., quadrangle -> rectangle 
            double[] dst = new double[8];
            //cartesian coord
            dst[0] = destX1; dst[1] = destY1;//(x1,y1)
            dst[2] = destX2; dst[3] = destY1;//(x2,y1)
            dst[4] = destX2; dst[5] = destY2;//(x2,y2)
            dst[6] = destX1; dst[7] = destY2;//(x1,y2)
            double[,] result = new double[4, 2];
            if (GenerateMatrixQuadToQuad(srcQuad, dst, result))
            {
                return new Bilinear(result);
            }
            else
            {
                return new Bilinear();
            }
        }

        public static Bilinear QuadToQuad(double[] srcQuad, double[] dst)
        {
            //--------------------------------------------------------------------
            // Set the reverse transformations, i.e., quadrangle -> rectangle  
            double[,] result = new double[4, 2];
            if (GenerateMatrixQuadToQuad(srcQuad, dst, result))
            {
                return new Bilinear(result);
            }
            else
            {
                return new Bilinear();
            }
        }

        static bool GenerateMatrixQuadToQuad(double[] src, double[] dst, double[,] result)
        {
            double[,] left = new double[4, 4];
            double[,] right = new double[4, 2];
            for (int i = 0; i < 4; i++)
            {
                int ix = i << 1;
                int iy = ix + 1;
                left[i, 0] = 1.0;
                left[i, 1] = src[ix] * src[iy];
                left[i, 2] = src[ix];
                left[i, 3] = src[iy];
                right[i, 0] = dst[ix];
                right[i, 1] = dst[iy];
            }
            //create result  
            return SimulEqGeneral.Solve(left, right, result);
        }

        //--------------------------------------------------------------------
        // Check if the equations were solved successfully
        public bool IsValid { get { return m_valid; } }

        //--------------------------------------------------------------------
        // Transform a point (x, y)
        public void Transform(ref double x, ref double y)
        {
            double tx = x;
            double ty = y;
            double xy = tx * ty;
            x = rc00 + rc10 * xy + rc20 * tx + rc30 * ty;
            y = rc01 + rc11 * xy + rc21 * tx + rc31 * ty;
        }
        //-------------------------------------------------------------------------


     
    }
}