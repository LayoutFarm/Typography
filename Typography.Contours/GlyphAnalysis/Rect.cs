//
// System.Drawing.Rectangle.cs
//
// Author:
//   Mike Kestner (mkestner@speakeasy.net)
//
// Copyright (C) 2001 Mike Kestner
// Copyright (C) 2004 Novell, Inc.  http://www.novell.com 
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//



namespace Typography.Contours
{
    public struct Rectangle
    {
        private int x, y, width, height;
        /// <summary>
        ///	Empty Shared Field
        /// </summary>
        ///
        /// <remarks>
        ///	An uninitialized Rectangle Structure.
        /// </remarks>

        public static readonly Rectangle Empty;

        /// <summary>
        ///	FromLTRB Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a Rectangle structure from left, top, right,
        ///	and bottom coordinates.
        /// </remarks>

        public static Rectangle FromLTRB(int left, int top,
                          int right, int bottom)
        {
            return new Rectangle(left, top, right - left,
                          bottom - top);
        }



        /// <summary>
        ///	Rectangle Constructor
        /// </summary>
        ///
        /// <remarks>
        ///	Creates a Rectangle from a specified x,y location and
        ///	width and height values.
        /// </remarks>

        public Rectangle(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }



        /// <summary>
        ///	Bottom Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Y coordinate of the bottom edge of the Rectangle.
        ///	Read only.
        /// </remarks>


        public int Bottom
        {
            get
            {
                return y + height;
            }
        }

        /// <summary>
        ///	Height Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Height of the Rectangle.
        /// </remarks>

        public int Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }

        /// <summary>
        ///	IsEmpty Property
        /// </summary>
        ///
        /// <remarks>
        ///	Indicates if the width or height are zero. Read only.
        /// </remarks>		

        public bool IsEmpty
        {
            get
            {
                return ((x == 0) && (y == 0) && (width == 0) && (height == 0));
            }
        }

        /// <summary>
        ///	Left Property
        /// </summary>
        ///
        /// <remarks>
        ///	The X coordinate of the left edge of the Rectangle.
        ///	Read only.
        /// </remarks>

        public int Left
        {
            get
            {
                return X;
            }
        }

        /// <summary>
        ///	Right Property
        /// </summary>
        ///
        /// <remarks>
        ///	The X coordinate of the right edge of the Rectangle.
        ///	Read only.
        /// </remarks>


        public int Right
        {
            get
            {
                return X + Width;
            }
        }


        /// <summary>
        ///	Top Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Y coordinate of the top edge of the Rectangle.
        ///	Read only.
        /// </remarks>


        public int Top
        {
            get
            {
                return y;
            }
        }

        /// <summary>
        ///	Width Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Width of the Rectangle.
        /// </remarks>

        public int Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }

        /// <summary>
        ///	X Property
        /// </summary>
        ///
        /// <remarks>
        ///	The X coordinate of the Rectangle.
        /// </remarks>

        public int X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        /// <summary>
        ///	Y Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Y coordinate of the Rectangle.
        /// </remarks>

        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }

        /// <summary>
        ///	Contains Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks if an x,y coordinate lies within this Rectangle.
        /// </remarks>

        public bool Contains(int x, int y)
        {
            return ((x >= Left) && (x < Right) &&
                (y >= Top) && (y < Bottom));
        }





        /// <summary>
        ///	IntersectsWith Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks if a Rectangle intersects with this one.
        /// </remarks>

        public bool IntersectsWith(Rectangle rect)
        {
            return !((Left >= rect.Right) || (Right <= rect.Left) ||
                (Top >= rect.Bottom) || (Bottom <= rect.Top));
        }
        public bool IntersectsWith(int left, int top, int right, int bottom)
        {
            if (((this.Left <= left) && (this.Right > left)) || ((this.Left >= left) && (this.Left < right)))
            {
                if (((this.Top <= top) && (this.Bottom > top)) || ((this.Top >= top) && (this.Top < bottom)))
                {
                    return true;
                }
            }
            return false;
        }
        private bool IntersectsWithInclusive(Rectangle r)
        {
            return !((Left > r.Right) || (Right < r.Left) ||
                (Top > r.Bottom) || (Bottom < r.Top));
        }

        /// <summary>
        ///	Offset Method
        /// </summary>
        ///
        /// <remarks>
        ///	Moves the Rectangle a specified distance.
        /// </remarks>

        public void Offset(int x, int y)
        {
            this.x += x;
            this.y += y;
        }

