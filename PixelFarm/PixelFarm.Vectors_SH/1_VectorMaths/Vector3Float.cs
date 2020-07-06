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
    /// <summary>
    /// Represents a 3D vector using three float-precision floating-point numbers.
    /// </summary>
    //[Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3Float : IEquatable<Vector3Float>
    {

        /// <summary>
        /// The X component of the Vector3Float.
        /// </summary>
        public float x;
        /// <summary>
        /// The Y component of the Vector3Float.
        /// </summary>
        public float y;
        /// <summary>
        /// The Z component of the Vector3Float.
        /// </summary>
        public float z;


        /// <summary>
        /// Constructs a new Vector3Float.
        /// </summary>
        /// <param name="x">The x component of the Vector3Float.</param>
        /// <param name="y">The y component of the Vector3Float.</param>
        /// <param name="z">The z component of the Vector3Float.</param>
        public Vector3Float(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Constructs a new instance from the given Vector2d.
        /// </summary>
        /// <param name="v">The Vector2d to copy components from.</param>
        public Vector3Float(Vector2 v, float z = 0)
        {
#if true
            throw new NotImplementedException();
#else
            x = v.x;
            y = v.y;
            this.z = z;
#endif
        }

        /// <summary>
        /// Constructs a new instance from the given Vector3Floatd.
        /// </summary>
        /// <param name="v">The Vector3Floatd to copy components from.</param>
        public Vector3Float(Vector3Float v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vector3Float(float[] floatArray)
        {
            x = floatArray[0];
            y = floatArray[1];
            z = floatArray[2];
        }

        /// <summary>
        /// Constructs a new instance from the given Vector4d.
        /// </summary>
        /// <param name="v">The Vector4d to copy components from.</param>
        public Vector3Float(Vector4 v)
        {
#if true
            throw new NotImplementedException();
#else
            x = v.x;
            y = v.y;
            z = v.z;
#endif
        }

        public Vector3Float(Vector3 position)
        {
            this.x = (float)position.x;
            this.y = (float)position.y;
            this.z = (float)position.z;
        }



        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
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
                    case 2:
                        z = value;
                        break;
                    default:
                        throw new Exception();
                }
            }
        }





        /// <summary>
        /// Gets the length (magnitude) of the vector.
        /// </summary>
        /// <see cref="LengthFast"/>
        /// <seealso cref="LengthSquared"/>
        public float Length
        {
            get
            {
                return (float)Math.Sqrt(x * x + y * y + z * z);
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
        /// <seealso cref="LengthFast"/>
        public float LengthSquared
        {
            get
            {
                return x * x + y * y + z * z;
            }
        }



        /// <summary>
        /// Returns a normalized Vector of this.
        /// </summary>
        /// <returns></returns>
        public Vector3Float GetNormal()
        {
            Vector3Float temp = this;
            temp.Normalize();
            return temp;
        }

        /// <summary>
        /// Scales the Vector3Floatd to unit length.
        /// </summary>
        public void Normalize()
        {
            float scale = 1.0f / this.Length;
            x *= scale;
            y *= scale;
            z *= scale;
        }



        public float[] ToArray()
        {
            return new float[] { x, y, z };
        }





        /// <summary>
        /// Defines a unit-length Vector3Floatd that points towards the X-axis.
        /// </summary>
        public static readonly Vector3Float UnitX = new Vector3Float(1, 0, 0);
        /// <summary>
        /// Defines a unit-length Vector3Floatd that points towards the Y-axis.
        /// </summary>
        public static readonly Vector3Float UnitY = new Vector3Float(0, 1, 0);
        /// <summary>
        /// /// Defines a unit-length Vector3Floatd that points towards the Z-axis.
        /// </summary>
        public static readonly Vector3Float UnitZ = new Vector3Float(0, 0, 1);
        /// <summary>
        /// Defines a zero-length Vector3Float.
        /// </summary>
        public static readonly Vector3Float Zero = new Vector3Float(0, 0, 0);
        /// <summary>
        /// Defines an instance with all components set to 1.
        /// </summary>
        public static readonly Vector3Float One = new Vector3Float(1, 1, 1);
        /// <summary>
        /// Defines an instance with all components set to positive infinity.
        /// </summary>
        public static readonly Vector3Float PositiveInfinity = new Vector3Float(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        /// <summary>
        /// Defines an instance with all components set to negative infinity.
        /// </summary>
        public static readonly Vector3Float NegativeInfinity = new Vector3Float(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        /// <summary>
        /// Defines the size of the Vector3Floatd struct in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf(new Vector3Float());


        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="a">Left operand.</param>
        /// <param name="b">Right operand.</param>
        /// <returns>Result of operation.</returns>
        public static Vector3Float Add(Vector3Float a, Vector3Float b)
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
        public static void Add(ref Vector3Float a, ref Vector3Float b, out Vector3Float result)
        {
            result = new Vector3Float(a.x + b.x, a.y + b.y, a.z + b.z);
        }



        /// <summary>
        /// Subtract one Vector from another
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>Result of subtraction</returns>
        public static Vector3Float Subtract(Vector3Float a, Vector3Float b)
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
        public static void Subtract(ref Vector3Float a, ref Vector3Float b, out Vector3Float result)
        {
            result = new Vector3Float(a.x - b.x, a.y - b.y, a.z - b.z);
        }



        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="vector">Left operand.</param>
        /// <param name="scale">Right operand.</param>
        /// <returns>Result of the operation.</returns>
        public static Vector3Float Multiply(Vector3Float vector, float scale)
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
        public static void Multiply(ref Vector3Float vector, float scale, out Vector3Float result)
        {
            result = new Vector3Float(vector.x * scale, vector.y * scale, vector.z * scale);
        }

        /// <summary>
        /// Multiplies a vector by the components a vector (scale).
        /// </summary>
        /// <param name="vector">Left operand.</param>
        /// <param name="scale">Right operand.</param>
        /// <returns>Result of the operation.</returns>
        public static Vector3Float Multiply(Vector3Float vector, Vector3Float scale)
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
        public static void Multiply(ref Vector3Float vector, ref Vector3Float scale, out Vector3Float result)
        {
            result = new Vector3Float(vector.x * scale.x, vector.y * scale.y, vector.z * scale.z);
        }



        /// <summary>
        /// Divides a vector by a scalar.
        /// </summary>
        /// <param name="vector">Left operand.</param>
        /// <param name="scale">Right operand.</param>
        /// <returns>Result of the operation.</returns>
        public static Vector3Float Divide(Vector3Float vector, float scale)
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
        public static void Divide(ref Vector3Float vector, float scale, out Vector3Float result)
        {
            Multiply(ref vector, 1 / scale, out result);
        }

        /// <summary>
        /// Divides a vector by the components of a vector (scale).
        /// </summary>
        /// <param name="vector">Left operand.</param>
        /// <param name="scale">Right operand.</param>
        /// <returns>Result of the operation.</returns>
        public static Vector3Float Divide(Vector3Float vector, Vector3Float scale)
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
        public static void Divide(ref Vector3Float vector, ref Vector3Float scale, out Vector3Float result)
        {
            result = new Vector3Float(vector.x / scale.x, vector.y / scale.y, vector.z / scale.z);
        }



        /// <summary>
        /// Calculate the component-wise minimum of two vectors
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>The component-wise minimum</returns>
        public static Vector3Float ComponentMin(Vector3Float a, Vector3Float b)
        {
            a.x = a.x < b.x ? a.x : b.x;
            a.y = a.y < b.y ? a.y : b.y;
            a.z = a.z < b.z ? a.z : b.z;
            return a;
        }

        /// <summary>
        /// Calculate the component-wise minimum of two vectors
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <param name="result">The component-wise minimum</param>
        public static void ComponentMin(ref Vector3Float a, ref Vector3Float b, out Vector3Float result)
        {
            result.x = a.x < b.x ? a.x : b.x;
            result.y = a.y < b.y ? a.y : b.y;
            result.z = a.z < b.z ? a.z : b.z;
        }



        /// <summary>
        /// Calculate the component-wise maximum of two vectors
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>The component-wise maximum</returns>
        public static Vector3Float ComponentMax(Vector3Float a, Vector3Float b)
        {
            a.x = a.x > b.x ? a.x : b.x;
            a.y = a.y > b.y ? a.y : b.y;
            a.z = a.z > b.z ? a.z : b.z;
            return a;
        }

        /// <summary>
        /// Calculate the component-wise maximum of two vectors
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <param name="result">The component-wise maximum</param>
        public static void ComponentMax(ref Vector3Float a, ref Vector3Float b, out Vector3Float result)
        {
            result.x = a.x > b.x ? a.x : b.x;
            result.y = a.y > b.y ? a.y : b.y;
            result.z = a.z > b.z ? a.z : b.z;
        }



        /// <summary>
        /// Returns the Vector3d with the minimum magnitude
        /// </summary>
        /// <param name="left">Left operand</param>
        /// <param name="right">Right operand</param>
        /// <returns>The minimum Vector3Float</returns>
        public static Vector3Float Min(Vector3Float left, Vector3Float right)
        {
            return left.LengthSquared < right.LengthSquared ? left : right;
        }



        /// <summary>
        /// Returns the Vector3d with the minimum magnitude
        /// </summary>
        /// <param name="left">Left operand</param>
        /// <param name="right">Right operand</param>
        /// <returns>The minimum Vector3Float</returns>
        public static Vector3Float Max(Vector3Float left, Vector3Float right)
        {
            return left.LengthSquared >= right.LengthSquared ? left : right;
        }



        /// <summary>
        /// Clamp a vector to the given minimum and maximum vectors
        /// </summary>
        /// <param name="vec">Input vector</param>
        /// <param name="min">Minimum vector</param>
        /// <param name="max">Maximum vector</param>
        /// <returns>The clamped vector</returns>
        public static Vector3Float Clamp(Vector3Float vec, Vector3Float min, Vector3Float max)
        {
            vec.x = vec.x < min.x ? min.x : vec.x > max.x ? max.x : vec.x;
            vec.y = vec.y < min.y ? min.y : vec.y > max.y ? max.y : vec.y;
            vec.z = vec.z < min.z ? min.z : vec.z > max.z ? max.z : vec.z;
            return vec;
        }

        /// <summary>
        /// Clamp a vector to the given minimum and maximum vectors
        /// </summary>
        /// <param name="vec">Input vector</param>
        /// <param name="min">Minimum vector</param>
        /// <param name="max">Maximum vector</param>
        /// <param name="result">The clamped vector</param>
        public static void Clamp(ref Vector3Float vec, ref Vector3Float min, ref Vector3Float max, out Vector3Float result)
        {
            result.x = vec.x < min.x ? min.x : vec.x > max.x ? max.x : vec.x;
            result.y = vec.y < min.y ? min.y : vec.y > max.y ? max.y : vec.y;
            result.z = vec.z < min.z ? min.z : vec.z > max.z ? max.z : vec.z;
        }



        /// <summary>
        /// Scale a vector to unit length
        /// </summary>
        /// <param name="vec">The input vector</param>
        /// <returns>The normalized vector</returns>
        public static Vector3Float Normalize(Vector3Float vec)
        {
            float scale = 1.0f / vec.Length;
            vec.x *= scale;
            vec.y *= scale;
            vec.z *= scale;
            return vec;
        }

        /// <summary>
        /// Scale a vector to unit length
        /// </summary>
        /// <param name="vec">The input vector</param>
        /// <param name="result">The normalized vector</param>
        public static void Normalize(ref Vector3Float vec, out Vector3Float result)
        {
            float scale = 1.0f / vec.Length;
            result.x = vec.x * scale;
            result.y = vec.y * scale;
            result.z = vec.z * scale;
        }



        /// <summary>
        /// Calculate the dot (scalar) product of two vectors
        /// </summary>
        /// <param name="left">First operand</param>
        /// <param name="right">Second operand</param>
        /// <returns>The dot product of the two inputs</returns>
        public static float Dot(Vector3Float left, Vector3Float right)
        {
            return left.x * right.x + left.y * right.y + left.z * right.z;
        }

        /// <summary>
        /// Calculate the dot (scalar) product of two vectors
        /// </summary>
        /// <param name="left">First operand</param>
        /// <param name="right">Second operand</param>
        /// <param name="result">The dot product of the two inputs</param>
        public static void Dot(ref Vector3Float left, ref Vector3Float right, out float result)
        {
            result = left.x * right.x + left.y * right.y + left.z * right.z;
        }



        /// <summary>
        /// Caclulate the cross (vector) product of two vectors
        /// </summary>
        /// <param name="left">First operand</param>
        /// <param name="right">Second operand</param>
        /// <returns>The cross product of the two inputs</returns>
        public static Vector3Float Cross(Vector3Float left, Vector3Float right)
        {
            Vector3Float result;
            Cross(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Caclulate the cross (vector) product of two vectors
        /// </summary>
        /// <param name="left">First operand</param>
        /// <param name="right">Second operand</param>
        /// <returns>The cross product of the two inputs</returns>
        /// <param name="result">The cross product of the two inputs</param>
        public static void Cross(ref Vector3Float left, ref Vector3Float right, out Vector3Float result)
        {
            result = new Vector3Float(left.y * right.z - left.z * right.y,
                left.z * right.x - left.x * right.z,
                left.x * right.y - left.y * right.x);
        }


        /// <summary>
        /// Checks if 3 points are collinear (all lie on the same line).
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static bool Collinear(Vector3Float a, Vector3Float b, Vector3Float c, float epsilon = .000001f)
        {
            // Return true if a, b, and c all lie on the same line.
            return Math.Abs(Cross(b - a, c - a).Length) < epsilon;
        }



        /// <summary>
        /// Returns a new Vector that is the linear blend of the 2 given Vectors
        /// </summary>
        /// <param name="a">First input vector</param>
        /// <param name="b">Second input vector</param>
        /// <param name="blend">The blend factor. a when blend=0, b when blend=1.</param>
        /// <returns>a when blend=0, b when blend=1, and a linear combination otherwise</returns>
        public static Vector3Float Lerp(Vector3Float a, Vector3Float b, float blend)
        {
            a.x = blend * (b.x - a.x) + a.x;
            a.y = blend * (b.y - a.y) + a.y;
            a.z = blend * (b.z - a.z) + a.z;
            return a;
        }

        /// <summary>
        /// Returns a new Vector that is the linear blend of the 2 given Vectors
        /// </summary>
        /// <param name="a">First input vector</param>
        /// <param name="b">Second input vector</param>
        /// <param name="blend">The blend factor. a when blend=0, b when blend=1.</param>
        /// <param name="result">a when blend=0, b when blend=1, and a linear combination otherwise</param>
        public static void Lerp(ref Vector3Float a, ref Vector3Float b, float blend, out Vector3Float result)
        {
            result.x = blend * (b.x - a.x) + a.x;
            result.y = blend * (b.y - a.y) + a.y;
            result.z = blend * (b.z - a.z) + a.z;
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
        public static Vector3Float BaryCentric(Vector3Float a, Vector3Float b, Vector3Float c, float u, float v)
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
        public static void BaryCentric(ref Vector3Float a, ref Vector3Float b, ref Vector3Float c, float u, float v, out Vector3Float result)
        {
            result = a; // copy
            Vector3Float temp = b; // copy
            Subtract(ref temp, ref a, out temp);
            Multiply(ref temp, u, out temp);
            Add(ref result, ref temp, out result);
            temp = c; // copy
            Subtract(ref temp, ref a, out temp);
            Multiply(ref temp, v, out temp);
            Add(ref result, ref temp, out result);
        }



        /// <summary>Transform a direction vector by the given Matrix
        /// Assumes the matrix has a bottom row of (0,0,0,1), that is the translation part is ignored.
        /// </summary>
        /// <param name="vec">The vector to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <returns>The transformed vector</returns>
        public static Vector3Float TransformVector(Vector3Float vec, Matrix4X4 mat)
        {
            return new Vector3Float(
                Vector3Float.Dot(vec, new Vector3Float(mat.Column0)),
                Vector3Float.Dot(vec, new Vector3Float(mat.Column1)),
                Vector3Float.Dot(vec, new Vector3Float(mat.Column2)));
        }

        /// <summary>Transform a direction vector by the given Matrix
        /// Assumes the matrix has a bottom row of (0,0,0,1), that is the translation part is ignored.
        /// </summary>
        /// <param name="vec">The vector to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <param name="result">The transformed vector</param>
        public static void TransformVector(ref Vector3Float vec, ref Matrix4X4 mat, out Vector3Float result)
        {
#if true
            throw new NotImplementedException();
#else
            result.x = vec.x * mat.Row0.x +
                       vec.y * mat.Row1.x +
                       vec.z * mat.Row2.x;

            result.y = vec.x * mat.Row0.y +
                       vec.y * mat.Row1.y +
                       vec.z * mat.Row2.y;

            result.z = vec.x * mat.Row0.z +
                       vec.y * mat.Row1.z +
                       vec.z * mat.Row2.z;
#endif
        }

        /// <summary>Transform a Normal by the given Matrix</summary>
        /// <remarks>
        /// This calculates the inverse of the given matrix, use TransformNormalInverse if you
        /// already have the inverse to avoid this extra calculation
        /// </remarks>
        /// <param name="norm">The normal to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <returns>The transformed normal</returns>
        public static Vector3Float TransformNormal(Vector3Float norm, Matrix4X4 mat)
        {
            mat.Invert();
            return TransformNormalInverse(norm, mat);
        }

        /// <summary>Transform a Normal by the given Matrix</summary>
        /// <remarks>
        /// This calculates the inverse of the given matrix, use TransformNormalInverse if you
        /// already have the inverse to avoid this extra calculation
        /// </remarks>
        /// <param name="norm">The normal to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <param name="result">The transformed normal</param>
        public static void TransformNormal(ref Vector3Float norm, ref Matrix4X4 mat, out Vector3Float result)
        {
            Matrix4X4 Inverse = Matrix4X4.Invert(mat);
            Vector3Float.TransformNormalInverse(ref norm, ref Inverse, out result);
        }

        /// <summary>Transform a Normal by the (transpose of the) given Matrix</summary>
        /// <remarks>
        /// This version doesn't calculate the inverse matrix.
        /// Use this version if you already have the inverse of the desired transform to hand
        /// </remarks>
        /// <param name="norm">The normal to transform</param>
        /// <param name="invMat">The inverse of the desired transformation</param>
        /// <returns>The transformed normal</returns>
        public static Vector3Float TransformNormalInverse(Vector3Float norm, Matrix4X4 invMat)
        {
            return new Vector3Float(
                Vector3Float.Dot(norm, new Vector3Float(invMat.Row0)),
                Vector3Float.Dot(norm, new Vector3Float(invMat.Row1)),
                Vector3Float.Dot(norm, new Vector3Float(invMat.Row2)));
        }

        /// <summary>Transform a Normal by the (transpose of the) given Matrix</summary>
        /// <remarks>
        /// This version doesn't calculate the inverse matrix.
        /// Use this version if you already have the inverse of the desired transform to hand
        /// </remarks>
        /// <param name="norm">The normal to transform</param>
        /// <param name="invMat">The inverse of the desired transformation</param>
        /// <param name="result">The transformed normal</param>
        public static void TransformNormalInverse(ref Vector3Float norm, ref Matrix4X4 invMat, out Vector3Float result)
        {
#if true
            throw new NotImplementedException();
#else
            result.x = norm.x * invMat.Row0.x +
                       norm.y * invMat.Row0.y +
                       norm.z * invMat.Row0.z;

            result.y = norm.x * invMat.Row1.x +
                       norm.y * invMat.Row1.y +
                       norm.z * invMat.Row1.z;

            result.z = norm.x * invMat.Row2.x +
                       norm.y * invMat.Row2.y +
                       norm.z * invMat.Row2.z;
#endif
        }

        /// <summary>Transform a Position by the given Matrix</summary>
        /// <param name="pos">The position to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <returns>The transformed position</returns>
        public static Vector3Float TransformPosition(Vector3Float pos, Matrix4X4 mat)
        {
#if true
            throw new NotImplementedException();
#else
            return new Vector3Float(
                Vector3Float.Dot(pos, new Vector3Float((float)mat.Column0)) + mat.Row3.x,
                Vector3Float.Dot(pos, new Vector3Float((float)mat.Column1)) + mat.Row3.y,
                Vector3Float.Dot(pos, new Vector3Float((float)mat.Column2)) + mat.Row3.z);
#endif
        }

        /// <summary>Transform a Position by the given Matrix</summary>
        /// <param name="pos">The position to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <param name="result">The transformed position</param>
        public static void TransformPosition(ref Vector3Float pos, ref Matrix4X4 mat, out Vector3Float result)
        {
#if true
            throw new NotImplementedException();
#else
            result.x = pos.x * mat.Row0.x +
                       pos.y * mat.Row1.x +
                       pos.z * mat.Row2.x +
                       mat.Row3.x;

            result.y = pos.x * mat.Row0.y +
                       pos.y * mat.Row1.y +
                       pos.z * mat.Row2.y +
                       mat.Row3.y;

            result.z = pos.x * mat.Row0.z +
                       pos.y * mat.Row1.z +
                       pos.z * mat.Row2.z +
                       mat.Row3.z;
#endif
        }

        /// <summary>
        /// Transform all the vectors in the array by the given Matrix.
        /// </summary>
        /// <param name="boundsVerts"></param>
        /// <param name="rotationQuaternion"></param>
        public static void Transform(Vector3Float[] vecArray, Matrix4X4 mat)
        {
            for (int i = 0; i < vecArray.Length; i++)
            {
                vecArray[i] = Transform(vecArray[i], mat);
            }
        }

        /// <summary>Transform a Vector by the given Matrix</summary>
        /// <param name="vec">The vector to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <returns>The transformed vector</returns>
        public static Vector3Float Transform(Vector3Float vec, Matrix4X4 mat)
        {
            Vector3Float result;
            Transform(ref vec, ref mat, out result);
            return result;
        }

        /// <summary>Transform a Vector by the given Matrix</summary>
        /// <param name="vec">The vector to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <param name="result">The transformed vector</param>
        public static void Transform(ref Vector3Float vec, ref Matrix4X4 mat, out Vector3Float result)
        {
#if true
            throw new NotImplementedException();
#else
            Vector4 v4 = new Vector4(vec.x, vec.y, vec.z, 1.0);
            Vector4.Transform(ref v4, ref mat, out v4);
            result = v4.Xyz;
#endif
        }

        /// <summary>
        /// Transforms a vector by a quaternion rotation.
        /// </summary>
        /// <param name="vec">The vector to transform.</param>
        /// <param name="quat">The quaternion to rotate the vector by.</param>
        /// <returns>The result of the operation.</returns>
        public static Vector3Float Transform(Vector3Float vec, Quaternion quat)
        {
#if true
            throw new NotImplementedException();
#else
            Vector3Float result;
            Transform(ref vec, ref quat, out result);
            return result;
#endif
        }

        /// <summary>
        /// Transforms a vector by a quaternion rotation.
        /// </summary>
        /// <param name="vec">The vector to transform.</param>
        /// <param name="quat">The quaternion to rotate the vector by.</param>
        /// <param name="result">The result of the operation.</param>
        public static void Transform(ref Vector3Float vec, ref Quaternion quat, out Vector3Float result)
        {
#if true
            throw new NotImplementedException();
#else
            // Since vec.W == 0, we can optimize quat * vec * quat^-1 as follows:
            // vec + 2.0 * cross(quat.xyz, cross(quat.xyz, vec) + quat.w * vec)
            Vector3Float xyz = quat.Xyz, temp, temp2;
            Vector3Float.Cross(ref xyz, ref vec, out temp);
            Vector3Float.Multiply(ref vec, quat.W, out temp2);
            Vector3Float.Add(ref temp, ref temp2, out temp);
            Vector3Float.Cross(ref xyz, ref temp, out temp);
            Vector3Float.Multiply(ref temp, 2, out temp);
            Vector3Float.Add(ref vec, ref temp, out result);
#endif
        }

        /// <summary>
        /// Transform all the vectors in the array by the quaternion rotation.
        /// </summary>
        /// <param name="boundsVerts"></param>
        /// <param name="rotationQuaternion"></param>
        public static void Transform(Vector3Float[] vecArray, Quaternion rotationQuaternion)
        {
            for (int i = 0; i < vecArray.Length; i++)
            {
                vecArray[i] = Transform(vecArray[i], rotationQuaternion);
            }
        }

        /// <summary>
        /// Transform a Vector3d by the given Matrix, and project the resulting Vector4 back to a Vector3Float
        /// </summary>
        /// <param name="vec">The vector to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <returns>The transformed vector</returns>
        public static Vector3Float TransformPerspective(Vector3Float vec, Matrix4X4 mat)
        {
#if true
            throw new NotImplementedException();
#else
            Vector3Float result;
            TransformPerspective(ref vec, ref mat, out result);
            return result;
#endif
        }

        /// <summary>Transform a Vector3d by the given Matrix, and project the resulting Vector4d back to a Vector3d</summary>
        /// <param name="vec">The vector to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <param name="result">The transformed vector</param>
        public static void TransformPerspective(ref Vector3Float vec, ref Matrix4X4 mat, out Vector3Float result)
        {
#if true
            throw new NotImplementedException();
#else
            Vector4 v = new Vector4(vec);
            Vector4.Transform(ref v, ref mat, out v);
            result.x = v.x / v.w;
            result.y = v.y / v.w;
            result.z = v.z / v.w;
#endif
        }



        /// <summary>
        /// Calculates the angle (in radians) between two vectors.
        /// </summary>
        /// <param name="first">The first vector.</param>
        /// <param name="second">The second vector.</param>
        /// <returns>Angle (in radians) between the vectors.</returns>
        /// <remarks>Note that the returned angle is never bigger than the constant Pi.</remarks>
        public static float CalculateAngle(Vector3Float first, Vector3Float second)
        {
            return (float)Math.Acos((Vector3Float.Dot(first, second)) / (first.Length * second.Length));
        }

        /// <summary>Calculates the angle (in radians) between two vectors.</summary>
        /// <param name="first">The first vector.</param>
        /// <param name="second">The second vector.</param>
        /// <param name="result">Angle (in radians) between the vectors.</param>
        /// <remarks>Note that the returned angle is never bigger than the constant Pi.</remarks>
        public static void CalculateAngle(ref Vector3Float first, ref Vector3Float second, out float result)
        {
            float temp;
            Vector3Float.Dot(ref first, ref second, out temp);
            result = (float)Math.Acos(temp / (first.Length * second.Length));
        }




        /// <summary>
        /// Gets or sets an OpenTK.Vector2d with the X and Y components of this instance.
        /// </summary>
        public Vector2 Xy
        {
            get
            {
                return new Vector2(x, y);
            }
            set
            {
#if true
                throw new NotImplementedException();
#else
                x = value.x; y = value.y; 
#endif
            }
        }



        /// <summary>
        /// Adds two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static Vector3Float operator +(Vector3Float left, Vector3Float right)
        {
            left.x += right.x;
            left.y += right.y;
            left.z += right.z;
            return left;
        }

        /// <summary>
        /// Subtracts two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static Vector3Float operator -(Vector3Float left, Vector3Float right)
        {
            left.x -= right.x;
            left.y -= right.y;
            left.z -= right.z;
            return left;
        }

        /// <summary>
        /// Negates an instance.
        /// </summary>
        /// <param name="vec">The instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static Vector3Float operator -(Vector3Float vec)
        {
            vec.x = -vec.x;
            vec.y = -vec.y;
            vec.z = -vec.z;
            return vec;
        }

        /// <summary>
        /// Component wise multiply two vectors together, x*x, y*y, z*z.
        /// </summary>
        /// <param name="vecA"></param>
        /// <param name="vecB"></param>
        /// <returns></returns>
        public static Vector3Float operator *(Vector3Float vecA, Vector3Float vecB)
        {
            vecA.x *= vecB.x;
            vecA.y *= vecB.y;
            vecA.z *= vecB.z;
            return vecA;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="vec">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>The result of the calculation.</returns>
        public static Vector3Float operator *(Vector3Float vec, float scale)
        {
            vec.x *= scale;
            vec.y *= scale;
            vec.z *= scale;
            return vec;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="scale">The scalar.</param>
        /// <param name="vec">The instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static Vector3Float operator *(float scale, Vector3Float vec)
        {
            vec.x *= scale;
            vec.y *= scale;
            vec.z *= scale;
            return vec;
        }

        /// <summary>
        /// Creates a new vector which is the numerator devided by each component of the vector.
        /// </summary>
        /// <param name="numerator"></param>
        /// <param name="vec"></param>
        /// <returns>The result of the calculation.</returns>
        public static Vector3Float operator /(float numerator, Vector3Float vec)
        {
            return new Vector3Float((numerator / vec.x), (numerator / vec.y), (numerator / vec.z));
        }

        /// <summary>
        /// Divides an instance by a scalar.
        /// </summary>
        /// <param name="vec">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>The result of the calculation.</returns>
        public static Vector3Float operator /(Vector3Float vec, float scale)
        {
            float mult = 1 / scale;
            vec.x *= mult;
            vec.y *= mult;
            vec.z *= mult;
            return vec;
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left equals right; false otherwise.</returns>
        public static bool operator ==(Vector3Float left, Vector3Float right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left does not equa lright; false otherwise.</returns>
        public static bool operator !=(Vector3Float left, Vector3Float right)
        {
            return !left.Equals(right);
        }




        /// <summary>
        /// Returns a System.String that represents the current Vector3Float.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}]", x, y, z);
        }



        /// <summary>
        /// Returns the hashcode for this instance.
        /// </summary>
        /// <returns>A System.Int32 containing the unique hashcode for this instance.</returns>
        public override int GetHashCode()
        {
            return new { x, y, z }.GetHashCode();
        }



        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>True if the instances are equal; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Vector3Float))
                return false;
            return this.Equals((Vector3Float)obj);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal within an error range.
        /// </summary>
        /// <param name="OtherVector"></param>
        /// <param name="ErrorValue"></param>
        /// <returns>True if the instances are equal; false otherwise.</returns>
        public bool Equals(Vector3Float OtherVector, float ErrorValue)
        {
            if ((x < OtherVector.x + ErrorValue && x > OtherVector.x - ErrorValue) &&
                (y < OtherVector.y + ErrorValue && y > OtherVector.y - ErrorValue) &&
                (z < OtherVector.z + ErrorValue && z > OtherVector.z - ErrorValue))
            {
                return true;
            }

            return false;
        }





        /// <summary>Indicates whether the current vector is equal to another vector.</summary>
        /// <param name="other">A vector to compare with this vector.</param>
        /// <returns>true if the current vector is equal to the vector parameter; otherwise, false.</returns>
        public bool Equals(Vector3Float other)
        {
            return
                x == other.x &&
                y == other.y &&
                z == other.z;
        }


        public static float ComponentMax(Vector3Float vector3)
        {
            return Math.Max(vector3.x, Math.Max(vector3.y, vector3.z));
        }

        public static float ComponentMin(Vector3Float vector3)
        {
            return Math.Min(vector3.x, Math.Min(vector3.y, vector3.z));
        }
    }
}