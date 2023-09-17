using Typography.OpenFont;
using DrawingGL;
using DrawingGL.Text;
using Typography.OpenFont.Contours;
using Microsoft.Maui.Graphics;
using Typography.OpenFont.Extensions;

namespace MauiApp1
{
    public class MyFontPanel : IDrawable
    {
        Typeface _typeface; //may be use later,
        PathF _glyphPath; //a path of single glyph
        public MyFontPanel()
        {
            if (GetFontResource != null)
            {
                Stream stream = GetFontResource("DroidSans.ttf");
                OpenFontReader reader = new OpenFontReader();
                _typeface = reader.Read(stream);

                _glyphPath = new PathF();
                var path = new MyWritablePath(_glyphPath);
                var txToPath = new GlyphTranslatorToPath();
                txToPath.SetOutput(path);

                var builder = new GlyphOutlineBuilder(_typeface);
                char selectedChar = 'a';
                float pointSize = 300;//pt
                builder.BuildFromGlyphIndex(_typeface.GetGlyphIndex(selectedChar), pointSize, txToPath);

                //original glyph coordinate is upside down
                //for simplicity, we invert to path

                // Before applying the transformation, you might want to move (translate) the path to the desired position
                // This is necessary because flipping will also invert the position

                float lineHeight = _typeface.CalculateMaxLineClipHeight() * _typeface.CalculateScaleToPixelFromPointSize(pointSize);
                System.Numerics.Matrix3x2 transformMat = new System.Numerics.Matrix3x2(
                    1, 0,
                    0, -1,
                    0, lineHeight);
                _glyphPath.Transform(transformMat);
                // Now, apply the inversion (flip) transformation 
            }
        }
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (_glyphPath == null) { return; }

            canvas.SaveState();
            canvas.FillColor = Colors.Gold;
            canvas.FillPath(_glyphPath);
            canvas.ResetState();
        }

        public static Func<string, Stream> GetFontResource;


        readonly struct MyWritablePath : IWritablePath
        {
            readonly PathF _path;
            public MyWritablePath(PathF path)
            {
                _path = path;
            }
            public void BezireTo(float x1, float y1, float x2, float y2, float x3, float y3)
            {
                _path.CurveTo(x1, y1, x2, y2, x3, y3);
            }

            public void CloseFigure()
            {
                _path.Close();
            }

            public void LineTo(float x1, float y1)
            {
                _path.LineTo(x1, y1);
            }

            public void MoveTo(float x0, float y0)
            {
                _path.MoveTo(x0, y0);
            }
        }
    }
}