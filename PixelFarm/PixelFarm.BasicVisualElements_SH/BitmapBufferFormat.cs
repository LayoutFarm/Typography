//BSD, 2014-present, WinterDev
//MIT, 2018-present, WinterDev
using System;
namespace PixelFarm.Drawing
{
    public enum BitmapBufferFormat
    {
        BGRA, //eg. System.Drawing.Bitmap
        BGR, //eg. Native Windows GDI surface
        RGBA, //eg. OpenGL 

        RGBO, //my extension, 32 bits RGB ignore Alpha, assume its value= 1
    }
}