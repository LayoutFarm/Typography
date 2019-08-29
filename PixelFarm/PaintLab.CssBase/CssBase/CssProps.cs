//BSD, 2014-present, WinterDev
//ArthurHub, Jose Manuel Menendez Poo


namespace LayoutFarm.Css
{
    //    <display-outside>
    //  = block | inline | run-in ;
    //<display-inside>
    //   = flow | flow-root | table | flex | grid | ruby ;
    //<display-listitem>
    // = list-item && <display-outside>? && [ flow | flow-root ]?
    //<display-internal>
    // = table-row-group | table-header-group |
    //                     table-footer-group | table-row | table-cell |
    //                     table-column-group | table-column | table-caption |
    //                     ruby-base | ruby-text | ruby-base-container |
    //                     ruby-text-container ;
    //<display-box>
    //      = contents | none ;
    //<display-legacy>
    //   = inline-block | inline-list-item |
    //                     inline-table | inline-flex | inline-grid ;
    //    //--------------------------------------------------

    public enum CssDisplayOutside : byte
    {
        Internal,
        Block,
        Inline,
        RunIn,
        TableCell,
        TableCaption,
    }

    public enum CssDisplayInside : byte
    {
        Internal,
        Flow,
        FlowRoot,
        Table,
        Flex,
        Grid,
        Ruby,
    }



    public enum CssDisplay : byte
    {
        [Map(CssConstants.Inline)]
        Inline,//default 
        [Map(CssConstants.InlineBlock)]
        InlineBlock,
        [Map(CssConstants.InlineFlex)]
        InlineFlex,
        //----------------------------
        [Map(CssConstants.TableRow)]
        TableRow,
        [Map(CssConstants.InlineTable)]
        InlineTable,
        [Map(CssConstants.TableColumn)]
        TableColumn,
        [Map(CssConstants.TableColumnGroup)]
        TableColumnGroup,
        [Map(CssConstants.TableRowGroup)]
        TableRowGroup,
        [Map(CssConstants.TableCaption)]
        TableCaption,
        [Map(CssConstants.TableHeaderGroup)]
        TableHeaderGroup,
        [Map(CssConstants.TableFooterGroup)]
        TableFooterGroup,
        [Map(CssConstants.None)]
        None,
        [Map(CssConstants.Block)]
        Block,
        [Map(CssConstants.Table)]
        Table,
        [Map(CssConstants.TableCell)]
        TableCell,
        [Map(CssConstants.ListItem)]
        ListItem,
        [Map(CssConstants.Flex)]
        Flex
    }
    public enum CssWhiteSpace : byte
    {
        [Map(CssConstants.Normal)]
        Normal,//default
        [Map(CssConstants.Pre)]
        Pre,
        [Map(CssConstants.PreLine)]
        PreLine,
        [Map(CssConstants.PreWrap)]
        PreWrap,
        [Map(CssConstants.NoWrap)]
        NoWrap,
    }
    public enum CssBorderStyle : byte
    {
        /// <summary>
        /// default
        /// </summary>
        [Map(CssConstants.None)]
        None,
        [Map(CssConstants.Hidden)]
        Hidden,
        [Map(CssConstants.Visible)]
        Visible,//boundary-- extension ***
        [Map(CssConstants.Dotted)]
        Dotted,
        [Map(CssConstants.Dashed)]
        Dashed,
        [Map(CssConstants.Solid)]
        Solid,
        [Map(CssConstants.Double)]
        Double,
        [Map(CssConstants.Groove)]
        Groove,
        [Map(CssConstants.Ridge)]
        Ridge,
        [Map(CssConstants.Inset)]
        Inset,
        [Map(CssConstants.Outset)]
        Outset,
        [Map(CssConstants.Inherit)]
        Inherit,
        //extension
        Unknown
    }
    public enum CssWordBreak : byte
    {
        [Map(CssConstants.Normal)]
        Normal,//default
        [Map(CssConstants.BreakAll)]
        BreakAll,
        [Map(CssConstants.KeepAll)]
        KeepAll,
        [Map(CssConstants.Inherit)]
        Inherit
    }

    public enum CssDirection : byte
    {
        [Map(CssConstants.Ltr)]
        Ltl,//default
        [Map(CssConstants.Rtl)]
        Rtl
    }
    public enum CssVerticalAlign : byte
    {
        [Map(CssConstants.Baseline)]
        Baseline,
        [Map(CssConstants.Sub)]
        Sub,
        [Map(CssConstants.Super)]
        Super,
        [Map(CssConstants.TextTop)]
        TextTop,
        [Map(CssConstants.TextBottom)]
        TextBottom,
        [Map(CssConstants.Top)]
        Top,
        [Map(CssConstants.Bottom)]
        Bottom,
        [Map(CssConstants.Middle)]
        Middle
    }
    public enum CssVisibility : byte
    {
        [Map(CssConstants.Visible)]
        Visible,//default
        [Map(CssConstants.Hidden)]
        Hidden,
        [Map(CssConstants.Collapse)]
        Collapse,
        [Map(CssConstants.Inherit)]
        Inherit
    }


