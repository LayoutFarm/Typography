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
//
// conv_stroke
//
//----------------------------------------------------------------------------

namespace PixelFarm.Agg.VertexSource
{
    public sealed class Contour
    {
        ContourGenerator generator;
        VertexStoreSnap vertexSource;
        public Contour(VertexStoreSnap vertexSource)
        {
            this.generator = new ContourGenerator();
            this.vertexSource = vertexSource;
        }
        ContourGenerator GetGenerator()
        {
            return this.generator;
        }
        public LineJoin LineJoin
        {
            get { return this.GetGenerator().LineJoin; }
            set { this.GetGenerator().LineJoin = value; }
        }
        public InnerJoin InnerJoin
        {
            get { return this.GetGenerator().InnerJoin; }
            set { this.GetGenerator().InnerJoin = value; }
        }
        public double InnerMiterLimit
        {
            get { return this.GetGenerator().InnerMiterLimit; }
            set { this.GetGenerator().InnerMiterLimit = value; }
        }
        public double MiterLimit
        {
            get { return this.GetGenerator().MiterLimit; }
            set { this.GetGenerator().MiterLimit = value; }
        }

        public double Width
        {
            get { return this.GetGenerator().Width; }
            set { this.GetGenerator().Width = value; }
        }
        public void SetMiterLimitTheta(double t) { this.GetGenerator().SetMiterLimitTheta(t); }


        public bool AutoDetectOrientation
        {
            get { return this.GetGenerator().AutoDetectOrientation; }
            set { this.GetGenerator().AutoDetectOrientation = value; }
        }
        public double ApproximateScale
        {
            get { return this.GetGenerator().ApproximateScale; }
            set { this.GetGenerator().ApproximateScale = value; }
        }
    }
}
