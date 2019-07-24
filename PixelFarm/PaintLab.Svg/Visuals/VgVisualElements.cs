//MIT, 2014-present, WinterDev

using System;
using System.Collections.Generic;

using PixelFarm.Drawing;
using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.VertexProcessing;


namespace PaintLab.Svg
{


    /// <summary>
    /// Base class of vg element that draw on rendering surface with painter. 
    /// </summary>
    public abstract class VgVisualElementBase
    {
        public virtual void Paint(VgPaintArgs p)
        {
            //paint with painter interface
        }
        public abstract WellknownSvgElementName ElemName { get; }
        public virtual void Accept(VgVisitorArgs p) { }

        /// <summary>
        /// clone visual part
        /// </summary>
        /// <returns></returns>
        public abstract VgVisualElementBase Clone();

#if DEBUG
        public bool dbugHasParent;
        public string dbugNote;
        public readonly int dbugId = s_dbugTotalId++;
        static int s_dbugTotalId;
#endif
    }


    public class VgTextNodeVisualElement : VgVisualElementBase
    {
        public VgTextNodeVisualElement() { }
        public string TextContent { get; set; }
        public override WellknownSvgElementName ElemName => WellknownSvgElementName.Text;
        public override VgVisualElementBase Clone()
        {
            return new VgTextNodeVisualElement { TextContent = this.TextContent };
        }
    }
    class VgPathVisualMarkers
    {
        public PointF StartMarkerPos { get; set; }
        public PointF EndMarkerPos { get; set; }
        public PointF[] AllPoints { get; set; }

        public Affine StartMarkerAffine { get; set; }
        public Affine MidMarkerAffine { get; set; }
        public Affine EndMarkerAffine { get; set; }

        public VgVisualElement StartMarker { get; set; }
        public VgVisualElement MidMarker { get; set; }
        public VgVisualElement EndMarker { get; set; }
    }

    class VgUseVisualElement : VgVisualElement
    {
        internal VgUseVisualElement(SvgUseSpec useSpec, VgVisualDoc root)
            : base(WellknownSvgElementName.Use, useSpec, root)
        {

        }
        internal VgVisualElement HRefSvgRenderElement { get; set; }


        public override void Paint(VgPaintArgs vgPainterArgs)
        {

            Painter p = vgPainterArgs.P;
            SvgUseSpec useSpec = (SvgUseSpec)_visualSpec;
            //
            ICoordTransformer current_tx = vgPainterArgs._currentTx;

            Color color = p.FillColor;
            double strokeW = p.StrokeWidth;
            Color strokeColor = p.StrokeColor;

            if (current_tx != null)
            {
                //*** IMPORTANT : matrix transform order !***
                //TODO:
                //in this version, assume X number is pixel => not always correct
                //please use this => LayoutFarm.WebDom.Parser.CssValueParser.ConvertToPx() instead
                //
                vgPainterArgs._currentTx = Affine.NewTranslation(useSpec.X.Number, useSpec.Y.Number).MultiplyWith(current_tx);
            }
            else
            {
                vgPainterArgs._currentTx = Affine.NewTranslation(useSpec.X.Number, useSpec.Y.Number);
            }

            if (_visualSpec.HasFillColor)
            {
                p.FillColor = _visualSpec.FillColor;
            }

            if (_visualSpec.HasStrokeColor)
            {
                //temp fix
                p.StrokeColor = _visualSpec.StrokeColor;
            }
            else
            {

            }
            if (_visualSpec.HasOpacity)
            {

            }
            if (_visualSpec.HasStrokeWidth)
            {
                //temp fix
                p.StrokeWidth = _visualSpec.StrokeWidth.Number;
            }
            else
            {

            }

            HRefSvgRenderElement.Paint(vgPainterArgs);

            //restore
            p.FillColor = color;
            p.StrokeColor = strokeColor;
            p.StrokeWidth = strokeW;
            //
            vgPainterArgs._currentTx = current_tx;
        }
        public override void Accept(VgVisitorArgs visitor)
        {

            SvgUseSpec useSpec = (SvgUseSpec)_visualSpec;
            ICoordTransformer current_tx = visitor._currentTx;

            if (current_tx != null)
            {
                //*** IMPORTANT : matrix transform order !***           
                visitor._currentTx = Affine.NewTranslation(useSpec.X.Number, useSpec.Y.Number).MultiplyWith(current_tx);
            }
            else
            {
                visitor._currentTx = Affine.NewTranslation(useSpec.X.Number, useSpec.Y.Number);
            }


            HRefSvgRenderElement.Accept(visitor);

            visitor._currentTx = current_tx;
            //base.Walk(vgPainterArgs);
        }
    }



    public class VgVisualForeignNode : VgVisualElementBase
    {
        public object _foriegnNode;

        public VgVisualForeignNode() { }

        public override VgVisualElementBase Clone()
        {
            return new VgVisualForeignNode { _foriegnNode = _foriegnNode };
        }
        public override WellknownSvgElementName ElemName => WellknownSvgElementName.ForeignNode;
    }

    public class VgVisualElement : VgVisualElementBase, IDisposable
    {

        bool _handleBitmapSnapshotAsOwner;
        //-------------------------
        List<VgVisualElementBase> _childNodes = null;
        WellknownSvgElementName _wellknownName;
        //-------------------------
        float _latestStrokeW; //latest caching stroke width, if strokeW changed then we need to regenerate _strokeVxs
        VertexStore _strokeVxs; //caching stroke vxs 
        //-------------------------

        object _controller;

        internal SvgVisualSpec _visualSpec;
        internal VgPathVisualMarkers _pathMarkers;


        LayoutFarm.ImageBinder _imgBinder;
        VgVisualDoc _vgVisualDoc;

        Image _backimg;
        RectD _boundRect;
        bool _needBoundUpdate;

        internal float _imgW;
        internal float _imgH;
        internal float _imgX;
        internal float _imgY;

        VertexStore _vxsPath;
        bool _isVxsPathOwner;

