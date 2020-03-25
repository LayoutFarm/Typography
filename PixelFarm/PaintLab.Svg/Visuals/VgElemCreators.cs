//MIT, 2019-present, WinterDev

using System;
using System.Collections.Generic;

using PixelFarm.Drawing;
using LayoutFarm.WebDom;
using LayoutFarm.WebDom.Parser;

using static PaintLab.Svg.CommonValueParsingUtils;

namespace PaintLab.Svg
{

    /// <summary>
    /// vg element creator
    /// </summary>
    abstract class VgElemCreator
    {
        internal CssParser _cssParser;

        public abstract string TagName { get; }
        public abstract WellknownSvgElementName WellKnownName { get; }

        /// <summary>
        /// create new and assign as current element
        /// </summary>
        public abstract void CreateNewAndAssignAsCurrent();
        /// <summary>
        /// pop current element
        /// </summary>
        /// <returns></returns>
        public abstract SvgElement Pop();
        public abstract SvgElement CurrentElem { get; }
        public abstract void AssignAttribute(string attrName, string attrValue);
        internal static SvgAttributeLink ParseAttributeLink(string value)
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

        protected static void ParseNumberList(string str, List<float> output)
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
        internal static readonly char[] strSeps1 = new char[] { ' ', ',' };
        protected static void ParsePointList(string str, List<PixelFarm.Drawing.PointF> output)
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
        protected static PixelFarm.Drawing.PointF[] ParsePointList(string str)
        {
            //TODO: review here again ...
            List<PixelFarm.Drawing.PointF> output = new List<PixelFarm.Drawing.PointF>();
            ParsePointList(str, output);
            return output.ToArray();
        }
        public virtual void OnTextNode(string content) { }
    }

