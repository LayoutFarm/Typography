//MIT, 2014-2016, WinterDev

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
namespace PixelFarm.Drawing
{
    public abstract class CustomRenderSurface
    {
        public CustomRenderSurface()
        {
        }

        public abstract bool FullModeUpdate
        {
            get;
            set;
        }


        public abstract int Width
        {
            get;
        }
        public abstract int Height
        {
            get;
        }
        public abstract void ConfirmSizeChanged();
        public abstract void QuadPagesCalculateCanvas();
        public abstract Size OwnerInnerContentSize
        {
            get;
        }


        public abstract void DrawToThisPage(Canvas destPage, Rectangle updateArea);
        //------------------------------------
    }
}