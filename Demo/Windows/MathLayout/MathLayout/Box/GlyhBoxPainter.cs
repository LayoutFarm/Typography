//MIT, 2020, Brezza92
using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.VertexProcessing;
using MathLayout;

namespace LayoutFarm.MathLayout
{
    public class GlyphBoxPainter
    {
        public GlyphBoxPainter() { }
        public Painter Painter { get; set; }
        Random _random = new Random();
        public void Paint(Box box)
        {
            switch (box.Kind)
            {
                default: throw new Exception();
                case BoxKind.StretchBox:
                    PaintStretchBox((StretchCharBox)box);
                    break;
                case BoxKind.StackBox:
                    PaintStackBox((StackBox)box);
                    break;
                case BoxKind.StackGroup:
                    PaintStackGroup((StackGroup)box);
                    break;
                case BoxKind.StackCarries:
                    PaintStackCarries((StackCarries)box);
                    break;
                case BoxKind.StackRow:
                    PaintStackRow((StackRow)box);
                    break;
                case BoxKind.StackCarry:
                    PaintCarryBox((StackCarryBox)box);
                    break;
                case BoxKind.SpaceBox://skip
                    break;
                case BoxKind.MultiScriptBox:
                    PaintMultiScriptBox((MultiScriptBox)box);
                    break;
                case BoxKind.UnderscriptOverscriptBox:
                    PaintUnderscriptOverscriptBox((UnderscriptOverscriptBox)box);
                    break;
                case BoxKind.SubscriptSuperscriptBox:
                    PaintSubscriptSuperScriptBox((SubscriptSuperscriptBox)box);
                    break;
                case BoxKind.Enclose:
                    PaintEncloseBox((EncloseBox)box);
                    break;
                case BoxKind.TableBox:
                    PaintTableBox((TableBox)box);
                    break;
                case BoxKind.StackLine:
                    PaintStackLine((StackLine)box);
                    break;
                case BoxKind.LineBox:
                    PaintLineBox((LineBox)box);
                    break;
                case BoxKind.FractionBox:
                    PaintFractionBox((FractionBox)box);
                    break;
                case BoxKind.RadicalBox:
                    PaintRadicalBox((RadicalBox)box);
                    break;
                case BoxKind.GlyphBox:
                    PaintGlyphBox((GlyphBox)box);
                    break;
                case BoxKind.HorizontalBox:
                    PaintHorizontalBox((HorizontalStackBox)box);
                    break;
                case BoxKind.VerticalBox:
                    PaintVerticalBox((VerticalStackBox)box);
                    break;
                case BoxKind.CustomNotationVsx:
                    PaintCustomVsx((CustomNotationVsxBox)box);
                    break;
            }

        }

        private void PaintStretchBox(StretchCharBox box)
        {
            float ox = Painter.OriginX;//***
            float oy = Painter.OriginY;
            Painter.SetOrigin(box.Left + ox, box.Top + oy);
            Paint(box.StretchContainer);
            Painter.SetOrigin(ox, oy);//restore
        }

        private void PaintCustomVsx(CustomNotationVsxBox box)
        {
            float ox = Painter.OriginX;//***
            float oy = Painter.OriginY;
            Color color = Color.Black;
            Painter.SetOrigin(box.Left + ox, box.Top + oy);
            if (box.NotationBox != null)
            {
                Paint(box.NotationBox);
            }
            if (box.CustomVxs != null)
            {
                Painter.Fill(box.CustomVxs, color);
            }
            
            Painter.SetOrigin(ox, oy);//restore
        }

        private void PaintEncloseBox(EncloseBox box)
        {
            float ox = Painter.OriginX;//***
            float oy = Painter.OriginY;

            Painter.SetOrigin(box.Left + ox, box.Top + oy);
            Paint(box.BaseBox);
            foreach(Box b in box.NotationBoxs)
            {
                Paint(b);
            }
            Painter.SetOrigin(ox, oy);//restore
        }

        private void PaintFractionBox(FractionBox box)
        {
            float ox = Painter.OriginX;//***
            float oy = Painter.OriginY;

            Painter.SetOrigin(box.Left + ox, box.Top + oy);
            Paint(box.Fraction);
            Painter.SetOrigin(ox, oy);//restore
        }
        private void PaintRadicalBox(RadicalBox box)
        {
            float ox = Painter.OriginX;//***
            float oy = Painter.OriginY;

            Painter.SetOrigin(box.Left + ox, box.Top + oy);
            if (box.Degree != null)
            {
                Paint(box.Degree);
            }
            Paint(box.Radical);
            Paint(box.BaseBox);
            Painter.SetOrigin(ox, oy);//restore
        }
        private void PaintCarryBox(StackCarryBox box)
        {
            float ox = Painter.OriginX;//***
            float oy = Painter.OriginY;

            Painter.SetOrigin(box.Left + ox, box.Top + oy);
            Paint(box.Carry);
            Painter.SetOrigin(ox, oy);//restore
        }

