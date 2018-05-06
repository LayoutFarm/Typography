//MIT, 2018, WinterDev
//https://www.microsoft.com/typography/otspec/math.htm

using System.Collections.Generic;
using System.IO;

namespace Typography.OpenFont.Tables
{
    using MathInternal;
    namespace MathInternal
    {
        static class MathValueRecordReaderHelper
        {
            public static MathValueRecord ReadMathValueRecord(this BinaryReader reader)
            {
                return new MathValueRecord(reader.ReadInt16(), reader.ReadUInt16());
            }

            public static MathValueRecord[] ReadMathValueRecords(this BinaryReader reader, int count)
            {
                MathValueRecord[] records = new MathValueRecord[count];
                for (int i = 0; i < count; ++i)
                {
                    records[i] = reader.ReadMathValueRecord();
                }
                return records;
            }
        }
    }



    class MathTable : TableEntry
    {
        MathConstantsTable _mathConstTable;

        public override string Name
        {
            get { return "MATH"; }
        }
        protected override void ReadContentFrom(BinaryReader reader)
        {
            //eg. latin-modern-math-regular.otf, asana-math.otf

            long beginAt = reader.BaseStream.Position;
            //math table header
            //Type          Name    Description
            //uint16        MajorVersion Major version of the MATH table, = 1.
            //uint16        MinorVersion    Minor version of the MATH table, = 0.
            //Offset16      MathConstants   Offset to MathConstants table -from the beginning of MATH table.
            //Offset16      MathGlyphInfo   Offset to MathGlyphInfo table -from the beginning of MATH table.
            //Offset16      MathVariants    Offset to MathVariants table -from the beginning of MATH table.

            ushort majorVersion = reader.ReadUInt16();
            ushort minorVersion = reader.ReadUInt16();
            ushort mathConstants_offset = reader.ReadUInt16();
            ushort mathGlyphInfo_offset = reader.ReadUInt16();
            ushort mathVariants_offset = reader.ReadUInt16();
            //---------------------------------

            reader.BaseStream.Position = beginAt + mathConstants_offset;
            ReadMathConstantsTable(reader);
            //
            reader.BaseStream.Position = beginAt + mathGlyphInfo_offset;
            ReadMathMathGlyphInfoTable(reader);
            //
            reader.BaseStream.Position = beginAt + mathVariants_offset;
            ReadMathMathVariantsTable(reader);
        }
        void ReadMathConstantsTable(BinaryReader reader)
        {
            //MathConstants Table

            //The MathConstants table defines miscellaneous constants required to properly position elements of mathematical formulas.
            //These constants belong to several groups of semantically related values such as values needed to properly position accents,
            //values for positioning superscripts and subscripts, and values for positioning elements of fractions.
            //The table also contains general use constants that may affect all parts of the formula,
            //such as axis height and math leading.Note that most of the constants deal with the vertical positioning.

            MathConstantsTable mc = new MathConstantsTable();
            mc.ScriptPercentScaleDown = reader.ReadInt16();
            mc.ScriptScriptPercentScaleDown = reader.ReadInt16();
            mc.DelimitedSubFormulaMinHeight = reader.ReadUInt16();
            mc.DisplayOperatorMinHeight = reader.ReadUInt16();
            //
            //            

            mc.MathLeading = reader.ReadMathValueRecord();
            mc.AxisHeight = reader.ReadMathValueRecord();
            mc.AccentBaseHeight = reader.ReadMathValueRecord();
            mc.FlattenedAccentBaseHeight = reader.ReadMathValueRecord();

            // 
            mc.SubscriptShiftDown = reader.ReadMathValueRecord();
            mc.SubscriptTopMax = reader.ReadMathValueRecord();
            mc.SubscriptBaselineDropMin = reader.ReadMathValueRecord();
            mc.SuperscriptShiftUp = reader.ReadMathValueRecord();
            //

            mc.SuperscriptShiftUpCramped = reader.ReadMathValueRecord();
            mc.SuperscriptBottomMin = reader.ReadMathValueRecord();
            mc.SuperscriptBaselineDropMax = reader.ReadMathValueRecord();
            mc.SubSuperscriptGapMin = reader.ReadMathValueRecord();

            mc.SuperscriptBottomMaxWithSubscript = reader.ReadMathValueRecord();
            mc.SpaceAfterScript = reader.ReadMathValueRecord();
            mc.UpperLimitGapMin = reader.ReadMathValueRecord();
            mc.UpperLimitBaselineRiseMin = reader.ReadMathValueRecord();

            mc.LowerLimitGapMin = reader.ReadMathValueRecord();
            mc.LowerLimitBaselineDropMin = reader.ReadMathValueRecord();
            // 
            mc.StackTopShiftUp = reader.ReadMathValueRecord();
            mc.StackTopDisplayStyleShiftUp = reader.ReadMathValueRecord();
            mc.StackBottomShiftDown = reader.ReadMathValueRecord();
            mc.StackBottomDisplayStyleShiftDown = reader.ReadMathValueRecord();

            mc.StackGapMin = reader.ReadMathValueRecord();
            mc.StackDisplayStyleGapMin = reader.ReadMathValueRecord();
            mc.StretchStackTopShiftUp = reader.ReadMathValueRecord();
            mc.StretchStackBottomShiftDown = reader.ReadMathValueRecord();

            mc.StretchStackGapAboveMin = reader.ReadMathValueRecord();
            mc.StretchStackGapBelowMin = reader.ReadMathValueRecord();
            // 

            mc.FractionNumeratorShiftUp = reader.ReadMathValueRecord();
            mc.FractionNumeratorDisplayStyleShiftUp = reader.ReadMathValueRecord();
            mc.FractionDenominatorShiftDown = reader.ReadMathValueRecord();
            mc.FractionDenominatorDisplayStyleShiftDown = reader.ReadMathValueRecord();

            mc.FractionNumeratorGapMin = reader.ReadMathValueRecord();
            mc.FractionNumDisplayStyleGapMin = reader.ReadMathValueRecord();
            mc.FractionRuleThickness = reader.ReadMathValueRecord();
            mc.FractionDenominatorGapMin = reader.ReadMathValueRecord();

            mc.FractionDenomDisplayStyleGapMin = reader.ReadMathValueRecord();
            // 
            mc.SkewedFractionHorizontalGap = reader.ReadMathValueRecord();
            mc.SkewedFractionVerticalGap = reader.ReadMathValueRecord();
            mc.OverbarVerticalGap = reader.ReadMathValueRecord();
            mc.OverbarRuleThickness = reader.ReadMathValueRecord();
            //
            mc.OverbarExtraAscender = reader.ReadMathValueRecord();
            mc.UnderbarVerticalGap = reader.ReadMathValueRecord();
            mc.UnderbarRuleThickness = reader.ReadMathValueRecord();
            mc.UnderbarExtraDescender = reader.ReadMathValueRecord();

            mc.RadicalVerticalGap = reader.ReadMathValueRecord();
            mc.RadicalDisplayStyleVerticalGap = reader.ReadMathValueRecord();
            mc.RadicalRuleThickness = reader.ReadMathValueRecord();
            mc.RadicalExtraAscender = reader.ReadMathValueRecord();

            mc.RadicalKernBeforeDegree = reader.ReadMathValueRecord();
            mc.RadicalKernAfterDegree = reader.ReadMathValueRecord();
            mc.RadicalDegreeBottomRaisePercent = reader.ReadInt16();


            this._mathConstTable = mc;
        }
        void ReadMathMathGlyphInfoTable(BinaryReader reader)
        {

            //MathGlyphInfo Table
            //  The MathGlyphInfo table contains positioning information that is defined on per - glyph basis.The table consists of the following parts:
            //    Offset to MathItalicsCorrectionInfo table that contains information on italics correction values.
            //    Offset to MathTopAccentAttachment table that contains horizontal positions for attaching mathematical accents.
            //    Offset to Extended Shape coverage table.The glyphs covered by this table are to be considered extended shapes.
            //    Offset to MathKernInfo table that provides per - glyph information for mathematical kerning.


            //  NOTE: Here, and elsewhere in the subclause – please refer to subclause 6.2.4 "Features and Lookups" for description of the coverage table formats.

            long startAt = reader.BaseStream.Position;
            ushort offsetTo_MathItalicsCorrectionInfo_Table = reader.ReadUInt16();
            ushort offsetTo_MathTopAccentAttachment_Table = reader.ReadUInt16();
            ushort offsetTo_Extended_Shape_coverage_Table = reader.ReadUInt16();
            ushort offsetTo_MathKernInfo_Table = reader.ReadUInt16();
            //-----------------------

            reader.BaseStream.Position = startAt + offsetTo_MathItalicsCorrectionInfo_Table;
            ReadMathItalicCorrectionInfoTable(reader);
            //
            reader.BaseStream.Position = startAt + offsetTo_MathTopAccentAttachment_Table;
            ReadMathTopAccentAttachment(reader);
            //
            reader.BaseStream.Position = startAt + offsetTo_Extended_Shape_coverage_Table;
            ReadExtendedShapeCoverageTable(reader);
            //
            reader.BaseStream.Position = startAt + offsetTo_MathKernInfo_Table;
            ReadMathKernInfoTable(reader);
        }


