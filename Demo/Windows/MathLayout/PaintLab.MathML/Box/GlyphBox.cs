//MIT, 2020, Brezza92
using System.Collections.Generic;

using MathLayout;
using Typography.OpenFont.MathGlyphs;

namespace LayoutFarm.MathLayout
{
    public enum BoxKind
    {
        HorizontalBox,
        VerticalBox,
        FractionBox,
        RadicalBox,
        TableBox,
        GlyphBox,
        LineBox,
        SpaceBox,
        SubscriptSuperscriptBox,
        UnderscriptOverscriptBox,
        StretchBox,
        MultiScriptBox,
        Enclose,
        StackBox,
        StackRow,
        StackGroup,
        StackCarries,
        StackCarry,
        StackLine,
        CustomNotationVsx,
    }
    public abstract class Box
    {
#if DEBUG
        static int s_dbugBox = 0;
        public int dbugBoxId = 0;
        public Box()
        {
            s_dbugBox++;
            dbugBoxId = s_dbugBox;
        }
#endif
        public MathNode MathNode { get => GetMathNode(); set => SetMathNode(value); }
        public MathConstants MathConstants { get; set; }
        public float PixelScale { get; set; }
        public float BaseLineShift { get; set; }
        public float Left { get; set; }
        public float Top { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Depth { get; set; }
        public Box Parent { get; set; }

        private float _topAccentScale = 0;
        public bool HasTopAccentAttachment { get; private set; }
        public float TopAccentAttachmentScale
        {
            get => _topAccentScale;
            set
            {
                _topAccentScale = value;
                HasTopAccentAttachment = true;
            }
        }
        public Alignment Align { get; set; }
        public abstract BoxKind Kind { get; }
        public abstract void Layout();
        MathNode _mathNode;
        protected virtual MathNode GetMathNode()
        {
            return _mathNode;
        }
        protected virtual void SetMathNode(MathNode node)
        {
            _mathNode = node;
        }
        public void SetLocation(float left, float top)
        {
            Left = left;
            Top = top;
        }
        public void SetSize(float width, float height)
        {
            Width = width;
            Height = height;
        }
        public void SetBounds(float left, float top, float width, float height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }
        public float MarginLeft { get; set; }
        public float MarginTop { get; set; }
        public float MarginRight { get; set; }
        public float MarginBottom { get; set; }

    }

    public static class MathMLNumUnitConverter
    {
        public static MathMLNumWithUnit ConvertFrom(MathMLNumWithUnit from, MathMLNumUnit unit)
        {
            throw new System.NotSupportedException();
        }

        private static float GetCompareWithPoint(MathMLNumUnit unit)
        {
            //1 point = n unit
            switch (unit)
            {
                default:
                    throw new System.NotSupportedException();
                case MathMLNumUnit.None:
                case MathMLNumUnit.Points:
                    return 1;
                case MathMLNumUnit.Inches:
                    return 72;
                case MathMLNumUnit.Picas:
                    return 1 / 12f;
                case MathMLNumUnit.Centimeters:
                    return 72 / 2.54f;//(1 inch = 2.54 centimeters)
                case MathMLNumUnit.Millimeters:
                    return 72 / 25.4f;
                case MathMLNumUnit.Pixels:
                case MathMLNumUnit.Percentage:
                case MathMLNumUnit.EM:
                case MathMLNumUnit.EX:
                    //TODO:
                    return 0;
            }
        }
    }
    public class MathMLNumWithUnit
    {
        public float Number { get; set; }
        public MathMLNumUnit Unit { get; set; }
    }

