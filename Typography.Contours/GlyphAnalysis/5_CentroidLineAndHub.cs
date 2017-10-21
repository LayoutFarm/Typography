//MIT, 2017, WinterDev
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Typography.Contours
{
    /// <summary>
    /// a collection of connected centroid pairs and bone
    /// </summary>
    class CentroidLine
    {


        //temp store centroid pair, we can clear it after AnalyzeEdgesAndCreateBoneJoints()
        List<GlyphCentroidPair> _centroid_pairs = new List<GlyphCentroidPair>();
        //
        //joint list is created from each centroid pair
        public List<GlyphBoneJoint> _joints = new List<GlyphBoneJoint>();
        public List<GlyphBone> bones = new List<GlyphBone>();

        internal CentroidLine()
        {
        }
        /// <summary>
        /// add a centroid pair
        /// </summary>
        /// <param name="pair"></param>
        public void AddCentroidPair(GlyphCentroidPair pair)
        {
            _centroid_pairs.Add(pair);
        }

        /// <summary>
        /// analyze edges of this line
        /// </summary>
        public void AnalyzeEdgesAndCreateBoneJoints()
        {
            List<GlyphCentroidPair> pairs = this._centroid_pairs;
            int j = pairs.Count;
            for (int i = 0; i < j; ++i)
            {
                //create bone joint (and tip edge) in each pair                
                _joints.Add(pairs[i].AnalyzeEdgesAndCreateBoneJoint());
            }

            //we dont' use it anymore
            _centroid_pairs.Clear();
            _centroid_pairs = null;
        }
        /// <summary>
        /// apply grid box to all bones in this line
        /// </summary>
        /// <param name="gridW"></param>
        /// <param name="gridH"></param>
        public void ApplyGridBox(List<BoneGroup> boneGroups, int gridW, int gridH)
        {
            //1. apply grid box to each joint
            for (int i = _joints.Count - 1; i >= 0; --i)
            {
                _joints[i].AdjustFitXY(gridW, gridH);
            }
            //2. (re) calculate slope for all bones.
            for (int i = bones.Count - 1; i >= 0; --i)
            {
                bones[i].EvaluateSlope();
            }
            //3. re-grouping 
            int j = bones.Count;
            BoneGroup boneGroup = new BoneGroup(this); //new group
            boneGroup.slopeKind = LineSlopeKind.Other;
            //
            float approxLen = 0;
            float ypos_sum = 0;
            float xpos_sum = 0;

            for (int i = 0; i < j; ++i)
            {
                GlyphBone bone = bones[i];
                LineSlopeKind slope = bone.SlopeKind;
                Vector2 mid_pos = bone.GetMidPoint();

                if (slope != boneGroup.slopeKind)
                {
                    //add existing to list and create a new group
                    if (boneGroup.count > 0)
                    {
                        //add existing bone group to bone-group list
                        boneGroup.approxLength = approxLen;
                        boneGroup.avg_x = xpos_sum / boneGroup.count;
                        boneGroup.avg_y = ypos_sum / boneGroup.count;
                        //
                        boneGroups.Add(boneGroup);
                    }
                    // 
                    boneGroup = new BoneGroup(this);
                    boneGroup.startIndex = i;
                    boneGroup.slopeKind = slope;
                    //
                    boneGroup.count++;
                    approxLen = bone.EvaluateFitLength(); //reset
                    //
                    xpos_sum = mid_pos.X;
                    ypos_sum = mid_pos.Y;

                }
                else
                {
                    boneGroup.count++;
                    approxLen += bone.EvaluateFitLength(); //append
                    //
                    xpos_sum += mid_pos.X;
                    ypos_sum += mid_pos.Y;
                }
            }
            //
            if (boneGroup.count > 0)
            {
                boneGroup.approxLength = approxLen;
                boneGroup.avg_x = xpos_sum / boneGroup.count;
                boneGroup.avg_y = ypos_sum / boneGroup.count;
                boneGroups.Add(boneGroup);
            }
        }

        /// <summary>
        /// find nearest joint that contains tri 
        /// </summary>
        /// <param name="tri"></param>
        /// <returns></returns>
        public GlyphBoneJoint FindNearestJoint(GlyphTriangle tri)
        {

            for (int i = _joints.Count - 1; i >= 0; --i)
            {
                GlyphBoneJoint joint = _joints[i];
                //each pair has 1 bone joint 
                //once we have 1 candidate
                if (joint.ComposeOf(tri))
                {
                    //found another joint
                    return joint;
                }
            }
            return null;
        }
#if DEBUG
        internal GlyphTriangle dbugStartTri;
#endif
    }

    struct BoneGroupingHelper
    {
        //this is a helper***

        List<BoneGroup> _selectedHorizontalBoneGroups;
        List<BoneGroup> _selectedVerticalBoneGroups;
        List<BoneGroup> _tmpBoneGroups;
        List<EdgeLine> tmpEdges;
        int gridBoxW, gridBoxH;

        public static BoneGroupingHelper CreateBoneGroupingHelper()
        {
            BoneGroupingHelper helper = new BoneGroupingHelper();
            helper._selectedHorizontalBoneGroups = new List<BoneGroup>();
            helper._selectedVerticalBoneGroups = new List<BoneGroup>();
            helper._tmpBoneGroups = new List<BoneGroup>();
            helper.tmpEdges = new List<EdgeLine>();
            return helper;
        }
        public void Reset(int gridBoxW, int gridBoxH)
        {
            this.gridBoxW = gridBoxW;
            this.gridBoxH = gridBoxH;
            _selectedHorizontalBoneGroups.Clear();
            _selectedVerticalBoneGroups.Clear();
        }
        public List<BoneGroup> SelectedHorizontalBoneGroups { get { return _selectedHorizontalBoneGroups; } }
        public List<BoneGroup> SelectedVerticalBoneGroups { get { return _selectedVerticalBoneGroups; } }
        public void CollectBoneGroups(CentroidLine line)
        {
            //
            _tmpBoneGroups.Clear();
            line.ApplyGridBox(_tmpBoneGroups, gridBoxW, gridBoxH);
            // 
            for (int i = _tmpBoneGroups.Count - 1; i >= 0; --i)
            {
                //this version, we focus on horizontal bone group
                BoneGroup boneGroup = _tmpBoneGroups[i];
                switch (boneGroup.slopeKind)
                {
                    case LineSlopeKind.Horizontal:
                        _selectedHorizontalBoneGroups.Add(boneGroup);
                        break;
                    case LineSlopeKind.Vertical:
                        _selectedVerticalBoneGroups.Add(boneGroup);
                        break; 
                }
            }
            _tmpBoneGroups.Clear();
        }
        public void AnalyzeHorizontalBoneGroups()
        {
            if (_selectedHorizontalBoneGroups.Count == 0) return;
            //
            //for Horizontal group analysis, we don't include short bone 
            Mark_ShortBones(_selectedHorizontalBoneGroups); //SHORT BONES
            //arrange by y-pos for horizontal group
            _selectedHorizontalBoneGroups.Sort((bg0, bg1) => bg0.avg_y.CompareTo(bg1.avg_y));
            //
            //collect outside edge of horizontal group
            for (int i = _selectedHorizontalBoneGroups.Count - 1; i >= 0; --i)
            {
                _selectedHorizontalBoneGroups[i].CollectOutsideEdges(tmpEdges);
            }
        }

        public void AnalyzeVerticalBoneGroups()
        {
            if (_selectedVerticalBoneGroups.Count == 0) return;
            //
            //for vertical group analysis, we use only long bones
            Mark_LongBones(_selectedVerticalBoneGroups); //LONG BONES
            //arrange by x-pos for vertical
            _selectedVerticalBoneGroups.Sort((bg0, bg1) => bg0.avg_x.CompareTo(bg1.avg_x));
            //
            //collect outside edge of vertical group
            for (int i = _selectedVerticalBoneGroups.Count - 1; i >= 0; --i)
            {
                _selectedVerticalBoneGroups[i].CollectOutsideEdges(tmpEdges);
            }
        }

        static void Mark_LongBones(List<BoneGroup> boneGroups)
        {

            int boneGroupsCount = boneGroups.Count;
            if (boneGroupsCount == 0)
            {
                return;
            }
            else if (boneGroupsCount == 1)
            {
                //eg 1 group
                //makr this long bone
                boneGroups[0]._lengKind = BoneGroupSumLengthKind.Long;
                return;
            }
            else
            {
                //----------------------
                //use median ?,
                boneGroups.Sort((bg0, bg1) => bg0.approxLength.CompareTo(bg1.approxLength));
                int groupCount = boneGroups.Count;
                //median
                int mid_index = groupCount / 2;
                BoneGroup bonegroup = boneGroups[mid_index];
                float upper_limit = bonegroup.approxLength * 2;
                bool foundSomeLongBone = false;
                for (int i = groupCount - 1; i >= mid_index; --i)
                {
                    //from end to mid_index => since the list is sorted
                    bonegroup = boneGroups[i];
                    if (bonegroup.approxLength > upper_limit)
                    {
                        foundSomeLongBone = true;
                        bonegroup._lengKind = BoneGroupSumLengthKind.Long;
                    }
                    else
                    {
                        //since to list is sorted                    
                        break;
                    }
                }
                //----------------------
                if (!foundSomeLongBone)
                {
                    for (int i = groupCount - 1; i >= mid_index; --i)
                    {
                        boneGroups[i]._lengKind = BoneGroupSumLengthKind.Long;
                    }
                }
            }
        }
        static void Mark_ShortBones(List<BoneGroup> boneGroups)
        {

            int boneGroupsCount = boneGroups.Count;
            if (boneGroupsCount < 2) { return; }
            //----------------------
            //use median ?,
            boneGroups.Sort((bg0, bg1) => bg0.approxLength.CompareTo(bg1.approxLength));
            int groupCount = boneGroups.Count;
            //median
            int mid_index = groupCount / 2;
            BoneGroup bonegroup = boneGroups[mid_index];
            float lower_limit = bonegroup.approxLength / 2;
            for (int i = 0; i < mid_index; ++i)
            {
                //from start to mid_index => since the list is sorted

                bonegroup = boneGroups[i];
                if (bonegroup.approxLength < lower_limit)
                {
                    bonegroup._lengKind = BoneGroupSumLengthKind.Short;
                }
                else
                {
                    //since to list is sorted                    
                    break;
                }
            }
        }
    }

    enum BoneGroupSumLengthKind : byte
    {
        General,
        Short,
        Long
    }

    class BoneGroup
    {
        public LineSlopeKind slopeKind;
        /// <summary>
        /// start index from owner centroid line
        /// </summary>
        public int startIndex;
        /// <summary>
        /// member count in this group
        /// </summary>
        public int count;
        /// <summary>
        /// approximation of summation of bone length in this group
        /// </summary>
        public float approxLength;

        /// <summary>
        /// average x pos of this group
        /// </summary>
        public float avg_x;
        /// <summary>
        /// average y pos of this group
        /// </summary>
        public float avg_y;

        public EdgeLine[] edges;

        public BoneGroupSumLengthKind _lengKind;

        internal readonly CentroidLine ownerCentroidLine;
        public BoneGroup(CentroidLine ownerCentroidLine)
        {
            this.ownerCentroidLine = ownerCentroidLine;
        } 
        internal void CollectOutsideEdges(List<EdgeLine> tmpEdges)
        {
            tmpEdges.Clear();
            int index = this.startIndex;
            for (int n = this.count - 1; n >= 0; --n)
            {
                GlyphBone bone = ownerCentroidLine.bones[index];
                //collect all outside edge arround  glyph bone
                bone.CollectOutsideEdge(tmpEdges);
                index++;
            }
            //
            if (tmpEdges.Count == 0) return;
            this.edges = tmpEdges.ToArray();
        }

        public void CalculateBounds(ref float minX, ref float minY, ref float maxX, ref float maxY)
        {

            for (int e = edges.Length - 1; e >= 0; --e)
            {
                EdgeLine edge = edges[e];

                // x
                MyMath.FindMinMax(ref minX, ref maxX, (float)edge.PX);
                MyMath.FindMinMax(ref minX, ref maxX, (float)edge.QX);
                // y
                MyMath.FindMinMax(ref minY, ref maxY, (float)edge.PY);
                MyMath.FindMinMax(ref minY, ref maxY, (float)edge.QY);
            }

        }

#if DEBUG
        public override string ToString()
        {
            return slopeKind + ",x:" + avg_x + ",y:" + avg_y + ",s:" + startIndex + ":" + count + " len:" + approxLength;
        }
#endif
    }

    /// <summary>
    /// a collection of centroid line and bone joint
    /// </summary>
    class CentroidLineHub
    {
        //-----------------------------------------------
        //a centroid line hub start at the same mainTri.
        //and can have more than 1 centroid line.
        //----------------------------------------------- 
        readonly GlyphTriangle startTriangle;
        //each centoid line start with main triangle       
        Dictionary<GlyphTriangle, CentroidLine> _lines = new Dictionary<GlyphTriangle, CentroidLine>();
        //-----------------------------------------------
        List<CentroidLineHub> otherConnectedLineHubs;//connections from other hub***
        //-----------------------------------------------
        CentroidLine currentLine;
        GlyphTriangle currentBranchTri;

        public CentroidLineHub(GlyphTriangle startTriangle)
        {
            this.startTriangle = startTriangle;
        }
        public GlyphTriangle StartTriangle
        {
            get { return startTriangle; }
        }
        /// <summary>
        /// set current centroid line to a centroid line that starts with triangle of centroid-line-head
        /// </summary>
        /// <param name="triOfCentroidLineHead"></param>
        public void SetCurrentCentroidLine(GlyphTriangle triOfCentroidLineHead)
        {
            //this method is used during centroid line hub creation
            if (currentBranchTri != triOfCentroidLineHead)
            {
                //check if we have already create it
                if (!_lines.TryGetValue(triOfCentroidLineHead, out currentLine))
                {
                    //if not found then create new
                    currentLine = new CentroidLine();
#if  DEBUG
                    currentLine.dbugStartTri = triOfCentroidLineHead;
#endif
                    _lines.Add(triOfCentroidLineHead, currentLine);
                }
                currentBranchTri = triOfCentroidLineHead;
            }
        }
        /// <summary>
        /// member centoid line count
        /// </summary>
        public int LineCount
        {
            get
            {
                return _lines.Count;
            }
        }
        /// <summary>
        /// add centroid pair to current centroid line
        /// </summary>
        /// <param name="pair"></param>
        public void AddCentroidPair(GlyphCentroidPair pair)
        {
            //add centroid pair to line 
            currentLine.AddCentroidPair(pair);
        }
        /// <summary>
        /// analyze each branch for edge information
        /// </summary>
        public void CreateBoneJoints()
        {
            foreach (CentroidLine line in _lines.Values)
            {
                line.AnalyzeEdgesAndCreateBoneJoints();
            }
        }


        /// <summary>
        /// create a set of GlyphBone
        /// </summary>
        /// <param name="newlyCreatedBones"></param>
        public void CreateBones(List<GlyphBone> newlyCreatedBones)
        {
            foreach (CentroidLine line in _lines.Values)
            {
                List<GlyphBoneJoint> jointlist = line._joints;
                //start with empty bone list
                List<GlyphBone> glyphBones = line.bones;
                int j = jointlist.Count;

                if (j == 0) { continue; }
                //
                GlyphBoneJoint joint = jointlist[0]; //first 
                {
                    GlyphTriangle firstTri = joint.P_Tri;
                    //test 3 edges, find edge that is inside
                    //and the joint is not the same as first_pair.BoneJoint
                    CreateTipBoneIfNeed(firstTri.e0 as InsideEdgeLine, joint, newlyCreatedBones, glyphBones);
                    CreateTipBoneIfNeed(firstTri.e1 as InsideEdgeLine, joint, newlyCreatedBones, glyphBones);
                    CreateTipBoneIfNeed(firstTri.e2 as InsideEdgeLine, joint, newlyCreatedBones, glyphBones);
                }

                for (int i = 0; i < j; ++i)
                {
                    //for each GlyphCentroidPair                    
                    //create bone that link the GlyphBoneJoint of the pair  
                    joint = jointlist[i];
                    //if (joint.dbugId > 20)
                    //{
                    //}
                    if (joint.TipEdgeP != null)
                    {
                        GlyphBone tipBone = new GlyphBone(joint, joint.TipEdgeP);
                        newlyCreatedBones.Add(tipBone);
                        glyphBones.Add(tipBone);
                    }
                    //----------------------------------------------------- 
                    if (i < j - 1)
                    {
                        //not the last one
                        GlyphBoneJoint nextJoint = jointlist[i + 1];
                        GlyphBone bone = new GlyphBone(joint, nextJoint);
                        newlyCreatedBones.Add(bone);
                        glyphBones.Add(bone);
                    }

                    if (joint.TipEdgeQ != null)
                    {
                        GlyphBone tipBone = new GlyphBone(joint, joint.TipEdgeQ);
                        newlyCreatedBones.Add(tipBone);
                        glyphBones.Add(tipBone);
                    }
                }


                //for (int i = 1; i < j; ++i)
                //{
                //    joint = jointlist[i]; //first 
                //    {
                //        GlyphTriangle tri = joint.P_Tri;
                //        //test 3 edges, find edge that is inside
                //        //and the joint is not the same as first_pair.BoneJoint
                //        CreateTipBoneIfNeed(tri.e0 as InsideEdgeLine, joint, newlyCreatedBones, glyphBones);
                //        CreateTipBoneIfNeed(tri.e1 as InsideEdgeLine, joint, newlyCreatedBones, glyphBones);
                //        CreateTipBoneIfNeed(tri.e2 as InsideEdgeLine, joint, newlyCreatedBones, glyphBones);
                //    } 
                //}

            }
        }

        static void CreateTipBoneIfNeed(
            InsideEdgeLine insideEdge, GlyphBoneJoint joint,
            List<GlyphBone> newlyCreatedBones, List<GlyphBone> glyphBones)
        {
            if (insideEdge != null &&
                insideEdge.inside_joint != null &&
                insideEdge.inside_joint != joint)
            {
                //create connection 
                GlyphBone tipBone = new GlyphBone(insideEdge.inside_joint, joint);
                newlyCreatedBones.Add(tipBone);
                glyphBones.Add(tipBone);
            }
        }

        public void CreateBoneLinkBetweenCentroidLine(List<GlyphBone> newlyCreatedBones)
        {
            foreach (CentroidLine line in _lines.Values)
            {
                List<GlyphBone> glyphBones = line.bones;
                GlyphBoneJoint firstJoint = line._joints[0];
                GlyphTriangle first_p_tri = firstJoint.P_Tri;
                //                 
                CreateBoneJointIfNeed(first_p_tri.e0 as InsideEdgeLine, first_p_tri, firstJoint, newlyCreatedBones, glyphBones);
                CreateBoneJointIfNeed(first_p_tri.e1 as InsideEdgeLine, first_p_tri, firstJoint, newlyCreatedBones, glyphBones);
                CreateBoneJointIfNeed(first_p_tri.e2 as InsideEdgeLine, first_p_tri, firstJoint, newlyCreatedBones, glyphBones);
            }
        }
        static void CreateBoneJointIfNeed(
            InsideEdgeLine insideEdge,
            GlyphTriangle first_p_tri,
            GlyphBoneJoint firstJoint,
            List<GlyphBone> newlyCreatedBones,
            List<GlyphBone> glyphBones)
        {
            if (insideEdge != null &&
                insideEdge.inside_joint == null)
            {
                InsideEdgeLine mainEdge = insideEdge;
                EdgeLine nbEdge = null;
                if (FindSameCoordEdgeLine(first_p_tri.N0, mainEdge, out nbEdge) ||
                    FindSameCoordEdgeLine(first_p_tri.N1, mainEdge, out nbEdge) ||
                    FindSameCoordEdgeLine(first_p_tri.N2, mainEdge, out nbEdge))
                {

                    //confirm that nbEdge is INSIDE edge
                    if (nbEdge.IsInside)
                    {
                        GlyphBoneJoint joint = new GlyphBoneJoint((InsideEdgeLine)nbEdge, mainEdge);
                        GlyphBone bone = new GlyphBone(mainEdge.inside_joint, firstJoint);
                        newlyCreatedBones.Add(bone);
                        glyphBones.Add(bone);
                    }
                    else
                    {
                        //?
                    }
                }
                else
                {
                    //?
                }
            }
        }
        /// <summary>
        /// find nb triangle that has the same edgeLine
        /// </summary>
        /// <param name="tri"></param>
        /// <returns></returns>
        static bool FindSameCoordEdgeLine(GlyphTriangle tri, EdgeLine edgeLine, out EdgeLine foundEdge)
        {
            foundEdge = null;
            if (tri == null)
            {
                return false;
            }

            if (SameCoord(foundEdge = tri.e0, edgeLine) ||
                SameCoord(foundEdge = tri.e1, edgeLine) ||
                SameCoord(foundEdge = tri.e2, edgeLine))
            {
                return true;
            }
            foundEdge = null; //not found
            return false;
        }
        static bool SameCoord(EdgeLine a, EdgeLine b)
        {
            //TODO: review this again
            return (a.P == b.P ||
                    a.P == b.Q) &&
                   (a.Q == b.P ||
                    a.Q == b.Q);
        }

        public Dictionary<GlyphTriangle, CentroidLine> GetAllCentroidLines()
        {
            return _lines;
        }

        public bool FindBoneJoint(GlyphTriangle tri,
            out CentroidLine foundOnBranch,
            out GlyphBoneJoint foundOnJoint)
        {
            foreach (CentroidLine line in _lines.Values)
            {
                if ((foundOnJoint = line.FindNearestJoint(tri)) != null)
                {
                    foundOnBranch = line;
                    return true;
                }
            }
            foundOnBranch = null;
            foundOnJoint = null;
            return false;

        }
        public void AddLineHubConnection(CentroidLineHub anotherHub)
        {
            if (otherConnectedLineHubs == null)
            {
                otherConnectedLineHubs = new List<CentroidLineHub>();
            }
            otherConnectedLineHubs.Add(anotherHub);
        }

        CentroidLine anotherCentroidLine;
        GlyphBoneJoint foundOnJoint;

        public void SetHeadConnnection(CentroidLine anotherCentroidLine, GlyphBoneJoint foundOnJoint)
        {
            this.anotherCentroidLine = anotherCentroidLine;
            this.foundOnJoint = foundOnJoint;
        }
        public GlyphBoneJoint GetHeadConnectedJoint()
        {
            return foundOnJoint;
        }
        public List<CentroidLineHub> GetConnectedLineHubs()
        {
            return this.otherConnectedLineHubs;
        }

    }




    static class CentroidLineExtensions
    {  //utils

        public static Vector2 GetHeadPosition(this CentroidLine line)
        {
            //after create bone process
            List<GlyphBone> bones = line.bones;
            if (bones.Count == 0)
            {
                return Vector2.Zero;
            }
            else
            {
                //TODO: review here
                //use jointA of bone of join B of bone
                return bones[0].JointA.OriginalJointPos;
            }
        }

        public static Vector2 CalculateAvgHeadPosition(this CentroidLineHub lineHub)
        {
            Dictionary<GlyphTriangle, CentroidLine> _lines = lineHub.GetAllCentroidLines();
            int j = _lines.Count;
            if (j == 0) return Vector2.Zero;
            //---------------------------------
            double cx = 0;
            double cy = 0;
            foreach (CentroidLine line in _lines.Values)
            {
                Vector2 headpos = line.GetHeadPosition();
                cx += headpos.X;
                cy += headpos.Y;
            }
            return new Vector2((float)(cx / j), (float)(cy / j));
        }

    }
}