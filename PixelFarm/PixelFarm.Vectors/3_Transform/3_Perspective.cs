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
// Perspective 2D transformations
//
//----------------------------------------------------------------------------

using System;
namespace PixelFarm.CpuBlit.VertexProcessing
{
    //=======================================================trans_perspective
    public sealed class Perspective : ICoordTransformer
    {
        const double EPSILON = 1e-14;
        double sx, shy, w0, shx, sy, w1, tx, ty, w2;
        //------------------------------------------------------- 
        // Identity matrix
        public Perspective()
        {
            sx = 1; shy = 0; w0 = 0;
            shx = 0; sy = 1; w1 = 0;
            tx = 0; ty = 0; w2 = 1;
        }

        // Custom matrix
        public Perspective(double v0_sx, double v1_shy, double v2_w0,
                          double v3_shx, double v4_sy, double v5_w1,
                          double v6_tx, double v7_ty, double v8_w2)
        {
            sx = v0_sx; shy = v1_shy; w0 = v2_w0;
            shx = v3_shx; sy = v4_sy; w1 = v5_w1;
            tx = v6_tx; ty = v7_ty; w2 = v8_w2;
        }


        // From affine
        public Perspective(Affine a)
        {
            sx = a.sx; shy = a.shy; w0 = 0;
            shx = a.shx; sy = a.sy; w1 = 0;
            tx = a.tx; ty = a.ty; w2 = 1;
        }

        // From trans_perspective
        public Perspective(Perspective a)
        {
            sx = a.sx; shy = a.shy; w0 = a.w0;
            shx = a.shx; sy = a.sy; w1 = a.w1;
            tx = a.tx; ty = a.ty; w2 = a.w2;
        }

        // Rectangle to quadrilateral s
        public Perspective(double x1, double y1, double x2, double y2, double[] quad)
        {
            unsafe
            {
                fixed (double* q_h = &quad[0])
                {
                    double* r = stackalloc double[8];
                    r[0] = r[6] = x1;
                    r[2] = r[4] = x2;
                    r[1] = r[3] = y1;
                    r[5] = r[7] = y2;
                    InternalGenerateQuadToQuad(r, q_h);
                }
            }
        }

        // Quadrilateral to rectangle
        public Perspective(double[] quad, double x1, double y1, double x2, double y2)
        {
            unsafe
            {
                fixed (double* q_h = &quad[0])
                {
                    double* r = stackalloc double[8];
                    r[0] = r[6] = x1;
                    r[2] = r[4] = x2;
                    r[1] = r[3] = y1;
                    r[5] = r[7] = y2;
                    InternalGenerateQuadToQuad(q_h, r);
                }
            }
        }

        // Arbitrary quadrilateral transformations
        public Perspective(double[] src, double[] dst)
        {
            quad_to_quad(src, dst);
        }

        void Set(Perspective Other)
        {
            sx = Other.sx;
            shy = Other.shy;
            w0 = Other.w0;
            shx = Other.shx;
            sy = Other.sy;
            w1 = Other.w1;
            tx = Other.tx;
            ty = Other.ty;
            w2 = Other.w2;
        }

        //-------------------------------------- Quadrilateral transformations
        // The arguments are double[8] that are mapped to quadrilaterals:
        // x1,y1, x2,y2, x3,y3, x4,y4
        public bool quad_to_quad(double[] qs, double[] qd)
        {
            unsafe
            {
                fixed (double* qs_h = &qs[0])
                fixed (double* qd_h = &qd[0])
                {
                    return InternalGenerateQuadToQuad(qs_h, qd_h);
                }
            }
        }
        unsafe bool InternalGenerateQuadToQuad(double* qs_h, double* qdHead)
        {
            Perspective p = new Perspective();
            if (!square_to_quad(qs_h))
            {
                return false;
            }

            invert();
            //---------------------------------
            if (!p.square_to_quad(qdHead))
            {
                return false;
            }
            multiply(p);
            return true;
        }



