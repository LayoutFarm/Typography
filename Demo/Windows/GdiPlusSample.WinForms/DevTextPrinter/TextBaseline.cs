//MIT, 2016-present, WinterDev
using System.Collections.Generic;
using Typography.OpenFont;
using Typography.TextLayout;

namespace PixelFarm.Drawing
{
    public enum TextBaseline
    {
        //top" || "hanging" || "middle" || "alphabetic" || "ideographic" || "bottom";
        //https://developer.mozilla.org/en-US/docs/Web/API/CanvasRenderingContext2D/textBaseline
        Alphabetic,//Html5 default

        Top,
        Hanging, //not implemented
        Middle,//not implemented
        Ideographics,//not implemented
        Bottom,
    }
}