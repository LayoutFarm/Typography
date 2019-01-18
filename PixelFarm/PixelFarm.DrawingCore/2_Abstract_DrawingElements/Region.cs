//MIT, 2014-present, WinterDev

namespace PixelFarm.Drawing
{
    public abstract class Region : System.IDisposable
    {
        public abstract void Dispose();
        public abstract object InnerRegion { get; }
        public abstract Rectangle GetRectBounds();
        public abstract bool IsSimpleRect { get; }

        public abstract Region CreateUnion(Region another); //OR
        public abstract Region CreateIntersect(Region another); //AND

        /// <summary>
        /// Region to contain the portion of the specified Region that does not intersect with this Region.
        /// </summary>
        /// <param name="another"></param>
        /// <returns></returns>
        public abstract Region CreateComplement(Region another); //invert selection 
        /// <summary>
        /// Region to contain only the portion of its interior that does not intersect with the specified Region.
        /// </summary>
        /// <param name="another"></param>
        /// <returns></returns>
        public abstract Region CreateExclude(Region another); //DIFF
        /// <summary>
        ///  Region object to the union minus the intersection of itself with the specified GraphicsPath object.
        /// </summary>
        /// <param name="another"></param>
        /// <returns></returns>
        public abstract Region CreateXor(Region another);
    }
}