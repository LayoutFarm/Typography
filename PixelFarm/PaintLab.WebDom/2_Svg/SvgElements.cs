//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using LayoutFarm.WebDom;
using LayoutFarm.WebDom.Parser;
namespace PaintLab.Svg
{

    //------------------------------------------------------------------------------
    public enum WellknownSvgElementName
    {
        Unknown,
        /// <summary>
        /// svg  
        /// </summary>
        Svg,
        /// <summary>
        /// g
        /// </summary>
        Group,
        /// <summary>
        /// path
        /// </summary>
        Path,
        /// <summary>
        /// defs
        /// </summary>
        Defs,
        /// <summary>
        /// line
        /// </summary>
        Line,
        /// <summary>
        /// polyline
        /// </summary>
        Polyline,
        /// <summary>
        /// polygon
        /// </summary>
        Polygon,
        /// <summary>
        /// title
        /// </summary>
        Title,
        /// <summary>
        /// rect
        /// </summary>
        Rect,
        /// <summary>
        /// ellipse
        /// </summary>
        Ellipse,
        /// <summary>
        /// circle
        /// </summary>
        Circle,
        /// <summary>
        /// clipPath
        /// </summary>
        ClipPath,
        /// <summary>
        /// linear gradient
        /// </summary>
        LinearGradient,
        /// <summary>
        /// circular gradient
        /// </summary>
        RadialGradient,
        /// <summary>
        /// text
        /// </summary>
        Text,
        /// <summary>
        /// image
        /// </summary>
        Image,

        RootSvg,

        /// <summary>
        /// style
        /// </summary>
        Style,

        /// <summary>
        /// marker
        /// </summary>
        Marker,
        /// <summary>
        /// mask
        /// </summary>
        Mask,
        /// <summary>
        /// pattern
        /// </summary>
        Pattern,
        /// <summary>
        /// use
        /// </summary>
        Use,
        /// <summary>
        /// stop
        /// </summary>
        Stop,

        /// <summary>
        /// my extension
        /// </summary>
        ForeignNode
    }

    /// <summary>
    /// vg dom element
    /// </summary>
    public class SvgElement
    {
        readonly WellknownSvgElementName _wellknownName;
        readonly string _unknownElemName;

        SvgElemSpec _elemSpec;
        List<SvgElement> _childNodes;
        object _controller;
        

        public SvgElement(WellknownSvgElementName wellknownName, SvgElemSpec elemSpec)
        {
            _wellknownName = wellknownName;
            _elemSpec = elemSpec;
        }
        public SvgElement(WellknownSvgElementName wellknownName, string name)
        {
            _wellknownName = wellknownName;
            _unknownElemName = name;
        }
        public string ElemId { get; set; }
        public void SetController(object controller)
        {
            _controller = controller;
        }

        public static object UnsafeGetController(SvgElement elem)
        {
            return elem._controller;
        }

        public WellknownSvgElementName WellknowElemName { get { return _wellknownName; } }

        public string ElemName
        {
            get
            {
                switch (_wellknownName)
                {
                    default:
                        throw new NotSupportedException();
                    case WellknownSvgElementName.Unknown:
                        return _unknownElemName;
                    case WellknownSvgElementName.Circle: return "circle";
                    case WellknownSvgElementName.ClipPath: return "clipPath";
                    case WellknownSvgElementName.Rect: return "rect";
                    case WellknownSvgElementName.Path: return "path";
                    case WellknownSvgElementName.Polygon: return "polygon";
                    case WellknownSvgElementName.Polyline: return "polyline";
                    case WellknownSvgElementName.Line: return "line";
                    case WellknownSvgElementName.Defs: return "defs";
                    case WellknownSvgElementName.Title: return "title";
                    case WellknownSvgElementName.Image: return "image";
                    case WellknownSvgElementName.Text: return "text";
                    case WellknownSvgElementName.LinearGradient: return "linearGradient";
                    case WellknownSvgElementName.RadialGradient: return "radialGradient";
                    case WellknownSvgElementName.Use: return "use";
                    case WellknownSvgElementName.Stop: return "stop";
                }
            }
        }
        public virtual void AddElement(SvgElement elem)
        {
            if (_childNodes == null)
            {
                _childNodes = new List<SvgElement>();
            }
            _childNodes.Add(elem);
        }

