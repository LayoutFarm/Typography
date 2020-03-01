/////////////////////////////////////////////////////////////////////////////////
// Paint.NET (MIT,from version 3.36.7, see=> https://github.com/rivy/OpenPDN   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////
//MIT, 2017-present, WinterDev
using System;
using PixelFarm.Drawing;
namespace PaintFx
{
    /// <summary>
    /// Encapsulates the arguments passed to a Render function.
    /// This way we can do on-demand and once-only creation of Bitmap and Graphics
    /// objects from a given Surface object.
    /// </summary>
    /// <remarks>
    /// Use of the Bitmap and Graphics objects is not thread safe because of how GDI+ works.
    /// You must wrap use of these objects with a critical section, like so:
    ///     object lockObject = new object();
    ///     lock (lockObject)
    ///     {
    ///         Graphics g = ra.Graphics;
    ///         g.DrawRectangle(...);
    ///         // etc.
    ///     }
    /// </remarks>
    public sealed class RenderArgs : IDisposable
    {
        Surface _surface;
        //private Bitmap bitmap;
        //private Graphics graphics;
        bool _disposed = false;

        /// <summary>
        /// Gets the Surface that has been associated with this instance of RenderArgs.
        /// </summary>
        public Surface Surface
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("RenderArgs");
                }

                return _surface;
            }
        }

        ///// <summary>
        ///// Gets a Bitmap reference that aliases the Surface.
        ///// </summary>
        //public Bitmap Bitmap
        //{
        //    get
        //    {
        //        if (this.disposed)
        //        {
        //            throw new ObjectDisposedException("RenderArgs");
        //        }

        //        if (this.bitmap == null)
        //        {
        //            this.bitmap = surface.CreateAliasedBitmap();
        //        }

        //        return this.bitmap;
        //    }
        //}

        /// <summary>
        /// Retrieves a Graphics instance that can be used to draw on to the Surface.
        /// </summary>
        /// <remarks>
        /// Use of this object is not thread-safe. You must wrap retrieval and consumption of this 
        /// property with a critical section.
        /// </remarks>
        //public Graphics Graphics
        //{
        //    get
        //    {
        //        throw new StillNotPortedException();
        //        //if (this.disposed)
        //        //{
        //        //    throw new ObjectDisposedException("RenderArgs");
        //        //}

        //        //if (this.graphics == null)
        //        //{
        //        //    this.graphics = Graphics.FromImage(Bitmap);
        //        //}

        //        //return this.graphics;
        //    }
        //}

        /// <summary>
        /// Gets the size of the associated Surface object.
        /// </summary>
        /// <remarks>
        /// This is a convenience method equivalent to using RenderArgs.Surface.Bounds.
        /// </remarks>
        public Rectangle Bounds
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("RenderArgs");
                }

                return this.Surface.Bounds;
            }
        }

        /// <summary>
        /// Gets the size of the associated Surface object.
        /// </summary>
        /// <remarks>
        /// This is a convenient method equivalent to using RenderArgs.Surface.Size.
        /// </remarks>
        public Size Size
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("RenderArgs");
                }

                return this.Surface.Size;
            }
        }

        /// <summary>
        /// Gets the width of the associated Surface object.
        /// </summary>
        /// <remarks>
        /// This is a convenience method equivalent to using RenderArgs.Surface.Width.
        /// </remarks>
        public int Width
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("RenderArgs");
                }

                return _surface.Width;
            }
        }

        /// <summary>
        /// Gets the height of the associated Surface object.
        /// </summary>
        /// <remarks>
        /// This is a convenience method equivalent to using RenderArgs.Surface.Height.
        /// </remarks>
        public int Height
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("RenderArgs");
                }

                return _surface.Height;
            }
        }

        /// <summary>
        /// Creates an instance of the RenderArgs class.
        /// </summary>
        /// <param name="surface">
        /// The Surface to associate with this instance. This instance of RenderArgs does not 
        /// take ownership of this Surface.
        /// </param>
        public RenderArgs(Surface surface)
        {
            _surface = surface;
            //this.bitmap = null;
            //this.graphics = null;
        }

        ~RenderArgs()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of the contained Bitmap and Graphics instances, if necessary.
        /// </summary>
        /// <remarks>
        /// Note that since this class does not take ownership of the Surface, it
        /// is not disposed.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;

                if (disposing)
                {
                    //if (this.graphics != null)
                    //{
                    //    this.graphics.Dispose();
                    //    this.graphics = null;
                    //}

                    //if (this.bitmap != null)
                    //{
                    //    this.bitmap.Dispose();
                    //    this.bitmap = null;
                    //}
                }
            }
        }
    }
}
