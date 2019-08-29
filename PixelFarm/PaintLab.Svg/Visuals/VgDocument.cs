//MIT, 2014-present, WinterDev

using LayoutFarm.WebDom; 
namespace PaintLab.Svg
{

    public class VgDocument
    {
        SvgElement _rootElement = new SvgElement(WellknownSvgElementName.Svg, null as string);
        public VgDocument()
        {
        }
        public SvgElement Root => _rootElement;
        public CssActiveSheet CssActiveSheet { get; set; }
        //hint
        public string OriginalContent { get; set; }
        public string OriginalFilename { get; set; }
    }

}