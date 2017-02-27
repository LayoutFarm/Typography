//MIT, 2014-2017, WinterDev
//-----------------------------------

using System;
namespace PixelFarm.Drawing.Fonts
{
    public abstract class FontFace : IDisposable
    {
        public bool HasKerning { get; set; }
        protected abstract void OnDispose();
        public void Dispose()
        {
            OnDispose();
        }
        ~FontFace()
        {
            OnDispose();
        }
        public abstract ActualFont GetFontAtPointsSize(float pointSize);
        public abstract string Name { get; }
        public abstract string FontPath { get; }
        public abstract float GetScale(float pointSize);
        public abstract int AscentInDzUnit { get; }
        public abstract int DescentInDzUnit { get; }
        public abstract int LineGapInDzUnit { get; }
        public abstract object GetInternalTypeface();
    }

}