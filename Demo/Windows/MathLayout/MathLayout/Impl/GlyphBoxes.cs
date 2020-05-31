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
            GlyphVxs = MathBoxTreeBuilder.ScaleVertexStoreWidthTo(GlyphVxs, width);
        }
        public override void ScalteToFitHeight(float height)
        {
            GlyphVxs = MathBoxTreeBuilder.ScaleVertexStoreHeightTo(GlyphVxs, height);
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