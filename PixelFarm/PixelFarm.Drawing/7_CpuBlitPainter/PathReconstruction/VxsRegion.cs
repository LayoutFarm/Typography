//MIT, 2019-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.VertexProcessing;

namespace PixelFarm.PathReconstruction
{

    public class VxsRegion : CpuBlitRegion
    {
        VertexStore _vxs;//vector path for the data
        bool _isSimpleRect;
        bool _evalRectBounds;
        Rectangle _bounds;

        /// <summary>
        /// create simple Rect region
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public VxsRegion(float left, float top, float width, float height)
        {
            _isSimpleRect = true;
            using (VxsTemp.Borrow(out VertexStore v1))
            using (VectorToolBox.Borrow(out SimpleRect rect))
            {
                rect.SetRectFromLTWH(left, top, width, height);
                rect.MakeVxs(v1);
                _vxs = v1.CreateTrim();
            }
            _bounds = new Rectangle(
               (int)Math.Round(left),
               (int)Math.Round(top),
               (int)Math.Round(width),
               (int)Math.Round(height));
            _evalRectBounds = true;
        }
        public override bool IsSimpleRect => _isSimpleRect;
        /// <summary>
        /// create a region from vxs (may be simple rect vxs or complex vxs)
        /// </summary>
        /// <param name="vxs"></param>
        public VxsRegion(VertexStore vxs)
        {
            //COPY
            _vxs = vxs.CreateTrim();//we don't store outside data  
        }
        public override CpuBlitRegionKind Kind => CpuBlitRegionKind.VxsRegion;

        public override void Dispose()
        {
            if (_vxs != null)
            {
                _vxs = null;
            }
        }

        public VertexStore GetVxs() => _vxs;

        public override Rectangle GetRectBounds()
        {
            if (!_evalRectBounds)
            {
                //
                RectD bound1 = BoundingRect.GetBoundingRect(_vxs);
                _bounds = new Rectangle(
                   (int)Math.Round(bound1.Left),
                   (int)Math.Round(bound1.Bottom), //*** 
                   (int)Math.Round(bound1.Width),
                   (int)Math.Round(bound1.Height));
                _evalRectBounds = true;
            }

            return _bounds;
        }
        public override Region CreateComplement(Region another)
        {
            CpuBlitRegion rgnB = another as CpuBlitRegion;
            if (rgnB == null) return null;
            //
            switch (rgnB.Kind)
            {
                default: throw new System.NotSupportedException();
                case CpuBlitRegionKind.BitmapBasedRegion:
                    break;
                case CpuBlitRegionKind.MixedRegion:
                    break;
                case CpuBlitRegionKind.VxsRegion:
                    //TODO: review complement
                    break;
            }
            return null;
        }

        public override Region CreateExclude(Region another)
        {
            CpuBlitRegion rgnB = another as CpuBlitRegion;
            if (rgnB == null) return null;
            switch (rgnB.Kind)
            {
                default: throw new System.NotSupportedException();
                case CpuBlitRegionKind.BitmapBasedRegion:
                    break;
                case CpuBlitRegionKind.MixedRegion:
                    break;
                case CpuBlitRegionKind.VxsRegion:
                    return new VxsRegion(VxsClipper.CombinePaths(_vxs, ((VxsRegion)another)._vxs, VxsClipperType.Difference, false, null));
            }
            return null;
        }
        public override Region CreateIntersect(Region another)
        {
            CpuBlitRegion rgnB = another as CpuBlitRegion;
            if (rgnB == null) return null;
            switch (rgnB.Kind)
            {
                default: throw new System.NotSupportedException();
                case CpuBlitRegionKind.BitmapBasedRegion:
                    //another is bitmap-based rgn
                    //we need to create bitmap presentation of this vxs rgn 
                    //and then send this bitmap to operate with another rgn

                    break;
                case CpuBlitRegionKind.MixedRegion:
                    break;
                case CpuBlitRegionKind.VxsRegion:
                    return new VxsRegion(VxsClipper.CombinePaths(_vxs, ((VxsRegion)another)._vxs, VxsClipperType.InterSect, false, null));
            }
            return null;
        }
        public override Region CreateUnion(Region another)
        {
            CpuBlitRegion rgnB = another as CpuBlitRegion;
            if (rgnB == null) return null;
            switch (rgnB.Kind)
            {
                default: throw new System.NotSupportedException();
                case CpuBlitRegionKind.BitmapBasedRegion:
                    //another is bitmap-based rgn
                    //we need to create bitmap presentation of this vxs rgn 
                    //and then send this bitmap to operate with another rgn

                    break;
                case CpuBlitRegionKind.MixedRegion:
                    break;
                case CpuBlitRegionKind.VxsRegion:
                    return new VxsRegion(VxsClipper.CombinePaths(_vxs, ((VxsRegion)another)._vxs, VxsClipperType.Union, false, null));
            }
            return null;
        }
        public override Region CreateXor(Region another)
        {
            CpuBlitRegion rgnB = another as CpuBlitRegion;
            if (rgnB == null) return null;
            switch (rgnB.Kind)
            {
                default: throw new System.NotSupportedException();
                case CpuBlitRegionKind.BitmapBasedRegion:
                    //another is bitmap-based rgn
                    //we need to create bitmap presentation of this vxs rgn 
                    //and then send this bitmap to operate with another rgn

                    break;
                case CpuBlitRegionKind.MixedRegion:
                    break;
                case CpuBlitRegionKind.VxsRegion:
                    return new VxsRegion(VxsClipper.CombinePaths(_vxs, ((VxsRegion)another)._vxs, VxsClipperType.Xor, false, null));
            }
            return null;
        }
    }
}