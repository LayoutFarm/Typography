//BSD, 2014-2017, WinterDev
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
using PixelFarm.VectorMath;
namespace PixelFarm.Agg.Transform
{
    //============================================================trans_affine
    //
    // See Implementation agg_trans_affine.cpp
    //
    // Affine transformation are linear transformations in Cartesian coordinates
    // (strictly speaking not only in Cartesian, but for the beginning we will 
    // think so). They are rotation, scaling, translation and skewing.  
    // After any affine transformation a line segment remains a line segment 
    // and it will never become a curve. 
    //
    // There will be no math about matrix calculations, since it has been 
    // described many times. Ask yourself a very simple question:
    // "why do we need to understand and use some matrix stuff instead of just 
    // rotating, scaling and so on". The answers are:
    //
    // 1. Any combination of transformations can be done by only 4 multiplications
    //    and 4 additions in floating point.
    // 2. One matrix transformation is equivalent to the number of consecutive
    //    discrete transformations, i.e. the matrix "accumulates" all transformations 
    //    in the order of their settings. Suppose we have 4 transformations: 
    //       * rotate by 30 degrees,
    //       * scale X to 2.0, 
    //       * scale Y to 1.5, 
    //       * move to (100, 100). 
    //    The result will depend on the order of these transformations, 
    //    and the advantage of matrix is that the sequence of discret calls:
    //    rotate(30), scaleX(2.0), scaleY(1.5), move(100,100) 
    //    will have exactly the same result as the following matrix transformations:
    //   
    //    affine_matrix m;
    //    m *= rotate_matrix(30); 
    //    m *= scaleX_matrix(2.0);
    //    m *= scaleY_matrix(1.5);
    //    m *= move_matrix(100,100);
    //
    //    m.transform_my_point_at_last(x, y);
    //
    // What is the good of it? In real life we will set-up the matrix only once
    // and then transform many points, let alone the convenience to set any 
    // combination of transformations.
    //
    // So, how to use it? Very easy - literally as it's shown above. Not quite,
    // let us write a correct example:
    //
    // agg::trans_affine m;
    // m *= agg::trans_affine_rotation(30.0 * 3.1415926 / 180.0);
    // m *= agg::trans_affine_scaling(2.0, 1.5);
    // m *= agg::trans_affine_translation(100.0, 100.0);
    // m.transform(&x, &y);
    //
    // The affine matrix is all you need to perform any linear transformation,
    // but all transformations have origin point (0,0). It means that we need to 
    // use 2 translations if we want to rotate someting around (100,100):
    // 
    // m *= agg::trans_affine_translation(-100.0, -100.0);         // move to (0,0)
    // m *= agg::trans_affine_rotation(30.0 * 3.1415926 / 180.0);  // rotate
    // m *= agg::trans_affine_translation(100.0, 100.0);           // move back to (100,100)
    //----------------------------------------------------------------------

    public enum AffineMatrixCommand : byte
    {
        None,
        Scale,
        Skew,
        Rotate,
        Translate,
        Invert
    }

    public struct AffinePlan
    {
        public readonly AffineMatrixCommand cmd;
        public readonly double x;
        public readonly double y;
        public AffinePlan(AffineMatrixCommand cmd, double x, double y)
        {
            this.x = x;
            this.y = y;
            this.cmd = cmd;
        }
        public AffinePlan(AffineMatrixCommand cmd, double x)
        {
            this.x = x;
            this.y = 0;
            this.cmd = cmd;
        }


        //----------------------------------------------------------------------------
        public static AffinePlan Translate(double x, double y)
        {
            return new AffinePlan(AffineMatrixCommand.Translate, x, y);
        }
        public static AffinePlan Rotate(double radAngle)
        {
            return new AffinePlan(AffineMatrixCommand.Rotate, radAngle);
        }
        public static AffinePlan Skew(double x, double y)
        {
            return new AffinePlan(AffineMatrixCommand.Skew, x, y);
        }
        public static AffinePlan Scale(double x, double y)
        {
            return new AffinePlan(AffineMatrixCommand.Scale, x, y);
        }
        public static AffinePlan Scale(double both)
        {
            return new AffinePlan(AffineMatrixCommand.Scale, both, both);
        }
    }



