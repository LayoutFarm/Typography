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
            if (_dic != null)
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
                _dic = null;
            }
        }


    }
    
    /// <summary>
    /// typeface-specific, glyph bitmap store
    /// </summary>
    class TypefaceGlyphBitmapCache : IDisposable
    {

        GlyphBitmapList _bitmapList;
        Dictionary<float, GlyphBitmapList> _specificFontSizeGlyphBitmaps;//sometime we may want to cache multiple size of glyph
        float _size;

        public TypefaceGlyphBitmapCache(Typeface typeface)
        {
            Typeface = typeface;
        }

        /// <summary>
        /// actual rounding cache font size
        /// </summary>
        public float ActualCacheSize => _size;
        /// <summary>
        /// set font size in point unit, this value may be rounded
        /// </summary>
        /// <param name="value"></param>
        public void SetFontSize(float value)
        {
            //check if we have size-specific bmp cache or not 
            float actualCacheSize = (float)Math.Round(value, 1);
            if (_size == actualCacheSize) { return; }

            //
            _size = actualCacheSize;

            if (_specificFontSizeGlyphBitmaps == null) { _specificFontSizeGlyphBitmaps = new Dictionary<float, GlyphBitmapList>(); }

            if (!_specificFontSizeGlyphBitmaps.TryGetValue(actualCacheSize, out _bitmapList))
            {
                //not found=> create a new one
                _bitmapList = new GlyphBitmapList();


                _specificFontSizeGlyphBitmaps.Add(actualCacheSize, _bitmapList);
            }
        }

        public Typeface Typeface { get; }

        public bool TryGetBitmap(ushort glyphIndex, out GlyphBitmap glyphBmp) => _bitmapList.TryGetBitmap(glyphIndex, out glyphBmp);
        public void RegisterBitmap(ushort glyphIndex, GlyphBitmap bmp) => _bitmapList.RegisterBitmap(glyphIndex, bmp);

        public void Dispose()
        {
            Clear();
            _specificFontSizeGlyphBitmaps = null;
        }
        public void Clear()
        {
            if (_specificFontSizeGlyphBitmaps != null)
            {
                foreach (GlyphBitmapList bmplist in _specificFontSizeGlyphBitmaps.Values)
                {
                    bmplist.Dispose();
                }
                _specificFontSizeGlyphBitmaps.Clear();
            }
        }
    }

    class GlyphBitmapStore : IDisposable
    {
        Typeface _currentTypeface;
        float _currentSizeInPts;//current size in point unit

        TypefaceGlyphBitmapCache _bmpCache;//current
        Dictionary<Typeface, TypefaceGlyphBitmapCache> _totalBmpCache = new Dictionary<Typeface, TypefaceGlyphBitmapCache>();

        public void SetCurrentTypeface(Typeface typeface, float sizeInPts)
        {
            if (_currentTypeface == typeface && _currentSizeInPts == sizeInPts)
            {
                return;
            }

            _currentSizeInPts = sizeInPts;
            _currentTypeface = typeface;

            if (_totalBmpCache.TryGetValue(typeface, out _bmpCache))
            {
                _bmpCache.SetFontSize(_currentSizeInPts);
                return;
            }

            //TODO: you can scale down to proper img size
            //or create a single texture atlas. 

            //if not create a new one
            _bmpCache = new TypefaceGlyphBitmapCache(typeface);
            _bmpCache.SetFontSize(_currentSizeInPts);
            return;

            //if (!delayCreateBmp)
            //{
            //    int glyphCount = typeface.GlyphCount;
            //    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            //    {
            //        for (ushort i = 0; i < glyphCount; ++i)
            //        {
            //            ms.SetLength(0);

            //            Glyph glyph = typeface.GetGlyph(i);
            //            typeface.ReadBitmapContent(glyph, ms);

            //            GlyphBitmap glyphBitmap = new GlyphBitmap();
            //            glyphBitmap.Width = glyph.MaxX - glyph.MinX;
            //            glyphBitmap.Height = glyph.MaxY - glyph.MinY;

            //            //glyphBitmap.Bitmap = ...                     
            //            glyphBitmap.Bitmap = MemBitmap.LoadBitmap(ms);
            //            //MemBitmapExtensions.SaveImage(glyphBitmap.Bitmap, "testGlyphBmp_" + i + ".png");

            //            //_bitmapList.RegisterBitmap(glyph.GlyphIndex, glyphBitmap);
            //            _bitmapList.RegisterBitmap(i, glyphBitmap);
            //        }
            //    }
            //}
        }
        public TypefaceGlyphBitmapCache CurrrentBitmapCache => _bmpCache;


        public void Clear(Typeface typeface)
        {
            if (_totalBmpCache.TryGetValue(typeface, out TypefaceGlyphBitmapCache found))
            {
                found.Clear();
                _totalBmpCache.Remove(typeface);
            }

            if (_currentTypeface == typeface)
            {
                _currentTypeface = null;
                _bmpCache = null;
            }

        }
        /// <summary>
        /// clear all cache bitmap
        /// </summary>
        public void Clear()
        {
            if (_currentTypeface == null)
            {
                return;
            }

            _currentTypeface = null;

            foreach (TypefaceGlyphBitmapCache bmplist in _totalBmpCache.Values)
            {
                bmplist.Dispose();
            }
            _totalBmpCache.Clear();
            //
            _bmpCache?.Dispose();
            _bmpCache = null;
        }
        public void Dispose()
        {
            Clear();
        }
    }

}