//MIT, 2016-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.CpuBlit.FragmentProcessing;

namespace PixelFarm.CpuBlit
{
    class ReusableRotationTransformer : ICoordTransformer
    {

        double _angle;
        Affine _affine;
        public ReusableRotationTransformer()
        {
            _affine = Affine.IdentityMatrix;
        }
        public double Angle
        {
            get => _angle;

            set
            {
                if (value != _angle)
                {
                    _affine = Affine.NewRotation(value);
                }
                _angle = value;
            }
        }

        public CoordTransformerKind Kind => CoordTransformerKind.Affine3x2;

        public void Transform(ref double x, ref double y)
        {
            _affine.Transform(ref x, ref y);
        }

        ICoordTransformer ICoordTransformer.MultiplyWith(ICoordTransformer another)
        {
            return this._affine.MultiplyWith(another);
        }
        ICoordTransformer ICoordTransformer.CreateInvert()
        {
            return _affine.CreateInvert();
        }

    }


    struct GradientSpanPart
    {
        public GradientSpanGen _spanGenGr;
        public LinearGradientColorsProvider _linearGradientColorProvider;
        public SpanInterpolatorLinear _linerInterpolator;
        public ReusableRotationTransformer _reusableRotationTransformer;

        public void SetData(IGradientValueCalculator gvc, LinearGradientPair pair)
        {

            _linerInterpolator = new SpanInterpolatorLinear();
            _linearGradientColorProvider = new LinearGradientColorsProvider();
            _spanGenGr = new GradientSpanGen();
            //TODO:
            //user can use other coord transformer 
            _linerInterpolator.Transformer =
                _reusableRotationTransformer = new ReusableRotationTransformer();
            _reusableRotationTransformer.Angle = pair.Angle;
            _linearGradientColorProvider.SetColors(pair.c1, pair.c2, pair.steps);
            _spanGenGr.Reset(_linerInterpolator,
                gvc,
                _linearGradientColorProvider,
               pair.Distance);

            _spanGenGr.SetStartPoint(pair.x1, pair.y1);

        }

        public void SetOffset(float x, float y)
        {
            _spanGenGr.SetOffset(x, y);
        }
    }

    class AggLinearGradientBrush : ISpanGenerator
    {
        static IGradientValueCalculator _gvcX = new GvcX();
        static IGradientValueCalculator _gvcY = new GvcY();
        GradientSpanPart _grSpanGenPart;
        List<GradientSpanPart> _moreSpanGenertors;
        bool _isInit;
        public void Prepare()
        {

        }
        public void ResolveBrush(LinearGradientBrush linearGrBrush)
        {
            //for gradient :
            int pairCount = linearGrBrush.PairCount;

            //resolve linear gradient to agg object  
            if (!_isInit)
            {
                //temp fix  
                _isInit = true;
            }
            if (_moreSpanGenertors == null)
            {
                _moreSpanGenertors = new List<GradientSpanPart>();
            }
            else
            {
                _moreSpanGenertors.Clear();
            }
            //
            //more than 1 pair   
            int partNo = 0;
            int partCount = linearGrBrush.PairCount;

            foreach (LinearGradientPair pair in linearGrBrush.GetColorPairIter())
            {
                IGradientValueCalculator gvc = null;
                switch (pair.Direction)
                {
                    case LinearGradientPair.GradientDirection.Vertical:
                        gvc = _gvcY;
                        break;
                    case LinearGradientPair.GradientDirection.Horizontal:
                        gvc = _gvcX;
                        break;
                    default:
                        //temp, 
                        //TODO: review here
                        gvc = _gvcX;
                        break;
                }

                _grSpanGenPart = new GradientSpanPart();
                _grSpanGenPart.SetData(gvc, pair);
                _grSpanGenPart._spanGenGr.PartNo = partNo;
                _grSpanGenPart._spanGenGr.IsLastPart = (partNo == partCount - 1);
                _moreSpanGenertors.Add(_grSpanGenPart);
                partNo++;
            }

            _grSpanGenPart = _moreSpanGenertors[0];

#if !COSMOS
            for (int i = 0; i < partCount - 1; ++i)
            {
                GradientSpanPart part = _moreSpanGenertors[i];
                part._spanGenGr.RequestGradientPart += (fromPartNo) =>
                {
                    if (fromPartNo < partCount)
                    {
                        return _moreSpanGenertors[fromPartNo]._spanGenGr;
                    }
                    else
                    {
                        return null;
                    }
                };
            }
#endif
        }


        public void SetOffset(float x, float y)
        {
            //apply offset to all span generator
            int j = _moreSpanGenertors.Count;
            for (int i = 0; i < j; ++i)
            {
                _moreSpanGenertors[i].SetOffset(x, y);
            }
        }
        public void GenerateColors(Color[] outputColors, int startIndex, int x, int y, int spanLen)
        {

            //start at current span generator 
            _grSpanGenPart._spanGenGr.GenerateColors(outputColors, startIndex, x, y, spanLen);
        }

    }

    class AggCircularGradientBrush : ISpanGenerator
    {

