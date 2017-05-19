//MIT, 2016-2017, WinterDev
using System.Collections.Generic;
namespace PixelFarm.Drawing.Fonts
{
    public interface IInstalledFontProvider
    {
        IEnumerable<string> GetInstalledFontIter();
    }
}