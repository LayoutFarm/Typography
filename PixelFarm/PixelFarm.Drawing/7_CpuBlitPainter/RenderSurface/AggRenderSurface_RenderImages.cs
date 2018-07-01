//MIT, 2014-present, WinterDev

//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------

using System;
using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.CpuBlit.Rasterization;
using PixelFarm.CpuBlit.FragmentProcessing;

using PixelFarm.VectorMath;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.Imaging;

namespace PixelFarm.CpuBlit
{
    partial class AggRenderSurface
    {
        public bool UseSubPixelRendering
        {
            get { return this._bmpRasterizer.ScanlineRenderMode == ScanlineRenderMode.SubPixelLcdEffect; }
            set { this._bmpRasterizer.ScanlineRenderMode = value ? ScanlineRenderMode.SubPixelLcdEffect : ScanlineRenderMode.Default; }
        }

        static void BuildOrgImgRectVxs(int srcW, int srcH, VertexStore output)
        {
            output.Clear();
            output.AddMoveTo(0, 0);
            output.AddLineTo(srcW, 0);
            output.AddLineTo(srcW, srcH);
            output.AddLineTo(0, srcH);
            output.AddCloseFigure();
        }

        static Affine CreateAffine(double destX, double destY,
            double hotspotOffsetX, double hotSpotOffsetY,
            double scaleX, double scaleY,
            double angleRad)
        {

            AffinePlan[] plans = new AffinePlan[4];
            int i = 0;
            if (hotspotOffsetX != 0.0f || hotSpotOffsetY != 0.0f)
            {
                plans[i] = AffinePlan.Translate(-hotspotOffsetX, -hotSpotOffsetY);
                i++;
            }

            if (scaleX != 1 || scaleY != 1)
            {
                plans[i] = AffinePlan.Scale(scaleX, scaleY);
                i++;
            }

            if (angleRad != 0)
            {
                plans[i] = AffinePlan.Rotate(angleRad);
                i++;
            }

            if (destX != 0 || destY != 0)
            {
                plans[i] = AffinePlan.Translate(destX, destY);
                i++;
            }
            return Affine.NewMatix(plans);
        }
        static Affine CreateAffine(double destX, double destY)
        {
            AffinePlan plan = new AffinePlan();
            if (destX != 0 || destY != 0)
            {
                plan = AffinePlan.Translate(destX, destY);
            }
            return Affine.NewMatix(plan);
        }

        /// <summary>
        /// we do NOT store vxs
        /// </summary>
        /// <param name="vxs"></param>
        /// <param name="spanGen"></param>
        void Render(VertexStore vxs, ISpanGenerator spanGen)
        {
            sclineRas.AddPath(vxs);
            _bmpRasterizer.RenderWithSpan(
                destImageReaderWriter,
                sclineRas,
                sclinePack8,
                spanGen);
        }

