//MIT, 2014-present, WinterDev

using System;
using System.Collections.Generic;

using PixelFarm.Drawing;
using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.VertexProcessing;
using LayoutFarm.WebDom;


namespace PaintLab.Svg
{



    //----------------------
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


    public class VgPaintArgs : VgVisitorBase
    {
        float _opacity;
        bool _maskMode;
        internal VgPaintArgs()
        {
            Opacity = 1;
        }
        public Painter P { get; internal set; }
        public Action<VertexStore, VgPaintArgs> PaintVisitHandler;
        public bool HasOpacity => _opacity < 1;
        public float Opacity
        {
            get => _opacity;
            set
            {
                if (value < 0)
                {
                    _opacity = 0;
                }
                else if (value > 1)
                {
                    _opacity = 1;//not use this opacity
                }
                else
                {
                    _opacity = value;
                }
            }
        }
        internal override void Reset()
        {
            base.Reset();//*** reset base class fields too
            //-------
            Opacity = 2;
            P = null;
            PaintVisitHandler = null;
        }
        public bool MaskMode
        {
            get => _maskMode;
            set
            {
                _maskMode = value;
            }
        }
    }

    public static class VgPainterArgsPool
    {

        public static TempContext<VgPaintArgs> Borrow(Painter painter, out VgPaintArgs paintArgs)
        {
            if (!Temp<VgPaintArgs>.IsInit())
            {
                Temp<VgPaintArgs>.SetNewHandler(
                    () => new VgPaintArgs(),
                    p => p.Reset());//when relese back
            }

            var context = Temp<VgPaintArgs>.Borrow(out paintArgs);
            paintArgs.P = painter;
            return context;
        }
    }
    public static class VgVistorArgsPool
    {

        public static TempContext<VgVisitorArgs> Borrow(out VgVisitorArgs visitor)
        {
            if (!Temp<VgVisitorArgs>.IsInit())
            {
                Temp<VgVisitorArgs>.SetNewHandler(
                    () => new VgVisitorArgs(),
                    p => p.Reset());//when relese back
            }

            return Temp<VgVisitorArgs>.Borrow(out visitor);
        }
    }

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


    public class VgVisualDocHost
    {
        Action<VgVisualElement> _invalidate;
        Action<LayoutFarm.ImageBinder, VgVisualElement, object> _imgReqHandler;
        public void SetInvalidateDelegate(Action<VgVisualElement> invalidate)
        {
            _invalidate = invalidate;
        }
        public void SetImgRequestDelgate(Action<LayoutFarm.ImageBinder, VgVisualElement, object> imgRequestHandler)
        {
            _imgReqHandler = imgRequestHandler;
        }

        internal void RequestImageAsync(LayoutFarm.ImageBinder binder, VgVisualElement imgRun, object requestFrom)
        {
            if (_imgReqHandler != null)
            {
                _imgReqHandler(binder, imgRun, requestFrom);
            }
            else
            {
                //ask for coment resource IO
                _imgReqHandler = VgResourceIO.VgImgIOHandler;
                if (_imgReqHandler != null)
                {
                    _imgReqHandler(binder, imgRun, requestFrom);
                }
            }
        }
        internal void InvalidateGraphics(VgVisualElement e)
        {
            //***

            if (_invalidate != null)
            {
                _invalidate(e);
            }
        }
    }

    public class VgVisualDoc
    {
        //vector graphic document root


        VgVisualDocHost _vgVisualDocHost;
        internal List<SvgElement> _defsList = new List<SvgElement>();
        internal List<SvgElement> _styleList = new List<SvgElement>();
        internal Dictionary<string, VgVisualElement> _registeredElemsById = new Dictionary<string, VgVisualElement>();
        internal Dictionary<string, VgVisualElement> _clipPathDic = new Dictionary<string, VgVisualElement>();
        internal Dictionary<string, VgVisualElement> _markerDic = new Dictionary<string, VgVisualElement>();
        internal Dictionary<string, VgVisualElement> _filterDic = new Dictionary<string, VgVisualElement>();

        public VgVisualDoc(VgVisualDocHost vgVisualDocHost = null)
        {
            _vgVisualDocHost = vgVisualDocHost;
        }
        public VgVisualDocHost DocHost
        {
            get => _vgVisualDocHost;
            set => _vgVisualDocHost = value;
        }
        public bool TryGetVgVisualElementById(string id, out VgVisualElement found)
        {
            return _registeredElemsById.TryGetValue(id, out found);
        }
        internal void Invalidate(VgVisualElement e)
        {
            _vgVisualDocHost?.InvalidateGraphics(e);
        }

        internal void RequestImageAsync(LayoutFarm.ImageBinder binder, VgVisualElement imgRun, object requestFrom)
        {
            _vgVisualDocHost?.RequestImageAsync(binder, imgRun, requestFrom);
        }

        //
        public VgVisualElement VgRootElem { get; set; }


        public VgVisualElement CreateVgUseVisualElement(VgVisualElement refVgVisualElem)
        {

            //#if DEBUG 
            //#endif
            SvgUseSpec useSpec = new SvgUseSpec();
            VgUseVisualElement vgUseVisualElem = new VgUseVisualElement(useSpec, this);
            vgUseVisualElem.HRefSvgRenderElement = refVgVisualElem;
            return vgUseVisualElem;
        }
    }


