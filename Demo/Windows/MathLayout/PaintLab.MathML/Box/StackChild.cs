//MIT, 2020, Brezza92

using System.Collections.Generic;
using System;

using MathLayout;
using PixelFarm.Drawing;

namespace LayoutFarm.MathLayout
{
    public abstract class StackChild : ContainerBox
    {
        public int Position { get => _position; set => SetStackPosition(value); }
        private int _position = 0;
        private void SetStackPosition(int position)
        {
            _position = position;
            Structure.Position = position;
        }
        public DigitsStructure Structure = new DigitsStructure();

        internal abstract void Build();
        protected override void SetMathNode(MathNode node)
        {
            Position = AttributeParser.ParseInteger(node.GetAttributeValue("position"), 0);
            base.SetMathNode(node);
        }
        protected void HorizontalLayout()
        {
            int count = Structure.Digits.Count;

            float left = 0, top = 0;
            float latestMarginRight = 0;
            float maxHeight = 0;
            float minHeight = 0;
            float maxTopAccent = 0;
            int maxVertical = -1;
            float expectNumberWidth = Structure.NumberAndSpaceWidth;
            for (int i = 0; i < count; ++i)
            {
                DigitInfo info = Structure.Digits[i];
                Box b = info.Digit;
                b.Layout();
                if (b.Height > maxHeight)
                {
                    maxHeight = b.Height;
                    if (b.Kind == BoxKind.VerticalBox)
                    {
                        maxVertical = i;
                    }
                }
                if (b.TopAccentAttachmentScale > maxTopAccent)
                {
                    maxTopAccent = b.TopAccentAttachmentScale;
                }
                if (b.Height != 0 && b.Height < minHeight)
                {
                    minHeight = b.Height;
                }
                if (info.Type == DigitType.Number || info.Type == DigitType.Space)
                {
                    if (b.Width < expectNumberWidth)
                    {
                        float margin = (expectNumberWidth - b.Width) / 2f;
                        b.MarginLeft = margin;
                        b.MarginRight = margin;
                    }
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

            this.Width = left; //expand this box
            this.Height = maxHeight;
            if (maxTopAccent != 0)
                this.TopAccentAttachmentScale = maxTopAccent;

            float baseLine = 0;
            count = ChildCount;
            for (int i = 0; i < count; ++i)
            {
                if (i == maxVertical)
                {
                    continue;
                }
                Box b = _children[i];
                if (b is GlyphBox gbox)
                {
                    gbox.Top -= baseLine;
                }
                else
                {
                    b.MarginTop = this.Height - b.Height;
                    b.Top = b.MarginTop - baseLine;
                }
            }
        }
        protected void VerticalLayout()
        {
            int j = _children.Count;
            //this.MarginLeft = Structure.MarginLeft;
            float left = 0, top = 0;
            float latestMarginBottom = 0;
            float max_width = 0;
            float maxTopAccent = 0;
            for (int i = 0; i < j; ++i)
            {
                Box b = _children[i];
                b.Layout();


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

                if (b.Width > max_width)
                {
                    max_width = b.Width;
                }

                b.SetLocation(left + b.MarginLeft, top); //same top
                top += b.Height;
                latestMarginBottom = b.MarginBottom;
            }
            this.Height = top;
            this.Width = max_width;
            if (maxTopAccent != 0)
                this.TopAccentAttachmentScale = maxTopAccent;
        }
    }

    public class DigitsStructure
    {
        public DigitsStructure()
        {
            Digits = new List<DigitInfo>();
        }
        public List<DigitInfo> Digits { get; private set; }
        public float NumberAndSpaceWidth { get; set; }

        public DigitInfo LeftMost
        {
            get
            {
                if (Digits.Count > 0)
                {
                    return Digits[0];
                }
                return null;
            }
        }
        public DigitInfo LeftMostNumber
        {
            get
            {
                if (Digits.Count > 0)
                {
                    foreach (DigitInfo info in Digits)
                    {
                        if (info.Type == DigitType.Number)
                        {
                            return info;
                        }
                    }
                }
                return null;
            }
        }

        public float Width
        {
            get
            {
                float sum = 0;
                int count = Digits.Count;
                for (int i = 0; i < count; i++)
                {
                    DigitInfo info = Digits[i];
                    if (info.Type == DigitType.Operator && i > 0)
                    {
                        continue;
                    }
                    if (info.Type == DigitType.Number || info.Type == DigitType.Space)
                    {
                        sum += NumberAndSpaceWidth;
                    }
                    else
                    {
                        sum += info.Digit.Width + info.Digit.MarginLeft + info.Digit.MarginRight;
                    }
                }
                return sum;
            }
        }
        public int Position { get => _position; set => UpdatePosition(value); }
        private int _position = 0;
        private bool _foundDecimalPoint = false;
        private int _currentDecimal = -1;
        public void Add(DigitInfo digitInfo)
        {
            digitInfo.Digit.Layout();
            switch (digitInfo.Type)
            {
                default: throw new NotSupportedException();
                case DigitType.Number:
                case DigitType.Space:
                    //
                    NumberAndSpaceWidth = Math.Max(NumberAndSpaceWidth, digitInfo.Digit.Width);
                    if (_foundDecimalPoint)
                    {
                        digitInfo.Position = _currentDecimal--;
                    }
                    else
                    {
                        digitInfo.Position = Position;
                        foreach (DigitInfo info in Digits)
                        {
                            info.Position++;
                        }
                    }
                    break;
                case DigitType.DecimalPoint:
                    _foundDecimalPoint = true;
                    break;
                case DigitType.Seperator:
                    digitInfo.Position = Position;
                    break;
                case DigitType.Operator://ex. '+' ',' '-' operator
                    break;
            }
            Digits.Add(digitInfo);
        }
        internal int GetIndexOf(int position, DigitType type)
        {
            int count = Digits.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                DigitInfo digitInfo = Digits[i];
                if (digitInfo.Position == position)
                {
                    if ((digitInfo.Type & type) == digitInfo.Type)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        public DigitInfo GetDigitAt(int position, DigitType type)
        {
            foreach (DigitInfo digitInfo in Digits)
            {
                if (digitInfo.Position == position)
                {
                    if ((digitInfo.Type & type) == digitInfo.Type)
                    {
                        return digitInfo;
                    }
                }
            }
            return null;
        }
        private void UpdatePosition(int position)
        {
            foreach (DigitInfo digitInfo in Digits)
            {
                digitInfo.Position = digitInfo.Position - _position + position;
            }
            _position = position;
        }
        public float GetLengthFromStartTo(DigitInfo targetInfo)
        {
            if (targetInfo == null)
            {
                return 0;
            }
            int count = Digits.Count;
            float sum = 0;
            for (int i = 0; i < count; i++)
            {
                DigitInfo info = Digits[i];
                if (info.Position == targetInfo.Position && info.Type == targetInfo.Type)
                {
                    break;
                }
                if (info.Type == DigitType.Operator && i > 0)
                {
                    continue;
                }
                if (info.Position >= targetInfo.Position)
                {
                    if (info.Type == DigitType.Number || info.Type == DigitType.Space)
                    {
                        sum += NumberAndSpaceWidth;
                    }
                    else
                    {
                        sum += info.Digit.Width + info.Digit.MarginLeft + info.Digit.MarginRight;
                    }
                }
            }
            return sum;
        }

        public void Clear()
        {
            Digits.Clear();
        }
    }

    public enum DigitType
    {
        Number = 1,
        Space = 2,
        DecimalPoint = 4,
        Operator = 8,
        Seperator = 16,
    }
    public class DigitInfo
    {
        public Box Digit { get; set; }
        public DigitType Type { get; set; }
        public int Position { get; set; }
    }

    public class StackBox : ContainerBox
    {
        public override BoxKind Kind => BoxKind.StackBox;

        public DigitsStructure StackStructure { get; private set; }
        public override void AddChild(Box child)
        {
            if (child is StackChild)
            {
                base.AddChild(child);
            }
            else
            {
                StackRow stackRow = new StackRow();
                stackRow.AddChild(child);
                base.AddChild(stackRow);
            }

        }
        public override void Layout()
        {
            Build();

            int count = ChildCount;
            float left = 0, top = 0;
            float latestMarginBottom = 0;
            float maxTopAccent = 0;
            for (int i = 0; i < count; i++)
            {
                StackChild stackChild = GetChild(i) as StackChild;
                if (stackChild != null)
                {
                    stackChild.Layout();
                    if (stackChild.TopAccentAttachmentScale > maxTopAccent)
                    {
                        maxTopAccent = stackChild.TopAccentAttachmentScale;
                    }
                    if (i == 0)
                    {
                        top += stackChild.MarginTop;
                        latestMarginBottom = stackChild.MarginBottom;
                    }
                    else
                    {
                        top += latestMarginBottom + stackChild.MarginTop;
                    }
                    {
                        DigitInfo leftMost = stackChild.Structure.LeftMost;
                        float shift = 0;
                        if (leftMost.Type == DigitType.Number || leftMost.Type == DigitType.Space)
                        {
                            shift = StackStructure.GetLengthFromStartTo(leftMost);
                        }
                        else
                        {
                            leftMost = stackChild.Structure.LeftMostNumber;
                            float beforeLeftNum = stackChild.Structure.GetLengthFromStartTo(leftMost);
                            shift = StackStructure.GetLengthFromStartTo(leftMost) - beforeLeftNum;
                        }
                        stackChild.SetLocation(shift + stackChild.MarginLeft, top); //same top
                        top += stackChild.Height;
                        latestMarginBottom = stackChild.MarginBottom;
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            this.Height = top;
            this.Width = StackStructure.Width;
            if (maxTopAccent != 0)
                this.TopAccentAttachmentScale = maxTopAccent;

        }

        private void Build()
        {
            StackStructure = new DigitsStructure();
            int count = ChildCount;
            List<StackCarries> temp = new List<StackCarries>();
            Comparison<DigitInfo> comparison = (a, b) =>
            {
                if (a.Position == b.Position)
                {
                    return a.Type.CompareTo(b.Type);
                }
                return b.Position.CompareTo(a.Position);
            };
            for (int i = 0; i < count; i++)
            {
                Box box = GetChild(i);
                if (box is StackCarries carries)
                {
                    temp.Add(carries);
                    continue;
                }
                if (box is StackChild child)
                {
                    child.Build();
                    CombineStructure(child.Structure, StackStructure);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            foreach (StackCarries carries1 in temp)
            {
                carries1.Build();
                DigitInfo last = carries1.Structure.Digits[carries1.Structure.Digits.Count - 1];
                ReplaceCarriesInfoWith(carries1, StackStructure);
                CombineStructure(carries1.Structure, StackStructure);
            }
            for (int i = 0; i < count; i++)
            {
                StackChild box = GetChild(i) as StackChild;
                box.Structure.NumberAndSpaceWidth = StackStructure.NumberAndSpaceWidth;
            }
        }

        private void ReplaceCarriesInfoWith(StackCarries carries, DigitsStructure stackStructure)
        {
            DigitsStructure carriesStructure = carries.Structure;
            if (carriesStructure.Digits.Count > 0)
            {
                int carriesCount = carriesStructure.Digits.Count;
                DigitInfo lastCarry = carriesStructure.Digits[carriesCount - 1];
                int stackIndex = stackStructure.GetIndexOf(lastCarry.Position, lastCarry.Type);
                if (stackIndex >= 0)
                {
                    for (int istack = stackIndex, icarries = carriesCount - 1; istack >= 0; istack--)
                    {
                        if (icarries < 0)
                        {
                            break;
                        }
                        DigitInfo stackDigitInfo = stackStructure.Digits[istack];
                        if (stackDigitInfo.Type != DigitType.Operator)
                        {
                            DigitInfo carry = carriesStructure.Digits[icarries];
                            carry.Position = stackDigitInfo.Position;
                            carry.Type = stackDigitInfo.Type;
                            if (carry.Digit.Width < stackDigitInfo.Digit.Width)
                            {
                                float margin = (stackDigitInfo.Digit.Width - carry.Digit.Width) / 2f;
                                carry.Digit.MarginLeft = margin;
                                carry.Digit.MarginRight = margin;
                            }
                            icarries--;
                        }
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        private void CombineStructure(DigitsStructure source, DigitsStructure target)
        {
            int count = source.Digits.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                DigitInfo sourceInfo = source.Digits[i];
                DigitInfo targetInfo = null;
                if (sourceInfo.Type == DigitType.Number || sourceInfo.Type == DigitType.Space)
                {
                    targetInfo = target.GetDigitAt(sourceInfo.Position, DigitType.Number | DigitType.Space);
                }
                else
                {
                    targetInfo = target.GetDigitAt(sourceInfo.Position, sourceInfo.Type);
                }
                if (targetInfo == null)
                {
                    DigitInfo digitInfo = new DigitInfo();
                    digitInfo.Digit = new SpaceBox(sourceInfo.Digit.Width, sourceInfo.Digit.Height);
                    digitInfo.Position = sourceInfo.Position;
                    digitInfo.Type = sourceInfo.Type;
                    target.Digits.Insert(0, digitInfo);
                }
            }
            if (source.NumberAndSpaceWidth < target.NumberAndSpaceWidth)
            {
                source.NumberAndSpaceWidth = target.NumberAndSpaceWidth;
            }
            else
            {
                target.NumberAndSpaceWidth = source.NumberAndSpaceWidth;
            }

        }
    }

    public class StackRow : StackChild
    {
        public override BoxKind Kind => BoxKind.StackRow;

        public override void Layout()
        {
            HorizontalLayout();
        }

        internal override void Build()
        {
            Structure.Clear();
            Construction(this);
        }

        private void Construction(ContainerBox container)
        {
            int count = container.ChildCount;
            for (int i = 0; i < count; i++)
            {
                Box box = container.GetChild(i);
                if (box is GlyphBox gbox)
                {
                    DigitInfo digitInfo = new DigitInfo();
                    digitInfo.Digit = gbox;
                    if (char.IsDigit(gbox.Character))
                    {
                        digitInfo.Type = DigitType.Number;
                    }
                    else if (gbox.Character == ' ')
                    {
                        digitInfo.Type = DigitType.Space;
                    }
                    else if (gbox.Character == '.')
                    {
                        digitInfo.Type = DigitType.DecimalPoint;
                    }
                    else if (gbox.Character == ',')
                    {
                        digitInfo.Type = DigitType.Seperator;
                    }
                    else
                    {
                        digitInfo.Type = DigitType.Operator;
                    }
                    Structure.Add(digitInfo);
                }
                else if (box is ContainerBox cbox)
                {
                    Construction(cbox);
                }
                else if (box is SpaceBox space)
                {
                    DigitInfo digitInfo = new DigitInfo();
                    digitInfo.Digit = space;
                    digitInfo.Type = DigitType.Space;
                    Structure.Add(digitInfo);
                }
            }
        }
    }

    public class StackGroup : StackChild
    {
        public override BoxKind Kind => BoxKind.StackGroup;
        public int Shift { get; set; }
        public override void AddChild(Box child)
        {
            if (child is StackRow)
            {
                base.AddChild(child);
            }
            else
            {
                StackRow stackRow = new StackRow();
                stackRow.AddChild(child);
                base.AddChild(stackRow);
            }
        }

        public override void Layout()
        {
            int count = ChildCount;
            float top = 0;
            float latestMarginBottom = 0;
            float maxTopAccent = 0;
            for (int i = 0; i < count; i++)
            {
                StackChild stackChild = GetChild(i) as StackChild;
                if (stackChild != null)
                {
                    stackChild.Layout();
                    if (stackChild.TopAccentAttachmentScale > maxTopAccent)
                    {
                        maxTopAccent = stackChild.TopAccentAttachmentScale;
                    }
                    if (i == 0)
                    {
                        top += stackChild.MarginTop;
                        latestMarginBottom = stackChild.MarginBottom;
                    }
                    else
                    {
                        top += latestMarginBottom + stackChild.MarginTop;
                    }
                    DigitInfo leftMost = stackChild.Structure.LeftMost;
                    float shift = 0;
                    if (leftMost.Type == DigitType.Number || leftMost.Type == DigitType.Space)
                    {
                        shift = Structure.GetLengthFromStartTo(leftMost);
                    }
                    else
                    {
                        leftMost = stackChild.Structure.LeftMostNumber;
                        float beforeLeftNum = stackChild.Structure.GetLengthFromStartTo(leftMost);
                        shift = Structure.GetLengthFromStartTo(leftMost) - beforeLeftNum;
                    }
                    stackChild.SetLocation(shift + stackChild.MarginLeft, top); //same top
                    top += stackChild.Height;
                    latestMarginBottom = stackChild.MarginBottom;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            this.Height = top;
            this.Width = Structure.Width;
            if (maxTopAccent != 0)
                this.TopAccentAttachmentScale = maxTopAccent;
        }

        internal override void Build()
        {
            Structure.Clear();
            int count = ChildCount;
            for (int i = 0; i < count; i++)
            {
                StackRow row = GetChild(i) as StackRow;
                if (row != null)
                {
                    row.Position = Position + (i * Shift);
                    row.Build();
                    CombineStructure(row.Structure, Structure);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        private void CombineStructure(DigitsStructure source, DigitsStructure target)
        {
            int count = source.Digits.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                DigitInfo sourceInfo = source.Digits[i];
                DigitInfo targetInfo = null;
                if (sourceInfo.Type == DigitType.Number || sourceInfo.Type == DigitType.Space)
                {
                    targetInfo = target.GetDigitAt(sourceInfo.Position, DigitType.Number | DigitType.Space);
                }
                else
                {
                    targetInfo = target.GetDigitAt(sourceInfo.Position, sourceInfo.Type);
                }
                if (targetInfo == null)
                {
                    DigitInfo digitInfo = new DigitInfo();
                    digitInfo.Digit = new SpaceBox(sourceInfo.Digit.Width, sourceInfo.Digit.Height);
                    digitInfo.Position = sourceInfo.Position;
                    digitInfo.Type = sourceInfo.Type;
                    target.Digits.Insert(0, digitInfo);
                }
            }
            if (source.NumberAndSpaceWidth < target.NumberAndSpaceWidth)
            {
                source.NumberAndSpaceWidth = target.NumberAndSpaceWidth;
            }
            else
            {
                target.NumberAndSpaceWidth = source.NumberAndSpaceWidth;
            }
        }
    }

    public class StackCarries : StackChild
    {
        public override BoxKind Kind => BoxKind.StackCarries;
        public CarryLocation Location { get; set; }
        public CarryCrossout Crossout { get; set; }
        public float ScriptSizeMultiplier { get; set; }
        public override void Layout()
        {
            HorizontalLayout();
        }

        internal override void Build()
        {
            Structure.Clear();
            Construction(this);
        }

        private void Construction(ContainerBox container)
        {
            int count = container.ChildCount;
            for (int i = 0; i < count; i++)
            {
                Box box = container.GetChild(i);
                DigitInfo digitInfo = new DigitInfo();
                digitInfo.Digit = box;
                if (box is SpaceBox)
                {
                    digitInfo.Type = DigitType.Space;
                }
                else
                {
                    digitInfo.Type = DigitType.Number;
                }
                Structure.Add(digitInfo);
            }
        }
    }

    public class StackCarryBox : Box
    {
        public override BoxKind Kind => BoxKind.StackCarry;
        public Box Carry { get; set; }
        public CarryLocation Location { get; set; }
        public CarryCrossout Crossout { get; set; }
        public override void Layout()
        {
            Carry.Layout();
            this.Width = Carry.Width;
            this.Height = Carry.Height;
        }
    }

    public class StackLine : StackChild
    {
        public override BoxKind Kind => BoxKind.StackLine;
        public float StrokeWidth { get; set; }
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

        public uint Length { get; set; }
        public float LeftOverHang { get; set; }
        public float RightOverHang { get; set; }
        public float MSLineThickness { get; set; }
        public override void Layout()
        {
            if (Parent is StackChild mschild)
            {
                Structure = mschild.Structure;
            }
            else if (Parent is StackBox sbox)
            {
                Structure = sbox.StackStructure;
            }

            if (Structure != null)
            {
                EndPoint = new Point((int)Structure.Width, 0);
            }
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

        internal override void Build()
        {
            //throw new NotImplementedException();
        }
    }
}