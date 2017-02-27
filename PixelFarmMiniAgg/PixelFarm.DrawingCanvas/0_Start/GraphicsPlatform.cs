//BSD, 2014-2017, WinterDev

namespace PixelFarm.Drawing
{

    public abstract class GraphicsPlatform
    { 
        public abstract Canvas CreateCanvas(
            int left,
            int top,
            int width,
            int height,
            CanvasInitParameters canvasInitPars = new CanvasInitParameters());
        //----------------------------------------------------------------------
        //set provider delegates before use it from comment graphics platform
        //---------------------------------------------------------------------- 
        //----------------------
        //2. image buffer provider from filename
        static ImageBufferProviderDelegate s_imgBufferProviderDel;
        public static void SetImageBufferProviderDelegate(ImageBufferProviderDelegate imgBufferProviderDel)
        {
            s_imgBufferProviderDel = imgBufferProviderDel;
        }
    }



    public delegate byte[] ImageBufferProviderDelegate(string filename);

    public struct CanvasInitParameters
    {
        public object externalCanvas;
        public CanvasBackEnd canvasBackEnd;

        internal bool IsEmpty()
        {
            return externalCanvas == null && canvasBackEnd == CanvasBackEnd.Software;
        }
    }


}