        void ReadMathMathVariantsTable(BinaryReader reader)
        {
            _mathVariantsTable = new MathVariantsTable();
            _mathVariantsTable.ReadContentFrom(reader);
        }

        MathVariantsTable _mathVariantsTable;
        MathItalicsCorrectonInfoTable _mathItalicCorrectionInfo;
        void ReadMathItalicCorrectionInfoTable(BinaryReader reader)
        {
            long startAt = reader.BaseStream.Position;
            _mathItalicCorrectionInfo = new MathItalicsCorrectonInfoTable();


            //MathItalicsCorrectionInfo Table
            //Type           Name                           Description
            //Offset16       Coverage                       Offset to Coverage table - from the beginning of MathItalicsCorrectionInfo table.
            //uint16         ItalicsCorrectionCount         Number of italics correction values.Should coincide with the number of covered glyphs.
            //MathValueRecord ItalicsCorrection[ItalicsCorrectionCount]  Array of MathValueRecords defining italics correction values for each covered glyph.


            ushort coverageOffset = reader.ReadUInt16();
            ushort italicCorrectionCount = reader.ReadUInt16();
            MathValueRecord[] italicCorrections = reader.ReadMathValueRecords(italicCorrectionCount);
            //read coverage ...

        }
        void ReadMathTopAccentAttachment(BinaryReader reader)
        {
            //MathTopAccentAttachment Table

            //The MathTopAccentAttachment table contains information on horizontal positioning of top math accents. The table consists of the following parts:

            //Coverage of glyphs for which information on horizontal positioning of math accents is provided.To position accents over any other glyph, its geometrical center(with respect to advance width) can be used.

            //Count of covered glyphs.

            //Array of top accent attachment points for each covered glyph, in order of coverage.These attachment points are to be used for finding horizontal positions of accents over characters.It is done by aligning the attachment point of the base glyph with the attachment point of the accent.Note that this is very similar to mark - to - base attachment, but here alignment only happens in the horizontal direction, and the vertical positions of accents are determined by different means.
            //MathTopAccentAttachment Table
            //Type          Name                        Description
            //Offset16      TopAccentCoverage           Offset to Coverage table - from the beginning of MathTopAccentAttachment table.
            //uint16        TopAccentAttachmentCount    Number of top accent attachment point values.Should coincide with the number of covered glyphs.
            //MathValueRecord TopAccentAttachment[TopAccentAttachmentCount]  Array of MathValueRecords defining top accent attachment points for each covered glyph.
            ushort topAccentCoverage = reader.ReadUInt16();
            ushort topAccentAttachmentCount = reader.ReadUInt16();
            MathValueRecord[] topAccentAttachMents = reader.ReadMathValueRecords(topAccentAttachmentCount);
        }
        void ReadExtendedShapeCoverageTable(BinaryReader reader)
        {
            //TODO:...
            //The glyphs covered by this table are to be considered extended shapes.
            //These glyphs are variants extended in the vertical direction, e.g.,
            //to match height of another part of the formula.
            //Because their dimensions may be very large in comparison with normal glyphs in the glyph set,
            //the standard positioning algorithms will not produce the best results when applied to them.
            //In the vertical direction, other formula elements will be positioned not relative to those glyphs,
            //but instead to the ink box of the subexpression containing them

            //....

        }
        void ReadMathKernInfoTable(BinaryReader reader)
        {
            // MathKernInfo Table

            //The MathKernInfo table provides information on glyphs for which mathematical (height - dependent) kerning values are defined.It consists of the following fields:

            //    Coverage of glyphs for which mathematical kerning information is provided.
            //    Count of MathKernInfoRecords.Should coincide with the number of glyphs in Coverage table.
            //    Array of MathKernInfoRecords for each covered glyph, in order of coverage.

            //MathKernInfo Table
            //Type          Name                Description
            //Offset16      MathKernCoverage    Offset to Coverage table - from the beginning of the MathKernInfo table.
            //uint16        MathKernCount       Number of MathKernInfoRecords.
            //MathKernInfoRecord MathKernInfoRecords[MathKernCount]     Array of MathKernInfoRecords, per - glyph information for mathematical positioning of subscripts and superscripts.

            //...
            //Each MathKernInfoRecord points to up to four kern tables for each of the corners around the glyph.

            ushort mathKernCoverage = reader.ReadUInt16();
            ushort mathKernCount = reader.ReadUInt16();
            MathKernInfoRecord[] mathKernInfoRecords = new MathKernInfoRecord[mathKernCount];
            for (int i = 0; i < mathKernCount; ++i)
            {
                mathKernInfoRecords[i] = new MathKernInfoRecord(
                    reader.ReadUInt16(),
                    reader.ReadUInt16(),
                    reader.ReadUInt16(),
                    reader.ReadUInt16()
                    );
            }

        }
    }


