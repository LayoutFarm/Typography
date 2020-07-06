#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2006-2008 the OpenTK Team.
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing detailed licensing details.
 * 
 * Contributions by Georg W�chter.
 */
#endregion
using PixelFarm.VectorMath;
namespace PixelFarm.Contours
{
    static class Vector2fExtensions
    {
        public static Vector2f GetPerpendicularRight(this Vector2f v)
        {
            return new Vector2f(v.Y, -v.X);
        }
    }

   
    /// <summary>
    /// Represents a cubic bezier curve with two anchor and two control points.
    /// </summary>
    //[Serializable]
    struct BezierCurveCubic
    {
        #region Fields

        /// <summary>
        /// Start anchor point.
        /// </summary>
        public Vector2f StartAnchor;
        /// <summary>
        /// End anchor point.
        /// </summary>
        public Vector2f EndAnchor;
        /// <summary>
        /// First control point, controls the direction of the curve start.
        /// </summary>
        public Vector2f FirstControlPoint;
        /// <summary>
        /// Second control point, controls the direction of the curve end.
        /// </summary>
        public Vector2f SecondControlPoint;
        /// <summary>
        /// Gets or sets the parallel value.
        /// </summary>
        /// <remarks>This value defines whether the curve should be calculated as a
        /// parallel curve to the original bezier curve. A value of 0.0f represents
        /// the original curve, 5.0f i.e. stands for a curve that has always a distance
        /// of 5.f to the original curve at any point.</remarks>
        public float Parallel;
        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new <see cref="BezierCurveCubic"/>.
        /// </summary>
        /// <param name="startAnchor">The start anchor point.</param>
        /// <param name="endAnchor">The end anchor point.</param>
        /// <param name="firstControlPoint">The first control point.</param>
        /// <param name="secondControlPoint">The second control point.</param>
        public BezierCurveCubic(Vector2f startAnchor, Vector2f endAnchor, Vector2f firstControlPoint, Vector2f secondControlPoint)
        {
            this.StartAnchor = startAnchor;
            this.EndAnchor = endAnchor;
            this.FirstControlPoint = firstControlPoint;
            this.SecondControlPoint = secondControlPoint;
            this.Parallel = 0.0f;
        }

        /// <summary>
        /// Constructs a new <see cref="BezierCurveCubic"/>.
        /// </summary>
        /// <param name="parallel">The parallel value.</param>
        /// <param name="startAnchor">The start anchor point.</param>
        /// <param name="endAnchor">The end anchor point.</param>
        /// <param name="firstControlPoint">The first control point.</param>
        /// <param name="secondControlPoint">The second control point.</param>
        public BezierCurveCubic(float parallel, Vector2f startAnchor, Vector2f endAnchor, Vector2f firstControlPoint, Vector2f secondControlPoint)
        {
            this.Parallel = parallel;
            this.StartAnchor = startAnchor;
            this.EndAnchor = endAnchor;
            this.FirstControlPoint = firstControlPoint;
            this.SecondControlPoint = secondControlPoint;
        }

        #endregion

        #region Functions

        /// <summary>
        /// Calculates the point with the specified t.
        /// </summary>
        /// <param name="t">The t value, between 0.0f and 1.0f.</param>
        /// <returns>Resulting point.</returns>
        public Vector2f CalculatePoint(float t)
        {
            Vector2f r = new Vector2f();
            float c = 1.0f - t;
            r.X = (StartAnchor.X * c * c * c) + (FirstControlPoint.X * 3 * t * c * c) + (SecondControlPoint.X * 3 * t * t * c)
                + EndAnchor.X * t * t * t;
            r.Y = (StartAnchor.Y * c * c * c) + (FirstControlPoint.Y * 3 * t * c * c) + (SecondControlPoint.Y * 3 * t * t * c)
                + EndAnchor.Y * t * t * t;
            if (Parallel == 0.0f)
                return r;
            Vector2f perpendicular = new Vector2f();
            if (t == 0.0f)
                perpendicular = FirstControlPoint - StartAnchor;
            else
                perpendicular = r - CalculatePointOfDerivative(t);
            return r + Vector2f.Normalize(perpendicular).GetPerpendicularRight() * Parallel;
        }

        /// <summary>
        /// Calculates the point with the specified t of the derivative of this function.
        /// </summary>
        /// <param name="t">The t, value between 0.0f and 1.0f.</param>
        /// <returns>Resulting point.</returns>
        private Vector2f CalculatePointOfDerivative(float t)
        {
            Vector2f r = new Vector2f();
            float c = 1.0f - t;
            r.X = (c * c * StartAnchor.X) + (2 * t * c * FirstControlPoint.X) + (t * t * SecondControlPoint.X);
            r.Y = (c * c * StartAnchor.Y) + (2 * t * c * FirstControlPoint.Y) + (t * t * SecondControlPoint.Y);
            return r;
        }

