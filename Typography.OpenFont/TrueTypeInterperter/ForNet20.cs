//MIT, 2015, Michael Popoloski, WinterDev
/*
MIT License
Copyright Â© 2006 The Mono.Xna Team

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

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


namespace System.Numerics
{
    //FOR NET20

    public struct Vector2
    {
        static Vector2 zeroVector = new Vector2(0f, 0f);
        static Vector2 unitVector = new Vector2(1f, 1f);
        static Vector2 unitXVector = new Vector2(1f, 0f);
        static Vector2 unitYVector = new Vector2(0f, 1f);
        public float X;
        public float Y;
        public static readonly Vector2 Zero = new Vector2(0, 0);
        public Vector2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
        public static Vector2 operator +(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.X + v2.X, v1.Y + v2.Y);
        }
        public static Vector2 operator +(Vector2 v1, float n)
        {
            return new Vector2(v1.X + n, v1.Y + n);
        }

        public static Vector2 operator -(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.X - v2.X, v1.Y - v2.Y);
        }
        public static Vector2 operator /(Vector2 v1, float n)
        {
            return new Vector2(v1.X / n, v1.Y / n);
        }
        public static Vector2 operator *(Vector2 v1, float n)
        {
            return new Vector2(v1.X * n, v1.Y * n);
        }
        public static Vector2 operator *(float n, Vector2 v1)
        {
            return new Vector2(v1.X * n, v1.Y * n);
        }
        public static bool operator ==(Vector2 v1, Vector2 v2)
        {
            return (v1.X == v2.X) && (v1.Y == v2.Y);
        }
        public static bool operator !=(Vector2 v1, Vector2 v2)
        {
            return (v1.X != v2.X) || (v1.Y != v2.Y);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static Vector2 Min(Vector2 value1, Vector2 value2)
        {
            return new Vector2(
                (value1.X < value2.X) ? value1.X : value2.X,
                (value1.Y < value2.Y) ? value1.Y : value2.Y
                );
        }
        public static Vector2 Max(Vector2 value1, Vector2 value2)
        {
            return new Vector2(
                (value1.X > value2.X) ? value1.X : value2.X,
                (value1.Y > value2.Y) ? value1.Y : value2.Y
                );
        }
        public override bool Equals(object obj)
        {
            if (obj is Vector2)
            {
                Vector2 v2 = (Vector2)obj;
                return this == v2;
            }
            else
            {
                return false;
            }
        }
        public static Vector2 Normalize(Vector2 value)
        {
            double factor = 1.0f / (double)Math.Sqrt((value.X * value.X) + (value.Y * value.Y));
            return new Vector2((float)(value.X * factor), (float)(value.Y * factor));
            //value.X *= val;
            //value.Y *= val;
            //return value;
        }


        public static Vector2 One
        {
            get { return unitVector; }
        }

        public static Vector2 UnitX
        {
            get { return unitXVector; }
        }

        public static Vector2 UnitY
        {
            get { return unitYVector; }
        }
        public static double Dot(Vector2 value1, Vector2 value2)
        {
            return (value1.X * value2.X) + (value1.Y * value2.Y);
        }
        public double Length()
        {
            return (double)Math.Sqrt((X * X) + (Y * Y));
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


        public Vector2 Rotate(int degree)
        {
            double radian = degree * Math.PI / 180.0;
            double sin = Math.Sin(radian);
            double cos = Math.Cos(radian);
            double nx = X * cos - Y * sin;
            double ny = X * sin + Y * cos;

            return new Vector2((float)nx, (float)ny);
        }
        public Vector2 NewLength(double newLength)
        {
            //radian
            double atan = Math.Atan2(Y, X);
            return new Vector2(
                      (float)(Math.Cos(atan) * newLength),
                      (float)(Math.Sin(atan) * newLength));
        }

    }

    public struct Matrix3x2
    {
        /// <summary>
        /// Gets the identity matrix.
        /// </summary>
        /// <value>The identity matrix.</value>
        public readonly static Matrix3x2 Identity = new Matrix3x2(1, 0, 0, 1, 0, 0);
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
        public Matrix3x2(float value)
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
        public Matrix3x2(float M11, float M12, float M21, float M22, float M31, float M32)
        {
            this.M11 = M11; this.M12 = M12;
            this.M21 = M21; this.M22 = M22;
            this.M31 = M31; this.M32 = M32;
        }
    }
}