    //MathValueRecord
    //Type      Name            Description
    //int16     Value           The X or Y value in design units
    //Offset16  DeviceTable     Offset to the device table – from the beginning of parent table.May be NULL. Suggested format for device table is 1.
    struct MathValueRecord
    {
        public readonly short Value;
        public readonly ushort DeviceTable;
        public MathValueRecord(short value, ushort deviceTable)
        {
            this.Value = value;
            this.DeviceTable = deviceTable;
        }
    }

    struct MathKernInfoRecord
    {
        public readonly ushort TopRightMathKern;
        public readonly ushort TopLeftMathKern;
        public readonly ushort BottomRightMathKern;
        public readonly ushort BottomLeftMathKern;
        public MathKernInfoRecord(ushort topRight, ushort topLeft, ushort bottomRight, ushort bottomLeft)
        {
            this.TopRightMathKern = topRight;
            this.TopLeftMathKern = topLeft;
            this.BottomRightMathKern = bottomRight;
            this.BottomLeftMathKern = bottomLeft;
        }
    }

    class MathKernTable
    {
        //The MathKern table contains adjustments to horizontal positions of subscripts and superscripts
        //The kerning algorithm consists of the following steps:

        //1. Calculate vertical positions of subscripts and superscripts.
        //2. Set the default horizontal position for the subscript immediately after the base glyph.
        //3. Set the default horizontal position for the superscript as shifted relative to the position of the subscript by the italics correction of the base glyph.
        //4. Based on the vertical positions, calculate the height of the top/ bottom for the bounding boxes of sub/superscript relative to the base glyph, and the height of the top/ bottom of the base relative to the super/ subscript.These will be the correction heights.
        //5. Get the kern values corresponding to these correction heights for the appropriate corner of the base glyph and sub/superscript glyph from the appropriate MathKern tables.Kern the default horizontal positions by the minimum of sums of those values at the correction heights for the base and for the sub/superscript.
        //6. If either one of the base or superscript expression has to be treated as a box not providing glyph
        //MathKern Table
        //Type              Name                                Description
        //uint16            HeightCount                         Number of heights on which the kern value changes.
        //MathValueRecord   CorrectionHeight[HeightCount]       Array of correction heights at which the kern value changes.Sorted by the height value in design units.
        //MathValueRecord   KernValue[HeightCount+1]            Array of kern values corresponding to heights.

