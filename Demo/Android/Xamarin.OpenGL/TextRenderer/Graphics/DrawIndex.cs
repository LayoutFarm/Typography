using System.Diagnostics;

namespace Xamarin.OpenGL
{
    [DebuggerDisplay("{Index}")]
    internal struct DrawIndex
    {
        int index;

        public int Index
        {
            get { return index; }
            set { this.index = value; }
        }

        public static implicit operator int(DrawIndex v)
        {
            return v.Index;
        }
    }
}