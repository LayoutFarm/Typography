//MIT, 2020, Brezza92
using LayoutFarm.MathLayout;
using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.Drawing;
using System;
using Typography.Contours;
using Typography.OpenFont;

namespace MathLayout
{

    class VxsMathBoxTreeBuilder : MathBoxTreeBuilderBase
    {
        public VxsMathBoxTreeBuilder() { }

        GlyphMeshStore _glyphMeshStore = new GlyphMeshStore();

        protected override CustomNotationVsxBox NewCustomVxsBox() => new MyCustomNotationVsxBox();
        protected override GlyphBox NewGlyphBox() => new VxsGlyphBox();

        protected override void SetGlyphVxs(GlyphBox glyphBox, Typeface typeface, float sizeInPoint)
        {
            if (glyphBox is VxsGlyphBox vxsGlyphBox)
            {
                _glyphMeshStore.SetFont(typeface, sizeInPoint);//20= font size
                _glyphMeshStore.FlipGlyphUpward = true;
                if (glyphBox.IsItalic)
                {
                    _glyphMeshStore.SimulateOblique = glyphBox.IsItalic;
                }
                vxsGlyphBox.GlyphVxs = _glyphMeshStore.GetGlyphMesh(glyphBox.GlyphIndex);
                if (_glyphMeshStore.SimulateOblique)
                {
                    _glyphMeshStore.SimulateOblique = false;
                }
            }
        }
        protected override void CreateCustomNotation(EncloseNotation notation,
            float thickness, float w, float h,
            HorizontalStackBox hbox, float maxLeft, float maxTop, float extend, float over,
            EncloseBox encloseBox)
        {
            //notations that only custom lines

            using (Tools.BorrowVxs(out VertexStore vsx1, out VertexStore vsx2))
            using (Tools.BorrowStroke(out Stroke stroke))
            using (Tools.BorrowPathWriter(vsx1, out PathWriter pathWriter))
            {
                var customVsxBox = new MyCustomNotationVsxBox();
                stroke.LineJoin = LineJoin.Bevel;
                stroke.Width = thickness;
                int useVxs = 1;//default = vxs1
                switch (notation)
                {
                    default:
                        useVxs = 0;//not match only lines notation
                        break;
                    case EncloseNotation.actuarial:
                        pathWriter.MoveTo(0, 0);
                        pathWriter.LineTo(w, 0);
                        pathWriter.LineTo(w, h);
                        break;
                    case EncloseNotation.box:
                        pathWriter.MoveTo(0, 0);
                        pathWriter.LineTo(0, h);
                        pathWriter.LineTo(w, h);
                        pathWriter.LineTo(w, 0);
                        pathWriter.LineTo(0, 0);
                        break;
                    case EncloseNotation.left:
                        pathWriter.MoveTo(0, 0);
                        pathWriter.LineTo(0, h);
                        break;
                    case EncloseNotation.right:
                        pathWriter.MoveTo(w, 0);
                        pathWriter.LineTo(w, h);
                        break;
                    case EncloseNotation.top:
                        pathWriter.MoveTo(0, 0);
                        pathWriter.LineTo(w, 0);
                        break;
                    case EncloseNotation.bottom:
                        pathWriter.MoveTo(0, h);
                        pathWriter.LineTo(w, h);
                        break;
                    case EncloseNotation.updiagonalstrike:
                        pathWriter.MoveTo(0, h);
                        pathWriter.LineTo(w, 0);
                        break;
                    case EncloseNotation.downdiagonalstrike:
                        pathWriter.MoveTo(0, 0);
                        pathWriter.LineTo(w, h);
                        break;
                    case EncloseNotation.verticalstrike:
                        pathWriter.MoveTo(w / 2f, 0);
                        pathWriter.LineTo(w / 2f, h);
                        break;
                    case EncloseNotation.horizontalstrike:
                        pathWriter.MoveTo(0, h / 2f);
                        pathWriter.LineTo(w, h / 2f);
                        break;
                    case EncloseNotation.madruwb:
                        pathWriter.MoveTo(w, 0);
                        pathWriter.LineTo(w, h);
                        pathWriter.LineTo(0, h);
                        break;
                    case EncloseNotation.updiagonalarrow:
                        double arrowAngleDegree = Math.Atan(h / w) * 180.0 / Math.PI;
                        double arrowLength = Math.Sqrt(Math.Pow(h, 2) + Math.Pow(w, 2));//pythagoras

                        float arrowWing = GetPixelScale() * 150;
                        pathWriter.MoveTo(0, 0);
                        pathWriter.LineTo(arrowLength, 0);
                        pathWriter.LineTo(arrowLength - arrowWing, -arrowWing);
                        pathWriter.LineTo(arrowLength - arrowWing, arrowWing);
                        pathWriter.LineTo(arrowLength, 0);

                        AffineMat mat = AffineMat.Iden();
                        mat.RotateDeg(-arrowAngleDegree);
                        mat.Translate(0, h);
                        mat.TransformToVxs(vsx1, vsx2);

                        useVxs = 2;
                        break;
                    case EncloseNotation.phasorangle:
                        float angleWidth = 640 * GetPixelScale();//x 637.5
                        float angleHeight = 1160 * GetPixelScale();//y 1162.5
                        float shiftH = h - angleHeight;
                        pathWriter.MoveTo(angleWidth, shiftH);
                        pathWriter.LineTo(0, angleHeight + shiftH);
                        pathWriter.LineTo(maxLeft - angleWidth + w, angleHeight + shiftH);

                        customVsxBox.BeforeBaseBox = angleWidth;
                        break;
                    case EncloseNotation.longdiv:
                        GlyphBox ldiv = NewGlyphBox();
                        ldiv.Character = ')';
                        AssignGlyphVxs(ldiv);
                        ldiv.Layout();

                        Box actualDiv = StretchHeightIfStretchable(ldiv, hbox.Height + over);
                        actualDiv.Layout();
                        customVsxBox.NotationBox = actualDiv;
                        float shiftLeft = maxLeft - actualDiv.Width;
                        float shiftTop = maxTop - over;
                        actualDiv.SetLocation(shiftLeft, -shiftTop - over);
                        pathWriter.MoveTo(shiftLeft, shiftTop);
                        pathWriter.LineTo(shiftLeft + hbox.Width + actualDiv.Width + extend, shiftTop);
                        pathWriter.Stop();

                        customVsxBox.BeforeBaseBox = actualDiv.Width + extend;
                        break;
                    case EncloseNotation.radical:
                        GlyphBox radical = NewGlyphBox();
                        radical.Character = (char)0x221A;
                        AssignGlyphVxs(radical);
                        radical.Layout();

                        Box actualRadical = StretchHeightIfStretchable(radical, hbox.Height + over);
                        actualRadical.Layout();
                        float shiftLeft1 = maxLeft - actualRadical.Width;
                        float shiftTop1 = maxTop - over;
                        actualRadical.SetLocation(shiftLeft1, -shiftTop1 - over);
                        customVsxBox.NotationBox = actualRadical;

                        pathWriter.MoveTo(shiftLeft1 + actualRadical.Width, shiftTop1);
                        pathWriter.LineTo(shiftLeft1 + actualRadical.Width + hbox.Width + extend, shiftTop1);
                        pathWriter.Stop();

                        customVsxBox.BeforeBaseBox = actualRadical.Width + extend;
                        break;
                    case EncloseNotation.roundedbox:
                        using (Tools.BorrowRoundedRect(out var roundedRect))
                        {
                            roundedRect.SetRadius(over, over, over, over, over, over, over, over);
                            roundedRect.SetRect(0, 0, w, h);
                            roundedRect.MakeVxs(vsx1);
                            customVsxBox.CustomVxs = stroke.CreateTrim(vsx1);
                        }
                        customVsxBox.BeforeBaseBox = over;
                        break;
                    case EncloseNotation.circle:
                        using (Tools.BorrowEllipse(out Ellipse ellipse))
                        {
                            float xLength = hbox.Width / 2 + maxLeft;
                            float yLength = hbox.Height / 2 + maxTop;

                            ellipse.Set(xLength, yLength, xLength, yLength);
                            ellipse.MakeVxs(vsx1);
                            customVsxBox.CustomVxs = stroke.CreateTrim(vsx1);
                            customVsxBox.BeforeBaseBox = maxLeft;
                        }
                        break;
                }
                if (useVxs > 0)
                {
                    if (useVxs == 1)
                    {
                        customVsxBox.CustomVxs = stroke.CreateTrim(vsx1);
                    }
                    else if (useVxs == 2)
                    {
                        customVsxBox.CustomVxs = stroke.CreateTrim(vsx2);
                    }
                    encloseBox.NotationBoxs.Add(customVsxBox);
                }
            }
        }
    }
}