//Apache2, 2017-present, WinterDev
//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.Text;
namespace Typography.OpenFont
{

    public class Glyph
    {
        ushort _orgAdvWidth;

        internal Glyph(
            GlyphPointF[] glyphPoints,
            ushort[] contourEndPoints,
            Bounds bounds,
            byte[]? glyphInstructions,
            ushort index)
        {
            //create from TTF 

#if DEBUG
            this.dbugId = s_debugTotalId++;
#endif
            TtfWoffInfo = (contourEndPoints, glyphPoints);
            Bounds = bounds;
            GlyphInstructions = glyphInstructions;
            GlyphIndex = index;

        }
        public Bounds Bounds { get; internal set; }
        //
        public (ushort[] endPoints, GlyphPointF[] glyphPoints)? TtfWoffInfo { get; private set; }
        //
        public ushort OriginalAdvanceWidth
        {
            get => _orgAdvWidth;
            set
            {
                _orgAdvWidth = value;
                HasOriginalAdvancedWidth = true;
            }
        }
        public bool HasOriginalAdvancedWidth { get; private set; }
        //      

        internal static void OffsetXY(Glyph glyph, short dx, short dy)
        {
            if (!(glyph.TtfWoffInfo is var (_, glyphPoints)))
                throw new NotSupportedException("Only TTF glyphs are supported");
            //change data on current glyph
            for (int i = glyphPoints.Length - 1; i >= 0; --i)
            {
                glyphPoints[i] = glyphPoints[i].Offset(dx, dy);
            }
            //-------------------------
            Bounds orgBounds = glyph.Bounds;
            glyph.Bounds = new Bounds(
               (short)(orgBounds.XMin + dx),
               (short)(orgBounds.YMin + dy),
               (short)(orgBounds.XMax + dx),
               (short)(orgBounds.YMax + dy));

        }
        public byte[]? GlyphInstructions { get; set; }

        public bool HasGlyphInstructions => this.GlyphInstructions != null;

        internal static void TransformNormalWith2x2Matrix(Glyph glyph, float m00, float m01, float m10, float m11)
        {

            if (!(glyph.TtfWoffInfo is var (_, glyphPoints)))
                throw new ArgumentException("Only TTF/WOFF glyphs are supported", nameof(glyph));
            //http://stackoverflow.com/questions/13188156/whats-the-different-between-vector2-transform-and-vector2-transformnormal-i
            //http://www.technologicalutopia.com/sourcecode/xnageometry/vector2.cs.htm

            //change data on current glyph
            float new_xmin = 0;
            float new_ymin = 0;
            float new_xmax = 0;
            float new_ymax = 0;


            for (int i = glyphPoints.Length - 1; i >= 0; --i)
            {
                GlyphPointF p = glyphPoints[i];
                float x = p.P.X;
                float y = p.P.Y;

                float newX, newY;
                //please note that this is transform normal***
                glyphPoints[i] = new GlyphPointF(
                   newX = (float)Math.Round((x * m00) + (y * m10)),
                   newY = (float)Math.Round((x * m01) + (y * m11)),
                   p.onCurve);

                //short newX = xs[i] = (short)Math.Round((x * m00) + (y * m10));
                //short newY = ys[i] = (short)Math.Round((x * m01) + (y * m11));
                //------
                if (newX < new_xmin)
                {
                    new_xmin = newX;
                }
                if (newX > new_xmax)
                {
                    new_xmax = newX;
                }
                //------
                if (newY < new_ymin)
                {
                    new_ymin = newY;
                }
                if (newY > new_ymax)
                {
                    new_ymax = newY;
                }
            }
            //TODO: review here
            glyph.Bounds = new Bounds(
               (short)new_xmin, (short)new_ymin,
               (short)new_xmax, (short)new_ymax);
        }

        internal static Glyph Clone(Glyph original, ushort newGlyphIndex)
        {
            if (original.TtfWoffInfo is var (endPoints, glyphPoints))
                return new Glyph(
                    Utils.CloneArray(glyphPoints),
                    Utils.CloneArray(endPoints),
                    original.Bounds,
                    original.GlyphInstructions != null ? Utils.CloneArray(original.GlyphInstructions) : null,
                    newGlyphIndex);
            else if (original.CffInfo is { } data)
                return new Glyph(data);
            else if (original.BitmapSVGInfo is var (offset, len, format))
                return new Glyph(original.GlyphIndex, offset, len, format);
            else throw new NotImplementedException();
        }