    public sealed class Affine : ICoordTransformer
    {
        const double EPSILON = 1e-14;
        public readonly double sx, shy, shx, sy, tx, ty;
        bool isIdenHint;
        public static readonly Affine IdentityMatrix = Affine.NewIdentity();
        //------------------------------------------ Construction
        private Affine(Affine copyFrom)
        {
            sx = copyFrom.sx;
            shy = copyFrom.shy;
            shx = copyFrom.shx;
            sy = copyFrom.sy;
            tx = copyFrom.tx;
            ty = copyFrom.ty;
        }

        // Custom matrix. Usually used in derived classes
        private Affine(double v0_sx, double v1_shy,
                       double v2_shx, double v3_sy,
                       double v4_tx, double v5_ty)
        {
            sx = v0_sx;
            shy = v1_shy;
            shx = v2_shx;
            sy = v3_sy;
            tx = v4_tx;
            ty = v5_ty;
        }
        public double m11 { get { return sx; } }
        public double m12 { get { return shy; } }
        public double m21 { get { return shx; } }
        public double m22 { get { return sy; } }
        public double dx { get { return tx; } }
        public double dy { get { return ty; } }
        // Custom matrix from m[6]
        private Affine(double[] m)
        {
            sx = m[0];
            shy = m[1];
            shx = m[2];
            sy = m[3];
            tx = m[4];
            ty = m[5];
        }
        private Affine(Affine a, Affine b)
        {
            //copy from a
            //multiply with b
            sx = a.sx;
            shy = a.shy;
            shx = a.shx;
            sy = a.sy;
            tx = a.tx;
            ty = a.ty;
            MultiplyMatrix(ref sx, ref sy, ref shx, ref shy, ref tx, ref ty, b);
        }
        private Affine(Affine copyFrom, AffinePlan creationPlan)
        {
            //-----------------------
            sx = copyFrom.sx;
            shy = copyFrom.shy;
            shx = copyFrom.shx;
            sy = copyFrom.sy;
            tx = copyFrom.tx;
            ty = copyFrom.ty;
            //-----------------------
            switch (creationPlan.cmd)
            {
                default:
                    {
                        throw new NotSupportedException();
                    }
                case AffineMatrixCommand.None:
                    break;
                case AffineMatrixCommand.Rotate:
                    {
                        double angleRad = creationPlan.x;
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
                    break;
                case AffineMatrixCommand.Scale:
                    {
                        double mm0 = creationPlan.x;
                        double mm3 = creationPlan.y;
                        sx *= mm0;
                        shx *= mm0;
                        tx *= mm0;
                        shy *= mm3;
                        sy *= mm3;
                        ty *= mm3;
                    }
                    break;
                case AffineMatrixCommand.Skew:
                    {
                        double m_sx = 1;
                        double m_sy = 1;
                        double m_shx = Math.Tan(creationPlan.x);
                        double m_shy = Math.Tan(creationPlan.y);
                        double t0 = sx * m_sx + shy * m_shx;
                        double t2 = shx * m_sx + sy * m_shx;
                        double t4 = tx * m_sx + ty * m_shx + 0;//0=m.tx
                        shy = sx * m_shy + shy * m_sy;
                        sy = shx * m_shy + sy * m_sy;
                        ty = tx * m_shy + ty * m_sy + 0;//0= m.ty;
                        sx = t0;
                        shx = t2;
                        tx = t4;
                        //return new Affine(1.0, Math.Tan(y), Math.Tan(x), 1.0, 0.0, 0.0);

                    }
                    break;
                case AffineMatrixCommand.Translate:
                    {
                        tx += creationPlan.x;
                        ty += creationPlan.y;
                    }
                    break;
                case AffineMatrixCommand.Invert:
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
                    break;
            }
        }

        private Affine(AffinePlan[] creationPlans)
        {
            //-----------------------
            //start with identity matrix

            sx = 1;
            shy = 0;
            shx = 0;
            sy = 1;
            tx = 0;
            ty = 0;
            //-----------------------
            int j = creationPlans.Length;
            for (int i = 0; i < j; ++i)
            {
                AffinePlan plan = creationPlans[i];
                switch (plan.cmd)
                {
                    case AffineMatrixCommand.None:
                        break;
                    case AffineMatrixCommand.Rotate:
                        {
                            double angleRad = plan.x;
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
                        break;
                    case AffineMatrixCommand.Scale:
                        {
                            double mm0 = plan.x;
                            double mm3 = plan.y;
                            sx *= mm0;
                            shx *= mm0;
                            tx *= mm0;
                            shy *= mm3;
                            sy *= mm3;
                            ty *= mm3;
                        }
                        break;
                    case AffineMatrixCommand.Translate:
                        {
                            tx += plan.x;
                            ty += plan.y;
                        }
                        break;
                    case AffineMatrixCommand.Skew:
                        {
                            double m_sx = 1;
                            double m_sy = 1;
                            double m_shx = Math.Tan(plan.x);
                            double m_shy = Math.Tan(plan.y);
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
                        break;
                    case AffineMatrixCommand.Invert:
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
                        break;
                    default:
                        {
                            throw new NotSupportedException();
                        }
                }
            }
        }


        //----------------------------------------------------------
        public static Affine operator *(Affine a, Affine b)
        {
            //new input
            return new Affine(a, b);
        }
        //----------------------------------------------------------

        // Identity matrix
        static Affine NewIdentity()
        {
            var newIden = new Affine(
                1, 0,
                0, 1,
                0, 0);
            newIden.isIdenHint = true;
            return newIden;
        }
        public static Affine NewMatix(params AffinePlan[] creationPlans)
        {
            return new Affine(creationPlans);
        }
        public static Affine NewMatix(AffinePlan creationPlan)
        {
            return new Affine(IdentityMatrix, creationPlan);
        }
        //====================================================trans_affine_rotation
        // Rotation matrix. sin() and cos() are calculated twice for the same angle.
        // There's no harm because the performance of sin()/cos() is very good on all
        // modern processors. Besides, this operation is not going to be invoked too 
        // often.
        public static Affine NewRotation(double angRad)
        {
            double cos_rad, sin_rad;
            return new Affine(
               cos_rad = Math.Cos(angRad), sin_rad = Math.Sin(angRad),
                -sin_rad, cos_rad,
                0.0, 0.0);
        }

        //====================================================trans_affine_scaling
        // Scaling matrix. x, y - scale coefficients by X and Y respectively
        public static Affine NewScaling(double scale)
        {
            return new Affine(
                scale, 0.0,
                0.0, scale,
                0.0, 0.0);
        }

        public static Affine NewScaling(double x, double y)
        {
            return new Affine(
                x, 0.0,
                0.0, y,
                0.0, 0.0);
        }

        public static Affine NewTranslation(double x, double y)
        {
            return new Affine(
                1.0, 0.0,
                0.0, 1.0,
                x, y);
        }

        public static Affine NewTranslation(Vector2 offset)
        {
            return new Affine(
                1.0, 0.0,
                0.0, 1.0,
                offset.x, offset.y);
        }

        public static Affine NewSkewing(double x, double y)
        {
            return new Affine(
                1.0, Math.Tan(y),
                Math.Tan(x), 1.0,
                0.0, 0.0);
        }

        /*
        //===============================================trans_affine_line_segment
        // Rotate, Scale and Translate, associating 0...dist with line segment 
        // x1,y1,x2,y2
        public static Affine NewScaling(double x, double y)
        {
            return new Affine(x, 0.0, 0.0, y, 0.0, 0.0);
        }
        public sealed class trans_affine_line_segment : Affine
        {
            public trans_affine_line_segment(double x1, double y1, double x2, double y2,
                                      double dist)
            {
                double dx = x2 - x1;
                double dy = y2 - y1;
                if (dist > 0.0)
                {
                    //multiply(trans_affine_scaling(sqrt(dx * dx + dy * dy) / dist));
                }
                //multiply(trans_affine_rotation(Math.Atan2(dy, dx)));
                //multiply(trans_affine_translation(x1, y1));
            }
        };


        //============================================trans_affine_reflection_unit
        // Reflection matrix. Reflect coordinates across the line through 
        // the origin containing the unit vector (ux, uy).
        // Contributed by John Horigan
        public static Affine NewScaling(double x, double y)
        {
            return new Affine(x, 0.0, 0.0, y, 0.0, 0.0);
        }
        public class trans_affine_reflection_unit : Affine
        {
            public trans_affine_reflection_unit(double ux, double uy)
                :
              base(2.0 * ux * ux - 1.0,
                           2.0 * ux * uy,
                           2.0 * ux * uy,
                           2.0 * uy * uy - 1.0,
                           0.0, 0.0)
            { }
        };


        //=================================================trans_affine_reflection
        // Reflection matrix. Reflect coordinates across the line through 
        // the origin at the angle a or containing the non-unit vector (x, y).
        // Contributed by John Horigan
        public static Affine NewScaling(double x, double y)
        {
            return new Affine(x, 0.0, 0.0, y, 0.0, 0.0);
        }
        public sealed class trans_affine_reflection : trans_affine_reflection_unit
        {
            public trans_affine_reflection(double a)
                :
              base(Math.Cos(a), Math.Sin(a))
            { }


            public trans_affine_reflection(double x, double y)
                :
              base(x / Math.Sqrt(x * x + y * y), y / Math.Sqrt(x * x + y * y))
            { }
        };
         */

        /*
        // Rectangle to a parallelogram.
        trans_affine(double x1, double y1, double x2, double y2, double* parl)
        {
            rect_to_parl(x1, y1, x2, y2, parl);
        }

        // Parallelogram to a rectangle.
        trans_affine(double* parl, 
                     double x1, double y1, double x2, double y2)
        {
            parl_to_rect(parl, x1, y1, x2, y2);
        }

        // Arbitrary parallelogram transformation.
        trans_affine(double* src, double* dst)
        {
            parl_to_parl(src, dst);
        }

        //---------------------------------- Parellelogram transformations
        // transform a parallelogram to another one. Src and dst are 
        // pointers to arrays of three points (double[6], x1,y1,...) that 
        // identify three corners of the parallelograms assuming implicit 
        // fourth point. The arguments are arrays of double[6] mapped 
        // to x1,y1, x2,y2, x3,y3  where the coordinates are:
        //        *-----------------*
        //       /          (x3,y3)/
        //      /                 /
        //     /(x1,y1)   (x2,y2)/
        //    *-----------------*
        trans_affine parl_to_parl(double* src, double* dst)
        {
            sx  = src[2] - src[0];
            shy = src[3] - src[1];
            shx = src[4] - src[0];
            sy  = src[5] - src[1];
            tx  = src[0];
            ty  = src[1];
            invert();
            multiply(trans_affine(dst[2] - dst[0], dst[3] - dst[1], 
                dst[4] - dst[0], dst[5] - dst[1],
                dst[0], dst[1]));
            return *this;
        }

        trans_affine rect_to_parl(double x1, double y1, 
            double x2, double y2, 
            double* parl)
        {
            double src[6];
            src[0] = x1; src[1] = y1;
            src[2] = x2; src[3] = y1;
            src[4] = x2; src[5] = y2;
            parl_to_parl(src, parl);
            return *this;
        }

        trans_affine parl_to_rect(double* parl, 
            double x1, double y1, 
            double x2, double y2)
        {
            double dst[6];
            dst[0] = x1; dst[1] = y1;
            dst[2] = x2; dst[3] = y1;
            dst[4] = x2; dst[5] = y2;
            parl_to_parl(parl, dst);
            return *this;
        }

         */

        //------------------------------------------ Operations
        // Reset - load an identity matrix
        //public void identity()
        //{
        //    sx = sy = 1.0;
        //    shy = shx = tx = ty = 0.0;
        //}

        //Direct transformations operations
        //public void translate(double x, double y)
        //{
        //    tx += x;
        //    ty += y;
        //}

        //public void rotate(double AngleRadians)
        //{
        //    double ca = Math.Cos(AngleRadians);
        //    double sa = Math.Sin(AngleRadians);
        //    double t0 = sx * ca - shy * sa;
        //    double t2 = shx * ca - sy * sa;
        //    double t4 = tx * ca - ty * sa;
        //    shy = sx * sa + shy * ca;
        //    sy = shx * sa + sy * ca;
        //    ty = tx * sa + ty * ca;
        //    sx = t0;
        //    shx = t2;
        //    tx = t4;
        //}

        //public void scale(double x, double y)
        //{
        //    double mm0 = x; // Possible hint for the optimizer
        //    double mm3 = y;
        //    sx *= mm0;
        //    shx *= mm0;
        //    tx *= mm0;
        //    shy *= mm3;
        //    sy *= mm3;
        //    ty *= mm3;
        //}

        //public void scale(double scaleAmount)
        //{
        //    sx *= scaleAmount;
        //    shx *= scaleAmount;
        //    tx *= scaleAmount;
        //    shy *= scaleAmount;
        //    sy *= scaleAmount;
        //    ty *= scaleAmount;
        //}

        // Multiply matrix to another one
        static void MultiplyMatrix(
            ref double sx, ref double sy,
            ref double shx, ref double shy,
            ref double tx, ref double ty,
            Affine m)
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
        /*

        // Multiply "m" to "this" and assign the result to "this"
        trans_affine premultiply(trans_affine m)
        {
            trans_affine t = m;
            return *this = t.multiply(*this);
        }

        // Multiply matrix to inverse of another one
        trans_affine multiply_inv(trans_affine m)
        {
            trans_affine t = m;
            t.invert();
            return multiply(t);
        }

        // Multiply inverse of "m" to "this" and assign the result to "this"
        trans_affine premultiply_inv(trans_affine m)
        {
            trans_affine t = m;
            t.invert();
            return *this = t.multiply(*this);
        }
         */

        // Invert matrix. Do not try to invert degenerate matrices, 
        // there's no check for validity. If you set scale to 0 and 
        // then try to invert matrix, expect unpredictable result.
        //public void invert()
        //{
        //    double d = determinant_reciprocal();

        //    double t0 = sy * d;
        //    sy = sx * d;
        //    shy = -shy * d;
        //    shx = -shx * d;

        //    double t4 = -tx * t0 - ty * shx;
        //    ty = -tx * shy - ty * sy;

        //    sx = t0;
        //    tx = t4;
        //}

        // Invert matrix. Do not try to invert degenerate matrices, 
        // there's no check for validity. If you set scale to 0 and 
        // then try to invert matrix, expect unpredictable result.
        public Affine CreateInvert()
        {
            return new Affine(this, new AffinePlan(AffineMatrixCommand.Invert, 0));
        }
        public Affine CreateTranslation(double x, double y)
        {
            return new Affine(this, AffinePlan.Translate(x, y));
        }

        /*

        // Mirroring around X
        trans_affine flip_x()
        {
            sx  = -sx;
            shy = -shy;
            tx  = -tx;
            return *this;
        }

        // Mirroring around Y
        trans_affine flip_y()
        {
            shx = -shx;
            sy  = -sy;
            ty  = -ty;
            return *this;
        }

        //------------------------------------------- Load/Store
        // Store matrix to an array [6] of double
        void store_to(double* m)
        {
            *m++ = sx; *m++ = shy; *m++ = shx; *m++ = sy; *m++ = tx; *m++ = ty;
        }

        // Load matrix from an array [6] of double
        trans_affine load_from(double* m)
        {
            sx = *m++; shy = *m++; shx = *m++; sy = *m++; tx = *m++;  ty = *m++;
            return *this;
        }

        //------------------------------------------- Operators
        
         */
        // Multiply the matrix by another one and return
        // the result in a separete matrix.


        //public static Affine operator +(Affine a, Vector2 b)
        //{
        //    //new input
        //    Affine temp = new Affine(a);
        //    temp.tx += b.x;
        //    temp.ty += b.y;
        //    return temp;
        //    return new Affine(a, MatrixCommand.Translate, b);
        //}
        /*

        // Multiply the matrix by inverse of another one 
        // and return the result in a separete matrix.
        static trans_affine operator / (trans_affine a, trans_affine b)
        {
            return new trans_affine(a).multiply_inv(b);
        }

        // Calculate and return the inverse matrix
        static trans_affine operator ~ (trans_affine a)
        {
            new trans_affine(a).invert();
        }

        // Equal operator with default epsilon
        static bool operator == (trans_affine a, trans_affine b)
        {
            return a.is_equal(b, affine_epsilon);
        }

        // Not Equal operator with default epsilon
        static bool operator != (trans_affine a, trans_affine b)
        {
            return !a.is_equal(b, affine_epsilon);
        }

         */
        //-------------------------------------------- Transformations
        // Direct transformation of x and y
        public void Transform(ref double x, ref double y)
        {
            double tmp = x;
            x = tmp * sx + y * shx + tx;
            y = tmp * shy + y * sy + ty;
        }

        //public void transform(ref Vector2 pointToTransform)
        //{
        //    Transform(ref pointToTransform.x, ref pointToTransform.y);
        //}

        //public void transform(ref RectangleDouble rectToTransform)
        //{
        //    Transform(ref rectToTransform.Left, ref rectToTransform.Bottom);
        //    Transform(ref rectToTransform.Right, ref rectToTransform.Top);
        //}
        /*

        // Direct transformation of x and y, 2x2 matrix only, no translation
        void transform_2x2(double* x, double* y)
        {
            register double tmp = *x;
            *x = tmp * sx  + *y * shx;
            *y = tmp * shy + *y * sy;
        }

         */
        // Inverse transformation of x and y. It works slower than the 
        // direct transformation. For massive operations it's better to 
        // invert() the matrix and then use direct transformations. 
        public void InverseTransform(ref double x, ref double y)
        {
            double d = CalculateDeterminantReciprocal();
            double a = (x - tx) * d;
            double b = (y - ty) * d;
            x = a * sy - b * shx;
            y = b * sx - a * shy;
        }

        //public void inverse_transform(ref Vector2 pointToTransform)
        //{
        //    inverse_transform(ref pointToTransform.x, ref pointToTransform.y);
        //}
        /*

        //-------------------------------------------- Auxiliary
        // Calculate the determinant of matrix
        double determinant()
        {
            return sx * sy - shy * shx;
        }

         */
        // Calculate the reciprocal of the determinant
        double CalculateDeterminantReciprocal()
        {
            return 1.0 / (sx * sy - shy * shx);
        }

        // Get the average scale (by X and Y). 
        // Basically used to calculate the approximation_scale when
        // decomposinting curves into line segments.
        public double GetScale()
        {
            double x = 0.707106781 * sx + 0.707106781 * shx;
            double y = 0.707106781 * shy + 0.707106781 * sy;
            return Math.Sqrt(x * x + y * y);
        }

        // Check to see if the matrix is not degenerate
        public bool IsNotDegenerated(double epsilon)
        {
            return Math.Abs(sx) > epsilon && Math.Abs(sy) > epsilon;
        }

        // Check to see if it's an identity matrix
        public bool IsIdentity()
        {
            if (!isIdenHint)
            {
                return is_equal_eps(sx, 1.0) && is_equal_eps(shy, 0.0) &&
                   is_equal_eps(shx, 0.0) && is_equal_eps(sy, 1.0) &&
                   is_equal_eps(tx, 0.0) && is_equal_eps(ty, 0.0);
            }
            else
            {
                return true;
            }
        }

        static bool is_equal_eps(double v1, double v2)
        {
            return Math.Abs(v1 - v2) <= (EPSILON);
        }
        /// <summary>
        /// we do NOT store vxs, return original outputVxs
        /// </summary>
        /// <param name="src"></param>
        /// <param name="outputVxs"></param>
        public VertexStore TransformToVxs(VertexStore src, VertexStore outputVxs)
        {
            int count = src.Count;
            VertexCmd cmd;
            double x, y;
            for (int i = 0; i < count; ++i)
            {
                cmd = src.GetVertex(i, out x, out y);
                this.Transform(ref x, ref y);
                outputVxs.AddVertex(x, y, cmd);
            }

            //outputVxs.HasMoreThanOnePart = src.HasMoreThanOnePart;
            return outputVxs;
        }
        /// <summary>
        /// we do NOT store vxs, return original outputVxs
        /// </summary>
        /// <param name="src"></param>
        /// <param name="outputVxs"></param>
        /// <returns></returns>
        public VertexStore TransformToVxs(VertexStoreSnap src, VertexStore outputVxs)
        {
            var snapIter = src.GetVertexSnapIter();
            VertexCmd cmd;
            double x, y;
            while ((cmd = snapIter.GetNextVertex(out x, out y)) != VertexCmd.NoMore)
            {
                this.Transform(ref x, ref y);
                outputVxs.AddVertex(x, y, cmd);
            }
            return outputVxs;
        }
        /// <summary>
        ///we do NOT store vxs, return original outputVxs
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="outputVxs"></param>
        /// <returns></returns>
        public static VertexStore TranslateToVxs(VertexStore src, double dx, double dy, VertexStore outputVxs)
        {
            int count = src.Count;
            VertexCmd cmd;
            double x, y;
            for (int i = 0; i < count; ++i)
            {
                cmd = src.GetVertex(i, out x, out y);
                x += dx;
                y += dy;
                outputVxs.AddVertex(x, y, cmd);
            }
            return outputVxs;
        }





        //public static VertexStore TranslateTransformToVxs(VertexStoreSnap src, double dx, double dy, VertexStore vxs)
        //{
        //    var snapIter = src.GetVertexSnapIter();
        //    VertexCmd cmd;
        //    double x, y;
        //    while ((cmd = snapIter.GetNextVertex(out x, out y)) != VertexCmd.Stop)
        //    {
        //        x += dx;
        //        y += dy;
        //        vxs.AddVertex(x, y, cmd);
        //    }
        //    return vxs;
        //}




        // Check to see if two matrices are equal
        //public bool is_equal(Affine m, double epsilon)
        //{
        //    return agg_basics.is_equal_eps(sx, m.sx, epsilon) &&
        //        agg_basics.is_equal_eps(shy, m.shy, epsilon) &&
        //        agg_basics.is_equal_eps(shx, m.shx, epsilon) &&
        //        agg_basics.is_equal_eps(sy, m.sy, epsilon) &&
        //        agg_basics.is_equal_eps(tx, m.tx, epsilon) &&
        //        agg_basics.is_equal_eps(ty, m.ty, epsilon);
        //}

        // Determine the major parameters. Use with caution considering 
        // possible degenerate cases.
        //public double rotation()
        //{
        //    double x1 = 0.0;
        //    double y1 = 0.0;
        //    double x2 = 1.0;
        //    double y2 = 0.0;
        //    Transform(ref x1, ref y1);
        //    Transform(ref x2, ref y2);
        //    return Math.Atan2(y2 - y1, x2 - x1);
        //}

        //public void translation(out double dx, out double dy)
        //{
        //    dx = tx;
        //    dy = ty;
        //}

        //void scaling(out double x, out double y)
        //{
        //    double x1 = 0.0;
        //    double y1 = 0.0;
        //    double x2 = 1.0;
        //    double y2 = 1.0;

        //    Affine t = new Affine(this);
        //    t *= NewRotation(-rotation());
        //    t.Transform(ref x1, ref y1);
        //    t.Transform(ref x2, ref y2);
        //    x = x2 - x1;
        //    y = y2 - y1;
        //}
        //void scaling_abs(out double x, out double y)
        //{
        //    // Used to calculate scaling coefficients in image resampling. 
        //    // When there is considerable shear this method gives us much
        //    // better estimation than just sx, sy.
        //    x = Math.Sqrt(sx * sx + shx * shx);
        //    y = Math.Sqrt(shy * shy + sy * sy);
        //}
    }


}