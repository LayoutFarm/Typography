//MIT, 2014-2016, WinterDev

using System;
namespace PixelFarm.Drawing.Fonts
{

    /// <summary>
    /// specific fontface + size + style
    /// </summary>
    public abstract class ActualFont : IDisposable
    {

        public abstract float SizeInPoints { get; }
        public abstract float SizeInPixels { get; }
        public void Dispose()
        {
            OnDispose();
        }
#if DEBUG
        static int dbugTotalId = 0;
        public readonly int dbugId = dbugTotalId++;
        public ActualFont()
        {

        }
#endif
        protected abstract void OnDispose();
        //---------------------
        public abstract FontGlyph GetGlyphByIndex(uint glyphIndex);
        public abstract FontGlyph GetGlyph(char c);
        public abstract FontFace FontFace { get; }
        public abstract FontStyle FontStyle { get; }
        public abstract string FontName { get; }


        public abstract float AscentInPixels { get; }
        public abstract float DescentInPixels { get; }
        public abstract float LineGapInPixels { get; }
        public abstract float RecommendedLineSpacingInPixels { get; }
        ~ActualFont()
        {
            Dispose();
        }
        //---------------------

        protected static ActualFont GetCacheActualFont(RequestFont r)
        {
            return RequestFont.GetCacheActualFont(r);
        }
        protected static void SetCacheActualFont(RequestFont r, ActualFont a)
        {
            RequestFont.SetCacheActualFont(r, a);
        }
    }



}