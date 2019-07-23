//MIT, 2017-present, WinterDev

using PixelFarm.VectorMath;

namespace PixelFarm.Contours
{

    public class Joint
    {

        //A GlyphBoneJoint is on a midpoint of two 'inside' adjacent edges.
        //(2 contact edges)
        //of 2 triangles,      
        //(_p_contact_edge, _q_contact_edge)

        internal readonly EdgeLine _p_contact_edge;
        internal readonly EdgeLine _q_contact_edge;
        //one bone joint can have up to 2 tips  
        EdgeLine _tipEdge_p;
        EdgeLine _tipEdge_q;

        float _fitX, _fitY;

        internal Joint(
            InsideEdgeLine p_contact_edge,
            InsideEdgeLine q_contact_edge)
        {

            //both p and q is INSIDE, contact edge
            _p_contact_edge = p_contact_edge;
            _q_contact_edge = q_contact_edge;
            //this is original x,y
            Vector2f midpos = p_contact_edge.GetMidPoint();
            _fitX = midpos.X;
            _fitY = midpos.Y;

#if DEBUG
            if (p_contact_edge.inside_joint != null ||
                q_contact_edge.inside_joint != null)
            {
                throw new System.NotSupportedException();
            }
#endif
            p_contact_edge.inside_joint = this;
            q_contact_edge.inside_joint = this;
        }
        /// <summary>
        /// dynamic fit x
        /// </summary>
        public float FitX => _fitX;
        /// <summary>
        /// dynamic fit y
        /// </summary>
        public float FitY => _fitY;

        internal void SetFitXY(float newx, float newy)
        {
            _fitX = newx;
            _fitY = newy;
        }
        internal AnalyzedTriangle P_Tri => _p_contact_edge.OwnerTriangle;
        internal AnalyzedTriangle Q_Tri => _q_contact_edge.OwnerTriangle;

        /// <summary>
        /// get position of this bone joint (mid point of the edge)
        /// </summary>
        /// <returns></returns>
        public Vector2f OriginalJointPos => _p_contact_edge.GetMidPoint();//mid point of the contact edge line
        //
        public Vector2f DynamicFitPos => new Vector2f(_fitX, _fitY);
        //
        /// <summary>
        /// calculate distance^2 from contact point to specific point v
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public double CalculateSqrDistance(Vector2f v)
        {

            Vector2f contactPoint = this.OriginalJointPos;
            float xdiff = contactPoint.X - v.X;
            float ydiff = contactPoint.Y - v.Y;

            return (xdiff * xdiff) + (ydiff * ydiff);
        }
        internal void SetTipEdge_P(EdgeLine e)
        {
#if DEBUG
            if (_tipEdge_p != null)
            {
                throw new System.NotSupportedException();
            }
#endif
            e.IsTip = true;
            _tipEdge_p = e;
        }
        internal void SetTipEdge_Q(EdgeLine e)
        {
#if DEBUG
            if (_tipEdge_q != null)
            {
                throw new System.NotSupportedException();
            }
            if (_tipEdge_q != null && _tipEdge_q == _tipEdge_p)
            {
                throw new System.NotSupportedException();
            }
#endif
            e.IsTip = true;
            _tipEdge_q = e;
        }
        //
        public bool HasTipP => _tipEdge_p != null;
        public bool HasTipQ => _tipEdge_q != null;
        //
        public Vector2f TipPointP => _tipEdge_p.GetMidPoint();
        public EdgeLine TipEdgeP => _tipEdge_p;
        //
        public Vector2f TipPointQ => _tipEdge_q.GetMidPoint();
        public EdgeLine TipEdgeQ => _tipEdge_q;
        //
        internal bool ComposeOf(AnalyzedTriangle tri)
        {
            return this.P_Tri == tri || this.Q_Tri == tri;
        }
        /// <summary>
        /// adjust this bone joint to fit with given gridboxW and H
        /// </summary>
        /// <param name="gridW"></param>
        /// <param name="gridH"></param>
        internal void AdjustFitXY(int gridW, int gridH)
        {

            Vector2f jointPos = this.OriginalJointPos;
            //set fit (x,y) to joint, then we will evaluate bone slope again (next step)
            this.SetFitXY(
                MyMath.FitToHalfGrid(jointPos.X, gridW), //use half?
                MyMath.FitToHalfGrid(jointPos.Y, gridH));//use half?
        }
        public void GetCentroidBoneCenters(out float cx0, out float cy0, out float cx1, out float cy1)
        {   
            this.P_Tri.CalculateCentroid(out cx0, out cy0);
            this.Q_Tri.CalculateCentroid(out cx1, out cy1);
        }
#if DEBUG
        public readonly int dbugId = dbugTotalId++;
        public static int dbugTotalId;

        public override string ToString()
        {
            return "id:" + dbugId + " " + this.OriginalJointPos.ToString();
        }

        public EdgeLine dbugGetEdge_P() { return _p_contact_edge; }
        public EdgeLine dbugGetEdge_Q() { return _q_contact_edge; }
       
#endif

    }

}
