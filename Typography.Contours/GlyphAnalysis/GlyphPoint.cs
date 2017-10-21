//MIT, 2016-2017, WinterDev

namespace Typography.Contours
{

    public enum PointKind : byte
    {
        LineStart,
        LineStop,
        //
        C3Start,
        C3Control1,
        C3End,
        //
        C4Start,
        C4Control1,
        C4Control2,
        C4End,

        CurveInbetween,
    }


    public class GlyphPoint
    {
        readonly float _ox; //original x
        readonly float _oy; //original y
        readonly PointKind kind;

        float newX;
        float newY;
        //----------------------------------------  
        //float _adjust_fit_x; //not use 
        float _adjust_fit_y; //px scale specific y fitting value
        //------------------------------------- 

        /// <summary>
        /// outside inward edge ?, TODO: review inward, outward concept again 
        /// </summary>
        OutsideEdgeLine _e0;
        /// <summary>
        /// outside outward edge ? TODO: review inward, outward concept again 
        /// </summary>
        OutsideEdgeLine _e1;

        public GlyphPoint(float x, float y, PointKind kind)
        {
            this._ox = this.newX = x;
            this._oy = this.newY = y;
            this.kind = kind;
        }
        public int SeqNo { get; internal set; }
        public PointKind PointKind
        {
            get
            {
                return this.kind;
            }
        }

        internal bool IsPartOfHorizontalEdge { get; set; }

        /// <summary>
        /// original X
        /// </summary>
        public float OX { get { return this._ox; } }
        /// <summary>
        /// original Y
        /// </summary>
        public float OY { get { return this._oy; } }
        /// <summary>
        /// modified X
        /// </summary>
        public float X { get { return this.newX; } }
        /// <summary>
        /// modified Y
        /// </summary>
        public float Y { get { return this.newY; } }


        //in this version, we don't use this _adjust_fit_x for each point
        //public float FitAdjustX
        //{
        //  get { return _adjust_fit_x; }
        //  internal set
        //  {
        //      _adjust_fit_x = value;
        //#if DEBUG
        //      _dbug_has_adjust_x = true;
        //      _dbug_fit_analyzed = true;
        //#endif
        //  }
        //}
        public float FitAdjustY
        {
            get
            {
                return _adjust_fit_y;
            }
            internal set
            {
                _adjust_fit_y = value;
#if DEBUG
                _dbug_has_adjust_y = true;
                _dbug_fit_analyzed = true;
#endif 
            }
        }

        internal void ResetFitAdjustValues()
        {
            _adjust_fit_y = 0;
            //_adjust_fit_x = 0
        }

        internal void GetFitXY(float pxscale, out float x, out float y)
        {
            x = this.newX * pxscale;
            y = (this.newY * pxscale) + _adjust_fit_y;
        }

        internal void SetNewXY(float x, float y)
        {
            this.newX = x;
            this.newY = y;
        }
        //-----------

        /// <summary>
        /// outside inward edge
        /// </summary>
        internal OutsideEdgeLine E0
        {
            //TODO: review inward and outward edge again
            get { return this._e0; }

        }
        /// <summary>
        /// outside outward edge
        /// </summary>
        internal OutsideEdgeLine E1
        {
            //TODO: review inward and outward edge again
            get { return this._e1; }
        }

        /// <summary>         
        /// set outside edge that link with this glyph point
        /// </summary>
        /// <param name="edge">edge must be outside edge</param>
        internal void SetOutsideEdgeUnconfirmEdgeDirection(OutsideEdgeLine edge)
        {
            //at this stage, we don't known the edge is outward or inward.
            //so just set it
            //------------------------------------------
            //e0 and e1 will be swaped later for this point SetCorrectInwardAndOutWardEdge() ***

            if (_e0 == null)
            {
                _e0 = edge;
            }
            else if (_e1 == null)
            {
                _e1 = edge;
            }
            else
            {
                throw new System.NotSupportedException();
            }
            //----
#if DEBUG
            if (edge == null)
            {

            }
            if (_e0 == _e1)
            {
                throw new System.NotSupportedException();
            }
#endif
        }


        internal static bool SameCoordAs(GlyphPoint a, GlyphPoint b)
        {
            return a._ox == b._ox && a._oy == b._oy;
        }

#if DEBUG
        bool _dbug_fit_analyzed;
        bool _dbug_has_adjust_x;
        bool _dbug_has_adjust_y;
        public readonly int dbugId = dbugTotalId++;
        static int dbugTotalId;
        internal GlyphPart dbugOwnerPart;  //link back to owner part
        public Poly2Tri.TriangulationPoint dbugTriangulationPoint;

        public override string ToString()
        {
            //TODO: review adjust value again
            return this.dbugId + " :" +
                    (_ox + "," + _oy + " " + PointKind.ToString());
        }
#endif
    }

}