        ///// <summary>
        ///// Calculates the length of this bezier curve.
        ///// </summary>
        ///// <param name="precision">The precision.</param>
        ///// <returns>Length of the curve.</returns>
        ///// <remarks>The precision gets better when the <paramref name="precision"/>
        ///// value gets smaller.</remarks>
        //public float CalculateLength(float precision)
        //{
        //    double length = 0.0f;
        //    Vector2f old = CalculatePoint(0.0f);
        //    for (float i = precision; i < (1.0f + precision); i += precision)
        //    {
        //        Vector2f n = CalculatePoint(i);
        //        length += (n - old).Length;
        //        old = n;
        //    }

        //    return (float)length;
        //}

        #endregion
    }

    /// <summary>
    /// Represents a quadric bezier curve with two anchor and one control point.
    /// </summary>
    //[Serializable]
    public struct BezierCurveQuadric
    {
        #region Fields

        /// <summary>
        /// Start anchor point.
        /// </summary>
        public Vector2f StartAnchor;
        /// <summary>
        /// End anchor point.
        /// </summary>
        public Vector2f EndAnchor;
        /// <summary>
        /// Control point, controls the direction of both endings of the curve.
        /// </summary>
        public Vector2f ControlPoint;
        /// <summary>
        /// The parallel value.
        /// </summary>
        /// <remarks>This value defines whether the curve should be calculated as a
        /// parallel curve to the original bezier curve. A value of 0.0f represents
        /// the original curve, 5.0f i.e. stands for a curve that has always a distance
        /// of 5.f to the original curve at any point.</remarks>
        public float Parallel;
        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new <see cref="BezierCurveQuadric"/>.
        /// </summary>
        /// <param name="startAnchor">The start anchor.</param>
        /// <param name="endAnchor">The end anchor.</param>
        /// <param name="controlPoint">The control point.</param>
        public BezierCurveQuadric(Vector2f startAnchor, Vector2f endAnchor, Vector2f controlPoint)
        {
            this.StartAnchor = startAnchor;
            this.EndAnchor = endAnchor;
            this.ControlPoint = controlPoint;
            this.Parallel = 0.0f;
        }

        /// <summary>
        /// Constructs a new <see cref="BezierCurveQuadric"/>.
        /// </summary>
        /// <param name="parallel">The parallel value.</param>
        /// <param name="startAnchor">The start anchor.</param>
        /// <param name="endAnchor">The end anchor.</param>
        /// <param name="controlPoint">The control point.</param>
        public BezierCurveQuadric(float parallel, Vector2f startAnchor, Vector2f endAnchor, Vector2f controlPoint)
        {
            this.Parallel = parallel;
            this.StartAnchor = startAnchor;
            this.EndAnchor = endAnchor;
            this.ControlPoint = controlPoint;
        }

        #endregion

        #region Functions

        /// <summary>
        /// Calculates the point with the specified t.
        /// </summary>
        /// <param name="t">The t value, between 0.0f and 1.0f.</param>
        /// <returns>Resulting point.</returns>
        public Vector2f CalculatePoint(float t)
        {
            Vector2f r = new Vector2f();
            float c = 1.0f - t;
            r.X = (c * c * StartAnchor.X) + (2 * t * c * ControlPoint.X) + (t * t * EndAnchor.X);
            r.Y = (c * c * StartAnchor.Y) + (2 * t * c * ControlPoint.Y) + (t * t * EndAnchor.Y);
            if (Parallel == 0.0f)
                return r;
            Vector2f perpendicular = new Vector2f();
            if (t == 0.0f)
                perpendicular = ControlPoint - StartAnchor;
            else
                perpendicular = r - CalculatePointOfDerivative(t);
            return r + Vector2f.Normalize(perpendicular).GetPerpendicularRight() * Parallel;
        }

        /// <summary>
        /// Calculates the point with the specified t of the derivative of this function.
        /// </summary>
        /// <param name="t">The t, value between 0.0f and 1.0f.</param>
        /// <returns>Resulting point.</returns>
        private Vector2f CalculatePointOfDerivative(float t)
        {
            Vector2f r = new Vector2f();
            r.X = (1.0f - t) * StartAnchor.X + t * ControlPoint.X;
            r.Y = (1.0f - t) * StartAnchor.Y + t * ControlPoint.Y;
            return r;
        }

        /// <summary>
        /// Calculates the length of this bezier curve.
        /// </summary>
        /// <param name="precision">The precision.</param>
        /// <returns>Length of curve.</returns>
        /// <remarks>The precision gets better when the <paramref name="precision"/>
        /// value gets smaller.</remarks>
        public float CalculateLength(float precision)
        {
            double length = 0.0f;
            Vector2f old = CalculatePoint(0.0f);
            for (float i = precision; i < (1.0f + precision); i += precision)
            {
                Vector2f n = CalculatePoint(i);
                length += (n - old).Length();
                old = n;
            }

            return (float)length;
        }

        #endregion
    }
}
