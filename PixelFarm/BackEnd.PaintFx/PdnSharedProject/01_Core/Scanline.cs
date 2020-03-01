/////////////////////////////////////////////////////////////////////////////////
// Paint.NET (MIT,from version 3.36.7, see=> https://github.com/rivy/OpenPDN   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

//MIT, 2017-present, WinterDev
namespace PaintFx
{
    public struct Scanline
    {
        public int X { get; private set; }

        public int Y { get; private set; }

        public int Length { get; private set; }

        public override int GetHashCode()
        {
            unchecked
            {
                return Length.GetHashCode() + X.GetHashCode() + Y.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Scanline)
            {
                Scanline rhs = (Scanline)obj;
                return X == rhs.X && Y == rhs.Y && Length == rhs.Length;
            }
            else
            {
                return false;
            }
        }

        public static bool operator ==(Scanline lhs, Scanline rhs)
        {
            return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Length == rhs.Length;
        }

        public static bool operator !=(Scanline lhs, Scanline rhs)
        {
            return !(lhs == rhs);
        }

        public override string ToString()
        {
            return "(" + X + "," + Y + "):[" + Length.ToString() + "]";
        }

        public Scanline(int x, int y, int length)
        {
            this.X = x;
            this.Y = y;
            this.Length = length;
        }
    }
}
