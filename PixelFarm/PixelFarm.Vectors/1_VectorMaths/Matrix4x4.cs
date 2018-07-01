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
/*
Copyright (c) 2014, Lars Brubaker
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies, 
either expressed or implied, of the FreeBSD Project.
*/


using System;
using System.Runtime.InteropServices;
namespace PixelFarm.VectorMath
{
#if true
    /// <summary>
    /// Represents a 4x4 Matrix with double-precision components.
    /// </summary>
    //[Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix4X4 : IEquatable<Matrix4X4>
    {

        /// <summary>
        /// Top row of the matrix
        /// </summary>
        public Vector4 Row0;
        /// <summary>
        /// 2nd row of the matrix
        /// </summary>
        public Vector4 Row1;
        /// <summary>
        /// 3rd row of the matrix
        /// </summary>
        public Vector4 Row2;
        /// <summary>
        /// Bottom row of the matrix
        /// </summary>
        public Vector4 Row3;
        /// <summary>
        /// The identity matrix
        /// </summary>
        public static Matrix4X4 Identity = new Matrix4X4(Vector4.UnitX, Vector4.UnitY, Vector4.UnitZ, Vector4.UnitW);


        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="row0">Top row of the matrix</param>
        /// <param name="row1">Second row of the matrix</param>
        /// <param name="row2">Third row of the matrix</param>
        /// <param name="row3">Bottom row of the matrix</param>
        public Matrix4X4(Vector4 row0, Vector4 row1, Vector4 row2, Vector4 row3)
        {
            Row0 = row0;
            Row1 = row1;
            Row2 = row2;
            Row3 = row3;
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="m00">First item of the first row.</param>
        /// <param name="m01">Second item of the first row.</param>
        /// <param name="m02">Third item of the first row.</param>
        /// <param name="m03">Fourth item of the first row.</param>
        /// <param name="m10">First item of the second row.</param>
        /// <param name="m11">Second item of the second row.</param>
        /// <param name="m12">Third item of the second row.</param>
        /// <param name="m13">Fourth item of the second row.</param>
        /// <param name="m20">First item of the third row.</param>
        /// <param name="m21">Second item of the third row.</param>
        /// <param name="m22">Third item of the third row.</param>
        /// <param name="m23">First item of the third row.</param>
        /// <param name="m30">Fourth item of the fourth row.</param>
        /// <param name="m31">Second item of the fourth row.</param>
        /// <param name="m32">Third item of the fourth row.</param>
        /// <param name="m33">Fourth item of the fourth row.</param>
        public Matrix4X4(
            double m00, double m01, double m02, double m03,
            double m10, double m11, double m12, double m13,
            double m20, double m21, double m22, double m23,
            double m30, double m31, double m32, double m33)
        {
            Row0 = new Vector4(m00, m01, m02, m03);
            Row1 = new Vector4(m10, m11, m12, m13);
            Row2 = new Vector4(m20, m21, m22, m23);
            Row3 = new Vector4(m30, m31, m32, m33);
        }

        public Matrix4X4(double[] double16)
        {
            Row0 = new Vector4(double16[0], double16[1], double16[2], double16[3]);
            Row1 = new Vector4(double16[4], double16[5], double16[6], double16[7]);
            Row2 = new Vector4(double16[8], double16[9], double16[10], double16[11]);
            Row3 = new Vector4(double16[12], double16[13], double16[14], double16[15]);
        }




        /// <summary>
        /// The determinant of this matrix
        /// </summary>
        public double Determinant
        {
            get
            {
                return
                    Row0.x * Row1.y * Row2.z * Row3.w - Row0.x * Row1.y * Row2.w * Row3.z + Row0.x * Row1.z * Row2.w * Row3.y - Row0.x * Row1.z * Row2.y * Row3.w
                  + Row0.x * Row1.w * Row2.y * Row3.z - Row0.x * Row1.w * Row2.z * Row3.y - Row0.y * Row1.z * Row2.w * Row3.x + Row0.y * Row1.z * Row2.x * Row3.w
                  - Row0.y * Row1.w * Row2.x * Row3.z + Row0.y * Row1.w * Row2.z * Row3.x - Row0.y * Row1.x * Row2.z * Row3.w + Row0.y * Row1.x * Row2.w * Row3.z
                  + Row0.z * Row1.w * Row2.x * Row3.y - Row0.z * Row1.w * Row2.y * Row3.x + Row0.z * Row1.x * Row2.y * Row3.w - Row0.z * Row1.x * Row2.w * Row3.y
                  + Row0.z * Row1.y * Row2.w * Row3.x - Row0.z * Row1.y * Row2.x * Row3.w - Row0.w * Row1.x * Row2.y * Row3.z + Row0.w * Row1.x * Row2.z * Row3.y
                  - Row0.w * Row1.y * Row2.z * Row3.x + Row0.w * Row1.y * Row2.x * Row3.z - Row0.w * Row1.z * Row2.x * Row3.y + Row0.w * Row1.z * Row2.y * Row3.x;
            }
        }

        /// <summary>
        /// Get just the position out of the matrix.
        /// </summary>
        public Vector3 Position
        {
            get { return new Vector3(Row3); }
        }

        /// <summary>
        /// The first column of this matrix
        /// </summary>
        public Vector4 Column0
        {
            get { return new Vector4(Row0.x, Row1.x, Row2.x, Row3.x); }
        }

        /// <summary>
        /// The second column of this matrix
        /// </summary>
        public Vector4 Column1
        {
            get { return new Vector4(Row0.y, Row1.y, Row2.y, Row3.y); }
        }

        /// <summary>
        /// The third column of this matrix
        /// </summary>
        public Vector4 Column2
        {
            get { return new Vector4(Row0.z, Row1.z, Row2.z, Row3.z); }
        }

        /// <summary>
        /// The fourth column of this matrix
        /// </summary>
        public Vector4 Column3
        {
            get { return new Vector4(Row0.w, Row1.w, Row2.w, Row3.w); }
        }

        /// <summary>
        /// Gets or sets the value at row 1, column 1 of this instance.
        /// </summary>
        public double M11 { get { return Row0.x; } set { Row0.x = value; } }

        /// <summary>
        /// Gets or sets the value at row 1, column 2 of this instance.
        /// </summary>
        public double M12 { get { return Row0.y; } set { Row0.y = value; } }

        /// <summary>
        /// Gets or sets the value at row 1, column 3 of this instance.
        /// </summary>
        public double M13 { get { return Row0.z; } set { Row0.z = value; } }

        /// <summary>
        /// Gets or sets the value at row 1, column 4 of this instance.
        /// </summary>
        public double M14 { get { return Row0.w; } set { Row0.w = value; } }

        /// <summary>
        /// Gets or sets the value at row 2, column 1 of this instance.
        /// </summary>
        public double M21 { get { return Row1.x; } set { Row1.x = value; } }

        /// <summary>
        /// Gets or sets the value at row 2, column 2 of this instance.
        /// </summary>
        public double M22 { get { return Row1.y; } set { Row1.y = value; } }

        /// <summary>
        /// Gets or sets the value at row 2, column 3 of this instance.
        /// </summary>
        public double M23 { get { return Row1.z; } set { Row1.z = value; } }

        /// <summary>
        /// Gets or sets the value at row 2, column 4 of this instance.
        /// </summary>
        public double M24 { get { return Row1.w; } set { Row1.w = value; } }

        /// <summary>
        /// Gets or sets the value at row 3, column 1 of this instance.
        /// </summary>
        public double M31 { get { return Row2.x; } set { Row2.x = value; } }

        /// <summary>
        /// Gets or sets the value at row 3, column 2 of this instance.
        /// </summary>
        public double M32 { get { return Row2.y; } set { Row2.y = value; } }

        /// <summary>
        /// Gets or sets the value at row 3, column 3 of this instance.
        /// </summary>
        public double M33 { get { return Row2.z; } set { Row2.z = value; } }

        /// <summary>
        /// Gets or sets the value at row 3, column 4 of this instance.
        /// </summary>
        public double M34 { get { return Row2.w; } set { Row2.w = value; } }

        /// <summary>
        /// Gets or sets the value at row 4, column 1 of this instance.
        /// </summary>
        public double M41 { get { return Row3.x; } set { Row3.x = value; } }

        /// <summary>
        /// Gets or sets the value at row 4, column 2 of this instance.
        /// </summary>
        public double M42 { get { return Row3.y; } set { Row3.y = value; } }

        /// <summary>
        /// Gets or sets the value at row 4, column 3 of this instance.
        /// </summary>
        public double M43 { get { return Row3.z; } set { Row3.z = value; } }

        /// <summary>
        /// Gets or sets the value at row 4, column 4 of this instance.
        /// </summary>
        public double M44 { get { return Row3.w; } set { Row3.w = value; } }



        public double this[int row, int column]
        {
            get
            {
                switch (row)
                {
                    case 0:
                        return Row0[column];
                    case 1:
                        return Row1[column];
                    case 2:
                        return Row2[column];
                    case 3:
                        return Row3[column];
                    default:
                        throw new IndexOutOfRangeException();
                }
            }

            set
            {
                switch (row)
                {
                    case 0:
                        Row0[column] = value;
                        break;
                    case 1:
                        Row1[column] = value;
                        break;
                    case 2:
                        Row2[column] = value;
                        break;
                    case 3:
                        Row3[column] = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public Quaternion GetRotation()
        {
            Quaternion rotation = Quaternion.Identity;
            double tr = M11 + M22 + M33;
            if (tr > 0)
            {
                double S = Math.Sqrt(tr + 1.0) * 2; // S=4*qw 
                rotation.W = 0.25 * S;
                rotation.X = (M32 - M23) / S;
                rotation.Y = (M13 - M31) / S;
                rotation.Z = (M21 - M12) / S;
            }
            else if ((M11 > M22) & (M11 > M33))
            {
                double S = Math.Sqrt(1.0 + M11 - M22 - M33) * 2; // S=4*qx 
                rotation.W = (M32 - M23) / S;
                rotation.X = 0.25 * S;
                rotation.Y = (M12 + M21) / S;
                rotation.Z = (M13 + M31) / S;
            }
            else if (M22 > M33)
            {
                double S = Math.Sqrt(1.0 + M22 - M11 - M33) * 2; // S=4*qy
                rotation.W = (M13 - M31) / S;
                rotation.X = (M12 + M21) / S;
                rotation.Y = 0.25 * S;
                rotation.Z = (M23 + M32) / S;
            }
            else
            {
                double S = Math.Sqrt(1.0 + M33 - M11 - M22) * 2; // S=4*qz
                rotation.W = (M21 - M12) / S;
                rotation.X = (M13 + M31) / S;
                rotation.Y = (M23 + M32) / S;
                rotation.Z = 0.25 * S;
            }

            return rotation;
        }


        /// <summary>
        /// Converts this instance into its inverse.
        /// </summary>
        public void Invert()
        {
            this = Matrix4X4.Invert(this);
        }



        /// <summary>
        /// Converts this instance into its transpose.
        /// </summary>
        public void Transpose()
        {
            this = Matrix4X4.Transpose(this);
        }





        /// <summary>
        /// Build a rotation matrix from the specified axis/angle rotation.
        /// </summary>
        /// <param name="axis">The axis to rotate about.</param>
        /// <param name="angle">Angle in radians to rotate counter-clockwise (looking in the direction of the given axis).</param>
        /// <param name="result">A matrix instance.</param>
        public static void CreateFromAxisAngle(Vector3 axis, double angle, out Matrix4X4 result)
        {
            double cos = System.Math.Cos(-angle);
            double sin = System.Math.Sin(-angle);
            double t = 1.0 - cos;
            axis.Normalize();
            result = new Matrix4X4(t * axis.x * axis.x + cos, t * axis.x * axis.y - sin * axis.z, t * axis.x * axis.z + sin * axis.y, 0.0,
                                 t * axis.x * axis.y + sin * axis.z, t * axis.y * axis.y + cos, t * axis.y * axis.z - sin * axis.x, 0.0,
                                 t * axis.x * axis.z - sin * axis.y, t * axis.y * axis.z + sin * axis.x, t * axis.z * axis.z + cos, 0.0,
                                 0, 0, 0, 1);
        }

        /// <summary>
        /// Build a rotation matrix from the specified axis/angle rotation.
        /// </summary>
        /// <param name="axis">The axis to rotate about.</param>
        /// <param name="angle">Angle in radians to rotate counter-clockwise (looking in the direction of the given axis).</param>
        /// <returns>A matrix instance.</returns>
        public static Matrix4X4 CreateFromAxisAngle(Vector3 axis, double angle)
        {
            Matrix4X4 result;
            CreateFromAxisAngle(axis, angle, out result);
            return result;
        }



        public static Matrix4X4 CreateRotation(Vector3 radians)
        {
            return Matrix4X4.CreateRotation(Quaternion.FromEulerAngles(radians));
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the x-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <param name="result">The resulting Matrix4 instance.</param>
        public static void CreateRotationX(double angle, out Matrix4X4 result)
        {
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);
            result.Row0 = Vector4.UnitX;
            result.Row1 = new Vector4(0, cos, sin, 0);
            result.Row2 = new Vector4(0, -sin, cos, 0);
            result.Row3 = Vector4.UnitW;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the x-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <returns>The resulting Matrix4 instance.</returns>
        public static Matrix4X4 CreateRotationX(double angle)
        {
            Matrix4X4 result;
            CreateRotationX(angle, out result);
            return result;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the y-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <param name="result">The resulting Matrix4 instance.</param>
        public static void CreateRotationY(double angle, out Matrix4X4 result)
        {
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);
            result.Row0 = new Vector4(cos, 0, -sin, 0);
            result.Row1 = Vector4.UnitY;
            result.Row2 = new Vector4(sin, 0, cos, 0);
            result.Row3 = Vector4.UnitW;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the y-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <returns>The resulting Matrix4 instance.</returns>
        public static Matrix4X4 CreateRotationY(double angle)
        {
            Matrix4X4 result;
            CreateRotationY(angle, out result);
            return result;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the z-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <param name="result">The resulting Matrix4 instance.</param>
        public static void CreateRotationZ(double angle, out Matrix4X4 result)
        {
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);
            result.Row0 = new Vector4(cos, sin, 0, 0);
            result.Row1 = new Vector4(-sin, cos, 0, 0);
            result.Row2 = Vector4.UnitZ;
            result.Row3 = Vector4.UnitW;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the z-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <returns>The resulting Matrix4 instance.</returns>
        public static Matrix4X4 CreateRotationZ(double angle)
        {
            Matrix4X4 result;
            CreateRotationZ(angle, out result);
            return result;
        }

        /// <summary>
        /// Build a rotation matrix to rotate about the given axis
        /// </summary>
        /// <param name="axis">the axis to rotate about</param>
        /// <param name="angle">angle in radians to rotate counter-clockwise (looking in the direction of the given axis)</param>
        /// <returns>A rotation matrix</returns>
        public static Matrix4X4 CreateRotation(Vector3 axis, double angle)
        {
            double cos = System.Math.Cos(-angle);
            double sin = System.Math.Sin(-angle);
            double t = 1.0 - cos;
            axis.Normalize();
            Matrix4X4 result;
            result.Row0 = new Vector4(t * axis.x * axis.x + cos, t * axis.x * axis.y - sin * axis.z, t * axis.x * axis.z + sin * axis.y, 0.0);
            result.Row1 = new Vector4(t * axis.x * axis.y + sin * axis.z, t * axis.y * axis.y + cos, t * axis.y * axis.z - sin * axis.x, 0.0);
            result.Row2 = new Vector4(t * axis.x * axis.z - sin * axis.y, t * axis.y * axis.z + sin * axis.x, t * axis.z * axis.z + cos, 0.0);
            result.Row3 = Vector4.UnitW;
            return result;
        }

        /// <summary>
        /// Build a rotation matrix from a quaternion
        /// </summary>
        /// <param name="q">the quaternion</param>
        /// <returns>A rotation matrix</returns>
        public static Matrix4X4 CreateRotation(Quaternion q)
        {
            Vector3 axis;
            double angle;
            q.ToAxisAngle(out axis, out angle);
            return CreateRotation(axis, angle);
        }

        /// <summary>
        /// Build a rotation matrix that will rotate from one direction to another
        /// </summary>
        /// <param name="startingDirection"></param>
        /// <param name="endingDirection"></param>
        /// <returns></returns>
        public static Matrix4X4 CreateRotation(Vector3 startingDirection, Vector3 endingDirection)
        {
            Quaternion q = new Quaternion(startingDirection, endingDirection);
            return CreateRotation(q);
        }



        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="x">X translation.</param>
        /// <param name="y">Y translation.</param>
        /// <param name="z">Z translation.</param>
        /// <param name="result">The resulting Matrix4d instance.</param>
        public static void CreateTranslation(double x, double y, double z, out Matrix4X4 result)
        {
            result = Identity;
            result.Row3 = new Vector4(x, y, z, 1);
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="vector">The translation vector.</param>
        /// <param name="result">The resulting Matrix4d instance.</param>
        public static void CreateTranslation(ref Vector3 vector, out Matrix4X4 result)
        {
            result = Identity;
            result.Row3 = new Vector4(vector.x, vector.y, vector.z, 1);
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="x">X translation.</param>
        /// <param name="y">Y translation.</param>
        /// <param name="z">Z translation.</param>
        /// <returns>The resulting Matrix4d instance.</returns>
        public static Matrix4X4 CreateTranslation(double x, double y, double z)
        {
            Matrix4X4 result;
            CreateTranslation(x, y, z, out result);
            return result;
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="vector">The translation vector.</param>
        /// <returns>The resulting Matrix4d instance.</returns>
        public static Matrix4X4 CreateTranslation(Vector3 vector)
        {
            Matrix4X4 result;
            CreateTranslation(vector.x, vector.y, vector.z, out result);
            return result;
        }



        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="width">The width of the projection volume.</param>
        /// <param name="height">The height of the projection volume.</param>
        /// <param name="zNear">The near edge of the projection volume.</param>
        /// <param name="zFar">The far edge of the projection volume.</param>
        /// <param name="result">The resulting Matrix4d instance.</param>
        public static void CreateOrthographic(double width, double height, double zNear, double zFar, out Matrix4X4 result)
        {
            CreateOrthographicOffCenter(-width / 2, width / 2, -height / 2, height / 2, zNear, zFar, out result);
        }

        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="width">The width of the projection volume.</param>
        /// <param name="height">The height of the projection volume.</param>
        /// <param name="zNear">The near edge of the projection volume.</param>
        /// <param name="zFar">The far edge of the projection volume.</param>
        /// <rereturns>The resulting Matrix4d instance.</rereturns>
        public static Matrix4X4 CreateOrthographic(double width, double height, double zNear, double zFar)
        {
            Matrix4X4 result;
            CreateOrthographicOffCenter(-width / 2, width / 2, -height / 2, height / 2, zNear, zFar, out result);
            return result;
        }



        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="left">The left edge of the projection volume.</param>
        /// <param name="right">The right edge of the projection volume.</param>
        /// <param name="bottom">The bottom edge of the projection volume.</param>
        /// <param name="top">The top edge of the projection volume.</param>
        /// <param name="zNear">The near edge of the projection volume.</param>
        /// <param name="zFar">The far edge of the projection volume.</param>
        /// <param name="result">The resulting Matrix4d instance.</param>
        public static void CreateOrthographicOffCenter(double left, double right, double bottom, double top, double zNear, double zFar, out Matrix4X4 result)
        {
            result = new Matrix4X4();
            double invRL = 1 / (right - left);
            double invTB = 1 / (top - bottom);
            double invFN = 1 / (zFar - zNear);
            result.M11 = 2 * invRL;
            result.M22 = 2 * invTB;
            result.M33 = -2 * invFN;
            result.M41 = -(right + left) * invRL;
            result.M42 = -(top + bottom) * invTB;
            result.M43 = -(zFar + zNear) * invFN;
            result.M44 = 1;
        }

        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="left">The left edge of the projection volume.</param>
        /// <param name="right">The right edge of the projection volume.</param>
        /// <param name="bottom">The bottom edge of the projection volume.</param>
        /// <param name="top">The top edge of the projection volume.</param>
        /// <param name="zNear">The near edge of the projection volume.</param>
        /// <param name="zFar">The far edge of the projection volume.</param>
        /// <returns>The resulting Matrix4d instance.</returns>
        public static Matrix4X4 CreateOrthographicOffCenter(double left, double right, double bottom, double top, double zNear, double zFar)
        {
            Matrix4X4 result;
            CreateOrthographicOffCenter(left, right, bottom, top, zNear, zFar, out result);
            return result;
        }



        /// <summary>
        /// Creates a perspective projection matrix.
        /// </summary>
        /// <param name="fovYRadians">Angle of the field of view in the y direction (in radians)</param>
        /// <param name="aspectWidthOverHeight">Aspect ratio of the view (width / height)</param>
        /// <param name="zNear">Distance to the near clip plane</param>
        /// <param name="zFar">Distance to the far clip plane</param>
        /// <param name="result">A projection matrix that transforms camera space to raster space</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown under the following conditions:
        /// <list type="bullet">
        /// <item>fovy is zero, less than zero or larger than Math.PI</item>
        /// <item>aspect is negative or zero</item>
        /// <item>zNear is negative or zero</item>
        /// <item>zFar is negative or zero</item>
        /// <item>zNear is larger than zFar</item>
        /// </list>
        /// </exception>
        public static void CreatePerspectiveFieldOfView(double fovYRadians, double aspectWidthOverHeight, double zNear, double zFar, out Matrix4X4 result)
        {
            if (fovYRadians <= 0 || fovYRadians > Math.PI)
                throw new ArgumentOutOfRangeException("fovy");
            if (aspectWidthOverHeight <= 0)
                throw new ArgumentOutOfRangeException("aspect");
            if (zNear <= 0)
                throw new ArgumentOutOfRangeException("zNear");
            if (zFar <= 0)
                throw new ArgumentOutOfRangeException("zFar");
            if (zNear >= zFar)
                throw new ArgumentOutOfRangeException("zNear");
            double yMax = zNear * System.Math.Tan(0.5 * fovYRadians);
            double yMin = -yMax;
            double xMin = yMin * aspectWidthOverHeight;
            double xMax = yMax * aspectWidthOverHeight;
            CreatePerspectiveOffCenter(xMin, xMax, yMin, yMax, zNear, zFar, out result);
        }

        /// <summary>
        /// Creates a perspective projection matrix.
        /// </summary>
        /// <param name="fovYRadians">Angle of the field of view in the y direction (in radians)</param>
        /// <param name="aspectWidthOverHeight">Aspect ratio of the view (width / height)</param>
        /// <param name="zNear">Distance to the near clip plane</param>
        /// <param name="zFar">Distance to the far clip plane</param>
        /// <returns>A projection matrix that transforms camera space to raster space</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown under the following conditions:
        /// <list type="bullet">
        /// <item>fovYRadians is zero, less than zero or larger than Math.PI</item>
        /// <item>aspect is negative or zero</item>
        /// <item>zNear is negative or zero</item>
        /// <item>zFar is negative or zero</item>
        /// <item>zNear is larger than zFar</item>
        /// </list>
        /// </exception>
        public static Matrix4X4 CreatePerspectiveFieldOfView(double fovYRadians, double aspectWidthOverHeight, double zNear, double zFar)
        {
            Matrix4X4 result;
            CreatePerspectiveFieldOfView(fovYRadians, aspectWidthOverHeight, zNear, zFar, out result);
            return result;
        }



        /// <summary>
        /// Creates an perspective projection matrix.
        /// </summary>
        /// <param name="left">Left edge of the view frustum</param>
        /// <param name="right">Right edge of the view frustum</param>
        /// <param name="bottom">Bottom edge of the view frustum</param>
        /// <param name="top">Top edge of the view frustum</param>
        /// <param name="zNear">Distance to the near clip plane</param>
        /// <param name="zFar">Distance to the far clip plane</param>
        /// <param name="result">A projection matrix that transforms camera space to raster space</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown under the following conditions:
        /// <list type="bullet">
        /// <item>zNear is negative or zero</item>
        /// <item>zFar is negative or zero</item>
        /// <item>zNear is larger than zFar</item>
        /// </list>
        /// </exception>
        public static void CreatePerspectiveOffCenter(double left, double right, double bottom, double top, double zNear, double zFar, out Matrix4X4 result)
        {
            if (zNear <= 0)
            {
                throw new ArgumentOutOfRangeException("zNear");
            }
            if (zFar <= 0)
            {
                throw new ArgumentOutOfRangeException("zFar");
            }
            if (zNear >= zFar)
            {
                throw new ArgumentOutOfRangeException("zNear");
            }

            double x = (2.0 * zNear) / (right - left);
            double y = (2.0 * zNear) / (top - bottom);
            double a = (right + left) / (right - left);
            double b = (top + bottom) / (top - bottom);
            double c = -(zFar + zNear) / (zFar - zNear);
            double d = -(2.0 * zFar * zNear) / (zFar - zNear);
            result = new Matrix4X4(x, 0, 0, 0,
                                 0, y, 0, 0,
                                 a, b, c, -1,
                                 0, 0, d, 0);
        }

        /// <summary>
        /// Creates an perspective projection matrix.
        /// </summary>
        /// <param name="left">Left edge of the view frustum</param>
        /// <param name="right">Right edge of the view frustum</param>
        /// <param name="bottom">Bottom edge of the view frustum</param>
        /// <param name="top">Top edge of the view frustum</param>
        /// <param name="zNear">Distance to the near clip plane</param>
        /// <param name="zFar">Distance to the far clip plane</param>
        /// <returns>A projection matrix that transforms camera space to raster space</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown under the following conditions:
        /// <list type="bullet">
        /// <item>zNear is negative or zero</item>
        /// <item>zFar is negative or zero</item>
        /// <item>zNear is larger than zFar</item>
        /// </list>
        /// </exception>
        public static Matrix4X4 CreatePerspectiveOffCenter(double left, double right, double bottom, double top, double zNear, double zFar)
        {
            Matrix4X4 result;
            CreatePerspectiveOffCenter(left, right, bottom, top, zNear, zFar, out result);
            return result;
        }



        /// <summary>
        /// Build a scaling matrix
        /// </summary>
        /// <param name="scale">Single scale factor for x,y and z axes</param>
        /// <returns>A scaling matrix</returns>
        public static Matrix4X4 CreateScale(double scale)
        {
            return CreateScale(scale, scale, scale);
        }

        /// <summary>
        /// Build a scaling matrix
        /// </summary>
        /// <param name="scale">Scale factors for x,y and z axes</param>
        /// <returns>A scaling matrix</returns>
        public static Matrix4X4 CreateScale(Vector3 scale)
        {
            return CreateScale(scale.x, scale.y, scale.z);
        }

        /// <summary>
        /// Build a scaling matrix
        /// </summary>
        /// <param name="x">Scale factor for x-axis</param>
        /// <param name="y">Scale factor for y-axis</param>
        /// <param name="z">Scale factor for z-axis</param>
        /// <returns>A scaling matrix</returns>
        public static Matrix4X4 CreateScale(double x, double y, double z)
        {
            Matrix4X4 result;
            result.Row0 = Vector4.UnitX * x;
            result.Row1 = Vector4.UnitY * y;
            result.Row2 = Vector4.UnitZ * z;
            result.Row3 = Vector4.UnitW;
            return result;
        }



        /// <summary>
        /// Build a world space to camera space matrix
        /// </summary>
        /// <param name="eye">Eye (camera) position in world space</param>
        /// <param name="target">Target position in world space</param>
        /// <param name="up">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <returns>A Matrix that transforms world space to camera space</returns>
        public static Matrix4X4 LookAt(Vector3 eye, Vector3 target, Vector3 up)
        {
            // There are lots of examples of look at code on the internet that don't do all these normalizes and also find the position
            // through several dot products.  The problem with them is that they have a bit of error in that all the vectors arn't normal and need to be.
            Vector3 z = Vector3.Normalize(eye - target);
            Vector3 x = Vector3.Normalize(Vector3.Cross(up, z));
            Vector3 y = Vector3.Normalize(Vector3.Cross(z, x));
            Matrix4X4 rot = new Matrix4X4(new Vector4(x.x, y.x, z.x, 0.0),
                                        new Vector4(x.y, y.y, z.y, 0.0),
                                        new Vector4(x.z, y.z, z.z, 0.0),
                                        Vector4.UnitW);
            Matrix4X4 trans = Matrix4X4.CreateTranslation(-eye);
            return trans * rot;
        }

        /// <summary>
        /// Build a world space to camera space matrix
        /// </summary>
        /// <param name="eyeX">Eye (camera) position in world space</param>
        /// <param name="eyeY">Eye (camera) position in world space</param>
        /// <param name="eyeZ">Eye (camera) position in world space</param>
        /// <param name="targetX">Target position in world space</param>
        /// <param name="targetY">Target position in world space</param>
        /// <param name="targetZ">Target position in world space</param>
        /// <param name="upX">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <param name="upY">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <param name="upZ">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <returns>A Matrix4 that transforms world space to camera space</returns>
        public static Matrix4X4 LookAt(double eyeX, double eyeY, double eyeZ, double targetX, double targetY, double targetZ, double upX, double upY, double upZ)
        {
            return LookAt(new Vector3(eyeX, eyeY, eyeZ), new Vector3(targetX, targetY, targetZ), new Vector3(upX, upY, upZ));
        }

        /// <summary>
        /// Build a projection matrix
        /// </summary>
        /// <param name="left">Left edge of the view frustum</param>
        /// <param name="right">Right edge of the view frustum</param>
        /// <param name="bottom">Bottom edge of the view frustum</param>
        /// <param name="top">Top edge of the view frustum</param>
        /// <param name="near">Distance to the near clip plane</param>
        /// <param name="far">Distance to the far clip plane</param>
        /// <returns>A projection matrix that transforms camera space to raster space</returns>
        public static Matrix4X4 Frustum(double left, double right, double bottom, double top, double near, double far)
        {
            double invRL = 1.0 / (right - left);
            double invTB = 1.0 / (top - bottom);
            double invFN = 1.0 / (far - near);
            return new Matrix4X4(new Vector4(2.0 * near * invRL, 0.0, 0.0, 0.0),
                               new Vector4(0.0, 2.0 * near * invTB, 0.0, 0.0),
                               new Vector4((right + left) * invRL, (top + bottom) * invTB, -(far + near) * invFN, -1.0),
                               new Vector4(0.0, 0.0, -2.0 * far * near * invFN, 0.0));
        }

        /// <summary>
        /// Build a projection matrix
        /// </summary>
        /// <param name="fovy">Angle of the field of view in the y direction (in radians)</param>
        /// <param name="aspect">Aspect ratio of the view (width / height)</param>
        /// <param name="near">Distance to the near clip plane</param>
        /// <param name="far">Distance to the far clip plane</param>
        /// <returns>A projection matrix that transforms camera space to raster space</returns>
        public static Matrix4X4 Perspective(double fovy, double aspect, double near, double far)
        {
            double yMax = near * System.Math.Tan(0.5f * fovy);
            double yMin = -yMax;
            double xMin = yMin * aspect;
            double xMax = yMax * aspect;
            return Frustum(xMin, xMax, yMin, yMax, near, far);
        }



        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The left operand of the multiplication.</param>
        /// <param name="right">The right operand of the multiplication.</param>
        /// <returns>A new instance that is the result of the multiplication</returns>
        public static Matrix4X4 Mult(Matrix4X4 left, Matrix4X4 right)
        {
            Matrix4X4 result;
            Mult(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The left operand of the multiplication.</param>
        /// <param name="right">The right operand of the multiplication.</param>
        /// <param name="result">A new instance that is the result of the multiplication</param>
        public static void Mult(ref Matrix4X4 left, ref Matrix4X4 right, out Matrix4X4 result)
        {
            result = new Matrix4X4();
            result.M11 = left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31 + left.M14 * right.M41;
            result.M12 = left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32 + left.M14 * right.M42;
            result.M13 = left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33 + left.M14 * right.M43;
            result.M14 = left.M11 * right.M14 + left.M12 * right.M24 + left.M13 * right.M34 + left.M14 * right.M44;
            result.M21 = left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31 + left.M24 * right.M41;
            result.M22 = left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32 + left.M24 * right.M42;
            result.M23 = left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33 + left.M24 * right.M43;
            result.M24 = left.M21 * right.M14 + left.M22 * right.M24 + left.M23 * right.M34 + left.M24 * right.M44;
            result.M31 = left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31 + left.M34 * right.M41;
            result.M32 = left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32 + left.M34 * right.M42;
            result.M33 = left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33 + left.M34 * right.M43;
            result.M34 = left.M31 * right.M14 + left.M32 * right.M24 + left.M33 * right.M34 + left.M34 * right.M44;
            result.M41 = left.M41 * right.M11 + left.M42 * right.M21 + left.M43 * right.M31 + left.M44 * right.M41;
            result.M42 = left.M41 * right.M12 + left.M42 * right.M22 + left.M43 * right.M32 + left.M44 * right.M42;
            result.M43 = left.M41 * right.M13 + left.M42 * right.M23 + left.M43 * right.M33 + left.M44 * right.M43;
            result.M44 = left.M41 * right.M14 + left.M42 * right.M24 + left.M43 * right.M34 + left.M44 * right.M44;
        }



        /// <summary>
        /// Calculate the inverse of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to invert</param>
        /// <returns>The inverse of the given matrix if it has one, or the input if it is singular</returns>
        /// <exception cref="InvalidOperationException">Thrown if the Matrix4d is singular.</exception>
        public static Matrix4X4 Invert(Matrix4X4 mat)
        {
            int[] colIdx = { 0, 0, 0, 0 };
            int[] rowIdx = { 0, 0, 0, 0 };
            int[] pivotIdx = { -1, -1, -1, -1 };
            // convert the matrix to an array for easy looping
            double[,] inverse = {{mat.Row0.x, mat.Row0.y, mat.Row0.z, mat.Row0.w},
                                {mat.Row1.x, mat.Row1.y, mat.Row1.z, mat.Row1.w},
                                {mat.Row2.x, mat.Row2.y, mat.Row2.z, mat.Row2.w},
                                {mat.Row3.x, mat.Row3.y, mat.Row3.z, mat.Row3.w} };
            int icol = 0;
            int irow = 0;
            for (int i = 0; i < 4; i++)
            {
                // Find the largest pivot value
                double maxPivot = 0.0;
                for (int j = 0; j < 4; j++)
                {
                    if (pivotIdx[j] != 0)
                    {
                        for (int k = 0; k < 4; ++k)
                        {
                            if (pivotIdx[k] == -1)
                            {
                                double absVal = System.Math.Abs(inverse[j, k]);
                                if (absVal > maxPivot)
                                {
                                    maxPivot = absVal;
                                    irow = j;
                                    icol = k;
                                }
                            }
                            else if (pivotIdx[k] > 0)
                            {
                                return mat;
                            }
                        }
                    }
                }

                ++(pivotIdx[icol]);
                // Swap rows over so pivot is on diagonal
                if (irow != icol)
                {
                    for (int k = 0; k < 4; ++k)
                    {
                        double f = inverse[irow, k];
                        inverse[irow, k] = inverse[icol, k];
                        inverse[icol, k] = f;
                    }
                }

                rowIdx[i] = irow;
                colIdx[i] = icol;
                double pivot = inverse[icol, icol];
                // check for singular matrix
                if (pivot == 0.0)
                {
                    throw new InvalidOperationException("Matrix is singular and cannot be inverted.");
                    //return mat;
                }

                // Scale row so it has a unit diagonal
                double oneOverPivot = 1.0 / pivot;
                inverse[icol, icol] = 1.0;
                for (int k = 0; k < 4; ++k)
                    inverse[icol, k] *= oneOverPivot;
                // Do elimination of non-diagonal elements
                for (int j = 0; j < 4; ++j)
                {
                    // check this isn't on the diagonal
                    if (icol != j)
                    {
                        double f = inverse[j, icol];
                        inverse[j, icol] = 0.0;
                        for (int k = 0; k < 4; ++k)
                            inverse[j, k] -= inverse[icol, k] * f;
                    }
                }
            }

            for (int j = 3; j >= 0; --j)
            {
                int ir = rowIdx[j];
                int ic = colIdx[j];
                for (int k = 0; k < 4; ++k)
                {
                    double f = inverse[k, ir];
                    inverse[k, ir] = inverse[k, ic];
                    inverse[k, ic] = f;
                }
            }

            mat.Row0 = new Vector4(inverse[0, 0], inverse[0, 1], inverse[0, 2], inverse[0, 3]);
            mat.Row1 = new Vector4(inverse[1, 0], inverse[1, 1], inverse[1, 2], inverse[1, 3]);
            mat.Row2 = new Vector4(inverse[2, 0], inverse[2, 1], inverse[2, 2], inverse[2, 3]);
            mat.Row3 = new Vector4(inverse[3, 0], inverse[3, 1], inverse[3, 2], inverse[3, 3]);
            return mat;
        }



        /// <summary>
        /// Calculate the transpose of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to transpose</param>
        /// <returns>The transpose of the given matrix</returns>
        public static Matrix4X4 Transpose(Matrix4X4 mat)
        {
            return new Matrix4X4(mat.Column0, mat.Column1, mat.Column2, mat.Column3);
        }


        /// <summary>
        /// Calculate the transpose of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to transpose</param>
        /// <param name="result">The result of the calculation</param>
        public static void Transpose(ref Matrix4X4 mat, out Matrix4X4 result)
        {
            result.Row0 = mat.Column0;
            result.Row1 = mat.Column1;
            result.Row2 = mat.Column2;
            result.Row3 = mat.Column3;
        }




        /// <summary>
        /// Matrix multiplication
        /// </summary>
        /// <param name="left">left-hand operand</param>
        /// <param name="right">right-hand operand</param>
        /// <returns>A new Matrix44 which holds the result of the multiplication</returns>
        public static Matrix4X4 operator *(Matrix4X4 left, Matrix4X4 right)
        {
            return Matrix4X4.Mult(left, right);
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left equals right; false otherwise.</returns>
        public static bool operator ==(Matrix4X4 left, Matrix4X4 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left does not equal right; false otherwise.</returns>
        public static bool operator !=(Matrix4X4 left, Matrix4X4 right)
        {
            return !left.Equals(right);
        }




        /// <summary>
        /// Returns a System.String that represents the current Matrix44.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}\n{1}\n{2}\n{3}", Row0, Row1, Row2, Row3);
        }



        /// <summary>
        /// Returns the hashcode for this instance.
        /// </summary>
        /// <returns>A System.Int32 containing the unique hashcode for this instance.</returns>
        public override int GetHashCode()
        {
            return new { Row0, Row1, Row2, Row3 }.GetHashCode();
        }



        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>True if the instances are equal; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Matrix4X4))
                return false;
            return this.Equals((Matrix4X4)obj);
        }





        /// <summary>Indicates whether the current matrix is equal to another matrix.</summary>
        /// <param name="other">An matrix to compare with this matrix.</param>
        /// <returns>true if the current matrix is equal to the matrix parameter; otherwise, false.</returns>
        public bool Equals(Matrix4X4 other)
        {
            return
                Row0 == other.Row0 &&
                Row1 == other.Row1 &&
                Row2 == other.Row2 &&
                Row3 == other.Row3;
        }


        public float[] GetAsFloatArray()
        {
            float[] contents = new float[16];
            contents[0] = (float)Row0[0]; contents[1] = (float)Row0[1]; contents[2] = (float)Row0[2]; contents[3] = (float)Row0[3];
            contents[4] = (float)Row1[0]; contents[5] = (float)Row1[1]; contents[6] = (float)Row1[2]; contents[7] = (float)Row1[3];
            contents[8] = (float)Row2[0]; contents[9] = (float)Row2[1]; contents[10] = (float)Row2[2]; contents[11] = (float)Row2[3];
            contents[12] = (float)Row3[0]; contents[13] = (float)Row3[1]; contents[14] = (float)Row3[2]; contents[15] = (float)Row3[3];
            return contents;
        }

        public double[] GetAsDoubleArray()
        {
            double[] contents = new double[16];
            contents[0] = Row0[0]; contents[1] = Row0[1]; contents[2] = Row0[2]; contents[3] = Row0[3];
            contents[4] = Row1[0]; contents[5] = Row1[1]; contents[6] = Row1[2]; contents[7] = Row1[3];
            contents[8] = Row2[0]; contents[9] = Row2[1]; contents[10] = Row2[2]; contents[11] = Row2[3];
            contents[12] = Row3[0]; contents[13] = Row3[1]; contents[14] = Row3[2]; contents[15] = Row3[3];
            return contents;
        }
    }
#endif
}