        private void PaintStackLine(StackLine box)
        {
            Painter.StrokeWidth = box.StrokeWidth;
            Painter.StrokeColor = Color.Black;

            float offset = 0.01f;//when start point and end point are on X=0 line disappear then add little point
            Painter.DrawLine(
                box.StartPoint.X + box.Left + offset,
                box.StartPoint.Y + box.Top,
                box.EndPoint.X + box.Left + offset,
                box.EndPoint.Y + box.Top);
        }

        private void PaintStackRow(StackRow stackRow)
        {
            float ox = Painter.OriginX;//***
            float oy = Painter.OriginY;

            Painter.SetOrigin(stackRow.Left + ox, stackRow.Top + oy);
            _horizontalHeight.Push(stackRow.Height);
            _fromPaintHorizontal.Push(true);
            int count = stackRow.ChildCount;
            for (int i = 0; i < count; ++i)
            {
                Paint(stackRow.GetChild(i));
            }
            _fromPaintHorizontal.Pop();
            _horizontalHeight.Pop();
            Painter.SetOrigin(ox, oy);//restore
        }

        private void PaintStackCarries(StackCarries stackCarries)
        {
            float ox = Painter.OriginX;//***
            float oy = Painter.OriginY;

            Painter.SetOrigin(stackCarries.Left + ox, stackCarries.Top + oy);
            _horizontalHeight.Push(stackCarries.Height);
            _fromPaintHorizontal.Push(true);
            int count = stackCarries.ChildCount;
            for (int i = 0; i < count; ++i)
            {
                Paint(stackCarries.GetChild(i));
            }
            _fromPaintHorizontal.Pop();
            _horizontalHeight.Pop();
            Painter.SetOrigin(ox, oy);//restore
        }

        private void PaintStackGroup(StackGroup stackGroup)
        {
            float ox = Painter.OriginX;//***
            float oy = Painter.OriginY;

            int count = stackGroup.ChildCount;
            Painter.SetOrigin(stackGroup.Left + ox, stackGroup.Top + oy);
            _fromPaintHorizontal.Push(false);
            for (int i = 0; i < count; ++i)
            {
                Paint(stackGroup.GetChild(i));
            }
            _fromPaintHorizontal.Pop();
            Painter.SetOrigin(ox, oy);//restore
        }

        private void PaintStackBox(StackBox stackBox)
        {
            float ox = Painter.OriginX;//***
            float oy = Painter.OriginY;

            int count = stackBox.ChildCount;
            Painter.SetOrigin(stackBox.Left + ox, stackBox.Top + oy);
            _fromPaintHorizontal.Push(false);
            for (int i = 0; i < count; ++i)
            {
                Paint(stackBox.GetChild(i));
            }
            _fromPaintHorizontal.Pop();
            Painter.SetOrigin(ox, oy);//restore
        }

        private void PaintUnderscriptOverscriptBox(UnderscriptOverscriptBox box)
        {
            float ox = Painter.OriginX;//***
            float oy = Painter.OriginY;

            Painter.SetOrigin(box.Left + ox, box.Top + oy);
            _fromPaintHorizontal.Push(false);
            Paint(box.BaseBox);
            if (box.OverscriptBox != null)
            {
                Paint(box.OverscriptBox);
            }
            if (box.UnderscriptBox != null)
            {
                Paint(box.UnderscriptBox);
            }
            _fromPaintHorizontal.Pop();
            Painter.SetOrigin(ox, oy);//restore
        }

        private void PaintSubscriptSuperScriptBox(SubscriptSuperscriptBox box)
        {
            float ox = Painter.OriginX;//***
            float oy = Painter.OriginY;

            Painter.SetOrigin(box.Left + ox, box.Top + oy);
            Paint(box.BaseBox);
            if (box.SuperscriptBox != null)
            {
                Paint(box.SuperscriptBox);
            }
            if (box.SubscriptBox != null)
            {
                Paint(box.SubscriptBox);
            }
            Painter.SetOrigin(ox, oy);//restore
        }