    public class VgVisualElement : VgVisualElementBase
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
        public VertexStore VxsPath { get; set; }


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
                clone.VxsPath = this.VxsPath.CreateTrim();
            }
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
                        return transformation.ResolvedICoordTransformer = Affine.NewMatix(
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
                                VxsPath = vxs.CreateTrim();
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
                            using (VgPainterArgsPool.Borrow(painter, out VgPaintArgs paintArgs2))
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
        public SvgDocument OwnerDocument { get; set; } //optional
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


    public static class VgResourceIO
    {
        //IO 
        [System.ThreadStatic]
        static Action<LayoutFarm.ImageBinder, VgVisualElement, object> s_vgIO;
        public static Action<LayoutFarm.ImageBinder, VgVisualElement, object> VgImgIOHandler
        {
            get
            {
                return s_vgIO;
            }
            set
            {
                if (value == null)
                {
                    //clear existing value
                    s_vgIO = null;
                }
                else
                {
                    if (s_vgIO == null)
                    {
                        //please note that if the system already has one=>
                        //we do not replace it
                        s_vgIO = value;
                    }
                }
            }
        }
    }

    /// <summary>
    /// vector graphics (vg) document builder
    /// </summary>
    public class VgVisualDocBuilder
    {
        //-----------------------------------------------------------
        MyVgPathDataParser _pathDataParser = new MyVgPathDataParser();
        List<VgVisualElement> _waitingList = new List<VgVisualElement>();


        SvgDocument _svgdoc;
        //a copy from
        List<SvgElement> _defsList;
        List<SvgElement> _styleList;
        Dictionary<string, VgVisualElement> _registeredElemsById;
        Dictionary<string, VgVisualElement> _clipPathDic;
        Dictionary<string, VgVisualElement> _markerDic;
        Dictionary<string, VgVisualElement> _filterDic;

        float _containerWidth = 500;//default?
        float _containerHeight = 500;//default?
        float _emHeight = 17;//default TODO: review here

        LayoutFarm.WebDom.CssActiveSheet _activeSheet1; //temp fix1 
        VgVisualDoc _vgVisualDoc; //result  
        public VgVisualDocBuilder()
        {

        }

        public void SetContainerSize(float width, float height)
        {
            _containerWidth = width;
            _containerHeight = height;
        }

        public VgVisualDoc CreateVgVisualDoc(SvgDocument svgdoc, VgVisualDocHost docHost)
        {
            //
            //reset some value
            _containerWidth = 500;
            _containerHeight = 500;
            _emHeight = 17;
            _waitingList.Clear();

            //
            _svgdoc = svgdoc;
            _activeSheet1 = svgdoc.CssActiveSheet;

            _vgVisualDoc = new VgVisualDoc(docHost);

            _defsList = _vgVisualDoc._defsList;
            _styleList = _vgVisualDoc._styleList;
            _registeredElemsById = _vgVisualDoc._registeredElemsById;
            _clipPathDic = _vgVisualDoc._clipPathDic;
            _markerDic = _vgVisualDoc._markerDic;
            _filterDic = _vgVisualDoc._filterDic;

            //---------------------------
            //create visual element for the svg
            SvgElement rootElem = svgdoc.Root;

            VgVisualElement vgVisualRootElem = new VgVisualElement(WellknownSvgElementName.RootSvg, null, _vgVisualDoc);
            _vgVisualDoc.VgRootElem = vgVisualRootElem;//**

            int childCount = rootElem.ChildCount;
            for (int i = 0; i < childCount; ++i)
            {
                //translate SvgElement to  
                //command stream?
                CreateSvgVisualElement(vgVisualRootElem, rootElem.GetChild(i));
            }

            //resolve
            int j = _waitingList.Count;
            BuildDefinitionNodes();
            for (int i = 0; i < j; ++i)
            {
                //resolve
                VgUseVisualElement useNodeVisElem = (VgUseVisualElement)_waitingList[i];
                if (useNodeVisElem.HRefSvgRenderElement == null)
                {
                    //resolve
                    SvgUseSpec useSpec = (SvgUseSpec)useNodeVisElem._visualSpec;
                    if (_registeredElemsById.TryGetValue(useSpec.Href.Value, out VgVisualElement result))
                    {
                        useNodeVisElem.HRefSvgRenderElement = result;
                    }
                    else
                    {

                    }
                }
            }

            //------------

            return _vgVisualDoc;
        }


        VgVisualElement CreateSvgVisualElement(VgVisualElement parentNode, SvgElement elem)
        {
            VgVisualElement vgVisElem = null;
            bool skipChildrenNode = false;

            switch (elem.WellknowElemName)
            {
                default:
                    throw new NotSupportedException();
                //-----------------
                case WellknownSvgElementName.FeColorMatrix:
                    vgVisElem = CreateFeColorMatrix(parentNode, elem, (SvgFeColorMatrixSpec)elem.ElemSpec);//no more child 
                    parentNode.AddChildElement(vgVisElem);
                    return vgVisElem;
                case WellknownSvgElementName.Filter:
                    vgVisElem = CreateFilterElem(parentNode, elem, (SvgFilterSpec)elem.ElemSpec);
                    break;
                case WellknownSvgElementName.RadialGradient:
                    //TODO: add radial grapdient support 
                    //this version not support linear gradient
                    return CreateRadialGradient(parentNode, elem, (SvgRadialGradientSpec)elem.ElemSpec);
                case WellknownSvgElementName.LinearGradient:
                    //TODO: add linear grapdient support 
                    //this version not support linear gradient
                    return CreateLinearGradient(parentNode, elem, (SvgLinearGradientSpec)elem.ElemSpec);
                case WellknownSvgElementName.Defs:
                    _defsList.Add(elem);
                    return null;
                case WellknownSvgElementName.Style:
                    _styleList.Add(elem);
                    return null;
                case WellknownSvgElementName.Marker:
                    vgVisElem = CreateMarker(parentNode, (SvgMarkerSpec)elem.ElemSpec);
                    break;
                //-----------------
                case WellknownSvgElementName.Unknown:
                    return null;
                case WellknownSvgElementName.Mask:
                    vgVisElem = CreateMask(parentNode, (SvgMaskSpec)elem.ElemSpec);
                    break;
                case WellknownSvgElementName.Svg:
                    vgVisElem = new VgVisualElement(WellknownSvgElementName.Svg, (SvgVisualSpec)elem.ElemSpec, _vgVisualDoc);
                    break;
                case WellknownSvgElementName.Rect:
                    vgVisElem = CreateRect(parentNode, (SvgRectSpec)elem.ElemSpec);
                    break;
                case WellknownSvgElementName.Image:
                    vgVisElem = CreateImage(parentNode, (SvgImageSpec)elem.ElemSpec);
                    break;
                case WellknownSvgElementName.Polyline:
                    vgVisElem = CreatePolyline(parentNode, (SvgPolylineSpec)elem.ElemSpec);
                    break;
                case WellknownSvgElementName.Polygon:
                    vgVisElem = CreatePolygon(parentNode, (SvgPolygonSpec)elem.ElemSpec);
                    break;
                case WellknownSvgElementName.Ellipse:
                    vgVisElem = CreateEllipse(parentNode, (SvgEllipseSpec)elem.ElemSpec);
                    break;
                case WellknownSvgElementName.Circle:
                    vgVisElem = CreateCircle(parentNode, (SvgCircleSpec)elem.ElemSpec);
                    break;
                case WellknownSvgElementName.Line:
                    vgVisElem = CreateLine(parentNode, (SvgLineSpec)elem.ElemSpec);
                    break;
                case WellknownSvgElementName.ClipPath:
                    vgVisElem = CreateClipPath(parentNode, (SvgVisualSpec)elem.ElemSpec);
                    break;
                case WellknownSvgElementName.Group:
                    vgVisElem = CreateGroup(parentNode, (SvgVisualSpec)elem.ElemSpec);
                    break;
                //---------------------------------------------
                case WellknownSvgElementName.Path:
                    vgVisElem = CreatePath(parentNode, (SvgPathSpec)elem.ElemSpec, elem);
                    skipChildrenNode = true;//***
                    break;
                case WellknownSvgElementName.Text:
                    vgVisElem = CreateTextElem(parentNode, elem, (SvgTextSpec)elem.ElemSpec);
                    skipChildrenNode = true;//***
                    break;
                case WellknownSvgElementName.Use:
                    vgVisElem = CreateUseElement(parentNode, (SvgUseSpec)elem.ElemSpec);
                    skipChildrenNode = true;
                    break;

            }

            if (vgVisElem == null)
            {
                //TODO: review here
                return null;
            }
            //-----------------------------------
            vgVisElem.DomElem = elem;
            if (elem.ElemId != null)
            {
                //replace duplicated item ???
                _registeredElemsById[elem.ElemId] = vgVisElem;
            }

            vgVisElem.SetController(elem);


#if DEBUG
            if (skipChildrenNode && !vgVisElem.dbugHasParent)
            {

            }
#endif
            //-----------------------------------
            if (!skipChildrenNode)
            {

                parentNode.AddChildElement(vgVisElem);


                int childCount = elem.ChildCount;
                for (int i = 0; i < childCount; ++i)
                {
                    CreateSvgVisualElement(vgVisElem, elem.GetChild(i));
                }
            }

            return vgVisElem;
        }

        VgVisualElement CreateClipPath(VgVisualElement parentNode, SvgVisualSpec visualSpec)
        {
            VgVisualElement vgVisElem = new VgVisualElement(WellknownSvgElementName.ClipPath, visualSpec, _vgVisualDoc);
            AssignAttributes(visualSpec);
            return vgVisElem;
        }
        VgVisualElement CreateMarker(VgVisualElement parentNode, SvgMarkerSpec visualSpec)
        {
            VgVisualElement renderE = new VgVisualElement(WellknownSvgElementName.Marker, visualSpec, _vgVisualDoc);
            AssignAttributes(visualSpec);
            return renderE;
        }
        VgVisualElement CreateMask(VgVisualElement parentNode, SvgMaskSpec visualSpec)
        {
            VgVisualElement renderE = new VgVisualElement(WellknownSvgElementName.Mask, visualSpec, _vgVisualDoc);
            AssignAttributes(visualSpec);
            return renderE;
        }

        int _lastestBuiltDefIndex = 0;

        void BuildDefinitionNodes()
        {
            int j = _defsList.Count;
            if (_lastestBuiltDefIndex >= j)
            {
                return;
            }
            VgVisualElement definitionRoot = new VgVisualElement(WellknownSvgElementName.Defs, null, _vgVisualDoc);


            for (int i = _lastestBuiltDefIndex; i < j; ++i)
            {
                SvgElement defsElem = _defsList[i];
                _lastestBuiltDefIndex++;

                //get definition content
                int childCount = defsElem.ChildCount;
                for (int c = 0; c < childCount; ++c)
                {
                    SvgElement child = defsElem.GetChild(c);
                    switch (child.WellknowElemName)
                    {
                        default:
                            {
                                switch (child.ElemName)
                                {
                                    case "filter":
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        case WellknownSvgElementName.Filter:
                            {
                                VgVisualElement renderE = CreateSvgVisualElement(definitionRoot, child);
                                if (child.ElemId != null)
                                {
                                    _filterDic.Add(child.ElemId, renderE);
                                }

                            }
                            break;
                        case WellknownSvgElementName.Ellipse:
                        case WellknownSvgElementName.Rect:
                        case WellknownSvgElementName.Polygon:
                        case WellknownSvgElementName.Circle:
                        case WellknownSvgElementName.Polyline:
                        case WellknownSvgElementName.Path:
                        case WellknownSvgElementName.Text:
                            {
                                CreateSvgVisualElement(definitionRoot, child);
                            }
                            break;
                        case WellknownSvgElementName.ClipPath:
                            {
                                //clip path definition  
                                //make this as a clip path 
                                VgVisualElement renderE = CreateSvgVisualElement(definitionRoot, child);
                                _clipPathDic.Add(child.ElemId, renderE);
                            }
                            break;
                        case WellknownSvgElementName.Marker:
                            {
                                //clip path definition  
                                //make this as a clip path 
                                VgVisualElement renderE = CreateSvgVisualElement(definitionRoot, child);
                                _markerDic.Add(child.ElemId, renderE);
                            }
                            break;
                    }
                }
            }
        }

        void AssignAttributes(SvgVisualSpec spec, VgVisualElement visualElem = null)
        {
            BuildDefinitionNodes();
            if (spec.ClipPathLink != null)
            {
                //resolve this clip 
                if (_clipPathDic.TryGetValue(spec.ClipPathLink.Value, out VgVisualElement clip))
                {
                    spec.ResolvedClipPath = clip;
                    //cmds.Add(clip);
                }
                else
                {
                    if (_registeredElemsById.TryGetValue(spec.ClipPathLink.Value, out VgVisualElement clipElem))
                    {
                        spec.ResolvedClipPath = clipElem;
                    }
                    else
                    {

                    }
                }
            }
            if (spec.HasFillColor && spec.FillPathLink != null)
            {
                if (_registeredElemsById.TryGetValue(spec.FillPathLink.Value, out VgVisualElement fillElem))
                {
                    spec.ResolvedFillBrush = fillElem;
                }
                else
                {

                }
            }
            if (spec.MaskPathLink != null)
            {
                //TODO: resolve this later
                if (_registeredElemsById.TryGetValue(spec.MaskPathLink.Value, out VgVisualElement maskElem))
                {
                    spec.ResolvedMask = maskElem;
                }
                else
                {

                }
            }
            if (spec.FilterPathLink != null)
            {
                if (_registeredElemsById.TryGetValue(spec.FilterPathLink.Value, out VgVisualElement filterElem))
                {
                    spec.ResolvedFilter = filterElem;
                }
                else
                {

                }
            }
        }
        VgVisualElement CreatePath(VgVisualElement parentNode, SvgPathSpec pathSpec, SvgElement node)
        {

            VgVisualElement vgVisElem = new VgVisualElement(WellknownSvgElementName.Path, pathSpec, _vgVisualDoc); //**

            //#if DEBUG
            //            if (node.ElemId == "x11")
            //            {
            //                vgVisElem.dbugNote = node.ElemId;
            //            }
            //#endif
            //d             
            AssignAttributes(pathSpec);
            vgVisElem.VxsPath = ParseSvgPathDefinitionToVxs(pathSpec.D.ToCharArray());
            ResolveMarkers(vgVisElem, pathSpec);


            if (vgVisElem._pathMarkers != null)
            {
                //create primary instance plan for this 

            }
            parentNode.AddChildElement(vgVisElem);
            return vgVisElem;
        }

        struct ReEvaluateArgs : LayoutFarm.Css.IHasEmHeight
        {
            public readonly float containerW;
            public readonly float containerH;
            public readonly float emHeight;

            public ReEvaluateArgs(float containerW, float containerH, float emHeight)
            {
                this.containerW = containerW;
                this.containerH = containerH;
                this.emHeight = emHeight;
            }
            public float GetEmHeight() => emHeight;
        }
        VgVisualElement CreateEllipse(VgVisualElement parentNode, SvgEllipseSpec ellipseSpec)
        {

            VgVisualElement vgEllipse = new VgVisualElement(WellknownSvgElementName.Ellipse, ellipseSpec, _vgVisualDoc);
            using (VectorToolBox.Borrow(out Ellipse ellipse))
            using (VxsTemp.Borrow(out var v1))
            {
                ReEvaluateArgs a = new ReEvaluateArgs(_containerWidth, _containerHeight, _emHeight); //temp fix 
                double x = ConvertToPx(ellipseSpec.X, ref a);
                double y = ConvertToPx(ellipseSpec.Y, ref a);
                double rx = ConvertToPx(ellipseSpec.RadiusX, ref a);
                double ry = ConvertToPx(ellipseSpec.RadiusY, ref a);

                ellipse.Set(x, y, rx, ry);////TODO: review here => temp fix for ellipse step  
                vgEllipse.VxsPath = ellipse.MakeVxs(v1).CreateTrim();
                AssignAttributes(ellipseSpec);
                return vgEllipse;
            }


        }
        VgVisualElement CreateImage(VgVisualElement parentNode, SvgImageSpec imgspec)
        {
            VgVisualElement vgImg = new VgVisualElement(WellknownSvgElementName.Image, imgspec, _vgVisualDoc);
            using (VectorToolBox.Borrow(out SimpleRect rectTool))
            using (VxsTemp.Borrow(out var v1))
            {
                ReEvaluateArgs a = new ReEvaluateArgs(_containerWidth, _containerHeight, _emHeight); //temp fix
                vgImg._imgX = ConvertToPx(imgspec.X, ref a);
                vgImg._imgY = ConvertToPx(imgspec.Y, ref a);
                vgImg._imgW = ConvertToPx(imgspec.Width, ref a);
                vgImg._imgH = ConvertToPx(imgspec.Height, ref a);
                //
                rectTool.SetRect(
                    vgImg._imgX,
                    vgImg._imgY + vgImg._imgH,
                    vgImg._imgX + vgImg._imgW,
                    vgImg._imgY);
                vgImg.VxsPath = rectTool.MakeVxs(v1).CreateTrim();
                //
                AssignAttributes(imgspec);
                //
                return vgImg;
            }
        }
        VgVisualElement CreatePolygon(VgVisualElement parentNode, SvgPolygonSpec polygonSpec)
        {
            VgVisualElement vgPolygon = new VgVisualElement(WellknownSvgElementName.Polygon, polygonSpec, _vgVisualDoc);

            PointF[] points = polygonSpec.Points;
            int j = points.Length;
            if (j > 1)
            {
                using (VxsTemp.Borrow(out var v1))
                {
                    PointF p = points[0];
                    PointF p0 = p;
                    v1.AddMoveTo(p.X, p.Y);

                    for (int i = 1; i < j; ++i)
                    {
                        p = points[i];
                        v1.AddLineTo(p.X, p.Y);
                    }
                    //close
                    v1.AddMoveTo(p0.X, p0.Y);
                    v1.AddCloseFigure();

                    vgPolygon.VxsPath = v1.CreateTrim();
                }
                AssignAttributes(polygonSpec);
                ResolveMarkers(vgPolygon, polygonSpec);
                if (vgPolygon._pathMarkers != null)
                {
                    //create primary instance plan for this 

                }
            }
            return vgPolygon;
        }
        VgVisualElement CreatePolyline(VgVisualElement parentNode, SvgPolylineSpec polylineSpec)
        {
            VgVisualElement vgPolyline = new VgVisualElement(WellknownSvgElementName.Polyline, polylineSpec, _vgVisualDoc);
            PointF[] points = polylineSpec.Points;
            int j = points.Length;
            if (j > 1)
            {
                using (VxsTemp.Borrow(out var v1))
                {
                    PointF p = points[0];
                    v1.AddMoveTo(p.X, p.Y);
                    for (int i = 1; i < j; ++i)
                    {
                        p = points[i];
                        v1.AddLineTo(p.X, p.Y);
                    }
                    vgPolyline.VxsPath = v1.CreateTrim();
                }
                AssignAttributes(polylineSpec);

                //--------------------------------------------------------------------
                ResolveMarkers(vgPolyline, polylineSpec);
                if (vgPolyline._pathMarkers != null)
                {

                    //create primary instance plan for this polyline
                    VgPathVisualMarkers pathMarkers = vgPolyline._pathMarkers;
                    pathMarkers.AllPoints = points;

                    //start, mid, end
                    if (pathMarkers.StartMarker != null)
                    {
                        //turn marker to the start direction
                        PointF p0 = points[0];
                        PointF p1 = points[1];
                        //find rotation angle
                        double rotateRad = Math.Atan2(p0.Y - p1.Y, p0.X - p1.X);
                        SvgMarkerSpec markerSpec = (SvgMarkerSpec)pathMarkers.StartMarker._visualSpec;

                        //create local-transformation matrix
                        pathMarkers.StartMarkerPos = new PointF(p0.X, p0.Y);
                        pathMarkers.StartMarkerAffine = Affine.NewMatix(
                            AffinePlan.Translate(-markerSpec.RefX.Number, -markerSpec.RefY.Number), //move to the ref point
                            AffinePlan.Rotate(rotateRad) //rotate                            
                        );
                    }
                    //-------------------------------
                    if (pathMarkers.MidMarker != null)
                    {
                        SvgMarkerSpec markerSpec = (SvgMarkerSpec)pathMarkers.StartMarker._visualSpec;
                        pathMarkers.MidMarkerAffine = Affine.NewTranslation(-markerSpec.RefX.Number, -markerSpec.RefY.Number);
                    }
                    //-------------------------------
                    if (pathMarkers.EndMarker != null)
                    {
                        //turn marker to the start direction
                        PointF p0 = points[j - 2]; //before the last one
                        PointF p1 = points[j - 1];//the last one
                                                  //find rotation angle
                        double rotateRad = Math.Atan2(p1.Y - p0.Y, p1.X - p0.X);
                        SvgMarkerSpec markerSpec = (SvgMarkerSpec)pathMarkers.EndMarker._visualSpec;


                        //create local-transformation matrix
                        pathMarkers.EndMarkerPos = new PointF(p1.X, p1.Y);
                        pathMarkers.EndMarkerAffine = Affine.NewMatix(
                            AffinePlan.Translate(-markerSpec.RefX.Number, -markerSpec.RefY.Number), //move to the ref point
                            AffinePlan.Rotate(rotateRad) //rotate                            
                        );
                    }
                }

            }
            return vgPolyline;
        }
        void ResolveMarkers(VgVisualElement vgVisElem, IMayHaveMarkers mayHasMarkers)
        {
            //TODO: review here again***
            //assume marker link by id

            VgPathVisualMarkers pathRenderMarkers = vgVisElem._pathMarkers;

            if (mayHasMarkers.MarkerStart != null)
            {
                if (pathRenderMarkers == null)
                {
                    vgVisElem._pathMarkers = pathRenderMarkers = new VgPathVisualMarkers();
                }
                BuildDefinitionNodes();

                if (_markerDic.TryGetValue(mayHasMarkers.MarkerStart.Value, out VgVisualElement marker))
                {
                    pathRenderMarkers.StartMarker = marker;
                }

            }
            if (mayHasMarkers.MarkerMid != null)
            {
                if (pathRenderMarkers == null)
                {
                    vgVisElem._pathMarkers = pathRenderMarkers = new VgPathVisualMarkers();
                }
                BuildDefinitionNodes();

                if (_markerDic.TryGetValue(mayHasMarkers.MarkerMid.Value, out VgVisualElement marker))
                {
                    pathRenderMarkers.MidMarker = marker;
                }
            }
            if (mayHasMarkers.MarkerEnd != null)
            {
                if (pathRenderMarkers == null)
                {
                    vgVisElem._pathMarkers = pathRenderMarkers = new VgPathVisualMarkers();
                }
                BuildDefinitionNodes();

                if (_markerDic.TryGetValue(mayHasMarkers.MarkerEnd.Value, out VgVisualElement marker))
                {
                    pathRenderMarkers.EndMarker = marker;
                }
            }
        }
        VgVisualElement CreateLine(VgVisualElement parentNode, SvgLineSpec linespec)
        {
            VgVisualElement lineVisualElem = new VgVisualElement(WellknownSvgElementName.Line, linespec, _vgVisualDoc);
            using (VxsTemp.Borrow(out var v1))
            {
                v1.AddMoveTo(linespec.X1.Number, linespec.Y1.Number);
                v1.AddLineTo(linespec.X2.Number, linespec.Y2.Number);
                v1.AddNoMore();
                lineVisualElem.VxsPath = v1.CreateTrim();
            }
            return lineVisualElem;
        }
        VgVisualElement CreateCircle(VgVisualElement parentNode, SvgCircleSpec cirSpec)
        {

            VgVisualElement cir = new VgVisualElement(WellknownSvgElementName.Circle, cirSpec, _vgVisualDoc);

            using (VectorToolBox.Borrow(out Ellipse ellipse))
            using (VxsTemp.Borrow(out var v1))
            {
                ReEvaluateArgs a = new ReEvaluateArgs(_containerWidth, _containerHeight, _emHeight); //temp fix
                double x = ConvertToPx(cirSpec.X, ref a);
                double y = ConvertToPx(cirSpec.Y, ref a);
                double r = ConvertToPx(cirSpec.Radius, ref a);

                ellipse.Set(x, y, r, r);////TODO: review here => temp fix for ellipse step  
                cir.VxsPath = ellipse.MakeVxs(v1).CreateTrim();
                AssignAttributes(cirSpec);
                return cir;
            }
        }

        void RegisterElementById(SvgElement elem, VgVisualElement vgVisualElem)
        {
            if (elem.ElemId != null)
            {
                //replace duplicated item ???
                _registeredElemsById[elem.ElemId] = vgVisualElem;
            }

        }

        VgVisualElement CreateFilterElem(VgVisualElement parentNode, SvgElement elem, SvgFilterSpec spec)
        {
            VgVisualElement filterElem = new VgVisualElement(WellknownSvgElementName.Filter, spec, _vgVisualDoc);


            return filterElem;
        }
        VgVisualElement CreateFeColorMatrix(VgVisualElement parentNode, SvgElement elem, SvgFeColorMatrixSpec spec)
        {
            VgVisualElement feColorMatrixElem = new VgVisualElement(WellknownSvgElementName.FeColorMatrix, spec, _vgVisualDoc);
            PaintFx.Effects.ImgFilterSvgFeColorMatrix colorMat = new PaintFx.Effects.ImgFilterSvgFeColorMatrix();
            spec.ResolvedFilter = colorMat;
            //TODO: check if matrix is identify matrix or not
            //
            colorMat.Elements = spec.matrix;
            return feColorMatrixElem;
        }
        VgVisualElement CreateRadialGradient(VgVisualElement parentNode, SvgElement elem, SvgRadialGradientSpec spec)
        {
            //<radialGradient id="a" cx="59.6" cy="54.845" r="55.464" fx="27.165" fy="53.715" gradientUnits="userSpaceOnUse">
            //    <stop stop-color="#FFEB3B" offset="0" />
            //    <stop stop-color="#FBC02D" offset="1" />
            //</radialGradient>
            //create linear gradient texure (or brush)
            VgVisualElement radialGrd = new VgVisualElement(WellknownSvgElementName.RadialGradient, spec, _vgVisualDoc);

            //read attribute 
            RegisterElementById(elem, radialGrd);

            int childCount = elem.ChildCount;
            for (int i = 0; i < childCount; ++i)
            {
                SvgElement child = elem.GetChild(i);
                if (child.ElemName == "stop")
                {
                    //color stop
                    //TODO: implement this.... 
                    SvgColorStopSpec stopSpec = child.ElemSpec as SvgColorStopSpec;
                    if (stopSpec != null)
                    {
                        if (spec.StopList == null)
                        {
                            //TODO
                            spec.StopList = new List<SvgColorStopSpec>();
                        }
                        spec.StopList.Add(stopSpec);
                    }
                }
                else
                {

                }
            }
            return radialGrd;
        }
        VgVisualElement CreateLinearGradient(VgVisualElement parentNode, SvgElement elem, SvgLinearGradientSpec spec)
        {

            // <linearGradient id="polygon101_1_" gradientUnits="userSpaceOnUse" x1="343.1942" y1="259.6319" x2="424.394" y2="337.1182" gradientTransform="matrix(1.2948 0 0 1.2948 -0.9411 368.7214)">
            //	<stop offset="1.348625e-002" style="stop-color:#DC2E19"/>
            //	<stop offset="0.3012" style="stop-color:#DC2B19"/>
            //	<stop offset="1" style="stop-color:#FDEE00"/>
            //</linearGradient>


            //create linear gradient texure (or brush)
            VgVisualElement linearGrd = new VgVisualElement(WellknownSvgElementName.LinearGradient, spec, _vgVisualDoc);
            //read attribute

            RegisterElementById(elem, linearGrd);

            float x1 = spec.X1.Number;
            float y1 = spec.Y1.Number;
            float x2 = spec.X2.Number;
            float y2 = spec.Y2.Number;
            int childCount = elem.ChildCount;

            for (int i = 0; i < childCount; ++i)
            {
                SvgElement child = elem.GetChild(i);
                if (child.ElemName == "stop")
                {
                    //color stop
                    //TODO: implement this.... 
                    SvgColorStopSpec stopSpec = child.ElemSpec as SvgColorStopSpec;
                    if (stopSpec != null)
                    {
                        if (spec.StopList == null)
                        {
                            //TODO
                            spec.StopList = new List<SvgColorStopSpec>();
                        }
                        spec.StopList.Add(stopSpec);
                    }
                }
                else
                {

                }
            }

            return linearGrd;
        }


        VgVisualElement CreateUseElement(VgVisualElement parentNode, SvgUseSpec spec)
        {
            VgUseVisualElement vgVisElem = new VgUseVisualElement(spec, _vgVisualDoc);
            AssignAttributes(spec);
            if (spec.Href != null)
            {
                //add to waiting list
                _waitingList.Add(vgVisElem);
            }

            //text x,y
            parentNode.AddChildElement(vgVisElem);
            return vgVisElem;
        }
        VgVisualElement CreateTextElem(VgVisualElement parentNode, SvgElement elem, SvgTextSpec textspec)
        {
            //text render element  
            VgVisualElement vgVisElem = new VgVisualElement(WellknownSvgElementName.Text, textspec, _vgVisualDoc);
            vgVisElem.DomElem = elem;
            //some att

            if (textspec.Class != null && _activeSheet1 != null)
            {
                //resolve style definition
                LayoutFarm.WebDom.CssRuleSetGroup ruleSetGroup = _activeSheet1.GetRuleSetForClassName(textspec.Class);
                if (ruleSetGroup != null)
                {
                    //assign
                    foreach (LayoutFarm.WebDom.CssPropertyDeclaration propDecl in ruleSetGroup.GetPropertyDeclIter())
                    {
                        switch (propDecl.WellknownPropertyName)
                        {
                            case LayoutFarm.WebDom.WellknownCssPropertyName.Font:
                                //set font detail 
                                break;
                            case LayoutFarm.WebDom.WellknownCssPropertyName.FontStyle:
                                //convert font style
                                break;
                            case LayoutFarm.WebDom.WellknownCssPropertyName.FontSize:
                                textspec.FontSize = propDecl.GetPropertyValue(0).AsLength();
                                break;
                            case LayoutFarm.WebDom.WellknownCssPropertyName.FontFamily:
                                textspec.FontFace = propDecl.GetPropertyValue(0).ToString();
                                break;
                            case LayoutFarm.WebDom.WellknownCssPropertyName.Fill:
                                textspec.FillColor = LayoutFarm.WebDom.Parser.CssValueParser.ParseCssColor(propDecl.GetPropertyValue(0).ToString());
                                break;
                            case LayoutFarm.WebDom.WellknownCssPropertyName.Unknown:
                                {
                                    switch (propDecl.UnknownRawName)
                                    {
                                        case "fill":
                                            //svg 
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                }
            }


            ReEvaluateArgs a = new ReEvaluateArgs(_containerWidth, _containerHeight, _emHeight); //temp fix
            textspec.ActualX = ConvertToPx(textspec.X, ref a);
            textspec.ActualY = ConvertToPx(textspec.Y, ref a);


            AssignAttributes(textspec);

            //text x,y


            parentNode.AddChildElement(vgVisElem);
            return vgVisElem;
        }
        VgVisualElement CreateRect(VgVisualElement parentNode, SvgRectSpec rectSpec)
        {

            VgVisualElement rect = new VgVisualElement(WellknownSvgElementName.Rect, rectSpec, _vgVisualDoc);


            if (!rectSpec.CornerRadiusX.IsEmpty || !rectSpec.CornerRadiusY.IsEmpty)
            {

                using (VectorToolBox.Borrow(out RoundedRect roundRect))
                using (VxsTemp.Borrow(out var v1))
                {
                    ReEvaluateArgs a = new ReEvaluateArgs(_containerWidth, _containerHeight, _emHeight); //temp fix
                    roundRect.SetRect(
                        ConvertToPx(rectSpec.X, ref a),
                        ConvertToPx(rectSpec.Y, ref a) + ConvertToPx(rectSpec.Height, ref a),
                        ConvertToPx(rectSpec.X, ref a) + ConvertToPx(rectSpec.Width, ref a),
                        ConvertToPx(rectSpec.Y, ref a));

                    roundRect.SetRadius(ConvertToPx(rectSpec.CornerRadiusX, ref a), ConvertToPx(rectSpec.CornerRadiusY, ref a));
                    rect.VxsPath = roundRect.MakeVxs(v1).CreateTrim();
                }


            }
            else
            {

                using (VectorToolBox.Borrow(out SimpleRect rectTool))
                using (VxsTemp.Borrow(out var v1))
                {
                    ReEvaluateArgs a = new ReEvaluateArgs(_containerWidth, _containerHeight, _emHeight); //temp fix
                    rectTool.SetRect(
                        ConvertToPx(rectSpec.X, ref a),
                        ConvertToPx(rectSpec.Y, ref a) + ConvertToPx(rectSpec.Height, ref a),
                        ConvertToPx(rectSpec.X, ref a) + ConvertToPx(rectSpec.Width, ref a),
                        ConvertToPx(rectSpec.Y, ref a));
                    // 
                    rect.VxsPath = rectTool.MakeVxs(v1).CreateTrim();

                }


            }

            AssignAttributes(rectSpec);
            return rect;
        }

        static float ConvertToPx(LayoutFarm.Css.CssLength length, ref ReEvaluateArgs args)
        {
            return LayoutFarm.WebDom.Parser.CssValueParser.ConvertToPx(length, args.containerW, args);
        }

        VertexStore ParseSvgPathDefinitionToVxs(char[] buffer)
        {
            using (VectorToolBox.Borrow(out CurveFlattener curveFlattener))
            using (VxsTemp.Borrow(out var v1, out var v2))
            using (VectorToolBox.Borrow(v1, out PathWriter pathWriter))
            {
                _pathDataParser.SetPathWriter(pathWriter);
                _pathDataParser.Parse(buffer);
                curveFlattener.MakeVxs(v1, v2);
                //create a small copy of the vxs                  
                return v2.CreateTrim();
            }
        }
        VgVisualElement CreateGroup(VgVisualElement parentNode, SvgVisualSpec visSpec)
        {

            VgVisualElement vgVisElem = new VgVisualElement(WellknownSvgElementName.Group, visSpec, _vgVisualDoc);
            AssignAttributes(visSpec);
            return vgVisElem;
        }
    }


    sealed class SvgArcSegment
    {
        //from SVG.NET (https://github.com/vvvv/SVG)

        private const double RadiansPerDegree = Math.PI / 180.0;
        private const double DoublePI = Math.PI * 2;

        public float RadiusX
        {
            get;
            set;
        }

        public float RadiusY
        {
            get;
            set;
        }

        public float Angle
        {
            get;
            set;
        }

        public SvgArcSweep Sweep
        {
            get;
            set;
        }

        public SvgArcSize Size
        {
            get;
            set;
        }
        public PointF Start { get; set; }
        public PointF End { get; set; }
        public SvgArcSegment(PointF start, float radiusX, float radiusY, float angle, SvgArcSize size, SvgArcSweep sweep, PointF end)
        {
            this.Start = start;
            this.End = end;
            this.RadiusX = Math.Abs(radiusX);
            this.RadiusY = Math.Abs(radiusY);
            this.Angle = angle;
            this.Sweep = sweep;
            this.Size = size;
        }

        static double CalculateVectorAngle(double ux, double uy, double vx, double vy)
        {
            double ta = Math.Atan2(uy, ux);
            double tb = Math.Atan2(vy, vx);

            if (tb >= ta)
            {
                return tb - ta;
            }

            return SvgArcSegment.DoublePI - (ta - tb);
        }

        public void AddToPath(PathWriter graphicsPath)
        {
            if (PointF.Equals(Start, End))
            {
                return;
            }

            if (this.RadiusX == 0.0f && this.RadiusY == 0.0f)
            {
                //graphicsPath.AddLine(this.Start, this.End);
                graphicsPath.LineTo(this.Start.X, this.Start.Y);
                graphicsPath.LineTo(this.End.X, this.End.Y);
                return;
            }

            double sinPhi = Math.Sin(this.Angle * SvgArcSegment.RadiansPerDegree);
            double cosPhi = Math.Cos(this.Angle * SvgArcSegment.RadiansPerDegree);

            double x1dash = cosPhi * (this.Start.X - this.End.X) / 2.0 + sinPhi * (this.Start.Y - this.End.Y) / 2.0;
            double y1dash = -sinPhi * (this.Start.X - this.End.X) / 2.0 + cosPhi * (this.Start.Y - this.End.Y) / 2.0;

            double root;
            double numerator = this.RadiusX * this.RadiusX * this.RadiusY * this.RadiusY - this.RadiusX * this.RadiusX * y1dash * y1dash - this.RadiusY * this.RadiusY * x1dash * x1dash;

            float rx = this.RadiusX;
            float ry = this.RadiusY;

            if (numerator < 0.0)
            {
                float s = (float)Math.Sqrt(1.0 - numerator / (this.RadiusX * this.RadiusX * this.RadiusY * this.RadiusY));

                rx *= s;
                ry *= s;
                root = 0.0;
            }
            else
            {
                root = ((this.Size == SvgArcSize.Large && this.Sweep == SvgArcSweep.Positive) || (this.Size == SvgArcSize.Small && this.Sweep == SvgArcSweep.Negative) ? -1.0 : 1.0) * Math.Sqrt(numerator / (this.RadiusX * this.RadiusX * y1dash * y1dash + this.RadiusY * this.RadiusY * x1dash * x1dash));
            }

            double cxdash = root * rx * y1dash / ry;
            double cydash = -root * ry * x1dash / rx;

            double cx = cosPhi * cxdash - sinPhi * cydash + (this.Start.X + this.End.X) / 2.0;
            double cy = sinPhi * cxdash + cosPhi * cydash + (this.Start.Y + this.End.Y) / 2.0;

            double theta1 = SvgArcSegment.CalculateVectorAngle(1.0, 0.0, (x1dash - cxdash) / rx, (y1dash - cydash) / ry);
            double dtheta = SvgArcSegment.CalculateVectorAngle((x1dash - cxdash) / rx, (y1dash - cydash) / ry, (-x1dash - cxdash) / rx, (-y1dash - cydash) / ry);

            if (this.Sweep == SvgArcSweep.Negative && dtheta > 0)
            {
                dtheta -= 2.0 * Math.PI;
            }
            else if (this.Sweep == SvgArcSweep.Positive && dtheta < 0)
            {
                dtheta += 2.0 * Math.PI;
            }

            int segments = (int)Math.Ceiling((double)Math.Abs(dtheta / (Math.PI / 2.0)));
            double delta = dtheta / segments;
            double t = 8.0 / 3.0 * Math.Sin(delta / 4.0) * Math.Sin(delta / 4.0) / Math.Sin(delta / 2.0);

            double startX = this.Start.X;
            double startY = this.Start.Y;

            for (int i = 0; i < segments; ++i)
            {
                double cosTheta1 = Math.Cos(theta1);
                double sinTheta1 = Math.Sin(theta1);
                double theta2 = theta1 + delta;
                double cosTheta2 = Math.Cos(theta2);
                double sinTheta2 = Math.Sin(theta2);

                double endpointX = cosPhi * rx * cosTheta2 - sinPhi * ry * sinTheta2 + cx;
                double endpointY = sinPhi * rx * cosTheta2 + cosPhi * ry * sinTheta2 + cy;

                double dx1 = t * (-cosPhi * rx * sinTheta1 - sinPhi * ry * cosTheta1);
                double dy1 = t * (-sinPhi * rx * sinTheta1 + cosPhi * ry * cosTheta1);

                double dxe = t * (cosPhi * rx * sinTheta2 + sinPhi * ry * cosTheta2);
                double dye = t * (sinPhi * rx * sinTheta2 - cosPhi * ry * cosTheta2);

                //graphicsPath.AddBezier((float)startX, (float)startY, (float)(startX + dx1), (float)(startY + dy1),
                //    (float)(endpointX + dxe), (float)(endpointY + dye), (float)endpointX, (float)endpointY);
                graphicsPath.Curve4((float)(startX + dx1), (float)(startY + dy1),
                 (float)(endpointX + dxe), (float)(endpointY + dye), (float)endpointX, (float)endpointY);
                theta1 = theta2;
                startX = (float)endpointX;
                startY = (float)endpointY;
            }
        }

    }


    class MyVgPathDataParser : VgPathDataParser
    {
        PathWriter _writer;
        public void SetPathWriter(PathWriter writer)
        {
            _writer = writer;
            _writer.StartFigure();
        }
        protected override void OnArc(float r1, float r2, float xAxisRotation, int largeArcFlag, int sweepFlags, float x, float y, bool isRelative)
        {
            //TODO: implement arc again
            //arc can be approximated by lines or by bezire curve
            //this version we use lines to approximate arc            
            using (VectorToolBox.Borrow(out Arc arc))
            {

                if (isRelative)
                {
                    SvgArcSegment argSegment = new SvgArcSegment(
                        new PointF((float)_writer.CurrentX, (float)_writer.CurrentY),
                        r1, r2,
                        xAxisRotation,
                        (SvgArcSize)largeArcFlag,
                        (SvgArcSweep)sweepFlags,
                        new PointF((float)(_writer.CurrentX + x), (float)(_writer.CurrentY + y)));
                    //
                    argSegment.AddToPath(_writer);
                }
                else
                {
                    //approximate with bezier curve
                    SvgArcSegment argSegment = new SvgArcSegment(
                        new PointF((float)_writer.CurrentX, (float)_writer.CurrentY),
                        r1, r2,
                        xAxisRotation,
                       (SvgArcSize)largeArcFlag,
                       (SvgArcSweep)sweepFlags,
                       new PointF(x, y));
                    //
                    argSegment.AddToPath(_writer);
                }
            }
        }
        protected override void OnCloseFigure()
        {
            _writer.CloseFigure();

        }
        protected override void OnCurveToCubic(
            float x1, float y1,
            float x2, float y2,
            float x, float y, bool isRelative)
        {

            if (isRelative)
            {
                _writer.Curve4Rel(x1, y1, x2, y2, x, y);
            }
            else
            {
                _writer.Curve4(x1, y1, x2, y2, x, y);
            }
        }
        protected override void OnCurveToCubicSmooth(float x2, float y2, float x, float y, bool isRelative)
        {
            if (isRelative)
            {
                _writer.SmoothCurve4Rel(x2, y2, x, y);
            }
            else
            {
                _writer.SmoothCurve4(x2, y2, x, y);
            }

        }
        protected override void OnCurveToQuadratic(float x1, float y1, float x, float y, bool isRelative)
        {
            if (isRelative)
            {
                _writer.Curve3Rel(x1, y1, x, y);
            }
            else
            {
                _writer.Curve3(x1, y1, x, y);
            }
        }
        protected override void OnCurveToQuadraticSmooth(float x, float y, bool isRelative)
        {
            if (isRelative)
            {
                _writer.SmoothCurve3Rel(x, y);
            }
            else
            {
                _writer.SmoothCurve3(x, y);
            }

        }
        protected override void OnHLineTo(float x, bool isRelative)
        {
            if (isRelative)
            {
                _writer.HorizontalLineToRel(x);
            }
            else
            {
                _writer.HorizontalLineTo(x);
            }
        }

        protected override void OnLineTo(float x, float y, bool isRelative)
        {
            if (isRelative)
            {
                _writer.LineToRel(x, y);
            }
            else
            {
                _writer.LineTo(x, y);
            }
        }
        protected override void OnMoveTo(float x, float y, bool isRelative)
        {

            if (isRelative)
            {
                _writer.MoveToRel(x, y);
            }
            else
            {


                _writer.MoveTo(x, y);
            }
        }
        protected override void OnVLineTo(float y, bool isRelative)
        {
            if (isRelative)
            {
                _writer.VerticalLineToRel(y);
            }
            else
            {
                _writer.VerticalLineTo(y);
            }
        }
    }


    public static class VgVisualDocHelper
    {
        public static VgVisualDoc CreateVgVisualDocFromFile(string filename)
        {
            SvgDocBuilder docBuilder = new SvgDocBuilder();
            SvgParser svg = new SvgParser(docBuilder);
            svg.ReadSvgFile(filename);
            //
            VgVisualDocBuilder builder = new VgVisualDocBuilder();
            return builder.CreateVgVisualDoc(docBuilder.ResultDocument, null);
        }

    }


}