    public class LineBox : Box
    {
        public override BoxKind Kind => BoxKind.LineBox;
        public bool StretchHeight { get; set; }
        public bool StretchWidth { get; set; }
        public float StrokeWidth { get; set; }
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public LineBox()
        {
            StrokeWidth = 1;
        }
        public void SetAsBar(float lenght, bool isVericalBar = false)
        {
            StartPoint = new Point(0, 0);
            if (isVericalBar)
            {
                EndPoint = new Point(0, (int)lenght);
            }
            else
            {
                EndPoint = new Point((int)lenght, 0);
            }
        }
        public override void Layout()
        {
            this.Height = System.Math.Abs(EndPoint.Y - StartPoint.Y);
            this.Width = System.Math.Abs(EndPoint.X - StartPoint.X);

            if (this.Height < 1)
            {
                this.Height = StrokeWidth;
            }
            if (this.Width < 1)
            {
                this.Width = StrokeWidth;
            }
        }
    }
    public class SubscriptSuperscriptBox : Box
    {
        public override BoxKind Kind => BoxKind.SubscriptSuperscriptBox;
        public Box BaseBox { get; set; }
        public Box SuperscriptBox { get; set; }
        public Box SubscriptBox { get; set; }
        public float SubscriptShiftDown { get; set; }//default font shiftdown
        public float SuperscriptShiftUp { get; set; }//default font shiftdown
        public float SuperscriptShift { get; set; }//user attribute custom
        public float SubscriptShift { get; set; }//use attribute custom
        protected override void SetMathNode(MathNode node)
        {
            base.SetMathNode(node);

            string subscriptshift = node.GetAttributeValue("subscriptshift");
            string superscriptshift = node.GetAttributeValue("superscriptshift");
        }
        public override void Layout()
        {
            BaseBox.Layout();
            float sumHeight = BaseBox.Height;
            float scriptMaxWidth = 0;
            if (SuperscriptBox != null)
            {
                SuperscriptBox.Layout();
                scriptMaxWidth = System.Math.Max(scriptMaxWidth, SuperscriptBox.Width);
                SuperscriptBox.Top = SuperscriptBox.MarginTop - SuperscriptShiftUp;
                SuperscriptBox.Left = SuperscriptBox.MarginLeft + BaseBox.Width;
                sumHeight += SuperscriptShiftUp + SuperscriptBox.Height - BaseBox.Height;
            }
            if (SubscriptBox != null)
            {
                SubscriptBox.Layout();
                scriptMaxWidth = System.Math.Max(scriptMaxWidth, SubscriptBox.Width);
                SubscriptBox.Top = SubscriptBox.MarginTop + SubscriptShiftDown;
                SubscriptBox.Left = SubscriptBox.MarginLeft + BaseBox.Width;
            }
            this.Height = sumHeight;
            this.Width = BaseBox.Width + scriptMaxWidth;
            this.PixelScale = BaseBox.PixelScale;
        }
    }

    public class MultiScriptBox : Box
    {
        public override BoxKind Kind => BoxKind.MultiScriptBox;

        public Box BaseBox { get; set; }
        public List<Box> Prescripts { get; private set; }
        public List<Box> PostScripts { get; private set; }
        public float SubscriptShiftDown { get; set; }//default font shiftdown
        public float SuperscriptShiftUp { get; set; }//default font shiftdown

        public MultiScriptBox()
        {
            Prescripts = new List<Box>();
            PostScripts = new List<Box>();
        }

        public void AddMultiScript(Box subScriptBox, Box superScriptBox)
        {
            //Add even null
            PostScripts.Add(subScriptBox);
            PostScripts.Add(superScriptBox);
        }

        public void AddMutiPrescript(Box subScriptBox, Box superScriptBox)
        {
            //Add even null
            Prescripts.Add(subScriptBox);
            Prescripts.Add(superScriptBox);
        }
        public override void Layout()
        {
            BaseBox.Layout();

            float left = 0;
            float maxSupHeight = 0;
            //Layput Prescript
            if (Prescripts.Count > 0)
            {
                if (Prescripts.Count % 2 == 0)
                {
                    int scriptPair = Prescripts.Count / 2;
                    for (int i = 0; i < scriptPair; i++)
                    {
                        Box subBox = Prescripts[i * 2];
                        Box supBox = Prescripts[(i * 2) + 1];

                        float maxWidth = 0;
                        if (subBox != null)
                        {
                            subBox.Layout();
                            maxWidth = subBox.Width + subBox.MarginLeft;
                            subBox.Top = subBox.MarginTop + SubscriptShiftDown;
                            subBox.Left = left;
                        }
                        if (supBox != null)
                        {
                            supBox.Layout();
                            maxWidth = System.Math.Max(maxWidth, supBox.Width + supBox.MarginLeft);
                            supBox.Top = supBox.MarginTop - SuperscriptShiftUp;
                            supBox.Left = left;
                            maxSupHeight = System.Math.Max(supBox.Height, maxSupHeight);
                        }
                        left += maxWidth;
                    }
                }
            }
            //LayoutBaseBox
            BaseBox.Left = left;
            left += BaseBox.Width;

            //Layput Postscript
            if (PostScripts.Count > 0)
            {
                if (PostScripts.Count % 2 == 0)
                {
                    int scriptPair = PostScripts.Count / 2;
                    for (int i = 0; i < scriptPair; i++)
                    {
                        Box subBox = PostScripts[i * 2];
                        Box supBox = PostScripts[(i * 2) + 1];

                        float maxWidth = 0;
                        if (subBox != null)
                        {
                            subBox.Layout();
                            maxWidth = subBox.Width;
                            subBox.Top = subBox.MarginTop + SubscriptShiftDown;
                            subBox.Left = subBox.MarginLeft + left;
                        }
                        if (supBox != null)
                        {
                            supBox.Layout();
                            maxWidth = System.Math.Max(maxWidth, supBox.Width);
                            supBox.Top = supBox.MarginTop - SuperscriptShiftUp;
                            supBox.Left = supBox.MarginLeft + left;
                            maxSupHeight = System.Math.Max(supBox.Height, maxSupHeight);
                        }
                        left += maxWidth;
                    }
                }
            }

            this.Width = left;
            this.Height = BaseBox.Height + maxSupHeight;
            this.PixelScale = BaseBox.PixelScale;
        }
    }
    public class UnderscriptOverscriptBox : Box
    {
        public override BoxKind Kind => BoxKind.UnderscriptOverscriptBox;
        public Box BaseBox { get; set; }
        public Box OverscriptBox { get; set; }
        public Box UnderscriptBox { get; set; }

