//MIT, 2016-2017, WinterDev
//-----------------------------------  
using System;
using PixelFarm.CpuBlit.VertexProcessing;

using PixelFarm.Drawing;
using PixelFarm.CpuBlit;
using PixelFarm.Drawing.Fonts;

using Typography.Rendering;
namespace Typography.Contours
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

        public GlyphImage CreateGlyphImage(GlyphPathBuilder builder, float pxscale)
        {

            _txToVxs.Reset();
            builder.ReadShapes(_txToVxs);
            //create new one
            using (VxsTemp.Borrow(out var glyphVxs, out var vxs2))
            {
                _txToVxs.WriteOutput(glyphVxs, pxscale);
                //find bound
                //-------------------------------------------- 

                RectD bounds = glyphVxs.GetBoundingRect();
                ////-------------------------------------------- 
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

                //translate to positive quadrant and use minimum space

                double dx = -bounds.Left;
                double dy = -bounds.Bottom;


                dx = Math.Ceiling(dx); //since we need to move it, then move it with integer value
                dy = Math.Ceiling(dy); //since we need to move it, then move it with integer value

                //we need some borders
                int horizontal_margin = 1; //'margin' 1px
                int vertical_margin = 1; //margin 1 px

                dx += horizontal_margin; //+ left margin
                dy += vertical_margin; //+ top margin 

                //--------------------------------------------  

                w += horizontal_margin;
                h += vertical_margin;

                //create glyph img    
                using (MemBitmap memBmp = new MemBitmap(w, h))
                {

#if DEBUG
                    memBmp._dbugNote = "CreateGlyphImage()";
#endif
                    //TODO: review painter here
                    //
                    AggPainter painter = AggPainter.Create(memBmp);
                    if (TextureKind == TextureKind.StencilLcdEffect)
                    {

                        glyphVxs.TranslateToNewVxs(dx + 0.33f, dy, vxs2); //offset to proper x of subpixel rendering  ***
                        glyphVxs = vxs2;
                        // 
                        painter.UseSubPixelLcdEffect = true;

                        //we use white glyph on black bg for this texture                
                        painter.Clear(Color.Black);
                        painter.FillColor = Color.White;
                        painter.Fill(glyphVxs);

                        //apply sharpen filter
                        //painter.DoFilter(new RectInt(0, h, w, 0), 2);
                        //painter.DoFilter(new RectInt(0, h, w, 0), 2); //? 
                    }
                    else
                    {

                        glyphVxs.TranslateToNewVxs(dx, dy, vxs2);
                        glyphVxs = vxs2;

                        painter.UseSubPixelLcdEffect = false;

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
                    var glyphImage = new GlyphImage(w, h);
                    glyphImage.TextureOffsetX = dx;
                    glyphImage.TextureOffsetY = dy;
                    glyphImage.SetImageBuffer(MemBitmapExtensions.CopyImgBuffer(memBmp, w), false);
                    //copy data from agg canvas to glyph image 
                    return glyphImage;
                }
            }

        }
    }

}