//BSD, 2014-2016, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------

using System;
namespace PixelFarm.Agg.Image
{
    public static class ImageHelper
    {
        /// <summary>
        /// This will create a new ImageBuffer that references the same memory as the image that you took the sub image from.
        /// It will modify the original main image when you draw to it.
        /// </summary>
        /// <param name="parentImage"></param>
        /// <param name="childImageBounds"></param>
        /// <returns></returns>
        public static ChildImage CreateChildImage(IImageReaderWriter parentImage, RectInt childImageBounds)
        {
            if (childImageBounds.Left < 0 || childImageBounds.Bottom < 0 || childImageBounds.Right > parentImage.Width || childImageBounds.Top > parentImage.Height
                || childImageBounds.Left >= childImageBounds.Right || childImageBounds.Bottom >= childImageBounds.Top)
            {
                throw new ArgumentException("The subImageBounds must be on the image and valid.");
            }

            int left = Math.Max(0, childImageBounds.Left);
            int bottom = Math.Max(0, childImageBounds.Bottom);
            int width = Math.Min(parentImage.Width - left, childImageBounds.Width);
            int height = Math.Min(parentImage.Height - bottom, childImageBounds.Height);
            int bufferOffsetToFirstPixel = parentImage.GetBufferOffsetXY(left, bottom);
            return new ChildImage(parentImage, bufferOffsetToFirstPixel, width, height);
        }
    }
}