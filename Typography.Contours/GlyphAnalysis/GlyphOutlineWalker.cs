//MIT, 2017, WinterDev
using System;
using System.Numerics;
using System.Collections.Generic;


namespace Typography.Contours
{

    public abstract class GlyphOutlineWalker
    {
        GlyphDynamicOutline _dynamicOutline;
        public GlyphOutlineWalker()
        {
            //default
            WalkTrianglesAndEdges = WalkCentroidBone = WalkGlyphBone = true;
        }
        public bool WalkTrianglesAndEdges { get; set; }
        public bool WalkCentroidBone { get; set; }
        public bool WalkGlyphBone { get; set; }
        //
        public void Walk(GlyphDynamicOutline dynamicOutline)
        {
#if DEBUG
            this._dynamicOutline = dynamicOutline;
            int triNumber = 0;
            if (WalkTrianglesAndEdges)
            {
                foreach (GlyphTriangle tri in _dynamicOutline.dbugGetGlyphTriangles())
                {
                    float centroidX, centriodY;
                    tri.CalculateCentroid(out centroidX, out centriodY);
                    OnTriangle(triNumber++, tri.e0, tri.e1, tri.e2, centroidX, centriodY);
                }
            }
            //--------------- 
            List<GlyphContour> contours = _dynamicOutline._contours;

            List<CentroidLineHub> centroidLineHubs = _dynamicOutline.dbugGetCentroidLineHubs();
            foreach (CentroidLineHub lineHub in centroidLineHubs)
            {
                Dictionary<GlyphTriangle, CentroidLine> lines = lineHub.GetAllCentroidLines();
                Vector2 hubCenter = lineHub.CalculateAvgHeadPosition();

                OnBegingLineHub(hubCenter.X, hubCenter.Y);
                foreach (CentroidLine line in lines.Values)
                {
                    List<GlyphBoneJoint> joints = line._joints;
                    int pairCount = joints.Count;

                    for (int i = 0; i < pairCount; ++i)
                    {
                        GlyphBoneJoint joint = joints[i];
                        if (WalkCentroidBone)
                        {
                            float px, py, qx, qy;
                            joint.dbugGetCentroidBoneCenters(out px, out py, out qx, out qy);
                            OnCentroidLine(px, py, qx, qy);
                            //--------------------------------------------------
                            if (joint.TipEdgeP != null)
                            {
                                Vector2 pos = joint.TipPointP;
                                OnCentroidLineTip_P(px, py, pos.X, pos.Y);
                            }
                            if (joint.TipEdgeQ != null)
                            {
                                Vector2 pos = joint.TipPointQ;
                                OnCentroidLineTip_Q(qx, qy, pos.X, pos.Y);
                            }
                        }
                        if (WalkGlyphBone)
                        {
                            OnBoneJoint(joint);
                        }
                    }
                    if (WalkGlyphBone)
                    {
                        //draw bone list
                        DrawBoneLinks(line);
                    }
                }
                //
                OnEndLineHub(hubCenter.X, hubCenter.Y, lineHub.GetHeadConnectedJoint());
            }
            //----------------

            List<GlyphContour> cnts = _dynamicOutline._contours;
            int j = cnts.Count;
            for (int i = 0; i < j; ++i)
            {
                GlyphContour cnt = cnts[i];
                List<GlyphPoint> points = cnt.flattenPoints;
                int n = points.Count;
                for (int m = 0; m < n; ++m)
                {
                    OnGlyphEdgeN(points[m].E0);
                }
            }
#endif

        }
        void DrawBoneLinks(CentroidLine line)
        {
#if DEBUG
            List<GlyphBone> glyphBones = line.bones;
            int glyphBoneCount = glyphBones.Count;
            int startAt = 0;
            int endAt = startAt + glyphBoneCount;
            OnBeginDrawingBoneLinks(line.GetHeadPosition(), startAt, endAt);
            int nn = 0;
            for (int i = startAt; i < endAt; ++i)
            {
                //draw line
                OnDrawBone(glyphBones[i], nn);
                nn++;
            }
            OnEndDrawingBoneLinks();
#endif
            ////draw link between each branch to center of hub
            //var brHead = branch.GetHeadPosition();
            //painter.Line(
            //    hubCenter.X * pxscale, hubCenter.Y * pxscale,
            //    brHead.X * pxscale, brHead.Y * pxscale);

            ////draw  a line link to centroid of target triangle

            //painter.Line(
            //    (float)brHead.X * pxscale, (float)brHead.Y * pxscale,
            //     hubCenter.X * pxscale, hubCenter.Y * pxscale,
            //     PixelFarm.Drawing.Color.Red);

        }


#if DEBUG
        protected abstract void OnTriangle(int triAngleId, EdgeLine e0, EdgeLine e1, EdgeLine e2, double centroidX, double centroidY);

        protected abstract void OnCentroidLine(double px, double py, double qx, double qy);
        protected abstract void OnCentroidLineTip_P(double px, double py, double tip_px, double tip_py);
        protected abstract void OnCentroidLineTip_Q(double qx, double qy, double tip_qx, double tip_qy);
        protected abstract void OnBoneJoint(GlyphBoneJoint joint);
        protected abstract void OnBeginDrawingBoneLinks(Vector2 branchHeadPos, int startAt, int endAt);
        protected abstract void OnEndDrawingBoneLinks();
        protected abstract void OnDrawBone(GlyphBone bone, int boneIndex);
        protected abstract void OnBegingLineHub(float centerX, float centerY);
        protected abstract void OnEndLineHub(float centerX, float centerY, GlyphBoneJoint joint); 
        protected abstract void OnGlyphEdgeN(EdgeLine edge);
        //
#endif
    }
}