        public bool Accent { get; set; }
        public bool AccentUnder { get; set; }

        public UnderscriptOverscriptBox()
        {
            Align = Alignment.Center;
            Accent = false;
            AccentUnder = false;
        }
        protected override void SetMathNode(MathNode node)
        {
            base.SetMathNode(node);

            string alignStr = node.GetAttributeValue("align");
            Align = AttributeParser.ParseAlignment(alignStr);
            string accentStr = node.GetAttributeValue("accent");
            Accent = AttributeParser.ParseBoolean(accentStr, false);
            string accentUnderStr = node.GetAttributeValue("accentunder");
            AccentUnder = AttributeParser.ParseBoolean(accentUnderStr, false);
        }


        public override void Layout()
        {
            BaseBox.Layout();
            float sumHeight = 0;
            float scriptMaxWidth = BaseBox.Width;
            if (BaseBox.HasTopAccentAttachment)
            {
                this.TopAccentAttachmentScale = BaseBox.TopAccentAttachmentScale;
            }
            if (OverscriptBox != null)
            {
                bool isAccentBox = false, isTopLine = false;

                if (OverscriptBox is GlyphBox gbox)
                {
                    if (MathMLOperatorTable.IsStretchyPropertyOperator(gbox.Character + ""))
                    {
                        if (BaseBox is GlyphBox baseGlyph)
                        {
                            gbox.ScaleToFitWidth((float)baseGlyph.GetBoundingRect().Width);
                        }
                        else
                        {
                            gbox.ScaleToFitWidth(BaseBox.Width);
                        }
                    }

                    var bounding = gbox.GetBoundingRect();

                    OverscriptBox.MarginTop = -(float)(bounding.Top + bounding.Height);
                    //if (MathMLOperatorTable.IsAccentPropertyOperator(gbox.Character + ""))
                    {
                        //isAccentBox = true;
                        if (gbox.GlyphIndex == 2256 || gbox.Character == '_')//special condition for overline and lowline
                        {
                            isTopLine = true;
                        }
                    }

                }
                else
                {
                    OverscriptBox.MarginTop = MathConstants.AxisHeight.Value * BaseBox.PixelScale;
                }
                OverscriptBox.Layout();
                if (!isTopLine)
                {
                    OverscriptBox.Top = OverscriptBox.MarginTop + OverscriptBox.Height;
                    sumHeight += OverscriptBox.Height + (Accent ? BaseBox.TopAccentAttachmentScale / 4f : BaseBox.TopAccentAttachmentScale);
                }
                else
                {
                    if (isTopLine)
                    {
                        OverscriptBox.Top = OverscriptBox.MarginTop + OverscriptBox.Height;
                        sumHeight += OverscriptBox.Height * 2 + OverscriptBox.MarginBottom;
                    }

                }

                if (OverscriptBox.Width < scriptMaxWidth)//center
                {
                    OverscriptBox.Left = OverscriptBox.MarginLeft + ((scriptMaxWidth - OverscriptBox.Width) / 2.0f);
                }
                scriptMaxWidth = System.Math.Max(scriptMaxWidth, OverscriptBox.Width);
            }
            sumHeight += BaseBox.MarginTop;
            BaseBox.Top = sumHeight;
            sumHeight += BaseBox.Height + BaseBox.MarginBottom;
            float depth = BaseBox.Depth;
            if (UnderscriptBox != null)
            {
                bool isLowLine = false, isAccentChar = false;
                if (UnderscriptBox is GlyphBox gbox)
                {
                    if (MathMLOperatorTable.IsStretchyPropertyOperator(gbox.Character + ""))
                    {
                        if (BaseBox is GlyphBox baseGlyph)
                        {
                            gbox.ScaleToFitWidth((float)baseGlyph.GetBoundingRect().Width);
                        }
                        else
                        {
                            gbox.ScaleToFitWidth(BaseBox.Width);
                        }
                    }

                    var bounding = gbox.GetBoundingRect();
                    UnderscriptBox.MarginTop = (float)(-bounding.Top - bounding.Height);
                    if (MathMLOperatorTable.IsAccentPropertyOperator(gbox.Character + ""))
                    {
                        isAccentChar = true;

                        //TODO: review here, (temp fix for latin-modern?)
                        if (gbox.GlyphIndex == 2256 || gbox.Character == '_')//special condition for overline and lowline
                        {
                            isLowLine = true;
                        }
                    }

                }
                else
                {
                    if (UnderscriptBox is StretchCharBox stretchChar)
                    {
                        isAccentChar = true;
                        var bounding = stretchChar.OriginalGlyph.GetBoundingRect();
                        UnderscriptBox.MarginTop = (float)(-bounding.Top - bounding.Height);
                        //UnderscriptBox.MarginTop = MathConstants.AxisHeight.Value * BaseBox.PixelScale;
                        //UnderscriptBox.MarginTop = 1000;
                    }
                    else
                    {
                        UnderscriptBox.MarginTop = -MathConstants.AxisHeight.Value * BaseBox.PixelScale;
                    }
                }
                UnderscriptBox.Layout();
                scriptMaxWidth = System.Math.Max(scriptMaxWidth, UnderscriptBox.Width);
                UnderscriptBox.Top = UnderscriptBox.MarginTop + sumHeight + BaseBox.Depth;
                float accunder = UnderscriptBox.Height + System.Math.Abs(AccentUnder ? BaseBox.TopAccentAttachmentScale / 4f : BaseBox.TopAccentAttachmentScale);
                if (isAccentChar && !isLowLine)
                {
                    float accentH = UnderscriptBox.Height + (System.Math.Abs(BaseBox.TopAccentAttachmentScale) * (AccentUnder ? -1 : 1));
                    UnderscriptBox.Top += accentH;
                    depth += accentH;
                }
                if (isLowLine)
                {
                    UnderscriptBox.Top += UnderscriptBox.Height * 2;
                    depth += UnderscriptBox.Height * 2;
                }
                else
                {
                    UnderscriptBox.Top += accunder;
                    depth += accunder;
                }
                if (UnderscriptBox.Width < scriptMaxWidth)//center
                {
                    UnderscriptBox.Left = UnderscriptBox.MarginLeft + ((scriptMaxWidth - UnderscriptBox.Width) / 2.0f);
                }
            }
            this.Height = sumHeight;
            this.Width = scriptMaxWidth;
            this.PixelScale = BaseBox.PixelScale;
            this.BaseLineShift = BaseBox.BaseLineShift;
            this.Depth = depth;
        }
    }
    public class StretchCharBox : Box
    {
        public ContainerBox StretchContainer { get; set; }
        public GlyphBox OriginalGlyph { get; set; }

