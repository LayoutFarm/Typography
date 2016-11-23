//BSD, 2014-2016, WinterDev

using System.Collections.Generic;
namespace PixelFarm.Agg
{
    //----------------------------------------
    public struct VertexSnapIter
    {
        int currentIterIndex;
        VertexStore vxs;
        internal VertexSnapIter(VertexStoreSnap vsnap)
        {
            this.vxs = vsnap.GetInternalVxs();
            this.currentIterIndex = vsnap.StartAt;
        }

        public VertexCmd GetNextVertex(out double x, out double y)
        {
            return vxs.GetVertex(currentIterIndex++, out x, out y);
        }
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
        public VertexStore GetInternalVxs()
        {
            return this.vxs;
        }
        public int StartAt
        {
            get { return this.startAt; }
        }
        public bool VxsHasMoreThanOnePart
        {
            get { return this.vxs.HasMoreThanOnePart; }
        }
        public VertexSnapIter GetVertexSnapIter()
        {
            return new VertexSnapIter(this);
        }
    }

    /// <summary>
    /// for vertex store pool mx
    /// </summary>
    public class VertexStorePool
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