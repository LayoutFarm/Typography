//MIT, 2014-present, WinterDev

using System;
using System.Collections.Generic;

using PixelFarm.Drawing;
using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.VertexProcessing;
using LayoutFarm.WebDom;

namespace PaintLab.Svg
{


    /// <summary>
    /// vector graphics (vg) document builder
    /// </summary>
    public class VgVisualDocBuilder
    {
        //this class create visual tree for svg 


        MyVgPathDataParser _pathDataParser = new MyVgPathDataParser();
        List<VgVisualElement> _waitingList = new List<VgVisualElement>();


        VgDocument _svgdoc;
        //a copy from
        List<SvgElement> _defsList;
        List<SvgElement> _styleList;
        Dictionary<string, VgVisualElement> _registeredElemsById;

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
        public VgVisualDoc CreateVgVisualDoc(VgDocument svgdoc, VgVisualDocHost docHost)
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

            //---------------------------

            //create visual tree for svg nodes
            SvgElement rootElem = svgdoc.Root;
            VgVisualElement vgVisualRootElem = new VgVisualElement(WellknownSvgElementName.RootSvg, null, _vgVisualDoc);
            _vgVisualDoc.VgRootElem = vgVisualRootElem;//**

            int childCount = rootElem.ChildCount;
            for (int i = 0; i < childCount; ++i)
            {
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
                case WellknownSvgElementName.Unknown:
                    return null;
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
                        case WellknownSvgElementName.Ellipse:
                        case WellknownSvgElementName.Rect:
                        case WellknownSvgElementName.Polygon:
                        case WellknownSvgElementName.Circle:
                        case WellknownSvgElementName.Polyline:
                        case WellknownSvgElementName.Path:
                        case WellknownSvgElementName.Text:
                        case WellknownSvgElementName.ClipPath:
                        case WellknownSvgElementName.Marker:
                            CreateSvgVisualElement(definitionRoot, child);
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
                if (_registeredElemsById.TryGetValue(spec.ClipPathLink.Value, out VgVisualElement clip))
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

            vgVisElem.VxsPath = CreateVxsFromPathDefinition(pathSpec.D.ToCharArray());
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
            using (Tools.BorrowEllipse(out var ellipse))
            using (Tools.BorrowVxs(out var v1))
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

            using (Tools.BorrowRect(out var rectTool))
            using (Tools.BorrowVxs(out var v1))
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
                using (Tools.BorrowVxs(out var v1))
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
                using (Tools.BorrowVxs(out var v1))
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
                        pathMarkers.StartMarkerAffine = Affine.New(
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
                        pathMarkers.EndMarkerAffine = Affine.New(
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


                if (_registeredElemsById.TryGetValue(mayHasMarkers.MarkerStart.Value, out VgVisualElement marker))
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

                if (_registeredElemsById.TryGetValue(mayHasMarkers.MarkerMid.Value, out VgVisualElement marker))
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

                if (_registeredElemsById.TryGetValue(mayHasMarkers.MarkerEnd.Value, out VgVisualElement marker))
                {
                    pathRenderMarkers.EndMarker = marker;
                }
            }
        }
        VgVisualElement CreateLine(VgVisualElement parentNode, SvgLineSpec linespec)
        {
            VgVisualElement lineVisualElem = new VgVisualElement(WellknownSvgElementName.Line, linespec, _vgVisualDoc);
            using (Tools.BorrowVxs(out var v1))
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

            using (Tools.BorrowEllipse(out var ellipse))
            using (Tools.BorrowVxs(out var v1))
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
            //TODO: check if matrix is identify matrix or not            //
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
                if (child.WellknowElemName == WellknownSvgElementName.Stop)
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

            //https://developer.mozilla.org/en-US/docs/Web/SVG/Element/linearGradient

            //Categories Gradient element
            //Permitted content Any number of the following elements, in any order:
            //Descriptive elements
            //<animate>, <animateTransform>, <set>, <stop>

            //Attributes
            //Section
            //Global attributes
            //Section

            //    Core attributes
            //    Presentation attributes
            //    Xlink attributes
            //    class
            //    style
            //    externalResourcesRequired

            //Specific attributes
            //Section

            //    gradientUnits
            //    gradientTransform
            //    x1
            //    y1
            //    x2
            //    y2
            //    spreadMethod
            //    xlink:href



            VgVisualElement linearGrd = new VgVisualElement(WellknownSvgElementName.LinearGradient, spec, _vgVisualDoc);
            //read attribute
            RegisterElementById(elem, linearGrd);
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

                using (Tools.BorrowRoundedRect(out var roundRect))
                using (Tools.BorrowVxs(out var v1))
                {
                    ReEvaluateArgs a = new ReEvaluateArgs(_containerWidth, _containerHeight, _emHeight); //temp fix
                    roundRect.SetRect(
                        ConvertToPx(rectSpec.X, ref a),
                        ConvertToPx(rectSpec.Y, ref a) + ConvertToPx(rectSpec.Height, ref a),
                        ConvertToPx(rectSpec.X, ref a) + ConvertToPx(rectSpec.Width, ref a),
                        ConvertToPx(rectSpec.Y, ref a));

                    roundRect.SetRadius(
                        ConvertToPx(rectSpec.CornerRadiusX, ref a),
                        ConvertToPx(rectSpec.CornerRadiusY, ref a));

                    rect.VxsPath = roundRect.MakeVxs(v1).CreateTrim();
                }
            }
            else
            {

                using (Tools.BorrowRect(out var rectTool))
                using (Tools.BorrowVxs(out var v1))
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

        VertexStore CreateVxsFromPathDefinition(char[] pathDefinition)
        {
            using (Tools.BorrowCurveFlattener(out var curveFlattener))
            using (Tools.BorrowVxs(out var v1, out var v2))
            using (Tools.BorrowPathWriter(v1, out PathWriter pathWriter))
            {
                _pathDataParser.SetPathWriter(pathWriter);
                _pathDataParser.Parse(pathDefinition);
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

    public static class VgVisualDocHelper
    {
        public static VgVisualDoc CreateVgVisualDocFromFile(string filename)
        {

            VgDocBuilder docBuilder = new VgDocBuilder();

            SvgParser svgParser = new SvgParser(docBuilder);

            //TODO: don't access file system here, 
            //we should ask from 'host' for a file content or stream of filecontent
            string svgFileContent = System.IO.File.ReadAllText(filename);
            svgParser.ParseSvg(svgFileContent);
            //2. create visual tree from svg document and its spec
            VgVisualDocBuilder vgDocBuilder = new VgVisualDocBuilder();
            return vgDocBuilder.CreateVgVisualDoc(docBuilder.ResultDocument, null);
        }

    }






}