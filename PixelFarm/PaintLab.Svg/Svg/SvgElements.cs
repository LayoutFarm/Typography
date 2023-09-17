//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic;
namespace PaintLab.Svg
{
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
            //if (dbugId == 3)
            //{

            //}
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

        public void SetController(object controller) => _controller = controller;
        public static object UnsafeGetController(SvgElement elem) => elem._controller;


        public string ElemId { get; set; }

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

    public class SvgElement<T> : SvgElement
        where T : SvgElemSpec
    {
        public SvgElement(WellknownSvgElementName wellknownName, T elemSpec)
            : base(wellknownName, elemSpec)
        {
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



}