        // Map square (0,0,1,1) to the quadrilateral and vice versa
        unsafe bool square_to_quad(double* q)
        {
            double dx = q[0] - q[2] + q[4] - q[6];
            double dy = q[1] - q[3] + q[5] - q[7];
            if (dx == 0.0 && dy == 0.0)
            {
                // Affine case (parallelogram)
                //---------------
                sx = q[2] - q[0];
                shy = q[3] - q[1];
                w0 = 0.0;
                shx = q[4] - q[2];
                sy = q[5] - q[3];
                w1 = 0.0;
                tx = q[0];
                ty = q[1];
                w2 = 1.0;
            }
            else
            {
                double dx1 = q[2] - q[4];
                double dy1 = q[3] - q[5];
                double dx2 = q[6] - q[4];
                double dy2 = q[7] - q[5];
                double den = dx1 * dy2 - dx2 * dy1;
                if (den == 0.0)
                {
                    // Singular case
                    //---------------
                    sx = shy = w0 = shx = sy = w1 = tx = ty = w2 = 0.0;
                    return false;
                }
                // General case
                //---------------
                double u = (dx * dy2 - dy * dx2) / den;
                double v = (dy * dx1 - dx * dy1) / den;
                sx = q[2] - q[0] + u * q[2];
                shy = q[3] - q[1] + u * q[3];
                w0 = u;
                shx = q[6] - q[0] + v * q[6];
                sy = q[7] - q[1] + v * q[7];
                w1 = v;
                tx = q[0];
                ty = q[1];
                w2 = 1.0;
            }


            //double dx = q[0] - q[2] + q[4] - q[6];
            //double dy = q[1] - q[3] + q[5] - q[7];
            //if (dx == 0.0 && dy == 0.0)
            //{
            //    // Affine case (parallelogram)
            //    //---------------
            //    sx = q[2] - q[0];
            //    shy = q[3] - q[1];
            //    w0 = 0.0;
            //    shx = q[4] - q[2];
            //    sy = q[5] - q[3];
            //    w1 = 0.0;
            //    tx = q[0];
            //    ty = q[1];
            //    w2 = 1.0;
            //}
            //else
            //{
            //    double dx1 = q[2] - q[4];
            //    double dy1 = q[3] - q[5];
            //    double dx2 = q[6] - q[4];
            //    double dy2 = q[7] - q[5];
            //    double den = dx1 * dy2 - dx2 * dy1;
            //    if (den == 0.0)
            //    {
            //        // Singular case
            //        //---------------
            //        sx = shy = w0 = shx = sy = w1 = tx = ty = w2 = 0.0;
            //        return false;
            //    }
            //    // General case
            //    //---------------
            //    double u = (dx * dy2 - dy * dx2) / den;
            //    double v = (dy * dx1 - dx * dy1) / den;
            //    sx = q[2] - q[0] + u * q[2];
            //    shy = q[3] - q[1] + u * q[3];
            //    w0 = u;
            //    shx = q[6] - q[0] + v * q[6];
            //    sy = q[7] - q[1] + v * q[7];
            //    w1 = v;
            //    tx = q[0];
            //    ty = q[1];
            //    w2 = 1.0;
            //}
            return true;
        }
        //--------------------------------------------------------- Operations
        public Perspective from_affine(Affine a)
        {
            sx = a.sx; shy = a.shy; w0 = 0;
            shx = a.shx; sy = a.sy; w1 = 0;
            tx = a.tx; ty = a.ty; w2 = 1;
            return this;
        }

        // Reset - load an identity matrix
        Perspective reset()
        {
            sx = 1; shy = 0; w0 = 0;
            shx = 0; sy = 1; w1 = 0;
            tx = 0; ty = 0; w2 = 1;
            return this;
        }