    abstract class VgElemCreator<T> : VgElemCreator
        where T : SvgElemSpec
    {
        Stack<SvgElement> _buildingSvgElemsStack = new Stack<SvgElement>();
        Stack<T> _specStack = new Stack<T>();

        protected SvgElement _currentElem;
        protected T _spec;

        public override SvgElement CurrentElem => _currentElem;
        public sealed override void CreateNewAndAssignAsCurrent()
        {
            if (_currentElem != null)
            {
                _buildingSvgElemsStack.Push(_currentElem);
                _specStack.Push(_spec);
            }
            //create new, set as current  
            _currentElem = new SvgElement(this.WellKnownName, _spec = NewSpec());
        }

        public sealed override SvgElement Pop()
        {
            //pop current element from stack 
            SvgElement result = _currentElem;
            if (_specStack.Count > 0)
            {
                _currentElem = _buildingSvgElemsStack.Pop();
                _spec = _specStack.Pop();
            }
            else
            {
                _currentElem = null;
                _spec = null;
            }
            return result;
        }


        string _tagName;
        WellknownSvgElementName _wellknownName;
        System.Func<T> _newSpecFunc;
        public VgElemCreator(string tagName, WellknownSvgElementName wellKnownName, System.Func<T> newSpec)
        {
            _tagName = tagName;
            _wellknownName = wellKnownName;
            _newSpecFunc = newSpec;
        }
        public override string TagName => _tagName;
        public override WellknownSvgElementName WellKnownName => _wellknownName;
        protected virtual T NewSpec() => _newSpecFunc();


        protected bool AssignCommonAttribute(string attrName, string attrValue)
        {
            SvgVisualSpec visualSpec = _spec as SvgVisualSpec;

            switch (attrName)
            {
                default: return false;
                case "id":
                    _currentElem.ElemId = attrValue;
                    return true;

                case "class":
                    {

                        if (visualSpec != null)
                        {
                            visualSpec.Class = attrValue;
                        }
                    }
                    return true;

                case "style":
                    {

                        if (visualSpec != null)
                        {
                            AddStyle(visualSpec, attrValue);
                        }
                    }

                    return true;
                case "clip-path":
                    {
                        SvgAttributeLink clip_path = ParseAttributeLink(attrValue);
                        if (visualSpec != null)
                        {
                            visualSpec.ClipPathLink = clip_path;
                        }
                    }
                    return true;
                case "clip-rule":
                    {

                        if (visualSpec != null)
                        {
                            visualSpec.ClipRule = ParseClipRule(attrValue);
                        }
                    }
                    return true;
                case "fill-rule":
                    {

                        if (visualSpec != null)
                        {
                            visualSpec.FillRule = ParseFillRule(attrValue);
                        }
                    }
                    return true;
                case "fill":
                    {
                        if (attrValue != "none")
                        {
                            if (attrValue.StartsWith("url("))
                            {
                                //eg. url(#aaa)
                                SvgAttributeLink attrLink = ParseAttributeLink(attrValue);
                                if (attrLink != null)
                                {

                                    if (visualSpec != null)
                                    {
                                        visualSpec.FillPathLink = attrLink;
                                    }
                                }
                            }
                            else
                            {

                                if (visualSpec != null)
                                {
                                    visualSpec.FillColor = CssValueParser.ParseCssColor(attrValue);
                                }
                            }
                        }
                    }
                    return true;
                case "mask":
                    {
                        //eg. url(#aaa)
                        SvgAttributeLink attrLink = ParseAttributeLink(attrValue);
                        if (attrLink != null)
                        {
                            //resolve later
                            if (visualSpec != null)
                            {
                                visualSpec.MaskPathLink = attrLink;
                            }
                        }
                    }
                    return true;
                case "stroke-width":
                    {
                        if (visualSpec != null)
                        {
                            visualSpec.StrokeWidth = ParseGenericLength(attrValue);
                        }
                    }
                    return true;
                case "stroke":
                    {
                        if (attrValue != "none")
                        {
                            //
                            //TODO: set color to none if stroke= none
                            //
                            if (visualSpec != null)
                            {
                                visualSpec.StrokeColor = CssValueParser.ParseCssColor(attrValue);
                            }
                        }
                    }
                    return true;
                case "opacity":
                    {
                        if (visualSpec != null)
                        {
                            visualSpec.Opacity = ParseGenericLength(attrValue).Number;
                        }
                    }
                    return true;
                case "fill-opacity":
                    {
                        //adjust fill opacity
                        //0f-1f?

                    }
                    return true;
                case "stroke-opacity":
                    {

                    }
                    return true;
                case "stroke-linecap":
                    //set line-cap and line join again

                    return true;
                case "stroke-linejoin":

                    return true;
                case "stroke-miterlimit":

                    return true;
                case "transform":
                    {
                        //parse trans
                        if (visualSpec != null)
                        {
                            SvgParser.ParseTransform(attrValue, visualSpec);
                        }
                    }
                    return true;

            }
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
                                            spec.StrokeWidth = ParseGenericLength(value);
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

                                    spec.FillColor = ParseCssColor(value);
                                }
                            }
                            break;
                    }

                }
            }
        }

    }

    static class CommonValueParsingUtils
    {
        internal static Color ParseCssColor(string value)
        {
            return CssValueParser.ParseCssColor(value);
        }
        internal static LayoutFarm.Css.CssLength ParseGenericLength(string value)
        {
            return UserMapUtil.ParseGenericLength(value);
        }
        internal static SvgContentUnit ParseContentUnit(string value)
        {
            switch (value)
            {
                case "userSpaceOnUse": return SvgContentUnit.UserSpaceOnUse;
                case "objectBoundingBox": return SvgContentUnit.ObjectBoudingBox;
                default: return SvgContentUnit.Unknown;
            }
        }
        internal static SvgClipRule ParseClipRule(string value)
        {
            //https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/clip-rule
            //nonzero | evenodd | inherit
            switch (value)
            {
                case "nonzero": return SvgClipRule.NoneZero;
                case "evenodd": return SvgClipRule.EvenOdd;
                case "inherit": return SvgClipRule.Inherit;
                default: return SvgClipRule.Unknown;
            }
        }
        internal static SvgFillRule ParseFillRule(string value)
        {
            //https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/fill-rule
            //nonzero | evenodd | inherit
            switch (value)
            {
                case "nonzero": return SvgFillRule.NoneZero;//default
                case "evenodd": return SvgFillRule.EvenOdd;
                default: return SvgFillRule.Unknown;
            }
        }
    }

    class StyleElemCr : VgElemCreator<SvgStyleSpec>
    {
        public StyleElemCr() : base("style", WellknownSvgElementName.Style, () => new SvgStyleSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {

            if (!AssignCommonAttribute(attrName, attrValue))
            {
                throw new NotSupportedException();
            }
        }
    }


    class ClipPathElemCr : VgElemCreator<SvgClipPathSpec>
    {
        public ClipPathElemCr() : base("clipPath", WellknownSvgElementName.ClipPath, () => new SvgClipPathSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:

                    if (!AssignCommonAttribute(attrName, attrValue))
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case "clipPathUnits":
                    //_spec.ClipPathUnits = ParseCssColor(attrValue);
                    break;
            }
        }
    }
    class StopElemCr : VgElemCreator<SvgColorStopSpec>
    {
        public StopElemCr() : base("stop", WellknownSvgElementName.Stop, () => new SvgColorStopSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:

                    if (!AssignCommonAttribute(attrName, attrValue))
                    {
                        throw new NotSupportedException();
                    }
                    break;

                case "stop-color":
                    _spec.StopColor = ParseCssColor(attrValue);
                    break;
                case "offset":
                    //default unit of offset=%?
                    _spec.Offset = ParseGenericLength(attrValue);
                    break;
                case "stop-opacity":
                    {
                        if (float.TryParse(attrValue, out float result))
                        {
                            if (result < 0)
                            {
                                _spec.StopOpacity = 0;
                            }
                            else if (result > 1)
                            {
                                _spec.StopOpacity = 1;
                            }
                            else
                            {
                                _spec.StopOpacity = result;
                            }
                        }
                    }
                    break;
            }
        }

    }
    class MaskElemCr : VgElemCreator<SvgMaskSpec>
    {
        public MaskElemCr() : base("mask", WellknownSvgElementName.Mask, () => new SvgMaskSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {

            //https://developer.mozilla.org/en-US/docs/Web/SVG/Element/mask
            //height
            //    This attribute defines the height of the masking area.
            //    Value type: < length > ; Default value: 120 %; Animatable: yes
            //maskContentUnits
            //    This attribute defines the coordinate system for the contents of the<mask>.
            //    Value type: userSpaceOnUse | objectBoundingBox ; Default value: userSpaceOnUse; Animatable: yes
            //    maskUnits
            //    This attribute defines defines the coordinate system for attributes x, y, width and height on the<mask>.
            //    Value type: userSpaceOnUse | objectBoundingBox ; Default value: objectBoundingBox; Animatable: yes
            //x
            //    This attribute defines the x - axis coordinate of the top-left corner of the masking area.
            //      Value type: < coordinate > ; Default value: -10 %; Animatable: yes
            // y
            //    This attribute defines the y - axis coordinate of the top-left corner of the masking area.
            //      Value type: < coordinate > ; Default value: -10 %; Animatable: yes
            //width
            //    This attribute defines the width of the masking area.
            //    Value type: < length > ; Default value: 120 %; Animatable: yes
            switch (attrName)
            {
                default:
                    if (!AssignCommonAttribute(attrName, attrValue))
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case "x":
                    _spec.X = ParseGenericLength(attrValue);
                    break;
                case "y":
                    _spec.Y = ParseGenericLength(attrValue);
                    break;
                case "width":
                    _spec.Width = ParseGenericLength(attrValue);
                    break;
                case "height":
                    _spec.Height = ParseGenericLength(attrValue);
                    break;
                case "maskUnits":
                    _spec.MaskUnits = ParseContentUnit(attrValue);
                    break;
            }
        }
    }

    class MarkerElemCr : VgElemCreator<SvgMarkerSpec>
    {
        public MarkerElemCr() : base("marker", WellknownSvgElementName.Marker, () => new SvgMarkerSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:
                    if (!AssignCommonAttribute(attrName, attrValue))
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case "refX":
                    _spec.RefX = ParseGenericLength(attrValue);
                    break;
                case "refY":
                    _spec.RefY = ParseGenericLength(attrValue);
                    break;
                case "markerWidth":
                    _spec.MarkerWidth = ParseGenericLength(attrValue);
                    break;
                case "markerHeight":
                    _spec.MarkerHeight = ParseGenericLength(attrValue);
                    break;
            }
        }
    }

    class FilterElemCr : VgElemCreator<SvgFilterSpec>
    {

        public FilterElemCr() : base("filter", WellknownSvgElementName.Filter, () => new SvgFilterSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:
                    if (!AssignCommonAttribute(attrName, attrValue))
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case "filterUnits":
                    _spec.FilterUnit = ParseContentUnit(attrValue);
                    break;
                case "x":
                    _spec.X = ParseGenericLength(attrValue);
                    break;
                case "y":
                    _spec.Y = ParseGenericLength(attrValue);
                    break;
                case "width":
                    _spec.Width = ParseGenericLength(attrValue);
                    break;
                case "height":
                    _spec.Height = ParseGenericLength(attrValue);
                    break;
            }
        }
    }
    class UseElemCr : VgElemCreator<SvgUseSpec>
    {

        public UseElemCr() : base("use", WellknownSvgElementName.Use, () => new SvgUseSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:
                    if (!AssignCommonAttribute(attrName, attrValue))
                    {
                        throw new NotSupportedException();
                    }
                    break;

                case "x":
                    _spec.X = ParseGenericLength(attrValue);
                    break;
                case "y":
                    _spec.Y = ParseGenericLength(attrValue);
                    break;
                case "href":
                    _spec.Href = ParseAttributeLink(attrValue);
                    break;
            }
        }
    }



    class FeColorMatrixElemCr : VgElemCreator<SvgFeColorMatrixSpec>
    {
        public FeColorMatrixElemCr() : base("feColorMatrix", WellknownSvgElementName.FeColorMatrix, () => new SvgFeColorMatrixSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:
                    if (!AssignCommonAttribute(attrName, attrValue))
                    {
                        throw new NotSupportedException();
                    }
                    break;

                case "values":
                    {
                        List<float> numberList = new List<float>();
                        ParseNumberList(attrValue, numberList);
                        //last row
                        numberList.Add(0); numberList.Add(0); numberList.Add(0); numberList.Add(0); numberList.Add(1);
                        // 
                        _spec.matrix = numberList.ToArray();
                    }
                    break;
            }
        }
    }
    class GroupElemCr : VgElemCreator<SvgGroupSpec>
    {
        public GroupElemCr() : base("g", WellknownSvgElementName.Group, () => new SvgGroupSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:
                    if (!AssignCommonAttribute(attrName, attrValue))
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("unsuppported attrValue:" + attrValue);
#endif
                        return;
                        //throw new NotSupportedException();
                    }
                    break;

                case "filter":
                    {
                        //value may be in refer form
                        SvgAttributeLink attrLink = ParseAttributeLink(attrValue);
                        if (attrLink != null)
                        {
                            //resolve later
                            _spec.FilterPathLink = attrLink;
                        }
                    }
                    break;
                case "mask":
                    {
                        //TODO
                    }
                    break;
            }
        }
    }

    class LineElemCr : VgElemCreator<SvgLineSpec>
    {
        public LineElemCr() : base("line", WellknownSvgElementName.Line, () => new SvgLineSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:
                    if (!AssignCommonAttribute(attrName, attrValue))
                    {
                        throw new NotSupportedException();
                    }
                    break;

                case "x1":
                    _spec.X1 = ParseGenericLength(attrValue);
                    break;
                case "y1":
                    _spec.Y1 = ParseGenericLength(attrValue);
                    break;
                case "x2":
                    _spec.X2 = ParseGenericLength(attrValue);
                    break;
                case "y2":
                    _spec.Y2 = ParseGenericLength(attrValue);
                    break;
            }
        }
    }
    class RadialGradientElemCr : VgElemCreator<SvgRadialGradientSpec>
    {
        public RadialGradientElemCr() : base("radialGradient", WellknownSvgElementName.RadialGradient, () => new SvgRadialGradientSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:
                    if (!AssignCommonAttribute(attrName, attrValue))
                    {
                        throw new NotSupportedException();
                    }
                    break;

                case "cx":
                    _spec.CX = ParseGenericLength(attrValue);
                    break;
                case "cy":
                    _spec.CY = ParseGenericLength(attrValue);
                    break;
                case "fx":
                    _spec.FX = ParseGenericLength(attrValue);
                    break;
                case "fy":
                    _spec.FY = ParseGenericLength(attrValue);
                    break;
                case "fr":
                    _spec.FR = ParseGenericLength(attrValue);
                    break;
                case "r":
                    _spec.R = ParseGenericLength(attrValue);
                    break;
                case "gradientTransform":
                    SvgParser.ParseTransform(attrValue, _spec);
                    break;
                case "gradientUnits":
                    _spec.GradientUnits = ParseContentUnit(attrValue);
                    break;
            }
        }
    }

    class LinearGradientElemCr : VgElemCreator<SvgLinearGradientSpec>
    {
        public LinearGradientElemCr() : base("linearGradient", WellknownSvgElementName.LinearGradient, () => new SvgLinearGradientSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:
                    if (!AssignCommonAttribute(attrName, attrValue))
                    {
                        throw new NotSupportedException();
                    }
                    break;

                case "x1":
                    _spec.X1 = ParseGenericLength(attrValue);
                    break;
                case "y1":
                    _spec.Y1 = ParseGenericLength(attrValue);
                    break;
                case "x2":
                    _spec.X2 = ParseGenericLength(attrValue);
                    break;
                case "y2":
                    _spec.Y2 = ParseGenericLength(attrValue);
                    break;
                case "gradientTransform":
                    SvgParser.ParseTransform(attrValue, _spec);
                    break;
            }
        }
    }
    class RectElemCr : VgElemCreator<SvgRectSpec>
    {
        public RectElemCr() : base("rect", WellknownSvgElementName.Rect, () => new SvgRectSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:
                    if (!AssignCommonAttribute(attrName, attrValue))
                    {
                        throw new NotSupportedException();
                    }
                    break;

                case "x":
                    _spec.X = ParseGenericLength(attrValue);
                    break;
                case "y":
                    _spec.Y = ParseGenericLength(attrValue);
                    break;
                case "width":
                    _spec.Width = ParseGenericLength(attrValue);
                    break;
                case "height":
                    _spec.Height = ParseGenericLength(attrValue);
                    break;
                case "rx":
                    _spec.CornerRadiusX = ParseGenericLength(attrValue);
                    break;
                case "ry":
                    _spec.CornerRadiusY = ParseGenericLength(attrValue);
                    break;
            }
        }
    }

    class ImageElemCr : VgElemCreator<SvgImageSpec>
    {
        public ImageElemCr() : base("image", WellknownSvgElementName.Image, () => new SvgImageSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:
                    if (!AssignCommonAttribute(attrName, attrValue))
                    {
                        throw new NotSupportedException();
                    }
                    break;

                case "x":
                    _spec.X = ParseGenericLength(attrValue);
                    break;
                case "y":
                    _spec.Y = ParseGenericLength(attrValue);
                    break;
                case "width":
                    _spec.Width = ParseGenericLength(attrValue);
                    break;
                case "height":
                    _spec.Height = ParseGenericLength(attrValue);
                    break;
                case "href":
                    //image spec
                    _spec.ImageSrc = attrValue;//TODO: check if it is a valid value/path
                    break;
            }
        }
    }

    static class MarkerAssigner
    {
        public static bool AssignMarker(IMayHaveMarkers mayHasMarker, string attrName, string attrValue)
        {
            switch (attrName)
            {
                case "marker-start":
                    mayHasMarker.MarkerStart = VgElemCreator.ParseAttributeLink(attrValue);
                    return true;
                case "marker-mid":
                    mayHasMarker.MarkerMid = VgElemCreator.ParseAttributeLink(attrValue);
                    return true;
                case "marker-end":
                    mayHasMarker.MarkerEnd = VgElemCreator.ParseAttributeLink(attrValue);
                    return true;
            }
            return false;
        }
    }
    class PolygonElemCr : VgElemCreator<SvgPolygonSpec>
    {
        public PolygonElemCr() : base("polygon", WellknownSvgElementName.Polygon, () => new SvgPolygonSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:
                    if (!AssignCommonAttribute(attrName, attrValue) &&
                       !MarkerAssigner.AssignMarker(_spec, attrName, attrValue))
                    {
                        throw new NotSupportedException();
                    }
                    break;

                case "points":
                    //image spec
                    _spec.Points = ParsePointList(attrValue);
                    break;
            }
        }
    }
    class PathElemCr : VgElemCreator<SvgPathSpec>
    {
        public PathElemCr() : base("path", WellknownSvgElementName.Path, () => new SvgPathSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:
                    if (!AssignCommonAttribute(attrName, attrValue) &&
                       !MarkerAssigner.AssignMarker(_spec, attrName, attrValue))
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("NOT IMPL Attr:" + attrName + "=" + attrValue);
#endif
                        //throw new NotSupportedException();
                    }
                    break;

                case "d":
                    _spec.D = attrValue;
                    break;
            }
        }
    }
    class PolylineElemCr : VgElemCreator<SvgPolylineSpec>
    {
        public PolylineElemCr() : base("polyline", WellknownSvgElementName.Polyline, () => new SvgPolylineSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:
                    if (!AssignCommonAttribute(attrName, attrValue) &&
                       !MarkerAssigner.AssignMarker(_spec, attrName, attrValue))
                    {
                        throw new NotSupportedException();
                    }
                    break;

                case "points":
                    _spec.Points = ParsePointList(attrValue);
                    break;
            }
        }
    }
    class EllipseElemCr : VgElemCreator<SvgEllipseSpec>
    {
        public EllipseElemCr() : base("ellipse", WellknownSvgElementName.Ellipse, () => new SvgEllipseSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:
                    if (!AssignCommonAttribute(attrName, attrValue))
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case "cx":
                    //image spec
                    _spec.X = ParseGenericLength(attrValue);
                    break;
                case "cy":
                    _spec.Y = ParseGenericLength(attrValue);
                    break;
                case "rx":
                    _spec.RadiusX = ParseGenericLength(attrValue);
                    break;
                case "ry":
                    _spec.RadiusY = ParseGenericLength(attrValue);
                    break;
            }
        }
    }

    class CircleElemCr : VgElemCreator<SvgCircleSpec>
    {
        public CircleElemCr() : base("circle", WellknownSvgElementName.Circle, () => new SvgCircleSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:
                    if (!AssignCommonAttribute(attrName, attrValue))
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case "cx":
                    //image spec
                    _spec.X = ParseGenericLength(attrValue);
                    break;
                case "cy":
                    _spec.Y = ParseGenericLength(attrValue);
                    break;
                case "r":
                    _spec.Radius = ParseGenericLength(attrValue);
                    break;
            }
        }
    }
    class TextElemCr : VgElemCreator<SvgTextSpec>
    {
        public TextElemCr() : base("text", WellknownSvgElementName.Text, () => new SvgTextSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:
                    if (!AssignCommonAttribute(attrName, attrValue))
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case "x":
                    _spec.X = ParseGenericLength(attrValue);
                    break;
                case "y":
                    _spec.Y = ParseGenericLength(attrValue);
                    break;
                case "font":
                    //parse font
                    break;
                case "font-family":
                    _spec.FontFamily = attrValue;
                    break;
                case "font-size":
                    _spec.FontSize = ParseGenericLength(attrValue);
                    break;
            }
        }

        public override void OnTextNode(string content)
        {
            //TODO: review here for > 1 content
            _spec.TextContent = content;
        }
    }

    class SvgBoxElemCr : VgElemCreator<SvgBoxSpec>
    {
        public SvgBoxElemCr() : base("svg", WellknownSvgElementName.Svg, () => new SvgBoxSpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {
            switch (attrName)
            {
                default:
                    if (!AssignCommonAttribute(attrName, attrValue))
                    {
                        System.Diagnostics.Debug.WriteLine("please impl " + attrName);
                        //throw new NotSupportedException();
                    }
                    break;
                //case "cc"
                //case "dc":
                //case "version":
                //case "width":
                //case "height":
                //case "xmlns":
                //case "enable-background":
                //    System.Diagnostics.Debug.WriteLine("please impl " + attrName);
                //    break;
                case "viewBox":
                    {
                        string[] allPoints = attrValue.Split(strSeps1, StringSplitOptions.RemoveEmptyEntries);
                        if (allPoints.Length == 4)
                        {
                            //x,y,w,h 
                            float num;
                            _spec.ViewBoxX = float.TryParse(allPoints[0], out num) ? num : 0;
                            _spec.ViewBoxY = float.TryParse(allPoints[1], out num) ? num : 0;
                            _spec.ViewBoxW = float.TryParse(allPoints[2], out num) ? num : 0;
                            _spec.ViewBoxH = float.TryParse(allPoints[3], out num) ? num : 0;
                        }
                    }
                    break;
            }
        }
    }

    class EmptySpec : SvgElemSpec
    {

    }

    class DefsElemCr : VgElemCreator<EmptySpec>
    {
        public DefsElemCr() : base("defs", WellknownSvgElementName.Defs, () => new EmptySpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {

        }
    }
    class TitleElemCr : VgElemCreator<EmptySpec>
    {
        public TitleElemCr() : base("title", WellknownSvgElementName.Title, () => new EmptySpec()) { }
        public override void AssignAttribute(string attrName, string attrValue)
        {

        }
    }
}
