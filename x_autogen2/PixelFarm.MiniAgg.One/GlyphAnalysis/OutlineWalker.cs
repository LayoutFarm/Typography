//MIT, 2017-present, WinterDev
using System;
using System.Collections.Generic;
using PixelFarm.VectorMath;

namespace PixelFarm.Contours
{

    public abstract class OutlineWalker
    {
        DynamicOutline _dynamicOutline;
        public OutlineWalker()
        {
            //default
            WalkTrianglesAndEdges = WalkCentroidBone = WalkGlyphBone = true;
        }
        public bool WalkTrianglesAndEdges { get; set; }
        public bool WalkCentroidBone { get; set; }
        public bool WalkGlyphBone { get; set; }
        //
        public void Walk(DynamicOutline dynamicOutline)
        {
#if DEBUG
            _dynamicOutline = dynamicOutline;
            int triNumber = 0;
            if (WalkTrianglesAndEdges)
            {
                foreach (Triangle tri in _dynamicOutline.dbugGetGlyphTriangles())
                {
                    float centroidX, centriodY;
                    tri.CalculateCentroid(out centroidX, out centriodY);
                    OnTriangle(triNumber++, tri.e0, tri.e1, tri.e2, centroidX, centriodY);
                }
            }
            //--------------- 
            List<Contour> contours = _dynamicOutline._contours;

            List<CentroidLineHub> centroidLineHubs = _dynamicOutline.dbugGetCentroidLineHubs();
            foreach (CentroidLineHub lineHub in centroidLineHubs)
            {
                Dictionary<Triangle, CentroidLine> lines = lineHub.GetAllCentroidLines();
                Vector2f hubCenter = lineHub.CalculateAvgHeadPosition();

                OnBegingLineHub(hubCenter.X, hubCenter.Y);
                foreach (CentroidLine line in lines.Values)
                {
                    List<Joint> joints = line._joints;
                    int pairCount = joints.Count;

                    for (int i = 0; i < pairCount; ++i)
                    {
                        Joint joint = joints[i];
                        if (WalkCentroidBone)
                        {
                            float px, py, qx, qy;
                            joint.dbugGetCentroidBoneCenters(out px, out py, out qx, out qy);
                            OnCentroidLine(px, py, qx, qy);
                            //--------------------------------------------------
                            if (joint.TipEdgeP != null)
                            {
                                Vector2f pos = joint.TipPointP;
                                OnCentroidLineTip_P(px, py, pos.X, pos.Y);
                            }
                            if (joint.TipEdgeQ != null)
                            {
                                Vector2f pos = joint.TipPointQ;
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

            List<Contour> cnts = _dynamicOutline._contours;
            int j = cnts.Count;
            for (int i = 0; i < j; ++i)
            {
                Contour cnt = cnts[i];
                List<Vertex> points = cnt.flattenPoints;
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
            List<Bone> glyphBones = line.bones;
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
        protected abstract void OnBoneJoint(Joint joint);
        protected abstract void OnBeginDrawingBoneLinks(Vector2f branchHeadPos, int startAt, int endAt);
        protected abstract void OnEndDrawingBoneLinks();
        protected abstract void OnDrawBone(Bone bone, int boneIndex);
        protected abstract void OnBegingLineHub(float centerX, float centerY);
        protected abstract void OnEndLineHub(float centerX, float centerY, Joint joint);
        protected abstract void OnGlyphEdgeN(EdgeLine edge);
        //
#endif
    }
}