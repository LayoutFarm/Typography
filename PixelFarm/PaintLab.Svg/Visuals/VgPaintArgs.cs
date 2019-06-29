//MIT, 2014-present, WinterDev

using System; 
using PixelFarm.Drawing; 
namespace PaintLab.Svg
{
    public class VgPaintArgs : VgVisitorBase
    {
        float _opacity;
        bool _maskMode;
        internal VgPaintArgs()
        {
            Opacity = 1;
        }
        public Painter P { get; internal set; }
        public Action<VertexStore, VgPaintArgs> PaintVisitHandler;
        public bool HasOpacity => _opacity < 1;
        public float Opacity
        {
            get => _opacity;
            set
            {
                if (value < 0)
                {
                    _opacity = 0;
                }
                else if (value > 1)
                {
                    _opacity = 1;//not use this opacity
                }
                else
                {
                    _opacity = value;
                }
            }
        }
        internal override void Reset()
        {
            base.Reset();//*** reset base class fields too
            //-------
            Opacity = 2;
            P = null;
            PaintVisitHandler = null;
        }
        public bool MaskMode
        {
            get => _maskMode;
            set
            {
                _maskMode = value;
            }
        }
    }

    public static class VgPaintArgsPool
    {
        public static PixelFarm.TempContext<VgPaintArgs> Borrow(Painter painter, out VgPaintArgs paintArgs)
        {
            if (!PixelFarm.Temp<VgPaintArgs>.IsInit())
            {
                PixelFarm.Temp<VgPaintArgs>.SetNewHandler(
                    () => new VgPaintArgs(),
                    p => p.Reset());//when relese back
            }

            var context = PixelFarm.Temp<VgPaintArgs>.Borrow(out paintArgs);
            paintArgs.P = painter;
            return context;
        }
    }

}