//MIT, 2014-present, WinterDev

using System;
using System.Collections.Generic;

using PixelFarm.Drawing;
using PixelFarm.CpuBlit.VertexProcessing;


namespace PaintLab.Svg
{
    public abstract class VgVisitorBase
    {
        public VgVisualElement Current;
        public ICoordTransformer _currentTx;
        internal VgVisitorBase() { }
        internal virtual void Reset()
        {
            _currentTx = null;
            Current = null;
        }
    }

    public class VgVisitorArgs : VgVisitorBase
    {
        internal VgVisitorArgs() { }
        public Action<VertexStore, VgVisitorArgs> VgElemenVisitHandler;

        /// <summary>
        /// use for finding vg boundaries
        /// </summary>
        public float TempCurrentStrokeWidth { get; internal set; }

        internal override void Reset()
        {
            base.Reset();//*** reset base class fiels too

            //-------
            TempCurrentStrokeWidth = 1;
            VgElemenVisitHandler = null;
        }
    }


    public static class VgVistorArgsPool
    {

        public static PixelFarm.TempContext<VgVisitorArgs> Borrow(out VgVisitorArgs visitor)
        {
            if (!PixelFarm.Temp<VgVisitorArgs>.IsInit())
            {
                PixelFarm.Temp<VgVisitorArgs>.SetNewHandler(
                    () => new VgVisitorArgs(),
                    p => p.Reset());//when relese back
            }

            return PixelFarm.Temp<VgVisitorArgs>.Borrow(out visitor);
        }
    }

}