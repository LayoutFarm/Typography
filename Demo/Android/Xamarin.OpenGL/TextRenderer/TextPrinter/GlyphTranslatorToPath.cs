using System;
using Typography.OpenFont;

namespace Typography.Rendering
{

    /// <summary>
    /// glyph-to-path translator
    /// </summary>
    internal class GlyphTranslatorToPath : IGlyphTranslator
    {
        #region Debug

        const bool WriteToText = true;
        const string MeshTextPath = "D:\\typography_mesh_text.txt";
        System.Text.StringBuilder builder = new System.Text.StringBuilder();

        #region renderer handlers

        public void _MoveTo(float x, float y)
        {
            if (WriteToText)
            {
                builder.AppendLine(string.Format("move to ({0:0.00}, {1:0.00})", x, y));
            }

            g.PathMoveTo(new Point(x, y));
        }

        public void AddLine(float x, float y)
        {
            if (WriteToText)
            {
                builder.AppendLine(string.Format("line to ({0:0.00}, {1:0.00})", x, y));
            }

            g.PathLineTo(new Point(x, y));
        }

        public void AddBezier(float c0x, float c0y, float c1x, float c1y, float p1x, float p1y)
        {
            if (WriteToText)
            {
                builder.AppendLine(string.Format("bezier c0 ({0:0.00}, {1:0.00}) c1 ({2:0.00}, {3:0.00}) end ({4:0.00}, {5:0.00})", c0x, c0y, c1x, c1y, p1x, p1y));
            }

            var p = new { X = (c0x + c1x) / 2, Y = (c0y + c1y) / 2 };
            g.PathLineTo(new Point(p.X, p.Y));
            g.PathLineTo(new Point(p1x, p1y));
        }

        public void FigureBegin(float x, float y)
        {
            if (WriteToText)
            {
                builder.AppendLine(string.Format("Figure begin at({0:0.00}, {1:0.00}).", x, y));
            }
            g.PathMoveTo(new Point(x, y));
        }

        public void FigureEnd()
        {
            if (WriteToText)
            {
                builder.AppendLine("Figure end.");
            }

            g.PathClose();
            g.AddContour(Color.Black);
            g.PathClear();
        }

        #endregion

        #endregion

        private ITextPathBuilder g;
        private float lastX;
        private float lastY;

        public ITextPathBuilder PathBuilder { set { this.g = value; } }

        public GlyphTranslatorToPath()
        {
        }

        public void BeginRead(int countourCount)
        {
            if (this.g == null)
            {
                throw new InvalidOperationException();
            }
        }

        public void EndRead()
        {
        }

        public void MoveTo(float x, float y)
        {
            lastX = (float)x;
            lastY = (float)y;
            _MoveTo(x, y);
        }

        public void CloseContour()
        {
            FigureEnd();
        }

        public void Curve3(float x1, float y1, float x2, float y2)
        {
            //from http://stackoverflow.com/questions/9485788/convert-quadratic-curve-to-cubic-curve
            float c1x = lastX + (float)((2f / 3f) * (x1 - lastX));//Control1X = StartX + (.66 * (ControlX - StartX))
            float c1y = lastY + (float)((2f / 3f) * (y1 - lastY));
            float c2x = (float)(x2 + ((2f / 3f) * (x1 - x2)));//Control2X = EndX + (.66 * (ControlX - EndX))
            float c2y = (float)(y2 + ((2f / 3f) * (y1 - y2)));

            AddBezier(c1x, c1y, c2x, c2y, x2, y2);

            lastX = x2;
            lastY = y2;
        }

        public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3)//not called
        {
            throw new NotSupportedException();
        }

        public void LineTo(float x, float y)
        {
            AddLine(x, y);

            lastX = x;
            lastY = y;
        }

        public void Reset()// dummy
        {
        }

        public void OutputDebugResult()
        {
            // show debug results
            if (WriteToText)
            {
                if (System.IO.File.Exists(MeshTextPath))
                {
                    System.IO.File.Delete(MeshTextPath);
                }
                System.IO.File.WriteAllText(MeshTextPath, builder.ToString());
            }
        }
    }
}

