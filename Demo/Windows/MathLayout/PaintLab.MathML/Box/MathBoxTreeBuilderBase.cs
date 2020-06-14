//MIT, 2020, Brezza92
using System;
using System.Collections.Generic;
using System.IO;

using Typography.OpenFont;
using Typography.OpenFont.MathGlyphs;
using LayoutFarm.MathLayout;

namespace MathLayout
{

    public abstract class MathBoxTreeBuilderBase
    {
        Typeface _typeface;
        float _scriptScale = 0.7f;
        float _scriptScriptScale = 0.5f;
        GlyphBox _spaceGlyph;
        bool isScriptScript = false;
        public MathBoxTreeBuilderBase()
        {
            FontSize = 20;
        }
        public Typeface MathTypeface
        {
            get => _typeface;
            set
            {
                if (_typeface == value) { return; }
                //
                _typeface = value;
                _mathConstants = _typeface.MathConsts;
                if (_mathConstants != null)
                {
                    _scriptScale = _mathConstants.ScriptPercentScaleDown / 100f;
                    _scriptScriptScale = _mathConstants.ScriptScriptPercentScaleDown / 100f;
                } 
                FontChanged(); 
            }
        } 

        protected abstract GlyphBox NewGlyphBox();
        protected abstract CustomNotationVsxBox NewCustomVxsBox();