        //First value is the kern value for all heights less or equal than the first height in this table.
        //Last value is the value to be applied for all heights greater than the last height in this table.
        //Negative values are interpreted as "move glyphs closer to each other".

        public ushort HeightCount;
        public MathValueRecord[] CorrectionHeights;
        public MathValueRecord[] KernValues;

    }

    class MathConstantsTable
    {

        //When selecting names for values in the MathConstants table, the following naming convention should be used:

        //Height – Specifies a distance from the main baseline.
        //Kern – Represents a fixed amount of empty space to be introduced.
        //Gap – Represents an amount of empty space that may need to be increased to meet certain criteria.
        //Drop and Rise – Specifies the relationship between measurements of two elements to be positioned relative to each other(but not necessarily in a stack - like manner) that must meet certain criteria.For a Drop, one of the positioned elements has to be moved down to satisfy those criteria; for a Rise, the movement is upwards.
        //Shift – Defines a vertical shift applied to an element sitting on a baseline.
        //Dist – Defines a distance between baselines of two elements.

        /// <summary>
        /// Percentage of scaling down for script level 1. 
        /// Suggested value: 80%.
        /// </summary>
        public short ScriptPercentScaleDown;
        /// <summary>
        /// Percentage of scaling down for script level 2 (ScriptScript).
        /// Suggested value: 60%.
        /// </summary>
        public short ScriptScriptPercentScaleDown;
        /// <summary>
        /// Minimum height required for a delimited expression to be treated as a sub-formula.
        /// Suggested value: normal line height ×1.5.
        /// </summary>
        public ushort DelimitedSubFormulaMinHeight;
        /// <summary>
        ///  	Minimum height of n-ary operators (such as integral and summation) for formulas in display mode.
        /// </summary>
        public ushort DisplayOperatorMinHeight;