        /// <summary>
        /// append data from src to dest, dest data will changed***
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        internal static void AppendGlyph(Glyph dest, Glyph src)
        {
            if (!(src.TtfWoffInfo is var (srcEndPoints, srcGlyphPoints)))
                throw new ArgumentException("Only TTF/WOFF glyphs are supported", nameof(src));
            if (!(dest.TtfWoffInfo is var (destEndPoints, destGlyphPoints)))
                throw new ArgumentException("Only TTF/WOFF glyphs are supported", nameof(src));
            int org_dest_len = destEndPoints.Length;
            int src_contour_count = srcEndPoints.Length;
            ushort org_last_point = (ushort)(destEndPoints[org_dest_len - 1] + 1); //since start at 0 

            destEndPoints = Utils.ConcatArray(destEndPoints, srcEndPoints);
            destGlyphPoints = Utils.ConcatArray(destGlyphPoints, srcGlyphPoints);
            dest.TtfWoffInfo = (destEndPoints, destGlyphPoints);

            //offset latest append contour  end points
            int newlen = destEndPoints.Length;
            for (int i = org_dest_len; i < newlen; ++i)
            {
                destEndPoints[i] += org_last_point;
            }
            //calculate new bounds
            Bounds destBound = dest.Bounds;
            Bounds srcBound = src.Bounds;
            short newXmin = Math.Min(destBound.XMin, srcBound.XMin);
            short newYMin = Math.Min(destBound.YMin, srcBound.YMin);
            short newXMax = Math.Max(destBound.XMax, srcBound.XMax);
            short newYMax = Math.Max(destBound.YMax, srcBound.YMax);

            dest.Bounds = new Bounds(newXmin, newYMin, newXMax, newYMax);
        }

        //
        public GlyphClassKind GlyphClass { get; set; }
        internal ushort MarkClassDef { get; set; }
        public short MinX => Bounds.XMin;
        public short MaxX => Bounds.XMax;
        public short MinY => Bounds.YMin;
        public short MaxY => Bounds.YMax;


#if DEBUG
        public readonly int dbugId;
        static int s_debugTotalId;
#endif

        public ushort GlyphIndex { get; }

#if DEBUG
        public override string ToString()
        {
            var stbuilder = new StringBuilder();
            if (CffInfo is { } cff)
            {
                stbuilder.Append("cff");
                stbuilder.Append(",index=" + GlyphIndex);
                stbuilder.Append(",name=" + cff.Name);
            }
            else if (BitmapSVGInfo is { } bitmapSvg)
            {
                stbuilder.Append("bitmapsvg");
                stbuilder.Append(",index=" + GlyphIndex);
                stbuilder.Append(",offset=" + bitmapSvg.streamOffset);
                stbuilder.Append(",len=" + bitmapSvg.streamLen);
                stbuilder.Append(",format=" + bitmapSvg.imgFormat);
            }
            else
            {
                stbuilder.Append("ttfwoff");
                stbuilder.Append(",index=" + GlyphIndex);
                stbuilder.Append(",class=" + GlyphClass.ToString());
                if (MarkClassDef != 0)
                {
                    stbuilder.Append(",mark_class=" + MarkClassDef);
                }
            }
            return stbuilder.ToString();
        }
#endif 

        //--------------------
        //cff

        public CFF.Cff1GlyphData? CffInfo { get; }
        internal Glyph(CFF.Cff1GlyphData cff1Glyph)
        {
#if DEBUG
            this.dbugId = s_debugTotalId++;
#endif

            //create from CFF 
            CffInfo = cff1Glyph;
            this.GlyphIndex = cff1Glyph.GlyphIndex;
        }
        //math glyph info, temp , TODO: review here again
        public MathGlyphs.MathGlyphInfo? MathGlyphInfo { get; internal set; }
        //--------------------
        //Bitmap and Svg

        internal (uint streamOffset, uint streamLen, ushort imgFormat)? BitmapSVGInfo { get; private set; }
        internal Glyph(ushort glyphIndex, uint streamOffset, uint streamLen, ushort imgFormat)
        {
            //_bmpGlyphSource = bmpGlyphSource;
            BitmapSVGInfo = (streamOffset, streamLen, imgFormat);
            this.GlyphIndex = glyphIndex;
        }

        //public void CopyBitmapContent(System.IO.Stream output)
        //{
        //    _bmpGlyphSource.CopyBitmapContent(this, output);
        //}
    }


    //https://www.microsoft.com/typography/otspec/gdef.htm
    public enum GlyphClassKind : byte
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