        public VgVisualElement(WellknownSvgElementName wellknownName,
            SvgVisualSpec visualSpec,
            VgVisualDoc vgVisualDoc)
        {
            //we can create visual element without its DOM
            _needBoundUpdate = true;
            _wellknownName = wellknownName;
            _visualSpec = visualSpec;
            _vgVisualDoc = vgVisualDoc;
        }

        //
        public VgVisualDoc VgVisualDoc => _vgVisualDoc;

        public SvgElement DomElem { get; set; }//*** its dom element(optional)
        public override WellknownSvgElementName ElemName => _wellknownName;
        //
        public void SetController(object o) => _controller = o;
        public object GetController() => _controller;
        //
        public ICoordTransformer CoordTx { get; set; }

        public VertexStore VxsPath
        {
            get => _vxsPath;
            set
            {
#if DEBUG
                if (value.IsShared)
                {
                    throw new NotSupportedException("can't not store shared vxs");
                }
#endif
                ReleaseVxsPath(); //release old _vxsPath

                _isVxsPathOwner = true;//
                _vxsPath = value;
            }
        }

        void ReleaseVxsPath()
        {
            if (_vxsPath != null)
            {
                if (_isVxsPathOwner)
                {
                    _vxsPath.Dispose();
                }
                //
                _vxsPath = null;
            }
        }
        public void Dispose()
        {
            ReleaseVxsPath();
        }

        public LayoutFarm.ImageBinder ImageBinder
        {
            get
            {
                return _imgBinder;
            }
            set
            {
                _imgBinder = value;
                if (value != null)
                {
                    //bind image change event
                    if (_imgW == 0)
                    {
                        _imgW = value.Width;
                    }
                    if (_imgH == 0)
                    {
                        _imgH = value.Height;
                    }
                    value.ImageChanged += (s, e) => _vgVisualDoc.Invalidate(this);
                }
            }
        }

        public void HitTest(float x, float y, Action<VgVisualElement, float, float, VertexStore> onHitSvg)
        {
            using (VgVistorArgsPool.Borrow(out VgVisitorArgs visitor))
            {
                visitor.VgElemenVisitHandler = (vxs, args) =>
                {
                    if (args.Current != null &&
                       PixelFarm.CpuBlit.VertexProcessing.VertexHitTester.IsPointInVxs(vxs, x, y))
                    {
                        //add actual transform vxs ... 
                        onHitSvg(args.Current, x, y, vxs);
                    }
                };
                this.Accept(visitor);
            }
        }
        public bool HitTest(VgHitChain hitChain)
        {
            using (VgVistorArgsPool.Borrow(out VgVisitorArgs paintArgs))
            {
                paintArgs.VgElemenVisitHandler = (vxs, args) =>
                {

                    if (args.Current != null &&
                       PixelFarm.CpuBlit.VertexProcessing.VertexHitTester.IsPointInVxs(vxs, hitChain.X, hitChain.Y))
                    {
                        //add actual transform vxs ... 
                        hitChain.AddHit(args.Current,
                        hitChain.X,
                        hitChain.Y,
                        hitChain.MakeCopyOfHitVxs ? vxs.CreateTrim() : null);
                    }
                };
                this.Accept(paintArgs);
                return hitChain.Count > 0;
            }
        }

        public override VgVisualElementBase Clone()
        {
            VgVisualElement clone = new VgVisualElement(_wellknownName, _visualSpec, _vgVisualDoc);
            clone.DomElem = this.DomElem;

            if (VxsPath != null)
            {
                clone.VxsPath = VxsPath.CreateTrim();
            }
            //
            if (_childNodes != null)
            {
                //deep clone
                int j = _childNodes.Count;
                List<VgVisualElementBase> cloneChildNodes = new List<VgVisualElementBase>(j);
                for (int i = 0; i < j; ++i)
                {
                    cloneChildNodes.Add(_childNodes[i].Clone());
                }
                clone._childNodes = cloneChildNodes;
            }
            //assign the same controller
            clone._controller = _controller;
            return clone;
        }



        static ICoordTransformer ResolveTransformation(SvgTransform transformation)
        {
            if (transformation.ResolvedICoordTransformer != null)
            {
                return transformation.ResolvedICoordTransformer;
            }
            //
            switch (transformation.TransformKind)
            {
                default: throw new NotSupportedException();

                case SvgTransformKind.Matrix:

                    SvgTransformMatrix matrixTx = (SvgTransformMatrix)transformation;
                    float[] elems = matrixTx.Elements;
                    return transformation.ResolvedICoordTransformer = new Affine(
                         elems[0], elems[1],
                         elems[2], elems[3],
                         elems[4], elems[5]);
                case SvgTransformKind.Rotation:
                    SvgRotate rotateTx = (SvgRotate)transformation;
                    if (rotateTx.SpecificRotationCenter)
                    {
                        //https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/transform
                        //svg's rotation=> angle in degree, so convert to rad ...

                        //translate to center 
                        //rotate and the translate back
                        return transformation.ResolvedICoordTransformer = Affine.New(
                                PixelFarm.CpuBlit.VertexProcessing.AffinePlan.Translate(-rotateTx.CenterX, -rotateTx.CenterY),
                                PixelFarm.CpuBlit.VertexProcessing.AffinePlan.Rotate(AggMath.deg2rad(rotateTx.Angle)),
                                PixelFarm.CpuBlit.VertexProcessing.AffinePlan.Translate(rotateTx.CenterX, rotateTx.CenterY)
                            );
                    }
                    else
                    {
                        return transformation.ResolvedICoordTransformer = PixelFarm.CpuBlit.VertexProcessing.Affine.NewRotation(AggMath.deg2rad(rotateTx.Angle));
                    }
                case SvgTransformKind.Scale:
                    SvgScale scaleTx = (SvgScale)transformation;
                    return transformation.ResolvedICoordTransformer = PixelFarm.CpuBlit.VertexProcessing.Affine.NewScaling(scaleTx.X, scaleTx.Y);
                case SvgTransformKind.Shear:
                    SvgShear shearTx = (SvgShear)transformation;
                    return transformation.ResolvedICoordTransformer = PixelFarm.CpuBlit.VertexProcessing.Affine.NewSkewing(shearTx.X, shearTx.Y);
                case SvgTransformKind.Translation:
                    SvgTranslate translateTx = (SvgTranslate)transformation;
                    return transformation.ResolvedICoordTransformer = PixelFarm.CpuBlit.VertexProcessing.Affine.NewTranslation(translateTx.X, translateTx.Y);
            }
        }