        /// <summary>
        /// White space to be left between math formulas to ensure proper line spacing. 
        /// For example, for applications that treat line gap as a part of line ascender,
        /// formulas with ink going above (os2.sTypoAscender + os2.sTypoLineGap - MathLeading) 
        /// or with ink going below os2.sTypoDescender will result in increasing line height.
        /// </summary>
        public MathValueRecord MathLeading;
        /// <summary>
        /// Axis height of the font.
        /// </summary>
        public MathValueRecord AxisHeight;
        /// <summary>
        /// Maximum (ink) height of accent base that does not require raising the accents.
        /// Suggested: x‑height of the font (os2.sxHeight) plus any possible overshots.
        /// </summary>
        public MathValueRecord AccentBaseHeight;
        /// <summary>
        ///Maximum (ink) height of accent base that does not require flattening the accents. 
        ///Suggested: cap height of the font (os2.sCapHeight).
        /// </summary>
        public MathValueRecord FlattenedAccentBaseHeight;

        //---------------------------------------------------------
        /// <summary>
        /// The standard shift down applied to subscript elements.
        /// Positive for moving in the downward direction. 
        /// Suggested: os2.ySubscriptYOffset.
        /// </summary>
        public MathValueRecord SubscriptShiftDown;
        /// <summary>
        /// Maximum allowed height of the (ink) top of subscripts that does not require moving subscripts further down.
        /// Suggested: 4/5 x- height.
        /// </summary>
        public MathValueRecord SubscriptTopMax;
        /// <summary>
        /// Minimum allowed drop of the baseline of subscripts relative to the (ink) bottom of the base.
        /// Checked for bases that are treated as a box or extended shape. 
        /// Positive for subscript baseline dropped below the base bottom.
        /// </summary>
        public MathValueRecord SubscriptBaselineDropMin;
        /// <summary>
        /// Standard shift up applied to superscript elements. 
        /// Suggested: os2.ySuperscriptYOffset.
        /// </summary>
        public MathValueRecord SuperscriptShiftUp;
        /// <summary>
        /// Standard shift of superscripts relative to the base, in cramped style.
        /// </summary>
        public MathValueRecord SuperscriptShiftUpCramped;
        /// <summary>
        /// Minimum allowed height of the (ink) bottom of superscripts that does not require moving subscripts further up. 
        /// Suggested: ¼ x-height.
        /// </summary>
        public MathValueRecord SuperscriptBottomMin;
        /// <summary>
        ///  Maximum allowed drop of the baseline of superscripts relative to the (ink) top of the base. Checked for bases that are treated as a box or extended shape. 
        ///  Positive for superscript baseline below the base top.
        /// </summary>
        public MathValueRecord SuperscriptBaselineDropMax;
        /// <summary>
        /// Minimum gap between the superscript and subscript ink. 
        /// Suggested: 4×default rule thickness.
        /// </summary>
        public MathValueRecord SubSuperscriptGapMin;
        /// <summary>
        /// The maximum level to which the (ink) bottom of superscript can be pushed to increase the gap between 
        /// superscript and subscript, before subscript starts being moved down. 
        /// Suggested: 4/5 x-height.
        /// </summary>
        public MathValueRecord SuperscriptBottomMaxWithSubscript;
        /// <summary>
        /// Extra white space to be added after each subscript and superscript. Suggested: 0.5pt for a 12 pt font.
        /// </summary>
        public MathValueRecord SpaceAfterScript;

        //---------------------------------------------------------
        /// <summary>
        /// Minimum gap between the (ink) bottom of the upper limit, and the (ink) top of the base operator.
        /// </summary>
        public MathValueRecord UpperLimitGapMin;
        /// <summary>
        /// Minimum distance between baseline of upper limit and (ink) top of the base operator.
        /// </summary>
        public MathValueRecord UpperLimitBaselineRiseMin;
        /// <summary>
        /// Minimum gap between (ink) top of the lower limit, and (ink) bottom of the base operator.
        /// </summary>
        public MathValueRecord LowerLimitGapMin;
        /// <summary>
        /// Minimum distance between baseline of the lower limit and (ink) bottom of the base operator.
        /// </summary>
        public MathValueRecord LowerLimitBaselineDropMin;

