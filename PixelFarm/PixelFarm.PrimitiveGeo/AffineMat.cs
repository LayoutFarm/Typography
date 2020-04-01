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
// Affine transformation classes.
//
//----------------------------------------------------------------------------
//#ifndef AGG_TRANS_AFFINE_INCLUDED
//#define AGG_TRANS_AFFINE_INCLUDED

//#include <math.h>
//#include "agg_basics.h"

using System;


namespace PixelFarm.CpuBlit.VertexProcessing
{
     
    /// <summary>
    /// struct version of Affine (Matrix)
    /// </summary>
    public struct AffineMat
    {
        //3x2 matrix (rows x cols)
        public double
            sx, shy,
            shx, sy,
            tx, ty;

        public void SetValues(double v0_sx, double v1_shy,
                              double v2_shx, double v3_sy,
                              double v4_tx, double v5_ty)
        {
            sx = v0_sx; shy = v1_shy;
            shx = v2_shx; sy = v3_sy;
            tx = v4_tx; ty = v5_ty;
        }
        public float[] Get3x3MatrixElements() =>
            new float[]
            {
               (float) sx,(float) shy,(float)0,
               (float) shx, (float) sy, 0,
               (float) tx, (float) ty, 1
            };
        /// <summary>
        /// inside-values will be CHANGED after call this
        /// </summary>
        /// <param name="m"></param>
        public void Multiply(ref AffineMat m)
        {
            double t0 = sx * m.sx + shy * m.shx;
            double t2 = shx * m.sx + sy * m.shx;
            double t4 = tx * m.sx + ty * m.shx + m.tx;

            shy = sx * m.shy + shy * m.sy;
            sy = shx * m.shy + sy * m.sy;
            ty = tx * m.shy + ty * m.sy + m.ty;
            sx = t0;
            shx = t2;
            tx = t4;
        }

        /// <summary>
        /// inside-values will be CHANGED after call this
        /// </summary>
        /// <param name="m"></param>
        public void Rotate(double angleRad)
        {
            if (angleRad == 0) return;

            double ca = Math.Cos(angleRad);
            double sa = Math.Sin(angleRad);
            double t0 = sx * ca - shy * sa;
            double t2 = shx * ca - sy * sa;
            double t4 = tx * ca - ty * sa;
            shy = sx * sa + shy * ca;
            sy = shx * sa + sy * ca;
            ty = tx * sa + ty * ca;
            sx = t0;
            shx = t2;
            tx = t4;
        }
        public void Rotate(double angleRad, double centerX, double centerY)
        {
            Translate(-centerX, -centerY);//move to center
            Rotate(angleRad);
            Translate(centerX, centerY);//move back
        }
        public void RotateDeg(double degree)
        {
            Rotate(DegToRad(degree));
        }
        public void RotateDeg(double degree, double centerX, double centerY)
        {
            Translate(-centerX, -centerY);//move to center
            Rotate(DegToRad(degree));
            Translate(centerX, centerY);//move back
        }

        /// <summary>
        /// inside-values will be CHANGED after call this
        /// </summary>
        /// <param name="m"></param>
        public void Scale(double mm0, double mm3)
        {
            if (mm0 == 1 && mm3 == 1) return;

            sx *= mm0;
            shx *= mm0;
            tx *= mm0;
            shy *= mm3;
            sy *= mm3;
            ty *= mm3;
        }
        public void Scale(double mm)
        {
            if (mm == 1) return;

            sx *= mm;
            shx *= mm;
            tx *= mm;
            shy *= mm;
            sy *= mm;
            ty *= mm;
        }
        /// <summary>
        /// inside-values will be CHANGED after call this
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public void Translate(double dx, double dy)
        {
            tx += dx;
            ty += dy;
        }

        const double m_sx = 1;
        const double m_sy = 1;
        /// <summary>
        /// inside-values will be CHANGED after call this
        /// </summary>
        /// <param name="shx"></param>
        /// <param name="shy"></param>
        public void Skew(double dx, double dy)
        {

            double m_shx = Math.Tan(dx);
            double m_shy = Math.Tan(dy);

            double t0 = sx * m_sx + shy * m_shx;
            double t2 = shx * m_sx + sy * m_shx;
            double t4 = tx * m_sx + ty * m_shx + 0;//0=m.tx
            shy = sx * m_shy + shy * m_sy;
            sy = shx * m_shy + sy * m_sy;
            ty = tx * m_shy + ty * m_sy + 0;//0= m.ty;
            sx = t0;
            shx = t2;
            tx = t4;
        }
        /// <summary>
        /// inside-values will be CHANGED after call this
        /// </summary>
        public void Invert()
        {
            double d = CalculateDeterminantReciprocal();
            double t0 = sy * d;
            sy = sx * d;
            shy = -shy * d;
            shx = -shx * d;
            double t4 = -tx * t0 - ty * shx;
            ty = -tx * shy - ty * sy;
            sx = t0;
            tx = t4;
        }

        /// <summary>
        /// create new invert matrix
        /// </summary>
        /// <returns></returns>
        public AffineMat CreateInvert()
        {
            AffineMat clone = this; //*** COPY by value
            clone.Invert();
            return clone;
        }


        double CalculateDeterminantReciprocal()
        {
            return 1.0 / (sx * sy - shy * shx);
        }



        public static readonly AffineMat Iden = new AffineMat() {
            sx = 1,
            shy = 0,
            shx = 0,
            sy = 1,
            tx = 0,
            ty = 0
        };

        /// <summary>
        /// transform input x and y with this matrix
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Transform(ref double x, ref double y)
        {
            double tmp = x;
            x = tmp * sx + y * shx + tx;
            y = tmp * shy + y * sy + ty;
        }
        public void Transform(ref float x, ref float y)
        {
            double tmp = x;
            x = (float)(tmp * sx + y * shx + tx);
            y = (float)(tmp * shy + y * sy + ty);
        }

        public void ScaleRotateTranslate(
             double hotspotOffsetX, double hotSpotOffsetY,
             //
             double scaleX, double scaleY,
             double angleRad,
             double destX, double destY)
        {
            //steps are essential.
            if (hotspotOffsetX != 0.0f || hotSpotOffsetY != 0.0f)
            {
                this.Translate(-hotspotOffsetX, -hotSpotOffsetY);
            }

            if (scaleX != 1 || scaleY != 1)
            {
                this.Scale(scaleX, scaleY);
            }

            if (angleRad != 0)
            {
                this.Rotate(angleRad);
            }

            if (destX != 0 || destY != 0)
            {
                this.Translate(destX, destY);
            }
        }

      
        static double DegToRad(double degree)
        {
            return degree * (Math.PI / 180d);
        }
        static double RadToDeg(double degree)
        {
            return degree * (180d / Math.PI);
        }
    }
}
