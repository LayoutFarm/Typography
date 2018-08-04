//MIT, 2016-2017, WinterDev
//-----------------------------------  
using System;
using Typography.Contours;

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
            //1. create  

            var txToVxs = new GlyphTranslatorToVxs();
            builder.ReadShapes(txToVxs);
            //
            //create new one
            var glyphVxs = new VertexStore();
            txToVxs.WriteOutput(glyphVxs, pxscale);
            //find bound
            //-------------------------------------------- 
            //GlyphImage glyphImg = new GlyphImage()
            RectD bounds = RectD.ZeroIntersection;
            PixelFarm.CpuBlit.VertexProcessing.BoundingRect.GetBoundingRect(new VertexStoreSnap(glyphVxs), ref bounds);

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

            //translate to positive quadrant 
            //            
            double dx = (bounds.Left < 0) ? -bounds.Left : 0;
            double dy = (bounds.Bottom < 0) ? -bounds.Bottom : 0;

            //
            dx = Math.Ceiling(dx); //since we need to move it, then move it with integer value
            dy = Math.Ceiling(dy); //since we need to move it, then move it with integer value

            //we need some borders
            int horizontal_margin = 1; //'margin' 1px
            int vertical_margin = 1; //margin 1 px

            dx += horizontal_margin; //+ left margin
            dy += vertical_margin; //+ top margin 
                                   //--------------------------------------------  
                                   //create glyph img   
            w = (int)Math.Ceiling(dx + w + horizontal_margin); //+right margin
            h = (int)Math.Ceiling(dy + h + vertical_margin); //+bottom margin 

            ActualBitmap img = new ActualBitmap(w, h);
            AggPainter painter = AggPainter.Create(img);

            if (TextureKind == TextureKind.StencilLcdEffect)
            {
                VertexStore vxs2 = new VertexStore();
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
                VertexStore vxs2 = new VertexStore();
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
            glyphImage.SetImageBuffer(ActualBitmapExtensions.CopyImgBuffer(img, w), false);
            //copy data from agg canvas to glyph image 
            return glyphImage;

        }
    }

}