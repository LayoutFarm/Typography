//MIT, 2016-present, WinterDev 

using PixelFarm.Drawing;

namespace PixelFarm.CpuBlit.VertexProcessing
{
    public struct DashSegment
    {
        public readonly float Len;
        public readonly bool IsSolid;
        public DashSegment(float len, bool isSolid)
        {
            Len = len;
            IsSolid = isSolid;
        }
        public override string ToString() => (IsSolid ? "s" : "b") + Len;
    }



    class VxsLineSegmentWalkerOutput : ILineSegmentWalkerOutput
    {
        VertexStore _vxs;
        public VxsLineSegmentWalkerOutput()
        {
        }
        public void SetOutput(VertexStore vsx)
        {
            _vxs = vsx;
        }
        public void AddLineTo(LineWalkerMark maker, double x, double y) => _vxs.AddLineTo(x, y);
        public void AddMoveTo(LineWalkerMark maker, double x, double y) => _vxs.AddMoveTo(x, y);
    }

    public class LineDashGenerator : IDashGenerator
    {
        LineWalker _dashGenLineWalker = new LineWalker();
        DashSegment[] _staicDashSegments;
        VxsLineSegmentWalkerOutput _output = new VxsLineSegmentWalkerOutput();
        string _patternAsString;

        public LineDashGenerator()
        {

        }


        public void SetDashPattern(DashSegment[] segments)
        {
            IsStaticPattern = true;
            _staicDashSegments = segments;
            _patternAsString = null;

            _dashGenLineWalker.Reset();

            for (int i = 0; i < segments.Length; ++i)
            {
                DashSegment seg = segments[i];
                _dashGenLineWalker.AddMark(seg.Len, seg.IsSolid ? LineWalkDashStyle.Solid : LineWalkDashStyle.Blank);
            }
        }

        public void SetDashPattern(LineWalker lineWalker)
        {
            //this is a dynamic pattern
            IsStaticPattern = false;
            _staicDashSegments = null;
            _dashGenLineWalker = lineWalker;
        }

        public DashSegment[] GetStaticDashSegments() => _staicDashSegments;

        public bool IsStaticPattern { get; set; }

        public string GetPatternAsString()
        {
            if (IsStaticPattern)
            {
                //create 
                if (_patternAsString == null)
                {
                    //TODO: string builder pool
                    _patternAsString = "";
                    for (int i = 0; i < _staicDashSegments.Length; ++i)
                    {
                        _patternAsString += _staicDashSegments[i].ToString();
                    }
                }
                return _patternAsString;
            }
            return null;
        }

        public void GenerateDash(VertexStore srcVxs, ILineSegmentWalkerOutput output)
        {
            if (_dashGenLineWalker == null)
            {
                return;
            }
            _dashGenLineWalker.Walk(srcVxs, output);
        }
        public void CreateDash(VertexStore srcVxs, VertexStore output)
        {
            _output.SetOutput(output);
            GenerateDash(srcVxs, _output);
        }

    }

    public static class LineDashGeneratorExtension
    {
        public static void SetDashPattern(this LineDashGenerator generator, float solid, float blank)
        {
            generator.SetDashPattern(new DashSegment[] { new DashSegment(solid, true), new DashSegment(blank, false) });
        }
        public static void SetDashPattern(this LineDashGenerator generator, float solid0, float blank0, float solid1, float blank1)
        {
            generator.SetDashPattern(new DashSegment[] {
                new DashSegment(solid0, true),
                new DashSegment(blank0, false),
                new DashSegment(solid1, true),
                new DashSegment(blank1, false)
            });
        }
    }

}