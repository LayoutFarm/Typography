//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;
using System.IO;

namespace SampleWinForms
{
    class TempLocalFontFile
    {
        //temp only
        public readonly string actualFileName;
        public TempLocalFontFile(string actualFileName)
        {
            this.actualFileName = actualFileName;
        }
        public string OnlyFileName
        {
            get { return Path.GetFileName(actualFileName); }
        }
#if DEBUG
        public override string ToString()
        {
            return this.OnlyFileName;
        }
#endif
    }

    public static class CommonWorkspace
    {
          

    }
}