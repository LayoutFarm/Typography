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

using PixelFarm.Agg.Imaging;
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
        /// <summary>
        /// we do NOT store vxs
        /// </summary>
        /// <param name="vertexSource"></param>
        /// <param name="c"></param>
        public abstract void Render(VertexStoreSnap vertexSource, Drawing.Color c);
        //------------------------------------------------------------------------

        /// <summary>
        /// we do NOT store vxs
        /// </summary>
        /// <param name="vxs"></param>
        /// <param name="c"></param>
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
        public static ImageGraphics2D CreateFromImage(ActualImage actualImage)
        {
            return new ImageGraphics2D(actualImage);
        }
        public abstract bool UseSubPixelRendering
        {
            get;
            set;
        }


#if DEBUG
        VertexStore dbug_v1 = new VertexStore(8);
        VertexStore dbug_v2 = new VertexStore();
        Stroke dbugStroke = new Stroke(1);
        public void dbugLine(double x1, double y1, double x2, double y2, Drawing.Color color)
        {


            dbug_v1.AddMoveTo(x1, y1);
            dbug_v1.AddLineTo(x2, y2);
            dbug_v1.AddStop();

            dbugStroke.MakeVxs(dbug_v1, dbug_v2);
            Render(dbug_v2, color);
            dbug_v1.Clear();
            dbug_v2.Clear();
        }
#endif



    }
}
