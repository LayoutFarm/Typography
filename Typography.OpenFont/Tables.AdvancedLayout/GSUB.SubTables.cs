//Apache2, 2016-2017,  WinterDev


namespace Typography.OpenFont.Tables
{
    /// <summary>
    /// replaceable glyph index list
    /// </summary>
    public interface IGlyphIndexList
    {
        void Clear();
        /// <summary>
        /// add original char and its glyph index
        /// </summary>
        /// <param name="glyphIndex"></param>
        void AddGlyph(int originalChar, ushort glyphIndex);
        int Count { get; }
        ushort this[int index] { get; }

        /// <summary>
        /// remove:add_new 1:1
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newGlyphIndex"></param>
        void Replace(int index, ushort newGlyphIndex);
        /// <summary>
        /// remove:add_new >=1:1
        /// </summary>
        /// <param name="index"></param>
        /// <param name="removeLen"></param>
        /// <param name="newGlyhIndex"></param>
        void Replace(int index, int removeLen, ushort newGlyhIndex);
        /// <summary>
        /// remove: add_new 1:>=1
        /// </summary>
        /// <param name="index"></param>
        /// <param name="removeLen"></param>
        /// <param name="newGlyhIndex"></param>
        void Replace(int index, ushort[] newGlyhIndices);
    }

    partial class GSUB : TableEntry
    {
        /// <summary>
        /// base class of lookup sub table
        /// </summary>
        public abstract class LookupSubTable
        {
            public abstract void DoSubtitution(IGlyphIndexList glyphIndices, int startAt, int len);
            public GSUB OwnerGSub;
        }
    }
}