        public int ScriptLevel { get; private set; }
        float _fontSize = 0;
        public float FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                FontChanged();
            }
        }


        public float GetCurrentLevelFontSize()
        {
            if (ScriptLevel == 0)
            {
                return FontSize;
            }
            else if (ScriptLevel == 1)
            {
                return FontSize * _scriptScale;
            }
            else if (ScriptLevel >= 2)
            {
                return FontSize * _scriptScriptScale;
            }
            else
            {
                float result = FontSize;
                for (int i = 0; i < ScriptLevel; i++)
                {
                    if (isScriptScript)
                    {
                        result *= _scriptScriptScale;
                    }
                    else
                    {
                        result *= _scriptScale;
                    }
                }
                return result;
            }
        }


        void FontChanged()
        {
            if (_typeface != null)
            {
                _spaceGlyph = NewGlyphBox();
                _spaceGlyph.Character = ' ';
                AssignGlyphVxs(_spaceGlyph);
            }
        }

        Box CreateSpaceGlyph()
        {

            GlyphBox space = NewGlyphBox();
            space.Character = ' ';
            AssignGlyphVxs(space);
            return space;

        }
        public Box CreateMathBoxs(List<math> nodes)
        {
            VerticalStackBox vbox = new VerticalStackBox();
            int count = nodes.Count;
            for (int i = 0; i < count; i++)
            {
                vbox.AddChild(CreateMathBox(nodes[i]));
            }
            return vbox;
        }
        public Box CreateMathBox(MathNode node)
        {
            //create box foreach node
            //TODO: most box are glyph box + its text content
            //except some boxes are Horizontal (eg. mrow) or some box are vertical (...)
            //this should be config from DomSpec.xml or Autogen code             
            Box result = null;
            switch (node.Name)
            {
                default:
                    {
                        result = CreateTextBox(node);
                    }
                    break;
                case "mstyle":
                case "merror":
                case "mpadded":
                case "mlongdiv":
                case "maction":
                    //TODO: implement next version
                    result = null;
                    break;
                case "math":
                case "mrow":
                case "mlabeledtr":
                case "mtr":
                case "mtd":
                    {
                        result = CreateHorizontalBox(node);
                    }
                    break;
                case "mfrac":
                    {
                        result = CreateFractionsBox(node);
                    }
                    break;
                case "msqrt":
                case "mroot":
                    {
                        result = CreateRadicalBox(node);
                    }
                    break;
                case "munder":
                    {
                        result = CreateUnderscriptBox(node);
                    }
                    break;
                case "mover":
                    {
                        result = CreateOverscriptBox(node);
                    }
                    break;
                case "munderover":
                    {
                        result = CreateUnderscriptOverscriptBox(node);
                    }
                    break;
                case "msub":
                    {
                        result = CreateSubscriptBox(node);
                    }
                    break;
                case "msup":
                    {
                        result = CreateSuperscriptBox(node);
                    }
                    break;
                case "msubsup":
                    {
                        result = CreateSubscriptSuperscriptBox(node);
                    }
                    break;
                case "mprescript"://must be child in mmultiscript
                    throw new NotSupportedException("support only in mmultiscript tag");
                case "mspace":
                case "none":
                    {
                        result = CreateSpaceGlyph();
                    }
                    break;
                case "mmultiscripts":
                    {
                        result = CreateMultiScriptBox(node);
                    }
                    break;
                case "mphantom":
                    {
                        result = CreatePhantomBox(node);
                    }
                    break;
                case "mfenced":
                    {
                        result = CreateFencedBox(node);
                    }
                    break;
                case "mtable":
                    {
                        result = CreateTableBox(node);
                    }
                    break;
                case "mstack":
                    {
                        result = CreateStackBox(node);
                    }
                    break;
                case "msgroup":
                    {
                        result = CreateStackGroup(node);
                    }
                    break;
                case "msrow":
                    {
                        result = CreateStackRow(node);
                    }
                    break;
                case "msline":
                    {
                        result = CreateStackLine(node);
                    }
                    break;
                case "mscarries":
                    {
                        result = CreateCarriesBox(node);
                    }
                    break;
                case "mscarry":
                    {
                        result = CreateCarryBox(node);
                    }
                    break;
                case "menclose":
                    {
                        result = CreateEncloseBox(node);
                    }
                    break;
            }
            if (result != null)
            {
                result.MathNode = node;
                result.MathConstants = _mathConstants;
                AssignGlyphVxs(result);
                if ((result is ContainerBox) && !(result is StackChild))
                {
                    result.Layout();
                    StretchHeightIfFenceAndStretchable(result, result.Height);
                }

            }
            return result;
        }

        Box CreateEncloseBox(MathNode node)
        {
            EncloseBox encloseBox = new EncloseBox();
            encloseBox.MathNode = node;

            HorizontalStackBox hbox = new HorizontalStackBox();
            FillContainerBox(hbox, node);
            hbox.MathNode = node;
            hbox.MathConstants = _mathConstants;

            string[] notationsStr = node.GetAttributeValue("notation").Split(' ');//maybe multiple notation
            int length = notationsStr.Length;
            EncloseNotation[] notations = new EncloseNotation[length];

            hbox.Layout();
            encloseBox.BaseBox = hbox;

            float over = _mathConstants.OverbarVerticalGap.Value * GetPixelScale();
            float under = _mathConstants.UnderbarVerticalGap.Value * GetPixelScale();
            float thickness = _mathConstants.OverbarRuleThickness.Value * GetPixelScale();
            float maxTop, maxBottom, maxLeft, maxRight;
            maxTop = maxBottom = maxLeft = maxRight = 0;
            bool alwaySymmetric = false;
            for (int i = 0; i < length; i++)
            {
                EncloseNotation notation = AttributeParser.ParseEnum(notationsStr[i], EncloseNotation.longdiv);
                notations[i] = notation;
                switch (notation)
                {
                    default:
                        //always symmetric
                        maxTop = maxBottom = Math.Max(maxTop, over * 2);
                        maxLeft = maxRight = Math.Max(maxLeft, over * 2);
                        alwaySymmetric = true;
                        break;
                    case EncloseNotation.updiagonalstrike:
                    case EncloseNotation.downdiagonalstrike:
                    case EncloseNotation.verticalstrike:
                    case EncloseNotation.horizontalstrike:
                        //asymmetric possible
                        maxLeft = Math.Max(maxLeft, over * 2);
                        maxRight = Math.Max(maxRight, over * 2);
                        maxTop = Math.Max(maxTop, over * 2);
                        maxBottom = Math.Max(maxBottom, under * 2);
                        break;
                    case EncloseNotation.circle:
                        float xRadius = Math.Max((hbox.Width * 1.45f) / 2f, hbox.Width / 2f + _mathConstants.StackDisplayStyleGapMin.Value * GetPixelScale());
                        float yRadius = Math.Max((hbox.Height * 1.4f) / 2f, hbox.Height / 2f + _mathConstants.StackBottomShiftDown.Value * GetPixelScale());

                        maxTop = maxBottom = Math.Max(maxTop, yRadius - hbox.Height / 2f);
                        maxLeft = maxRight = Math.Max(maxLeft, xRadius - hbox.Width / 2f);
                        alwaySymmetric = true;
                        break;
                    case EncloseNotation.phasorangle:
                        float angleWidth = 640 * GetPixelScale();//x 637.5 constants
                        maxLeft = Math.Max(maxLeft, angleWidth);
                        maxRight = Math.Max(maxRight, over);
                        maxTop = Math.Max(maxTop, over);
                        maxBottom = Math.Max(maxBottom, under);
                        break;
                    case EncloseNotation.longdiv:
                        GlyphBox ldiv = NewGlyphBox();
                        ldiv.Character = ')';
                        AssignGlyphVxs(ldiv);
                        ldiv.Layout();

                        Box actualDiv = StretchHeightIfStretchable(ldiv, hbox.Height + over);
                        actualDiv.Layout();
                        maxLeft = Math.Max(maxLeft, actualDiv.Width);
                        maxRight = Math.Max(maxRight, over);
                        maxTop = Math.Max(maxTop, over);
                        maxBottom = Math.Max(maxBottom, under);
                        break;
                    case EncloseNotation.radical:
                        GlyphBox radical = NewGlyphBox();
                        radical.Character = (char)0x221A;
                        AssignGlyphVxs(radical);
                        radical.Layout();

                        Box actualRadical = StretchHeightIfStretchable(radical, hbox.Height + over);
                        actualRadical.Layout();
                        maxLeft = Math.Max(maxLeft, actualRadical.Width);
                        maxRight = Math.Max(maxRight, over);
                        maxTop = Math.Max(maxTop, over);
                        maxBottom = Math.Max(maxBottom, under);
                        break;
                }
            }
            if (alwaySymmetric)
            {
                maxLeft = Math.Max(maxLeft, maxRight);
                maxRight = maxLeft;
                maxTop = Math.Max(maxTop, maxBottom);
                maxBottom = maxTop;
            }

            float extend = GetPixelScale() * _mathConstants.SpaceAfterScript.Value;
            float w = maxLeft + hbox.Width + maxRight;
            float h = maxTop + hbox.Height + maxBottom;
            for (int i = 0; i < length; i++)
            {
                CreateCustomNotation(notations[i], thickness, w, h, hbox, maxLeft, maxTop, extend, over, encloseBox);
            }
            encloseBox.TopSpace = maxTop;
            encloseBox.BottomSpace = maxBottom;
            encloseBox.LeftSpace = maxLeft;
            encloseBox.RightSpace = maxRight;
            encloseBox.BaseLineShift = -maxBottom + hbox.BaseLineShift;
            return encloseBox;
        }

        protected abstract void CreateCustomNotation(EncloseNotation notation,
           float thickness, float w, float h,
           HorizontalStackBox hbox, float maxLeft, float maxTop, float extend, float over,
           EncloseBox encloseBox);

        Box CreateStackLine(MathNode node)
        {
            StackLine sline = new StackLine();
            sline.StartPoint = new LayoutFarm.MathLayout.Point();
            sline.StrokeWidth = _mathConstants.UnderbarRuleThickness.Value * GetPixelScale();

            return sline;
        }

        void FillContainerBox(ContainerBox containerBox, MathNode node, bool reverse = false)
        {
            int child_count = node.ChildCount;
            if (reverse)
            {
                for (int i = child_count - 1; i >= 0; --i)
                {
                    Box childBox = CreateMathBox(node.GetNode(i));
                    if (childBox != null)
                    {
                        containerBox.AddChild(childBox);
                    }
                }
            }
            else
            {
                for (int i = 0; i < child_count; ++i)
                {
                    Box childBox = CreateMathBox(node.GetNode(i));
                    if (childBox != null)
                    {
                        containerBox.AddChild(childBox);
                    }
                }
            }
        }
        Box CreateTableBox(MathNode node)
        {
            TableBox tableBox = new TableBox();
            int childCount = node.ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                tableBox.AddChild(CreateMathBox(node.GetNode(i)));
            }
            return tableBox;
        }

        Box CreateFencedBox(MathNode node)
        {
            string open = node.GetAttributeValue("open");
            if (open == null)
            {
                open = "(";
            }
            string close = node.GetAttributeValue("close");
            if (close == null)
            {
                close = ")";
            }
            string separators = node.GetAttributeValue("separators");
            if (separators == null)
            {
                separators = ",";
            }
            string[] sepEach = separators.Split(' ');

            HorizontalStackBox hbox = new HorizontalStackBox();
            int childCount = node.ChildCount;
            int sepMaxIndex = sepEach.Length - 1;
            hbox.AddChild(CreateGlyphBox(open));
            for (int i = 0; i < childCount; i++)
            {
                hbox.AddChild(CreateMathBox(node.GetNode(i)));
                if (i != childCount - 1)
                {
                    if (i > sepMaxIndex)
                    {
                        hbox.AddChild(CreateGlyphBox(sepEach[sepMaxIndex]));
                    }
                    else
                    {
                        hbox.AddChild(CreateGlyphBox(sepEach[i]));
                    }
                }
            }
            hbox.AddChild(CreateGlyphBox(close));
            return hbox;
        }

        bool _isPhantom = false;
        Box CreatePhantomBox(MathNode node)
        {
            HorizontalStackBox hbox = new HorizontalStackBox();
            int childCount = node.ChildCount;
            _isPhantom = true;
            for (int i = 0; i < childCount; i++)
            {
                hbox.AddChild(CreateMathBox(node.GetNode(i)));
            }
            _isPhantom = false;
            return hbox;
        }
        MathConstants _mathConstants;


        protected void AssignGlyphVxs(Box box)
        {
            if (box is GlyphBox glyphBox)
            {
                AssignGlyphVxs(glyphBox);
            }
            else if (box is ContainerBox container)
            {
                int length = container.ChildCount;
                for (int i = 0; i < length; i++)
                {
                    AssignGlyphVxs(container.GetChild(i));
                }
            }
        }

        protected float GetPixelScale() => _typeface.CalculateScaleToPixelFromPointSize(GetCurrentLevelFontSize());

        float GetPixelFromDesignSize(MathValueRecord value) => value.Value * GetPixelScale();

        void AssignGlyphVxs(GlyphBox glyphBox)
        {
          
            char ch = glyphBox.Character;
            if (ch == 0 || glyphBox.HasVxs || MathMLOperatorTable.IsInvicibleCharacter(ch))
            {
                return;
            }
            ushort glyphIndex = _typeface.GetGlyphIndex((int)ch);
            if (glyphIndex == 0)//glyphindex not found
            {
                switch ((int)ch)
                {
                    case 8254:
                        glyphIndex = 2246;//overline
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            AssignGlyphVxsByGlyphIndex(glyphBox, glyphIndex);
        }


        void AssignGlyphVxsByGlyphIndex(GlyphBox glyphBox, ushort glyphIndex)
        {
           
            float font_size_in_Point = GetCurrentLevelFontSize();
            float px_scale = GetPixelScale();
            if (glyphBox.HasVxs)
            {
                return;
            }

            if (_typeface.HasMathTable())
            {
                Glyph glyph = _typeface.GetGlyph(glyphIndex);
                if (glyph.MathGlyphInfo != null)
                {
                    glyphBox.MathGlyphInfo = glyph.MathGlyphInfo;
                    if (glyph.MathGlyphInfo.TopAccentAttachment != null)
                    {
                        glyphBox.TopAccentAttachmentScale = px_scale * glyph.MathGlyphInfo.TopAccentAttachment.Value.Value;
                    }
                    else
                    {
                        glyphBox.TopAccentAttachmentScale = -font_size_in_Point / 5;
                    }
                }
            }
            ushort advW = _typeface.GetAdvanceWidthFromGlyphIndex(glyphIndex);//unscale glyph width
            int advW_s = (int)System.Math.Round(px_scale * advW);

            glyphBox.GlyphIndex = glyphIndex;
            glyphBox.AdvanceWidthScale = advW_s;
            glyphBox.MathConstants = _mathConstants;
            glyphBox.PixelScale = px_scale;

            SetGlyphVxs(glyphBox, _typeface, font_size_in_Point);


        }

        protected abstract void SetGlyphVxs(GlyphBox glyphbox, Typeface typeface, float sizeInPoint);

        private Box StretchGlyphHeight(GlyphBox glyphBox, float targetHeight)
        {
            if (MathMLOperatorTable.IsStretchyPropertyOperator(glyphBox.Character + ""))
            {
                if (glyphBox.Height == targetHeight)
                {
                    return glyphBox;
                }
                if (glyphBox.MathGlyphInfo != null && glyphBox.MathGlyphInfo.VertGlyphConstruction != null)
                {
                    MathGlyphVariantRecord[] variantRecords = glyphBox.MathGlyphInfo.VertGlyphConstruction.glyphVariantRecords;
                    if (variantRecords != null)
                    {
                        int count = variantRecords.Length;

                        float px_scale = GetPixelScale();
                        bool scale = false;
                        float lastPx = 0;

                        bool stretch = true;
                        float fivePercent = targetHeight * 0.05f;
                        float diff = Math.Abs(glyphBox.Height - targetHeight);
                        if (diff <= fivePercent)
                        {
                            stretch = false;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            float px = variantRecords[i].AdvanceMeasurement * px_scale;
                            if (targetHeight > px)
                            {
                                lastPx = px;
                                continue;
                            }
                            else
                            {
                                if (i > 0)
                                {
                                    float dif1 = System.Math.Abs(targetHeight - lastPx);//prev
                                    float dif2 = System.Math.Abs(targetHeight - px);
                                    if (dif1 < dif2)//prev is closest
                                    {
                                        i -= 1;
                                    }
                                }

                                glyphBox.ClearVxs();
                                AssignGlyphVxsByGlyphIndex(glyphBox, variantRecords[i].VariantGlyph);
                                glyphBox.ScalteToFitHeight(targetHeight);
                                glyphBox.Stretched = stretch;
                                glyphBox.Depth = 0;
                                scale = true;
                                break;
                            }
                        }
                        if (scale)
                        {
                            return glyphBox;
                        }
                    }

                    GlyphPartRecord[] partRecords = glyphBox.MathGlyphInfo.VertGlyphConstruction.GlyphAsm_GlyphPartRecords;
                    if (partRecords == null)
                    {
                        return glyphBox;
                    }
                    int partCount = partRecords.Length;
                    VerticalStackBox vbox = new VerticalStackBox();
                    for (int i = partCount - 1; i >= 0; i--)
                    {
                        GlyphBox gbox = NewGlyphBox();
                        AssignGlyphVxsByGlyphIndex(gbox, partRecords[i].GlyphId);
                        vbox.AddChild(gbox);
                    }
                    vbox.Layout();
                    if (vbox.Height < targetHeight)
                    {
                        float remain = targetHeight - vbox.Height;
                        int stretchPart = (int)Math.Floor(partCount / 2.0f);
                        float eachPartStrectH = remain / stretchPart;
                        for (int i = 0; i < stretchPart; i++)
                        {
                            GlyphBox gbox = (GlyphBox)vbox.GetChild((i * 2) + 1);
                            gbox.ScalteToFitHeight(gbox.Height + eachPartStrectH);
                            gbox.Layout();
                            gbox.Depth = 0;
                        }
                    }
                    else
                    {
                        float eachPartStrectH = targetHeight / partCount;
                        for (int i = 0; i < partCount; i++)
                        {
                            GlyphBox gbox = (GlyphBox)vbox.GetChild(i);
                            gbox.ScalteToFitHeight(eachPartStrectH);
                            gbox.Layout();
                            gbox.Depth = 0;
                        }
                    }
                    StretchCharBox stretchChar = new StretchCharBox();
                    stretchChar.StretchContainer = vbox;
                    stretchChar.OriginalGlyph = glyphBox;
                    return stretchChar;
                }
                else
                {

                }
            }
            return glyphBox;
        }

        Box StretchGlyphWidth(GlyphBox glyphBox, float targetWidth)
        {
            if (MathMLOperatorTable.IsStretchyPropertyOperator(glyphBox.Character + ""))
            {
                if (glyphBox.MathGlyphInfo != null && glyphBox.MathGlyphInfo.HoriGlyphConstruction != null)
                {
                    MathGlyphVariantRecord[] variantRecords = glyphBox.MathGlyphInfo.HoriGlyphConstruction.glyphVariantRecords;
                    if (variantRecords != null)
                    {
                        int count = variantRecords.Length;

                        float px_scale = GetPixelScale();
                        bool scale = false;
                        float lastPx = 0;
                        for (int i = 0; i < count; i++)
                        {
                            float px = variantRecords[i].AdvanceMeasurement * px_scale;
                            if (targetWidth > px)
                            {
                                lastPx = px;
                                continue;
                            }
                            else
                            {
                                if (i > 0)
                                {
                                    float dif1 = System.Math.Abs(targetWidth - lastPx);//prev
                                    float dif2 = System.Math.Abs(targetWidth - px);
                                    if (dif1 < dif2)//prev is closest
                                    {
                                        i -= 1;
                                    }
                                }

                                glyphBox.ClearVxs();
                                AssignGlyphVxsByGlyphIndex(glyphBox, variantRecords[i].VariantGlyph);
                                glyphBox.ScaleToFitWidth(targetWidth);
                                scale = true;
                                break;
                            }
                        }
                        if (scale)
                        {
                            return glyphBox;
                        }
                    }

                    GlyphPartRecord[] partRecords = glyphBox.MathGlyphInfo.HoriGlyphConstruction.GlyphAsm_GlyphPartRecords;
                    if (partRecords == null)
                    {
                        return glyphBox;
                    }

                    int partCount = partRecords.Length;
                    HorizontalStackBox hbox = new HorizontalStackBox();
                    for (int i = 0; i < partCount; i++)
                    {
                        GlyphBox gbox = NewGlyphBox();
                        AssignGlyphVxsByGlyphIndex(gbox, partRecords[i].GlyphId);
                        hbox.AddChild(gbox);
                    }
                    hbox.Layout();
                    if (hbox.Width < targetWidth)
                    {
                        float remain = targetWidth - hbox.Width;
                        int stretchPart = (int)Math.Floor(partCount / 2.0f);
                        float eachPartStrectH = remain / stretchPart;
                        for (int i = 0; i < stretchPart; i++)
                        {
                            GlyphBox gbox = (GlyphBox)hbox.GetChild((i * 2) + 1);
                            float newWidth = eachPartStrectH + gbox.Width;
                            gbox.ScaleToFitWidth(newWidth);
                            gbox.AdvanceWidthScale = (int)newWidth;
                        }
                    }
                    else
                    {
                        float eachPartStrectH = targetWidth / partCount;
                        for (int i = 0; i < partCount; i++)
                        {
                            GlyphBox gbox = (GlyphBox)hbox.GetChild(i);
                            gbox.ScaleToFitWidth(eachPartStrectH);
                            gbox.AdvanceWidthScale = (int)eachPartStrectH;
                        }
                    }
                    StretchCharBox stretchChar = new StretchCharBox();
                    stretchChar.StretchContainer = hbox;
                    stretchChar.OriginalGlyph = glyphBox;
                    return stretchChar;
                }
                else
                {

                }
            }
            return glyphBox;
        }

        GlyphBox CreateGlyphBox(char ch)
        {
            GlyphBox glyphBox = NewGlyphBox();
            glyphBox.Character = ch;
            glyphBox.IsInvisible = _isPhantom;
            return glyphBox;
        }

        HorizontalStackBox CreateGlyphBox(string str)
        {
            if (str != null && str.Length > 0)
            {
                int length = str.Length;
                HorizontalStackBox hbox = new HorizontalStackBox();
                for (int i = 0; i < length; i++)
                {
                    GlyphBox glyphBox = NewGlyphBox();
                    glyphBox.Character = str[i];
                    glyphBox.IsInvisible = _isPhantom;
                    hbox.AddChild(glyphBox);
                }
                return hbox;
            }
            return null;
        }
        Box CreateTextBox(MathNode node)
        {
            if (node.Text == null)
            {
                return null;
            }
            char[] text_buff = node.Text.ToCharArray();
            if (text_buff.Length == 0)
            {
                //????
                return null;
            }
            else if (text_buff.Length > 1)
            {
                HorizontalStackBox textSpan = new HorizontalStackBox();
                textSpan.MathNode = node;
                for (int i = 0; i < text_buff.Length; ++i)
                {
                    char ch = text_buff[i];
                    if (!MathMLOperatorTable.IsInvicibleCharacter(ch))
                    {
                        GlyphBox glyphBox = NewGlyphBox();
                        glyphBox.Character = ch;
                        glyphBox.IsInvisible = _isPhantom;
                        textSpan.AddChild(glyphBox);
                    }
                }
                return textSpan;
            }
            else
            {
                //len=1
                if (!MathMLOperatorTable.IsInvicibleCharacter(text_buff[0]))
                {
                    GlyphBox glyphBox = NewGlyphBox();
                    glyphBox.MathNode = node;
                    glyphBox.Character = text_buff[0];
                    glyphBox.IsInvisible = _isPhantom;
                    return glyphBox;
                }
                return null;
            }
        }

        HorizontalStackBox CreateHorizontalBox(MathNode node, bool reverse = false)
        {
            HorizontalStackBox hbox = new HorizontalStackBox();
            hbox.MathNode = node;
            //
            FillContainerBox(hbox, node, reverse);
            return hbox;
        }
        Box StretchHeightIfFenceAndStretchable(Box box, float height)
        {
            if (box is GlyphBox gbox)
            {
                if (MathMLOperatorTable.IsFencePropertyOperator(gbox.Character + "") &&
                    MathMLOperatorTable.IsStretchyPropertyOperator(gbox.Character + ""))
                {
                    return StretchHeightIfStretchable(gbox, height);
                }
                else
                {
                    return box;
                }
            }
            else if (box is ContainerBox containerBox)
            {
                int length = containerBox.ChildCount;
                for (int i = 0; i < length; i++)
                {
                    containerBox.Replace(i, StretchHeightIfFenceAndStretchable(containerBox.GetChild(i), containerBox.Height));
                }
                return containerBox;
            }
            else if (box is UnderscriptOverscriptBox underOverScript)
            {
                underOverScript.BaseBox = StretchHeightIfFenceAndStretchable(underOverScript.BaseBox, height);
                if (underOverScript.UnderscriptBox != null)
                {
                    underOverScript.UnderscriptBox = StretchHeightIfFenceAndStretchable(underOverScript.UnderscriptBox, underOverScript.UnderscriptBox.Height);
                }
                if (underOverScript.OverscriptBox != null)
                {
                    underOverScript.OverscriptBox = StretchHeightIfFenceAndStretchable(underOverScript.OverscriptBox, underOverScript.OverscriptBox.Height);
                }
            }
            else if (box is SubscriptSuperscriptBox subSuperScript)
            {
                subSuperScript.BaseBox = StretchHeightIfFenceAndStretchable(subSuperScript.BaseBox, height);
                if (subSuperScript.SubscriptBox != null)
                {
                    subSuperScript.SubscriptBox = StretchHeightIfFenceAndStretchable(subSuperScript.SubscriptBox, subSuperScript.SubscriptBox.Height);
                }
                if (subSuperScript.SuperscriptBox != null)
                {
                    subSuperScript.SuperscriptBox = StretchHeightIfFenceAndStretchable(subSuperScript.SuperscriptBox, subSuperScript.SuperscriptBox.Height);
                }
            }
            return box;
        }

        protected Box StretchHeightIfStretchable(Box box, float height)
        {
            if (box is GlyphBox gbox)
            {
                return StretchHeightIfStretchable(gbox, height);
            }
            else if (box is ContainerBox containerBox)
            {
                int length = containerBox.ChildCount;
                for (int i = 0; i < length; i++)
                {
                    containerBox.Replace(i, StretchHeightIfStretchable(containerBox.GetChild(i), containerBox.Height));
                }
                return containerBox;
            }
            return box;
        }

        Box StretchHeightIfStretchable(GlyphBox gbox, float height)
        {
            if (MathMLOperatorTable.IsStretchyPropertyOperator(gbox.Character + ""))
            {
                return StretchGlyphHeight(gbox, height);
            }
            return gbox;
        }

        Box StretchWidthIfStretchable(Box box, float width)
        {
            if (box is GlyphBox gbox)
            {
                return StretchWidthIfStretchable(gbox, width);
            }
            else if (box is ContainerBox containerBox)
            {
                int length = containerBox.ChildCount;
                for (int i = 0; i < length; i++)
                {
                    containerBox.Replace(i, StretchWidthIfStretchable(containerBox.GetChild(i), containerBox.Width));
                }
                return containerBox;
            }
            return box;
        }

        Box StretchWidthIfStretchable(GlyphBox gbox, float width)
        {
            return StretchGlyphWidth(gbox, width);
        }

        VerticalStackBox CreateVerticalBox(MathNode node, bool reverse = false)
        {
            VerticalStackBox vbox = new VerticalStackBox();
            vbox.MathNode = node;

            FillContainerBox(vbox, node, reverse);
            return vbox;
        }

        Box CreateFractionsBox(MathNode node)
        {
            if (node.ChildCount != 2)//Required argument count
            {
                return null;
            }
            FractionBox fractionBox = new FractionBox();
            fractionBox.MathNode = node;

            Box numerator = CreateMathBox(node.GetNode(0));
            numerator.Layout();
            Box denominator = CreateMathBox(node.GetNode(1));
            denominator.Layout();

            LineBox fractionLine = new LineBox();
            string linethickness = node.GetAttributeValue("linethickness");
            float thickness = _mathConstants.FractionRuleThickness.Value * GetPixelScale();//default
            if (linethickness != null)
            {
                if (linethickness.EndsWith("%"))
                {
                    if (float.TryParse(linethickness.Substring(0, linethickness.Length - 1), out float v))
                    {
                        thickness *= (v / 100f);
                    }
                }
                else if (float.TryParse(linethickness, out float v))
                {
                    thickness = v;
                }
                else
                {
                    switch (linethickness)
                    {
                        //"thin" | "medium" | "thick" 
                        case "thin":
                            thickness /= 2;
                            break;
                        case "medium":
                            break;
                        case "thick":
                            thickness *= 2;
                            break;
                    }
                }
            }

            float a = _mathConstants.AccentBaseHeight.Value * GetPixelScale();
            bool bevelled = AttributeParser.ParseBoolean(node.GetAttributeValue("bevelled"), false);
            if (bevelled)
            {

                LineBox bevelLine = new LineBox();
                bevelLine.StrokeWidth = thickness;
                float horGap = _mathConstants.SkewedFractionHorizontalGap.Value * GetPixelScale();
                float verGap = _mathConstants.SkewedFractionVerticalGap.Value * GetPixelScale();
                float maxHeight = Math.Max(numerator.Height, denominator.Height) + verGap;
                bevelLine.StartPoint = new LayoutFarm.MathLayout.Point(0, (int)(maxHeight));
                bevelLine.EndPoint = new LayoutFarm.MathLayout.Point((int)(horGap), 0);

                if (numerator.Height < maxHeight)
                {
                    float diff = maxHeight - numerator.Height;
                    VerticalStackBox temp = new VerticalStackBox();
                    temp.AddChild(numerator);
                    float accent = _mathConstants.AccentBaseHeight.Value * GetPixelScale();
                    temp.AddChild(new SpaceBox(numerator.Width, accent + verGap));
                    numerator = temp;
                }
                fractionLine = bevelLine;
            }
            else
            {
                float maxWidth = Math.Max(numerator.Width, denominator.Width);
                if (numerator.Width < denominator.Width)
                {
                    Box temp = CreateStoreBoxNewWidth(numerator, maxWidth);

                    float shift = (maxWidth - numerator.Width) / 2;
                    numerator.MarginLeft = shift;
                    Alignment numalign = AttributeParser.ParseAlignment(node.GetAttributeValue("numalign"), Alignment.Center);
                    switch (numalign)
                    {
                        default:
                        case Alignment.Center:
                            break;
                        case Alignment.Left:
                            numerator.MarginLeft = 0;
                            break;
                        case Alignment.Right:
                            numerator.MarginLeft = maxWidth - numerator.Width;
                            break;
                    }
                    numerator = temp;
                }
                else
                {
                    Box temp = CreateStoreBoxNewWidth(denominator, maxWidth);

                    float shift = (maxWidth - denominator.Width) / 2;
                    denominator.MarginLeft = shift;
                    Alignment denomalign = AttributeParser.ParseAlignment(node.GetAttributeValue("denomalign"), Alignment.Center);
                    switch (denomalign)
                    {
                        default:
                        case Alignment.Center:
                            break;
                        case Alignment.Left:
                            denominator.MarginLeft = 0;
                            break;
                        case Alignment.Right:
                            denominator.MarginLeft = maxWidth - denominator.Width;
                            break;
                    }
                    denominator = temp;
                }
                LineBox h_sepBar = new LineBox();
                h_sepBar.SetAsBar(maxWidth, false);
                h_sepBar.StrokeWidth = thickness;
                h_sepBar.MarginTop = _mathConstants.FractionNumDisplayStyleGapMin.Value * GetPixelScale();
                h_sepBar.MarginBottom = _mathConstants.FractionDenomDisplayStyleGapMin.Value * GetPixelScale();
                fractionLine = h_sepBar;
            }
            fractionBox.Numerator = numerator;
            fractionBox.FractionLine = fractionLine;
            fractionBox.Denominator = denominator;
            fractionBox.Bevelled = bevelled;
            fractionBox.LineThickness = thickness;
            return fractionBox;
        }
        Box CreateStoreBoxNewWidth(Box box, float newWidth)
        {
            HorizontalStackBox storeBox = new HorizontalStackBox();
            storeBox.SetSize(newWidth, box.Height);
            storeBox.AddChild(box);
            return storeBox;
        }

        Box CreateRadicalBox(MathNode node)
        {
            if (node.ChildCount < 1)
            {
                return null;
            }
            RadicalBox hbox = new RadicalBox();
            hbox.MathNode = node;
            GlyphBox radicalChar = NewGlyphBox();
            radicalChar.MathNode = node;
            radicalChar.Character = (char)0x221A;//radical character
            AssignGlyphVxs(radicalChar);
            radicalChar.Layout();

            VerticalStackBox baseVertical = new VerticalStackBox();
            Box baseBox = CreateMathBox(node.GetNode(0));
            baseBox.Layout();
            LineBox topBar = new LineBox();
            topBar.SetAsBar(baseBox.Width, false);
            topBar.StrokeWidth = _mathConstants.RadicalRuleThickness.Value * GetPixelScale();
            baseVertical.AddChild(topBar);
            topBar.MarginBottom = _mathConstants.RadicalVerticalGap.Value * GetPixelScale();

            baseVertical.AddChild(baseBox);
            if (node.ChildCount == 1)
            {
                //skip
            }
            else if (node.ChildCount == 2)
            {
                ScriptLevel++;
                Box degreeBox = CreateMathBox(node.GetNode(1));
                hbox.Degree = degreeBox;
                ScriptLevel--;
            }
            else
            {
                //>2
                return null;
            }
            radicalChar.Layout();
            baseVertical.Layout();
            baseVertical.BaseLineShift = baseBox.BaseLineShift;
            if (radicalChar.Height > baseVertical.Height)
            {
                topBar.MarginBottom = topBar.MarginBottom + radicalChar.Height - baseVertical.Height;
            }
            else
            {
                Box radicalStretched = StretchHeightIfStretchable(radicalChar, baseVertical.Height);
                if (radicalStretched.Kind == BoxKind.GlyphBox)
                {
                    radicalChar = (GlyphBox)radicalStretched;
                }
                else
                {
                    hbox.Radical = radicalStretched;
                    hbox.BaseBox = baseVertical;
                    return hbox;
                }
            }
            hbox.Radical = radicalChar;
            hbox.BaseBox = baseVertical;
            return hbox;
        }

        Box CreateUnderscriptBox(MathNode node)
        {
            if (node.ChildCount != 2)
            {
                return null;
            }
            bool accentUnder = AttributeParser.ParseBoolean(node.GetAttributeValue("accentunder"), false);
            Alignment alignment = AttributeParser.ParseAlignment(node.GetAttributeValue("align"), Alignment.Center);

            UnderscriptOverscriptBox underscriptBox = new UnderscriptOverscriptBox();
            underscriptBox.MathNode = node;

            Box baseBox = CreateMathBox(node.GetNode(0));
            baseBox.Layout();

            Box underBox;
            if (accentUnder)
            {
                underBox = CreateMathBox(node.GetNode(1));
            }
            else
            {
                ScriptLevel++;
                underBox = CreateMathBox(node.GetNode(1));
                ScriptLevel--;
            }
            underBox.Align = alignment;
            underBox.Layout();

            float newWidth = Math.Max(baseBox.Width, underBox.Width);
            baseBox = StretchWidthIfStretchable(baseBox, newWidth);
            if (accentUnder)
            {
                underBox = StretchWidthIfStretchable(underBox, newWidth);
            }
            else
            {
                ScriptLevel++;
                underBox = StretchWidthIfStretchable(underBox, newWidth);
                ScriptLevel--;
            }

            underscriptBox.BaseBox = baseBox;
            underscriptBox.UnderscriptBox = underBox;

            return underscriptBox;
        }

        Box CreateOverscriptBox(MathNode node)
        {
            if (node.ChildCount != 2)
            {
                return null;
            }
            bool accent = AttributeParser.ParseBoolean(node.GetAttributeValue("accent"), false);
            Alignment alignment = AttributeParser.ParseAlignment(node.GetAttributeValue("align"), Alignment.Center);

            UnderscriptOverscriptBox overscriptBox = new UnderscriptOverscriptBox();
            overscriptBox.MathNode = node;

            Box baseBox = CreateMathBox(node.GetNode(0));
            baseBox.Layout();

            Box overBox;
            if (accent)
            {
                overBox = CreateMathBox(node.GetNode(1));
            }
            else
            {
                ScriptLevel++;
                overBox = CreateMathBox(node.GetNode(1));
                ScriptLevel--;
            }
            overBox.Align = alignment;
            overBox.Layout();

            float newWidth = Math.Max(baseBox.Width, overBox.Width);
            baseBox = StretchWidthIfStretchable(baseBox, newWidth);
            if (accent)
            {
                overBox = StretchWidthIfStretchable(overBox, newWidth);
            }
            else
            {
                ScriptLevel++;
                overBox = StretchWidthIfStretchable(overBox, newWidth);
                ScriptLevel--;
            }

            overscriptBox.BaseBox = baseBox;
            overscriptBox.OverscriptBox = overBox;

            return overscriptBox;
        }

        Box CreateUnderscriptOverscriptBox(MathNode node)
        {
            if (node.ChildCount != 3)
            {
                return null;
            }
            bool accent = AttributeParser.ParseBoolean(node.GetAttributeValue("accent"), false);
            bool accentUnder = AttributeParser.ParseBoolean(node.GetAttributeValue("accentunder"), false);
            Alignment alignment = AttributeParser.ParseAlignment(node.GetAttributeValue("align"), Alignment.Center);

            UnderscriptOverscriptBox underscriptOverscriptBox = new UnderscriptOverscriptBox();
            underscriptOverscriptBox.MathNode = node;

            Box baseBox = CreateMathBox(node.GetNode(0));
            baseBox.Layout();

            Box underBox;
            if (accentUnder)
            {
                underBox = CreateMathBox(node.GetNode(1));
            }
            else
            {
                ScriptLevel++;
                underBox = CreateMathBox(node.GetNode(1));
                ScriptLevel--;
            }
            underBox.Align = alignment;
            underBox.Layout();

            Box overBox;
            if (accent)
            {
                overBox = CreateMathBox(node.GetNode(2));
            }
            else
            {
                ScriptLevel++;
                overBox = CreateMathBox(node.GetNode(2));
                ScriptLevel--;
            }
            overBox.Align = alignment;
            overBox.Layout();

            float newWidth = Math.Max(baseBox.Width, underBox.Width);
            newWidth = Math.Max(newWidth, overBox.Width);
            baseBox = StretchWidthIfStretchable(baseBox, newWidth);
            if (accentUnder)
            {
                underBox = StretchWidthIfStretchable(underBox, newWidth);
            }
            else
            {
                ScriptLevel++;
                underBox = StretchWidthIfStretchable(underBox, newWidth);
                ScriptLevel--;
            }
            if (accent)
            {
                overBox = StretchWidthIfStretchable(overBox, newWidth);
            }
            else
            {
                ScriptLevel++;
                overBox = StretchWidthIfStretchable(overBox, newWidth);
                ScriptLevel--;
            }

            underscriptOverscriptBox.BaseBox = baseBox;
            underscriptOverscriptBox.UnderscriptBox = underBox;
            underscriptOverscriptBox.OverscriptBox = overBox;

            return underscriptOverscriptBox;
        }

        Box CreateSubscriptBox(MathNode node)
        {
            if (node.ChildCount != 2)
            {
                return null;
            }
            SubscriptSuperscriptBox subscript = new SubscriptSuperscriptBox();
            subscript.MathNode = node;
            Box baseBox = CreateMathBox(node.GetNode(0));

            ScriptLevel++;
            Box subscriptBox = CreateMathBox(node.GetNode(1));

            ScriptLevel--;
            if (_mathConstants != null)
            {
                float shiftdown = GetPixelFromDesignSize(_mathConstants.SubscriptShiftDown);
                subscript.SubscriptShiftDown = shiftdown;
            }
            subscript.BaseBox = baseBox;
            subscript.SubscriptBox = subscriptBox;

            return subscript;
        }

        Box CreateSuperscriptBox(MathNode node)
        {
            if (node.ChildCount != 2)
            {
                return null;
            }
            SubscriptSuperscriptBox superscript = new SubscriptSuperscriptBox();
            superscript.MathNode = node;

            Box baseBox = CreateMathBox(node.GetNode(0));

            ScriptLevel++;
            Box superscriptBox = CreateMathBox(node.GetNode(1));
            ScriptLevel--;
            if (_mathConstants != null)
            {
                float shiftup = GetPixelFromDesignSize(_mathConstants.SuperscriptShiftUp);
                superscript.SuperscriptShiftUp = shiftup;
            }
            superscript.BaseBox = baseBox;
            superscript.SuperscriptBox = superscriptBox;

            return superscript;
        }

        Box CreateSubscriptSuperscriptBox(MathNode node)
        {
            if (node.ChildCount != 3)
            {
                return null;
            }
            SubscriptSuperscriptBox subscriptSuperscriptBox = new SubscriptSuperscriptBox();
            subscriptSuperscriptBox.MathNode = node;

            Box baseBox = CreateMathBox(node.GetNode(0));

            ScriptLevel++;
            VerticalStackBox vbox = new VerticalStackBox();
            Box subBox = CreateMathBox(node.GetNode(1));
            Box superBox = CreateMathBox(node.GetNode(2));
            isScriptScript = false;
            ScriptLevel--;
            if (_mathConstants != null)
            {
                float shiftdown = GetPixelFromDesignSize(_mathConstants.SubscriptShiftDown);
                subscriptSuperscriptBox.SubscriptShiftDown = shiftdown;

                float shiftup = GetPixelFromDesignSize(_mathConstants.SuperscriptShiftUp);
                subscriptSuperscriptBox.SuperscriptShiftUp = shiftup;
            }
            subscriptSuperscriptBox.BaseBox = baseBox;
            subscriptSuperscriptBox.SuperscriptBox = superBox;
            subscriptSuperscriptBox.SubscriptBox = subBox;

            return subscriptSuperscriptBox;
        }

        Box CreateMultiScriptBox(MathNode node)
        {
            int count = node.ChildCount;
            if (count < 3)
            {
                return null;
            }
            MultiScriptBox multiScriptBox = new MultiScriptBox();

            Box baseBox = CreateMathBox(node.GetNode(0));

            bool isPrescript = false;
            for (int i = 1; i < count; i++)
            {
                MathNode mnode = node.GetNode(i);
                if (mnode.Name == "mprescripts")
                {
                    isPrescript = true;
                    continue;
                }
                else
                {
                    if (i + 1 <= count - 1)
                    {
                        ScriptLevel++;
                        Box sub = CreateMathBox(mnode);
                        Box sup = CreateMathBox(node.GetNode(++i));
                        if (isPrescript)
                        {
                            multiScriptBox.AddMutiPrescript(sub, sup);
                        }
                        else
                        {
                            multiScriptBox.AddMultiScript(sub, sup);
                        }
                        ScriptLevel--;
                    }
                    else
                    {
                        throw new NotSupportedException("invalid-markup");
                    }
                }
            }

            if (_mathConstants != null)
            {
                float shiftdown = GetPixelFromDesignSize(_mathConstants.SubscriptShiftDown);
                multiScriptBox.SubscriptShiftDown = shiftdown;

                float shiftup = GetPixelFromDesignSize(_mathConstants.SuperscriptShiftUp);
                multiScriptBox.SuperscriptShiftUp = shiftup;
            }

            multiScriptBox.BaseBox = baseBox;
            return multiScriptBox;
        }

        Box CreateStackBox(MathNode node)
        {
            StackBox stackBox = new StackBox();
            stackBox.MathNode = node;
            if (node.ChildCount > 0)
            {
                FillContainerBox(stackBox, node);
                stackBox.Layout();

                int count = stackBox.ChildCount;
                float maxWidth = stackBox.Width;
                for (int i = 0; i < count; i++)
                {
                    Box box = stackBox.GetChild(i);
                    if (box is LineBox lnb)
                    {
                        Box target = null;
                        if (i > 0)
                        {
                            target = stackBox.GetChild(i - 1);
                        }
                        else if (i + 1 < count)
                        {
                            target = stackBox.GetChild(i + 1);
                        }
                        else
                        {
                            continue;
                        }
                        lnb.StartPoint = new LayoutFarm.MathLayout.Point(0, 0);
                        lnb.EndPoint = new LayoutFarm.MathLayout.Point((int)target.Width, 0);
                        lnb.StrokeWidth = _mathConstants.UnderbarRuleThickness.Value * GetPixelScale();
                        lnb.MarginTop = _mathConstants.FractionNumeratorShiftUp.Value * GetPixelScale();
                        lnb.MarginBottom = _mathConstants.FractionDenominatorShiftDown.Value * GetPixelScale();
                        lnb.Layout();
                    }
                }
            }
            return stackBox;
        }

        Box CreateStackRow(MathNode node)
        {
            StackRow stackRow = new StackRow();
            stackRow.MathNode = node;

            FillContainerBox(stackRow, node);

            return stackRow;
        }

        Box CreateStackGroup(MathNode node)
        {
            StackGroup stackGroup = new StackGroup();
            stackGroup.MathNode = node;
            stackGroup.Shift = AttributeParser.ParseInteger(node.GetAttributeValue("shift"), 0);
            FillContainerBox(stackGroup, node);

            return stackGroup;
        }

        Box CreateCarriesBox(MathNode node)
        {
            ScriptLevel++;
            ScriptLevel++;
            StackCarries carriesBox = new StackCarries();
            carriesBox.MathNode = node;

            FillContainerBox(carriesBox, node);

            ScriptLevel--;
            ScriptLevel--;
            return carriesBox;
        }

        Box CreateCarryBox(MathNode node)
        {
            StackCarryBox carryBox = new StackCarryBox();
            carryBox.MathNode = node;

            Box box = CreateMathBox(node.GetNode(0));
            carryBox.Carry = box;

            return carryBox;
        }
    }
}