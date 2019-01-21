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
            System.Text.StringBuilder stbuilder = new System.Text.StringBuilder();
            for (ushort i = 0; i < glyphCount; ++i)
            {
                stbuilder.Length = 0;//reset
                Glyph glyph = typeface.GetGlyphByIndex(i);
                typeface.ReadSvgContent(glyph, stbuilder);
                //create bitmap from svg  

                GlyphBitmap glyphBitmap = new GlyphBitmap();
                glyphBitmap.Width = glyph.MaxX - glyph.MinX;
                glyphBitmap.Height = glyph.MaxY - glyph.MinY;

                if (glyphBitmap.Width == 0 || glyphBitmap.Height == 0)
                {
                    continue;
                }

                glyphBitmap.Bitmap = ParseAndRenderSvg(stbuilder, vgDocHost);
                //MemBitmapExtensions.SaveImage(glyphBitmap.Bitmap, "d:\\WImageTest\\testGlyphBmp_" + i + ".png");
                _bitmapList.RegisterBitmap(glyph.GlyphIndex, glyphBitmap);
            }

        }
        public GlyphBitmap GetGlyphBitmap(ushort glyphIndex)
        {
            _bitmapList.TryGetBitmap(glyphIndex, out GlyphBitmap found);
            return found;
        }

        MemBitmap ParseAndRenderSvg(System.Text.StringBuilder svgContent, VgVisualDocHost vgDocHost)
        {
            //----------
            //copy from HtmlRenderer's SvgViewer demo
            //----------  
            var docBuilder = new SvgDocBuilder();
            var parser = new SvgParser(docBuilder);
            TextSnapshot textSnapshot = new TextSnapshot(svgContent.ToString());
            parser.ParseDocument(textSnapshot);

            VgVisualDocBuilder builder = new VgVisualDocBuilder();
            VgVisualElement vgVisElem = builder.CreateVgVisualDoc(docBuilder.ResultDocument, vgDocHost).VgRootElem;
            RectD bounds = vgVisElem.GetRectBounds();
            float actualXOffset = (float)-bounds.Left;
            float actualYOffset = (float)-bounds.Bottom;

            int bmpW = (int)Math.Round(bounds.Width);
            int bmpH = (int)Math.Round(bounds.Height);

            MemBitmap memBitmap = new MemBitmap(bmpW, bmpH);
            using (AggPainterPool.Borrow(memBitmap, out AggPainter p))
            using (VgPainterArgsPool.Borrow(p, out VgPaintArgs paintArgs))
            {
                float orgX = p.OriginX;
                float orgY = p.OriginY;
                p.SetOrigin(actualXOffset, actualYOffset);

                p.Clear(Color.White);

                p.FillColor = Color.Black;

                double prevStrokeW = p.StrokeWidth;

                vgVisElem.Paint(paintArgs);

                p.StrokeWidth = prevStrokeW;//restore 

                p.SetOrigin(orgX, orgY);//restore
            }

            return memBitmap;
        }
    }

}