        public override BoxKind Kind => BoxKind.StretchBox;

        public override void Layout()
        {
            StretchContainer.Layout();
            this.Height = StretchContainer.Height;
            this.Width = StretchContainer.Width;
            this.BaseLineShift = StretchContainer.BaseLineShift;
            this.Depth = StretchContainer.Depth;
            this.PixelScale = StretchContainer.PixelScale;
        }
    }

    public class SpaceBox : Box
    {
        public override BoxKind Kind => BoxKind.SpaceBox;
        public SpaceBox(float width, float height)
        {
            Width = width;
            Height = height;
        }
        public override void Layout()
        {

        }
    }
    public abstract class GlyphBox : Box
    {
        public override BoxKind Kind => BoxKind.GlyphBox;
        public GlyphBox() { }

        public char Character { get; set; }
        public bool Stretched { get; set; }
        public int GlyphIndex { get; set; }
        public bool IsInvisible { get; set; }


        public abstract void ScaleToFitWidth(float width);
        public abstract void ScalteToFitHeight(float height);
        public abstract Rect GetBoundingRect();
        
        public abstract bool HasVxs { get; }
        public abstract void ClearVxs();

        public MathGlyphInfo MathGlyphInfo { get; set; }
        public int AdvanceWidthScale { get; set; }
        protected bool _alreadyLayout = false;
        public override void Layout()
        {
#if DEBUG
            if (dbugBoxId == 31)
            {

            }
#endif
            if (_alreadyLayout)
            {
                return;
            }
            if (HasVxs)
            {
                var bounds = GetBoundingRect();
                if (MathMLOperatorTable.IsLargeOpPropertyOperator(Character + ""))
                {
                    this.Depth = (float)bounds.Top;
                }
                this.Width = AdvanceWidthScale;
                this.Height = (float)bounds.Height;
                _alreadyLayout = true;
            }
        }
    }

