//MIT, 2014-present, WinterDev

using System;
using PixelFarm.CpuBlit;
using System.Collections.Generic;
namespace PixelFarm.Drawing.Fonts
{
    /// <summary>
    /// provide information about a glyph
    /// </summary>
    public class FontGlyph
    {
        //metrics
        public int horiz_adv_x;
        public string glyphName;
        public int unicode;
        /// <summary>
        /// code point/glyph index?
        /// </summary>
        public int codePoint;

        public GlyphMatrix glyphMatrix;
        /// <summary>
        /// 32 bpp image for render
        /// </summary>
        public ActualBitmap glyphImage32;
        //----------------------------
        /// <summary>
        /// original glyph outline
        /// </summary>
        public VertexStore originalVxs;
        /// <summary>
        /// flaten version of original glyph outline
        /// </summary>
        public VertexStore flattenVxs;
        //----------------------------
    }
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
        public abstract FontGlyph GetGlyphByIndex(ushort glyphIndex);
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
            //throw new NotSupportedException();
            //return RequestFont.GetCacheActualFont(r);
            return CacheFont.GetCacheActualFont(r);
        }
        protected static void SetCacheActualFont(RequestFont r, ActualFont a)
        {
            CacheFont.SetCacheActualFont(r, a);
            //throw new NotSupportedException();
            //RequestFont.SetCacheActualFont(r, a);
        }
    }

    static class CacheFont
    {
        static Dictionary<int, ActualFont> s_actualFonts = new Dictionary<int, ActualFont>();
        public static ActualFont GetCacheActualFont(RequestFont r)
        {
            ActualFont font;
            s_actualFonts.TryGetValue(r.FontKey, out font);
            return font;
        }
        public static void SetCacheActualFont(RequestFont r, ActualFont a)
        {
            s_actualFonts[r.FontKey] = a;
            //throw new NotSupportedException();
            //RequestFont.SetCacheActualFont(r, a);
        }
    }


}