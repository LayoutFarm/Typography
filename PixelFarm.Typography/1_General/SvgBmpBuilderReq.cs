//MIT, 2020-present, WinterDev
using System;
using PixelFarm.CpuBlit;
namespace PixelFarm.Drawing
{
    public class SvgBmpBuilderReq
    {
        //input
        public System.Text.StringBuilder SvgContent;
        public float ExpectedWidth;
        public Color DefaultBgColor = Color.White;
        //output 
        public MemBitmap Output;
        public int BitmapXOffset;
        public int BitmapYOffset;
    }

    public delegate void SvgBmpBuilderFunc(SvgBmpBuilderReq req);
}