    public abstract class ContainerBox : Box
    {
        protected List<Box> _children = new List<Box>();
        public int ChildCount => _children.Count;
        public float AboveBaseLineShift { get; set; }
        public float UnderBaseLineShift { get; set; }
        public Box GetChild(int index) => _children[index];
        public virtual void Replace(int index, Box box)
        {
            _children[index] = box;
        }

        public virtual void Insert(int index, Box child)
        {
            _children.Insert(index, child);
        }
        public virtual void Remove(int index)
        {
            _children.RemoveAt(index);
        }
        public virtual void AddChild(Box child)
        {
            //child box align horizontally
            child.Parent = this;
            _children.Add(child);
        }
    }

    public class HorizontalStackBox : ContainerBox
    {
        public override BoxKind Kind => BoxKind.HorizontalBox;
        public override void Layout()
        {
            int j = _children.Count;

            float left = 0, top = 0;
            float latestMarginRight = 0;
            float maxHeight = 0;
            float minHeight = 0;
            float maxTopAccent = 0;
            int maxVertical = -1;

            float maxFractionLineTop = 0;
            float maxPixelScale = 0;
            float lowestBaseLineShift = 0;
            float highestBaseLineShift = 0;

            float maxAboveBLS = 0;
            int maxAboveIndex = -1;
            int highestBLSindex = -1;
            for (int i = 0; i < j; ++i)
            {
                Box b = _children[i];
                b.Layout();
                if (maxPixelScale < b.PixelScale)
                {
                    maxPixelScale = b.PixelScale;
                }
                if (b.BaseLineShift < 0)
                {
                    if (lowestBaseLineShift < 0)
                    {
                        if (lowestBaseLineShift < b.BaseLineShift)
                        {
                            lowestBaseLineShift = b.BaseLineShift;
                        }
                    }
                    else
                    {
                        lowestBaseLineShift = b.BaseLineShift;
                        maxVertical = i;
                    }
                    if (highestBaseLineShift > b.BaseLineShift)
                    {
                        highestBaseLineShift = b.BaseLineShift;
                        highestBLSindex = i;
                        maxVertical = i;
                    }
                }
                float aboveBLS = 0;
                if (b is FractionBox fbox)
                {
                    aboveBLS = fbox.Height - System.Math.Abs(fbox.BaseLineShift);
                    if (!fbox.Bevelled)
                    {
                        maxFractionLineTop = System.Math.Max(maxFractionLineTop, fbox.FractionLine.Top + fbox.FractionLine.Height);
                    }

                }
                else if (b is ContainerBox cbox)
                {
                    aboveBLS = cbox.AboveBaseLineShift;
                }
                else
                {
                    aboveBLS = b.Height - System.Math.Abs(b.BaseLineShift);
                }

                if (maxAboveBLS < aboveBLS)
                {
                    maxAboveBLS = aboveBLS;
                    maxAboveIndex = i;
                    if (highestBaseLineShift == b.BaseLineShift && highestBaseLineShift != 0)
                    {
                        maxVertical = i;
                    }
                }

                if (b.Height > maxHeight)
                {
                    maxHeight = b.Height;
                    if (b.Kind == BoxKind.VerticalBox)
                    {
                    }
                    maxVertical = i;
                }
                if (b.TopAccentAttachmentScale > maxTopAccent)
                {
                    maxTopAccent = b.TopAccentAttachmentScale;
                }
                if (b.Height != 0 && b.Height < minHeight)
                {
                    minHeight = b.Height;
                }
                if (i == 0)
                {
                    left += b.MarginLeft;
                    latestMarginRight = b.MarginRight;
                    minHeight = b.Height;
                }
                else
                {
                    left += latestMarginRight + b.MarginLeft;
                }
                top = b.MarginTop;
                b.SetLocation(left, top); //same top

                left += b.Width;
                latestMarginRight = b.MarginRight;
            }

            float actualHigh = System.Math.Abs(highestBaseLineShift) + maxAboveBLS;
            float additionShift = 0;
            if (actualHigh > maxHeight)
            {
                if (highestBLSindex >= 0 && maxAboveIndex >= 0 && highestBLSindex != maxAboveIndex)
                {
                    Box b = _children[maxAboveIndex];
                    if (b.BaseLineShift < 0)
                    {
                        additionShift = actualHigh - maxHeight;
                        maxHeight = actualHigh;
                    }
                }
            }

            float extraH = 0;
            for (int i = 0; i < j; i++)
            {
                Box b = _children[i];
                if (b is UnderscriptOverscriptBox || b is SubscriptSuperscriptBox)
                {
                    float temp = b.Height - highestBaseLineShift + b.BaseLineShift;
                    if (temp > actualHigh)
                    {
                        extraH = temp - actualHigh;
                        actualHigh = temp;
                    }
                }
            }

            this.Width = left; //expand this box
            this.Height = maxHeight;

            this.PixelScale = maxPixelScale;
            this.BaseLineShift = highestBaseLineShift;
            this.AboveBaseLineShift = maxHeight - System.Math.Abs(highestBaseLineShift);
            if (maxTopAccent != 0)
                this.TopAccentAttachmentScale = maxTopAccent;

            float baseLine = highestBaseLineShift;
            float runtimeHeight = this.Height;
            for (int i = 0; i < j; ++i)
            {
                Box b = _children[i];
                if (i == maxVertical)
                {
                    b.Top = additionShift + extraH;
                    continue;
                }
                if (b is GlyphBox gbox)
                {
                    if (MathMLOperatorTable.IsFencePropertyOperator(gbox.Character + ""))
                    {
                        continue;
                    }
                    gbox.Top = baseLine;
                }
                else if (b is FractionBox fbox)
                {
                    float accent = ((MathConstants.AccentBaseHeight.Value * this.PixelScale) / 2);
                    if (!fbox.Bevelled)
                    {
                        float diff = baseLine - b.BaseLineShift + additionShift + maxHeight - b.Height;
                        fbox.Top = diff;
                    }
                    else
                    {
                        float diff = b.BaseLineShift - baseLine + additionShift;
                        if (fbox.Fraction is ContainerBox cbox)
                        {
                            b.Top = diff;
                        }
                        else
                        {
                            b.Top = diff + b.Height;
                        }
                    }
                }
                else if (b is VerticalStackBox vbox)
                {
                    vbox.Top = (maxHeight - vbox.Height) / 2;
                    vbox.Top = baseLine;
                }
                else if (b is UnderscriptOverscriptBox underOverBox)
                {
                    underOverBox.MarginTop = this.Height - underOverBox.Height;
                    if (underOverBox.BaseBox is GlyphBox undOvrBase)
                    {
                        if (MathMLOperatorTable.IsFencePropertyOperator(undOvrBase.Character + ""))
                        {
                            underOverBox.Top = underOverBox.MarginTop;
                            continue;
                        }
                    }
                    underOverBox.Top = underOverBox.MarginTop + baseLine - underOverBox.BaseLineShift;
                }
                else if (b is SubscriptSuperscriptBox subsupBox)
                {
                    if (subsupBox.BaseBox is GlyphBox)
                    {
                        subsupBox.Top = baseLine;
                    }
                    else
                    {
                        subsupBox.MarginTop = this.Height - subsupBox.BaseBox.Height;
                        subsupBox.Top = baseLine + subsupBox.MarginTop;
                    }
                }
                else if (b is SpaceBox || b is LineBox)
                {
                    b.Top = baseLine;
                    continue;
                }
                else
                {
                    if (b.BaseLineShift < 0)
                    {
                        float accent = ((MathConstants.AccentBaseHeight.Value * this.PixelScale) / 2);
                        float diff = b.BaseLineShift - baseLine + additionShift;
                        if (b is ContainerBox cbox)
                        {
                            b.Top = diff + (accent / 2);
                        }
                        else
                        {
                            b.Top = diff + b.Height;
                        }
                    }
                    else
                    {
                        b.MarginTop = this.Height - b.Height;
                        b.Top = b.MarginTop + baseLine;
                    }
                }
            }

            this.Height = runtimeHeight;
        }
    }
    public class VerticalStackBox : ContainerBox
    {
        public override BoxKind Kind => BoxKind.VerticalBox;

