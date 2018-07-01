//Apache2, 2017-present, WinterDev
//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.Text;
namespace Typography.OpenFont
{

    public class Glyph
    {
        //--------------------
        //ttf
        GlyphPointF[] glyphPoints;
        ushort[] _contourEndPoints;

        ushort _orgAdvWidth;
        bool _hasOrgAdvWidth;

        Bounds _bounds;

        internal Glyph(
            GlyphPointF[] glyphPoints,
            ushort[] contourEndPoints,
            Bounds bounds,
            byte[] glyphInstructions,
            ushort index)
        {
            //create from TTF 

#if DEBUG
            this.dbugId = s_debugTotalId++;
#endif
            this.glyphPoints = glyphPoints;
            _contourEndPoints = contourEndPoints;
            Bounds = bounds;
            GlyphInstructions = glyphInstructions;
            GlyphIndex = index;
        }
        public Bounds Bounds
        {
            get { return _bounds; }
            internal set { _bounds = value; }
        }

        public ushort[] EndPoints { get { return _contourEndPoints; } }
        public GlyphPointF[] GlyphPoints { get { return glyphPoints; } }



        public ushort OriginalAdvanceWidth
        {
            get { return _orgAdvWidth; }
            set
            {
                _orgAdvWidth = value;
                _hasOrgAdvWidth = true;
            }
        }
        public bool HasOriginalAdvancedWidth { get { return _hasOrgAdvWidth; } }
        //--------------



        internal static void OffsetXY(Glyph glyph, short dx, short dy)
        {

            //change data on current glyph
            GlyphPointF[] glyphPoints = glyph.glyphPoints;
            for (int i = glyphPoints.Length - 1; i >= 0; --i)
            {
                glyphPoints[i] = glyphPoints[i].Offset(dx, dy);
            }
            //-------------------------
            Bounds orgBounds = glyph._bounds;
            glyph._bounds = new Bounds(
               (short)(orgBounds.XMin + dx),
               (short)(orgBounds.YMin + dy),
               (short)(orgBounds.XMax + dx),
               (short)(orgBounds.YMax + dy));

        }
        internal byte[] GlyphInstructions { get; set; }

        public bool HasGlyphInstructions { get { return this.GlyphInstructions != null; } }

        internal static void TransformNormalWith2x2Matrix(Glyph glyph, float m00, float m01, float m10, float m11)
        {

            //http://stackoverflow.com/questions/13188156/whats-the-different-between-vector2-transform-and-vector2-transformnormal-i
            //http://www.technologicalutopia.com/sourcecode/xnageometry/vector2.cs.htm

            //change data on current glyph
            float new_xmin = 0;
            float new_ymin = 0;
            float new_xmax = 0;
            float new_ymax = 0;


            GlyphPointF[] glyphPoints = glyph.glyphPoints;
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
            glyph._bounds = new Bounds(
               (short)new_xmin, (short)new_ymin,
               (short)new_xmax, (short)new_ymax); 
        }

        internal static Glyph Clone(Glyph original, ushort newGlyphIndex)
        {
            return new Glyph(
                Utils.CloneArray(original.glyphPoints),
                Utils.CloneArray(original._contourEndPoints),
                original.Bounds,
                original.GlyphInstructions != null ? Utils.CloneArray(original.GlyphInstructions) : null,
                newGlyphIndex);
        }

        /// <summary>
        /// append data from src to dest, dest data will changed***
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        internal static void AppendGlyph(Glyph dest, Glyph src)
        {
            int org_dest_len = dest._contourEndPoints.Length;
            int src_contour_count = src._contourEndPoints.Length;
            ushort org_last_point = (ushort)(dest._contourEndPoints[org_dest_len - 1] + 1); //since start at 0 

            dest.glyphPoints = Utils.ConcatArray(dest.glyphPoints, src.glyphPoints);
            dest._contourEndPoints = Utils.ConcatArray(dest._contourEndPoints, src._contourEndPoints);

            //offset latest append contour  end points
            int newlen = dest._contourEndPoints.Length;
            for (int i = org_dest_len; i < newlen; ++i)
            {
                dest._contourEndPoints[i] += (ushort)org_last_point;
            }
            //calculate new bounds
            Bounds destBound = dest.Bounds;
            Bounds srcBound = src.Bounds;
            short newXmin = (short)Math.Min(destBound.XMin, srcBound.XMin);
            short newYMin = (short)Math.Min(destBound.YMin, srcBound.YMin);
            short newXMax = (short)Math.Max(destBound.XMax, srcBound.XMax);
            short newYMax = (short)Math.Max(destBound.YMax, srcBound.YMax);

            dest._bounds = new Bounds(newXmin, newYMin, newXMax, newYMax);
        }


        public GlyphClassKind GlyphClass { get; set; }
        internal ushort MarkClassDef { get; set; }
        public short MinX
        {
            get { return _bounds.XMin; }
        }
        public short MaxX
        {
            get { return _bounds.XMax; }
        }
        public short MinY
        {
            get { return _bounds.YMin; }
        }
        public short MaxY
        {
            get { return _bounds.YMax; }
        }

        //--------------------
        //both ttf and cff
        public static readonly Glyph Empty = new Glyph(new GlyphPointF[0], new ushort[0], Bounds.Zero, null, 0);

#if DEBUG
        public readonly int dbugId;
        static int s_debugTotalId;
#endif

        public ushort GlyphIndex { get; }

#if DEBUG
        public override string ToString()
        {
            var stbuilder = new StringBuilder();
            if (IsCffGlyph)
            {
                stbuilder.Append("cff");
                stbuilder.Append(",index=" + GlyphIndex);
                stbuilder.Append(",name=" + _cff1GlyphData.Name);
            }
            else
            {
                stbuilder.Append("ttf");
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

        internal CFF.Cff1Font _ownerCffFont;
        internal CFF.Cff1GlyphData _cff1GlyphData; //temp
        internal Glyph(CFF.Cff1Font owner, CFF.Cff1GlyphData cff1Glyph)
        {
#if DEBUG
            this.dbugId = s_debugTotalId++;
#endif

            this._ownerCffFont = owner;
            //create from CFF 
            this._cff1GlyphData = cff1Glyph;
            this.GlyphIndex = cff1Glyph.GlyphIndex;

        }
        public bool IsCffGlyph
        {
            get
            {
                return _ownerCffFont != null;
            }
        }
        public CFF.Cff1Font GetOwnerCff()
        {
            //temp 
            return _ownerCffFont;
        }
        public CFF.Cff1GlyphData GetCff1GlyphData()
        {
            return _cff1GlyphData;
        }
        //math glyph info, temp , TODO: review here again
        public MathGlyphs.MathGlyphInfo MathGlyphInfo { get; internal set; }
        public bool HasMathGlyphInfo { get; internal set; }
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
