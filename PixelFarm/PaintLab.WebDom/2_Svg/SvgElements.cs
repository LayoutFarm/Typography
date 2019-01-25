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
        /// filter
        /// </summary>
        Filter,

        /// <summary>
        /// feColorMatrix
        /// </summary>
        FeColorMatrix,

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
#if DEBUG
        static int s_dbugTotalId;
        public readonly int dbugId = s_dbugTotalId++;
#endif
        public SvgElement(WellknownSvgElementName wellknownName, SvgElemSpec elemSpec)
        {
#if DEBUG
            if (dbugId == 3)
            {

            }
#endif
            _wellknownName = wellknownName;
            _elemSpec = elemSpec;
        }
        public SvgElement(WellknownSvgElementName wellknownName, string name)
        {
#if DEBUG
            if (dbugId == 3)
            {

            }
#endif
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

        public WellknownSvgElementName WellknowElemName => _wellknownName;

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
                    case WellknownSvgElementName.Ellipse: return "ellipse";
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
                    case WellknownSvgElementName.Filter: return "filter";
                    case WellknownSvgElementName.FeColorMatrix: return "feColorMetrix";

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

        public int ChildCount => _childNodes == null ? 0 : _childNodes.Count;

        public SvgElement GetChild(int index) => _childNodes[index];

        public SvgElemSpec ElemSpec => _elemSpec;
#if DEBUG
        public override string ToString()
        {
            return _wellknownName.ToString();
        }
#endif

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
                    System.Diagnostics.Debug.WriteLine("svg unimplemented element: " + elemName);
#endif
                    return new SvgElement(WellknownSvgElementName.Unknown, elemName);
                case "svg":
                    return new SvgElement(WellknownSvgElementName.Svg, new SvgBoxSpec());
                case "defs":
                    return new SvgElement(WellknownSvgElementName.Defs, null as string);
                case "title":
                    return new SvgElement(WellknownSvgElementName.Title, null as string);
                case "filter":
                    return new SvgElement(WellknownSvgElementName.Filter, new SvgFilterSpec());
                case "feColorMatrix":
                    return new SvgElement(WellknownSvgElementName.FeColorMatrix, new SvgFeColorMatrixSpec());
                case "mask":
                    return new SvgElement(WellknownSvgElementName.Mask, new SvgMaskSpec());
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
                case "linearGradient":
                    return new SvgElement(WellknownSvgElementName.LinearGradient, new SvgLinearGradientSpec());
                case "radialGradient":
                    return new SvgElement(WellknownSvgElementName.RadialGradient, new SvgRadialGradientSpec());
                case "stop":
                    return new SvgElement(WellknownSvgElementName.Stop, new SvgColorStopSpec());
                case "circle":
                    return new SvgElement(WellknownSvgElementName.Circle, new SvgCircleSpec());
                case "ellipse":
                    return new SvgElement(WellknownSvgElementName.Ellipse, new SvgEllipseSpec());
                case "use":
                    return new SvgElement(WellknownSvgElementName.Use, new SvgUseSpec());

            }
        }

        public SvgElement Root => _rootElement;

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
            get => _svgDoc;
            set => _svgDoc = value;
        }
        public SvgElement CurrentSvgElem => _currentElem;

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
                                    case "opacity":
                                        {

                                        }
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
        static void AssignFilterSpec(SvgFilterSpec filterSpec, string attrName, string attrValue)
        {
            switch (attrName)
            {
                //rect 
                case "x":
                    filterSpec.X = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "y":
                    filterSpec.Y = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "width":
                    filterSpec.Width = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "height":
                    filterSpec.Height = UserMapUtil.ParseGenericLength(attrValue);
                    break;
            }
        }
        static void AssignMaskSpec(SvgMaskSpec maskSpec, string attrName, string attrValue)
        {

            switch (attrName)
            {
                //rect 
                case "x":
                    maskSpec.X = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "y":
                    maskSpec.Y = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "width":
                    maskSpec.Width = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "height":
                    maskSpec.Height = UserMapUtil.ParseGenericLength(attrValue);
                    break;
            }

        }
        static void AssignFeColorMatrixSpec(SvgFeColorMatrixSpec feColorMatrixSpec, string attrName, string attrValue)
        {
            switch (attrName)
            {
                case "values":
                    {
                        List<float> numberList = new List<float>();
                        ParseNumberList(attrValue, numberList);
                        //last row
                        numberList.Add(0); numberList.Add(0); numberList.Add(0); numberList.Add(0); numberList.Add(1);
                        //
                        feColorMatrixSpec.matrix = numberList.ToArray();
                    }
                    break;
            }
        }
        static void AssignGroupSpec(SvgGroupSpec groupSpec, string attrName, string attrValue)
        {
            switch (attrName)
            {
                case "filter":
                    {
                        //value may be in refer form
                        SvgAttributeLink attrLink = ParseAttributeLink(attrValue);
                        if (attrLink != null)
                        {
                            //resolve later
                            groupSpec.FilterPathLink = attrLink;
                        }
                    }
                    break;
                case "mask":
                    {

                    }
                    break;
            }
        }
        static void AssignLineSpec(SvgLineSpec spec, string attrName, string attrValue)
        {
            switch (attrName)
            {

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
            }
        }
        static void AssignStopColorSpec(SvgColorStopSpec spec, string attrName, string attrValue)
        {
            switch (attrName)
            {
                //rect 
                case "stop-color":
                    spec.StopColor = CssValueParser.ParseCssColor(attrValue);
                    break;
                case "offset":
                    //default unit of offset=%?
                    spec.Offset = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "stop-opacity":
                    {
                        if (float.TryParse(attrValue, out float result))
                        {
                            if (result < 0)
                            {
                                spec.StopOpacity = 0;
                            }
                            else if (result > 1)
                            {
                                spec.StopOpacity = 1;
                            }
                            else
                            {
                                spec.StopOpacity = result;
                            }
                        }
                    }
                    break;
            }
        }
        static void AssignRadialGradientSpec(SvgRadialGradientSpec spec, string attrName, string attrValue)
        {
            switch (attrName)
            {
                //rect 
                case "cx":
                    spec.CX = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "cy":
                    spec.CY = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "fx":
                    spec.FX = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "fy":
                    spec.FY = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "fr":
                    spec.FR = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "r":
                    spec.R = UserMapUtil.ParseGenericLength(attrValue);
                    break;
                case "gradientTransform":
                    SvgParser.ParseTransform(attrValue, spec);
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


        static readonly char[] strSeps1 = new char[] { ' ', ',' };
        static void ParsePointList(string str, List<PixelFarm.Drawing.PointF> output)
        {
            //easy parse 01
            string[] allPoints = str.Split(strSeps1, StringSplitOptions.RemoveEmptyEntries);
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

        static void ParseNumberList(string str, List<float> output)
        {
            //easy parse 01
            string[] allPoints = str.Split(strSeps1, StringSplitOptions.RemoveEmptyEntries);
            //should be even number 
            for (int i = 0; i < allPoints.Length; i++)
            {
                float x;
                if (!float.TryParse(allPoints[i], out x))
                {
                    x = 0;
                }
                output.Add(x);
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
                        string[] allPoints = attrValue.Split(strSeps1, StringSplitOptions.RemoveEmptyEntries);
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
                //if it is not common attribute name
                //then go to specific current element name 

                default:
                    {
                        //unknown attribute
                        //some specfic attr for some elem
                        switch (_currentElem.WellknowElemName)
                        {
                            default:
                                {
                                    switch (_currentElem.ElemName)
                                    {

                                    }
                                }
                                break;
                            case WellknownSvgElementName.Mask:
                                AssignMaskSpec((SvgMaskSpec)elemSpec, attrName, value);
                                break;
                            case WellknownSvgElementName.FeColorMatrix:
                                AssignFeColorMatrixSpec((SvgFeColorMatrixSpec)elemSpec, attrName, value);
                                break;
                            case WellknownSvgElementName.Filter:
                                AssignFilterSpec((SvgFilterSpec)elemSpec, attrName, value);
                                break;
                            case WellknownSvgElementName.Group:
                                AssignGroupSpec((SvgGroupSpec)elemSpec, attrName, value);
                                break;
                            case WellknownSvgElementName.Line:
                                AssignLineSpec((SvgLineSpec)elemSpec, attrName, value);
                                break;
                            case WellknownSvgElementName.Stop:
                                AssignStopColorSpec((SvgColorStopSpec)elemSpec, attrName, value);
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
                            case WellknownSvgElementName.RadialGradient:
                                AssignRadialGradientSpec((SvgRadialGradientSpec)spec, attrName, value);
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
                            if (value.StartsWith("url("))
                            {
                                //eg. url(#aaa)
                                SvgAttributeLink attrLink = ParseAttributeLink(value);
                                if (attrLink != null)
                                {
                                    spec.FillPathLink = attrLink;
                                }
                            }
                            else
                            {
                                //solid brush
                                spec.FillColor = CssValueParser.ParseCssColor(value);
                            }
                        }
                    }
                    break;
                case "mask":
                    {
                        //eg. url(#aaa)
                        SvgAttributeLink attrLink = ParseAttributeLink(value);
                        if (attrLink != null)
                        {
                            //resolve later
                            spec.MaskPathLink = attrLink;
                        }
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
                case "opacity":
                    {
                        //apply opacity
                        //TODO: review here, UserMapUtil => use CssValueParser
                        spec.Opacity = UserMapUtil.ParseGenericLength(value).Number;
                    }
                    break;
                case "fill-opacity":
                    {
                        //adjust fill opacity
                        //0f-1f?

                    }
                    break;
                case "stroke-opacity":
                    {

                    }
                    break;
                case "stroke-linecap":
                    //set line-cap and line join again

                    break;
                case "stroke-linejoin":

                    break;
                case "stroke-miterlimit":

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