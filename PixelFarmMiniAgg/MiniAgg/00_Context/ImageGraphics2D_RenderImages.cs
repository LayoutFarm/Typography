//MIT, 2014-2016, WinterDev

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
using PixelFarm.Agg.Image;
using PixelFarm.Agg.Transform;
using PixelFarm.VectorMath;
namespace PixelFarm.Agg
{
    partial class ImageGraphics2D
    {
        public override bool UseSubPixelRendering
        {
            get { return this.sclineRasToBmp.ScanlineRenderMode == ScanlineRenderMode.SubPixelRendering; }
            set { this.sclineRasToBmp.ScanlineRenderMode = value ? ScanlineRenderMode.SubPixelRendering : ScanlineRenderMode.Default; }
        }
        Affine BuildImageBoundsPath(
            int srcW, int srcH,
            VertexStore drawImageRectPath,
            double destX, double destY,
            double hotspotOffsetX, double hotSpotOffsetY,
            double scaleX, double scaleY,
            double angleRad)
        {
            AffinePlan[] plan = new AffinePlan[4];
            int i = 0;
            if (hotspotOffsetX != 0.0f || hotSpotOffsetY != 0.0f)
            {
                plan[i] = AffinePlan.Translate(-hotspotOffsetX, -hotSpotOffsetY);
                i++;
            }

            if (scaleX != 1 || scaleY != 1)
            {
                plan[i] = AffinePlan.Scale(scaleX, scaleY);
                i++;
            }

            if (angleRad != 0)
            {
                plan[i] = AffinePlan.Rotate(angleRad);
                i++;
            }

            if (destX != 0 || destY != 0)
            {
                plan[i] = AffinePlan.Translate(destX, destY);
                i++;
            }

            drawImageRectPath.Clear();
            drawImageRectPath.AddMoveTo(0, 0);
            drawImageRectPath.AddLineTo(srcW, 0);
            drawImageRectPath.AddLineTo(srcW, srcH);
            drawImageRectPath.AddLineTo(0, srcH);
            drawImageRectPath.AddCloseFigure();
            return Affine.NewMatix(plan);
        }
        Affine BuildImageBoundsPath(
           int srcW, int srcH,
           VertexStore drawImageRectPath,
           double destX, double destY)
        {
            AffinePlan plan = new AffinePlan();
            if (destX != 0 || destY != 0)
            {
                plan = AffinePlan.Translate(destX, destY);
            }

            drawImageRectPath.Clear();
            drawImageRectPath.AddMoveTo(0, 0);
            drawImageRectPath.AddLineTo(srcW, 0);
            drawImageRectPath.AddLineTo(srcW, srcH);
            drawImageRectPath.AddLineTo(0, srcH);
            drawImageRectPath.AddCloseFigure();
            return Affine.NewMatix(plan);
        }
        static Affine BuildImageBoundsPath(int srcW, int srcH,
           VertexStore drawImageRectPath, AffinePlan[] affPlans)
        {
            drawImageRectPath.Clear();
            drawImageRectPath.AddMoveTo(0, 0);
            drawImageRectPath.AddLineTo(srcW, 0);
            drawImageRectPath.AddLineTo(srcW, srcH);
            drawImageRectPath.AddLineTo(0, srcH);
            drawImageRectPath.AddCloseFigure();
            return Affine.NewMatix(affPlans);
        }
        void Render(VertexStore vxs, ISpanGenerator spanGen)
        {
            sclineRas.AddPath(vxs);
            sclineRasToBmp.RenderWithSpan(
                destImageReaderWriter,
                sclineRas,
                sclinePack8,
                spanGen);
        }

        public void Render(IImageReaderWriter source,
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
            VertexStore imgBoundsPath = GetFreeVxs();
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

                Affine destRectTransform = BuildImageBoundsPath(source.Width, source.Height, imgBoundsPath,
                    destX, destY, ox, oy, scaleX, scaleY, angleRadians);
                // We invert it because it is the transform to make the image go to the same position as the polygon. LBB [2/24/2004]
                Affine sourceRectTransform = destRectTransform.CreateInvert();
                var interpolator = new SpanInterpolatorLinear(sourceRectTransform);
                var imgSpanGen = new ImgSpanGenRGBA_BilinearClip(source, Drawing.Color.Black, interpolator);
                Render(destRectTransform.TransformToVxs(imgBoundsPath), imgSpanGen);
                // this is some debug you can enable to visualize the dest bounding box
                //LineFloat(BoundingRect.left, BoundingRect.top, BoundingRect.right, BoundingRect.top, WHITE);
                //LineFloat(BoundingRect.right, BoundingRect.top, BoundingRect.right, BoundingRect.bottom, WHITE);
                //LineFloat(BoundingRect.right, BoundingRect.bottom, BoundingRect.left, BoundingRect.bottom, WHITE);
                //LineFloat(BoundingRect.left, BoundingRect.bottom, BoundingRect.left, BoundingRect.top, WHITE);

            }
            else // TODO: this can be even faster if we do not use an intermediat buffer
            {
                Affine destRectTransform = BuildImageBoundsPath(source.Width, source.Height, imgBoundsPath, destX, destY);
                // We invert it because it is the transform to make the image go to the same position as the polygon. LBB [2/24/2004]
                Affine sourceRectTransform = destRectTransform.CreateInvert();
                var interpolator = new SpanInterpolatorLinear(sourceRectTransform);
                ImgSpanGen imgSpanGen = null;
                switch (source.BitDepth)
                {
                    case 32:
                        imgSpanGen = new ImgSpanGenRGBA_NN_StepXBy1(source, interpolator);
                        break;
                    case 24:
                        imgSpanGen = new ImgSpanGenRGB_NNStepXby1(source, interpolator);
                        break;
                    case 8:
                        imgSpanGen = new ImgSpanGenGray_NNStepXby1(source, interpolator);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                Render(destRectTransform.TransformToVxs(imgBoundsPath), imgSpanGen);
                unchecked { destImageChanged++; };
            }
            ReleaseVxs(imgBoundsPath);
        }