        public int ChildCount
        {
            get { return _childNodes == null ? 0 : _childNodes.Count; }
        }
        public SvgElement GetChild(int index)
        {
            return _childNodes[index];
        }
        public SvgElemSpec ElemSpec
        {
            get { return _elemSpec; }
        }

    }

    public interface ISvgDocBuilder
    {
        void OnBegin();
        void OnVisitNewElement(string elemName);

        void OnAttribute(string attrName, string value);
        void OnEnteringElementBody();
        void OnTextNode(string text);
        void OnExitingElementBody();
        void OnEnd();
    }

    public class SvgDocument
    {
        SvgElement _rootElement = new SvgElement(WellknownSvgElementName.Svg, null as string);
        public SvgDocument()
        {
        }

        public SvgElement CreateElement(string elemName)
        {
            //TODO: review here again***
            //------
            switch (elemName)
            {
                default:
#if DEBUG
                    //Console.WriteLine("svg unimplemented element: " + elemName);
#endif
                    return new SvgElement(WellknownSvgElementName.Unknown, elemName);
                case "svg":
                    return new SvgElement(WellknownSvgElementName.Svg, new SvgBoxSpec());

                case "defs":
                    return new SvgElement(WellknownSvgElementName.Defs, null as string);
                case "title":
                    return new SvgElement(WellknownSvgElementName.Title, null as string);

                //------------------------------------------------------------------------------
                case "style":
                    return new SvgElement(WellknownSvgElementName.Style, new SvgStyleSpec());
                //------------------------------------------------------------------------------
                case "marker":
                    return new SvgElement(WellknownSvgElementName.Marker, new SvgMarkerSpec());
                case "text":
                    return new SvgElement(WellknownSvgElementName.Text, new SvgTextSpec());
                case "clipPath":
                    return new SvgElement(WellknownSvgElementName.ClipPath, new SvgPathSpec());
                case "g":
                    return new SvgElement(WellknownSvgElementName.Group, new SvgGroupSpec());
                case "rect":
                    return new SvgElement(WellknownSvgElementName.Rect, new SvgRectSpec());
                case "line":
                    return new SvgElement(WellknownSvgElementName.Line, new SvgLineSpec());
                case "polyline":
                    return new SvgElement(WellknownSvgElementName.Polyline, new SvgPolylineSpec());
                case "polygon":
                    return new SvgElement(WellknownSvgElementName.Polygon, new SvgPolygonSpec());
                case "path":
                    return new SvgElement(WellknownSvgElementName.Path, new SvgPathSpec());
                case "image":
                    return new SvgElement(WellknownSvgElementName.Image, new SvgImageSpec());
                //case "linearGradient":
                //    return new SvgElement(WellknownSvgElementName.LinearGradient, new SvgLinearGradientSpec());
                //case "radialGradient":
                //    return new SvgElement(WellknownSvgElementName.RadialGradient, new SvgRadialGradientSpec());
                //case "stop":
                //    return new SvgElement(WellknownSvgElementName.Stop, new SvgColorStopSpec());
                case "circle":
                    return new SvgElement(WellknownSvgElementName.Circle, new SvgCircleSpec());
                case "ellipse":
                    return new SvgElement(WellknownSvgElementName.Ellipse, new SvgEllipseSpec());
                case "use":
                    return new SvgElement(WellknownSvgElementName.Use, new SvgUseSpec());

            }
        }

        public SvgElement Root
        {
            get { return _rootElement; }
        }
        public CssActiveSheet CssActiveSheet { get; set; }


        //hint
        public string OriginalContent { get; set; }
        public string OriginalFilename { get; set; }
    }

    public class SvgDocBuilder : ISvgDocBuilder
    {
        Stack<SvgElement> _elems = new Stack<SvgElement>();

        SvgElementSpecEvaluator _specEvaluator = new SvgElementSpecEvaluator();
        SvgElement _currentElem;
        SvgDocument _svgDoc;