        public void Render(IBitmapSrc source,
            double destX, double destY,
            double angleRadians,
            double inScaleX, double inScaleY)
        {
            {   // exit early if the dest and source bounds don't touch.
                // TODO: <BUG> make this do rotation and scalling
                RectInt sourceBounds = source.GetBounds();
                RectInt destBounds = this.destImageReaderWriter.GetBounds();
                sourceBounds.Offset((int)destX, (int)destY);
                if (!RectInt.DoIntersect(sourceBounds, destBounds))
                {
                    if (inScaleX != 1 || inScaleY != 1 || angleRadians != 0)
                    {
                        throw new NotImplementedException();
                    }
                    return;
                }
            }

            double scaleX = inScaleX;
            double scaleY = inScaleY;
            Affine graphicsTransform = this.CurrentTransformMatrix;
            if (!graphicsTransform.IsIdentity())
            {
                if (scaleX != 1 || scaleY != 1 || angleRadians != 0)
                {
                    throw new NotImplementedException();
                }
                graphicsTransform.Transform(ref destX, ref destY);
            }

#if false // this is an optomization that eliminates the drawing of images that have their alpha set to all 0 (happens with generated images like explosions).
	        MaxAlphaFrameProperty maxAlphaFrameProperty = MaxAlphaFrameProperty::GetMaxAlphaFrameProperty(source);

	        if((maxAlphaFrameProperty.GetMaxAlpha() * color.A_Byte) / 256 <= ALPHA_CHANNEL_BITS_DIVISOR)
	        {
		        m_OutFinalBlitBounds.SetRect(0,0,0,0);
	        }
#endif
            bool isScale = (scaleX != 1 || scaleY != 1);
            bool isRotated = true;
            if (Math.Abs(angleRadians) < (0.1 * MathHelper.Tau / 360))
            {
                isRotated = false;
                angleRadians = 0;
            }

            //bool IsMipped = false;
            //double ox, oy;
            //source.GetOriginOffset(out ox, out oy);

            bool canUseMipMaps = isScale;
            if (scaleX > 0.5 || scaleY > 0.5)
            {
                canUseMipMaps = false;
            }

            bool renderRequriesSourceSampling = isScale || isRotated || destX != (int)destX || destY != (int)destY;

            VectorToolBox.GetFreeVxs(out VertexStore imgBoundsPath);

            // this is the fast drawing path
            if (renderRequriesSourceSampling)
            {
                // if the scalling is small enough the results can be improved by using mip maps
                //if(CanUseMipMaps)
                //{
                //    CMipMapFrameProperty* pMipMapFrameProperty = CMipMapFrameProperty::GetMipMapFrameProperty(source);
                //    double OldScaleX = scaleX;
                //    double OldScaleY = scaleY;
                //    const CFrameInterface* pMippedFrame = pMipMapFrameProperty.GetMipMapFrame(ref scaleX, ref scaleY);
                //    if(pMippedFrame != source)
                //    {
                //        IsMipped = true;
                //        source = pMippedFrame;
                //        sourceOriginOffsetX *= (OldScaleX / scaleX);
                //        sourceOriginOffsetY *= (OldScaleY / scaleY);
                //    }

                //    HotspotOffsetX *= (inScaleX / scaleX);
                //    HotspotOffsetY *= (inScaleY / scaleY);
                //} 
                //Affine destRectTransform = BuildImageBoundsPath(source.Width, source.Height,
                //    destX, destY, ox, oy, scaleX, scaleY, angleRadians, imgBoundsPath); 

                //1. 
                BuildOrgImgRectVxs(source.Width, source.Height, imgBoundsPath);
                //2. 
                Affine destRectTransform = CreateAffine(destX, destY, ox, oy, scaleX, scaleY, angleRadians);
                //TODO: review reusable span generator an interpolator ***
                var interpolator = new SpanInterpolatorLinear();

                // We invert it because it is the transform to make the image go to the same position as the polygon. LBB [2/24/2004]
                interpolator.Transformer = destRectTransform.CreateInvert();
                var imgSpanGen = new ImgSpanGenRGBA_BilinearClip(source, Drawing.Color.Black, interpolator);

                VectorToolBox.GetFreeVxs(out var v1);
                destRectTransform.TransformToVxs(imgBoundsPath, v1);
                Render(v1, imgSpanGen);
                VectorToolBox.ReleaseVxs(ref v1);
                // this is some debug you can enable to visualize the dest bounding box
                //LineFloat(BoundingRect.left, BoundingRect.top, BoundingRect.right, BoundingRect.top, WHITE);
                //LineFloat(BoundingRect.right, BoundingRect.top, BoundingRect.right, BoundingRect.bottom, WHITE);
                //LineFloat(BoundingRect.right, BoundingRect.bottom, BoundingRect.left, BoundingRect.bottom, WHITE);
                //LineFloat(BoundingRect.left, BoundingRect.bottom, BoundingRect.left, BoundingRect.top, WHITE);

            }
            else // TODO: this can be even faster if we do not use an intermediat buffer
            {
                //Affine destRectTransform = BuildImageBoundsPath(source.Width, source.Height, destX, destY, imgBoundsPath);

                BuildOrgImgRectVxs(source.Width, source.Height, imgBoundsPath);

                //
                var destRectTransform = new AffineMat();
                destRectTransform.Translate(destX, destY);
                //TODO: review reusable span generator an interpolator ***
                var interpolator = new SpanInterpolatorLinear();
                // We invert it because it is the transform to make the image go to the same position as the polygon. LBB [2/24/2004] 
                _reuseableAffine.SetElements(destRectTransform.CreateInvert());
                interpolator.Transformer = _reuseableAffine;
                ImgSpanGen imgSpanGen = null;
                switch (source.BitDepth)
                {
                    case 32:
                        imgSpanGen = new ImgSpanGenRGBA_NN_StepXBy1(source, interpolator);
                        break;
                    //case 24:
                    //    imgSpanGen = new ImgSpanGenRGB_NNStepXby1(source, interpolator);
                    //    break;
                    //case 8:
                    //    imgSpanGen = new ImgSpanGenGray_NNStepXby1(source, interpolator);
                    //    break;
                    default:
                        throw new NotImplementedException();
                }

                VectorToolBox.GetFreeVxs(out var v1);
                TransformToVxs(ref destRectTransform, imgBoundsPath, v1);

                Render(v1, imgSpanGen);
                VectorToolBox.ReleaseVxs(ref v1);
                //
                unchecked { destImageChanged++; };
            }
            VectorToolBox.ReleaseVxs(ref imgBoundsPath);
        }


