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

using PixelFarm.Agg.Image;
using PixelFarm.Agg.Transform;
namespace PixelFarm.Agg
{
    public abstract class Graphics2D
    {
        protected ActualImage destActualImage;
        protected ScanlineRasterizer sclineRas;
        Affine currentTxMatrix = Affine.IdentityMatrix;
         
        //------------------------------------------------------------------------

        public abstract void SetClippingRect(RectInt rect);
        public abstract RectInt GetClippingRect();
        public abstract void Clear(Drawing.Color color);
        //------------------------------------------------------------------------
        //render vertices
        public abstract void Render(VertexStoreSnap vertexSource, Drawing.Color c);
        //------------------------------------------------------------------------


        public void Render(VertexStore vxs, Drawing.Color c)
        {
            Render(new VertexStoreSnap(vxs), c);
        }

        public Affine CurrentTransformMatrix
        {
            get { return this.currentTxMatrix; }
            set
            {
                this.currentTxMatrix = value;
            }
        }

        public ScanlineRasterizer ScanlineRasterizer
        {
            get { return sclineRas; }
        }
        public abstract ScanlinePacked8 ScanlinePacked8
        {
            get;
        }
        public abstract ScanlineRasToDestBitmapRenderer ScanlineRasToDestBitmap
        {
            get;
        }
        public ActualImage DestActualImage
        {
            get { return this.destActualImage; }
        }
        public abstract ImageReaderWriterBase DestImage
        {
            get;
        }
        public abstract IPixelBlender PixelBlender
        {
            get;
            set;
        }
        //================
        public static ImageGraphics2D CreateFromImage(ActualImage actualImage,PixelFarm.Drawing.GraphicsPlatform gfxPlatform)
        {
            return new ImageGraphics2D(actualImage, gfxPlatform);
        }
        public abstract bool UseSubPixelRendering
        {
            get;
            set;
        }


#if DEBUG
        public void dbugLine(double x1, double y1, double x2, double y2, Drawing.Color color)
        {
            VertexStore vxs = new VertexStore(8);
            vxs.AddMoveTo(x1, y1);
            vxs.AddLineTo(x2, y2);
            vxs.AddStop();
            Render(new Stroke(1).MakeVxs(vxs), color);
        }
#endif



    }
}
