//MIT, 2017, WinterDev
using System;
using Typography.OpenFont;

namespace TypographyTest
{
    public class TypefaceChangedEventArgs : EventArgs
    {
        public TypefaceChangedEventArgs(Typeface selectedTypeface)
        {
            this.SelectedTypeface = selectedTypeface;
        }
        public Typeface SelectedTypeface { get; private set; }
    }


}