        int destImageChanged = 0;
        public void Render(IImageReaderWriter source, AffinePlan[] affinePlans)
        {
            VertexStore tmpImgBoundVxs = GetFreeVxs();
            Affine destRectTransform = BuildImageBoundsPath(source.Width, source.Height, tmpImgBoundVxs, affinePlans);
            // We invert it because it is the transform to make the image go to the same position as the polygon. LBB [2/24/2004]
            Affine sourceRectTransform = destRectTransform.CreateInvert();
            var imgSpanGen = new ImgSpanGenRGBA_BilinearClip(
                source,
                Drawing.Color.Black,
                new SpanInterpolatorLinear(sourceRectTransform));
            Render(destRectTransform.TransformToVxs(tmpImgBoundVxs), imgSpanGen);
            ReleaseVxs(tmpImgBoundVxs);
        }
        public void Render(IImageReaderWriter source, double destX, double destY)
        {
            int inScaleX = 1;
            int inScaleY = 1;
            int angleRadians = 0;
            // exit early if the dest and source bounds don't touch.
            // TODO: <BUG> make this do rotation and scalling
            RectInt sourceBounds = source.GetBounds();
            RectInt destBounds = this.destImageReaderWriter.GetBounds();
            sourceBounds.Offset((int)destX, (int)destY);
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

            bool needSourceResampling = isScale || isRotated || destX != (int)destX || destY != (int)destY;
            var imgBoundsPath = GetFreeVxs();
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


                Affine destRectTransform = BuildImageBoundsPath(source.Width, source.Height, imgBoundsPath,
                    destX, destY, ox, oy, scaleX, scaleY, angleRadians);
                // We invert it because it is the transform to make the image go to the same position as the polygon. LBB [2/24/2004]
                Affine sourceRectTransform = destRectTransform.CreateInvert();
                var imgSpanGen = new ImgSpanGenRGBA_BilinearClip(
                    source,
                    Drawing.Color.Black,
                    new SpanInterpolatorLinear(sourceRectTransform));
                Render(destRectTransform.TransformToVxs(imgBoundsPath), imgSpanGen);
#if false // this is some debug you can enable to visualize the dest bounding box
		        LineFloat(BoundingRect.left, BoundingRect.top, BoundingRect.right, BoundingRect.top, WHITE);
		        LineFloat(BoundingRect.right, BoundingRect.top, BoundingRect.right, BoundingRect.bottom, WHITE);
		        LineFloat(BoundingRect.right, BoundingRect.bottom, BoundingRect.left, BoundingRect.bottom, WHITE);
		        LineFloat(BoundingRect.left, BoundingRect.bottom, BoundingRect.left, BoundingRect.top, WHITE);
#endif
            }
            else // TODO: this can be even faster if we do not use an intermediat buffer
            {
                Affine destRectTransform = BuildImageBoundsPath(source.Width, source.Height,
                    imgBoundsPath,
                    destX, destY);
                // We invert it because it is the transform to make the image go to the same position as the polygon. LBB [2/24/2004]
                Affine sourceRectTransform = destRectTransform.CreateInvert();
                var interpolator = new SpanInterpolatorLinear(sourceRectTransform);
                ImgSpanGen imgSpanGen = null;
                switch (source.BitDepth)
                {
                    case 32:
                        imgSpanGen = new ImgSpanGenRGBA_NN_StepXBy1(source, interpolator);
                        break;
                    case 24:
                        imgSpanGen = new ImgSpanGenRGB_NNStepXby1(source, interpolator);
                        break;
                    case 8:
                        imgSpanGen = new ImgSpanGenGray_NNStepXby1(source, interpolator);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                Render(destRectTransform.TransformToVxs(imgBoundsPath), imgSpanGen);
                unchecked { destImageChanged++; };
            }
            ReleaseVxs(imgBoundsPath);
        }
    }
}