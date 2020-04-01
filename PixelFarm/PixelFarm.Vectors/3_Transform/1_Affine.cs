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


    public struct Quad2f
    {
        public float left_top_x;
        public float left_top_y;

        public float right_top_x;
        public float right_top_y;

        public float right_bottom_x;
        public float right_bottom_y;


        public float left_bottom_x;
        public float left_bottom_y;

        public Quad2f(float width, float height)
        {
            left_top_x = 0;              /**/left_top_y = 0;
            right_top_x = 0 + width;     /**/right_top_y = 0;
            right_bottom_x = right_top_x;   /**/right_bottom_y = 0 + height;
            left_bottom_x = 0;           /**/left_bottom_y = right_bottom_y;
        }
        public Quad2f(float left, float top, float width, float height)
        {
            left_top_x = left;              /**/left_top_y = top;
            right_top_x = left + width;     /**/right_top_y = top;
            right_bottom_x = right_top_x;   /**/right_bottom_y = top + height;
            left_bottom_x = left;           /**/left_bottom_y = right_bottom_y;
        }
        public Quad2f(float left, float top, float width, float height, float hostW, float hostH)
        {
            left_top_x = left / hostW;            /**/left_top_y = top / hostH;
            right_top_x = (left + width) / hostW; /**/right_top_y = left_top_y;
            right_bottom_x = right_top_x;         /**/right_bottom_y = (top + height) / hostH;
            left_bottom_x = left_top_x;           /**/left_bottom_y = right_bottom_y;
        }
        /// <summary>
        /// set corners from simple rect
        /// </summary>
        /// <param name="left"></param>
        /// <param name=""></param>
        public void SetCornersFromRect(float left, float top, float width, float height)
        {
            left_top_x = left;              /**/left_top_y = top;
            right_top_x = left + width;     /**/right_top_y = top;
            right_bottom_x = right_top_x;   /**/right_bottom_y = top + height;
            left_bottom_x = left;           /**/left_bottom_y = right_bottom_y;
        }
        public void SetCornersFromRect(in PixelFarm.Drawing.RectangleF rect)
        {
            SetCornersFromRect(rect.Left, rect.Top, rect.Width, rect.Height);
        }
        public void SetCornersFromRect(in PixelFarm.Drawing.Rectangle rect)
        {
            SetCornersFromRect(rect.Left, rect.Top, rect.Width, rect.Height);
        }
        /// <summary>
        /// set corner from rect and normalize value with w and h
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="hostW"></param>
        /// <param name="hostH"></param>
        public void SetCornersFromRect(float left, float top, float width, float height, float hostW, float hostH)
        {
            left_top_x = left / hostW;                /**/left_top_y = top / hostH;
            right_top_x = (left + width) / hostW;     /**/right_top_y = left_top_y;
            right_bottom_x = right_top_x;         /**/right_bottom_y = (top + height) / hostH;
            left_bottom_x = left_top_x;           /**/left_bottom_y = right_bottom_y;
        }

        public void OffsetX(float dx)
        {
            left_top_x += dx;
            right_top_x += dx;
            right_bottom_x += dx;
            left_bottom_x += dx;
        }
        public void OffsetY(float dy)
        {
            left_top_y += dy;
            right_top_y += dy;
            right_bottom_y += dy;
            left_bottom_y += dy;
        }
        public void Offset(float dx, float dy)
        {
            left_top_x += dx;
            right_top_x += dx;
            right_bottom_x += dx;
            left_bottom_x += dx;

            left_top_y += dy;
            right_top_y += dy;
            right_bottom_y += dy;
            left_bottom_y += dy;
        }

        /// <summary>
        /// set corners from simple rect
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetCornersFromRect(float width, float height)
        {

            left_top_x = 0;              /**/left_top_y = 0;
            right_top_x = 0 + width;     /**/right_top_y = 0;
            right_bottom_x = right_top_x;   /**/right_bottom_y = 0 + height;
            left_bottom_x = 0;           /**/left_bottom_y = right_bottom_y;
        }
        public void Transform(in AffineMat mat)
        {
            mat.Transform(ref left_top_x, ref left_top_y);
            mat.Transform(ref right_top_x, ref right_top_y);
            mat.Transform(ref right_bottom_x, ref right_bottom_y);
            mat.Transform(ref left_bottom_x, ref left_bottom_y);
        }
        public void Transform(in Affine mat)
        {
            mat.Transform(ref left_top_x, ref left_top_y);
            mat.Transform(ref right_top_x, ref right_top_y);
            mat.Transform(ref right_bottom_x, ref right_bottom_y);
            mat.Transform(ref left_bottom_x, ref left_bottom_y);
        }
    }



    public class ReusableAffineMatrix : Affine
    {
        public void SetElems(in AffineMat aff)
        {
            SetElements(aff);
        }
    }


    public class Affine : ICoordTransformer
    {
        const double EPSILON = 1e-14;
        AffineMat _elems;
        bool _isIdenHint;

        public static readonly Affine IdentityMatrix = Affine.NewIdentity();
        //------------------------------------------ Construction
        private Affine(Affine copyFrom)
        {
            //sx = copyFrom.sx;
            //shy = copyFrom.shy;
            //shx = copyFrom.shx;
            //sy = copyFrom.sy;
            //tx = copyFrom.tx;
            //ty = copyFrom.ty;

            _elems = copyFrom._elems;
            _isIdenHint = copyFrom._isIdenHint;
        }
        public Affine(AffineMat copyFrom)
        {
            //sx = copyFrom.sx;
            //shy = copyFrom.shy;
            //shx = copyFrom.shx;
            //sy = copyFrom.sy;
            //tx = copyFrom.tx;
            //ty = copyFrom.ty;

            _elems = copyFrom;
            _isIdenHint = false;
        }
        // Custom matrix. Usually used in derived classes
        public Affine(double v0_sx, double v1_shy,
                      double v2_shx, double v3_sy,
                      double v4_tx, double v5_ty)
        {
            _elems.SetValues(
                v0_sx, v1_shy,
                v2_shx, v3_sy,
                v4_tx, v5_ty);

            _isIdenHint = false;
        }
        public Affine() { }



        public float[] Get3x3MatrixElements() => _elems.Get3x3MatrixElements();
        ICoordTransformer ICoordTransformer.CreateInvert() => CreateInvert();
        public ICoordTransformer MultiplyWith(ICoordTransformer another)
        {
            if (another is Affine)
            {
                return this * (Affine)another;
            }
            else if (another is Perspective)
            {
                Perspective p = new Perspective(this);
                return p * (Perspective)another;
            }
            else
            {
                return new CoordTransformationChain(this, another);

            }
        }
        public double m11 => _elems.sx;
        public double m12 => _elems.shy;
        public double m21 => _elems.shx;
        public double m22 => _elems.sy;
        public double dx => _elems.tx;
        public double dy => _elems.ty;

        //-----------------------------------------
        public double sx => _elems.sx;
        public double shy => _elems.shy;
        public double shx => _elems.shx;
        public double sy => _elems.sy;
        public double tx => _elems.tx;
        public double ty => _elems.ty;
        //-----------------------------------------
        public CoordTransformerKind Kind => CoordTransformerKind.Affine3x2;
        public AffineMat GetInternalMat() => _elems;

        /// <summary>
        /// set elements by copy values from input elems
        /// </summary>
        /// <param name="elems"></param>
        internal void SetElements(in AffineMat elems)
        {
            _elems = elems;
            _isIdenHint = false;
        }

        public Affine Clone() => new Affine(this);

        //-----------------------------------------
        private Affine(Affine a, Affine b)
        {
            //copy from a
            //multiply with b

            _isIdenHint = a._isIdenHint;
            _elems = a._elems; //copy
            _elems.Multiply(ref b._elems);

        }
        //private Affine(Affine copyFrom, AffinePlan creationPlan)
        //{
        //    _elems = copyFrom._elems;
        //    //-----------------------             
        //    switch (creationPlan.cmd)
        //    {
        //        default:
        //            {
        //                throw new NotSupportedException();
        //            }
        //        case AffineMatrixCommand.None:
        //            _isIdenHint = copyFrom._isIdenHint;
        //            break;
        //        case AffineMatrixCommand.Rotate:
        //            _isIdenHint = false;
        //            _elems.Rotate(creationPlan.x);

        //            break;
        //        case AffineMatrixCommand.Scale:
        //            _isIdenHint = false;
        //            _elems.Scale(creationPlan.x, creationPlan.y);

        //            break;
        //        case AffineMatrixCommand.Skew:
        //            _isIdenHint = false;
        //            _elems.Skew(creationPlan.x, creationPlan.y);

        //            break;
        //        case AffineMatrixCommand.Translate:
        //            _isIdenHint = false;
        //            _elems.Translate(creationPlan.x, creationPlan.y);

        //            break;
        //        case AffineMatrixCommand.Invert:
        //            _isIdenHint = false;
        //            _elems.Invert();
        //            break;
        //    }
        //}

        //void BuildAff(ref AffinePlan plan)
        //{
        //    switch (plan.cmd)
        //    {
        //        case AffineMatrixCommand.None:
        //            break;
        //        case AffineMatrixCommand.Rotate:

        //            _isIdenHint = false;
        //            _elems.Rotate(plan.x);
        //            break;
        //        case AffineMatrixCommand.Scale:
        //            _isIdenHint = false;
        //            _elems.Scale(plan.x, plan.y);
        //            break;
        //        case AffineMatrixCommand.Translate:
        //            _isIdenHint = false;
        //            _elems.Translate(plan.x, plan.y);
        //            break;
        //        case AffineMatrixCommand.Skew:
        //            _isIdenHint = false;
        //            _elems.Skew(plan.x, plan.y);
        //            break;
        //        case AffineMatrixCommand.Invert:
        //            _isIdenHint = false;
        //            _elems.Invert();
        //            break;
        //        default:
        //            throw new NotSupportedException();

        //    }
        //}
        //private Affine(AffinePlan[] creationPlans)
        //{
        //    _elems = AffineMat.Iden;//copy
        //    _isIdenHint = true;
        //    if (creationPlans == null) return;
        //    //-----------------------
        //    for (int i = 0; i < creationPlans.Length; ++i)
        //    {
        //        BuildAff(ref creationPlans[i]);
        //    }
        //}
        //private Affine(int pcount, ref AffinePlan p0, ref AffinePlan p1, ref AffinePlan p2, ref AffinePlan p3, ref AffinePlan p4, params AffinePlan[] creationPlans)
        //{

        //    //-----------------------
        //    //start with identity matrix
        //    _elems = AffineMat.Iden;//copy
        //    _isIdenHint = true;
        //    //-----------------------
        //    switch (pcount)
        //    {
        //        case 0:
        //            return;
        //        case 1:
        //            BuildAff(ref p0);
        //            break;
        //        case 2:
        //            BuildAff(ref p0);
        //            BuildAff(ref p1);
        //            break;
        //        case 3:
        //            BuildAff(ref p0);
        //            BuildAff(ref p1);
        //            BuildAff(ref p2);
        //            break;
        //        case 4:
        //            BuildAff(ref p0);
        //            BuildAff(ref p1);
        //            BuildAff(ref p2);
        //            BuildAff(ref p3);
        //            break;
        //        case 5:
        //            BuildAff(ref p0);
        //            BuildAff(ref p1);
        //            BuildAff(ref p2);
        //            BuildAff(ref p3);
        //            BuildAff(ref p4);
        //            break;
        //        default:
        //            BuildAff(ref p0);
        //            BuildAff(ref p1);
        //            BuildAff(ref p2);
        //            BuildAff(ref p3);
        //            BuildAff(ref p4);
        //            if (creationPlans != null)
        //            {
        //                for (int i = 0; i < creationPlans.Length; ++i)
        //                {
        //                    BuildAff(ref creationPlans[i]);
        //                }
        //            }
        //            break;

        //    }
        //}

        ////----------------------------------------------------------
        public static Affine operator *(Affine a, Affine b)
        {
            //new input
            return new Affine(a, b);
        }
        //----------------------------------------------------------
        public Affine CreateInvert()
        {
            return new Affine(_elems.CreateInvert());
        }
        // Identity matrix
        internal static Affine NewIdentity()
        {
            var newIden = new Affine(
                1, 0,
                0, 1,
                0, 0);
            newIden._isIdenHint = true;
            return newIden;
        }

        //public static Affine New(AffinePlan p0, AffinePlan p1)
        //{
        //    AffinePlan p_empty = new AffinePlan();
        //    return new Affine(2, ref p0, ref p1, ref p_empty, ref p_empty, ref p_empty, null);
        //}
        //public static Affine New(AffinePlan p0, AffinePlan p1, AffinePlan p2)
        //{
        //    AffinePlan p_empty = new AffinePlan();
        //    return new Affine(3, ref p0, ref p1, ref p2, ref p_empty, ref p_empty, null);
        //}
        //public static Affine New(AffinePlan p0, AffinePlan p1, AffinePlan p2, AffinePlan p3)
        //{
        //    AffinePlan p_empty = new AffinePlan();
        //    return new Affine(4, ref p0, ref p1, ref p2, ref p3, ref p_empty, null);
        //}
        //public static Affine New(AffinePlan p0, AffinePlan p1, AffinePlan p2, AffinePlan p3, AffinePlan p4)
        //{
        //    return new Affine(5, ref p0, ref p1, ref p2, ref p3, ref p4, null);
        //}

        //public static Affine New(AffinePlan plan)
        //{
        //    return new Affine(IdentityMatrix, plan);
        //}
        //public static Affine New(params AffinePlan[] plans)
        //{
        //    return new Affine(plans);
        //}

        //====================================================trans_affine_rotation
        // Rotation matrix. sin() and cos() are calculated twice for the same angle.
        // There's no harm because the performance of sin()/cos() is very good on all
        // modern processors. Besides, this operation is not going to be invoked too 
        // often.
        public static Affine NewRotation(double angRad)
        {
            double cos_rad, sin_rad;

            return new Affine(
               cos_rad = Math.Cos(angRad),
               sin_rad = Math.Sin(angRad),
                -sin_rad, cos_rad,
                0.0, 0.0);

        }
        public static Affine NewRotation(double angRad, double rotationCenterX, double rotationCenterY)
        {
            AffineMat mat = AffineMat.Iden;
            
            mat.Translate(-rotationCenterX, -rotationCenterY);
            mat.Rotate(angRad);
            mat.Translate(rotationCenterX, rotationCenterY);
            return new Affine(mat);

            //return Affine.New(AffinePlan.Translate(-rotationCenterX, -rotationCenterY),
            //                  AffinePlan.Rotate(angRad),
            //                  AffinePlan.Translate(rotationCenterX, rotationCenterY)
            //               );

        }
        public static Affine NewRotationDeg(double degree)
        {
            return NewRotation(degree * (Math.PI / 180d));
        }
        public static Affine NewRotationDeg(double degree, double rotationCenterX, double rotationCenterY)
        {
            AffineMat mat = AffineMat.Iden;
            mat.Translate(-rotationCenterX, -rotationCenterY);
            mat.RotateDeg(degree);
            mat.Translate(rotationCenterX, rotationCenterY);
            return new Affine(mat);
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

        //---------------------------------- Parallelogram transformations
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
        //static void MultiplyMatrix(
        //    ref double sx, ref double sy,
        //    ref double shx, ref double shy,
        //    ref double tx, ref double ty,
        //    Affine m)
        //{
        //    double t0 = sx * m._elems.sx + shy * m._elems.shx;
        //    double t2 = shx * m._elems.sx + sy * m._elems.shx;
        //    double t4 = tx * m._elems.sx + ty * m._elems.shx + m._elems.tx;
        //    shy = sx * m._elems.shy + shy * m._elems.sy;
        //    sy = shx * m._elems.shy + sy * m._elems.sy;
        //    ty = tx * m._elems.shy + ty * m._elems.sy + m._elems.ty;
        //    sx = t0;
        //    shx = t2;
        //    tx = t4;
        //}

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
        //public Affine CreateInvert()
        //{
        //    return new Affine(this, new AffinePlan(AffineMatrixCommand.Invert, 0));
        //}
        //public Affine CreateTranslation(double x, double y)
        //{
        //    return new Affine(this, AffinePlan.Translate(x, y));
        //}

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
            x = tmp * _elems.sx + y * _elems.shx + _elems.tx;
            y = tmp * _elems.shy + y * _elems.sy + _elems.ty;
        }
        public void Transform(ref float x, ref float y)
        {
            //accessory 
            //TODO: move to extension method?
            double tmp = x;
            x = (float)(tmp * _elems.sx + y * _elems.shx + _elems.tx);
            y = (float)(tmp * _elems.shy + y * _elems.sy + _elems.ty);
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
            double a = (x - _elems.tx) * d;
            double b = (y - _elems.ty) * d;
            x = a * _elems.sy - b * _elems.shx;
            y = b * _elems.sx - a * _elems.shy;
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
            return 1.0 / (_elems.sx * _elems.sy - _elems.shy * _elems.shx);
        }

        // Get the average scale (by X and Y). 
        // Basically used to calculate the approximation_scale when
        // decomposinting curves into line segments.
        public double GetScale()
        {
            double x = 0.707106781 * _elems.sx + 0.707106781 * _elems.shx;
            double y = 0.707106781 * _elems.shy + 0.707106781 * _elems.sy;
            return Math.Sqrt(x * x + y * y);
        }

        // Check to see if the matrix is not degenerate
        public bool IsNotDegenerated(double epsilon) => Math.Abs(_elems.sx) > epsilon && Math.Abs(_elems.sy) > epsilon;

        // Check to see if it's an identity matrix
        public bool IsIdentity
        {
            get
            {
                if (!_isIdenHint)
                {
                    return is_equal_eps(_elems.sx, 1.0) && is_equal_eps(_elems.shy, 0.0) &&
                       is_equal_eps(_elems.shx, 0.0) && is_equal_eps(_elems.sy, 1.0) &&
                       is_equal_eps(_elems.tx, 0.0) && is_equal_eps(_elems.ty, 0.0);
                }
                else
                {
                    return true;
                }
            }
        }

        static bool is_equal_eps(double v1, double v2) => Math.Abs(v1 - v2) <= (EPSILON);

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



    public static class AffineTransformExtensions
    {
        public static void TransformPoints(this Affine aff, PixelFarm.Drawing.PointF[] points)
        {
            for (int i = 0; i < points.Length; ++i)
            {
                Drawing.PointF p = points[i];

                float x = p.X;
                float y = p.Y;
                aff.Transform(ref x, ref y);

                points[i] = new Drawing.PointF(x, y);
            }
        }
    }




}