        public SvgDocBuilder()
        {

        }
        public SvgDocument ResultDocument
        {
            get { return _svgDoc; }
            set { _svgDoc = value; }
        }
        public SvgElement CurrentSvgElem
        {
            get { return _currentElem; }
        }

        public void OnBegin()
        {
            _elems.Clear();//** reset

            if (_svgDoc == null)
            {
                _svgDoc = new SvgDocument();
            }
            _currentElem = _svgDoc.Root;
        }
        public void OnVisitNewElement(string elemName)
        {
            SvgElement newElem = _svgDoc.CreateElement(elemName);
            if (_currentElem != null)
            {
                _elems.Push(_currentElem);
                _currentElem.AddElement(newElem);
            }
            _currentElem = newElem;
            _specEvaluator.SetCurrentElement(_currentElem);
        }

        public void OnAttribute(string attrName, string value)
        {
            _specEvaluator.OnAttribute(attrName, value);
        }
        public void OnEnteringElementBody()
        {

        }
        public void OnTextNode(string text)
        {
            _specEvaluator.OnTextNode(text);
        }
        public void OnExitingElementBody()
        {

            //

            if (_elems.Count > 0)
            {
                _currentElem = _elems.Pop();
            }
        }

        public void OnEnd()
        {
        }
    }

    class SvgElementSpecEvaluator
    {
        CssParser _cssParser = new CssParser();
        SvgElement _currentElem;
        public void SetCurrentElement(SvgElement elem)
        {
            _currentElem = elem;
        }
        void AddStyle(SvgVisualSpec spec, string cssStyle)
        {
            if (!String.IsNullOrEmpty(cssStyle))
            {
                //***                
                CssRuleSet cssRuleSet = _cssParser.ParseCssPropertyDeclarationList(cssStyle.ToCharArray());

                foreach (CssPropertyDeclaration propDecl in cssRuleSet.GetAssignmentIter())
                {
                    switch (propDecl.WellknownPropertyName)
                    {
                        default:
                            //handle unknown prop name
                            {
                                switch (propDecl.UnknownRawName)
                                {

                                    default:
                                        break;

                                    case "fill-opacity":
                                        {
                                            //TODO:
                                            //adjust fill opacity
                                        }
                                        break;
                                    case "stroke-width":
                                        {
                                            int valueCount = propDecl.ValueCount;
                                            //1
                                            string value = propDecl.GetPropertyValue(0).ToString();
                                            spec.StrokeWidth = UserMapUtil.ParseGenericLength(value);
                                        }
                                        break;
                                    case "stroke":
                                        {
                                            //stroke color

                                            //TODO:
                                            //if (attr.Value != "none")
                                            //{
                                            //    spec.StrokeColor = ConvToActualColor(CssValueParser2.GetActualColor(attr.Value));
                                            //}
                                        }
                                        break;
                                    case "stroke-linecap":
                                        //set line-cap and line join again
                                        //TODO:
                                        break;
                                    case "stroke-linejoin":
                                        //TODO:
                                        break;
                                    case "stroke-miterlimit":
                                        //TODO:
                                        break;
                                    case "stroke-opacity":
                                        //TODO:
                                        break;
                                    case "transform":
                                        {
                                            ////parse trans
                                            //ParseTransform(attr.Value, spec);
                                        }
                                        break;
                                }
                            }
                            break;
                        case WellknownCssPropertyName.Font:
                            break;
                        case WellknownCssPropertyName.FontFamily:
                            break;
                        case WellknownCssPropertyName.FontWeight:
                            break;
                        case WellknownCssPropertyName.FontStyle:
                            break;
                        case WellknownCssPropertyName.Fill:
                            {
                                int valueCount = propDecl.ValueCount;
                                //1
                                string value = propDecl.GetPropertyValue(0).ToString();
                                if (value != "none")
                                {

                                    spec.FillColor = CssValueParser.ParseCssColor(value);
                                }
                            }
                            break;
                    }

                }
            }
        }

