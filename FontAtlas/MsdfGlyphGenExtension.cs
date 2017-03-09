//MIT, 2016-2017, WinterDev
//-----------------------------------  
using System;
using Typography.OpenFont;

namespace Typography.Rendering
{
    public static class MsdfGlyphGenExtension
    {
        public static GlyphImage CreateMsdfImage(
            this MsdfGlyphGen msdfGlyphGen,
            GlyphPointF[] glyphPoints, ushort[] contourEndPoints, float pxScale = 1)
        {
            // create msdf shape , then convert to actual image
            Msdfgen.Shape shape = msdfGlyphGen.CreateMsdfShape(glyphPoints, contourEndPoints, pxScale);
            double left, bottom, right, top;
            shape.findBounds(out left, out bottom, out right, out top);
            int w = (int)Math.Ceiling((right - left));
            int h = (int)Math.Ceiling((top - bottom));
            if (w < 5)
            {
                w = 5;
            }
            if (h < 5)
            {
                h = 5;
            }


            int borderW = (int)((float)w / 5f);
            var translate = new Msdfgen.Vector2(left < 0 ? -left + borderW : borderW, bottom < 0 ? -bottom + borderW : borderW);
            w += borderW * 2; //borders,left- right
            h += borderW * 2; //borders, top- bottom



            Msdfgen.FloatRGBBmp frgbBmp = new Msdfgen.FloatRGBBmp(w, h);
            Msdfgen.EdgeColoring.edgeColoringSimple(shape, 3);


            Msdfgen.MsdfGenerator.generateMSDF(frgbBmp,
                shape,
                4,
                new Msdfgen.Vector2(1, 1), //scale                 
                translate,//translate to positive quadrant
                -1);
            //-----------------------------------
            int[] buffer = Msdfgen.MsdfGenerator.ConvertToIntBmp(frgbBmp);

            GlyphImage img = new GlyphImage(w, h);
            img.TextureOffsetX = translate.x;
            img.TextureOffsetY = translate.y;
            img.SetImageBuffer(buffer, false);
            return img;
        }
    }
}