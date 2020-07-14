//MIT, 2020-present, WinterDev, Sam Hocevar

namespace Typography.Text
{
    public class MeasureStringArgs
    {
        //output
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public int CharFit { get; internal set; }
        public int CharFitWidth { get; internal set; }


        public int LimitWidth { get; set; }
        public void Reset()
        {
            Width = Height = 0;
            CharFit = CharFitWidth = 0;
            LimitWidth = -1;
        }

    }
}