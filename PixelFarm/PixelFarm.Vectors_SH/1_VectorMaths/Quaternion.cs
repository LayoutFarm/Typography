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
    /// Represents a double-precision Quaternion.
    /// </summary>
    //[Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Quaternion : IEquatable<Quaternion>
    {

        Vector3 xyz;
        double w;


        /// <summary>
        /// Construct a new Quaterniond from vector and w components
        /// </summary>
        /// <param name="v">The vector part</param>
        /// <param name="w">The w part</param>
        public Quaternion(Vector3 v, double w)
        {
            this.xyz = v;
            this.w = w;
        }

        /// <summary>
        /// Construct a new Quaterniond
        /// </summary>
        /// <param name="x">The x component</param>
        /// <param name="y">The y component</param>
        /// <param name="z">The z component</param>
        /// <param name="w">The w component</param>
        public Quaternion(double x, double y, double z, double w)
            : this(new Vector3(x, y, z), w)
        { }

        /// <summary>
        /// Construct a quaternion that rotates from one direction to another
        /// </summary>
        /// <param name="startingDirection"></param>
        /// <param name="endingDirection"></param>
        public Quaternion(Vector3 startingDirection, Vector3 endingDirection)
        {
            this.xyz = Vector3.Cross(startingDirection, endingDirection);
            this.w = Math.Sqrt(Math.Pow(startingDirection.Length, 2) * Math.Pow(endingDirection.Length, 2)) + Vector3.Dot(startingDirection, endingDirection);
            Normalize();
        }




        /// <summary>
        /// Gets or sets an OpenTK.Vector3d with the X, Y and Z components of this instance.
        /// </summary>
        public Vector3 Xyz { get { return xyz; } set { xyz = value; } }

        /// <summary>
        /// Gets or sets the X component of this instance.
        /// </summary>
        public double X { get { return xyz.x; } set { xyz.x = value; } }

        /// <summary>
        /// Gets or sets the Y component of this instance.
        /// </summary>
        public double Y { get { return xyz.y; } set { xyz.y = value; } }

        /// <summary>
        /// Gets or sets the Z component of this instance.
        /// </summary>
        public double Z { get { return xyz.z; } set { xyz.z = value; } }

        /// <summary>
        /// Gets or sets the W component of this instance.
        /// </summary>
        public double W { get { return w; } set { w = value; } }




        /// <summary>
        /// Convert the current quaternion to axis angle representation
        /// </summary>
        /// <param name="axis">The resultant axis</param>
        /// <param name="angle">The resultant angle</param>
        public void ToAxisAngle(out Vector3 axis, out double angle)
        {
            Vector4 result = ToAxisAngle();
            axis = result.Xyz;
            angle = result.w;
        }

        /// <summary>
        /// Convert this instance to an axis-angle representation.
        /// </summary>
        /// <returns>A Vector4 that is the axis-angle representation of this quaternion.</returns>
        public Vector4 ToAxisAngle()
        {
            Quaternion q = this;
            if (q.W > 1.0f)
                q.Normalize();
            Vector4 result = new Vector4();
            result.w = 2.0f * (float)System.Math.Acos(q.W); // angle
            float den = (float)System.Math.Sqrt(1.0 - q.W * q.W);
            if (den > 0.0001f)
            {
                result.Xyz = q.Xyz / den;
            }
            else
            {
                // This occurs when the angle is zero. 
                // Not a problem: just set an arbitrary normalized axis.
                result.Xyz = Vector3.UnitX;
            }

            return result;
        }



        /// <summary>
        /// Gets the length (magnitude) of the Quaterniond.
        /// </summary>
        /// <seealso cref="LengthSquared"/>
        public double Length
        {
            get
            {
                return (double)System.Math.Sqrt(W * W + Xyz.LengthSquared);
            }
        }



        /// <summary>
        /// Gets the square of the Quaterniond length (magnitude).
        /// </summary>
        public double LengthSquared
        {
            get
            {
                return W * W + Xyz.LengthSquared;
            }
        }



        /// <summary>
        /// Scales the Quaterniond to unit length.
        /// </summary>
        public void Normalize()
        {
            double scale = 1.0f / this.Length;
            Xyz *= scale;
            W *= scale;
        }



        /// <summary>
        /// Convert this Quaterniond to its conjugate
        /// </summary>
        public void Conjugate()
        {
            Xyz = -Xyz;
        }





        /// <summary>
        /// Defines the identity quaternion.
        /// </summary>
        public readonly static Quaternion Identity = new Quaternion(0, 0, 0, 1);


        /// <summary>
        /// Add two quaternions
        /// </summary>
        /// <param name="left">The first operand</param>
        /// <param name="right">The second operand</param>
        /// <returns>The result of the addition</returns>
        public static Quaternion Add(Quaternion left, Quaternion right)
        {
            return new Quaternion(
                left.Xyz + right.Xyz,
                left.W + right.W);
        }

        /// <summary>
        /// Add two quaternions
        /// </summary>
        /// <param name="left">The first operand</param>
        /// <param name="right">The second operand</param>
        /// <param name="result">The result of the addition</param>
        public static void Add(ref Quaternion left, ref Quaternion right, out Quaternion result)
        {
            result = new Quaternion(
                left.Xyz + right.Xyz,
                left.W + right.W);
        }



        /// <summary>
        /// Subtracts two instances.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <returns>The result of the operation.</returns>
        public static Quaternion Sub(Quaternion left, Quaternion right)
        {
            return new Quaternion(
                left.Xyz - right.Xyz,
                left.W - right.W);
        }

        /// <summary>
        /// Subtracts two instances.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <param name="result">The result of the operation.</param>
        public static void Sub(ref Quaternion left, ref Quaternion right, out Quaternion result)
        {
            result = new Quaternion(
                left.Xyz - right.Xyz,
                left.W - right.W);
        }



        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        [Obsolete("Use Multiply instead.")]
        public static Quaternion Mult(Quaternion left, Quaternion right)
        {
            return new Quaternion(
                right.W * left.Xyz + left.W * right.Xyz + Vector3.Cross(left.Xyz, right.Xyz),
                left.W * right.W - Vector3.Dot(left.Xyz, right.Xyz));
        }

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <param name="result">A new instance containing the result of the calculation.</param>
        [Obsolete("Use Multiply instead.")]
        public static void Mult(ref Quaternion left, ref Quaternion right, out Quaternion result)
        {
            result = new Quaternion(
                right.W * left.Xyz + left.W * right.Xyz + Vector3.Cross(left.Xyz, right.Xyz),
                left.W * right.W - Vector3.Dot(left.Xyz, right.Xyz));
        }

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        public static Quaternion Multiply(Quaternion left, Quaternion right)
        {
            Quaternion result;
            Multiply(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <param name="result">A new instance containing the result of the calculation.</param>
        public static void Multiply(ref Quaternion left, ref Quaternion right, out Quaternion result)
        {
            result = new Quaternion(
                right.W * left.Xyz + left.W * right.Xyz + Vector3.Cross(left.Xyz, right.Xyz),
                left.W * right.W - Vector3.Dot(left.Xyz, right.Xyz));
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="quaternion">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <param name="result">A new instance containing the result of the calculation.</param>
        public static void Multiply(ref Quaternion quaternion, double scale, out Quaternion result)
        {
            result = new Quaternion(quaternion.X * scale, quaternion.Y * scale, quaternion.Z * scale, quaternion.W * scale);
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="quaternion">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        public static Quaternion Multiply(Quaternion quaternion, double scale)
        {
            return new Quaternion(quaternion.X * scale, quaternion.Y * scale, quaternion.Z * scale, quaternion.W * scale);
        }



        /// <summary>
        /// Get the conjugate of the given Quaterniond
        /// </summary>
        /// <param name="q">The Quaterniond</param>
        /// <returns>The conjugate of the given Quaterniond</returns>
        public static Quaternion Conjugate(Quaternion q)
        {
            return new Quaternion(-q.Xyz, q.W);
        }

        /// <summary>
        /// Get the conjugate of the given Quaterniond
        /// </summary>
        /// <param name="q">The Quaterniond</param>
        /// <param name="result">The conjugate of the given Quaterniond</param>
        public static void Conjugate(ref Quaternion q, out Quaternion result)
        {
            result = new Quaternion(-q.Xyz, q.W);
        }



        /// <summary>
        /// Get the inverse of the given Quaterniond
        /// </summary>
        /// <param name="q">The Quaterniond to invert</param>
        /// <returns>The inverse of the given Quaterniond</returns>
        public static Quaternion Invert(Quaternion q)
        {
            Quaternion result;
            Invert(ref q, out result);
            return result;
        }

        /// <summary>
        /// Get the inverse of the given Quaterniond
        /// </summary>
        /// <param name="q">The Quaterniond to invert</param>
        /// <param name="result">The inverse of the given Quaterniond</param>
        public static void Invert(ref Quaternion q, out Quaternion result)
        {
            double lengthSq = q.LengthSquared;
            if (lengthSq != 0.0)
            {
                double i = 1.0f / lengthSq;
                result = new Quaternion(q.Xyz * -i, q.W * i);
            }
            else
            {
                result = q;
            }
        }



        /// <summary>
        /// Scale the given Quaterniond to unit length
        /// </summary>
        /// <param name="q">The Quaterniond to normalize</param>
        /// <returns>The normalized Quaterniond</returns>
        public static Quaternion Normalize(Quaternion q)
        {
            Quaternion result;
            Normalize(ref q, out result);
            return result;
        }

        /// <summary>
        /// Scale the given Quaterniond to unit length
        /// </summary>
        /// <param name="q">The Quaterniond to normalize</param>
        /// <param name="result">The normalized Quaterniond</param>
        public static void Normalize(ref Quaternion q, out Quaternion result)
        {
            double scale = 1.0f / q.Length;
            result = new Quaternion(q.Xyz * scale, q.W * scale);
        }



        public static Quaternion FromEulerAngles(Vector3 rotation)
        {
            Quaternion xRotation = FromAxisAngle(Vector3.UnitX, rotation.x);
            Quaternion yRotation = FromAxisAngle(Vector3.UnitY, rotation.y);
            Quaternion zRotation = FromAxisAngle(Vector3.UnitZ, rotation.z);
            //return xRotation * yRotation * zRotation;
            return zRotation * yRotation * xRotation;
        }



        /// <summary>
        /// Build a Quaterniond from the given axis and angle
        /// </summary>
        /// <param name="axis">The axis to rotate about</param>
        /// <param name="angle">The rotation angle in radians</param>
        /// <returns></returns>
        public static Quaternion FromAxisAngle(Vector3 axis, double angle)
        {
            if (axis.LengthSquared == 0.0f)
            {
                return Identity;
            }

            Quaternion result = Identity;
            angle *= 0.5f;
            axis.Normalize();
            result.Xyz = axis * (double)System.Math.Sin(angle);
            result.W = (double)System.Math.Cos(angle);
            return Normalize(result);
        }



        /// <summary>
        /// Do Spherical linear interpolation between two quaternions 
        /// </summary>
        /// <param name="q1">The first Quaterniond</param>
        /// <param name="q2">The second Quaterniond</param>
        /// <param name="blend">The blend factor</param>
        /// <returns>A smooth blend between the given quaternions</returns>
        public static Quaternion Slerp(Quaternion q1, Quaternion q2, double blend)
        {
            // if either input is zero, return the other.
            if (q1.LengthSquared == 0.0f)
            {
                if (q2.LengthSquared == 0.0f)
                {
                    return Identity;
                }
                return q2;
            }
            else if (q2.LengthSquared == 0.0f)
            {
                return q1;
            }


            double cosHalfAngle = q1.W * q2.W + Vector3.Dot(q1.Xyz, q2.Xyz);
            if (cosHalfAngle >= 1.0f || cosHalfAngle <= -1.0f)
            {
                // angle = 0.0f, so just return one input.
                return q1;
            }
            else if (cosHalfAngle < 0.0f)
            {
                q2.Xyz = -q2.Xyz;
                q2.W = -q2.W;
                cosHalfAngle = -cosHalfAngle;
            }

            double blendA;
            double blendB;
            if (cosHalfAngle < 0.99f)
            {
                // do proper slerp for big angles
                double halfAngle = (double)System.Math.Acos(cosHalfAngle);
                double sinHalfAngle = (double)System.Math.Sin(halfAngle);
                double oneOverSinHalfAngle = 1.0f / sinHalfAngle;
                blendA = (double)System.Math.Sin(halfAngle * (1.0f - blend)) * oneOverSinHalfAngle;
                blendB = (double)System.Math.Sin(halfAngle * blend) * oneOverSinHalfAngle;
            }
            else
            {
                // do lerp if angle is really small.
                blendA = 1.0f - blend;
                blendB = blend;
            }

            Quaternion result = new Quaternion(blendA * q1.Xyz + blendB * q2.Xyz, blendA * q1.W + blendB * q2.W);
            if (result.LengthSquared > 0.0f)
                return Normalize(result);
            else
                return Identity;
        }




        /// <summary>
        /// Adds two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static Quaternion operator +(Quaternion left, Quaternion right)
        {
            left.Xyz += right.Xyz;
            left.W += right.W;
            return left;
        }

        /// <summary>
        /// Subtracts two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static Quaternion operator -(Quaternion left, Quaternion right)
        {
            left.Xyz -= right.Xyz;
            left.W -= right.W;
            return left;
        }

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static Quaternion operator *(Quaternion left, Quaternion right)
        {
            Multiply(ref left, ref right, out left);
            return left;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="quaternion">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        public static Quaternion operator *(Quaternion quaternion, double scale)
        {
            Multiply(ref quaternion, scale, out quaternion);
            return quaternion;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="quaternion">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        public static Quaternion operator *(double scale, Quaternion quaternion)
        {
            return new Quaternion(quaternion.X * scale, quaternion.Y * scale, quaternion.Z * scale, quaternion.W * scale);
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left equals right; false otherwise.</returns>
        public static bool operator ==(Quaternion left, Quaternion right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left does not equal right; false otherwise.</returns>
        public static bool operator !=(Quaternion left, Quaternion right)
        {
            return !left.Equals(right);
        }




        /// <summary>
        /// Returns a System.String that represents the current Quaterniond.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("V: {0}, W: {1}", Xyz, W);
        }



        /// <summary>
        /// Compares this object instance to another object for equality. 
        /// </summary>
        /// <param name="other">The other object to be used in the comparison.</param>
        /// <returns>True if both objects are Quaternions of equal value. Otherwise it returns false.</returns>
        public override bool Equals(object other)
        {
            if (other is Quaternion == false) return false;
            return this == (Quaternion)other;
        }



        /// <summary>
        /// Provides the hash code for this object. 
        /// </summary>
        /// <returns>A hash code formed from the bitwise XOR of this objects members.</returns>
        public override int GetHashCode()
        {
            return new { Xyz.x, Xyz.y, Xyz.z, W }.GetHashCode();
        }





        /// <summary>
        /// Compares this Quaterniond instance to another Quaterniond for equality. 
        /// </summary>
        /// <param name="other">The other Quaterniond to be used in the comparison.</param>
        /// <returns>True if both instances are equal; false otherwise.</returns>
        public bool Equals(Quaternion other)
        {
            return Xyz == other.Xyz && W == other.W;
        }

    }
}