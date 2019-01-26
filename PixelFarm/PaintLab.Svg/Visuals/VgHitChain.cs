//MIT, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;

namespace PaintLab.Svg
{
    public class VgHitChain
    {
        float _rootHitX;
        float _rootHitY;
        List<VgHitInfo> _vgHitList = new List<VgHitInfo>();
        public VgHitChain()
        {
        }

        public float X { get; private set; }
        public float Y { get; private set; }
        public void SetHitTestPos(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
        public bool WithSubPartTest { get; set; }
        public bool MakeCopyOfHitVxs { get; set; }
        public void AddHit(VgVisualElement svg, float x, float y, VertexStore copyOfVxs)
        {
            _vgHitList.Add(new VgHitInfo(svg, x, y, copyOfVxs));
        }
        //
        public int Count => _vgHitList.Count;
        //
        public VgHitInfo GetHitInfo(int index) => _vgHitList[index];
        //
        public VgHitInfo GetLastHitInfo() => _vgHitList[_vgHitList.Count - 1];
        //
        public void Clear()
        {
            this.X = this.Y = 0;
            _rootHitX = _rootHitY = 0;
            _vgHitList.Clear();
            MakeCopyOfHitVxs = WithSubPartTest = false;


        }
        public void SetRootGlobalPosition(float x, float y)
        {
            _rootHitX = x;
            _rootHitY = y;
        }
    }

    public struct VgHitInfo
    {
        public readonly VgVisualElement hitElem;
        public readonly float x;
        public readonly float y;
        public readonly VertexStore copyOfVxs;
        public VgHitInfo(VgVisualElement svg, float x, float y, VertexStore copyOfVxs)
        {
            this.hitElem = svg;
            this.x = x;
            this.y = y;
            this.copyOfVxs = copyOfVxs;
        }
        public SvgElement GetSvgElement()
        {
            return hitElem.GetController() as SvgElement;
        }
    }

}