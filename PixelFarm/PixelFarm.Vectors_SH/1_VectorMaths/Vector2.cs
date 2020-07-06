/*
Copyright (c) 2006 - 2008 The Open Toolkit library.

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Runtime.InteropServices;
namespace PixelFarm.VectorMath
{
    /// <summary>Represents a 2D vector using two double-precision floating-point numbers.</summary>
    //[Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2 : IEquatable<Vector2>
    {

        /// <summary>The X coordinate of this instance.</summary>
        public double x;
        /// <summary>The Y coordinate of this instance.</summary>
        public double y;
        /// <summary>
        /// Defines a unit-length Vector2d that points towards the X-axis.
        /// </summary>
        public static readonly Vector2 UnitX = new Vector2(1, 0);
        /// <summary>
        /// Defines a unit-length Vector2d that points towards the Y-axis.
        /// </summary>
        public static readonly Vector2 UnitY = new Vector2(0, 1);
        /// <summary>
        /// Defines a zero-length Vector2d.
        /// </summary>
        public static readonly Vector2 Zero = new Vector2(0, 0);
        /// <summary>
        /// Defines an instance with all components set to 1.
        /// </summary>
        public static readonly Vector2 One = new Vector2(1, 1);
        /// <summary>
        /// Defines the size of the Vector2d struct in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf(new Vector2());


        /// <summary>Constructs left vector with the given coordinates.</summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public Vector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2(Vector3 vector)
        {
            this.x = vector.x;
            this.y = vector.y;
        }
        public Vector2 NewLength(double newLength)
        {
            //radian
            double atan = Math.Atan2(y, x);
            return new Vector2(Math.Cos(atan) * newLength,
                        Math.Sin(atan) * newLength);
        }


        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    default:
                        return 0;
                }
            }

            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    default:
                        throw new Exception();
                }
            }
        }




        /// <summary>
        /// Gets the length (magnitude) of the vector.
        /// </summary>
        /// <seealso cref="LengthSquared"/>
        public double Length
        {
            get
            {
                return System.Math.Sqrt(x * x + y * y);
            }
        }



        /// <summary>
        /// Gets the square of the vector length (magnitude).
        /// </summary>
        /// <remarks>
        /// This property avoids the costly square root operation required by the Length property. This makes it more suitable
        /// for comparisons.
        /// </remarks>
        /// <see cref="Length"/>
        public double LengthSquared
        {
            get
            {
                return x * x + y * y;
            }
        }


        public Vector2 RotateInDegree(double deg)
        {

            double rad = deg * (System.Math.PI / 180.0f); //convert from degree to rad 
            double sin = System.Math.Sin(rad);
            double cos = System.Math.Cos(rad);

            return new Vector2(
                x * cos - y * sin,
                x * sin + y * cos);
        }

        public double GetAngle()
        {
            return System.Math.Atan2(y, x);
        }

        public double GetAngle0To2PI()
        {
            return MathHelper.Range0ToTau(GetAngle());
        }


        /// <summary>
        /// Gets the perpendicular vector on the right side of this vector.
        /// </summary>
        public Vector2 GetPerpendicularRight()
        {
            return new Vector2(y, -x);
        }
        /// <summary>
        /// Gets the perpendicular vector on the right side of this vector.
        /// </summary>
        public Vector2 PerpendicularRight
        {
            get
            {
                return new Vector2(Y, -X);
            }
        }


        /// <summary>
        /// Gets the perpendicular vector on the left side of this vector.
        /// </summary>
        public Vector2 GetPerpendicularLeft()
        {
            return new Vector2(-y, x);
        }



        /// <summary>
        /// Returns a normalized Vector of this.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetNormal()
        {
            Vector2 temp = this;
            temp.Normalize();
            return temp;
        }

        /// <summary>
        /// Scales the Vector2 to unit length.
        /// </summary>
        public void Normalize()
        {
            double scale = 1.0 / Length;
            x *= scale;
            y *= scale;
        }





        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="a">Left operand.</param>
        /// <param name="b">Right operand.</param>
        /// <returns>Result of operation.</returns>
        public static Vector2 Add(Vector2 a, Vector2 b)
        {
            Add(ref a, ref b, out a);
            return a;
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="a">Left operand.</param>
        /// <param name="b">Right operand.</param>
        /// <param name="result">Result of operation.</param>
        public static void Add(ref Vector2 a, ref Vector2 b, out Vector2 result)
        {
            result = new Vector2(a.x + b.x, a.y + b.y);
        }



        /// <summary>
        /// Subtract one Vector from another
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>Result of subtraction</returns>
        public static Vector2 Subtract(Vector2 a, Vector2 b)
        {
            Subtract(ref a, ref b, out a);
            return a;
        }

        /// <summary>
        /// Subtract one Vector from another
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <param name="result">Result of subtraction</param>
        public static void Subtract(ref Vector2 a, ref Vector2 b, out Vector2 result)
        {
            result = new Vector2(a.x - b.x, a.y - b.y);
        }



        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="vector">Left operand.</param>
        /// <param name="scale">Right operand.</param>
        /// <returns>Result of the operation.</returns>
        public static Vector2 Multiply(Vector2 vector, double scale)
        {
            Multiply(ref vector, scale, out vector);
            return vector;
        }

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="vector">Left operand.</param>
        /// <param name="scale">Right operand.</param>
        /// <param name="result">Result of the operation.</param>
        public static void Multiply(ref Vector2 vector, double scale, out Vector2 result)
        {
            result = new Vector2(vector.x * scale, vector.y * scale);
        }

        /// <summary>
        /// Multiplies a vector by the components a vector (scale).
        /// </summary>
        /// <param name="vector">Left operand.</param>
        /// <param name="scale">Right operand.</param>
        /// <returns>Result of the operation.</returns>
        public static Vector2 Multiply(Vector2 vector, Vector2 scale)
        {
            Multiply(ref vector, ref scale, out vector);
            return vector;
        }

        /// <summary>
        /// Multiplies a vector by the components of a vector (scale).
        /// </summary>
        /// <param name="vector">Left operand.</param>
        /// <param name="scale">Right operand.</param>
        /// <param name="result">Result of the operation.</param>
        public static void Multiply(ref Vector2 vector, ref Vector2 scale, out Vector2 result)
        {
            result = new Vector2(vector.x * scale.x, vector.y * scale.y);
        }



        /// <summary>
        /// Divides a vector by a scalar.
        /// </summary>
        /// <param name="vector">Left operand.</param>
        /// <param name="scale">Right operand.</param>
        /// <returns>Result of the operation.</returns>
        public static Vector2 Divide(Vector2 vector, double scale)
        {
            Divide(ref vector, scale, out vector);
            return vector;
        }

        /// <summary>
        /// Divides a vector by a scalar.
        /// </summary>
        /// <param name="vector">Left operand.</param>
        /// <param name="scale">Right operand.</param>
        /// <param name="result">Result of the operation.</param>
        public static void Divide(ref Vector2 vector, double scale, out Vector2 result)
        {
            Multiply(ref vector, 1 / scale, out result);
        }

        /// <summary>
        /// Divides a vector by the components of a vector (scale).
        /// </summary>
        /// <param name="vector">Left operand.</param>
        /// <param name="scale">Right operand.</param>
        /// <returns>Result of the operation.</returns>
        public static Vector2 Divide(Vector2 vector, Vector2 scale)
        {
            Divide(ref vector, ref scale, out vector);
            return vector;
        }

        /// <summary>
        /// Divide a vector by the components of a vector (scale).
        /// </summary>
        /// <param name="vector">Left operand.</param>
        /// <param name="scale">Right operand.</param>
        /// <param name="result">Result of the operation.</param>
        public static void Divide(ref Vector2 vector, ref Vector2 scale, out Vector2 result)
        {
            result = new Vector2(vector.x / scale.x, vector.y / scale.y);
        }



        /// <summary>
        /// Calculate the component-wise minimum of two vectors
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>The component-wise minimum</returns>
        public static Vector2 Min(Vector2 a, Vector2 b)
        {
            a.x = a.x < b.x ? a.x : b.x;
            a.y = a.y < b.y ? a.y : b.y;
            return a;
        }

        /// <summary>
        /// Calculate the component-wise minimum of two vectors
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <param name="result">The component-wise minimum</param>
        public static void Min(ref Vector2 a, ref Vector2 b, out Vector2 result)
        {
            result.x = a.x < b.x ? a.x : b.x;
            result.y = a.y < b.y ? a.y : b.y;
        }



        /// <summary>
        /// Calculate the component-wise maximum of two vectors
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>The component-wise maximum</returns>
        public static Vector2 Max(Vector2 a, Vector2 b)
        {
            a.x = a.x > b.x ? a.x : b.x;
            a.y = a.y > b.y ? a.y : b.y;
            return a;
        }

        /// <summary>
        /// Calculate the component-wise maximum of two vectors
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <param name="result">The component-wise maximum</param>
        public static void Max(ref Vector2 a, ref Vector2 b, out Vector2 result)
        {
            result.x = a.x > b.x ? a.x : b.x;
            result.y = a.y > b.y ? a.y : b.y;
        }



        /// <summary>
        /// Clamp a vector to the given minimum and maximum vectors
        /// </summary>
        /// <param name="vec">Input vector</param>
        /// <param name="min">Minimum vector</param>
        /// <param name="max">Maximum vector</param>
        /// <returns>The clamped vector</returns>
        public static Vector2 Clamp(Vector2 vec, Vector2 min, Vector2 max)
        {
            vec.x = vec.x < min.x ? min.x : vec.x > max.x ? max.x : vec.x;
            vec.y = vec.y < min.y ? min.y : vec.y > max.y ? max.y : vec.y;
            return vec;
        }

        /// <summary>
        /// Clamp a vector to the given minimum and maximum vectors
        /// </summary>
        /// <param name="vec">Input vector</param>
        /// <param name="min">Minimum vector</param>
        /// <param name="max">Maximum vector</param>
        /// <param name="result">The clamped vector</param>
        public static void Clamp(ref Vector2 vec, ref Vector2 min, ref Vector2 max, out Vector2 result)
        {
            result.x = vec.x < min.x ? min.x : vec.x > max.x ? max.x : vec.x;
            result.y = vec.y < min.y ? min.y : vec.y > max.y ? max.y : vec.y;
        }



        /// <summary>
        /// Scale a vector to unit length
        /// </summary>
        /// <param name="vec">The input vector</param>
        /// <returns>The normalized vector</returns>
        public static Vector2 Normalize(Vector2 vec)
        {
            double scale = 1.0 / vec.Length;
            vec.x *= scale;
            vec.y *= scale;
            return vec;
        }

        /// <summary>
        /// Scale a vector to unit length
        /// </summary>
        /// <param name="vec">The input vector</param>
        /// <param name="result">The normalized vector</param>
        public static void Normalize(ref Vector2 vec, out Vector2 result)
        {
            double scale = 1.0 / vec.Length;
            result.x = vec.x * scale;
            result.y = vec.y * scale;
        }



        /// <summary>
        /// Calculate the dot (scalar) product of two vectors
        /// </summary>
        /// <param name="left">First operand</param>
        /// <param name="right">Second operand</param>
        /// <returns>The dot product of the two inputs</returns>
        public static double Dot(Vector2 left, Vector2 right)
        {
            return left.x * right.x + left.y * right.y;
        }

        /// <summary>
        /// Calculate the dot (scalar) product of two vectors
        /// </summary>
        /// <param name="left">First operand</param>
        /// <param name="right">Second operand</param>
        /// <param name="result">The dot product of the two inputs</param>
        public static void Dot(ref Vector2 left, ref Vector2 right, out double result)
        {
            result = left.x * right.x + left.y * right.y;
        }



        /// <summary>
        /// Calculate the cross product of two vectors
        /// </summary>
        /// <param name="left">First operand</param>
        /// <param name="right">Second operand</param>
        /// <returns>The cross product of the two inputs</returns>
        public static double Cross(Vector2 left, Vector2 right)
        {
            return left.x * right.y - left.y * right.x;
        }


        //public static Vector2 Rotate(Vector2 toRotate, double radians)
        //{
        //    Vector2 temp;
        //    Rotate(ref toRotate, radians, out temp);
        //    return temp;
        //}

        //public static void Rotate(ref Vector2 input, double radians, out Vector2 output)
        //{
        //    double Cos, Sin;
        //    Cos = (double)System.Math.Cos(radians);
        //    Sin = (double)System.Math.Sin(radians);
        //    output.x = input.x * Cos - input.y * Sin;
        //    output.y = input.y * Cos + input.x * Sin;
        //}


        /// <summary>
        /// Returns a new Vector that is the linear blend of the 2 given Vectors
        /// </summary>
        /// <param name="a">First input vector</param>
        /// <param name="b">Second input vector</param>
        /// <param name="blend">The blend factor. a when blend=0, b when blend=1.</param>
        /// <returns>a when blend=0, b when blend=1, and a linear combination otherwise</returns>
        public static Vector2 Lerp(Vector2 a, Vector2 b, double blend)
        {
            a.x = blend * (b.x - a.x) + a.x;
            a.y = blend * (b.y - a.y) + a.y;
            return a;
        }

        /// <summary>
        /// Returns a new Vector that is the linear blend of the 2 given Vectors
        /// </summary>
        /// <param name="a">First input vector</param>
        /// <param name="b">Second input vector</param>
        /// <param name="blend">The blend factor. a when blend=0, b when blend=1.</param>
        /// <param name="result">a when blend=0, b when blend=1, and a linear combination otherwise</param>
        public static void Lerp(ref Vector2 a, ref Vector2 b, double blend, out Vector2 result)
        {
            result.x = blend * (b.x - a.x) + a.x;
            result.y = blend * (b.y - a.y) + a.y;
        }



        /// <summary>
        /// Interpolate 3 Vectors using Barycentric coordinates
        /// </summary>
        /// <param name="a">First input Vector</param>
        /// <param name="b">Second input Vector</param>
        /// <param name="c">Third input Vector</param>
        /// <param name="u">First Barycentric Coordinate</param>
        /// <param name="v">Second Barycentric Coordinate</param>
        /// <returns>a when u=v=0, b when u=1,v=0, c when u=0,v=1, and a linear combination of a,b,c otherwise</returns>
        public static Vector2 BaryCentric(Vector2 a, Vector2 b, Vector2 c, double u, double v)
        {
            return a + u * (b - a) + v * (c - a);
        }

        /// <summary>Interpolate 3 Vectors using Barycentric coordinates</summary>
        /// <param name="a">First input Vector.</param>
        /// <param name="b">Second input Vector.</param>
        /// <param name="c">Third input Vector.</param>
        /// <param name="u">First Barycentric Coordinate.</param>
        /// <param name="v">Second Barycentric Coordinate.</param>
        /// <param name="result">Output Vector. a when u=v=0, b when u=1,v=0, c when u=0,v=1, and a linear combination of a,b,c otherwise</param>
        public static void BaryCentric(ref Vector2 a, ref Vector2 b, ref Vector2 c, double u, double v, out Vector2 result)
        {
            result = a; // copy
            Vector2 temp = b; // copy
            Subtract(ref temp, ref a, out temp);
            Multiply(ref temp, u, out temp);
            Add(ref result, ref temp, out result);
            temp = c; // copy
            Subtract(ref temp, ref a, out temp);
            Multiply(ref temp, v, out temp);
            Add(ref result, ref temp, out result);
        }



        /// <summary>
        /// Transforms a vector by a quaternion rotation.
        /// </summary>
        /// <param name="vec">The vector to transform.</param>
        /// <param name="quat">The quaternion to rotate the vector by.</param>
        /// <returns>The result of the operation.</returns>
        public static Vector2 Transform(Vector2 vec, Quaternion quat)
        {
            Vector2 result;
            Transform(ref vec, ref quat, out result);
            return result;
        }

        /// <summary>
        /// Transforms a vector by a quaternion rotation.
        /// </summary>
        /// <param name="vec">The vector to transform.</param>
        /// <param name="quat">The quaternion to rotate the vector by.</param>
        /// <param name="result">The result of the operation.</param>
        public static void Transform(ref Vector2 vec, ref Quaternion quat, out Vector2 result)
        {
            Quaternion v = new Quaternion(vec.x, vec.y, 0, 0), i, t;
            Quaternion.Invert(ref quat, out i);
            Quaternion.Multiply(ref quat, ref v, out t);
            Quaternion.Multiply(ref t, ref i, out v);
            result = new Vector2(v.X, v.Y);
        }



        /// <summary>
        /// Calculate the component-wise minimum of two vectors
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>The component-wise minimum</returns>
        public static Vector2 ComponentMin(Vector2 a, Vector2 b)
        {
            a.x = a.x < b.x ? a.x : b.x;
            a.y = a.y < b.y ? a.y : b.y;
            return a;
        }

        /// <summary>
        /// Calculate the component-wise minimum of two vectors
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <param name="result">The component-wise minimum</param>
        public static void ComponentMin(ref Vector2 a, ref Vector2 b, out Vector2 result)
        {
            result.x = a.x < b.x ? a.x : b.x;
            result.y = a.y < b.y ? a.y : b.y;
        }



        /// <summary>
        /// Calculate the component-wise maximum of two vectors
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>The component-wise maximum</returns>
        public static Vector2 ComponentMax(Vector2 a, Vector2 b)
        {
            a.x = a.x > b.x ? a.x : b.x;
            a.y = a.y > b.y ? a.y : b.y;
            return a;
        }

        /// <summary>
        /// Calculate the component-wise maximum of two vectors
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <param name="result">The component-wise maximum</param>
        public static void ComponentMax(ref Vector2 a, ref Vector2 b, out Vector2 result)
        {
            result.x = a.x > b.x ? a.x : b.x;
            result.y = a.y > b.y ? a.y : b.y;
        }




        /// <summary>
        /// Adds two instances.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <returns>The result of the operation.</returns>
        public static Vector2 operator +(Vector2 left, Vector2 right)
        {
            left.x += right.x;
            left.y += right.y;
            return left;
        }

        /// <summary>
        /// Subtracts two instances.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <returns>The result of the operation.</returns>
        public static Vector2 operator -(Vector2 left, Vector2 right)
        {
            left.x -= right.x;
            left.y -= right.y;
            return left;
        }

        /// <summary>
        /// Negates an instance.
        /// </summary>
        /// <param name="vec">The instance.</param>
        /// <returns>The result of the operation.</returns>
        public static Vector2 operator -(Vector2 vec)
        {
            vec.x = -vec.x;
            vec.y = -vec.y;
            return vec;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="vec">The instance.</param>
        /// <param name="f">The scalar.</param>
        /// <returns>The result of the operation.</returns>
        public static Vector2 operator *(Vector2 vec, double f)
        {
            vec.x *= f;
            vec.y *= f;
            return vec;
        }

        /// <summary>
        /// Multiply an instance by a scalar.
        /// </summary>
        /// <param name="f">The scalar.</param>
        /// <param name="vec">The instance.</param>
        /// <returns>The result of the operation.</returns>
        public static Vector2 operator *(double f, Vector2 vec)
        {
            vec.x *= f;
            vec.y *= f;
            return vec;
        }

        /// <summary>
        /// Divides an instance by a scalar.
        /// </summary>
        /// <param name="vec">The instance.</param>
        /// <param name="f">The scalar.</param>
        /// <returns>The result of the operation.</returns>
        public static Vector2 operator /(Vector2 vec, double f)
        {
            double mult = 1.0 / f;
            vec.x *= mult;
            vec.y *= mult;
            return vec;
        }

        /// <summary>
        /// Divides a scaler by an instance components wise.
        /// </summary>
        /// <param name="vec">The scalar.</param>
        /// <param name="f">The instance.</param>
        /// <returns>The result of the operation.</returns>
        public static Vector2 operator /(double f, Vector2 vec)
        {
            vec.x = f / vec.x;
            vec.y = f / vec.y;
            return vec;
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <returns>True, if both instances are equal; false otherwise.</returns>
        public static bool operator ==(Vector2 left, Vector2 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for ienquality.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <returns>True, if the instances are not equal; false otherwise.</returns>
        public static bool operator !=(Vector2 left, Vector2 right)
        {
            return !left.Equals(right);
        }




        /// <summary>
        /// Returns a System.String that represents the current instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("({0}, {1})", x, y);
        }



        /// <summary>
        /// Returns the hashcode for this instance.
        /// </summary>
        /// <returns>A System.Int32 containing the unique hashcode for this instance.</returns>
        public override int GetHashCode()
        {
            return new { x, y }.GetHashCode();
        }



        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>True if the instances are equal; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Vector2))
                return false;
            return this.Equals((Vector2)obj);
        }





        /// <summary>Indicates whether the current vector is equal to another vector.</summary>
        /// <param name="other">A vector to compare with this vector.</param>
        /// <returns>true if the current vector is equal to the vector parameter; otherwise, false.</returns>
        public bool Equals(Vector2 other)
        {
            return
                x == other.x &&
                y == other.y;
        }

        /// <summary>Indicates whether the current vector is equal to another vector.</summary>
        /// <param name="other">A vector to compare with this vector.</param>
        /// <returns>true if the current vector is equal to the vector parameter; otherwise, false.</returns>
        public bool Equals(Vector2 other, double errorRange)
        {
            if ((x < other.x + errorRange && x > other.x - errorRange) &&
                (y < other.y + errorRange && y > other.y - errorRange))
            {
                return true;
            }

            return false;
        }
        public double X
        {
            get { return this.x; }
            set { this.x = value; }
        }
        public double Y
        {
            get { return this.y; }
            set { this.y = value; }
        }

    }


    public static class Vector2Ext
    {
        public static Vector2 Rotate(this Vector2 v, int degree)
        {
            double radian = degree * Math.PI / 180.0;
            double sin = Math.Sin(radian);
            double cos = Math.Cos(radian);
            double nx = v.X * cos - v.Y * sin;
            double ny = v.X * sin + v.Y * cos;

            return new Vector2((float)nx, (float)ny);
        }
        public static Vector2 NewLength(this Vector2 v, double newLength)
        {
            //radian
            double atan = Math.Atan2(v.Y, v.X);
            return new Vector2(
                      (float)(Math.Cos(atan) * newLength),
                      (float)(Math.Sin(atan) * newLength));
        }

        public static Vector2f Rotate(this Vector2f v, int degree)
        {
            double radian = degree * Math.PI / 180.0;
            double sin = Math.Sin(radian);
            double cos = Math.Cos(radian);
            double nx = v.X * cos - v.Y * sin;
            double ny = v.X * sin + v.Y * cos;

            return new Vector2f((float)nx, (float)ny);
        }
        public static Vector2f NewLength(this Vector2f v, double newLength)
        {
            //radian
            double atan = Math.Atan2(v.Y, v.X);
            return new Vector2f(
                      (float)(Math.Cos(atan) * newLength),
                      (float)(Math.Sin(atan) * newLength));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2f
    {
        static Vector2f zeroVector = new Vector2f(0f, 0f);
        static Vector2f unitVector = new Vector2f(1f, 1f);
        static Vector2f unitXVector = new Vector2f(1f, 0f);
        static Vector2f unitYVector = new Vector2f(0f, 1f);
        public float X;
        public float Y;
        public static readonly Vector2f Zero = new Vector2f(0, 0);
        public Vector2f(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
        public static Vector2f operator +(Vector2f v1, Vector2f v2)
        {
            return new Vector2f(v1.X + v2.X, v1.Y + v2.Y);
        }
        public static Vector2f operator +(Vector2f v1, float n)
        {
            return new Vector2f(v1.X + n, v1.Y + n);
        }

        public static Vector2f operator -(Vector2f v1, Vector2f v2)
        {
            return new Vector2f(v1.X - v2.X, v1.Y - v2.Y);
        }
        public static Vector2f operator /(Vector2f v1, float n)
        {
            return new Vector2f(v1.X / n, v1.Y / n);
        }
        public static Vector2f operator *(Vector2f v1, float n)
        {
            return new Vector2f(v1.X * n, v1.Y * n);
        }
        public static Vector2f operator *(float n, Vector2f v1)
        {
            return new Vector2f(v1.X * n, v1.Y * n);
        }
        public static bool operator ==(Vector2f v1, Vector2f v2)
        {
            return (v1.X == v2.X) && (v1.Y == v2.Y);
        }
        public static bool operator !=(Vector2f v1, Vector2f v2)
        {
            return (v1.X != v2.X) || (v1.Y != v2.Y);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static Vector2f Min(Vector2f value1, Vector2f value2)
        {
            return new Vector2f(
                (value1.X < value2.X) ? value1.X : value2.X,
                (value1.Y < value2.Y) ? value1.Y : value2.Y
                );
        }
        public static Vector2f Max(Vector2f value1, Vector2f value2)
        {
            return new Vector2f(
                (value1.X > value2.X) ? value1.X : value2.X,
                (value1.Y > value2.Y) ? value1.Y : value2.Y
                );
        }
        public override bool Equals(object obj)
        {
            if (obj is Vector2f)
            {
                Vector2f v2 = (Vector2f)obj;
                return this == v2;
            }
            else
            {
                return false;
            }
        }
        public static Vector2f Normalize(Vector2f value)
        {
            double factor = 1.0f / (double)Math.Sqrt((value.X * value.X) + (value.Y * value.Y));
            return new Vector2f((float)(value.X * factor), (float)(value.Y * factor));
            //value.X *= val;
            //value.Y *= val;
            //return value;
        }


        public static Vector2f One
        {
            get { return unitVector; }
        }

        public static Vector2f UnitX
        {
            get { return unitXVector; }
        }

        public static Vector2f UnitY
        {
            get { return unitYVector; }
        }
        public static double Dot(Vector2f value1, Vector2f value2)
        {
            return (value1.X * value2.X) + (value1.Y * value2.Y);
        }
        public double Length()
        {
            return Math.Sqrt((X * X) + (Y * Y));
        }

        public double LengthSquared()
        {
            return (X * X) + (Y * Y);
        }
#if DEBUG
        public override string ToString()
        {
            return "(" + X + "," + Y + ")";
        }
#endif


    }

    public struct Matrix3x2f
    {
        /// <summary>
        /// Gets the identity matrix.
        /// </summary>
        /// <value>The identity matrix.</value>
        public readonly static Matrix3x2f Identity = new Matrix3x2f(1, 0, 0, 1, 0, 0);
        /// <summary>
        /// Element (1,1)
        /// </summary>
        public float M11;

        /// <summary>
        /// Element (1,2)
        /// </summary>
        public float M12;

        /// <summary>
        /// Element (2,1)
        /// </summary>
        public float M21;

        /// <summary>
        /// Element (2,2)
        /// </summary>
        public float M22;

        /// <summary>
        /// Element (3,1)
        /// </summary>
        public float M31;

        /// <summary>
        /// Element (3,2)
        /// </summary>
        public float M32;

        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix3x2"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public Matrix3x2f(float value)
        {
            M11 = M12 =
            M21 = M22 =
            M31 = M32 = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix3x2"/> struct.
        /// </summary>
        /// <param name="M11">The value to assign at row 1 column 1 of the matrix.</param>
        /// <param name="M12">The value to assign at row 1 column 2 of the matrix.</param>
        /// <param name="M21">The value to assign at row 2 column 1 of the matrix.</param>
        /// <param name="M22">The value to assign at row 2 column 2 of the matrix.</param>
        /// <param name="M31">The value to assign at row 3 column 1 of the matrix.</param>
        /// <param name="M32">The value to assign at row 3 column 2 of the matrix.</param>
        public Matrix3x2f(float M11, float M12, float M21, float M22, float M31, float M32)
        {
            this.M11 = M11; this.M12 = M12;
            this.M21 = M21; this.M22 = M22;
            this.M31 = M31; this.M32 = M32;
        }
    }

}