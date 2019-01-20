//MIT, 2019-present, WinterDev 
using System;
using System.Collections.Generic;
using System.IO;
using Typography.OpenFont;
using PixelFarm.CpuBlit;

namespace PixelFarm.Drawing.Fonts
{

    class GlyphBitmap
    {
        public MemBitmap Bitmap { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int ImageStartX { get; set; } //in the case Bitmap is an Atlas,
        public int ImageStartY { get; set; } //in the case Bitmap is an Atlas,
    }
    class GlyphBitmapStore
    {
        class BitmapList : IDisposable
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

        Typeface _currentTypeface;
        BitmapList _bitmapList;
        Dictionary<Typeface, BitmapList> _cacheGlyphPathBuilders = new Dictionary<Typeface, BitmapList>();

        public void SetCurrentTypeface(Typeface typeface)
        {
            _currentTypeface = typeface;
            if (_cacheGlyphPathBuilders.TryGetValue(typeface, out _bitmapList))
            {
                return;
            }

            //TODO: you can scale down to proper img size
            //or create a single texture atlas.


            //if not create a new one
            _bitmapList = new BitmapList();
            _cacheGlyphPathBuilders.Add(typeface, _bitmapList);

            int glyphCount = typeface.GlyphCount;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                for (ushort i = 0; i < glyphCount; ++i)
                {
                    ms.SetLength(0);

                    Glyph glyph = typeface.GetGlyphByIndex(i);
                    glyph.CopyBitmapContent(ms);

                    GlyphBitmap glyphBitmap = new GlyphBitmap();
                    glyphBitmap.Width = glyph.MaxX - glyph.MinX;
                    glyphBitmap.Height = glyph.MaxY - glyph.MinY;

                    //glyphBitmap.Bitmap = ...                     
                    glyphBitmap.Bitmap = MemBitmap.LoadBitmap(ms);
                    //MemBitmapExtensions.SaveImage(glyphBitmap.Bitmap, "d:\\WImageTest\\testGlyphBmp_" + i + ".png");

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