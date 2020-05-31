//MIT, 2020, Brezza92, WinterDev

using System;
using LayoutFarm.MathLayout;
using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.Drawing;

namespace MathLayout
{
    public class VxsGlyphBox : GlyphBox
    {

        VertexStore _glyphVxs;
        public VertexStore GlyphVxs
        {
            get => _glyphVxs;
            set
            {
                _glyphVxs = value;
                _alreadyLayout = false;
            }
        }
        public override void ClearVxs()
        {
            GlyphVxs = null;
        }
        public override bool HasVxs => _glyphVxs != null;

        public override Rect GetBoundingRect()
        {
            Q1RectD rect1 = _glyphVxs.GetBoundingRect();
            return new Rect(rect1.Left, rect1.Top, rect1.Width, rect1.Height);
        }
        public override void ScaleToFitWidth(float width)
        {
            GlyphVxs = ScaleVertexStoreWidthTo(GlyphVxs, width);
        }
        public override void ScalteToFitHeight(float height)
        {
            GlyphVxs = ScaleVertexStoreHeightTo(GlyphVxs, height);
        }
        static VertexStore ScaleVertexStoreWidthTo(VertexStore source, float width)
        {
            var bound = source.GetBoundingRect();
            float scale = width / (float)bound.Width;

            VertexStore output = new VertexStore();
            AffineMat mat = AffineMat.Iden();
            mat.Translate(0, 0);
            mat.Scale(scale, 1);

            mat.TransformToVxs(source, output);
            return output;
        }

        static VertexStore ScaleVertexStoreHeightTo(VertexStore source, float height)
        {
            var bound = source.GetBoundingRect();
            float scale = height / (float)bound.Height;

            VertexStore output = new VertexStore();
            AffineMat mat = AffineMat.Iden();
            mat.Translate(0, 0);
            mat.Scale(1, scale);

            mat.TransformToVxs(source, output);
            return output;
        }

    }

    class MyCustomNotationVsxBox : CustomNotationVsxBox
    {
        public VertexStore CustomVxs { get; set; }
        public override void Layout()
        {
            float maxH = 0;
            float maxW = 0;
            if (NotationBox != null)
            {
                NotationBox.Layout();
                maxH = System.Math.Max(maxH, NotationBox.Height);
                maxW = System.Math.Max(maxW, NotationBox.Width);
            }
            if (CustomVxs != null)
            {
                var bounding = CustomVxs.GetBoundingRect();
                maxH = System.Math.Max(maxH, (float)bounding.Height);
                maxW = System.Math.Max(maxW, (float)bounding.Width);
            }
            this.Height = maxH;
            this.Width = maxW;
        }
    }


}