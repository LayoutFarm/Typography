//MIT, 2018-present, WinterDev 

namespace PixelFarm.Drawing.Internal
{
    public ref struct MicroPainter
    {
        public readonly DrawBoard _drawBoard;
        public MicroPainter(DrawBoard drawBoard)
        {
            _drawBoard = drawBoard;
        }
        public float ViewportWidth => _drawBoard.Width;
        public float ViewportHeight => _drawBoard.Height;

        public DrawboardBuffer CreateOffscreenDrawBoard(int width, int height) => _drawBoard.CreateBackbuffer(width, height);

        public void AttachTo(DrawboardBuffer attachToBackbuffer)
        {
            //save  
            _drawBoard.EnterNewDrawboardBuffer(attachToBackbuffer);
        }
        public void SetViewportSize(float width, float height)
        {
            //_viewportWidth = width;
            //_viewportHeight = height;
        }

        public void AttachToNormalBuffer()
        {
            _drawBoard.ExitCurrentDrawboardBuffer();
        }

        internal Rectangle CurrentClipRect => _drawBoard.CurrentClipRect;
        public void DrawImage(Image img, float x, float y)
        {
            _drawBoard.DrawImage(img, (int)x, (int)y);
        }
        public void DrawImage(Image img, float x, float y, float w, float h)
        {
            _drawBoard.DrawImage(img, new RectangleF(x, y, w, h));
        }
    }
}