        static void AssignTextSpecData(SvgTextSpec textspec, string attrName, string attrValue)
        {
            switch (attrName)
            {
                //rect 
                case "x":
                    textspec.X = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "y":
                    textspec.Y = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "font":
                    //parse font
                    break;
                case "font-family":
                    textspec.FontFamily = attrValue;
                    break;
                case "font-size":
                    textspec.FontSize = UserMapUtil.ParseGenericLength(attrValue);
                    break;
            }
        }
        static void AssignMarkerSpec(SvgMarkerSpec markderSpec, string attrName, string attrValue)
        {
            switch (attrName)
            {
                //rect 
                case "refX":
                    markderSpec.RefX = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "refY":
                    markderSpec.RefY = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "markerWidth":
                    markderSpec.MarkerWidth = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "markerHeight":
                    markderSpec.MarkerHeight = UserMapUtil.ParseGenericLength(attrValue);
                    break;
            }
        }
        static void AssignUseSpec(SvgUseSpec useSpec, string attrName, string attrValue)
        {
            switch (attrName)
            {
                //rect 
                case "x":
                    useSpec.X = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "y":
                    useSpec.Y = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "href":
                    useSpec.Href = ParseAttributeLink(attrValue);
                    break;
            }
        }
        static void AssignLinearGradientSpec(SvgLinearGradientSpec spec, string attrName, string attrValue)
        {
            switch (attrName)
            {
                //rect 
                case "x1":
                    spec.X1 = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "y1":
                    spec.Y1 = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "x2":
                    spec.X2 = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "y2":
                    spec.Y2 = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "gradientTransform":
                    SvgParser.ParseTransform(attrValue, spec);
                    break;
            }
        }
        static PixelFarm.Drawing.PointF[] ParsePointList(string str)
        {
            //
            List<PixelFarm.Drawing.PointF> output = new List<PixelFarm.Drawing.PointF>();
            ParsePointList(str, output);
            return output.ToArray();
        }

        static readonly char[] strSeps = new char[] { ' ', ',' };
        static void ParsePointList(string str, List<PixelFarm.Drawing.PointF> output)
        {
            //easy parse 01
            string[] allPoints = str.Split(strSeps, StringSplitOptions.RemoveEmptyEntries);
            //should be even number
            int j = allPoints.Length - 1;
            if (j > 1)
            {

                for (int i = 0; i < j; i += 2)
                {
                    float x, y;
                    if (!float.TryParse(allPoints[i], out x))
                    {
                        x = 0;
                    }
                    if (!float.TryParse(allPoints[i + 1], out y))
                    {
                        y = 0;
                    }
                    output.Add(new PixelFarm.Drawing.PointF(x, y));
                }
            }
        }
        static void AssignRectSpec(SvgRectSpec spec, string attrName, string attrValue)
        {
            switch (attrName)
            {
                //rect 
                case "x":
                    spec.X = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "y":
                    spec.Y = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "width":
                    spec.Width = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "height":
                    spec.Height = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "rx":
                    spec.CornerRadiusX = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "ry":
                    spec.CornerRadiusY = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                default:

                    break;
            }
        }
        static void AssignImageSpec(SvgImageSpec spec, string attrName, string attrValue)
        {
            switch (attrName)
            {
                case "x":
                    spec.X = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "y":
                    spec.Y = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "width":
                    spec.Width = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "height":
                    spec.Height = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "href":
                    //image spec
                    spec.ImageSrc = attrValue;//TODO: check if it is a valid value/path
                    break;
            }
        }
        static void AssignMarker(IMayHaveMarkers mayHasMarker, string attrName, string attrValue)
        {
            switch (attrName)
            {
                case "marker-start":
                    mayHasMarker.MarkerStart = ParseAttributeLink(attrValue);
                    break;
                case "marker-mid":
                    mayHasMarker.MarkerMid = ParseAttributeLink(attrValue);
                    break;
                case "marker-end":
                    mayHasMarker.MarkerEnd = ParseAttributeLink(attrValue);
                    break;
            }
        }
        static void AssignPolygonSpec(SvgPolygonSpec spec, string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:
                    AssignMarker(spec, attrName, attrValue);
                    break;
                case "points":
                    //image spec
                    spec.Points = ParsePointList(attrValue);
                    break;

            }
        }
        static void AssignPathSpecData(SvgPathSpec spec, string attrName, string attrValue)
        {
            switch (attrName)
            {
                case "d":
                    spec.D = attrValue;
                    break;
                //---------------------
                default:
                    AssignMarker(spec, attrName, attrValue);
                    break;
            }
        }