        private void PaintMultiScriptBox(MultiScriptBox box)
        {
            float ox = Painter.OriginX;//***
            float oy = Painter.OriginY;

            Painter.SetOrigin(box.Left + ox, box.Top + oy);
            _fromPaintHorizontal.Push(true);
            _horizontalHeight.Push(box.Height);
            if (box.Prescripts != null)
            {
                foreach(Box pre in box.Prescripts)
                {
                    if (pre != null)
                    {
                        Paint(pre);
                    }
                }
            }
            Paint(box.BaseBox);
            if (box.PostScripts != null)
            {
                foreach(Box post in box.PostScripts)
                {
                    if (post != null)
                    {
                        Paint(post);
                    }
                }
            }
            _horizontalHeight.Pop();
            _fromPaintHorizontal.Pop();
            Painter.SetOrigin(ox, oy);//restore
        }

        private void PaintTableBox(TableBox box)
        {
            int childCount = box.ChildCount;
            float ox = Painter.OriginX;//***
            float oy = Painter.OriginY;

            Painter.SetOrigin(box.Left + ox, box.Top + oy);
            for (int i = 0;i < childCount; i++)
            {
                Paint(box.GetChild(i));
            }
            Painter.SetOrigin(ox, oy);//restore
        }

        private void PaintLineBox(LineBox box)
        {
            Painter.StrokeWidth = box.StrokeWidth;
            Painter.StrokeColor = Color.Black;

            float offset = 0.01f;//when start point and end point are on X=0 line disappear then add little point
            Painter.DrawLine(
                box.StartPoint.X + box.Left + offset,
                box.StartPoint.Y + box.Top,
                box.EndPoint.X + box.Left + offset,
                box.EndPoint.Y + box.Top);
        }

        void PaintGlyphBox(GlyphBox box)
        {
            if (box.IsInvisible)
            {
                return;
            }
            float ox = Painter.OriginX;//***
            float oy = Painter.OriginY;

            if (box.GlyphVxs != null)
            {
                Color color = Color.Black;
                float shiftHeight = 0;
                float shiftWidth = 0;
                Q1RectD glyphBound = box.GlyphVxs.GetBoundingRect();

                {
                    if (_fromPaintHorizontal.Count > 0 && _fromPaintHorizontal.Peek() && _horizontalHeight.Count > 0)
                    {
                        shiftHeight = _horizontalHeight.Peek();
                    }
                    else
                    {
                        shiftHeight = box.Height;
                    }
                }
                shiftHeight -= box.Height - (float)glyphBound.Height;
                if (MathMLOperatorTable.IsStretchyPropertyOperator(box.Character+""))
                {
                    float bottom = (float)box.GlyphVxs.GetBoundingRect().Bottom;
                    float top = (float)box.GlyphVxs.GetBoundingRect().Top;
                    if (MathMLOperatorTable.IsFencePropertyOperator(box.Character + ""))
                    {
                        if (box.Stretched)
                        {
                            shiftHeight -= top;
                        }
                    }
                    else if (_fromPaintHorizontal.Peek())
                    {
                        shiftHeight -= top;
                    }
                }
                
                float x = box.Left + ox + shiftWidth;
                float y = box.Top + oy + shiftHeight;
                
                Painter.SetOrigin(box.Left + ox + shiftWidth, box.Top + oy + shiftHeight);

                Painter.Fill(box.GlyphVxs, color);
                Painter.SetOrigin(ox, oy);//restore
            }
            else
            {
                Color color = Color.FromArgb(100, Color.Black);
                Painter.FillRect(box.Left + ox, box.Top + oy, box.Width, box.Height, color);
            }
        }
        Stack<float> _horizontalHeight = new Stack<float>();
        Stack<bool> _fromPaintHorizontal = new Stack<bool>();
        void PaintHorizontalBox(HorizontalStackBox hbox)
        {
            float ox = Painter.OriginX;//***
            float oy = Painter.OriginY;

            Painter.SetOrigin(hbox.Left + ox, hbox.Top + oy);
            _horizontalHeight.Push(hbox.Height);
            _fromPaintHorizontal.Push(true);
            int count = hbox.ChildCount;
            for (int i = 0; i < count; ++i)
            {
                Paint(hbox.GetChild(i));
            }
            _fromPaintHorizontal.Pop();
            _horizontalHeight.Pop();
            Painter.SetOrigin(ox, oy);//restore

        }
        void PaintVerticalBox(VerticalStackBox vbox)
        {
            float ox = Painter.OriginX;//***
            float oy = Painter.OriginY;

            int count = vbox.ChildCount;
            Painter.SetOrigin(vbox.Left + ox, vbox.Top + oy);
            _fromPaintHorizontal.Push(false);
            for (int i = 0; i < count; ++i)
            {
                Paint(vbox.GetChild(i));
            }
            _fromPaintHorizontal.Pop();
            Painter.SetOrigin(ox, oy);//restore
        }
    }
}