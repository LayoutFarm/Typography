//MIT, 2016-2017, WinterDev
using System.Collections.Generic;
namespace Typography.TextService
{
    public interface IInstalledFontProvider
    {
        IEnumerable<string> GetInstalledFontIter();
    }
}