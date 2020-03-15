//MIT, 2019-present, WinterDev 
using System;
using System.Collections.Generic;
using Typography.OpenFont;
namespace PixelFarm.CpuBlit.BitmapAtlas
{

    class GlyphBitmap
    {
        public MemBitmap Bitmap { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int ImageStartX { get; set; } //in the case Bitmap is an Atlas,
        public int ImageStartY { get; set; } //in the case Bitmap is an Atlas,
    }
    class GlyphBitmapList : IDisposable
    {
        Dictionary<ushort, GlyphBitmap> _dic = new Dictionary<ushort, GlyphBitmap>();
        public void RegisterBitmap(ushort glyphIndex, GlyphBitmap bmp)
        {
            _dic.Add(glyphIndex, bmp);
        }
        public bool TryGetBitmap(ushort glyphIndex, out GlyphBitmap bmp)
        {
            return _dic.TryGetValue(glyphIndex, out bmp);
        }
        public void Dispose()
        {
            foreach (GlyphBitmap glyphBmp in _dic.Values)
            {
                if (glyphBmp.Bitmap != null)
                {
                    glyphBmp.Bitmap.Dispose();
                    glyphBmp.Bitmap = null;
                }
            }
            _dic.Clear();
        }
    }

    class GlyphBitmapStore
    {
        Typeface _currentTypeface;
        GlyphBitmapList _bitmapList;
        Dictionary<Typeface, GlyphBitmapList> _cachedBmpList = new Dictionary<Typeface, GlyphBitmapList>();

        public void SetCurrentTypeface(Typeface typeface)
        {
            _currentTypeface = typeface;
            if (_cachedBmpList.TryGetValue(typeface, out _bitmapList))
            {
                return;
            }

            //TODO: you can scale down to proper img size
            //or create a single texture atlas.


            //if not create a new one
            _bitmapList = new GlyphBitmapList();
            _cachedBmpList.Add(typeface, _bitmapList);

            int glyphCount = typeface.GlyphCount;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                for (ushort i = 0; i < glyphCount; ++i)
                {
                    ms.SetLength(0);

                    Glyph glyph = typeface.GetGlyph(i);
                    typeface.ReadBitmapContent(glyph, ms);                    

                    GlyphBitmap glyphBitmap = new GlyphBitmap();
                    glyphBitmap.Width = glyph.MaxX - glyph.MinX;
                    glyphBitmap.Height = glyph.MaxY - glyph.MinY;

                    //glyphBitmap.Bitmap = ...                     
                    glyphBitmap.Bitmap = MemBitmap.LoadBitmap(ms);
                    //MemBitmapExtensions.SaveImage(glyphBitmap.Bitmap, "testGlyphBmp_" + i + ".png");

                    _bitmapList.RegisterBitmap(glyph.GlyphIndex, glyphBitmap);
                }
            }
        }
        public GlyphBitmap GetGlyphBitmap(ushort glyphIndex)
        {
            _bitmapList.TryGetBitmap(glyphIndex, out GlyphBitmap found);
            return found;
        }
    }

}