        // Invert matrix. Returns false in degenerate case
        bool invert()
        {
            double d0 = sy * w2 - w1 * ty;
            double d1 = w0 * ty - shy * w2;
            double d2 = shy * w1 - w0 * sy;
            double d = sx * d0 + shx * d1 + tx * d2;
            if (d == 0.0)
            {
                sx = shy = w0 = shx = sy = w1 = tx = ty = w2 = 0.0;
                return false;
            }
            d = 1.0 / d;
            Perspective a = new Perspective(this);
            sx = d * d0;
            shy = d * d1;
            w0 = d * d2;
            shx = d * (a.w1 * a.tx - a.shx * a.w2);
            sy = d * (a.sx * a.w2 - a.w0 * a.tx);
            w1 = d * (a.w0 * a.shx - a.sx * a.w1);
            tx = d * (a.shx * a.ty - a.sy * a.tx);
            ty = d * (a.shy * a.tx - a.sx * a.ty);
            w2 = d * (a.sx * a.sy - a.shy * a.shx);
            return true;
        }

        // Direct transformations operations
        Perspective translate(double x, double y)
        {
            tx += x;
            ty += y;
            return this;
        }

        Perspective rotate(double a)
        {
            multiply(Affine.NewRotation(a));
            return this;
        }

        Perspective scale(double s)
        {
            multiply(Affine.NewScaling(s));
            return this;
        }

        Perspective scale(double x, double y)
        {
            multiply(Affine.NewScaling(x, y));
            return this;
        }

        Perspective multiply(Perspective a)
        {
            Perspective b = new Perspective(this);
            sx = a.sx * b.sx + a.shx * b.shy + a.tx * b.w0;
            shx = a.sx * b.shx + a.shx * b.sy + a.tx * b.w1;
            tx = a.sx * b.tx + a.shx * b.ty + a.tx * b.w2;
            shy = a.shy * b.sx + a.sy * b.shy + a.ty * b.w0;
            sy = a.shy * b.shx + a.sy * b.sy + a.ty * b.w1;
            ty = a.shy * b.tx + a.sy * b.ty + a.ty * b.w2;
            w0 = a.w0 * b.sx + a.w1 * b.shy + a.w2 * b.w0;
            w1 = a.w0 * b.shx + a.w1 * b.sy + a.w2 * b.w1;
            w2 = a.w0 * b.tx + a.w1 * b.ty + a.w2 * b.w2;
            return this;
        }

        //------------------------------------------------------------------------
        Perspective multiply(Affine a)
        {
            Perspective b = new Perspective(this);
            sx = a.sx * b.sx + a.shx * b.shy + a.tx * b.w0;
            shx = a.sx * b.shx + a.shx * b.sy + a.tx * b.w1;
            tx = a.sx * b.tx + a.shx * b.ty + a.tx * b.w2;
            shy = a.shy * b.sx + a.sy * b.shy + a.ty * b.w0;
            sy = a.shy * b.shx + a.sy * b.sy + a.ty * b.w1;
            ty = a.shy * b.tx + a.sy * b.ty + a.ty * b.w2;
            return this;
        }

        //------------------------------------------------------------------------
        Perspective premultiply(Perspective b)
        {
            Perspective a = new Perspective(this);
            sx = a.sx * b.sx + a.shx * b.shy + a.tx * b.w0;
            shx = a.sx * b.shx + a.shx * b.sy + a.tx * b.w1;
            tx = a.sx * b.tx + a.shx * b.ty + a.tx * b.w2;
            shy = a.shy * b.sx + a.sy * b.shy + a.ty * b.w0;
            sy = a.shy * b.shx + a.sy * b.sy + a.ty * b.w1;
            ty = a.shy * b.tx + a.sy * b.ty + a.ty * b.w2;
            w0 = a.w0 * b.sx + a.w1 * b.shy + a.w2 * b.w0;
            w1 = a.w0 * b.shx + a.w1 * b.sy + a.w2 * b.w1;
            w2 = a.w0 * b.tx + a.w1 * b.ty + a.w2 * b.w2;
            return this;
        }

        //------------------------------------------------------------------------
        //Perspective premultiply(Affine b)
        //{
        //    //copy this to a
        //    Perspective a = new Perspective(this);

