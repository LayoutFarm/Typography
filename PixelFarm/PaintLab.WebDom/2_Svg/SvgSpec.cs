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
        Color fillColor = Color.Black;
        Color strokeColor = Color.Transparent;
        CssLength _strokeWidth;

        public bool HasFillColor { get; set; }
        public bool HasStrokeColor { get; set; }
        public bool HasStrokeWidth { get; set; }

        public SvgTransform Transform { get; set; }

        public Color FillColor
        {
            get { return this.fillColor; }
            set
            {
                this.fillColor = value;
                this.HasFillColor = true;
            }
        }
        public Color StrokeColor
        {
            get { return this.strokeColor; }
            set
            {
                this.strokeColor = value;
                this.HasStrokeColor = true;
            }
        }
        public CssLength StrokeWidth
        {
            get { return _strokeWidth; }
            set
            {
                _strokeWidth = value;
                this.HasStrokeWidth = true;
            }
        }
        public string Class { get; set; }

        public SvgAttributeLink ClipPathLink { get; set; }
        public object ResolvedClipPath { get; set; } //TODO: review here 
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
        public CssLength X
        {
            get;
            set;
        }
        public CssLength Y
        {
            get;
            set;
        }
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
        public CssLength X
        {
            get;
            set;
        }
        public CssLength Y
        {
            get;
            set;
        }
        public CssLength Width
        {
            get;
            set;
        }
        public CssLength Height
        {
            get;
            set;
        }

        public CssLength CornerRadiusX
        {
            get;
            set;
        }
        public CssLength CornerRadiusY
        {
            get;
            set;
        }
    }
    public class SvgCircleSpec : SvgVisualSpec
    {
        public CssLength X
        {
            get;
            set;
        }
        public CssLength Y
        {
            get;
            set;
        }
        public CssLength Radius
        {
            get;
            set;
        }
    }
    public class SvgImageSpec : SvgVisualSpec
    {
        public CssLength X
        {
            get;
            set;
        }
        public CssLength Y
        {
            get;
            set;
        }
        public CssLength Width
        {
            get;
            set;
        }
        public CssLength Height
        {
            get;
            set;
        }

        public string ImageSrc
        {
            get;
            set;
        }
    }

    public class SvgEllipseSpec : SvgVisualSpec
    {
        public CssLength X
        {
            get;
            set;
        }
        public CssLength Y
        {
            get;
            set;
        }
        public CssLength RadiusX
        {
            get;
            set;
        }
        public CssLength RadiusY
        {
            get;
            set;
        }
    }



    public interface IMayHaveMarkers
    {
        SvgAttributeLink MarkerStart { get; set; }
        SvgAttributeLink MarkerMid { get; set; }
        SvgAttributeLink MarkerEnd { get; set; }
    }

    public class SvgLinearGradientSpec : SvgVisualSpec
    {
        public System.Collections.Generic.List<StopColorPoint> StopList { get; set; }
        public CssLength X1 { get; set; }
        public CssLength Y1 { get; set; }
        public CssLength X2 { get; set; }
        public CssLength Y2 { get; set; }
    }
    public class SvgRadialGradientSpec : SvgVisualSpec
    {
        public System.Collections.Generic.List<StopColorPoint> StopList { get; set; }
        public CssLength CX { get; set; }
        public CssLength CY { get; set; }
        public CssLength R { get; set; }
        public CssLength FX { get; set; }
        public CssLength FY { get; set; }
    }
    public class SvgColorStopSpec : SvgElemSpec
    {
        public CssLength Offset { get; set; }
        public Color StopColor { get; set; }
        public float StopOpacity { get; set; }
    }
    public class StopColorPoint
    {
        public CssLength Offset { get; set; }
        public Color StopColor { get; set; }

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

        public CssLength X
        {
            get;
            set;
        }
        public CssLength Y
        {
            get;
            set;
        }
        public CssLength Width
        {
            get;
            set;
        }
        public CssLength Height
        {
            get;
            set;
        }

        public string D
        {
            get;
            set;
        }


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
        public CssLength X1
        {
            get;
            set;
        }
        public CssLength Y1
        {
            get;
            set;
        }
        public CssLength X2
        {
            get;
            set;
        }
        public CssLength Y2
        {
            get;
            set;
        }

        //
        public SvgAttributeLink MarkerStart { get; set; }
        public SvgAttributeLink MarkerMid { get; set; }
        public SvgAttributeLink MarkerEnd { get; set; }
    }


    public class SvgMarkerSpec : SvgVisualSpec
    {
        public CssLength RefX
        {
            get;
            set;
        }
        public CssLength RefY
        {
            get;
            set;
        }
        public CssLength MarkerWidth
        {
            get;
            set;
        }
        public CssLength MarkerHeight
        {
            get;
            set;
        }
    }
}