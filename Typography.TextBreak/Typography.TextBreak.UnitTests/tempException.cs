using System;
namespace Typography.OpenFont
{
    public class OpenFontNotSupportedException : Exception
    {
        public OpenFontNotSupportedException() { }
        public OpenFontNotSupportedException(string msg) : base(msg) { }
    }
}