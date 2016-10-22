//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.Text;
namespace NOpenType
{


    public class Glyph
    {
        short[] _xs;
        short[] _ys;
        ushort[] _contourEndPoints;
        Bounds _bounds;
        bool[] _onCurves;
        public static readonly Glyph Empty = new Glyph(new short[0], new short[0], new bool[0], new ushort[0], Bounds.Zero);

#if DEBUG
        public readonly int dbugId;
        static int s_debugTotalId;
#endif
        internal Glyph(short[] xs, short[] ys, bool[] onCurves, ushort[] contourEndPoints, Bounds bounds)
        {

#if DEBUG
            this.dbugId = s_debugTotalId++;
#endif
            _xs = xs;
            _ys = ys;
            _onCurves = onCurves;
            _contourEndPoints = contourEndPoints;
            _bounds = bounds;
        }
        internal short[] Xs { get { return _xs; } }
        internal short[] Ys { get { return _ys; } }
        public Bounds Bounds { get { return _bounds; } }
        public ushort[] EndPoints { get { return _contourEndPoints; } }
        public bool[] OnCurves { get { return _onCurves; } }
        //--------------

        internal static void OffsetXY(Glyph glyph, short dx, short dy)
        {

            //change data on current glyph
            short[] xs = glyph._xs;
            short[] ys = glyph._ys;
            for (int i = xs.Length - 1; i >= 0; --i)
            {
                xs[i] += dx;
                ys[i] += dy;
            }
            //-------------------------
            Bounds orgBounds = glyph._bounds;
            glyph._bounds = new Bounds(
               (short)(orgBounds.XMin + dx),
               (short)(orgBounds.YMin + dy),
               (short)(orgBounds.XMax + dx),
               (short)(orgBounds.YMax + dy));

        }
        internal static void Apply2x2Matrix(Glyph glyph, float m00, float m01, float m10, float m11)
        {
            //x'= |m00 m01|x
            //y'= |m10 m11|y
            //--
            //x' = x*m00+ y*m01
            //y' = x*m10+ y*m11


            //change data on current glyph
            short[] xs = glyph._xs;
            short[] ys = glyph._ys;
            for (int i = xs.Length - 1; i >= 0; --i)
            {
                short x = xs[i];
                short y = ys[i];

                xs[i] = (short)((x * m00) + (y * m01));
                ys[i] = (short)((x * m10) + (y * m11));
            }
            //-------------------------
            Bounds orgBounds = glyph._bounds;
            short xmin = orgBounds.XMin;
            short ymin = orgBounds.YMin;

            short xmax = orgBounds.XMax;
            short ymax = orgBounds.YMax;

            glyph._bounds = new Bounds(
               (short)(xmin * m00 + ymin * m01),
               (short)(xmin * m10 + ymin * m11),
                //-----------------------------------
               (short)(xmax * m00 + ymax * m01),
               (short)(xmax * m00 + ymax * m01));

        }
        internal static Glyph Clone(Glyph original)
        {
            //----------------------
            short[] new_xs = CloneArray(original._xs);
            short[] new_ys = CloneArray(original._ys);
            ushort[] new_contourEndPoints = CloneArray(original._contourEndPoints);
            bool[] new_onCurves = CloneArray(original._onCurves);

            return new Glyph(new_xs, new_ys, new_onCurves, new_contourEndPoints, original.Bounds);
        }

        public static T[] CloneArray<T>(T[] original)
        {
            T[] newClone = new T[original.Length];
            Array.Copy(original, newClone, newClone.Length);
            return newClone;
        }



        internal GlyphClassKind GlyphClassDef { get; set; }
        internal ushort MarkClassDef { get; set; }
#if DEBUG
        public override string ToString()
        {
            var stbuilder = new StringBuilder();
            stbuilder.Append("class=" + GlyphClassDef.ToString());
            if (MarkClassDef != 0)
            {
                stbuilder.Append(",mark_class=" + MarkClassDef);
            }
            return stbuilder.ToString();
        }
#endif

    }

    //https://www.microsoft.com/typography/otspec/gdef.htm
    enum GlyphClassKind
    {
        //1 	Base glyph (single character, spacing glyph)
        //2 	Ligature glyph (multiple character, spacing glyph)
        //3 	Mark glyph (non-spacing combining glyph)
        //4 	Component glyph (part of single character, spacing glyph)
        //
        // The font developer does not have to classify every glyph in the font, 
        //but any glyph not assigned a class value falls into Class zero (0). 
        //For instance, class values might be useful for the Arabic glyphs in a font, but not for the Latin glyphs. 
        //Then the GlyphClassDef table will list only Arabic glyphs, and-by default-the Latin glyphs will be assigned to Class 0. 
        //Component glyphs can be put together to generate ligatures. 
        //A ligature can be generated by creating a glyph in the font that references the component glyphs, 
        //or outputting the component glyphs in the desired sequence. 
        //Component glyphs are not used in defining any GSUB or GPOS formats.
        //
        Zero = 0,//class0, classZero
        Base,
        Ligature,
        Mark,
        Component
    }
}