        public override void Accept(VgVisitorArgs visitor)
        {
            if (visitor.VgElemenVisitHandler == null)
            {
                return;
            }

            //----------------------------------------------------
            ICoordTransformer prevTx = visitor._currentTx; //backup
            ICoordTransformer currentTx = visitor._currentTx;

            if (_visualSpec != null)
            {
                //has visual spec
                if (_visualSpec.Transform != null)
                {
                    ICoordTransformer latest = ResolveTransformation(_visualSpec.Transform);
                    if (currentTx != null)
                    {
                        //*** IMPORTANT : matrix transform order !***                         
                        currentTx = latest.MultiplyWith(visitor._currentTx);
                    }
                    else
                    {
                        currentTx = latest;
                    }

                    visitor._currentTx = currentTx;
                }

                if (!_visualSpec.StrokeWidth.IsEmptyOrAuto &&
                    _visualSpec.StrokeWidth.Number > 1)
                {
                    visitor.TempCurrentStrokeWidth = _visualSpec.StrokeWidth.Number;
                }

                //***SKIP CLIPPING***
                //if (_visualSpec.ResolvedClipPath != null)
                //{
                //    //clip-path
                //    hasClip = true;

                //    SvgRenderElement clipPath = (SvgRenderElement)_visualSpec.ResolvedClipPath;
                //    VertexStore clipVxs = ((SvgRenderElement)clipPath.GetChildNode(0))._vxsPath;
                //    //----------
                //    //for optimization check if clip path is Rect
                //    //if yes => do simple rect clip  
                //    if (currentTx != null)
                //    {
                //        //have some tx
                //        using (VxsContext.Temp(out var v1))
                //        {
                //            currentTx.TransformToVxs(clipVxs, v1);
                //            p.SetClipRgn(v1);
                //        }
                //    }
                //    else
                //    {
                //        p.SetClipRgn(clipVxs);
                //    }
                //}
                //***SKIP CLIPPING***
            }


            switch (this.ElemName)
            {
                default:
                    //unknown
                    break;
                case WellknownSvgElementName.Text:
                    break;

                case WellknownSvgElementName.RootSvg:
                case WellknownSvgElementName.Svg:
                    break;
                case WellknownSvgElementName.Mask:
                    break;
                case WellknownSvgElementName.Image:
                    {
                        if (VxsPath == null)
                        {
                            //create rect path around img

                            using (VectorToolBox.Borrow(out SimpleRect ss))
                            using (VxsTemp.Borrow(out VertexStore vxs))
                            {
                                SvgImageSpec imgSpec = (SvgImageSpec)_visualSpec;
                                ss.SetRect(0, imgSpec.Height.Number, imgSpec.Width.Number, 0);
                                ss.MakeVxs(vxs);
                                //
                                this.VxsPath = vxs.CreateTrim();
                            }

                        }
                        goto case WellknownSvgElementName.Rect;
                    }

                case WellknownSvgElementName.Line:
                case WellknownSvgElementName.Path:
                case WellknownSvgElementName.Ellipse:
                case WellknownSvgElementName.Circle:
                case WellknownSvgElementName.Polygon:
                case WellknownSvgElementName.Polyline:
                case WellknownSvgElementName.Rect:

                    {
                        //render with rect spec 

                        if (currentTx == null)
                        {
                            visitor.Current = this;
                            visitor.VgElemenVisitHandler(VxsPath, visitor);
                            visitor.Current = null;
                        }
                        else
                        {
                            //have some tx
                            using (VxsTemp.Borrow(out var v1))
                            {
                                currentTx.TransformToVxs(VxsPath, v1);
                                visitor.Current = this;
                                visitor.VgElemenVisitHandler(v1, visitor);
                                visitor.Current = null;
                            }
                        }
                        //------
                        if (_pathMarkers != null)
                        {
                            //render each marker
                            if (_pathMarkers.StartMarker != null)
                            {
                                //draw this
                                //*** IMPORTANT : matrix transform order !***                 
                                //*** IMPORTANT : matrix transform order !***    
                                int cc = _pathMarkers.StartMarker.ChildCount;
                                for (int i = 0; i < cc; ++i)
                                {
                                    //temp fix
                                    //set offset   
                                    if (_pathMarkers.StartMarkerAffine != null)
                                    {
                                        visitor._currentTx = Affine.NewTranslation(
                                            _pathMarkers.StartMarkerPos.X,
                                            _pathMarkers.StartMarkerPos.Y) * _pathMarkers.StartMarkerAffine;
                                    }

                                    _pathMarkers.StartMarker.GetChildNode(i).Accept(visitor);
                                }
                                visitor._currentTx = currentTx;

                            }
                            if (_pathMarkers.EndMarker != null)
                            {
                                //draw this 
                                int cc = _pathMarkers.EndMarker.ChildCount;
                                for (int i = 0; i < cc; ++i)
                                {
                                    //temp fix 
                                    if (_pathMarkers.EndMarkerAffine != null)
                                    {
                                        visitor._currentTx = Affine.NewTranslation(
                                            _pathMarkers.EndMarkerPos.X, _pathMarkers.EndMarkerPos.Y) * _pathMarkers.EndMarkerAffine;
                                    }
                                    _pathMarkers.EndMarker.GetChildNode(i).Accept(visitor);
                                }
                                visitor._currentTx = currentTx;
                            }
                        }
                    }
                    break;
            }

            //-------------------------------------------------------
            int childCount = this.ChildCount;
            for (int i = 0; i < childCount; ++i)
            {
                var node = GetChildNode(i) as VgVisualElement;
                if (node != null)
                {
                    node.Accept(visitor);
                }
            }


            visitor._currentTx = prevTx;
            //***SKIP CLIPPING***
            //if (hasClip)
            //{
            //    p.SetClipRgn(null);
            //}
            //***SKIP CLIPPING***
        }

