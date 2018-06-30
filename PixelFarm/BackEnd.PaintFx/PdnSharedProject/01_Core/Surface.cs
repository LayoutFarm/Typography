/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

//Apache2, 2017-present, WinterDev
using System;
using PixelFarm.Drawing;

namespace PaintFx
{

    class MemHolder
    {
        unsafe int* memAddress;
        int len; //len of int32 array 
        /// <param name="ptr">ptr to int32*</param>
        /// <param name="len">length of this int32[]</param>
        public MemHolder(IntPtr ptr, int len)
        {
            this.len = len;
            unsafe
            {
                this.memAddress = (int*)ptr;
            }
        }
        /// <summary>
        /// len of int32[] array
        /// </summary>
        internal int Length
        {
            get { return len; }
        }
        internal IntPtr Ptr
        {
            get
            {
                unsafe
                {
                    return (IntPtr)memAddress;
                }
            }
        }
        public MemHolder CreateSubMem(int startOffset, int len)
        {
            if (startOffset >= 0 && len <= this.len)
            {
                unsafe
                {
                    return new MemHolder(
                        (IntPtr)(memAddress + startOffset),
                        len);
                }
            }
            return null;
        }
    }
    /// <summary>
    /// This is our Surface type. We allocate our own blocks of memory for this,
    /// and provide ways to create a GDI+ Bitmap object that aliases our surface.
    /// That way we can do everything fast, in memory and have complete control,
    /// and still have the ability to use GDI+ for drawing and rendering where
    /// appropriate.
    /// </summary>

    public sealed class Surface : IDisposable
    {

        int width;
        int height;
        int stride;
        MemHolder memHolder; 
        bool disposed = false;

        /// <summary>
        /// Creates a new instance of the Surface class.
        /// </summary>
        /// <param name="width">The width, in pixels, of the new Surface.</param>
        /// <param name="height">The height, in pixels, of the new Surface.</param>
        internal Surface(int stride, int width, int height, MemHolder memHolder)
        {
            this.stride = stride;
            this.width = width;
            this.height = height;
            this.memHolder = memHolder; //mem buffer 
            //try
            //{
            //    stride = checked(width * ColorBgra.SizeOf);
            //    bytes = (long)height * (long)stride;
            //}

            //catch (OverflowException ex)
            //{
            //    throw new OutOfMemoryException("Dimensions are too large - not enough memory, width=" + width.ToString() + ", height=" + height.ToString(), ex);
            //}

            //MemoryBlock scan0 = new MemoryBlock(width, height);
            //Create(width, height, stride, scan0);
        }
        public bool IsDisposed
        {
            get
            {
                return this.disposed;
            }
        }


        /// <summary>
        /// Gets the width, in pixels, of this Surface.
        /// </summary>
        /// <remarks>
        /// This property will never throw an ObjectDisposedException.
        /// </remarks>
        public int Width
        {
            get
            {
                return this.width;
            }
        }

        /// <summary>
        /// Gets the height, in pixels, of this Surface.
        /// </summary>
        /// <remarks>
        /// This property will never throw an ObjectDisposedException.
        /// </remarks>
        public int Height
        {
            get
            {
                return this.height;
            }
        }

        /// <summary>
        /// Gets the stride, in bytes, for this Surface.
        /// </summary>
        /// <remarks>
        /// Stride is defined as the number of bytes between the beginning of a row and
        /// the beginning of the next row. Thus, in loose C notation: stride = (byte *)&this[0, 1] - (byte *)&this[0, 0].
        /// Stride will always be equal to <b>or greater than</b> Width * ColorBgra.SizeOf.
        /// This property will never throw an ObjectDisposedException.
        /// </remarks>
        public int Stride
        {
            get
            {
                return this.stride;
            }
        }


