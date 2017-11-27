//MIT, 2017, WinterDev

namespace Typography.TextServices
{
    /// <summary>
    /// provide text service 
    /// </summary>
    public class TextServiceHub
    {
        internal TypefaceStore _typefaceStore;
        internal OpenFontStore _openFontStore;
        TextShapingService _shapingService;

        public TextServiceHub()
        {
            _openFontStore = new OpenFontStore();
            _typefaceStore = new TypefaceStore();
            _shapingService = new TextShapingService(this);
        }
        public TextShapingService ShapingService
        {
            get { return _shapingService; }
        }
        
    }
}