//MIT, 2014-present, WinterDev

using System;
using System.Collections.Generic;

using LayoutFarm.WebDom.Parser;

namespace PaintLab.Svg
{
    /// <summary>
    /// svg dom builder
    /// </summary>
    public class VgDocBuilder : ISvgDocBuilder
    {
        Stack<SvgElement> _elems = new Stack<SvgElement>();
        Stack<VgElemCreator> _creatorStack = new Stack<VgElemCreator>();
        VgElemCreator _currentCreator;


        SvgElement _currentElem;
        VgDocument _svgDoc;

        CssParser _cssParser = new CssParser();
        Dictionary<string, VgElemCreator> _creators = new Dictionary<string, VgElemCreator>();
        public VgDocBuilder()
        {
            RegisterSvgElementCreators();
        }
        public VgDocument ResultDocument
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
                _svgDoc = new VgDocument();
            }
            _currentElem = _svgDoc.Root;
        }
        public void OnVisitNewElement(string elemName)
        {
            SvgElement newElem = CreateElement(elemName);
            if (_currentElem != null)
            {
                _elems.Push(_currentElem);
                _currentElem.AddElement(newElem);
            }
            _currentElem = newElem;
        }
        public void OnAttribute(string attrName, string value)
        {
            _currentCreator.AssignAttribute(attrName, value);
        }
        public void OnEnteringElementBody()
        {

        }
        public void OnTextNode(string text)
        {
            //a text node is a kind of child node
            //a text node may interleave with other kind of node
            _currentCreator.OnTextNode(text);
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

        SvgElement CreateElement(string elemName)
        {
            if (_creators.TryGetValue(elemName, out VgElemCreator creator))
            {
                if (_currentCreator != null)
                {
                    _creatorStack.Push(_currentCreator);
                }
                _currentCreator = creator;
                _currentCreator.CreateNewAndAssignAsCurrent();
                return _currentCreator.CurrentElem;
            }
            else
            {
                //not found creator for a specific element
                throw new NotSupportedException();
            }
        }



        void RegisterSvgElementCreators()
        {
            //you can use reflection technique here
            //or register it manually

            RegisterSvgElementCreator(
                new SvgBoxElemCr(),
                new DefsElemCr(),
                new TitleElemCr(),
                new FilterElemCr(),
                new FeColorMatrixElemCr(),
                new MaskElemCr(),
                //
                new StyleElemCr(),
                //
                new TextElemCr(),
                new ClipPathElemCr(),
                new GroupElemCr(),
                new RectElemCr(),
                new LineElemCr(),
                new PolylineElemCr(),
                new PolygonElemCr(),
                new CircleElemCr(),
                new EllipseElemCr(),
                new UseElemCr(),
                new PathElemCr(),
                new ImageElemCr(),
                new LinearGradientElemCr(),
                new RadialGradientElemCr(),
                new StopElemCr(),
                new MarkerElemCr()
                );

        }
        void RegisterSvgElementCreator(params VgElemCreator[] creators)
        {
            for (int i = 0; i < creators.Length; ++i)
            {
                VgElemCreator creator = creators[i];
                creator._cssParser = _cssParser; //use common css parser
                _creators.Add(creator.TagName, creator);
            }

        }
    }
}