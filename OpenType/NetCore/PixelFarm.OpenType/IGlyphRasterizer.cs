//Apache2, 2017, WinterDev
//-----------------------------------------------------
//Apache2, 2014-2016, Samuel Carlsson, WinterDev
//some logics from FreeType Lib (FTL, BSD-3 clause)
//-----------------------------------------------------
 

namespace NOpenType
{
    //
    public interface IGlyphRasterizer
    {
        void BeginRead(int countourCount);
        void EndRead();

        void LineTo(double x, double y);
        void Curve3(double p2x, double p2y, double x, double y);
        void Curve4(double p2x, double p2y, double p3x, double p3y, double x, double y);
        void MoveTo(double x, double y);
        void CloseFigure();
    }
}

