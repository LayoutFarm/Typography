using System.Diagnostics;
using Typography.Rendering;

namespace Xamarin.OpenGL
{
    [DebuggerDisplay("{pos} {uv} {color}")]
    internal struct DrawVertex
    {
        public Point pos;
        public Point uv;
        public Color color;
    }
}