        /// <summary>
        /// Gets the bounds of this Surface, in pixels.
        /// </summary>
        /// <remarks>
        /// This is a convenience function that returns Rectangle(0, 0, Width, Height).
        /// This property will never throw an ObjectDisposedException.
        /// </remarks>
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(0, 0, width, height);
            }
        }

        ///// <summary>
        ///// Creates a new instance of the Surface class.
        ///// </summary>
        ///// <param name="size">The size, in pixels, of the new Surface.</param>
        //public Surface(Size size)
        //    : this(size.Width, size.Height)
        //{
        //}



        ///// <summary>
        ///// Creates a new instance of the Surface class that reuses a block of memory that was previously allocated.
        ///// </summary>
        ///// <param name="width">The width, in pixels, for the Surface.</param>
        ///// <param name="height">The height, in pixels, for the Surface.</param>
        ///// <param name="stride">The stride, in bytes, for the Surface.</param>
        ///// <param name="scan0">The MemoryBlock to use. The beginning of this buffer defines the upper left (0, 0) pixel of the Surface.</param>
        //private Surface(int width, int height, int stride, MemoryBlock scan0)
        //{
        //    Create(width, height, stride, scan0);
        //}

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "width")]
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "height")]
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "stride")]
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "scan0")]
        //private void Create(int width, int height, int stride, MemoryBlock scan0)
        //{
        //    this.width = width;
        //    this.height = height;
        //    this.stride = stride;
        //    this.scan0 = scan0;
        //}

        ~Surface()
        {
            Dispose(false);
        }

        /// <summary>
        /// Creates a Surface that aliases a portion of this Surface.
        /// </summary>
        /// <param name="bounds">The portion of this Surface that will be aliased.</param>
        /// <remarks>The upper left corner of the new Surface will correspond to the 
        /// upper left corner of this rectangle in the original Surface.</remarks>
        /// <returns>A Surface that aliases the requested portion of this Surface.</returns>
        public Surface CreateWindow(Rectangle bounds)
        {
            return CreateWindow(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        public Surface CreateWindow(int x, int y, int windowWidth, int windowHeight)
        {

            //find start point
            //1. windowWidth in 'pixel' unit 
            //2. we use only 32 bit version of the pixel
            //   so stride = pixelWidth *4
            //---------------------



            //if (disposed)
            //{
            //    throw new ObjectDisposedException("Surface");
            //}

            //if (windowHeight == 0)
            //{
            //    throw new ArgumentOutOfRangeException("windowHeight", "must be greater than zero");
            //}

            Rectangle original = this.Bounds;
            Rectangle sub = new Rectangle(x, y, windowWidth, windowHeight);
            Rectangle clipped = Rectangle.Intersect(original, sub);

            if (clipped != sub)
            {
                throw new ArgumentOutOfRangeException("bounds", new Rectangle(x, y, windowWidth, windowHeight),
                    "bounds parameters must be a subset of this Surface's bounds");
            }

            int startPos = (windowWidth * y) + x;
            int endPos = (windowWidth * (y + windowHeight)) + (x + windowWidth);
            //also check if 
            MemHolder newMemHolder = this.memHolder.CreateSubMem(startPos, endPos - startPos + 1);
            return new Surface(windowWidth * 4, windowWidth, windowHeight, newMemHolder);


            //long offset = ((long)stride * (long)y) + ((long)ColorBgra.SizeOf * (long)x);
            //long length = ((windowHeight - 1) * (long)stride) + (long)windowWidth * (long)ColorBgra.SizeOf;
            //MemoryBlock block = new MemoryBlock(this.scan0, offset, length);
            //return new Surface(windowWidth, windowHeight, this.stride, block);
        }

        /// <summary>
        /// Gets the offset, in bytes, of the requested row from the start of the surface.
        /// </summary>
        /// <param name="y">The row.</param>
        /// <returns>The number of bytes between (0,0) and (0,y).</returns>
        public long GetRowByteOffset(int y)
        {
            if (y < 0 || y >= height)
            {
                throw new ArgumentOutOfRangeException("y", "Out of bounds: y=" + y.ToString());
            }

            return (long)y * (long)stride;
        }

        /// <summary>
        /// Gets the offset, in bytes, of the requested row from the start of the surface.
        /// </summary>
        /// <param name="y">The row.</param>
        /// <returns>The number of bytes between (0,0) and (0,y)</returns>
        /// <remarks>
        /// This method does not do any bounds checking and is potentially unsafe to use,
        /// but faster than GetRowByteOffset().
        /// </remarks>
        public unsafe long GetRowByteOffsetUnchecked(int y)
        {
            //#if DEBUG
            //            if (y < 0 || y >= this.height)
            //            {
            //                Tracing.Ping("y=" + y.ToString() + " is out of bounds of [0, " + this.height.ToString() + ")");
            //            }
            //#endif

            return (long)y * (long)stride;
        }

        /// <summary>
        /// Gets a pointer to the beginning of the requested row in the surface.
        /// </summary>
        /// <param name="y">The row</param>
        /// <returns>A pointer that references (0,y) in this surface.</returns>
        /// <remarks>Since this returns a pointer, it is potentially unsafe to use.</remarks>
        public unsafe ColorBgra* GetRowAddress(int y)
        {
            return (ColorBgra*)(((byte*)this.memHolder.Ptr) + GetRowByteOffset(y));
        }

        /// <summary>
        /// Gets a pointer to the beginning of the requested row in the surface.
        /// </summary>
        /// <param name="y">The row</param>
        /// <returns>A pointer that references (0,y) in this surface.</returns>
        /// <remarks>
        /// This method does not do any bounds checking and is potentially unsafe to use,
        /// but faster than GetRowAddress().
        /// </remarks>
        public unsafe ColorBgra* GetRowAddressUnchecked(int y)
        {

            //#if DEBUG
            //            if (y < 0 || y >= this.height)
            //            {
            //                Tracing.Ping("y=" + y.ToString() + " is out of bounds of [0, " + this.height.ToString() + ")");
            //            }
            //#endif

            return (ColorBgra*)(((byte*)this.memHolder.Ptr)) + GetRowByteOffsetUnchecked(y);
        }

        /// <summary>
        /// Gets the number of bytes from the beginning of a row to the requested column.
        /// </summary>
        /// <param name="x">The column.</param>
        /// <returns>
        /// The number of bytes between (0,n) and (x,n) where n is in the range [0, Height).
        /// </returns>
        public long GetColumnByteOffset(int x)
        {
            if (x < 0 || x >= this.width)
            {
                throw new ArgumentOutOfRangeException("x", x, "Out of bounds");
            }

            return (long)x * (long)ColorBgra.SizeOf;
        }

        /// <summary>
        /// Gets the number of bytes from the beginning of a row to the requested column.
        /// </summary>
        /// <param name="x">The column.</param>
        /// <returns>
        /// The number of bytes between (0,n) and (x,n) where n is in the range [0, Height).
        /// </returns>
        /// <remarks>
        /// This method does not do any bounds checking and is potentially unsafe to use,
        /// but faster than GetColumnByteOffset().
        /// </remarks>
        public long GetColumnByteOffsetUnchecked(int x)
        {
            //#if DEBUG
            //            if (x < 0 || x >= this.width)
            //            {
            //                Tracing.Ping("x=" + x.ToString() + " is out of bounds of [0, " + this.width.ToString() + ")");
            //            }
            //#endif

            return (long)x * (long)ColorBgra.SizeOf;
        }

        /// <summary>
        /// Gets the number of bytes from the beginning of the surface's buffer to
        /// the requested point.
        /// </summary>
        /// <param name="x">The x offset.</param>
        /// <param name="y">The y offset.</param>
        /// <returns>
        /// The number of bytes between (0,0) and (x,y).
        /// </returns>
        public long GetPointByteOffset(int x, int y)
        {
            return GetRowByteOffset(y) + GetColumnByteOffset(x);
        }

        /// <summary>
        /// Gets the number of bytes from the beginning of the surface's buffer to
        /// the requested point.
        /// </summary>
        /// <param name="x">The x offset.</param>
        /// <param name="y">The y offset.</param>
        /// <returns>
        /// The number of bytes between (0,0) and (x,y).
        /// </returns>
        /// <remarks>
        /// This method does not do any bounds checking and is potentially unsafe to use,
        /// but faster than GetPointByteOffset().
        /// </remarks>
        public long GetPointByteOffsetUnchecked(int x, int y)
        {
            //#if DEBUG
            //            if (x < 0 || x >= this.width)
            //            {
            //                Tracing.Ping("x=" + x.ToString() + " is out of bounds of [0, " + this.width.ToString() + ")");
            //            }

            //            if (y < 0 || y >= this.height)
            //            {
            //                Tracing.Ping("y=" + y.ToString() + " is out of bounds of [0, " + this.height.ToString() + ")");
            //            }
            //#endif

            return GetRowByteOffsetUnchecked(y) + GetColumnByteOffsetUnchecked(x);
        }

        /// <summary>
        /// Gets the color at a specified point in the surface.
        /// </summary>
        /// <param name="x">The x offset.</param>
        /// <param name="y">The y offset.</param>
        /// <returns>The color at the requested location.</returns>
        public ColorBgra GetPoint(int x, int y)
        {
            return this[x, y];
        }

        /// <summary>
        /// Gets the color at a specified point in the surface.
        /// </summary>
        /// <param name="x">The x offset.</param>
        /// <param name="y">The y offset.</param>
        /// <returns>The color at the requested location.</returns>
        /// <remarks>
        /// This method does not do any bounds checking and is potentially unsafe to use,
        /// but faster than GetPoint().
        /// </remarks>
        public unsafe ColorBgra GetPointUnchecked(int x, int y)
        {


            //#if DEBUG
            //            if (x < 0 || x >= this.width)
            //            {
            //                Tracing.Ping("x=" + x.ToString() + " is out of bounds of [0, " + this.width.ToString() + ")");
            //            }

            //            if (y < 0 || y >= this.height)
            //            {
            //                Tracing.Ping("y=" + y.ToString() + " is out of bounds of [0, " + this.height.ToString() + ")");
            //            }
            //#endif

            return *(x + (ColorBgra*)(((byte*)memHolder.Ptr) + (y * stride)));
        }

        /// <summary>
        /// Gets the color at a specified point in the surface.
        /// </summary>
        /// <param name="pt">The point to retrieve.</param>
        /// <returns>The color at the requested location.</returns>
        /// <remarks>
        /// This method does not do any bounds checking and is potentially unsafe to use,
        /// but faster than GetPoint().
        /// </remarks>
        public unsafe ColorBgra GetPointUnchecked(Point pt)
        {
            return GetPointUnchecked(pt.X, pt.Y);
        }

        /// <summary>
        /// Gets the address in memory of the requested point.
        /// </summary>
        /// <param name="x">The x offset.</param>
        /// <param name="y">The y offset.</param>
        /// <returns>A pointer to the requested point in the surface.</returns>
        /// <remarks>Since this method returns a pointer, it is potentially unsafe to use.</remarks>
        public unsafe ColorBgra* GetPointAddress(int x, int y)
        {
            if (x < 0 || x >= Width)
            {
                throw new ArgumentOutOfRangeException("x", "Out of bounds: x=" + x.ToString());
            }

            return GetRowAddress(y) + x;
        }

        /// <summary>
        /// Gets the address in memory of the requested point.
        /// </summary>
        /// <param name="pt">The point to retrieve.</param>
        /// <returns>A pointer to the requested point in the surface.</returns>
        /// <remarks>Since this method returns a pointer, it is potentially unsafe to use.</remarks>
        public unsafe ColorBgra* GetPointAddress(Point pt)
        {
            return GetPointAddress(pt.X, pt.Y);
        }

        /// <summary>
        /// Gets the address in memory of the requested point.
        /// </summary>
        /// <param name="x">The x offset.</param>
        /// <param name="y">The y offset.</param>
        /// <returns>A pointer to the requested point in the surface.</returns>
        /// <remarks>
        /// This method does not do any bounds checking and is potentially unsafe to use,
        /// but faster than GetPointAddress().
        /// </remarks>
        public unsafe ColorBgra* GetPointAddressUnchecked(int x, int y)
        {


            //#if DEBUG
            //            if (x < 0 || x >= this.width)
            //            {
            //                Tracing.Ping("x=" + x.ToString() + " is out of bounds of [0, " + this.width.ToString() + ")");
            //            }

            //            if (y < 0 || y >= this.height)
            //            {
            //                Tracing.Ping("y=" + y.ToString() + " is out of bounds of [0, " + this.height.ToString() + ")");
            //            }
            //#endif

            return unchecked(x + (ColorBgra*)(((byte*)memHolder.Ptr) + (y * stride)));
        }

        /// <summary>
        /// Gets the address in memory of the requested point.
        /// </summary>
        /// <param name="pt">The point to retrieve.</param>
        /// <returns>A pointer to the requested point in the surface.</returns>
        /// <remarks>
        /// This method does not do any bounds checking and is potentially unsafe to use,
        /// but faster than GetPointAddress().
        /// </remarks>
        public unsafe ColorBgra* GetPointAddressUnchecked(Point pt)
        {
            return GetPointAddressUnchecked(pt.X, pt.Y);
        }

        ///// <summary>
        ///// Gets a MemoryBlock that references the row requested.
        ///// </summary>
        ///// <param name="y">The row.</param>
        ///// <returns>A MemoryBlock that gives access to the bytes in the specified row.</returns>
        ///// <remarks>This method is the safest to use for direct memory access to a row's pixel data.</remarks>
        //public MemoryBlock GetRow(int y)
        //{
        //    return new MemoryBlock(scan0, GetRowByteOffset(y), (long)width * (long)ColorBgra.SizeOf);
        //}

        public bool IsContiguousMemoryRegion(Rectangle bounds)
        {
            bool oneRow = (bounds.Height == 1);
            bool manyRows = (this.Stride == (this.Width * ColorBgra.SizeOf) &&
                this.Width == bounds.Width);

            return oneRow || manyRows;
        }

        /// <summary>
        /// Determines if the requested pixel coordinate is within bounds.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <returns>true if (x,y) is in bounds, false if it's not.</returns>
        public bool IsVisible(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        /// <summary>
        /// Determines if the requested pixel coordinate is within bounds.
        /// </summary>
        /// <param name="pt">The coordinate.</param>
        /// <returns>true if (pt.X, pt.Y) is in bounds, false if it's not.</returns>
        public bool IsVisible(Point pt)
        {
            return IsVisible(pt.X, pt.Y);
        }

        /// <summary>
        /// Determines if the requested row offset is within bounds.
        /// </summary>
        /// <param name="y">The row.</param>
        /// <returns>true if y &gt;= 0 and y &lt; height, otherwise false</returns>
        public bool IsRowVisible(int y)
        {
            return y >= 0 && y < Height;
        }

        /// <summary>
        /// Determines if the requested column offset is within bounds.
        /// </summary>
        /// <param name="x">The column.</param>
        /// <returns>true if x &gt;= 0 and x &lt; width, otherwise false.</returns>
        public bool IsColumnVisible(int x)
        {
            return x >= 0 && x < Width;
        }


        public ColorBgra GetBilinearSampleWrapped(float x, float y)
        {
            if (!PixelUtils.IsNumber(x) || !PixelUtils.IsNumber(y))
            {
                return ColorBgra.Transparent;
            }

            float u = x;
            float v = y;

            unchecked
            {
                int iu = (int)Math.Floor(u);
                uint sxfrac = (uint)(256 * (u - (float)iu));
                uint sxfracinv = 256 - sxfrac;

                int iv = (int)Math.Floor(v);
                uint syfrac = (uint)(256 * (v - (float)iv));
                uint syfracinv = 256 - syfrac;

                uint wul = (uint)(sxfracinv * syfracinv);
                uint wur = (uint)(sxfrac * syfracinv);
                uint wll = (uint)(sxfracinv * syfrac);
                uint wlr = (uint)(sxfrac * syfrac);

                int sx = iu;
                if (sx < 0)
                {
                    sx = (width - 1) + ((sx + 1) % width);
                }
                else if (sx > (width - 1))
                {
                    sx = sx % width;
                }

                int sy = iv;
                if (sy < 0)
                {
                    sy = (height - 1) + ((sy + 1) % height);
                }
                else if (sy > (height - 1))
                {
                    sy = sy % height;
                }

                int sleft = sx;
                int sright;

                if (sleft == (width - 1))
                {
                    sright = 0;
                }
                else
                {
                    sright = sleft + 1;
                }

                int stop = sy;
                int sbottom;

                if (stop == (height - 1))
                {
                    sbottom = 0;
                }
                else
                {
                    sbottom = stop + 1;
                }

                ColorBgra cul = GetPointUnchecked(sleft, stop);
                ColorBgra cur = GetPointUnchecked(sright, stop);
                ColorBgra cll = GetPointUnchecked(sleft, sbottom);
                ColorBgra clr = GetPointUnchecked(sright, sbottom);

                ColorBgra c = ColorBgra.BlendColors4W16IP(cul, wul, cur, wur, cll, wll, clr, wlr);

                return c;
            }
        }


        public unsafe ColorBgra GetBilinearSample(float x, float y)
        {
            if (!PixelUtils.IsNumber(x) || !PixelUtils.IsNumber(y))
            {
                return ColorBgra.Transparent;
            }

            float u = x;
            float v = y;

            if (u >= 0 && v >= 0 && u < width && v < height)
            {
                unchecked
                {
                    int iu = (int)Math.Floor(u);
                    uint sxfrac = (uint)(256 * (u - (float)iu));
                    uint sxfracinv = 256 - sxfrac;

                    int iv = (int)Math.Floor(v);
                    uint syfrac = (uint)(256 * (v - (float)iv));
                    uint syfracinv = 256 - syfrac;

                    uint wul = (uint)(sxfracinv * syfracinv);
                    uint wur = (uint)(sxfrac * syfracinv);
                    uint wll = (uint)(sxfracinv * syfrac);
                    uint wlr = (uint)(sxfrac * syfrac);

                    int sx = iu;
                    int sy = iv;
                    int sleft = sx;
                    int sright;

                    if (sleft == (width - 1))
                    {
                        sright = sleft;
                    }
                    else
                    {
                        sright = sleft + 1;
                    }

                    int stop = sy;
                    int sbottom;

                    if (stop == (height - 1))
                    {
                        sbottom = stop;
                    }
                    else
                    {
                        sbottom = stop + 1;
                    }

                    ColorBgra* cul = GetPointAddressUnchecked(sleft, stop);
                    ColorBgra* cur = cul + (sright - sleft);
                    ColorBgra* cll = GetPointAddressUnchecked(sleft, sbottom);
                    ColorBgra* clr = cll + (sright - sleft);

                    ColorBgra c = ColorBgra.BlendColors4W16IP(*cul, wul, *cur, wur, *cll, wll, *clr, wlr);
                    return c;
                }
            }
            else
            {
                return ColorBgra.FromUInt32(0);
            }
        }


        public unsafe ColorBgra GetBilinearSampleClamped(float x, float y)
        {
            if (!PixelUtils.IsNumber(x) || !PixelUtils.IsNumber(y))
            {
                return ColorBgra.Transparent;
            }

            float u = x;
            float v = y;

            if (u < 0)
            {
                u = 0;
            }
            else if (u > this.Width - 1)
            {
                u = this.Width - 1;
            }

            if (v < 0)
            {
                v = 0;
            }
            else if (v > this.Height - 1)
            {
                v = this.Height - 1;
            }

            unchecked
            {
                int iu = (int)Math.Floor(u);
                uint sxfrac = (uint)(256 * (u - (float)iu));
                uint sxfracinv = 256 - sxfrac;

                int iv = (int)Math.Floor(v);
                uint syfrac = (uint)(256 * (v - (float)iv));
                uint syfracinv = 256 - syfrac;

                uint wul = (uint)(sxfracinv * syfracinv);
                uint wur = (uint)(sxfrac * syfracinv);
                uint wll = (uint)(sxfracinv * syfrac);
                uint wlr = (uint)(sxfrac * syfrac);

                int sx = iu;
                int sy = iv;
                int sleft = sx;
                int sright;

                if (sleft == (width - 1))
                {
                    sright = sleft;
                }
                else
                {
                    sright = sleft + 1;
                }

                int stop = sy;
                int sbottom;

                if (stop == (height - 1))
                {
                    sbottom = stop;
                }
                else
                {
                    sbottom = stop + 1;
                }

                ColorBgra* cul = GetPointAddressUnchecked(sleft, stop);
                ColorBgra* cur = cul + (sright - sleft);
                ColorBgra* cll = GetPointAddressUnchecked(sleft, sbottom);
                ColorBgra* clr = cll + (sright - sleft);

                ColorBgra c = ColorBgra.BlendColors4W16IP(*cul, wul, *cur, wur, *cll, wll, *clr, wlr);
                return c;
            }
        }

        /// <summary>
        /// Gets or sets the pixel value at the requested offset.
        /// </summary>
        /// <remarks>
        /// This property is implemented with correctness and error checking in mind. If performance
        /// is a concern, do not use it.
        /// </remarks>
        public ColorBgra this[int x, int y]
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Surface");
                }

                if (x < 0 || y < 0 || x >= this.width || y >= this.height)
                {
                    throw new ArgumentOutOfRangeException("(x,y)", new Point(x, y), "Coordinates out of range, max=" + new Size(width - 1, height - 1).ToString());
                }

                unsafe
                {
                    return *GetPointAddressUnchecked(x, y);
                }
            }

            set
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Surface");
                }

                if (x < 0 || y < 0 || x >= this.width || y >= this.height)
                {
                    throw new ArgumentOutOfRangeException("(x,y)", new Point(x, y), "Coordinates out of range, max=" + new Size(width - 1, height - 1).ToString());
                }

                unsafe
                {
                    *GetPointAddressUnchecked(x, y) = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the pixel value at the requested offset.
        /// </summary>
        /// <remarks>
        /// This property is implemented with correctness and error checking in mind. If performance
        /// is a concern, do not use it.
        /// </remarks>
        public ColorBgra this[Point pt]
        {
            get
            {
                return this[pt.X, pt.Y];
            }

            set
            {
                this[pt.X, pt.Y] = value;
            }
        }


        ///// <summary>
        ///// Creates a new Surface and copies the pixels from a Bitmap to it.
        ///// </summary>
        ///// <param name="bitmap">The Bitmap to duplicate.</param>
        ///// <returns>A new Surface that is the same size as the given Bitmap and that has the same pixel values.</returns>
        //public static Surface CopyFromBitmap(Bitmap bitmap)
        //{
        //    throw new StillNotPortedException();
        //    //Surface surface = new Surface(bitmap.Width, bitmap.Height);
        //    //BitmapData bd = bitmap.LockBits(surface.Bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        //    //unsafe
        //    //{
        //    //    for (int y = 0; y < bd.Height; ++y)
        //    //    {
        //    //        PlatformMemory.Copy((void*)surface.GetRowAddress(y),
        //    //           (byte*)bd.Scan0.ToPointer() + (y * bd.Stride), (ulong)bd.Width * ColorBgra.SizeOf);
        //    //    }
        //    //}

        //    //bitmap.UnlockBits(bd);
        //    //return surface;
        //}
        //public Bitmap CreateAliasedBitmap(Rectangle bounds, bool alpha)
        //{
        //    throw new StillNotPortedException();
        //    //if (disposed)
        //    //{
        //    //    throw new ObjectDisposedException("Surface");
        //    //}

        //    //if (bounds.IsEmpty)
        //    //{
        //    //    throw new ArgumentOutOfRangeException();
        //    //}

        //    //Rectangle clipped = Rectangle.Intersect(this.Bounds, bounds);

        //    //if (clipped != bounds)
        //    //{
        //    //    throw new ArgumentOutOfRangeException();
        //    //}

        //    //unsafe
        //    //{
        //    //    return new Bitmap(bounds.Width, bounds.Height, stride, alpha ? this.PixelFormat : PixelFormat.Format32bppRgb,
        //    //        new IntPtr((void*)((byte*)scan0.VoidStar + GetPointByteOffsetUnchecked(bounds.X, bounds.Y))));
        //    //}

        //}
        /// <summary>
        /// Copies the contents of the given surface to the upper left corner of this surface.
        /// </summary>
        /// <param name="source">The surface to copy pixels from.</param>
        /// <remarks>
        /// The source surface does not need to have the same dimensions as this surface. Clipping
        /// will be handled automatically. No resizing will be done.
        /// </remarks>
        public void CopySurface(Surface source)
        {


            Surface ss = (Surface)source;
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            if (this.stride == ss.stride &&
                (this.width * ColorBgra.SizeOf) == this.stride &&
                this.width == ss.width &&
                this.height == ss.height)
            {
                unsafe
                {
                    PlatformMemory.Copy((byte*)source.memHolder.Ptr,
                               (void*)ss.memHolder.Ptr,
                                ((ulong)(height - 1) * (ulong)stride) + ((ulong)width * (ulong)ColorBgra.SizeOf));
                }
            }
            else
            {
                int copyWidth = Math.Min(width, ss.width);
                int copyHeight = Math.Min(height, ss.height);

                unsafe
                {
                    for (int y = 0; y < copyHeight; ++y)
                    {
                        PlatformMemory.Copy(GetRowAddressUnchecked(y), source.GetRowAddressUnchecked(y), (ulong)copyWidth * (ulong)ColorBgra.SizeOf);
                    }
                }
            }
        }

        /// <summary>
        /// Copies the contents of the given surface to a location within this surface.
        /// </summary>
        /// <param name="source">The surface to copy pixels from.</param>
        /// <param name="dstOffset">
        /// The offset within this surface to start copying pixels to. This will map to (0,0) in the source.
        /// </param>
        /// <remarks>
        /// The source surface does not need to have the same dimensions as this surface. Clipping
        /// will be handled automatically. No resizing will be done.
        /// </remarks>
        public void CopySurface(Surface source, Point dstOffset)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            Rectangle dstRect = new Rectangle(dstOffset, source.Size);
            dstRect.Intersect(Bounds);

            if (dstRect.Width == 0 || dstRect.Height == 0)
            {
                return;
            }

            Point sourceOffset = new Point(dstRect.Location.X - dstOffset.X, dstRect.Location.Y - dstOffset.Y);
            Rectangle sourceRect = new Rectangle(sourceOffset, dstRect.Size);
            Surface sourceWindow = (Surface)source.CreateWindow(sourceRect);
            Surface dstWindow = (Surface)this.CreateWindow(dstRect);
            dstWindow.CopySurface(sourceWindow);

            dstWindow.Dispose();
            sourceWindow.Dispose();
        }
        public Size Size
        {
            get { return new Size(this.Width, this.Height); }
        }
        ///// <summary>
        ///// Helper function. Same as calling CreateAliasedBounds(Bounds).
        ///// </summary>
        ///// <returns>A GDI+ Bitmap that aliases the entire Surface.</returns>
        //public Bitmap CreateAliasedBitmap()
        //{
        //    return CreateAliasedBitmap(this.Bounds);
        //}

        ///// <summary>
        ///// Helper function. Same as calling CreateAliasedBounds(bounds, true).
        ///// </summary>
        ///// <returns>A GDI+ Bitmap that aliases the entire Surface.</returns>
        //public Bitmap CreateAliasedBitmap(Rectangle bounds)
        //{
        //    return CreateAliasedBitmap(bounds, true);
        //}
        /// <summary>
        /// Copies the contents of the given surface to the upper left of this surface.
        /// </summary>
        /// <param name="source">The surface to copy pixels from.</param>
        /// <param name="sourceRoi">
        /// The region of the source to copy from. The upper left of this rectangle
        /// will be mapped to (0,0) on this surface.
        /// The source surface does not need to have the same dimensions as this surface. Clipping
        /// will be handled automatically. No resizing will be done.
        /// </param>
        public void CopySurface(Surface source, Rectangle sourceRoi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            sourceRoi.Intersect(source.Bounds);
            int copiedWidth = Math.Min(this.width, sourceRoi.Width);
            int copiedHeight = Math.Min(this.Height, sourceRoi.Height);

            if (copiedWidth == 0 || copiedHeight == 0)
            {
                return;
            }

            using (Surface src = (Surface)source.CreateWindow(sourceRoi))
            {
                CopySurface(src);
            }
        }

        /// <summary>
        /// Copies a rectangular region of the given surface to a specific location on this surface.
        /// </summary>
        /// <param name="source">The surface to copy pixels from.</param>
        /// <param name="dstOffset">The location on this surface to start copying pixels to.</param>
        /// <param name="sourceRoi">The region of the source surface to copy pixels from.</param>
        /// <remarks>
        /// sourceRoi.Location will be mapped to dstOffset.Location.
        /// The source surface does not need to have the same dimensions as this surface. Clipping
        /// will be handled automatically. No resizing will be done.
        /// </remarks>
        public void CopySurface(Surface source, Point dstOffset, Rectangle sourceRoi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            Rectangle dstRoi = new Rectangle(dstOffset, sourceRoi.Size);
            dstRoi.Intersect(Bounds);

            if (dstRoi.Height == 0 || dstRoi.Width == 0)
            {
                return;
            }

            sourceRoi.X += dstRoi.X - dstOffset.X;
            sourceRoi.Y += dstRoi.Y - dstOffset.Y;
            sourceRoi.Width = dstRoi.Width;
            sourceRoi.Height = dstRoi.Height;

            using (Surface src = (Surface)source.CreateWindow(sourceRoi))
            {
                CopySurface(src, dstOffset);
            }
        }

        ///// <summary>
        ///// Copies a region of the given surface to this surface.
        ///// </summary>
        ///// <param name="source">The surface to copy pixels from.</param>
        ///// <param name="region">The region to clip copying to.</param>
        ///// <remarks>
        ///// The upper left corner of the source surface will be mapped to the upper left of this
        ///// surface, and only those pixels that are defined by the region will be copied.
        ///// The source surface does not need to have the same dimensions as this surface. Clipping
        ///// will be handled automatically. No resizing will be done.
        ///// </remarks>
        //public void CopySurface(Surface source, PdnRegion region)
        //{
        //    Surface ss = (Surface)source;
        //    if (disposed)
        //    {
        //        throw new ObjectDisposedException("Surface");
        //    }

        //    Rectangle[] scans = region.GetRegionScansReadOnlyInt();
        //    for (int i = 0; i < scans.Length; ++i)
        //    {
        //        Rectangle rect = scans[i];

        //        rect.Intersect(this.Bounds);
        //        rect.Intersect(source.Bounds);

        //        if (rect.Width == 0 || rect.Height == 0)
        //        {
        //            continue;
        //        }

        //        unsafe
        //        {
        //            for (int y = rect.Top; y < rect.Bottom; ++y)
        //            {
        //                ColorBgra* dst = this.GetPointAddressUnchecked(rect.Left, y);
        //                ColorBgra* src = source.GetPointAddressUnchecked(rect.Left, y);
        //                PlatformMemory.Copy(dst, src, (ulong)rect.Width * (ulong)ColorBgra.SizeOf);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Copies a region of the given surface to this surface.
        /// </summary>
        /// <param name="source">The surface to copy pixels from.</param>
        /// <param name="region">The region to clip copying to.</param>
        /// <remarks>
        /// The upper left corner of the source surface will be mapped to the upper left of this
        /// surface, and only those pixels that are defined by the region will be copied.
        /// The source surface does not need to have the same dimensions as this surface. Clipping
        /// will be handled automatically. No resizing will be done.
        /// </remarks>
        public void CopySurface(Surface source, Rectangle[] region, int startIndex, int length)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Rectangle rect = region[i];

                rect.Intersect(this.Bounds);
                rect.Intersect(source.Bounds);

                if (rect.Width == 0 || rect.Height == 0)
                {
                    continue;
                }

                unsafe
                {
                    for (int y = rect.Top; y < rect.Bottom; ++y)
                    {
                        ColorBgra* dst = this.GetPointAddressUnchecked(rect.Left, y);
                        ColorBgra* src = source.GetPointAddressUnchecked(rect.Left, y);
                        PlatformMemory.Copy(dst, src, (ulong)rect.Width * (ulong)ColorBgra.SizeOf);
                    }
                }
            }
        }

        public void CopySurface(Surface source, Rectangle[] region)
        {
            CopySurface(source, region, 0, region.Length);
        }

        //object ICloneable.Clone()
        //{
        //    return Clone();
        //}

        ///// <summary>
        ///// Creates a new surface with the same dimensions and pixel values as this one.
        ///// </summary>
        ///// <returns>A new surface that is a clone of the current one.</returns>
        //public Surface Clone()
        //{
        //    if (disposed)
        //    {
        //        throw new ObjectDisposedException("Surface");
        //    }

        //    Surface ret = new Surface(this.Size);
        //    ret.CopySurface(this);
        //    return ret;
        //}

        /// <summary>
        /// Clears the surface to all-white (BGRA = [255,255,255,255]).
        /// </summary>
        public void Clear()
        {
            Clear(ColorBgra.FromBgra(255, 255, 255, 255));
        }

        /// <summary>
        /// Clears the surface to the given color value.
        /// </summary>
        /// <param name="color">The color value to fill the surface with.</param>
        public void Clear(ColorBgra color)
        {
            new UnaryPixelOps.Constant(color).Apply(this, this.Bounds);
        }

        /// <summary>
        /// Clears the given rectangular region within the surface to the given color value.
        /// </summary>
        /// <param name="color">The color value to fill the rectangular region with.</param>
        /// <param name="rect">The rectangular region to fill.</param>
        public void Clear(Rectangle rect, ColorBgra color)
        {
            Rectangle rect2 = Rectangle.Intersect(this.Bounds, rect);

            if (rect2 != rect)
            {
                throw new ArgumentOutOfRangeException("rectangle is out of bounds");
            }

            new UnaryPixelOps.Constant(color).Apply(this, rect);
        }

        public void ClearWithCheckboardPattern()
        {
            unsafe
            {
                for (int y = 0; y < this.height; ++y)
                {
                    ColorBgra* dstPtr = GetRowAddressUnchecked(y);

                    for (int x = 0; x < this.width; ++x)
                    {
                        byte v = (byte)((((x ^ y) & 8) * 8) + 191);
                        *dstPtr = ColorBgra.FromBgra(v, v, v, 255);
                        ++dstPtr;
                    }
                }
            }
        }

        /// <summary>
        /// Fits the source surface to this surface using super sampling. If the source surface is less wide
        /// or less tall than this surface (i.e. magnification), bicubic resampling is used instead. If either
        /// the source or destination has a dimension that is only 1 pixel, nearest neighbor is used.
        /// </summary>
        /// <param name="source">The Surface to read pixels from.</param>
        /// <remarks>This method was implemented with correctness, not performance, in mind.</remarks>
        public void SuperSamplingFitSurface(Surface source)
        {
            SuperSamplingFitSurface(source, this.Bounds);
        }

        /// <summary>
        /// Fits the source surface to this surface using super sampling. If the source surface is less wide
        /// or less tall than this surface (i.e. magnification), bicubic resampling is used instead. If either
        /// the source or destination has a dimension that is only 1 pixel, nearest neighbor is used.
        /// </summary>
        /// <param name="source">The surface to read pixels from.</param>
        /// <param name="dstRoi">The rectangle to clip rendering to.</param>
        /// <remarks>This method was implemented with correctness, not performance, in mind.</remarks>
        public void SuperSamplingFitSurface(Surface source, Rectangle dstRoi)
        {
            if (source.Width == Width && source.Height == Height)
            {
                CopySurface(source);
            }
            else if (source.Width <= Width || source.Height <= Height)
            {
                if (source.width < 2 || source.height < 2 || this.width < 2 || this.height < 2)
                {
                    this.NearestNeighborFitSurface(source, dstRoi);
                }
                else
                {
                    this.BicubicFitSurface(source, dstRoi);
                }
            }
            else unsafe
                {
                    Rectangle dstRoi2 = Rectangle.Intersect(dstRoi, this.Bounds);

                    for (int dstY = dstRoi2.Top; dstY < dstRoi2.Bottom; ++dstY)
                    {
                        double srcTop = (double)(dstY * source.height) / (double)height;
                        double srcTopFloor = Math.Floor(srcTop);
                        double srcTopWeight = 1 - (srcTop - srcTopFloor);
                        int srcTopInt = (int)srcTopFloor;

                        double srcBottom = (double)((dstY + 1) * source.height) / (double)height;
                        double srcBottomFloor = Math.Floor(srcBottom - 0.00001);
                        double srcBottomWeight = srcBottom - srcBottomFloor;
                        int srcBottomInt = (int)srcBottomFloor;

                        ColorBgra* dstPtr = this.GetPointAddressUnchecked(dstRoi2.Left, dstY);

                        for (int dstX = dstRoi2.Left; dstX < dstRoi2.Right; ++dstX)
                        {
                            double srcLeft = (double)(dstX * source.width) / (double)width;
                            double srcLeftFloor = Math.Floor(srcLeft);
                            double srcLeftWeight = 1 - (srcLeft - srcLeftFloor);
                            int srcLeftInt = (int)srcLeftFloor;

                            double srcRight = (double)((dstX + 1) * source.width) / (double)width;
                            double srcRightFloor = Math.Floor(srcRight - 0.00001);
                            double srcRightWeight = srcRight - srcRightFloor;
                            int srcRightInt = (int)srcRightFloor;

                            double blueSum = 0;
                            double greenSum = 0;
                            double redSum = 0;
                            double alphaSum = 0;

                            // left fractional edge
                            ColorBgra* srcLeftPtr = source.GetPointAddressUnchecked(srcLeftInt, srcTopInt + 1);

                            for (int srcY = srcTopInt + 1; srcY < srcBottomInt; ++srcY)
                            {
                                double a = srcLeftPtr->A;
                                blueSum += srcLeftPtr->B * srcLeftWeight * a;
                                greenSum += srcLeftPtr->G * srcLeftWeight * a;
                                redSum += srcLeftPtr->R * srcLeftWeight * a;
                                alphaSum += srcLeftPtr->A * srcLeftWeight;
                                srcLeftPtr = (ColorBgra*)((byte*)srcLeftPtr + source.stride);
                            }

                            // right fractional edge
                            ColorBgra* srcRightPtr = source.GetPointAddressUnchecked(srcRightInt, srcTopInt + 1);
                            for (int srcY = srcTopInt + 1; srcY < srcBottomInt; ++srcY)
                            {
                                double a = srcRightPtr->A;
                                blueSum += srcRightPtr->B * srcRightWeight * a;
                                greenSum += srcRightPtr->G * srcRightWeight * a;
                                redSum += srcRightPtr->R * srcRightWeight * a;
                                alphaSum += srcRightPtr->A * srcRightWeight;
                                srcRightPtr = (ColorBgra*)((byte*)srcRightPtr + source.stride);
                            }

                            // top fractional edge
                            ColorBgra* srcTopPtr = source.GetPointAddressUnchecked(srcLeftInt + 1, srcTopInt);
                            for (int srcX = srcLeftInt + 1; srcX < srcRightInt; ++srcX)
                            {
                                double a = srcTopPtr->A;
                                blueSum += srcTopPtr->B * srcTopWeight * a;
                                greenSum += srcTopPtr->G * srcTopWeight * a;
                                redSum += srcTopPtr->R * srcTopWeight * a;
                                alphaSum += srcTopPtr->A * srcTopWeight;
                                ++srcTopPtr;
                            }

                            // bottom fractional edge
                            ColorBgra* srcBottomPtr = source.GetPointAddressUnchecked(srcLeftInt + 1, srcBottomInt);
                            for (int srcX = srcLeftInt + 1; srcX < srcRightInt; ++srcX)
                            {
                                double a = srcBottomPtr->A;
                                blueSum += srcBottomPtr->B * srcBottomWeight * a;
                                greenSum += srcBottomPtr->G * srcBottomWeight * a;
                                redSum += srcBottomPtr->R * srcBottomWeight * a;
                                alphaSum += srcBottomPtr->A * srcBottomWeight;
                                ++srcBottomPtr;
                            }

                            // center area
                            for (int srcY = srcTopInt + 1; srcY < srcBottomInt; ++srcY)
                            {
                                ColorBgra* srcPtr = source.GetPointAddressUnchecked(srcLeftInt + 1, srcY);

                                for (int srcX = srcLeftInt + 1; srcX < srcRightInt; ++srcX)
                                {
                                    double a = srcPtr->A;
                                    blueSum += (double)srcPtr->B * a;
                                    greenSum += (double)srcPtr->G * a;
                                    redSum += (double)srcPtr->R * a;
                                    alphaSum += (double)srcPtr->A;
                                    ++srcPtr;
                                }
                            }

                            // four corner pixels
                            ColorBgra srcTL = source.GetPoint(srcLeftInt, srcTopInt);
                            double srcTLA = srcTL.A;
                            blueSum += srcTL.B * (srcTopWeight * srcLeftWeight) * srcTLA;
                            greenSum += srcTL.G * (srcTopWeight * srcLeftWeight) * srcTLA;
                            redSum += srcTL.R * (srcTopWeight * srcLeftWeight) * srcTLA;
                            alphaSum += srcTL.A * (srcTopWeight * srcLeftWeight);

                            ColorBgra srcTR = source.GetPoint(srcRightInt, srcTopInt);
                            double srcTRA = srcTR.A;
                            blueSum += srcTR.B * (srcTopWeight * srcRightWeight) * srcTRA;
                            greenSum += srcTR.G * (srcTopWeight * srcRightWeight) * srcTRA;
                            redSum += srcTR.R * (srcTopWeight * srcRightWeight) * srcTRA;
                            alphaSum += srcTR.A * (srcTopWeight * srcRightWeight);

                            ColorBgra srcBL = source.GetPoint(srcLeftInt, srcBottomInt);
                            double srcBLA = srcBL.A;
                            blueSum += srcBL.B * (srcBottomWeight * srcLeftWeight) * srcBLA;
                            greenSum += srcBL.G * (srcBottomWeight * srcLeftWeight) * srcBLA;
                            redSum += srcBL.R * (srcBottomWeight * srcLeftWeight) * srcBLA;
                            alphaSum += srcBL.A * (srcBottomWeight * srcLeftWeight);

                            ColorBgra srcBR = source.GetPoint(srcRightInt, srcBottomInt);
                            double srcBRA = srcBR.A;
                            blueSum += srcBR.B * (srcBottomWeight * srcRightWeight) * srcBRA;
                            greenSum += srcBR.G * (srcBottomWeight * srcRightWeight) * srcBRA;
                            redSum += srcBR.R * (srcBottomWeight * srcRightWeight) * srcBRA;
                            alphaSum += srcBR.A * (srcBottomWeight * srcRightWeight);

                            double area = (srcRight - srcLeft) * (srcBottom - srcTop);

                            double alpha = alphaSum / area;
                            double blue;
                            double green;
                            double red;

                            if (alpha == 0)
                            {
                                blue = 0;
                                green = 0;
                                red = 0;
                            }
                            else
                            {
                                blue = blueSum / alphaSum;
                                green = greenSum / alphaSum;
                                red = redSum / alphaSum;
                            }

                            // add 0.5 so that rounding goes in the direction we want it to
                            blue += 0.5;
                            green += 0.5;
                            red += 0.5;
                            alpha += 0.5;

                            dstPtr->Bgra = (uint)blue + ((uint)green << 8) + ((uint)red << 16) + ((uint)alpha << 24);
                            ++dstPtr;
                        }
                    }
                }
        }

        /// <summary>
        /// Fits the source surface to this surface using nearest neighbor resampling.
        /// </summary>
        /// <param name="source">The surface to read pixels from.</param>
        public void NearestNeighborFitSurface(Surface source)
        {
            NearestNeighborFitSurface(source, this.Bounds);
        }

        /// <summary>
        /// Fits the source surface to this surface using nearest neighbor resampling.
        /// </summary>
        /// <param name="source">The surface to read pixels from.</param>
        /// <param name="dstRoi">The rectangle to clip rendering to.</param>
        public void NearestNeighborFitSurface(Surface source, Rectangle dstRoi)
        {
            Rectangle roi = Rectangle.Intersect(dstRoi, this.Bounds);

            unsafe
            {
                for (int dstY = roi.Top; dstY < roi.Bottom; ++dstY)
                {
                    int srcY = (dstY * source.height) / height;
                    ColorBgra* srcRow = source.GetRowAddressUnchecked(srcY);
                    ColorBgra* dstPtr = this.GetPointAddressUnchecked(roi.Left, dstY);

                    for (int dstX = roi.Left; dstX < roi.Right; ++dstX)
                    {
                        int srcX = (dstX * source.width) / width;
                        *dstPtr = *(srcRow + srcX);
                        ++dstPtr;
                    }
                }
            }
        }

        /// <summary>
        /// Fits the source surface to this surface using bicubic interpolation.
        /// </summary>
        /// <param name="source">The Surface to read pixels from.</param>
        /// <remarks>
        /// This method was implemented with correctness, not performance, in mind. 
        /// Based on: "Bicubic Interpolation for Image Scaling" by Paul Bourke,
        ///           http://astronomy.swin.edu.au/%7Epbourke/colour/bicubic/
        /// </remarks>
        public void BicubicFitSurface(Surface source)
        {
            BicubicFitSurface(source, this.Bounds);
        }

        private double CubeClamped(double x)
        {
            if (x >= 0)
            {
                return x * x * x;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Implements R() as defined at http://astronomy.swin.edu.au/%7Epbourke/colour/bicubic/
        /// </summary>
        private double R(double x)
        {
            return (CubeClamped(x + 2) - (4 * CubeClamped(x + 1)) + (6 * CubeClamped(x)) - (4 * CubeClamped(x - 1))) / 6;
        }

        /// <summary>
        /// Fits the source surface to this surface using bicubic interpolation.
        /// </summary>
        /// <param name="source">The Surface to read pixels from.</param>
        /// <param name="dstRoi">The rectangle to clip rendering to.</param>
        /// <remarks>
        /// This method was implemented with correctness, not performance, in mind. 
        /// Based on: "Bicubic Interpolation for Image Scaling" by Paul Bourke,
        ///           http://astronomy.swin.edu.au/%7Epbourke/colour/bicubic/
        /// </remarks>
        public void BicubicFitSurface(Surface source, Rectangle dstRoi)
        {
            float leftF = (1 * (float)(width - 1)) / (float)(source.width - 1);
            float topF = (1 * (height - 1)) / (float)(source.height - 1);
            float rightF = ((float)(source.width - 3) * (float)(width - 1)) / (float)(source.width - 1);
            float bottomF = ((float)(source.Height - 3) * (float)(height - 1)) / (float)(source.height - 1);

            int left = (int)Math.Ceiling((double)leftF);
            int top = (int)Math.Ceiling((double)topF);
            int right = (int)Math.Floor((double)rightF);
            int bottom = (int)Math.Floor((double)bottomF);

            Rectangle[] rois = new Rectangle[] {
                                                   Rectangle.FromLTRB(left, top, right, bottom),
                                                   new Rectangle(0, 0, width, top),
                                                   new Rectangle(0, top, left, height - top),
                                                   new Rectangle(right, top, width - right, height - top),
                                                   new Rectangle(left, bottom, right - left, height - bottom)
                                               };

            for (int i = 0; i < rois.Length; ++i)
            {
                rois[i].Intersect(dstRoi);

                if (rois[i].Width > 0 && rois[i].Height > 0)
                {
                    if (i == 0)
                    {
                        BicubicFitSurfaceUnchecked(source, rois[i]);
                    }
                    else
                    {
                        BicubicFitSurfaceChecked(source, rois[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Implements bicubic filtering with bounds checking at every pixel.
        /// </summary>
        private void BicubicFitSurfaceChecked(Surface source, Rectangle dstRoi)
        {

            if (this.width < 2 || this.height < 2 || source.width < 2 || source.height < 2)
            {
                SuperSamplingFitSurface(source, dstRoi);
            }
            else
            {
                unsafe
                {
                    Rectangle roi = Rectangle.Intersect(dstRoi, this.Bounds);
                    Rectangle roiIn = Rectangle.Intersect(dstRoi, new Rectangle(1, 1, width - 1, height - 1));

                    IntPtr rColCacheIP = PlatformMemory.Allocate(4 * (ulong)roi.Width * (ulong)sizeof(double));
                    double* rColCache = (double*)rColCacheIP.ToPointer();

                    // Precompute and then cache the value of R() for each column
                    for (int dstX = roi.Left; dstX < roi.Right; ++dstX)
                    {
                        double srcColumn = (double)(dstX * (source.width - 1)) / (double)(width - 1);
                        double srcColumnFloor = Math.Floor(srcColumn);
                        double srcColumnFrac = srcColumn - srcColumnFloor;
                        int srcColumnInt = (int)srcColumn;

                        for (int m = -1; m <= 2; ++m)
                        {
                            int index = (m + 1) + ((dstX - roi.Left) * 4);
                            double x = m - srcColumnFrac;
                            rColCache[index] = R(x);
                        }
                    }

                    // Set this up so we can cache the R()'s for every row
                    double* rRowCache = stackalloc double[4];

                    for (int dstY = roi.Top; dstY < roi.Bottom; ++dstY)
                    {
                        double srcRow = (double)(dstY * (source.height - 1)) / (double)(height - 1);
                        double srcRowFloor = (double)Math.Floor(srcRow);
                        double srcRowFrac = srcRow - srcRowFloor;
                        int srcRowInt = (int)srcRow;
                        ColorBgra* dstPtr = this.GetPointAddressUnchecked(roi.Left, dstY);

                        // Compute the R() values for this row
                        for (int n = -1; n <= 2; ++n)
                        {
                            double x = srcRowFrac - n;
                            rRowCache[n + 1] = R(x);
                        }

                        // See Perf Note below
                        //int nFirst = Math.Max(-srcRowInt, -1);
                        //int nLast = Math.Min(source.height - srcRowInt - 1, 2);

                        for (int dstX = roi.Left; dstX < roi.Right; dstX++)
                        {
                            double srcColumn = (double)(dstX * (source.width - 1)) / (double)(width - 1);
                            double srcColumnFloor = Math.Floor(srcColumn);
                            double srcColumnFrac = srcColumn - srcColumnFloor;
                            int srcColumnInt = (int)srcColumn;

                            double blueSum = 0;
                            double greenSum = 0;
                            double redSum = 0;
                            double alphaSum = 0;
                            double totalWeight = 0;

                            // See Perf Note below
                            //int mFirst = Math.Max(-srcColumnInt, -1);
                            //int mLast = Math.Min(source.width - srcColumnInt - 1, 2);

                            ColorBgra* srcPtr = source.GetPointAddressUnchecked(srcColumnInt - 1, srcRowInt - 1);

                            for (int n = -1; n <= 2; ++n)
                            {
                                int srcY = srcRowInt + n;

                                for (int m = -1; m <= 2; ++m)
                                {
                                    // Perf Note: It actually benchmarks faster on my system to do
                                    // a bounds check for every (m,n) than it is to limit the loop
                                    // to nFirst-Last and mFirst-mLast.
                                    // I'm leaving the code above, albeit commented out, so that
                                    // benchmarking between these two can still be performed.
                                    if (source.IsVisible(srcColumnInt + m, srcY))
                                    {
                                        double w0 = rColCache[(m + 1) + (4 * (dstX - roi.Left))];
                                        double w1 = rRowCache[n + 1];
                                        double w = w0 * w1;

                                        blueSum += srcPtr->B * w * srcPtr->A;
                                        greenSum += srcPtr->G * w * srcPtr->A;
                                        redSum += srcPtr->R * w * srcPtr->A;
                                        alphaSum += srcPtr->A * w;

                                        totalWeight += w;
                                    }

                                    ++srcPtr;
                                }

                                srcPtr = (ColorBgra*)((byte*)(srcPtr - 4) + source.stride);
                            }

                            double alpha = alphaSum / totalWeight;
                            double blue;
                            double green;
                            double red;

                            if (alpha == 0)
                            {
                                blue = 0;
                                green = 0;
                                red = 0;
                            }
                            else
                            {
                                blue = blueSum / alphaSum;
                                green = greenSum / alphaSum;
                                red = redSum / alphaSum;

                                // add 0.5 to ensure truncation to uint results in rounding
                                alpha += 0.5;
                                blue += 0.5;
                                green += 0.5;
                                red += 0.5;
                            }

                            dstPtr->Bgra = (uint)blue + ((uint)green << 8) + ((uint)red << 16) + ((uint)alpha << 24);
                            ++dstPtr;
                        } // for (dstX...
                    } // for (dstY...

                    PlatformMemory.Free(rColCacheIP);
                } // unsafe
            }
        }

        /// <summary>
        /// Implements bicubic filtering with NO bounds checking at any pixel.
        /// </summary>
        public void BicubicFitSurfaceUnchecked(Surface source, Rectangle dstRoi)
        {
            if (this.width < 2 || this.height < 2 || source.width < 2 || source.height < 2)
            {
                SuperSamplingFitSurface(source, dstRoi);
            }
            else
            {
                unsafe
                {
                    Rectangle roi = Rectangle.Intersect(dstRoi, this.Bounds);
                    Rectangle roiIn = Rectangle.Intersect(dstRoi, new Rectangle(1, 1, width - 1, height - 1));

                    IntPtr rColCacheIP = PlatformMemory.Allocate(4 * (ulong)roi.Width * (ulong)sizeof(double));
                    double* rColCache = (double*)rColCacheIP.ToPointer();

                    // Precompute and then cache the value of R() for each column
                    for (int dstX = roi.Left; dstX < roi.Right; ++dstX)
                    {
                        double srcColumn = (double)(dstX * (source.width - 1)) / (double)(width - 1);
                        double srcColumnFloor = Math.Floor(srcColumn);
                        double srcColumnFrac = srcColumn - srcColumnFloor;
                        int srcColumnInt = (int)srcColumn;

                        for (int m = -1; m <= 2; ++m)
                        {
                            int index = (m + 1) + ((dstX - roi.Left) * 4);
                            double x = m - srcColumnFrac;
                            rColCache[index] = R(x);
                        }
                    }

                    // Set this up so we can cache the R()'s for every row
                    double* rRowCache = stackalloc double[4];

                    for (int dstY = roi.Top; dstY < roi.Bottom; ++dstY)
                    {
                        double srcRow = (double)(dstY * (source.height - 1)) / (double)(height - 1);
                        double srcRowFloor = Math.Floor(srcRow);
                        double srcRowFrac = srcRow - srcRowFloor;
                        int srcRowInt = (int)srcRow;
                        ColorBgra* dstPtr = this.GetPointAddressUnchecked(roi.Left, dstY);

                        // Compute the R() values for this row
                        for (int n = -1; n <= 2; ++n)
                        {
                            double x = srcRowFrac - n;
                            rRowCache[n + 1] = R(x);
                        }

                        rColCache = (double*)rColCacheIP.ToPointer();
                        ColorBgra* srcRowPtr = source.GetRowAddressUnchecked(srcRowInt - 1);

                        for (int dstX = roi.Left; dstX < roi.Right; dstX++)
                        {
                            double srcColumn = (double)(dstX * (source.width - 1)) / (double)(width - 1);
                            double srcColumnFloor = Math.Floor(srcColumn);
                            double srcColumnFrac = srcColumn - srcColumnFloor;
                            int srcColumnInt = (int)srcColumn;

                            double blueSum = 0;
                            double greenSum = 0;
                            double redSum = 0;
                            double alphaSum = 0;
                            double totalWeight = 0;

                            ColorBgra* srcPtr = srcRowPtr + srcColumnInt - 1;
                            for (int n = 0; n <= 3; ++n)
                            {
                                double w0 = rColCache[0] * rRowCache[n];
                                double w1 = rColCache[1] * rRowCache[n];
                                double w2 = rColCache[2] * rRowCache[n];
                                double w3 = rColCache[3] * rRowCache[n];

                                double a0 = srcPtr[0].A;
                                double a1 = srcPtr[1].A;
                                double a2 = srcPtr[2].A;
                                double a3 = srcPtr[3].A;

                                alphaSum += (a0 * w0) + (a1 * w1) + (a2 * w2) + (a3 * w3);
                                totalWeight += w0 + w1 + w2 + w3;

                                blueSum += (a0 * srcPtr[0].B * w0) + (a1 * srcPtr[1].B * w1) + (a2 * srcPtr[2].B * w2) + (a3 * srcPtr[3].B * w3);
                                greenSum += (a0 * srcPtr[0].G * w0) + (a1 * srcPtr[1].G * w1) + (a2 * srcPtr[2].G * w2) + (a3 * srcPtr[3].G * w3);
                                redSum += (a0 * srcPtr[0].R * w0) + (a1 * srcPtr[1].R * w1) + (a2 * srcPtr[2].R * w2) + (a3 * srcPtr[3].R * w3);

                                srcPtr = (ColorBgra*)((byte*)srcPtr + source.stride);
                            }

                            double alpha = alphaSum / totalWeight;

                            double blue;
                            double green;
                            double red;

                            if (alpha == 0)
                            {
                                blue = 0;
                                green = 0;
                                red = 0;
                            }
                            else
                            {
                                blue = blueSum / alphaSum;
                                green = greenSum / alphaSum;
                                red = redSum / alphaSum;

                                // add 0.5 to ensure truncation to uint results in rounding
                                alpha += 0.5;
                                blue += 0.5;
                                green += 0.5;
                                red += 0.5;
                            }

                            dstPtr->Bgra = (uint)blue + ((uint)green << 8) + ((uint)red << 16) + ((uint)alpha << 24);
                            ++dstPtr;
                            rColCache += 4;
                        } // for (dstX...
                    } // for (dstY...

                    PlatformMemory.Free(rColCacheIP);
                } // unsafe
            }
        }

        /// <summary>
        /// Fits the source surface to this surface using bilinear interpolation.
        /// </summary>
        /// <param name="source">The surface to read pixels from.</param>
        /// <remarks>This method was implemented with correctness, not performance, in mind.</remarks>
        public void BilinearFitSurface(Surface source)
        {
            BilinearFitSurface(source, this.Bounds);
        }

        /// <summary>
        /// Fits the source surface to this surface using bilinear interpolation.
        /// </summary>
        /// <param name="source">The surface to read pixels from.</param>
        /// <param name="dstRoi">The rectangle to clip rendering to.</param>
        /// <remarks>This method was implemented with correctness, not performance, in mind.</remarks>
        public void BilinearFitSurface(Surface source, Rectangle dstRoi)
        {
            if (dstRoi.Width < 2 || dstRoi.Height < 2 || this.width < 2 || this.height < 2)
            {
                SuperSamplingFitSurface(source, dstRoi);
            }
            else
            {
                unsafe
                {
                    Rectangle roi = Rectangle.Intersect(dstRoi, this.Bounds);

                    for (int dstY = roi.Top; dstY < roi.Bottom; ++dstY)
                    {
                        ColorBgra* dstRowPtr = this.GetRowAddressUnchecked(dstY);
                        float srcRow = (float)(dstY * (source.height - 1)) / (float)(height - 1);

                        for (int dstX = roi.Left; dstX < roi.Right; dstX++)
                        {
                            float srcColumn = (float)(dstX * (source.width - 1)) / (float)(width - 1);
                            *dstRowPtr = source.GetBilinearSample(srcColumn, srcRow);
                            ++dstRowPtr;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fits the source surface to this surface using the given algorithm.
        /// </summary>
        /// <param name="algorithm">The surface to copy pixels from.</param>
        /// <param name="source">The algorithm to use.</param>
        public void FitSurface(ResamplingAlgorithm algorithm, Surface source)
        {
            FitSurface(algorithm, source, this.Bounds);
        }

        /// <summary>
        /// Fits the source surface to this surface using the given algorithm.
        /// </summary>
        /// <param name="algorithm">The surface to copy pixels from.</param>
        /// <param name="dstRoi">The rectangle to clip rendering to.</param>
        /// <param name="source">The algorithm to use.</param>
        public void FitSurface(ResamplingAlgorithm algorithm, Surface source, Rectangle dstRoi)
        {
            switch (algorithm)
            {
                case ResamplingAlgorithm.Bicubic:
                    BicubicFitSurface(source, dstRoi);
                    break;

                case ResamplingAlgorithm.Bilinear:
                    BilinearFitSurface(source, dstRoi);
                    break;

                case ResamplingAlgorithm.NearestNeighbor:
                    NearestNeighborFitSurface(source, dstRoi);
                    break;

                case ResamplingAlgorithm.SuperSampling:
                    SuperSamplingFitSurface(source, dstRoi);
                    break;

                default:
                    //throw new InvalidEnumArgumentException("algorithm");
                    throw new Exception("algorithm");
            }
        }


        /// <summary>
        /// Releases all resources held by this Surface object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;

                //if (disposing)
                //{
                //    scan0.Dispose();
                //    scan0 = null;
                //}
            }
        }
    }
}