        Affine _reuseableAffine = Affine.NewIdentity();
        /// <summary>
        /// we do NOT store vxs, return original outputVxs
        /// </summary>
        /// <param name="src"></param>
        /// <param name="outputVxs"></param>
        static void TransformToVxs(ref AffineMat aff, VertexStore src, VertexStore outputVxs)
        {
            int count = src.Count;
            VertexCmd cmd;
            double x, y;
            for (int i = 0; i < count; ++i)
            {
                cmd = src.GetVertex(i, out x, out y);
                aff.Transform(ref x, ref y);
                outputVxs.AddVertex(x, y, cmd);
            }
        }


        int destImageChanged = 0;
        public void Render(IBitmapSrc source, AffinePlan[] affinePlans)
        {

            VectorToolBox.GetFreeVxs(out var v1, out var v2);

            //BuildImageBoundsPath(source.Width, source.Height, affinePlans, v1); 
            BuildOrgImgRectVxs(source.Width, source.Height, v1);


            //Affine destRectTransform = Affine.NewMatix(affinePlans);

            var destRectTransform = new AffineMat();
            destRectTransform.BuildFromAffinePlans(affinePlans);

            //TODO: review reusable span generator an interpolator ***
            var spanInterpolator = new SpanInterpolatorLinear();
            // We invert it because it is the transform to make the image go to the same position as the polygon. LBB [2/24/2004]

            _reuseableAffine.SetElements(destRectTransform.CreateInvert());
            spanInterpolator.Transformer = _reuseableAffine;//

            var imgSpanGen = new ImgSpanGenRGBA_BilinearClip(
                source,
                Drawing.Color.Transparent,
                spanInterpolator);

            TransformToVxs(ref destRectTransform, v1, v2);

            Render(v2, imgSpanGen);
            // 
            VectorToolBox.ReleaseVxs(ref v1, ref v2);
        }


