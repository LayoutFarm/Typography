//BSD, 2014-present, WinterDev
using PixelFarm.Drawing;
namespace PaintFx.Effects
{
    public class ImgFilterStackBlur : ImageFilter
    {
        public override ImageFilterName Name
        {
            get
            {
                return ImageFilterName.StackBlur;
            }
        }
    }
    public class ImgFilterRecursiveBlur : ImageFilter
    {
        public override ImageFilterName Name
        {
            get
            {
                return ImageFilterName.RecursiveBlur;
            }
        }
    }
    public class ImgFilterSharpen : ImageFilter
    {
        public override ImageFilterName Name
        {
            get
            {
                return ImageFilterName.Sharpen;
            }
        }

    }
}