    public enum CssTextAlign : byte
    {
        NotAssign,
        [Map(CssConstants.Left)]
        Left,//default
        [Map(CssConstants.Right)]
        Right,
        [Map(CssConstants.Center)]
        Center,
        [Map(CssConstants.Justify)]
        Justify,
        [Map(CssConstants.Inherit)]
        Inherit
    }

    public enum CssPosition : byte
    {
        [Map(CssConstants.Static)]
        Static,
        [Map(CssConstants.Relative)]
        Relative,
        [Map(CssConstants.Absolute)]
        Absolute,
        [Map(CssConstants.Fixed)]
        Fixed,
        [Map(CssConstants.Center)]
        Center,
        [Map(CssConstants.Page)]
        Page
    }

    public enum CssTextDecoration : byte
    {
        NotAssign,
        [Map(CssConstants.None)]
        None,
        [Map(CssConstants.Underline)]
        Underline,
        [Map(CssConstants.LineThrough)]
        LineThrough,
        [Map(CssConstants.Overline)]
        Overline
    }
    public enum CssOverflow : byte
    {
        [Map(CssConstants.Visible)]
        Visible,
        [Map(CssConstants.Hidden)]
        Hidden,
        [Map(CssConstants.Scroll)]
        Scroll,
        [Map(CssConstants.Auto)]
        Auto,
        [Map(CssConstants.Inherit)]
        Inherit
    }
    public enum CssBorderCollapse : byte
    {
        [Map(CssConstants.Separate)]
        Separate,
        [Map(CssConstants.Collapse)]
        Collapse,
        [Map(CssConstants.Inherit)]
        Inherit
    }
    public enum CssEmptyCell : byte
    {
        [Map(CssConstants.Show)]
        Show,
        [Map(CssConstants.Hide)]
        Hide,
        [Map(CssConstants.Inherit)]
        Inherit
    }

    public enum CssFloat : byte
    {
        [Map(CssConstants.None)]
        None,
        [Map(CssConstants.Left)]
        Left,
        [Map(CssConstants.Right)]
        Right,
        [Map(CssConstants.Inherit)]
        Inherit
    }

    public enum CssFontStyle : byte
    {
        [Map(CssConstants.Normal)]
        Normal,
        [Map(CssConstants.Italic)]
        Italic,
        [Map(CssConstants.Oblique)]
        Oblique,
        [Map(CssConstants.Inherit)]
        Inherit,
        Unknown,
    }
    public enum CssFontVariant : byte
    {
        [Map(CssConstants.Normal)]
        Normal,
        [Map(CssConstants.SmallCaps)]
        SmallCaps,
        [Map(CssConstants.Inherit)]
        Inherit,
        Unknown,
    }
    public enum CssFontWeight : byte
    {
        NotAssign,
        [Map(CssConstants.Normal)]
        Normal,
        [Map(CssConstants.Bold)]
        Bold,
        [Map(CssConstants.Bolder)]
        Bolder,
        [Map(CssConstants.Lighter)]
        Lighter,
        [Map("100")]
        _100,
        [Map("200")]
        _200,
        [Map("300")]
        _300,
        [Map("400")]
        _400,
        [Map("500")]
        _500,
        [Map("600")]
        _600,
        [Map(CssConstants.Inherit)]
        Inherit,
        Unknown,
    }
    public enum CssListStylePosition : byte
    {
        [Map(CssConstants.Outset)]
        Outside,
        [Map(CssConstants.Inside)]
        Inside,
        [Map(CssConstants.Inherit)]
        Inherit
    }
    public enum CssListStyleType : byte
    {
        [Map(CssConstants.None)]
        None,
        [Map(CssConstants.Disc)]
        Disc,
        [Map(CssConstants.Circle)]
        Circle,
        [Map(CssConstants.Separate)]
        Square,
        //-----------------------------