        public override void Layout()
        {
            int j = _children.Count;

            float left = 0, top = 0;
            float latestMarginBottom = 0;
            float max_width = 0;
            float maxTopAccent = 0;
            float maxPixelScale = 0;
            for (int i = 0; i < j; ++i)
            {
                Box b = _children[i];
                b.Layout();
                if (maxPixelScale < b.PixelScale)
                {
                    maxPixelScale = b.PixelScale;
                }
                if (b.Width > max_width)
                {
                    max_width = b.Width;
                }
                if (b.TopAccentAttachmentScale > maxTopAccent)
                {
                    maxTopAccent = b.TopAccentAttachmentScale;
                }
                if (i == 0)
                {
                    top += b.MarginTop;
                    latestMarginBottom = b.MarginBottom;
                }
                else
                {
                    top += latestMarginBottom + b.MarginTop;
                }

                b.SetLocation(left + b.MarginLeft, top); //same top
                top += b.Height;
                latestMarginBottom = b.MarginBottom;
            }
            this.Height = top;
            this.Width = max_width;
            this.PixelScale = maxPixelScale;
            if (maxTopAccent != 0)
                this.TopAccentAttachmentScale = maxTopAccent;

        }
    }
    public class FractionBox : Box
    {
        public override BoxKind Kind => BoxKind.FractionBox;
        public Box Numerator { get; set; }
        public LineBox FractionLine { get; set; }
        public Box Denominator { get; set; }
        public ContainerBox Fraction { get; private set; }
        public bool Bevelled { get; set; }
        public float LineThickness { get; set; }
        public float AboveBaseLineShift { get; set; }
        protected override void SetMathNode(MathNode node)
        {
            Bevelled = AttributeParser.ParseBoolean(node.GetAttributeValue("bevelled"), false);

            base.SetMathNode(node);
        }
        public override void Layout()
        {
            if (FractionLine != null)
                FractionLine.StrokeWidth = LineThickness;
            if (Bevelled)
            {
                if (Fraction != null && Fraction is HorizontalStackBox)
                {
                    Fraction.Layout();
                }
                else
                {
                    Fraction = new HorizontalStackBox();
                    Fraction.AddChild(Numerator);
                    Fraction.AddChild(FractionLine);
                    Fraction.AddChild(Denominator);
                    Fraction.Layout();
                    FractionLine.StartPoint = new Point(0, (int)(Fraction.Height));
                    FractionLine.EndPoint = new Point((int)(Fraction.Height * 0.3), 0);
                }
                float accent = ((MathConstants.AxisHeight.Value * Fraction.PixelScale));
                if (Numerator is VerticalStackBox vbox)
                {
                    Box space = vbox.GetChild(1);
                    space.Height = accent;
                }
                Fraction.AboveBaseLineShift = Numerator.Height;
            }
            else
            {
                if (Fraction != null && Fraction is VerticalStackBox)
                {
                    Fraction.Layout();
                }
                else
                {
                    Fraction = new VerticalStackBox();
                    Fraction.AddChild(Numerator);
                    Fraction.AddChild(FractionLine);
                    Fraction.AddChild(Denominator);
                    Fraction.Layout();
                }
                float accent = ((MathConstants.AxisHeight.Value * Fraction.PixelScale));
                Fraction.BaseLineShift = (Numerator.MarginTop + Numerator.Height + Numerator.MarginBottom + FractionLine.MarginTop) + accent - Fraction.Height;
                Fraction.AboveBaseLineShift = Numerator.Height + FractionLine.MarginTop;
            }
            this.Height = Fraction.Height;
            this.Width = Fraction.Width;
            this.PixelScale = Fraction.PixelScale;
            this.BaseLineShift = Fraction.BaseLineShift;
            this.AboveBaseLineShift = Fraction.AboveBaseLineShift;
        }
    }

