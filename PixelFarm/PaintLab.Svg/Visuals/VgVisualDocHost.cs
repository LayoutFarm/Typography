//MIT, 2014-present, WinterDev

using System;
namespace PaintLab.Svg
{
    public class VgVisualDocHost
    {
        Action<VgVisualElement> _invalidate;
        Action<PixelFarm.Drawing.ImageBinder, VgVisualElement, object> _imgReqHandler;
        public void SetInvalidateDelegate(Action<VgVisualElement> invalidate)
        {
            _invalidate = invalidate;
        }
        public void SetImgRequestDelgate(Action<PixelFarm.Drawing.ImageBinder, VgVisualElement, object> imgRequestHandler)
        {
            _imgReqHandler = imgRequestHandler;
        }

        internal void RequestImageAsync(PixelFarm.Drawing.ImageBinder binder, VgVisualElement imgRun, object requestFrom)
        {
            if (_imgReqHandler != null)
            {
                _imgReqHandler(binder, imgRun, requestFrom);
            }
            else
            {
                //ask for coment resource IO
                _imgReqHandler = VgResourceIO.VgImgIOHandler;
                if (_imgReqHandler != null)
                {
                    _imgReqHandler(binder, imgRun, requestFrom);
                }
            }
        }
        internal void InvalidateGraphics(VgVisualElement e)
        {
            //***
            _invalidate?.Invoke(e);
        }
    }

}