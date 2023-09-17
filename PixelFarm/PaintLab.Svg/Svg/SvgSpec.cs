//Apache2, 2014-present, WinterDev
//MS-PL,  

using LayoutFarm.Css;
using PixelFarm.Drawing;

namespace PaintLab.Svg
{

    public abstract class SvgElemSpec
    {

    }
    public abstract class SvgVisualSpec : SvgElemSpec
    {
        Color _fillColor = Color.Black;
        Color _strokeColor = Color.Transparent;
        CssLength _strokeWidth;
        float _opacity;
        SvgAttributeLink _fillColorLink;

        public bool HasFillColor { get; set; }
        public bool HasStrokeColor { get; set; }
        public bool HasStrokeWidth { get; set; }
        public bool HasOpacity { get; set; }
        public bool HasMask { get; set; }
        public bool HasFilter { get; set; }

        public SvgTransform Transform { get; set; }

        public Color FillColor
        {
            get => _fillColor;
            set
            {
                _fillColor = value;
                this.HasFillColor = true;
            }
        }
        public Color StrokeColor
        {
            get => _strokeColor;
            set
            {
                _strokeColor = value;
                this.HasStrokeColor = true;
            }
        }
        public CssLength StrokeWidth
        {
            get => _strokeWidth;
            set
            {
                _strokeWidth = value;
                this.HasStrokeWidth = true;
            }
        }
        public string Class { get; set; }
        public float Opacity
        {
            get => _opacity;
            set
            {
                _opacity = value;
                this.HasOpacity = true;
            }
        }
        public SvgClipRule ClipRule { get; set; }
        public SvgFillRule FillRule { get; set; }

        public SvgAttributeLink ClipPathLink { get; set; }
        public SvgAttributeLink FillPathLink
        {
            get => _fillColorLink;
            set
            {
                _fillColorLink = value;
                HasFillColor = true;
            }
        }
        public SvgAttributeLink MaskPathLink { get; set; }
        public SvgAttributeLink FilterPathLink { get; set; }
        public object ResolvedClipPath { get; set; } //TODO: review here 
        public object ResolvedFillBrush { get; set; }//TODO: review here 
        public object ResolvedMask { get; set; }
        public object ResolvedFilter { get; set; }

    }
    public class SvgGroupSpec : SvgVisualSpec
    {

    }
    public class SvgBoxSpec : SvgVisualSpec
    {
        public float ViewBoxX { get; set; }
        public float ViewBoxY { get; set; }
        public float ViewBoxW { get; set; }
        public float ViewBoxH { get; set; }
    }



    public class SvgStyleSpec : SvgElemSpec
    {
        public string RawTextContent { get; set; }
        public LayoutFarm.WebDom.CssActiveSheet CssSheet { get; set; }
    }

    public class SvgUseSpec : SvgVisualSpec
    {
        public SvgAttributeLink Href { get; set; }
        public CssLength X { get; set; }
        public CssLength Y { get; set; }
    }

    public enum SvgAttributeLinkKind
    {
        Id,
    }
    public class SvgAttributeLink
    {
        public SvgAttributeLink(SvgAttributeLinkKind kind, string value)
        {
            this.Kind = kind;
            this.Value = value;
        }
        public string Value { get; private set; }
        public SvgAttributeLinkKind Kind { get; private set; }
#if DEBUG
        public override string ToString()
        {
            return Value;
        }
#endif
    }

    public class SvgRectSpec : SvgVisualSpec
    {
        public SvgRectSpec() { }
        public CssLength X { get; set; }
        public CssLength Y { get; set; }
        public CssLength Width { get; set; }
        public CssLength Height { get; set; }
        public CssLength CornerRadiusX { get; set; }
        public CssLength CornerRadiusY { get; set; }
    }

    public enum SvgContentUnit : byte
    {
        Unknown,
        /// <summary>
        /// userSpaceOnUse
        /// </summary>
        UserSpaceOnUse,
        /// <summary>
        /// objectBoundingBox
        /// </summary>
        ObjectBoudingBox,
    }

    public enum SvgClipRule : byte
    {
        NoneZero,
        EvenOdd,
        Inherit,
        Unknown,
    }
    public enum SvgFillRule : byte
    {
        //https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/fill-rule

        NoneZero,
        EvenOdd,
        Unknown,
    }
    public class SvgFeColorMatrixSpec : SvgVisualSpec
    {
        public float[] matrix;
    }
    public class SvgFilterSpec : SvgVisualSpec
    {
        public SvgFilterSpec() { }
        public CssLength X { get; set; }
        public CssLength Y { get; set; }
        public CssLength Width { get; set; }
        public CssLength Height { get; set; }
        public SvgContentUnit FilterUnit { get; set; }
    }

    public class SvgCircleSpec : SvgVisualSpec
    {
        public CssLength X { get; set; }
        public CssLength Y { get; set; }
        public CssLength Radius { get; set; }
    }
    public class SvgImageSpec : SvgVisualSpec
    {
        public CssLength X { get; set; }
        public CssLength Y { get; set; }
        public CssLength Width { get; set; }
        public CssLength Height { get; set; }

        public string ImageSrc { get; set; }
    }