        //    sx = a.sx * b.sx + a.shx * b.shy;
        //    shx = a.sx * b.shx + a.shx * b.sy;
        //    tx = a.sx * b.tx + a.shx * b.ty + a.tx;
        //    shy = a.shy * b.sx + a.sy * b.shy;
        //    sy = a.shy * b.shx + a.sy * b.sy;
        //    ty = a.shy * b.tx + a.sy * b.ty + a.ty;
        //    w0 = a.w0 * b.sx + a.w1 * b.shy;
        //    w1 = a.w0 * b.shx + a.w1 * b.sy;
        //    w2 = a.w0 * b.tx + a.w1 * b.ty + a.w2;

        //    return this;
        //}

        //------------------------------------------------------------------------
        Perspective multiply_inv(Perspective m)
        {
            Perspective t = m;
            t.invert();
            return multiply(t);
        }

        //------------------------------------------------------------------------
        Perspective trans_perspectivemultiply_inv(Affine m)
        {
            Affine t = m;
            var invert = t.CreateInvert();
            return multiply(invert);
        }

        //------------------------------------------------------------------------
        Perspective premultiply_inv(Perspective m)
        {
            Perspective t = m;
            t.invert();
            Set(t.multiply(this));
            return this;
        }

        // Multiply inverse of "m" by "this" and assign the result to "this"
        Perspective premultiply_inv(Affine m)
        {
            Perspective t = new Perspective(m);
            t.invert();
            Set(t.multiply(this));
            return this;
        }

        //--------------------------------------------------------- Load/Store
        void store_to(double[] m)
        {
            m[0] = sx; m[1] = shy; m[2] = w0;
            m[3] = shx; m[4] = sy; m[5] = w1;
            m[6] = tx; m[7] = ty; m[8] = w2;
        }

        //------------------------------------------------------------------------
        Perspective load_from(double[] m)
        {
            sx = m[0]; shy = m[1]; w0 = m[2];
            shx = m[3]; sy = m[4]; w1 = m[5];
            tx = m[6]; ty = m[7]; w2 = m[8];
            return this;
        }

        //---------------------------------------------------------- Operators
        // Multiply the matrix by another one and return the result in a separate matrix.
        public static Perspective operator *(Perspective a, Perspective b)
        {
            Perspective temp = a;
            temp.multiply(b);
            return temp;
        }

        // Multiply the matrix by another one and return the result in a separate matrix.
        public static Perspective operator *(Perspective a, Affine b)
        {
            Perspective temp = a;
            temp.multiply(b);
            return temp;
        }

        //// Multiply the matrix by inverse of another one and return the result in a separate matrix.
        //public static Perspective operator /(Perspective a, Perspective b)
        //{
        //    Perspective temp = a;
        //    temp.multiply_inv(b);

        //    return temp;
        //}

        //// Calculate and return the inverse matrix
        //public static Perspective operator ~(Perspective b)
        //{
        //    Perspective ret = b;
        //    ret.invert();
        //    return ret;
        //}

        //// Equal operator with default epsilon
        //public static bool operator ==(Perspective a, Perspective b)
        //{
        //    return a.is_equal(b, EPSILON);
        //}

        //// Not Equal operator with default epsilon
        //public static bool operator !=(Perspective a, Perspective b)
        //{
        //    return !a.is_equal(b, EPSILON);
        //}

        //public override bool Equals(object obj)
        //{
        //    return base.Equals(obj);
        //}

        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}

        //---------------------------------------------------- Transformations
        // Direct transformation of x and y
        public void Transform(ref double px, ref double py)
        {
            double x = px;
            double y = py;
            double m = 1.0 / (x * w0 + y * w1 + w2);
            px = m * (x * sx + y * shx + tx);
            py = m * (x * shy + y * sy + ty);
        }

        // Direct transformation of x and y, affine part only
        void transform_affine(ref double x, ref double y)
        {
            double tmp = x;
            x = tmp * sx + y * shx + tx;
            y = tmp * shy + y * sy + ty;
        }

