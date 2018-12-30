//----------------------------------------------------------------------------
//MIT, 2014-present, WinterDev

using System;
using PixelFarm.Drawing;

namespace PixelFarm.CpuBlit
{

    public class VxsRenderVx : RenderVx
    {
        public VertexStore _vxs;
        object _resolvedObject;
        public VxsRenderVx(VertexStore vxs)
        {
            _vxs = vxs;

        }

        public static object GetResolvedObject(VxsRenderVx vxsRenerVx)
        {
            return vxsRenerVx._resolvedObject;
        }
        public static void SetResolvedObject(VxsRenderVx vxsRenerVx, object obj)
        {
            vxsRenerVx._resolvedObject = obj;
        } 
    } 
}