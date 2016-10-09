//MIT, 2014-2016, WinterDev 
namespace PixelFarm.Drawing
{
    public abstract class GraphicsPlatform
    {

        public abstract Canvas CreateCanvas(
            int left,
            int top,
            int width,
            int height);
        public abstract Canvas CreateCanvas(
            object platformCanvas,
            int left,
            int top,
            int width,
            int height
         );
        public abstract GraphicsPath CreateGraphicsPath();
        /// <summary>
        /// font management system for this graphics platform
        /// </summary>
        public abstract IFonts Fonts { get; }
        public abstract Bitmap CreatePlatformBitmap(int w, int h, byte[] rawBuffer, bool isBottomUp);
    }
  
}