        SubBitmap subBitmap = new SubBitmap();
        public void Render(IBitmapSrc source, double destX, double destY, double srcX, double srcY, double srcW, double srcH)
        {
            //copy some part of src img to destination

            subBitmap.SetSrcBitmap(source, (int)srcX, (int)srcY, (int)srcW, (int)srcH);
            Render(subBitmap, destX, destY);
        }
        public void Render(IBitmapSrc source, double destX, double destY)
        {
            int inScaleX = 1;
            int inScaleY = 1;
            int angleRadians = 0;
            // exit early if the dest and source bounds don't touch.
            // TODO: <BUG> make this do rotation and scalling
           
            RectInt sourceBounds = new RectInt((int)destX, (int)destY, (int)destX + source.Width, (int)destY+ source.Height);
            //sourceBounds.Offset((int)destX, (int)destY);

            RectInt destBounds = this.destImageReaderWriter.GetBounds();
            if (!RectInt.DoIntersect(sourceBounds, destBounds))
            {
                //if (inScaleX != 1 || inScaleY != 1 || angleRadians != 0)
                //{
                //    throw new NotImplementedException();
                //}
                return;
            }

            double scaleX = inScaleX;
            double scaleY = inScaleY;
            Affine graphicsTransform = this.CurrentTransformMatrix;
            if (!graphicsTransform.IsIdentity())
            {
                if (scaleX != 1 || scaleY != 1 || angleRadians != 0)
                {
                    throw new NotImplementedException();
                }
                graphicsTransform.Transform(ref destX, ref destY);
            }


#if false // this is an optomization that eliminates the drawing of images that have their alpha set to all 0 (happens with generated images like explosions).
	        MaxAlphaFrameProperty maxAlphaFrameProperty = MaxAlphaFrameProperty::GetMaxAlphaFrameProperty(source);

	        if((maxAlphaFrameProperty.GetMaxAlpha() * color.A_Byte) / 256 <= ALPHA_CHANNEL_BITS_DIVISOR)
	        {
		        m_OutFinalBlitBounds.SetRect(0,0,0,0);
	        }
#endif
            bool isScale = (scaleX != 1 || scaleY != 1);
            bool isRotated = false;
            if (angleRadians != 0 && Math.Abs(angleRadians) >= (0.1 * MathHelper.Tau / 360))
            {
                isRotated = true;
            }
            else
            {
                angleRadians = 0;//reset very small angle to 0 
            }

            //bool IsMipped = false;
            //double ox, oy;
            //source.GetOriginOffset(out ox, out oy);

            bool canUseMipMaps = isScale;
            if (scaleX > 0.5 || scaleY > 0.5)
            {
                canUseMipMaps = false;
            }

            bool needSourceResampling = isScale || isRotated;// || destX != (int)destX || destY != (int)destY;


            VectorToolBox.GetFreeVxs(out VertexStore imgBoundsPath);
            // this is the fast drawing path
            if (needSourceResampling)
            {

#if false // if the scalling is small enough the results can be improved by using mip maps
                
	        if(CanUseMipMaps)
	        {
		        CMipMapFrameProperty* pMipMapFrameProperty = CMipMapFrameProperty::GetMipMapFrameProperty(source);
		        double OldScaleX = scaleX;
		        double OldScaleY = scaleY;
		        const CFrameInterface* pMippedFrame = pMipMapFrameProperty.GetMipMapFrame(ref scaleX, ref scaleY);
		        if(pMippedFrame != source)
		        {
			        IsMipped = true;
			        source = pMippedFrame;
			        sourceOriginOffsetX *= (OldScaleX / scaleX);
			        sourceOriginOffsetY *= (OldScaleY / scaleY);
		        }

			    HotspotOffsetX *= (inScaleX / scaleX);
			    HotspotOffsetY *= (inScaleY / scaleY);
	        }
#endif


                BuildOrgImgRectVxs(source.Width, source.Height, imgBoundsPath);

                //Affine destRectTransform = BuildImageBoundsPath(source.Width, source.Height,
                //    destX, destY, ox, oy, scaleX, scaleY, angleRadians, imgBoundsPath);

                Affine destRectTransform = CreateAffine(destX, destY, ox, oy, scaleX, scaleY, angleRadians);
                //TODO: review reusable span generator an interpolator ***
                var spanInterpolator = new SpanInterpolatorLinear();
                // We invert it because it is the transform to make the image go to the same position as the polygon. LBB [2/24/2004]
                spanInterpolator.Transformer = destRectTransform.CreateInvert();

                var imgSpanGen = new ImgSpanGenRGBA_BilinearClip(
                    source,
                    Drawing.Color.Black,
                    spanInterpolator);

                VectorToolBox.GetFreeVxs(out VertexStore v1);
                destRectTransform.TransformToVxs(imgBoundsPath, v1);
                Render(v1, imgSpanGen);
                VectorToolBox.ReleaseVxs(ref v1);


#if false // this is some debug you can enable to visualize the dest bounding box
		        LineFloat(BoundingRect.left, BoundingRect.top, BoundingRect.right, BoundingRect.top, WHITE);
		        LineFloat(BoundingRect.right, BoundingRect.top, BoundingRect.right, BoundingRect.bottom, WHITE);
		        LineFloat(BoundingRect.right, BoundingRect.bottom, BoundingRect.left, BoundingRect.bottom, WHITE);
		        LineFloat(BoundingRect.left, BoundingRect.bottom, BoundingRect.left, BoundingRect.top, WHITE);
#endif
            }
            else // TODO: this can be even faster if we do not use an intermediate buffer
            {

                //Affine destRectTransform = BuildImageBoundsPath(
                //    source.Width, source.Height,
                //    destX, destY, imgBoundsPath);


                BuildOrgImgRectVxs(source.Width, source.Height, imgBoundsPath);
                //...
                Affine destRectTransform = CreateAffine(destX, destY);

                //TODO: review reusable span generator an interpolator ***
                var interpolator = new SpanInterpolatorLinear();

                // We invert it because it is the transform to make the image go to the same position as the polygon. LBB [2/24/2004]
                interpolator.Transformer = destRectTransform.CreateInvert();

                //we generate image by this imagespan generator

                ImgSpanGen imgSpanGen = null;
                switch (source.BitDepth)
                {
                    case 32:
                        imgSpanGen = new ImgSpanGenRGBA_NN_StepXBy1(source, interpolator);
                        break;
                    //case 8:
                    //    imgSpanGen = new ImgSpanGenGray_NNStepXby1(source, interpolator);
                    //    break;
                    default:
                        throw new NotImplementedException();
                }

                VectorToolBox.GetFreeVxs(out VertexStore v1);

                destRectTransform.TransformToVxs(imgBoundsPath, v1);
                //...
                Render(v1, imgSpanGen);

                VectorToolBox.ReleaseVxs(ref v1);
                //
                unchecked { destImageChanged++; };
            }
            VectorToolBox.ReleaseVxs(ref imgBoundsPath);
        }
    }

