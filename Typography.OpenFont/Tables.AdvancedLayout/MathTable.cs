
//MIT, 2018, WinterDev
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

            public static void ReadMathValueRecords(this BinaryReader reader,
                out MathValueRecord v0,
                out MathValueRecord v1,
                out MathValueRecord v2,
                out MathValueRecord v3)
            {
                v0 = ReadMathValueRecord(reader);
                v1 = ReadMathValueRecord(reader);
                v2 = ReadMathValueRecord(reader);
                v3 = ReadMathValueRecord(reader);
            }
        }
    }


    //https://www.microsoft.com/typography/otspec/math.htm
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
            reader.ReadMathValueRecords(
                out mc.MathLeading,
                out mc.AxisHeight,
                out mc.AccentBaseHeight,
                out mc.FlattenedAccentBaseHeight);
            //
            reader.ReadMathValueRecords(
                out mc.SubscriptShiftDown,
                out mc.SubscriptTopMax,
                out mc.SubscriptBaselineDropMin,
                out mc.SuperscriptShiftUp);
            //
            reader.ReadMathValueRecords(
                out mc.SuperscriptShiftUpCramped,
                out mc.SuperscriptBottomMin,
                out mc.SuperscriptBaselineDropMax,
                out mc.SubSuperscriptGapMin);
            //
            reader.ReadMathValueRecords(
               out mc.SuperscriptBottomMaxWithSubscript,
               out mc.SpaceAfterScript,
               out mc.UpperLimitGapMin,
               out mc.UpperLimitBaselineRiseMin);
            mc.LowerLimitGapMin = reader.ReadMathValueRecord();
            mc.LowerLimitBaselineDropMin = reader.ReadMathValueRecord();
            //
            reader.ReadMathValueRecords(
               out mc.StackTopShiftUp,
               out mc.StackTopDisplayStyleShiftUp,
               out mc.StackBottomShiftDown,
               out mc.StackBottomDisplayStyleShiftDown);
            reader.ReadMathValueRecords(
               out mc.StackGapMin,
               out mc.StackDisplayStyleGapMin,
               out mc.StretchStackTopShiftUp,
               out mc.StretchStackBottomShiftDown);
            mc.StretchStackGapAboveMin = reader.ReadMathValueRecord();
            mc.StretchStackGapBelowMin = reader.ReadMathValueRecord();
            // 
            reader.ReadMathValueRecords(
               out mc.FractionNumeratorShiftUp,
               out mc.FractionNumeratorDisplayStyleShiftUp,
               out mc.FractionDenominatorShiftDown,
               out mc.FractionDenominatorDisplayStyleShiftDown);
            reader.ReadMathValueRecords(
               out mc.FractionNumeratorGapMin,
               out mc.FractionNumDisplayStyleGapMin,
               out mc.FractionRuleThickness,
               out mc.FractionDenominatorGapMin);
            mc.FractionDenomDisplayStyleGapMin = reader.ReadMathValueRecord();
            //
            reader.ReadMathValueRecords(
               out mc.SkewedFractionHorizontalGap,
               out mc.SkewedFractionVerticalGap,
               out mc.OverbarVerticalGap,
               out mc.OverbarRuleThickness);
            reader.ReadMathValueRecords(
               out mc.OverbarExtraAscender,
               out mc.UnderbarVerticalGap,
               out mc.UnderbarRuleThickness,
               out mc.UnderbarExtraDescender);
            //
            reader.ReadMathValueRecords(
               out mc.RadicalVerticalGap,
               out mc.RadicalDisplayStyleVerticalGap,
               out mc.RadicalRuleThickness,
               out mc.RadicalExtraAscender);
            mc.RadicalKernBeforeDegree = reader.ReadMathValueRecord();
            mc.RadicalKernAfterDegree = reader.ReadMathValueRecord();
            mc.RadicalDegreeBottomRaisePercent = reader.ReadInt16();


        }
        void ReadMathMathGlyphInfoTable(BinaryReader reader)
        {

        }
        void ReadMathMathVariantsTable(BinaryReader reader)
        {


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

    class MathConstantsTable
    {

        //    When selecting names for values in the MathConstants table, the following naming convention should be used:

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

    class MathKernTable { }

    class MathVariantsTable { }

    class MathGlyphConstructionTable { }
    class GlyphAssemblyTable { }
}