        public SvgVisualSpec VisualSpec => _visualSpec;

        ICoordTransformer ConvertToICoordTransformer(SvgTransform svgTransform)
        {
            switch (svgTransform.TransformKind)
            {
                case SvgTransformKind.Matrix:
                    {
                        //   public Affine(
                        //double v0_sx, double v1_shy,
                        //double v2_shx, double v3_sy,
                        //double v4_tx, double v5_ty)
                        //            public SvgTransformMatrix(
                        //float sx, float shx,
                        //float shy, float sy,
                        //float tx, float ty
                        //)

                        SvgTransformMatrix svgTxMatrix = (SvgTransformMatrix)svgTransform;
                        float[] svgMatrixElem = svgTxMatrix.Elements;

                        return new Affine(
                            svgMatrixElem[0], svgMatrixElem[1],
                            svgMatrixElem[2], svgMatrixElem[3],
                            svgMatrixElem[4], svgMatrixElem[5]
                            );
                    }
                default:
                    return null;
            }
        }
        //---------------------------
        public override void Paint(VgPaintArgs vgPainterArgs)
        {

            //save
            Painter p = vgPainterArgs.P;

            Color color = p.FillColor;
            bool restoreFillColor;
            if (p.CurrentBrush != null && p.CurrentBrush.BrushKind != BrushKind.Solid)
            {
                restoreFillColor = false;
            }
            else
            {
                restoreFillColor = true;
            }


            double strokeW = p.StrokeWidth;
            Color strokeColor = p.StrokeColor;
            RequestFont currentFont = p.CurrentFont;
            PaintFx.Effects.CpuBlitImgFilter imgFilter = null;
            VgVisualElement filterElem = null;
            ICoordTransformer prevTx = vgPainterArgs._currentTx; //backup
            ICoordTransformer currentTx = vgPainterArgs._currentTx;

            bool hasClip = false;
            bool newFontReq = false;
            bool useGradientColor = false;
            bool userGradientWithOpacity = false;


            float prevOpacity = vgPainterArgs.Opacity;
            bool enableMaskMode = false;

            if (_visualSpec != null)
            {
                if (_visualSpec.Transform != null)
                {
                    ICoordTransformer latest = ResolveTransformation(_visualSpec.Transform);
                    if (currentTx != null)
                    {
                        //*** IMPORTANT : matrix transform order !***                         
                        currentTx = latest.MultiplyWith(vgPainterArgs._currentTx);
                    }
                    else
                    {
                        currentTx = latest;
                    }
                    vgPainterArgs._currentTx = currentTx;
                }
                //apply this to current tx 

                if (_visualSpec.MaskPathLink != null)
                {
                    //apply mask path 
                    if (_visualSpec.ResolvedMask == null)
                    {

                    }

                    if (_visualSpec.ResolvedMask != null)
                    {
                        //check if we have a cache mask or not
                        //if not we need to create a mask

                        VgVisualElement maskElem = (VgVisualElement)_visualSpec.ResolvedMask;
                        MemBitmap maskBmp = (MemBitmap)maskElem.BackingImage;
                        SvgMaskSpec maskSpec = maskElem.VisualSpec as SvgMaskSpec;
                        if (maskBmp == null)
                        {
                            //create a mask bitmap
                            //TODO: review this num conversion
                            maskBmp = new MemBitmap((int)maskSpec.Width.Number, (int)maskSpec.Height.Number);
                            //use software renderer for mask-bitmap
                            using (AggPainterPool.Borrow(maskBmp, out AggPainter painter))
                            using (VgPaintArgsPool.Borrow(painter, out VgPaintArgs paintArgs2))
                            {
                                painter.FillColor = Color.Black;
                                painter.Clear(Color.White);
                                paintArgs2.MaskMode = true;
                                maskElem.Paint(paintArgs2);
                                paintArgs2.MaskMode = false;

                            }
                            //our mask  need a little swap (TODO: review this, this is temp fix)
                            //maskBmp.SaveImage("d:\\WImageTest\\mask01.png");
                            // maskBmp.InvertColor();
                            maskElem.SetBitmapSnapshot(maskBmp, true);
                            // maskBmp.SaveImage("d:\\WImageTest\\mask01_inverted.png");
                        }


                        if (maskElem.BackingImage != null)
                        {
                            //use this as mask bitmap
                            vgPainterArgs.P.TargetBuffer = TargetBuffer.MaskBuffer;
                            vgPainterArgs.P.Clear(Color.Black);
                            vgPainterArgs.P.DrawImage(maskElem.BackingImage, maskSpec.X.Number, maskSpec.Y.Number);
                            vgPainterArgs.P.TargetBuffer = TargetBuffer.ColorBuffer;
                            enableMaskMode = vgPainterArgs.P.EnableMask = true;

                        }
                    }
                }

                if (_visualSpec.HasFillColor)
                {
                    if (_visualSpec.ResolvedFillBrush != null)
                    {
                        GeometryGradientBrush geoBrush = _visualSpec.ResolvedFillBrush as GeometryGradientBrush;
                        if (geoBrush == null)
                        {
                            VgVisualElement vgVisualElem = _visualSpec.ResolvedFillBrush as VgVisualElement;
                            if (vgVisualElem != null)
                            {
                                if (vgVisualElem.VisualSpec is SvgRadialGradientSpec)
                                {
                                    //TODO: review here
                                    //we should resolve this in some state before Paint
                                    SvgRadialGradientSpec svgRadialGrdSpec = (SvgRadialGradientSpec)vgVisualElem.VisualSpec;
                                    int stopListCount = svgRadialGrdSpec.StopList.Count;
                                    ColorStop[] colorStops = new ColorStop[stopListCount];
                                    for (int i = 0; i < stopListCount; ++i)
                                    {
                                        SvgColorStopSpec stop = svgRadialGrdSpec.StopList[i];
                                        if (stop.StopOpacity < 1)
                                        {
                                            Color stopColor = stop.StopColor.NewFromChangeCoverage((int)(stop.StopOpacity * 255));
                                            colorStops[i] = new ColorStop(stop.Offset.Number, stopColor);
                                        }
                                        else
                                        {
                                            colorStops[i] = new ColorStop(stop.Offset.Number, stop.StopColor);
                                        }
                                    }

                                    geoBrush = new RadialGradientBrush(
                                      new PointF(svgRadialGrdSpec.CX.Number, svgRadialGrdSpec.CY.Number),
                                      svgRadialGrdSpec.R.Number,
                                      colorStops);

                                    if (svgRadialGrdSpec.Transform != null)
                                    {
                                        geoBrush.CoordTransformer = ConvertToICoordTransformer(svgRadialGrdSpec.Transform);
                                    }


                                    _visualSpec.ResolvedFillBrush = geoBrush;
                                }
                                else if (vgVisualElem.VisualSpec is SvgLinearGradientSpec)
                                {
                                    SvgLinearGradientSpec linearGrSpec = (SvgLinearGradientSpec)vgVisualElem.VisualSpec;
                                    if (linearGrSpec.Transform != null)
                                    {

                                    }
                                    if (linearGrSpec.StopList != null)
                                    {
                                        int stopListCount = linearGrSpec.StopList.Count;
                                        //... 
                                        if (stopListCount > 1)
                                        {
                                            ColorStop[] colorStops = new ColorStop[stopListCount];
                                            for (int i = 0; i < stopListCount; ++i)
                                            {
                                                SvgColorStopSpec stop = linearGrSpec.StopList[i];
                                                if (stop.StopOpacity < 1)
                                                {
                                                    Color stopColor = stop.StopColor.NewFromChangeCoverage((int)(stop.StopOpacity * 255));
                                                    colorStops[i] = new ColorStop(stop.Offset.Number, stopColor);
                                                }
                                                else
                                                {
                                                    colorStops[i] = new ColorStop(stop.Offset.Number, stop.StopColor);
                                                }

                                            }

                                            LinearGradientBrush linearGr = new LinearGradientBrush(
                                              new PointF(linearGrSpec.X1.Number, linearGrSpec.Y1.Number),
                                              new PointF(linearGrSpec.X2.Number, linearGrSpec.Y2.Number), colorStops);


                                            geoBrush = linearGr;
                                            _visualSpec.ResolvedFillBrush = geoBrush;
                                        }
                                    }
                                }
                                else
                                {

                                }
                            }
                        }
                        if (geoBrush != null)
                        {
                            useGradientColor = true;
                        }
                        p.CurrentBrush = geoBrush;

                    }
                    else
                    {
                        p.FillColor = _visualSpec.FillColor;
                    }
                }

                if (_visualSpec.HasStrokeColor)
                {
                    //temp fix
                    p.StrokeColor = _visualSpec.StrokeColor;
                }
                else
                {

                }

                if (_visualSpec.HasStrokeWidth)
                {
                    //temp fix
                    p.StrokeWidth = _visualSpec.StrokeWidth.Number;
                }
                else
                {

                }

                if (_visualSpec.HasOpacity)
                {
                    vgPainterArgs.Opacity = prevOpacity * _visualSpec.Opacity;
                }

                if (vgPainterArgs.HasOpacity)
                {
                    if (useGradientColor)
                    {
                        userGradientWithOpacity = true;
                        p.FillOpacity = vgPainterArgs.Opacity;
                    }
                    else
                    {
                        p.FillColor = p.FillColor.NewFromChangeCoverage((int)(vgPainterArgs.Opacity * 255));
                    }

                }

                if (_visualSpec.FilterPathLink != null)
                {
                    //resolve filter
                    filterElem = _visualSpec.ResolvedFilter as VgVisualElement;
                    //TODO: implement filter
                    //TODO: how to get and apply children nodes
                    VgVisualElement content1 = (VgVisualElement)filterElem.GetChildNode(0);
                    SvgFeColorMatrixSpec colorMatSpec = content1.VisualSpec as SvgFeColorMatrixSpec;
                    imgFilter = colorMatSpec.ResolvedFilter as PaintFx.Effects.ImgFilterSvgFeColorMatrix;
                }


                if (_visualSpec.ResolvedClipPath != null)
                {
                    //clip-path
                    hasClip = true;
                    VgVisualElement clipPath = (VgVisualElement)_visualSpec.ResolvedClipPath;
                    //inside clipPath definition
                    //may be local definition or 'use' element

                    //TODO: review here
                    VgVisualElement clipPathChild = (VgVisualElement)clipPath.GetChildNode(0);
                    VertexStore clipVxs = null;
                    switch (clipPathChild.ElemName)
                    {
                        case WellknownSvgElementName.Use:
                            {
                                //TODO: review here
                                //
                                VgUseVisualElement useVisualElem = (VgUseVisualElement)clipPathChild;
                                clipVxs = useVisualElem.HRefSvgRenderElement.VxsPath;
                            }
                            break;
                        case WellknownSvgElementName.ClipPath:

                            break;
                        default:
                            clipVxs = clipPathChild.VxsPath;
                            break;
                    }

                    //----------
                    //for optimization check if clip path is Rect
                    //if yes => do simple rect clip 
                    if (currentTx != null)
                    {
                        //have some tx
                        using (VxsTemp.Borrow(out var v1))
                        {
                            currentTx.TransformToVxs(clipVxs, v1);
                            p.SetClipRgn(v1);
                        }
                    }
                    else
                    {
                        p.SetClipRgn(clipVxs);
                    }
                }
            }


            switch (this.ElemName)
            {
                default:
                    //unknown
                    break;
                case WellknownSvgElementName.Group:
                case WellknownSvgElementName.RootSvg:
                case WellknownSvgElementName.Svg:
                    break;
                case WellknownSvgElementName.Mask:
                    {
                        if (!vgPainterArgs.MaskMode)
                        {
                            return;
                        }
                    }
                    break;
                case WellknownSvgElementName.Image:
                    {
                        SvgImageSpec imgSpec = _visualSpec as SvgImageSpec;
                        //request from resource 
                        bool isOK = true;

                        if (this.ImageBinder == null)
                        {
                            isOK = false;
                            if (imgSpec.ImageSrc != null)
                            {
                                //create new 
                                this.ImageBinder = new LayoutFarm.ImageBinder(imgSpec.ImageSrc);
                                isOK = true;
                            }
                        }

                        if (!isOK)
                        {
                            return;
                        }

                        bool tryLoadOnce = false;
                        EVAL_STATE:
                        switch (this.ImageBinder.State)
                        {
                            case LayoutFarm.BinderState.Unload:
                                if (!tryLoadOnce)
                                {
                                    tryLoadOnce = true;

                                    _vgVisualDoc.RequestImageAsync(this.ImageBinder, this, this);
                                    goto EVAL_STATE;
                                }
                                break;
                            case LayoutFarm.BinderState.Loading:
                                break;
                            //
                            case LayoutFarm.BinderState.Loaded:
                                {
                                    //check if we need scale or not

                                    Image img = this.ImageBinder.LocalImage;

                                    if (currentTx != null)
                                    {

                                        if (_imgW == 0 || _imgH == 0)
                                        {
                                            //only X,and Y
                                            RenderQuality prevQ = p.RenderQuality;
                                            //p.RenderQuality = RenderQuality.Fast;
                                            p.DrawImage(this.ImageBinder.LocalImage, _imgX, _imgY, currentTx);
                                            p.RenderQuality = prevQ;
                                        }
                                        else if (_imgW == img.Width && _imgH == img.Height)
                                        {
                                            RenderQuality prevQ = p.RenderQuality;
                                            //p.RenderQuality = RenderQuality.Fast;
                                            p.DrawImage(this.ImageBinder.LocalImage, _imgX, _imgY, currentTx);
                                            p.RenderQuality = prevQ;
                                        }
                                        else
                                        {

                                            RenderQuality prevQ = p.RenderQuality;
                                            //p.RenderQuality = RenderQuality.Fast;
                                            p.DrawImage(this.ImageBinder.LocalImage, _imgX, _imgY, currentTx);
                                            p.RenderQuality = prevQ;
                                        }


                                    }
                                    else
                                    {
                                        if (_imgW == 0 || _imgH == 0)
                                        {
                                            //only X,and Y
                                            RenderQuality prevQ = p.RenderQuality;
                                            p.RenderQuality = RenderQuality.Fast;
                                            p.DrawImage(this.ImageBinder.LocalImage, _imgX, _imgY);
                                            p.RenderQuality = prevQ;
                                        }
                                        else if (_imgW == img.Width && _imgH == img.Height)
                                        {
                                            RenderQuality prevQ = p.RenderQuality;
                                            p.RenderQuality = RenderQuality.Fast;
                                            p.DrawImage(this.ImageBinder.LocalImage, _imgX, _imgY);
                                            p.RenderQuality = prevQ;
                                        }
                                        else
                                        {

                                            RenderQuality prevQ = p.RenderQuality;
                                            p.RenderQuality = RenderQuality.Fast;
                                            p.DrawImage(this.ImageBinder.LocalImage, _imgX, _imgY);

                                            p.RenderQuality = prevQ;
                                        }
                                    }

                                }
                                break;

                        }

                    }
                    break;
                case WellknownSvgElementName.Text:
                    {
                        //TODO: review here
                        //temp fix 
                        SvgTextSpec textSpec = _visualSpec as SvgTextSpec;
                        if (textSpec != null)
                        {
                            Color prevColor = p.FillColor;
                            if (textSpec.HasFillColor)
                            {
                                p.FillColor = textSpec.FillColor;
                            }
                            if (!textSpec.FontSize.IsEmpty && textSpec.FontFace != null)
                            {
                                //TODO: review this with CssValue Parser again
                                //check if input text size is in point or pixel
                                if (textSpec.FontSize.UnitOrNames == LayoutFarm.Css.CssUnitOrNames.Points)
                                {
                                    p.CurrentFont = new RequestFont(
                                      textSpec.FontFace,
                                      textSpec.FontSize.Number);
                                }
                                else
                                {
                                    //assum pixel unit , so we convert it to point
                                    p.CurrentFont = new RequestFont(
                                      textSpec.FontFace,
                                    LayoutFarm.WebDom.Parser.CssValueParser.ConvPixelsToPoints(textSpec.FontSize.Number));
                                }
                                newFontReq = true;
                            }
                            else if (textSpec.FontFace != null)
                            {
                                if (textSpec.FontSize.UnitOrNames == LayoutFarm.Css.CssUnitOrNames.Points)
                                {
                                    p.CurrentFont = new RequestFont(
                                      textSpec.FontFace,
                                      textSpec.FontSize.Number);
                                }
                                else
                                {
                                    //assum pixel unit , so we convert it to point
                                    p.CurrentFont = new RequestFont(
                                      textSpec.FontFace,
                                     LayoutFarm.WebDom.Parser.CssValueParser.ConvPixelsToPoints(textSpec.FontSize.Number));
                                }
                                newFontReq = true;
                            }
                            else if (!textSpec.FontSize.IsEmpty)
                            {
                                p.CurrentFont = new RequestFont(
                                     currentFont.Name,
                                     textSpec.FontSize.Number); //TODO: number, size in pts vs in px
                                newFontReq = true;
                            }

                            p.FillColor = Color.Black;

                            double pos_x = textSpec.ActualX;
                            double pos_y = textSpec.ActualY;
                            if (currentTx != null)
                            {
                                currentTx.Transform(ref pos_x, ref pos_y);
                            }
                            p.DrawString(textSpec.TextContent, (float)pos_x, (float)pos_y);
                            p.FillColor = prevColor;//restore back
                                                    //change font or not
                        }
                    }
                    break;
                case WellknownSvgElementName.Line:
                case WellknownSvgElementName.Path:
                case WellknownSvgElementName.Ellipse:
                case WellknownSvgElementName.Circle:
                case WellknownSvgElementName.Polygon:
                case WellknownSvgElementName.Polyline:
                case WellknownSvgElementName.Rect:
                case WellknownSvgElementName.Marker:
                    {
                        //render with rect spec  
                        if (currentTx == null)
                        {
                            //no transform
                            if (vgPainterArgs.PaintVisitHandler == null)
                            {
                                if (useGradientColor)
                                {
                                    p.Fill(VxsPath);
                                }
                                else if (p.FillColor.A > 0)
                                {
                                    p.Fill(VxsPath);
                                }
                                //to draw stroke
                                //stroke width must > 0 and stroke-color must not be transparent color
                                if (p.StrokeWidth > 0 && p.StrokeColor.A > 0)
                                {
                                    //has specific stroke color   
                                    //temp1
                                    //if (p.LineRenderingTech == LineRenderingTechnique.OutlineAARenderer)
                                    //{
                                    //    //TODO: review here again
                                    //    p.Draw(new VertexStoreSnap(_vxsPath.Vxs), p.StrokeColor);
                                    //}
                                    //else
                                    //{ 
                                    //check if we need to create a new stroke or not
                                    if (_strokeVxs == null || _latestStrokeW != (float)p.StrokeWidth)
                                    {
                                        //TODO: review here again***
                                        //vxs caching 
                                        _latestStrokeW = (float)p.StrokeWidth;

                                        using (VxsTemp.Borrow(out var v1))
                                        using (VectorToolBox.Borrow(out Stroke stroke))
                                        {
                                            stroke.Width = _latestStrokeW;
                                            stroke.MakeVxs(VxsPath, v1);
                                            _strokeVxs = v1.CreateTrim();
                                        }
                                    }
                                    p.Fill(_strokeVxs, p.StrokeColor);
                                }
                            }
                            else
                            {
                                vgPainterArgs.PaintVisitHandler(VxsPath, vgPainterArgs);
                            }


                            //----------------------------------------------------------------------
                            if (_pathMarkers != null)
                            {
                                //render each marker
                                if (_pathMarkers.StartMarker != null)
                                {
                                    //draw this
                                    //*** IMPORTANT : matrix transform order !***                 
                                    //*** IMPORTANT : matrix transform order !***    
                                    //Color prevFillColor = p.FillColor;
                                    //p.FillColor = Color.Red;
                                    int cc = _pathMarkers.StartMarker.ChildCount;
                                    for (int i = 0; i < cc; ++i)
                                    {
                                        //temp fix 
                                        if (_pathMarkers.StartMarkerAffine != null)
                                        {
                                            vgPainterArgs._currentTx = _pathMarkers.StartMarkerAffine * Affine.NewTranslation(_pathMarkers.StartMarkerPos.X, _pathMarkers.StartMarkerPos.Y);
                                        }
                                        else
                                        {
                                            vgPainterArgs._currentTx = Affine.NewTranslation(_pathMarkers.StartMarkerPos.X, _pathMarkers.StartMarkerPos.Y);
                                        }
                                        _pathMarkers.StartMarker.GetChildNode(i).Paint(vgPainterArgs);

                                    }
                                    //p.FillColor = prevFillColor;
                                    vgPainterArgs._currentTx = currentTx;
                                }

                                if (_pathMarkers.MidMarker != null)
                                {
                                    //draw this
                                    //vgPainterArgs._currentTx = Affine.IdentityMatrix;// _pathMarkers.StartMarkerAffine;
                                    //Color prevFillColor = p.FillColor;
                                    //p.FillColor = Color.Red;

                                    PointF[] allPoints = _pathMarkers.AllPoints;
                                    int allPointCount = allPoints.Length;
                                    if (allPointCount > 2)
                                    {
                                        //draw between first and last node

                                        for (int mm = 1; mm < allPointCount - 1; ++mm)
                                        {
                                            int cc = _pathMarkers.MidMarker.ChildCount;
                                            PointF btwPoint = allPoints[mm];
                                            for (int i = 0; i < cc; ++i)
                                            {
                                                //temp fix   

                                                if (_pathMarkers.MidMarkerAffine != null)
                                                {
                                                    vgPainterArgs._currentTx = _pathMarkers.MidMarkerAffine * Affine.NewTranslation(btwPoint.X, btwPoint.Y);
                                                }
                                                else
                                                {
                                                    vgPainterArgs._currentTx = Affine.NewTranslation(btwPoint.X, btwPoint.Y);
                                                }

                                                _pathMarkers.MidMarker.GetChildNode(i).Paint(vgPainterArgs);
                                            }
                                        }
                                    }
                                    //p.FillColor = prevFillColor;
                                    vgPainterArgs._currentTx = currentTx;
                                }

                                if (_pathMarkers.EndMarker != null)
                                {
                                    //draw this
                                    //vgPainterArgs._currentTx = Affine.IdentityMatrix;// _pathMarkers.StartMarkerAffine;
                                    //Color prevFillColor = p.FillColor;
                                    //p.FillColor = Color.Red;
                                    int cc = _pathMarkers.EndMarker.ChildCount;
                                    for (int i = 0; i < cc; ++i)
                                    {
                                        //temp fix 
                                        if (_pathMarkers.EndMarkerAffine != null)
                                        {
                                            vgPainterArgs._currentTx = _pathMarkers.EndMarkerAffine * Affine.NewTranslation(_pathMarkers.EndMarkerPos.X, _pathMarkers.EndMarkerPos.Y);
                                        }
                                        else
                                        {
                                            vgPainterArgs._currentTx = Affine.NewTranslation(_pathMarkers.EndMarkerPos.X, _pathMarkers.EndMarkerPos.Y);
                                        }
                                        _pathMarkers.EndMarker.GetChildNode(i).Paint(vgPainterArgs);
                                    }
                                    //p.FillColor = prevFillColor;
                                    vgPainterArgs._currentTx = currentTx;
                                }
                            }
                        }
                        else
                        {
                            //have some tx
                            using (VxsTemp.Borrow(out var v1))
                            {
                                currentTx.TransformToVxs(VxsPath, v1);

                                if (vgPainterArgs.PaintVisitHandler == null)
                                {
                                    if (p.FillColor.A > 0)
                                    {
                                        p.Fill(v1);
                                    }

                                    //to draw stroke
                                    //stroke width must > 0 and stroke-color must not be transparent color 
                                    if (p.StrokeWidth > 0 && p.StrokeColor.A > 0)
                                    {
                                        p.Draw(v1);

                                    }
                                }
                                else
                                {
                                    vgPainterArgs.PaintVisitHandler(v1, vgPainterArgs);
                                }
                            }

                            if (_pathMarkers != null)
                            {
                                //render each marker
                                if (_pathMarkers.StartMarker != null)
                                {
                                    //draw this
                                    //*** IMPORTANT : matrix transform order !***                 
                                    //*** IMPORTANT : matrix transform order !***                        

                                    vgPainterArgs._currentTx = _pathMarkers.StartMarkerAffine;
                                    _pathMarkers.StartMarker.Paint(vgPainterArgs);
                                    vgPainterArgs._currentTx = currentTx;

                                }
                                if (_pathMarkers.EndMarker != null)
                                {
                                    //draw this

                                }

                            }
                        }
                    }
                    break;
            }

            //-------------------------------------------------------
            int childCount = this.ChildCount;
            for (int i = 0; i < childCount; ++i)
            {
                var node = GetChildNode(i) as VgVisualElement;
                if (node != null)
                {
                    node.Paint(vgPainterArgs);
                }
            }

            //-----------------
            //
            if (filterElem != null)
            {
                vgPainterArgs.P.ApplyFilter(imgFilter);
            }

            //restore
            if (restoreFillColor)
            {
                p.FillColor = color;
            }
            else
            {

            }
            if (userGradientWithOpacity)
            {
                p.FillOpacity = prevOpacity;
            }
            if (useGradientColor)
            {
                //store back
                p.CurrentBrush = null;
            }
            p.StrokeColor = strokeColor;
            p.StrokeWidth = strokeW;
            vgPainterArgs.Opacity = prevOpacity;
            vgPainterArgs._currentTx = prevTx;
            if (hasClip)
            {
                p.SetClipRgn(null);
            }
            if (newFontReq)
            {
                p.CurrentFont = currentFont;
            }
            if (enableMaskMode)
            {
                p.EnableMask = false;
            }
        }


