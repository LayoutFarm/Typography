//MIT, 2016-present, WinterDev 
using System.Collections.Generic;
using PixelFarm.Drawing;
 
namespace PixelFarm.CpuBlit.VertexProcessing
{
    public class LineDashGenerator
    {

        LineWalker _dashGenLineWalker;
        public LineDashGenerator()
        {

        }
        public void SetDashPattern(float solid, float blank)
        {
            _dashGenLineWalker = new LineWalker();
            _dashGenLineWalker.AddMark(solid, LineWalkDashStyle.Solid);
            _dashGenLineWalker.AddMark(blank, LineWalkDashStyle.Blank);
        }
        public void SetDashPattern(float solid0, float blank0, float solid1, float blank1)
        {
            _dashGenLineWalker = new LineWalker(); 
            _dashGenLineWalker.AddMark(solid0, LineWalkDashStyle.Solid);
            _dashGenLineWalker.AddMark(blank0, LineWalkDashStyle.Blank);
            //
            _dashGenLineWalker.AddMark(solid1, LineWalkDashStyle.Solid);
            _dashGenLineWalker.AddMark(blank1, LineWalkDashStyle.Blank);
        }
        public void SetDashPattern(LineWalker lineWalker)
        {
            this._dashGenLineWalker = lineWalker;
        }

        public void CreateDash(VertexStore srcVxs, VertexStore output)
        {
            if (_dashGenLineWalker == null)
            {
                return;
            }
            //-------------------------------
            _dashGenLineWalker.Walk(srcVxs, output);
        }
    }
}