        [Map(CssConstants.Inherit)]
        Inherit,
        //-----------------------------
        [Map(CssConstants.Decimal)]
        Decimal,
        [Map(CssConstants.DecimalLeadingZero)]
        DecimalLeadingZero,
        [Map(CssConstants.LowerAlpha)]
        LowerAlpha,
        [Map(CssConstants.UpperAlpha)]
        UpperAlpha,
        [Map(CssConstants.LowerLatin)]
        LowerLatin,
        [Map(CssConstants.UpperLatin)]
        UpperLatin,
        [Map(CssConstants.LowerGreek)]
        LowerGreek,
        [Map(CssConstants.LowerRoman)]
        LowerRoman,
        [Map(CssConstants.UpperRoman)]
        UpperRoman,
        [Map(CssConstants.Armenian)]
        Armenian,
        [Map(CssConstants.Georgian)]
        Georgian,
        [Map(CssConstants.Hebrew)]
        Hebrew,
        [Map(CssConstants.Hiragana)]
        Hiragana,
        [Map(CssConstants.HiraganaIroha)]
        HiraganaIroha,
        [Map(CssConstants.Katakana)]
        Katakana,
        [Map(CssConstants.KatakanaIroha)]
        KatakanaIroha,
    }

    public enum CssNamedBorderWidth : byte
    {
        Unknown,
        [Map(CssConstants.Thin)]
        Thin,
        [Map(CssConstants.Medium)]
        Medium,
        [Map(CssConstants.Thick)]
        Thick
    }

    public enum CssBackgroundRepeat : byte
    {
        [Map(CssConstants.Repeat)]
        Repeat,
        [Map(CssConstants.RepeatX)]
        RepeatX,
        [Map(CssConstants.RepeatY)]
        RepeatY,
        [Map(CssConstants.NoRepeat)]
        NoRepeat,
        [Map(CssConstants.Inherit)]
        Inherit,
    }

    //flex spec: 5.1 flow flow direction ('flex-direction');
    //inherit =no
    public enum FlexFlowDirection : byte
    {
        [Map(CssConstants.Row)]
        Row,
        [Map(CssConstants.RowReverse)]
        RowReverse,
        [Map(CssConstants.Column)]
        Column,
        [Map(CssConstants.ColumnReverse)]
        ColumnReverse
    }
    //flex spec: 5.2 flex line wrapping, : the 'flex-wrap' property
    public enum FlexWrap : byte
    {
        [Map(CssConstants.NoWrap)]
        NoWrap,
        [Map(CssConstants.Wrap)]
        Wrap,
        [Map(CssConstants.WrapReverse)]
        WrapReverse
    }

    //flex spec: 8.2 axis alignment:this 'justify-content' property
    public enum FlexJustifyContent : byte
    {
        [Map(CssConstants.FlexStart)]
        FlexStart,//default
        [Map(CssConstants.FlexEnd)]
        FlextEnd,
        [Map(CssConstants.Center)]
        Center,
        [Map(CssConstants.SpaceBetween)]
        SpaceBetween,
        [Map(CssConstants.SpaceAround)]
        SpaceAround
    }

    //flex spec 8.3: Cross-axis aligment: the 'align-items' and 
    //'align-self' properties
    public enum FlexAlignItem : byte
    {
        [Map(CssConstants.Stretch)]
        Stretch, //initial value
        [Map(CssConstants.FlexStart)]
        FlexStart,//default
        [Map(CssConstants.FlexEnd)]
        FlextEnd,
        [Map(CssConstants.Center)]
        Center,
        [Map(CssConstants.Baseline)]
        Baseline
    }

    public enum FlexAlignSelf : byte
    {
        [Map(CssConstants.Auto)]
        Auto,
        [Map(CssConstants.Stretch)]
        Stretch, //initial value
        [Map(CssConstants.FlexStart)]
        FlexStart,//default
        [Map(CssConstants.FlexEnd)]
        FlextEnd,
        [Map(CssConstants.Center)]
        Center,
        [Map(CssConstants.Baseline)]
        Baseline
    }


    //flex spec 8.4 : Packing Flex Lines: the 'align-content' property
    public enum FlexAlignContent : byte
    {
        [Map(CssConstants.Stretch)]
        Stretch, //initial value
        [Map(CssConstants.FlexStart)]
        FlexStart,
        [Map(CssConstants.FlexEnd)]
        FlextEnd,
        [Map(CssConstants.Center)]
        Center,
        [Map(CssConstants.SpaceBetween)]
        SpaceBetween,
        [Map(CssConstants.SpaceAround)]
        SpaceAround,
    }

    public enum CssBoxSizing : byte
    {
        [Map(CssConstants.ContentBox)]
        ContentBox,//default
        [Map(CssConstants.BorderBox)]
        BorderBox,
        [Map(CssConstants.Initial)]
        Initial,
        [Map(CssConstants.Inherit)]
        Inherit
    }
}