    public class SvgEllipseSpec : SvgVisualSpec
    {
        public CssLength X { get; set; }
        public CssLength Y { get; set; }
        public CssLength RadiusX { get; set; }
        public CssLength RadiusY { get; set; }
    }



    public interface IMayHaveMarkers
    {
        SvgAttributeLink MarkerStart { get; set; }
        SvgAttributeLink MarkerMid { get; set; }
        SvgAttributeLink MarkerEnd { get; set; }
    }

    public class SvgLinearGradientSpec : SvgVisualSpec
    {
        public System.Collections.Generic.List<SvgColorStopSpec> StopList { get; set; }
        public CssLength X1 { get; set; }
        public CssLength Y1 { get; set; }
        public CssLength X2 { get; set; }
        public CssLength Y2 { get; set; }
    }

    public class SvgRadialGradientSpec : SvgVisualSpec
    {
        //https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/gradientUnits

        public System.Collections.Generic.List<SvgColorStopSpec> StopList { get; set; }
        public CssLength CX { get; set; }
        public CssLength CY { get; set; }
        /// <summary>
        /// radius of circle
        /// </summary>
        public CssLength R { get; set; }
        /// <summary>
        /// fx, focal x
        /// </summary>
        public CssLength FX { get; set; }
        /// <summary>
        /// fy, focal y
        /// </summary>
        public CssLength FY { get; set; }
        /// <summary>
        /// fr, focal radius
        /// </summary>
        public CssLength FR { get; set; }
        /// <summary>
        ///  It determines how a shape is filled beyond the defined edges of the gradient.
        /// </summary>
        public SpreadMethod SpreadMethod { get; set; }
        public SvgContentUnit GradientUnits { get; set; }
    }
    /// <summary>
    ///  It determines how a shape is filled beyond the defined edges of the gradient.
    /// </summary>
    public enum SpreadMethod
    {
        //pad
        //The final color of the gradient fills the shape beyond the gradient's edges.
        Pad,

        //reflect
        //The gradient repeats in reverse beyond its edges.
        Reflect,

        //repeat
        //The gradient repeats in the original order beyond its edges.
        Repeat
    }

    public class SvgColorStopSpec : SvgElemSpec
    {
        public CssLength Offset { get; set; }
        public Color StopColor { get; set; } = Color.Black;
        public float StopOpacity { get; set; } = 1;
    }

    public class SvgPolygonSpec : SvgVisualSpec, IMayHaveMarkers
    {
        public PixelFarm.Drawing.PointF[] Points { get; set; }
        //
        public SvgAttributeLink MarkerStart { get; set; }
        public SvgAttributeLink MarkerMid { get; set; }
        public SvgAttributeLink MarkerEnd { get; set; }
    }
    public class SvgPolylineSpec : SvgVisualSpec, IMayHaveMarkers
    {
        public PixelFarm.Drawing.PointF[] Points { get; set; }
        //
        public SvgAttributeLink MarkerStart { get; set; }
        public SvgAttributeLink MarkerMid { get; set; }
        public SvgAttributeLink MarkerEnd { get; set; }
    }


    public class SvgPathSpec : SvgVisualSpec, IMayHaveMarkers
    {
        public SvgPathSpec()
        {
        }

        public CssLength X { get; set; }
        public CssLength Y { get; set; }
        public CssLength Width { get; set; }
        public CssLength Height { get; set; }
        public string D { get; set; }

        //
        public SvgAttributeLink MarkerStart { get; set; }
        public SvgAttributeLink MarkerMid { get; set; }
        public SvgAttributeLink MarkerEnd { get; set; }
    }

    public class SvgTextSpec : SvgVisualSpec
    {
        public string FontFamily { get; set; }
        public string FontFace { get; set; }
        public CssLength FontSize { get; set; }

        public string TextContent { get; set; }
        public object ExternalTextNode { get; set; }
        public CssLength X { get; set; }
        public CssLength Y { get; set; }

        public float ActualX { get; set; }
        public float ActualY { get; set; }
    }

    public class SvgLineSpec : SvgVisualSpec, IMayHaveMarkers
    {
        public CssLength X1 { get; set; }
        public CssLength Y1 { get; set; }
        public CssLength X2 { get; set; }
        public CssLength Y2 { get; set; }

        //
        public SvgAttributeLink MarkerStart { get; set; }
        public SvgAttributeLink MarkerMid { get; set; }
        public SvgAttributeLink MarkerEnd { get; set; }
    }

    public class SvgMaskSpec : SvgVisualSpec
    {
        public CssLength X { get; set; }
        public CssLength Y { get; set; }
        public CssLength Width { get; set; }
        public CssLength Height { get; set; }
        public SvgContentUnit MaskUnits { get; set; }
    }

    public class SvgMarkerSpec : SvgVisualSpec
    {
        public CssLength RefX { get; set; }
        public CssLength RefY { get; set; }
        public CssLength MarkerWidth { get; set; }
        public CssLength MarkerHeight { get; set; }
    }
    public class SvgClipPathSpec : SvgVisualSpec
    {
        public SvgContentUnit ClipPathUnits { get; set; }
    }
}