        //---------------------------------------------------------
        /// <summary>
        /// Standard shift up applied to the top element of a stack.
        /// </summary>
        public MathValueRecord StackTopShiftUp;
        /// <summary>
        /// Standard shift up applied to the top element of a stack in display style.
        /// </summary>
        public MathValueRecord StackTopDisplayStyleShiftUp;
        /// <summary>
        /// Standard shift down applied to the bottom element of a stack. 
        /// Positive for moving in the downward direction.
        /// </summary>
        public MathValueRecord StackBottomShiftDown;
        /// <summary>
        /// Standard shift down applied to the bottom element of a stack in display style.
        /// Positive for moving in the downward direction.
        /// </summary>
        public MathValueRecord StackBottomDisplayStyleShiftDown;
        /// <summary>
        /// Minimum gap between (ink) bottom of the top element of a stack, and the (ink) top of the bottom element.
        /// Suggested: 3×default rule thickness.
        /// </summary>
        public MathValueRecord StackGapMin;
        /// <summary>
        /// Minimum gap between (ink) bottom of the top element of a stack, and the (ink) top of the bottom element in display style.
        /// Suggested: 7×default rule thickness.
        /// </summary>
        public MathValueRecord StackDisplayStyleGapMin;

        /// <summary>
        /// Standard shift up applied to the top element of the stretch stack.
        /// </summary>
        public MathValueRecord StretchStackTopShiftUp;
        /// <summary>
        /// Standard shift down applied to the bottom element of the stretch stack.
        /// Positive for moving in the downward direction.
        /// </summary>         
        public MathValueRecord StretchStackBottomShiftDown;
        /// <summary>
        /// Minimum gap between the ink of the stretched element, and the (ink) bottom of the element above. 
        /// Suggested: UpperLimitGapMin.
        /// </summary>
        public MathValueRecord StretchStackGapAboveMin;
        /// <summary>
        /// Minimum gap between the ink of the stretched element, and the (ink) top of the element below. 
        /// Suggested: LowerLimitGapMin.
        /// </summary>
        public MathValueRecord StretchStackGapBelowMin;



        //---------------------------------------------------------
        /// <summary>
        /// Standard shift up applied to the numerator.
        /// </summary>
        public MathValueRecord FractionNumeratorShiftUp;
        /// <summary>
        /// Standard shift up applied to the numerator in display style. Suggested: StackTopDisplayStyleShiftUp.
        /// </summary>
        public MathValueRecord FractionNumeratorDisplayStyleShiftUp;
        /// <summary>
        /// Standard shift down applied to the denominator. Positive for moving in the downward direction.
        /// </summary>
        public MathValueRecord FractionDenominatorShiftDown;
        /// <summary>
        /// Standard shift down applied to the denominator in display style. Positive for moving in the downward direction. 
        /// Suggested: StackBottomDisplayStyleShiftDown
        /// </summary>
        public MathValueRecord FractionDenominatorDisplayStyleShiftDown;
        /// <summary>
        ///  Minimum tolerated gap between the (ink) bottom of the numerator and the ink of the fraction bar. 
        ///  Suggested: default rule thickness.
        /// </summary>
        public MathValueRecord FractionNumeratorGapMin;
        /// <summary>
        /// Minimum tolerated gap between the (ink) bottom of the numerator and the ink of the fraction bar in display style. 
        /// Suggested: 3×default rule thickness
        /// </summary>
        public MathValueRecord FractionNumDisplayStyleGapMin;
        /// <summary>
        /// Thickness of the fraction bar. 
        /// Suggested: default rule thickness.
        /// </summary>
        public MathValueRecord FractionRuleThickness;
        /// <summary>
        ///  Minimum tolerated gap between the (ink) top of the denominator and the ink of the fraction bar.
        ///  Suggested: default rule thickness.
        /// </summary>
        public MathValueRecord FractionDenominatorGapMin;
        /// <summary>
        /// Minimum tolerated gap between the (ink) top of the denominator and the ink of the fraction bar in display style. 
        /// Suggested: 3×default rule thickness
        /// </summary>
        public MathValueRecord FractionDenomDisplayStyleGapMin;



