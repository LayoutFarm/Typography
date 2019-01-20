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
        internal VgPaintArgs() { }
        public Painter P { get; internal set; }
        public Action<VertexStore, VgPaintArgs> PaintVisitHandler;
        internal override void Reset()
        {
            base.Reset();//*** reset base class fields too
            //-------

            P = null;
            PaintVisitHandler = null;
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
                case WellknownSvgElementName.Group:
                case WellknownSvgElementName.RootSvg:
                case WellknownSvgElementName.Svg:
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
                case WellknownSvgElementName.Path:
                case WellknownSvgElementName.Line:
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

        public SvgVisualSpec VisualSpec
        {
            get { return _visualSpec; }
        }


        //---------------------------
        public override void Paint(VgPaintArgs vgPainterArgs)
        {
            //save
            Painter p = vgPainterArgs.P;
            Color color = p.FillColor;
            double strokeW = p.StrokeWidth;
            Color strokeColor = p.StrokeColor;
            RequestFont currentFont = p.CurrentFont;

            ICoordTransformer prevTx = vgPainterArgs._currentTx; //backup
            ICoordTransformer currentTx = vgPainterArgs._currentTx;

            bool hasClip = false;
            bool newFontReq = false;

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

                if (_visualSpec.HasStrokeWidth)
                {
                    //temp fix
                    p.StrokeWidth = _visualSpec.StrokeWidth.Number;
                }
                else
                {

                }

                if (_visualSpec.ResolvedClipPath != null)
                {
                    //clip-path
                    hasClip = true;

                    VgVisualElement clipPath = (VgVisualElement)_visualSpec.ResolvedClipPath;
                    VertexStore clipVxs = ((VgVisualElement)clipPath.GetChildNode(0)).VxsPath;

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
                case WellknownSvgElementName.Path:
                case WellknownSvgElementName.Line:
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
                                if (p.FillColor.A > 0)
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

                                    //}
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

            //restore
            p.FillColor = color;
            p.StrokeColor = strokeColor;
            p.StrokeWidth = strokeW;
            //
            vgPainterArgs._currentTx = prevTx;
            if (hasClip)
            {
                p.SetClipRgn(null);
            }
            if (newFontReq)
            {
                p.CurrentFont = currentFont;
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
        public int ChildCount
        {
            get
            {
                return (_childNodes == null) ? 0 : _childNodes.Count;
            }
        }
        public VgVisualElementBase GetChildNode(int index)
        {
            return _childNodes[index];
        }

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

        public Image BackingImage { get { return _backimg; } }
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
                    throw new KeyNotFoundException();
                //-----------------
                case WellknownSvgElementName.RadialGradient:
                    //TODO: add radial grapdient support 
                    //this version not support linear gradient
                    return null;
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

                case WellknownSvgElementName.ClipPath:
                    vgVisElem = CreateClipPath(parentNode, (SvgVisualSpec)elem.ElemSpec);
                    break;
                case WellknownSvgElementName.Group:
                    vgVisElem = CreateGroup(parentNode, (SvgVisualSpec)elem.ElemSpec);
                    break;
                //---------------------------------------------
                case WellknownSvgElementName.Path:
                    vgVisElem = CreatePath(parentNode, (SvgPathSpec)elem.ElemSpec);
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
        bool _buildDefs = false;
        void BuildDefinitionNodes()
        {
            if (_buildDefs)
            {
                return;
            }
            _buildDefs = true;
            VgVisualElement definitionRoot = new VgVisualElement(WellknownSvgElementName.Defs, null, _vgVisualDoc);

            int j = _defsList.Count;
            for (int i = 0; i < j; ++i)
            {
                SvgElement defsElem = _defsList[i];
                //get definition content
                int childCount = defsElem.ChildCount;
                for (int c = 0; c < childCount; ++c)
                {
                    SvgElement child = defsElem.GetChild(c);
                    switch (child.WellknowElemName)
                    {
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

        void AssignAttributes(SvgVisualSpec spec)
        {

            if (spec.ClipPathLink != null)
            {
                //resolve this clip
                BuildDefinitionNodes();
                if (_clipPathDic.TryGetValue(spec.ClipPathLink.Value, out VgVisualElement clip))
                {
                    spec.ResolvedClipPath = clip;
                    //cmds.Add(clip);
                }
            }
        }
        VgVisualElement CreatePath(VgVisualElement parentNode, SvgPathSpec pathSpec)
        {

            VgVisualElement vgVisElem = new VgVisualElement(WellknownSvgElementName.Path, pathSpec, _vgVisualDoc); //**

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


        VgVisualElement CreateLinearGradient(VgVisualElement parentNode, SvgElement elem, SvgLinearGradientSpec spec)
        {

            //create linear gradient texure (or brush)
            VgVisualElement linearGrd = new VgVisualElement(WellknownSvgElementName.LinearGradient, spec, _vgVisualDoc);
            //read attribute

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
                }
            }

            // <linearGradient id="polygon101_1_" gradientUnits="userSpaceOnUse" x1="343.1942" y1="259.6319" x2="424.394" y2="337.1182" gradientTransform="matrix(1.2948 0 0 1.2948 -0.9411 368.7214)">
            //	<stop offset="1.348625e-002" style="stop-color:#DC2E19"/>
            //	<stop offset="0.3012" style="stop-color:#DC2B19"/>
            //	<stop offset="1" style="stop-color:#FDEE00"/>
            //</linearGradient>

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
            throw new NotSupportedException();
            //base.OnArc(r1, r2, xAxisRotation, largeArcFlag, sweepFlags, x, y, isRelative);
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