    public class RadicalBox : Box
    {
        public Box Degree { get; set; }
        public Box Radical { get; set; }
        public Box BaseBox { get; set; }
        public override BoxKind Kind => BoxKind.RadicalBox;

        public override void Layout()
        {
            //TODO:
            //Degree.Layout();
            Radical.Layout();
            BaseBox.Layout();
            BaseBox.SetLocation(Radical.Width, 0);
            this.Height = System.Math.Max(Radical.Height, BaseBox.Height);
            this.Width = Radical.Width + BaseBox.Width;
            this.BaseLineShift = BaseBox.BaseLineShift;
            this.PixelScale = BaseBox.PixelScale;
        }
    }
    public class TableBox : ContainerBox
    {
        public override BoxKind Kind => BoxKind.TableBox;
        public override void Layout()
        {
            List<float> columnsWidths = new List<float>();
            int row = ChildCount;
            float top = 0;
            float latestMarginBottom = 0;
            float maxPixelScale = 0;
            for (int rowIndex = 0; rowIndex < row; rowIndex++)
            {
                Box box = _children[rowIndex];
                box.Layout();
                if (maxPixelScale < box.PixelScale)
                {
                    maxPixelScale = box.PixelScale;
                }
                if (rowIndex == 0)
                {
                    top += box.MarginTop;
                    latestMarginBottom = box.MarginBottom;
                }
                else
                {
                    top += latestMarginBottom + box.MarginTop;
                }
                box.Top = top;
                top += box.Height;
                latestMarginBottom = box.MarginBottom;

                if (box is HorizontalStackBox hbox)
                {
                    int col = hbox.ChildCount;
                    for (int colIndex = 0; colIndex < col; colIndex++)
                    {
                        Box hboxChild = hbox.GetChild(colIndex);
                        if (columnsWidths.Count >= colIndex + 1)
                        {
                            float maxWidth = System.Math.Max(hboxChild.Width, columnsWidths[colIndex]);
                            columnsWidths[0] = maxWidth;
                        }
                        else
                        {
                            columnsWidths.Add(hboxChild.Width);
                        }
                    }
                }
                else
                {
                    if (columnsWidths.Count > 0)
                    {
                        float maxWidth = System.Math.Max(box.Width, columnsWidths[0]);
                        columnsWidths[0] = maxWidth;
                    }
                    else
                    {
                        columnsWidths.Add(box.Width);
                    }
                }
            }

            for (int rowIndex = 0; rowIndex < row; rowIndex++)
            {
                Box box = _children[rowIndex];

                if (box is HorizontalStackBox hbox)
                {
                    int col = hbox.ChildCount;
                    for (int colIndex = 0; colIndex < col; colIndex++)
                    {
                        float colMaxWidth = columnsWidths[colIndex];
                        Box hboxChild = hbox.GetChild(colIndex);
                        if (hboxChild.Width < colMaxWidth)
                        {
                            HorizontalStackBox storageBox = new HorizontalStackBox();
                            storageBox.AddChild(hboxChild);
                            storageBox.SetBounds(hboxChild.Left, hboxChild.Top, colMaxWidth, hboxChild.Height);

                            float shift = (colMaxWidth - hboxChild.Width) / 2;
                            hboxChild.Left += shift;

                            hbox.Replace(colIndex, storageBox);
                        }
                    }
                }
                else
                {
                    float colMaxWidth = columnsWidths[0];
                    if (box.Width < colMaxWidth)
                    {
                        HorizontalStackBox storageBox = new HorizontalStackBox();
                        storageBox.AddChild(box);
                        storageBox.SetBounds(box.Left, box.Top, colMaxWidth, box.Height);

                        float shift = (colMaxWidth - box.Width) / 2;
                        box.Left += shift;

                        _children[rowIndex] = storageBox;
                    }
                }
            }

            this.Width = Sum(columnsWidths);
            this.Height = top;
            this.PixelScale = maxPixelScale;
        }
        static float Sum(List<float> list)
        {
            int j = list.Count;
            float sum = 0;
            for (int i = 0; i < j; ++i)
            {
                sum += list[i];
            }
            return sum;
        }
    }