#if DEBUG
        public override string ToString()
        {
            return "(" + x + "," + y + "w:" + width + ",h:" + height + ")";
        }
#endif

    }

    public struct RectangleF
    {
        float x, y, width, height;
        /// <summary>
        ///	Empty Shared Field
        /// </summary>
        ///
        /// <remarks>
        ///	An uninitialized RectangleF Structure.
        /// </remarks>

        public static readonly RectangleF Empty;
        /// <summary>
        ///	FromLTRB Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a RectangleF structure from left, top, right,
        ///	and bottom coordinates.
        /// </remarks>

        public static RectangleF FromLTRB(float left, float top,
                           float right, float bottom)
        {
            return new RectangleF(left, top, right - left, bottom - top);
        }


        /// <summary>
        ///	RectangleF Constructor
        /// </summary>
        ///
        /// <remarks>
        ///	Creates a RectangleF from a specified x,y location and
        ///	width and height values.
        /// </remarks>

        public RectangleF(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
        /// <summary>
        ///	Bottom Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Y coordinate of the bottom edge of the RectangleF.
        ///	Read only.
        /// </remarks> 

        public float Bottom
        {
            get
            {
                return Y + Height;
            }
        }

        /// <summary>
        ///	Height Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Height of the RectangleF.
        /// </remarks>

        public float Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }

        /// <summary>
        ///	IsEmpty Property
        /// </summary>
        ///
        /// <remarks>
        ///	Indicates if the width or height are zero. Read only.
        /// </remarks>
        //		

        public bool IsEmpty
        {
            get
            {
                return (width <= 0 || height <= 0);
            }
        }

        /// <summary>
        ///	Left Property
        /// </summary>
        ///
        /// <remarks>
        ///	The X coordinate of the left edge of the RectangleF.
        ///	Read only.
        /// </remarks>


        public float Left
        {
            get
            {
                return X;
            }
        }



        /// <summary>
        ///	Right Property
        /// </summary>
        ///
        /// <remarks>
        ///	The X coordinate of the right edge of the RectangleF.
        ///	Read only.
        /// </remarks>


        public float Right
        {
            get
            {
                return X + Width;
            }
        }


        /// <summary>
        ///	Top Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Y coordinate of the top edge of the RectangleF.
        ///	Read only.
        /// </remarks>


        public float Top
        {
            get
            {
                return Y;
            }
        }

        /// <summary>
        ///	Width Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Width of the RectangleF.
        /// </remarks>

        public float Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }

        /// <summary>
        ///	X Property
        /// </summary>
        ///
        /// <remarks>
        ///	The X coordinate of the RectangleF.
        /// </remarks>

        public float X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        /// <summary>
        ///	Y Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Y coordinate of the RectangleF.
        /// </remarks>

        public float Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }

        /// <summary>
        ///	Contains Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks if an x,y coordinate lies within this RectangleF.
        /// </remarks>

        public bool Contains(float x, float y)
        {
            return ((x >= Left) && (x < Right) &&
                (y >= Top) && (y < Bottom));
        }



        /// <summary>
        ///	Contains Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks if a RectangleF lies entirely within this 
        ///	RectangleF.
        /// </remarks>

        public bool Contains(RectangleF rect)
        {
            return X <= rect.X && Right >= rect.Right && Y <= rect.Y && Bottom >= rect.Bottom;
        }


    }

}