        //---------------------------------------------------------
        /// <summary>
        /// Horizontal distance between the top and bottom elements of a skewed fraction.
        /// </summary>
        public MathValueRecord SkewedFractionHorizontalGap;
        /// <summary>
        /// Vertical distance between the ink of the top and bottom elements of a skewed fraction
        /// </summary>
        public MathValueRecord SkewedFractionVerticalGap;



        //---------------------------------------------------------
        /// <summary>
        /// Distance between the overbar and the (ink) top of he base.
        /// Suggested: 3×default rule thickness.
        /// </summary>
        public MathValueRecord OverbarVerticalGap;
        /// <summary>
        /// Thickness of overbar. 
        /// Suggested: default rule thickness.
        /// </summary>
        public MathValueRecord OverbarRuleThickness;
        /// <summary>
        /// Extra white space reserved above the overbar. 
        /// Suggested: default rule thickness.
        /// </summary>
        public MathValueRecord OverbarExtraAscender;



        //---------------------------------------------------------
        /// <summary>
        /// Distance between underbar and (ink) bottom of the base. 
        /// Suggested: 3×default rule thickness.
        /// </summary>
        public MathValueRecord UnderbarVerticalGap;
        /// <summary>
        /// Thickness of underbar. 
        /// Suggested: default rule thickness.
        /// </summary>
        public MathValueRecord UnderbarRuleThickness;
        /// <summary>
        /// Extra white space reserved below the underbar. Always positive. 
        /// Suggested: default rule thickness.
        /// </summary>
        public MathValueRecord UnderbarExtraDescender;



        //---------------------------------------------------------
        /// <summary>
        /// Space between the (ink) top of the expression and the bar over it. 
        /// Suggested: 1¼ default rule thickness.
        /// </summary>
        public MathValueRecord RadicalVerticalGap;
        /// <summary>
        ///  Space between the (ink) top of the expression and the bar over it. 
        ///  Suggested: default rule thickness + ¼ x-height.
        /// </summary>
        public MathValueRecord RadicalDisplayStyleVerticalGap;
        /// <summary>
        ///  Thickness of the radical rule. This is the thickness of the rule in designed or constructed radical signs. 
        ///  Suggested: default rule thickness.
        /// </summary>
        public MathValueRecord RadicalRuleThickness;
        /// <summary>
        /// Extra white space reserved above the radical.
        /// Suggested: RadicalRuleThickness.
        /// </summary>
        public MathValueRecord RadicalExtraAscender;
        /// <summary>
        /// Extra horizontal kern before the degree of a radical, if such is present.
        /// </summary>
        public MathValueRecord RadicalKernBeforeDegree;
        /// <summary>
        /// Negative kern after the degree of a radical, if such is present. 
        /// Suggested: −10/18 of em
        /// </summary>
        public MathValueRecord RadicalKernAfterDegree;
        /// <summary>
        ///  Height of the bottom of the radical degree, 
        ///  if such is present, in proportion to the ascender of the radical sign. 
        ///  Suggested: 60%.
        /// </summary>
        public short RadicalDegreeBottomRaisePercent;
    }
    class MathGlyphInfoTable
    {

    }
    class MathItalicsCorrectonInfoTable
    {
        //MathItalicsCorrectonInfo Table 
        //The MathItalicsCorrectionInfo table contains italics correction values for slanted glyphs used in math typesetting.The table consists of the following parts:

        //    Coverage of glyphs for which the italics correction values are provided.It is assumed to be zero for all other glyphs.
        //    Count of covered glyphs.
        //    Array of italic correction values for each covered glyph, in order of coverage.The italics correction is the measurement of how slanted the glyph is, and how much its top part protrudes to the right. For example, taller letters tend to have larger italics correction, and a V will probably have larger italics correction than an L.

        //Italics correction can be used in the following situations:

        //    When a run of slanted characters is followed by a straight character (such as an operator or a delimiter), the italics correction of the last glyph is added to its advance width.
        //    When positioning limits on an N-ary operator (e.g., integral sign), the horizontal position of the upper limit is moved to the right by ½ of the italics correction, while the position of the lower limit is moved to the left by the same distance.
        //    When positioning superscripts and subscripts, their default horizontal positions are also different by the amount of the italics correction of the preceding glyph.




    }
    class MathTopAccentAttachmentTable
    {

    }
    class ExtendedShapeCoverageTable
    {

    }
    class MathKernInfoTable
    {

    }
    class MathKernInfoRecordTable { }



