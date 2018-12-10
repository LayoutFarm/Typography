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

    public struct BilinearMat
    {
        public double rc00, rc01,
             rc10, rc11,
             rc20, rc21,
             rc30, rc31;
    }



    public sealed class Bilinear : ICoordTransformer
    {
        struct Quad4
        {
            public readonly double m0, m1,
                          m2, m3,
                          m4, m5,
                          m6, m7;
            public Quad4(double m0, double m1,
                         double m2, double m3,
                         double m4, double m5,
                         double m6, double m7)
            {
                this.m0 = m0;
                this.m1 = m1;
                this.m2 = m2;
                this.m3 = m3;
                this.m4 = m4;
                this.m5 = m5;
                this.m6 = m6;
                this.m7 = m7;
            }
            public void FillIntoArray(double[] outputArr)
            {
#if DEBUG
                if (outputArr.Length != 8) throw new NotSupportedException();
#endif

                outputArr[0] = m0; outputArr[1] = m1;
                outputArr[2] = m2; outputArr[3] = m3;
                outputArr[4] = m4; outputArr[5] = m5;
                outputArr[6] = m6; outputArr[7] = m7;
            }
        }

        Quad4 _srcQuad; //backup of srcQuad
        Quad4 _dstQuad; //backup of dstQuad


        //readonly double[,] m_mtx = new double[4, 2];//row x column
        //4 row, 2 columns

        double rc00, rc01,
               rc10, rc11,
               rc20, rc21,
               rc30, rc31;
        //----------


        //----------
        //
        bool m_valid;
        private Bilinear()
        {
        }
        private Bilinear(double[,] result, Quad4 srcQuad, Quad4 dstQuad)
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

            _srcQuad = srcQuad;
            _dstQuad = dstQuad;

        }

        public Bilinear(double rc00, double rc01,
             double rc10, double rc11,
              double rc20, double rc21,
              double rc30, double rc31)
        {
            this.rc00 = rc00; this.rc01 = rc01;
            this.rc10 = rc10; this.rc11 = rc11;
            this.rc20 = rc20; this.rc21 = rc21;
            this.rc30 = rc30; this.rc31 = rc31;

        }
        //--------------------------------------------------------------------
        // Set the transformations using two arbitrary quadrangles. 
        public static Bilinear RectToQuad(
            double srcX1, double srcY1, double srcX2, double srcY2,
            double[] quad)
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
                //we want to backup srcQuad and destQuad

                return new Bilinear(result,
                    new Quad4(
                        srcX1, srcY1,
                        srcX2, srcY1,
                        srcX2, srcY2,
                        srcX1, srcY2
                    ),
                    new Quad4(
                        quad[0], quad[1],
                        quad[2], quad[3],
                        quad[4], quad[5],
                        quad[6], quad[7]
                    ));
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

                return new Bilinear(result,
                    new Quad4(
                        srcQuad[0], srcQuad[1],
                        srcQuad[2], srcQuad[3],
                        srcQuad[4], srcQuad[5],
                        srcQuad[6], srcQuad[7]
                    ),
                    new Quad4(
                        destX1, destY1,
                        destX2, destY1,
                        destX2, destY2,
                        destX1, destY2
                    ));
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
                return new Bilinear(result,
                    new Quad4(
                       srcQuad[0], srcQuad[1],
                       srcQuad[2], srcQuad[3],
                       srcQuad[4], srcQuad[5],
                       srcQuad[6], srcQuad[7]
                       ),
                    new Quad4(
                        dst[0], dst[1],
                        dst[2], dst[3],
                        dst[4], dst[5],
                        dst[6], dst[7]
                        ));
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
                int ix = i << 1; //*2
                int iy = ix + 1; //+1
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

        public bool IsValid => m_valid;
        public CoordTransformerKind Kind => CoordTransformerKind.Bilinear;

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
        ICoordTransformer ICoordTransformer.MultiplyWith(ICoordTransformer another)
        {
            return new CoordTransformationChain(this, another);
        }
        ICoordTransformer ICoordTransformer.CreateInvert()
        {

            double[] srcQuad = new double[8];
            double[] dstQuad = new double[8];

            _srcQuad.FillIntoArray(srcQuad);
            _dstQuad.FillIntoArray(dstQuad);

            //invert dst=> src *** (invert)
            return QuadToQuad(dstQuad, srcQuad);
        }
        //-------------------------------------------------------------------------

        public BilinearMat GetInternalElements()
        {
            var mat = new BilinearMat();
            mat.rc00 = rc00; mat.rc01 = rc01;
            mat.rc10 = rc10; mat.rc01 = rc11;
            mat.rc20 = rc20; mat.rc21 = rc21;
            mat.rc30 = rc30; mat.rc31 = rc31;
            return mat;
        }
    }
}