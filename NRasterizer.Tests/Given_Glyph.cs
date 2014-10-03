using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace NRasterizer.Tests
{
    [TestClass]
    public class Given_Glyph
    {
        private List<Segment> _segments;
        private int _segmentIndex;
        private int _x;
        private int _y;

        private void EatContour(Glyph glyph, int contourIndex)
        {
            _segments = glyph.GetContourIterator(contourIndex, 0, 0, 0, 0, 1, 1).ToList();
            _segmentIndex = 0;
        }

        private void StartAt(int x, int y)
        {
            _x = x;
            _y = y;
        }

        private void AssertLineTo(int x, int y)
        {
            var segment = _segments[_segmentIndex];
            Assert.IsNotNull(segment);
            Assert.IsInstanceOfType(segment, typeof(Line));
            var line = (Line)segment;
            Assert.AreEqual(_x, line.x0);
            Assert.AreEqual(_y, line.y0);
            Assert.AreEqual(x, line.x1);
            Assert.AreEqual(y, line.y1);
            _x = x;
            _y = y;
            _segmentIndex++;
        }

        private void AssertContourDone()
        {
            Assert.AreEqual(_segmentIndex, _segments.Count - 1);
        }

        [TestMethod]
        public void With_Four_Line_Countour()
        {
            var x = new short[] { 0, 128, 128, 0 };
            var y = new short[] { 0, 0, 128, 128 };
            var on = new bool[] { true, true, true, true };

            EatContour(new Glyph(x, y, on, new ushort[] { 3 }, null), 0);

            StartAt(0, 0);
            AssertLineTo(128, 0);
            AssertLineTo(128, 128);
            AssertLineTo(0, 128);
            AssertLineTo(0, 0);
            AssertContourDone();
        }
    }
}
