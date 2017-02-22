//MIT, 2016-2017, WinterDev
//-----------------------------------  
using System;
using Typography.OpenFont;

namespace Typography.Rendering
{
    public static class MsdfGlyphGenExtension
    {
        public static GlyphImage2 CreateMsdfImage(
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

            Msdfgen.FloatRGBBmp frgbBmp = new Msdfgen.FloatRGBBmp(w, h);
            Msdfgen.EdgeColoring.edgeColoringSimple(shape, 3);
            Msdfgen.MsdfGenerator.generateMSDF(frgbBmp, shape, 4, new Msdfgen.Vector2(1, 1), new Msdfgen.Vector2(), -1);
            //-----------------------------------
            int[] buffer = Msdfgen.MsdfGenerator.ConvertToIntBmp(frgbBmp);
            GlyphImage2 img = new Typography.Rendering.GlyphImage2(w, h);
            img.SetImageBuffer(buffer, false);
            return img;
        }



    }
}