    class MathVariantsTable
    {
        //MathVariants Table

        //The MathVariants table solves the following problem: given a particular default glyph shape and a certain width or height, find a variant shape glyph(or construct created by putting several glyph together) that has the required measurement.This functionality is needed for growing the parentheses to match the height of the expression within, growing the radical sign to match the height of the expression under the radical, stretching accents like tilde when they are put over several characters, for stretching arrows, horizontal curly braces, and so forth.

        //The MathVariants table consists of the following fields:


        //  Count and coverage of glyph that can grow in the vertical direction.
        //  Count and coverage of glyphs that can grow in the horizontal direction.
        //  MinConnectorOverlap defines by how much two glyphs need to overlap with each other when used to construct a larger shape. Each glyph to be used as a building block in constructing extended shapes will have a straight part at either or both ends.This connector part is used to connect that glyph to other glyphs in the assembly. These connectors need to overlap to compensate for rounding errors and hinting corrections at a lower resolution.The MinConnectorOverlap value tells how much overlap is necessary for this particular font.

        //  Two arrays of offsets to MathGlyphConstruction tables: one array for glyphs that grow in the vertical direction, and the other array for glyphs that grow in the horizontal direction.The arrays must be arranged in coverage order and have specified sizes.


        //MathVariants Table
        //Type          Name                    Description
        //uint16        MinConnectorOverlap     Minimum overlap of connecting glyphs during glyph construction, in design units.
        //Offset16      VertGlyphCoverage       Offset to Coverage table - from the beginning of MathVariants table.
        //Offset16      HorizGlyphCoverage      Offset to Coverage table - from the beginning of MathVariants table.
        //uint16        VertGlyphCount          Number of glyphs for which information is provided for vertically growing variants.
        //uint16        HorizGlyphCount         Number of glyphs for which information is provided for horizontally growing variants.
        //Offset16      VertGlyphConstruction[VertGlyphCount]  Array of offsets to MathGlyphConstruction tables - from the beginning of the MathVariants table, for shapes growing in vertical direction.
        //Offset16      HorizGlyphConstruction[HorizGlyphCount]    Array of offsets to MathGlyphConstruction tables - from the beginning of the MathVariants table, for shapes growing in horizontal direction.

        public ushort MinConnectorOverlap;
        public void ReadContentFrom(BinaryReader reader)
        {
            long startAt = reader.BaseStream.Position;
            //
            MinConnectorOverlap = reader.ReadUInt16();
            ushort vertGlyphCoverage = reader.ReadUInt16();
            ushort horizGlyphCoverage = reader.ReadUInt16();
            ushort vertGlyphCount = reader.ReadUInt16();
            ushort horizGlyphCount = reader.ReadUInt16();
            ushort[] vertGlyphConstructions = Utils.ReadUInt16Array(reader, vertGlyphCount);
            ushort[] horizonGlyphConstructions = Utils.ReadUInt16Array(reader, horizGlyphCount);
            //
        }
    }

    class MathGlyphConstructionTable
    {
        //MathGlyphConstruction Table  
        //The MathGlyphConstruction table provides information on finding or assembling extended variants for one particular glyph.It can be used for shapes that grow in both horizontal and vertical directions.

        //The first entry is the offset to the GlyphAssembly table that specifies how the shape for this glyph can be assembled from parts found in the glyph set of the font.If no such assembly exists, this offset will be set to NULL.

        //The MathGlyphConstruction table also contains the count and array of ready-made glyph variants for the specified glyph.Each variant consists of the glyph index and this glyph’s measurement in the direction of extension (vertical or horizontal).

        //Note that it is quite possible that both the GlyphAssembly table and some variants are defined for a particular glyph.For example, the font may specify several variants for curly braces of different sizes, and a general mechanism of how larger versions of curly braces can be constructed by stacking parts found in the glyph set.First attempt is made to find glyph among provided variants.However, if the required size is bigger than all glyph variants provided, the general mechanism can be employed to typeset the curly braces as a glyph assembly.

        //MathGlyphConstruction Table
        //Type          Name            Description
        //Offset16      GlyphAssembly   Offset to GlyphAssembly table for this shape - from the beginning of MathGlyphConstruction table.May be NULL.
        //uint16        VariantCount    Count of glyph growing variants for this glyph.
        //MathGlyphVariantRecord MathGlyphVariantRecord [VariantCount]   MathGlyphVariantRecords for alternative variants of the glyphs.


    }
    class GlyphAssemblyTable { }
}