        // Direct transformation of x and y, 2x2 matrix only, no translation
        void transform_2x2(ref double x, ref double y)
        {
            double tmp = x;
            x = tmp * sx + y * shx;
            y = tmp * shy + y * sy;
        }

        // Inverse transformation of x and y. It works slow because
        // it explicitly inverts the matrix on every call. For massive 
        // operations it's better to invert() the matrix and then use 
        // direct transformations. 
        void inverse_transform(ref double x, ref double y)
        {
            Perspective t = new Perspective(this);
            if (t.invert()) t.Transform(ref x, ref y);
        }


        //---------------------------------------------------------- Auxiliary
        double determinant()
        {
            return sx * (sy * w2 - ty * w1) +
                   shx * (ty * w0 - shy * w2) +
                   tx * (shy * w1 - sy * w0);
        }
        double determinant_reciprocal()
        {
            return 1.0 / determinant();
        }


        public bool IsValid
        {
            get
            {
                return Math.Abs(sx) > EPSILON &&
                    Math.Abs(sy) > EPSILON &&
                    Math.Abs(w2) > EPSILON;
            }
        }

        bool is_identity()
        {
            return AggMath.is_equal_eps(sx, 1.0, EPSILON) &&
                   AggMath.is_equal_eps(shy, 0.0, EPSILON) &&
                   AggMath.is_equal_eps(w0, 0.0, EPSILON) &&
                   AggMath.is_equal_eps(shx, 0.0, EPSILON) &&
                   AggMath.is_equal_eps(sy, 1.0, EPSILON) &&
                   AggMath.is_equal_eps(w1, 0.0, EPSILON) &&
                   AggMath.is_equal_eps(tx, 0.0, EPSILON) &&
                   AggMath.is_equal_eps(ty, 0.0, EPSILON) &&
                   AggMath.is_equal_eps(w2, 1.0, EPSILON);
        }

        //public bool is_equal(Perspective m)
        //{
        //    return is_equal(m, EPSILON);
        //}

        //public bool is_equal(Perspective m, double epsilon)
        //{
        //    return AggBasics.is_equal_eps(sx, m.sx, epsilon) &&
        //           AggBasics.is_equal_eps(shy, m.shy, epsilon) &&
        //           AggBasics.is_equal_eps(w0, m.w0, epsilon) &&
        //           AggBasics.is_equal_eps(shx, m.shx, epsilon) &&
        //           AggBasics.is_equal_eps(sy, m.sy, epsilon) &&
        //           AggBasics.is_equal_eps(w1, m.w1, epsilon) &&
        //           AggBasics.is_equal_eps(tx, m.tx, epsilon) &&
        //           AggBasics.is_equal_eps(ty, m.ty, epsilon) &&
        //           AggBasics.is_equal_eps(w2, m.w2, epsilon);
        //}

        // Determine the major affine parameters. Use with caution 
        // considering possible degenerate cases.
        double scale()
        {
            double x = 0.707106781 * sx + 0.707106781 * shx;
            double y = 0.707106781 * shy + 0.707106781 * sy;
            return Math.Sqrt(x * x + y * y);
        }
        double rotation()
        {
            double x1 = 0.0;
            double y1 = 0.0;
            double x2 = 1.0;
            double y2 = 0.0;
            Transform(ref x1, ref y1);
            Transform(ref x2, ref y2);
            return Math.Atan2(y2 - y1, x2 - x1);
        }
        void translation(out double dx, out double dy)
        {
            dx = tx;
            dy = ty;
        }
        void scaling(out double x, out double y)
        {
            double x1 = 0.0;
            double y1 = 0.0;
            double x2 = 1.0;
            double y2 = 1.0;
            Perspective t = new Perspective(this);
            t *= Affine.NewRotation(-rotation());
            t.Transform(ref x1, ref y1);
            t.Transform(ref x2, ref y2);
            x = x2 - x1;
            y = y2 - y1;
        }
        void scaling_abs(out double x, out double y)
        {
            x = Math.Sqrt(sx * sx + shx * shx);
            y = Math.Sqrt(shy * shy + sy * sy);
        }

        
    }
}