        static void AssignPolylineSpec(SvgPolylineSpec spec, string attrName, string attrValue)
        {
            switch (attrName)
            {   //---------------------
                default:
                    AssignMarker(spec, attrName, attrValue);
                    break;
                case "points":
                    //image spec
                    spec.Points = ParsePointList(attrValue);
                    break;
            }
        }
        static void AssignPolylineSpec(SvgLineSpec spec, string attrName, string attrValue)
        {

            AssignMarker(spec, attrName, attrValue);

        }

        static void AssignEllipseSpec(SvgEllipseSpec spec, string attrName, string attrValue)
        {

            switch (attrName)
            {
                case "cx":
                    //image spec
                    spec.X = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "cy":
                    spec.Y = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "rx":
                    spec.RadiusX = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "ry":
                    spec.RadiusY = UserMapUtil.ParseGenericLength(attrValue);
                    break;
            }
        }
        static void AssignCircleSpec(SvgCircleSpec spec, string attrName, string attrValue)
        {
            switch (attrName)
            {
                case "cx":
                    //image spec
                    spec.X = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "cy":
                    spec.Y = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "r":
                    spec.Radius = UserMapUtil.ParseGenericLength(attrValue);
                    break;
            }
        }
        static void AssignSvgBoxSpec(SvgBoxSpec spec, string attrName, string attrValue)
        {
            switch (attrName)
            {
                case "viewBox":
                    {
                        string[] allPoints = attrValue.Split(strSeps, StringSplitOptions.RemoveEmptyEntries);
                        if (allPoints.Length == 4)
                        {
                            //x,y,w,h 
                            float num;
                            spec.ViewBoxX = float.TryParse(allPoints[0], out num) ? num : 0;
                            spec.ViewBoxY = float.TryParse(allPoints[1], out num) ? num : 0;
                            spec.ViewBoxW = float.TryParse(allPoints[2], out num) ? num : 0;
                            spec.ViewBoxH = float.TryParse(allPoints[3], out num) ? num : 0;
                        }
                    }
                    break;
            }
        }

        ////------------------------------------------------------------
        //int j = elem.ChildrenCount;
        //List<StopColorPoint> stopColorPoints = new List<StopColorPoint>(j);
        //for (int i = 0; i < j; ++i)
        //{
        //    HtmlElement node = elem.GetChildNode(i) as HtmlElement;
        //    if (node == null)
        //    {
        //        continue;
        //    }
        //    switch (node.WellknownElementName)
        //    {
        //        case WellKnownDomNodeName.svg_stop:
        //            {
        //                //stop point
        //                StopColorPoint stopPoint = new StopColorPoint();
        //                foreach (WebDom.DomAttribute attr in node.GetAttributeIterForward())
        //                {
        //                    WebDom.WellknownName wellknownName = (WebDom.WellknownName)attr.LocalNameIndex;
        //                    switch (wellknownName)
        //                    {
        //                        case WellknownName.Svg_StopColor:
        //                            {
        //                                stopPoint.StopColor = CssValueParser2.ParseCssColor(attr.Value);
        //                            }
        //                            break;
        //                        case WellknownName.Svg_Offset:
        //                            {
        //                                stopPoint.Offset = UserMapUtil.ParseGenericLength(attr.Value);
        //                            }
        //                            break;
        //                    }
        //                }
        //                stopColorPoints.Add(stopPoint);
        //            }
        //            break;
        //    }
        //}

