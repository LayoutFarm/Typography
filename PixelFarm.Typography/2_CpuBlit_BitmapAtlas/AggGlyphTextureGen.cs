//MIT, 2016-present, WinterDev
//-----------------------------------  
using System;
using PixelFarm.CpuBlit.VertexProcessing;

using PixelFarm.Drawing;
using Typography.OpenFont.Contours;

namespace PixelFarm.CpuBlit.BitmapAtlas
{
    /// <summary>
    /// agg glyph texture generator
    /// </summary>
    public class AggGlyphTextureGen
    {
        GlyphTranslatorToVxs _txToVxs = new GlyphTranslatorToVxs();

        public AggGlyphTextureGen()
        {
            BackGroundColor = Color.Transparent;
            GlyphColor = Color.Black;
        }

        public Color BackGroundColor { get; set; }
        public Color GlyphColor { get; set; }
        public TextureKind TextureKind { get; set; }
        public AggPainter Painter { get; set; }


        public BitmapAtlasItemSource CreateAtlasItem(GlyphOutlineBuilder builder, float pxscale)
        {

            _txToVxs.Reset();
            //1. builder read shape and translate it with _txToVxs
            builder.ReadShapes(_txToVxs);

            using (Tools.BorrowVxs(out var glyphVxs, out var vxs2))
            {
                //2. write translated data (in the _txToVxs) to glyphVxs

                _txToVxs.WriteOutput(glyphVxs, pxscale);

                Q1RectD bounds = glyphVxs.GetBoundingRect();

                //-------------------------------------------- 
                int w = (int)System.Math.Ceiling(bounds.Width);
                int h = (int)System.Math.Ceiling(bounds.Height);
                if (w < 5)
                {
                    w = 5;
                }
                if (h < 5)
                {
                    h = 5;
                }
                //we need some margin
                int horizontal_margin = 1;
                int vertical_margin = 1;

                //translate to positive quadrant and use minimum space

                int dx = (int)Math.Ceiling((bounds.Left < 0) ? -bounds.Left : 0);
                int dy = 0;

                //vertical adjust =>since we need to move it, then move it with integer value
                if (bounds.Bottom < 0)
                {
                    dy = (int)Math.Ceiling(-bounds.Bottom);
                }
                else if (bounds.Bottom > 0)
                {
                    dy = (int)Math.Floor(-bounds.Bottom);
                }
                dx += horizontal_margin; //margin left
                dy += vertical_margin;
                //--------------------------------------------  
                w = dx + w + horizontal_margin; //+right margin

                h = vertical_margin + h + vertical_margin; //+bottom margin  
                AggPainter painter = Painter;
                if (TextureKind == TextureKind.StencilLcdEffect)
                {

                    glyphVxs.TranslateToNewVxs(dx + 0.33f, dy, vxs2); //offset to proper x of subpixel rendering  ***
                    glyphVxs = vxs2;

                    Q1RectD bounds2 = vxs2.GetBoundingRect();
                    if (w < bounds2.Right)
                    {
                        w = (int)Math.Ceiling(bounds2.Right);
                    }
                    // 
                    painter.UseLcdEffectSubPixelRendering = true;
                    //we use white glyph on black bg for this texture                


                    painter.Clear(Color.Black);
                    painter.FillColor = Color.White;


                    //painter.Clear(Color.FromArgb(0, 255, 255, 255)); //white -transparent
                    //painter.FillColor = Color.Black;


                    //painter.Clear(Color.White);
                    //painter.FillColor = Color.Black;

                    //painter.Clear(Color.Empty);
                    //painter.FillColor = Color.Black;

                    painter.Fill(glyphVxs);

                    //apply sharpen filter
                    //painter.DoFilter(new RectInt(0, h, w, 0), 2);
                    //painter.DoFilter(new RectInt(0, h, w, 0), 2); //? 
                }
                else
                {

                    glyphVxs.TranslateToNewVxs(dx, dy, vxs2);
                    glyphVxs = vxs2;

                    painter.UseLcdEffectSubPixelRendering = false;

                    if (TextureKind == TextureKind.StencilGreyScale)
                    {
                        painter.Clear(Color.Empty);
                        painter.FillColor = Color.Black;
                    } 
                    else
                    {
                        painter.Clear(BackGroundColor);
                        painter.FillColor = this.GlyphColor;
                    }
                    painter.Fill(glyphVxs);

                }
                //


                if (w > painter.RenderSurface.DestBitmap.Width)
                {
                    w = painter.RenderSurface.DestBitmap.Width;
                }
                if (h > painter.RenderSurface.DestBitmap.Height)
                {
                    h = painter.RenderSurface.DestBitmap.Height;
                }

                var glyphImage = new BitmapAtlasItemSource(w, h);

#if DEBUG
                if (dx < short.MinValue || dx > short.MaxValue)
                {
                    throw new NotSupportedException();
                }
                if (dy < short.MinValue || dy > short.MaxValue)
                {
                    throw new NotSupportedException();
                }
#endif

                glyphImage.TextureXOffset = (short)dx;
                glyphImage.TextureYOffset = (short)dy;

                glyphImage.SetImageBuffer(MemBitmapExt.CopyImgBuffer(painter.RenderSurface.DestBitmap, w, h), false);
                //copy data from agg canvas to glyph image 
                return glyphImage;
            }

        }
    }

}