    class SubBitmap : IBitmapSrc
    {
        IBitmapSrc _src;
        int _orgSrcW;
        int _x, _y, _w, _h;
        public SubBitmap()
        {
        }
        public void SetSrcBitmap(IBitmapSrc src, int x, int y, int w, int h)
        {
            _orgSrcW = src.Width;//
            _src = src;
            _x = x;
            _y = y;
            _w = w;
            _h = h;
        }

        public int BitDepth
        {
            get
            {
                return 32; //
            }
        }
        public int Width
        {
            get { return _w; }
        }

        public int Height
        {
            get { return _h; }
        }

        public int Stride
        {
            get { return _w << 2; }
        }
        public int BytesBetweenPixelsInclusive
        {
            get { throw new NotSupportedException(); }
        }
        public RectInt GetBounds()
        {
            return new RectInt(_x, _y, _x + _w, _y + _h);
        }

        public int GetBufferOffsetXY32(int x, int y)
        {
            //goto row
            return ((_y + y) * _orgSrcW) + _x + x;
        }
        //public int GetByteBufferOffsetXY(int x, int y)
        //{
        //    throw new NotImplementedException();
        //}
        public TempMemPtr GetBufferPtr()
        {
            return _src.GetBufferPtr();
        }
        public int[] GetOrgInt32Buffer()
        {
            return _src.GetOrgInt32Buffer();
        }
        public Color GetPixel(int x, int y)
        {
            //TODO: not support here
            throw new NotImplementedException();
        }
        public void ReplaceBuffer(int[] newBuffer)
        {
            //not support replace buffer?

        }
    }

}