        public void AddChildElement(VgVisualElementBase vgVisElem)
        {
            if (vgVisElem == null) return;
            //
#if DEBUG
            if (vgVisElem.dbugHasParent)
            {
                throw new NotSupportedException();
            }
            vgVisElem.dbugHasParent = true;
#endif
            if (_childNodes == null)
            {
                _childNodes = new List<VgVisualElementBase>();
            }
            _childNodes.Add(vgVisElem);

        }
        public int ChildCount => (_childNodes == null) ? 0 : _childNodes.Count;


        public VgVisualElementBase GetChildNode(int index) => _childNodes[index];

        public void RemoveAt(int index)
        {
            _childNodes.RemoveAt(index);
        }
        public void Clear()
        {
            if (_childNodes != null)
            {
                _childNodes.Clear();
            }
        }


        public object UserData { get; set; } //optional
        public VgDocument OwnerDocument { get; set; } //optional
        public void InvalidateBounds()
        {
            _needBoundUpdate = true;
            _boundRect = RectD.ZeroIntersection;// new RectD(this.X, this.Y, 2, 2);
        }


        public RectD GetRectBounds()
        {
            //TODO: check bounds where border-width > 1

            //***
            if (_needBoundUpdate)
            {
                using (VgVistorArgsPool.Borrow(out VgVisitorArgs paintArgs))
                {
                    //when we find bounds, lets start with  RectD.ZeroIntersectio
                    RectD rectTotal = RectD.ZeroIntersection;
                    bool evaluated = false;
                    paintArgs._currentTx = this.CoordTx;


#if DEBUG
                    //if (_coordTx != null)
                    //{ 
                    //}
#endif

                    float maxStrokeWidth = 1;
                    paintArgs.VgElemenVisitHandler = (vxs, args) =>
                    {
                        evaluated = true;//once 
                        BoundingRect.GetBoundingRect(vxs, ref rectTotal);
                        if (args.TempCurrentStrokeWidth > maxStrokeWidth)
                        {
                            maxStrokeWidth = args.TempCurrentStrokeWidth;
                        }

                    };


                    this.Accept(paintArgs);//**

                    _needBoundUpdate = false;

                    if (evaluated && maxStrokeWidth > 1)
                    {
                        float half = maxStrokeWidth / 2f;
                        rectTotal.Left -= half;
                        rectTotal.Right += half;
                        rectTotal.Top -= half;
                        rectTotal.Bottom += half;
                    }


                    return _boundRect = evaluated ? rectTotal : new RectD();
                }
            }

            return _boundRect;

        }
        public bool HasBitmapSnapshot { get; internal set; }

        public Image BackingImage => _backimg;
        public bool DisableBackingImage { get; set; }

        public void ClearBitmapSnapshot()
        {
            SetBitmapSnapshot(null, true);
        }
        public void SetBitmapSnapshot(Image img, bool handleImgAsOwner)
        {
            //
            if (_backimg != null && _handleBitmapSnapshotAsOwner)
            {
                //clear cache
                (Image.GetCacheInnerImage(_backimg) as IDisposable)?.Dispose(); //clear server side
                _backimg.Dispose();
            }
            //
            //set new value
            _backimg = img;
            _handleBitmapSnapshotAsOwner = handleImgAsOwner;
            HasBitmapSnapshot = img != null;
        }
    }





}