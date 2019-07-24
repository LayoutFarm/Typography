//MIT, 2017-present, WinterDev
using System;
using System.Collections.Generic;
using PixelFarm.VectorMath;

namespace PixelFarm.Contours
{

    public abstract class OutlineWalker
    {

        public OutlineWalker()
        {
            //default
            Stop = true;
        }

        protected bool Stop { get; set; }

        public void WalkTriangles(IntermediateOutline intermediateOutline)
        {
            Stop = false;
            foreach (AnalyzedTriangle tri in intermediateOutline.GetTriangles())
            {
                if (Stop)
                {
                    //user can cancel this walking session
                    break;
                }
                OnTriangle(tri);
            }
            Stop = true;
        }

        public void WalkCentroidLine(IntermediateOutline intermediateOutline)
        {
            Stop = false;

            List<CentroidLineHub> centroidLineHubs = intermediateOutline.GetCentroidLineHubs();
            foreach (CentroidLineHub lineHub in centroidLineHubs)
            {
                Dictionary<AnalyzedTriangle, CentroidLine> lines = lineHub.GetAllCentroidLines();
                Vector2f hubCenter = lineHub.CalculateAvgHeadPosition();
                //on each line hub
                OnStartLineHub(hubCenter.X, hubCenter.Y);
                foreach (CentroidLine line in lines.Values)
                {
                    List<Joint> joints = line._joints;
                    int pairCount = joints.Count;

                    for (int i = 0; i < pairCount; ++i)
                    {
                        OnJoint(joints[i]);
                    }

                    List<Bone> bones = line.bones;
                    int startAt = 0;
                    int endAt = startAt + bones.Count;
                    OnBeginBoneLinks(line.GetHeadPosition(), startAt, endAt);
                    int nn = 0;
                    for (int i = startAt; i < endAt; ++i)
                    {
                        //draw line
                        OnBone(bones[i], nn);
                        nn++;
                    }
                    OnEndBoneLinks();
                }
                //
                OnEndLineHub(hubCenter.X, hubCenter.Y, lineHub.GetHeadConnectedJoint());
                if (Stop)
                {
                    break;
                }
            }

            Stop = true;
        }

        public void WalkContour(IntermediateOutline intermediateOutline)
        {
            Stop = false;
            List<Contour> cnts = intermediateOutline.GetContours();
            int j = cnts.Count;
            for (int i = 0; i < j; ++i)
            {
                Contour cnt = cnts[i];
                List<Vertex> points = cnt.flattenPoints;
                int n = points.Count;
                for (int m = 0; m < n; ++m)
                {
                    if (Stop)
                    {
                        //user can cancel this walking session
                        break;
                    }
                    OnEdgeN(points[m].E0);

                }
            }
            Stop = true;
        }



        protected abstract void OnTriangle(AnalyzedTriangle tri);
        protected abstract void OnJoint(Joint joint);
        protected abstract void OnBeginBoneLinks(Vector2f branchHeadPos, int startAt, int endAt);
        protected abstract void OnEndBoneLinks();
        protected abstract void OnBone(Bone bone, int boneIndex);
        protected abstract void OnStartLineHub(float centerX, float centerY);
        protected abstract void OnEndLineHub(float centerX, float centerY, Joint joint);
        protected abstract void OnEdgeN(EdgeLine edge);
    }
}