        static IGradientValueCalculator _gvcCircular = new GvcRadial();
        GradientSpanPart _grSpanGenPart;
        List<GradientSpanPart> _moreSpanGenertors;
        bool _isInit;
        public void Prepare()
        {

        }
        public void ResolveBrush(CircularGradientBrush linearGrBrush)
        {
            //for gradient :
            int pairCount = linearGrBrush.PairCount;

            //resolve linear gradient to agg object  
            if (!_isInit)
            {
                //temp fix   
                _isInit = true;
            }
            if (_moreSpanGenertors == null)
            {
                _moreSpanGenertors = new List<GradientSpanPart>();
            }
            else
            {
                _moreSpanGenertors.Clear();
            }
            //
            //more than 1 pair   
            int partNo = 0;
            int partCount = linearGrBrush.PairCount;
            foreach (LinearGradientPair pair in linearGrBrush.GetColorPairIter())
            {
                _grSpanGenPart = new GradientSpanPart();
                _grSpanGenPart.SetData(_gvcCircular, pair);
                _grSpanGenPart._spanGenGr.PartNo = partNo;
                _grSpanGenPart._spanGenGr.IsLastPart = (partNo == partCount - 1);
                _moreSpanGenertors.Add(_grSpanGenPart);
                partNo++;
            }

            _grSpanGenPart = _moreSpanGenertors[0];

#if !COSMOS

            for (int i = 0; i < partCount - 1; ++i)
            {
                GradientSpanPart part = _moreSpanGenertors[i];
                part._spanGenGr.RequestGradientPart += (fromPartNo) =>
                {
                    if (fromPartNo != partCount - 1)
                    {
                        return _moreSpanGenertors[fromPartNo + 1]._spanGenGr;
                    }
                    else
                    {
                        return null;
                    }
                };
            }
#endif
        }
        public void SetOffset(float x, float y)
        {
            //apply offset to all span generator
            int j = _moreSpanGenertors.Count;
            for (int i = 0; i < j; ++i)
            {
                _moreSpanGenertors[i].SetOffset(x, y);
            }
        }
        public void GenerateColors(Color[] outputColors, int startIndex, int x, int y, int spanLen)
        {

            //start at current span generator 
            _grSpanGenPart._spanGenGr.GenerateColors(outputColors, startIndex, x, y, spanLen);
        }

    }

    class AggPolygonGradientBrush
    {

        float[] _xyCoords;
        Color[] _colors;

        internal ushort[] _vertIndices;
        internal float[] _outputCoords;
        internal int _vertexCount;

        List<VertexStore> _cacheVxsList = new List<VertexStore>();
        List<GouraudVerticeBuilder.CoordAndColor> _cacheColorAndVertexList = new List<GouraudVerticeBuilder.CoordAndColor>();

        public AggPolygonGradientBrush()
        {
            this.DilationValue = 0.175f;
            this.LinearGamma = 0.809f;
        }
        public float[] GetXYCoords() => _xyCoords;
        public Color[] GetColors() => _colors;
        public void BuildFrom(PolygonGraidentBrush polygonGrBrush)
        {
            List<PolygonGraidentBrush.ColorVertex2d> inputVertexList = polygonGrBrush.Vertices;

            int coordCount = inputVertexList.Count;
            _xyCoords = new float[coordCount * 2];
            _colors = new Color[coordCount];

            for (int i = 0; i < coordCount; ++i)
            {
                PolygonGraidentBrush.ColorVertex2d v = inputVertexList[i];
                _xyCoords[i << 1] = v.X;
                _xyCoords[(i << 1) + 1] = v.Y;
                _colors[i] = v.C;
            }
        }
        public float DilationValue { get; set; }
        public float LinearGamma { get; set; }

        public VertexStore CurrentVxs { get; set; }
        public int CachePartCount => _cacheVxsList.Count;


        public void BuildCacheVertices(GouraudVerticeBuilder grBuilder)
        {
            _cacheVxsList.Clear(); //clear prev data
            _cacheColorAndVertexList.Clear(); //clear prev data

            grBuilder.DilationValue = this.DilationValue;
            using (VxsTemp.Borrow(out var tmpVxs))
            {
                for (int i = 0; i < _vertexCount;)
                {
                    ushort v0 = _vertIndices[i];
                    ushort v1 = _vertIndices[i + 1];
                    ushort v2 = _vertIndices[i + 2];

                    grBuilder.SetColor(_colors[v0], _colors[v1], _colors[v2]);
                    grBuilder.SetTriangle(
                        _outputCoords[v0 << 1], _outputCoords[(v0 << 1) + 1],
                        _outputCoords[v1 << 1], _outputCoords[(v1 << 1) + 1],
                        _outputCoords[v2 << 1], _outputCoords[(v2 << 1) + 1]);

                    //get result from _gouraudSpanBuilder
                    grBuilder.MakeVxs(tmpVxs);

                    grBuilder.GetArrangedVertices(
                        out GouraudVerticeBuilder.CoordAndColor c0,
                        out GouraudVerticeBuilder.CoordAndColor c1,
                        out GouraudVerticeBuilder.CoordAndColor c2);

                    _cacheColorAndVertexList.Add(c0);
                    _cacheColorAndVertexList.Add(c1);
                    _cacheColorAndVertexList.Add(c2);

                    _cacheVxsList.Add(tmpVxs.CreateTrim());

                    i += 3;
                    tmpVxs.Clear(); //clear before reuse *** in next round
                }
            }
        }


        internal void SetSpanGenWithCurrentValues(int partNo, RGBAGouraudSpanGen spanGen)
        {
            CurrentVxs = _cacheVxsList[partNo];

            spanGen.SetColorAndCoords(
                _cacheColorAndVertexList[partNo * 3],
                _cacheColorAndVertexList[(partNo * 3) + 1],
                _cacheColorAndVertexList[(partNo * 3) + 2]);
        }
    }
}