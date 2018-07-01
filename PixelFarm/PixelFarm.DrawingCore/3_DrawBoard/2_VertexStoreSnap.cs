//BSD, 2014-present, WinterDev

using System.Collections.Generic;
using PixelFarm.CpuBlit;
namespace PixelFarm.Drawing
{
    //----------------------------------------
    public struct VertexSnapIter
    {
        int currentIterIndex;
        VertexStore vxs;
        internal VertexSnapIter(VertexStoreSnap vsnap)
        {
            this.vxs = VertexStoreSnap.GetInternalVxs(vsnap);
            this.currentIterIndex = vsnap.StartAt;
        }

        public VertexCmd GetNextVertex(out double x, out double y)
        {
            return vxs.GetVertex(currentIterIndex++, out x, out y);
        }

#if DEBUG
        public int dbugIterIndex { get { return currentIterIndex; } }
#endif
    }

    public struct VertexStoreSnap
    {
        int startAt;
        VertexStore vxs;
        public VertexStoreSnap(VertexStore vxs)
        {
            this.vxs = vxs;
            this.startAt = 0;
        }
        public VertexStoreSnap(VertexStore vxs, int startAt)
        {
            this.vxs = vxs;
            this.startAt = startAt;
        }

        public int StartAt
        {
            get { return this.startAt; }
        }

        public VertexSnapIter GetVertexSnapIter()
        {
            return new VertexSnapIter(this);
        }
        public static VertexStore GetInternalVxs(VertexStoreSnap snap)
        {
            return snap.vxs;
        }
    }

    /// <summary>
    /// for vertex store pool mx
    /// </summary>
    class VertexStorePool
    {
        Stack<VertexStore> _stack = new Stack<VertexStore>();
        public VertexStore GetFreeVxs()
        {
            if (_stack.Count > 0)
            {
                return _stack.Pop();
            }
            else
            {
                return new VertexStore();
            }
        }
        public void Release(ref VertexStore vxs)
        {
            vxs.Clear();
            _stack.Push(vxs);
            vxs = null;
        }
    }

}