        public void OnTextNode(string content)
        {
            if (_currentElem.ElemName == "text")
            {
                SvgTextSpec elemSpec = (SvgTextSpec)_currentElem.ElemSpec;
                elemSpec.TextContent = content;
            }


        }
        public void OnAttribute(string attrName, string value)
        {
            SvgElemSpec elemSpec = _currentElem.ElemSpec;
            if (elemSpec == null) return;

            SvgVisualSpec spec = elemSpec as SvgVisualSpec;
            switch (attrName)
            {
                default:
                    {
                        //unknown attribute
                        //some specfic attr for some elem
                        switch (_currentElem.WellknowElemName)
                        {
                            default:

                                break;
                            case WellknownSvgElementName.Use:
                                AssignUseSpec((SvgUseSpec)spec, attrName, value);
                                break;
                            case WellknownSvgElementName.Marker:
                                AssignMarkerSpec((SvgMarkerSpec)spec, attrName, value);
                                break;
                            case WellknownSvgElementName.Text:
                                AssignTextSpecData((SvgTextSpec)spec, attrName, value);
                                break;
                            case WellknownSvgElementName.Path:
                                AssignPathSpecData((SvgPathSpec)spec, attrName, value);
                                break;
                            case WellknownSvgElementName.Rect:
                                AssignRectSpec((SvgRectSpec)spec, attrName, value);
                                break;
                            case WellknownSvgElementName.LinearGradient:
                                AssignLinearGradientSpec((SvgLinearGradientSpec)spec, attrName, value);
                                break;
                            case WellknownSvgElementName.Polyline:
                                AssignPolylineSpec((SvgPolylineSpec)spec, attrName, value);
                                break;
                            case WellknownSvgElementName.Polygon:
                                AssignPolygonSpec((SvgPolygonSpec)spec, attrName, value);
                                break;
                            case WellknownSvgElementName.Image:
                                AssignImageSpec((SvgImageSpec)spec, attrName, value);
                                break;
                            case WellknownSvgElementName.Ellipse:
                                AssignEllipseSpec((SvgEllipseSpec)spec, attrName, value);
                                break;
                            case WellknownSvgElementName.Circle:
                                AssignCircleSpec((SvgCircleSpec)spec, attrName, value);
                                break;
                            case WellknownSvgElementName.Svg:
                                AssignSvgBoxSpec((SvgBoxSpec)spec, attrName, value);
                                break;
                        }
                    }
                    break;
                case "class":
                    spec.Class = value; //solve it later
                    break;
                case "id":
                    _currentElem.ElemId = value; //?
                    break;
                case "style":
                    AddStyle(spec, value);
                    break;
                case "clip-path":
                    AddClipPathLink(spec, value);
                    break;
                case "fill":
                    {
                        if (value != "none")
                        {
                            spec.FillColor = CssValueParser.ParseCssColor(value);
                        }
                    }
                    break;
                case "fill-opacity":
                    {
                        //adjust fill opacity
                        //0f-1f?

                    }
                    break;
                case "stroke-width":
                    {
                        spec.StrokeWidth = UserMapUtil.ParseGenericLength(value);
                    }
                    break;
                case "stroke":
                    {
                        if (value != "none")
                        {
                            //spec.StrokeColor = ConvToActualColor(CssValueParser2.GetActualColor(value));
                            spec.StrokeColor = CssValueParser.ParseCssColor(value);
                        }
                    }
                    break;
                case "stroke-linecap":
                    //set line-cap and line join again

                    break;
                case "stroke-linejoin":

                    break;
                case "stroke-miterlimit":

                    break;
                case "stroke-opacity":

                    break;
                case "transform":
                    {
                        //parse trans
                        SvgParser.ParseTransform(value, spec);
                    }
                    break;

            }
        }

        static SvgAttributeLink ParseAttributeLink(string value)
        {
            //eg. url(#aaa)
            if (value.StartsWith("url("))
            {
                int endAt = value.IndexOf(')', 4);
                if (endAt > -1)
                {
                    //get value 
                    string url_value = value.Substring(4, endAt - 4);
                    return new SvgAttributeLink(SvgAttributeLinkKind.Id, url_value.Substring(1));
                }
                else
                {

                }
            }
            else if (value.StartsWith("#"))
            {
                return new SvgAttributeLink(SvgAttributeLinkKind.Id, value.Substring(1));
            }
            else
            {

            }
            return null;
        }
        static void AddClipPathLink(SvgVisualSpec spec, string value)
        {
            //eg. url(#aaa)
            SvgAttributeLink attrLink = ParseAttributeLink(value);
            if (attrLink != null)
            {
                spec.ClipPathLink = attrLink;
            }

        }

    }


}