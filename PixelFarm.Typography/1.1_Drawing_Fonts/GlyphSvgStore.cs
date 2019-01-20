//MIT, 2019-present, WinterDev 
using System;
using System.Collections.Generic;
using Typography.OpenFont;
using PixelFarm.CpuBlit;
using PaintLab.Svg;
using LayoutFarm.WebLexer;
namespace PixelFarm.Drawing.Fonts
{

    class GlyphSvgStore
    {

        Typeface _currentTypeface;
        GlyphBitmapList _bitmapList;
        Dictionary<Typeface, GlyphBitmapList> _cacheGlyphPathBuilders = new Dictionary<Typeface, GlyphBitmapList>();

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
            _bitmapList = new GlyphBitmapList();
            _cacheGlyphPathBuilders.Add(typeface, _bitmapList);

            int glyphCount = typeface.GlyphCount;
            VgVisualDocHost vgDocHost = new VgVisualDocHost();
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                for (ushort i = 0; i < glyphCount; ++i)
                {
                    ms.SetLength(0);
                    Glyph glyph = typeface.GetGlyphByIndex(i);
                    //create bitmap from svg 



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

        void ParseAndRenderSvg(string svgContent, VgVisualDocHost vgDocHost)
        {
            //----------
            //copy from HtmlRenderer's SvgViewer demo
            //----------  
            var docBuilder = new SvgDocBuilder();
            var parser = new SvgParser(docBuilder);

            TextSnapshot textSnapshot = new TextSnapshot(svgContent);
            parser.ParseDocument(textSnapshot);
            VgVisualDocBuilder builder = new VgVisualDocBuilder();
            VgVisualElement vgVisElem = builder.CreateVgVisualDoc(docBuilder.ResultDocument, vgDocHost).VgRootElem;
        }


    }

}