    public class EncloseBox : Box
    {
        public override BoxKind Kind => BoxKind.Enclose;

        public EncloseBox()
        {
            NotationBoxs = new List<Box>();
        }
        public float TopSpace { get; set; }
        public float BottomSpace { get; set; }
        public float LeftSpace { get; set; }
        public float RightSpace { get; set; }
        public EncloseNotation Notation { get; set; }
        public List<Box> NotationBoxs { get; set; }
        public Box BaseBox { get; set; }
        public override void Layout()
        {
            float maxH = 0;
            float maxW = 0;

            BaseBox.Layout();
            maxH = BaseBox.Height;
            maxW = BaseBox.Width + BaseBox.MarginLeft;
            float beforeBaseBox = 0;
            foreach (Box b in NotationBoxs)
            {
                b.Layout();
            }
            if (beforeBaseBox > 0)
            {
                foreach (Box b in NotationBoxs)
                {
                    if (b is CustomNotationVsxBox customBox)
                    {
                        customBox.Left = LeftSpace - (customBox.MarginLeft + customBox.BeforeBaseBox);
                    }
                    else
                    {
                        b.Left = b.MarginLeft;
                    }
                }
            }

            BaseBox.Left = BaseBox.MarginLeft + LeftSpace;
            BaseBox.Top = TopSpace;
            this.Height = maxH + TopSpace + BottomSpace;
            this.Width = maxW + LeftSpace + RightSpace;
        }
    }

    public abstract class CustomNotationVsxBox : Box
    {
        public override BoxKind Kind => BoxKind.CustomNotationVsx;
        public float BeforeBaseBox { get; set; }

        public Box NotationBox { get; set; }

    }
}