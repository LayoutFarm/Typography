//MIT, 2014-present, WinterDev 

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