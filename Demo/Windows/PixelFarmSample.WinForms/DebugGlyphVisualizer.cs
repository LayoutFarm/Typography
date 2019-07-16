//MIT, 2014-present, WinterDev
using PixelFarm.CpuBlit;
using PixelFarm.Drawing.Fonts;
using System;
using PixelFarm.VectorMath;
using Typography.Contours;
using Typography.OpenFont;
using PixelFarm.Contours;

namespace SampleWinForms.UI
{
    class GlyphTriangleInfo
    {
        public GlyphTriangleInfo(int triangleId, EdgeLine e0, EdgeLine e1, EdgeLine e2, double centroidX, double centroidY)
        {
            this.Id = triangleId;
            this.E0 = e0;
            this.E1 = e1;
            this.E2 = e2;
            this.CentroidX = centroidX;
            this.CentroidY = centroidY;
        }
        public int Id { get; private set; }
        public double CentroidX { get; set; }
        public double CentroidY { get; set; }
        public EdgeLine E0 { get; set; }
        public EdgeLine E1 { get; set; }
        public EdgeLine E2 { get; set; }
        public override string ToString()
        {
            return this.CentroidX + "," + CentroidY;

        }
    }

    class DebugGlyphVisualizer : OutlineWalker
    {
        DebugGlyphVisualizerInfoView _infoView;

        Typeface _typeface;
        float _sizeInPoint;
        GlyphPathBuilder _builder;

        PixelFarm.Drawing.Painter _painter;
        float _pxscale;
        HintTechnique _latestHint;
        char _testChar;

        public PixelFarm.Drawing.Painter CanvasPainter
        {
            get => _painter;
            set => _painter = value;
        }

        public void SetFont(Typeface typeface, float sizeInPoint)
        {
            _typeface = typeface;
            _sizeInPoint = sizeInPoint;
            _builder = new GlyphPathBuilder(typeface);
            FillBackGround = true;//default 

        }

        public bool UseLcdTechnique { get; set; }
        public bool FillBackGround { get; set; }
        public bool DrawBorder { get; set; }
        public bool OffsetMinorX { get; set; }
        public bool ShowTess { get; set; }
        public bool ShowTriangles { get; set; }

        public string MinorOffsetInfo { get; set; }
        public DebugGlyphVisualizerInfoView VisualizeInfoView
        {
            get { return _infoView; }
            set
            {
                _infoView = value;
                value.Owner = this;
                value.RequestGlyphRender += (s, e) =>
                {
                    //refresh render output 
                    RenderChar(_testChar, _latestHint);
                };
            }
        }
        public void DrawMarker(float x, float y, PixelFarm.Drawing.Color color, float sizeInPx = 8)
        {
            _painter.FillRect(x, y, sizeInPx, sizeInPx, color);
        }
        public float GlyphEdgeOffset { get; set; }
        public void RenderChar(char testChar, HintTechnique hint)
        {
            _builder.SetHintTechnique(hint);
#if DEBUG
            Joint.dbugTotalId = 0;//reset
            _builder.dbugAlwaysDoCurveAnalysis = true;
#endif
            _infoView.Clear();
            _latestHint = hint;
            _testChar = testChar;
            //----------------------------------------------------
            //
            _builder.GlyphDynamicEdgeOffset = this.GlyphEdgeOffset;
            _builder.Build(testChar, _sizeInPoint);
            var txToVxs1 = new GlyphTranslatorToVxs();
            

            _builder.ReadShapes(txToVxs1);

#if DEBUG              
            _infoView.ShowOrgBorderInfo(txToVxs1.dbugVxs);
#endif

            PixelFarm.Drawing.VertexStore vxs = new PixelFarm.Drawing.VertexStore();
            txToVxs1.WriteOutput(vxs);
            _painter.UseSubPixelLcdEffect = this.UseLcdTechnique;
            //5. use PixelFarm's Agg to render to bitmap...
            //5.1 clear background
            _painter.Clear(PixelFarm.Drawing.Color.White);

            RectD bounds = RectD.ZeroIntersection;
            PixelFarm.CpuBlit.VertexProcessing.BoundingRect.GetBoundingRect(vxs, ref bounds);
            //----------------------------------------------------
            float scale = _typeface.CalculateScaleToPixelFromPointSize(_sizeInPoint);
            _pxscale = scale;
            _infoView.PxScale = scale;


            var left2 = 0;
            int floor_1 = (int)left2;
            float diff = left2 - floor_1;
            //----------------------------------------------------
            if (OffsetMinorX)
            {
                MinorOffsetInfo = left2.ToString() + " =>" + floor_1 + ",diff=" + diff;
            }
            else
            {
                MinorOffsetInfo = left2.ToString();
            }


            //5. use PixelFarm's Agg to render to bitmap...
            //5.1 clear background
            _painter.Clear(PixelFarm.Drawing.Color.White);

            if (FillBackGround)
            {
                //5.2 
                _painter.FillColor = PixelFarm.Drawing.Color.Black;

                float xpos = 5;// - diff;
                if (OffsetMinorX)
                {
                    xpos -= diff;
                }

                _painter.SetOrigin(xpos, 10);
                _painter.Fill(vxs);
            }
            if (DrawBorder)
            {
                //5.4  
                _painter.StrokeColor = PixelFarm.Drawing.Color.Green;
                //user can specific border width here... 
                //5.5 
                _painter.Draw(vxs);
                //--------------
                int markOnVertexNo = _infoView.DebugMarkVertexCommand;

                vxs.GetVertex(markOnVertexNo, out double x, out double y);
                _painter.FillRect(x, y, 4, 4, PixelFarm.Drawing.Color.Red);
                //--------------
                _infoView.ShowFlatternBorderInfo(vxs);
                //--------------
            }
#if DEBUG
            _builder.dbugAlwaysDoCurveAnalysis = false;
#endif

            if (ShowTess)
            {
                RenderTessTesult();
            }


        }

        public void RenderTessTesult()
        {
#if DEBUG

            DynamicOutline dynamicOutline = _builder.LatestGlyphFitOutline;
            if (dynamicOutline != null)
            {
                this.Walk(dynamicOutline);
            }
#endif
        }

        public bool DrawDynamicOutline { get; set; }
        public bool DrawRegenerateOutline { get; set; }
        public bool DrawEndLineHub { get; set; }
        public bool DrawPerpendicularLine { get; set; }
        public bool DrawGlyphPoint { get; set; }
        public bool DrawEdgeMidPoint { get; set; }

#if DEBUG
        void DrawPointKind(PixelFarm.Drawing.Painter painter, Vertex point)
        {
            if (!DrawGlyphPoint) { return; }

            switch (point.PointKind)
            {
                case VertexKind.C3Start:
                case VertexKind.C3End:
                case VertexKind.C4Start:
                case VertexKind.C4End:
                case VertexKind.LineStart:
                case VertexKind.LineStop:

                    painter.FillRect(point.OX * _pxscale, point.OY * _pxscale, 5, 5, PixelFarm.Drawing.Color.Red);

                    break;
            }
        }

        void DrawEdge(PixelFarm.Drawing.Painter painter, EdgeLine edge)
        {
            if (edge.IsOutside)
            {
                //free side      
                {
                    Vertex p = edge.P;
                    Vertex q = edge.Q;

                    DrawPointKind(painter, p);
                    DrawPointKind(painter, q);
                    _infoView.ShowEdge(edge);
                    switch (edge.SlopeKind)
                    {
                        default:
                            painter.StrokeColor = PixelFarm.Drawing.Color.Green;
                            break;
                        case LineSlopeKind.Vertical:
                            if (edge.IsLeftSide)
                            {
                                painter.StrokeColor = PixelFarm.Drawing.Color.Blue;
                            }
                            else
                            {
                                painter.StrokeColor = PixelFarm.Drawing.Color.LightGray;
                            }
                            break;
                        case LineSlopeKind.Horizontal:

                            if (edge.IsUpper)
                            {
                                painter.StrokeColor = PixelFarm.Drawing.Color.Red;
                            }
                            else
                            {
                                //lower edge
                                painter.StrokeColor = PixelFarm.Drawing.Color.Magenta;
                            }
                            break;
                    }
                }
                float scale = _pxscale;
                //show info: => edge point
                if (this.DrawPerpendicularLine && _infoView.HasDebugMark)
                {
                    //double prevWidth = painter.StrokeWidth;
                    //painter.StrokeWidth = 3;
                    //painter.Line(edge.PX * scale, edge.PY * scale, edge.QX * scale, edge.QY * scale, PixelFarm.Drawing.Color.Yellow);
                    //painter.StrokeWidth = prevWidth;

                    ////draw
                    //GlyphPoint p = edge.P;
                    //GlyphPoint q = edge.Q;

                    //
                    //AssocBoneCollection p_bones = glyphEdge._P.dbugGetAssocBones();
                    //if (p_bones != null)
                    //{
                    //    Vector2 v2 = new Vector2(q.x, q.y);
                    //    foreach (GlyphBone b in p_bones)
                    //    {
                    //        Vector2 v3 = b.GetMidPoint();
                    //        painter.Line(v2.X * scale, v2.Y * scale, v3.X * scale, v3.Y * scale, PixelFarm.Drawing.Color.Yellow);
                    //    }
                    //}

                    //AssocBoneCollection q_bones = glyphEdge._Q.dbugGetAssocBones();
                    //if (q_bones != null)
                    //{
                    //    Vector2 v2 = new Vector2(p.x, p.y);
                    //    foreach (GlyphBone b in q_bones)
                    //    {

                    //        //Vector2 v2 = new Vector2(q.x, q.y);
                    //        Vector2 v3 = b.GetMidPoint();
                    //        painter.Line(v2.X * scale, v2.Y * scale, v3.X * scale, v3.Y * scale, PixelFarm.Drawing.Color.Green);
                    //    }
                    //}

                    {
                        //TODO: reimplement this again
                        //Vector2 orginal_MidPoint = glyphEdge.GetMidPoint() * _pxscale;
                        //Vector2 newMidPoint = glyphEdge.GetNewMidPoint() * _pxscale;
                        //painter.FillRectLBWH(newMidPoint.X, newMidPoint.Y, 3, 3, PixelFarm.Drawing.Color.Red);
                        //painter.Line(newMidPoint.X, newMidPoint.Y, orginal_MidPoint.X, orginal_MidPoint.Y, PixelFarm.Drawing.Color.LightGray);


                        //painter.FillRectLBWH(glyphEdge.newEdgeCut_P_X * _pxscale, glyphEdge.newEdgeCut_P_Y * _pxscale, 6, 6, PixelFarm.Drawing.Color.Blue);
                        //painter.FillRectLBWH(glyphEdge.newEdgeCut_Q_X * _pxscale, glyphEdge.newEdgeCut_Q_Y * _pxscale, 6, 6, PixelFarm.Drawing.Color.Blue);

                    }

                }
                else
                {
                    painter.DrawLine(edge.PX * scale, edge.PY * scale, edge.QX * scale, edge.QY * scale);
                }

                {

                    Vertex p = edge.P;
                    Vertex q = edge.Q;
                    //---------   
                    {
                        //TODO: reimplement this again
                        //Vector2 orginal_MidPoint = glyphEdge.GetMidPoint() * _pxscale;
                        //Vector2 newMidPoint = glyphEdge.GetNewMidPoint() * _pxscale;

                        //if (DrawEdgeMidPoint)
                        //{
                        //    painter.FillRectLBWH(newMidPoint.X, newMidPoint.Y, 3, 3, PixelFarm.Drawing.Color.Red);
                        //}
                        ////
                        //painter.Line(newMidPoint.X, newMidPoint.Y, orginal_MidPoint.X, orginal_MidPoint.Y, PixelFarm.Drawing.Color.LightGray);

                        //painter.FillRectLBWH(glyphEdge.newEdgeCut_P_X * _pxscale, glyphEdge.newEdgeCut_P_Y * _pxscale, 4, 4, PixelFarm.Drawing.Color.Blue);
                        //painter.FillRectLBWH(glyphEdge.newEdgeCut_Q_X * _pxscale, glyphEdge.newEdgeCut_Q_Y * _pxscale, 4, 4, PixelFarm.Drawing.Color.Blue); 
                    }
                    //---------   
                    if (this.DrawPerpendicularLine)
                    {
                        var asOutsideEdge = edge as OutsideEdgeLine;
                        if (asOutsideEdge != null)
                        {
                            DrawPerpendicularEdgeControlPoints(painter, asOutsideEdge);
                        }
                    }

                }
            }
            else
            {
                //draw inside edge 
                painter.Line(
                    edge.PX * _pxscale, edge.PY * _pxscale,
                    edge.QX * _pxscale, edge.QY * _pxscale,
                    PixelFarm.Drawing.Color.Gray);

            }
        }
        void DrawPerpendicularEdgeControlPoints(PixelFarm.Drawing.Painter painter, OutsideEdgeLine internalEdgeLine)
        {

            //Vector2 regen0 = edge._newRegen0 * _pxscale;
            //Vector2 regen1 = edge._newRegen1 * _pxscale;
            //painter.FillRectLBWH(regen0.X, regen0.Y, 5, 5, PixelFarm.Drawing.Color.Green);
            //painter.FillRectLBWH(regen1.X, regen1.Y, 5, 5, PixelFarm.Drawing.Color.Blue);

            bool foundSomePerpendicularEdge = false;

            if (internalEdgeLine.ControlEdge_P != null && internalEdgeLine.ControlEdge_Q != null)
            {
                Vector2f m0 = internalEdgeLine.ControlEdge_P.GetMidPoint();
                Vector2f m1 = internalEdgeLine.ControlEdge_Q.GetMidPoint();

                //find angle from m0-> m1

                Vector2f v2 = (m0 + m1) / 2;
                //find perpendicular line  from  midpoint_m0m1 to edge
                Vector2f cutpoint;
                if (MyMath.FindPerpendicularCutPoint(internalEdgeLine, v2, out cutpoint))
                {
                    painter.Line(
                       v2.X * _pxscale, v2.Y * _pxscale,
                       cutpoint.X * _pxscale, cutpoint.Y * _pxscale,
                       PixelFarm.Drawing.Color.Red);
                    foundSomePerpendicularEdge = true;
                }

                //Vector2 e0_fitpos = internalEdgeLine.ControlEdge_P.GetFitPos() * _pxscale;
                //Vector2 e1_fitpos = internalEdgeLine.ControlEdge_Q.GetFitPos() * _pxscale;

                //painter.Line(
                //      e0_fitpos.X, e0_fitpos.Y,
                //      regen0.X, regen0.Y,
                //      PixelFarm.Drawing.Color.Yellow);
                //painter.Line(
                //    e1_fitpos.X, e1_fitpos.Y,
                //    regen1.X, regen1.Y,
                //    PixelFarm.Drawing.Color.Yellow);
            }

            if (internalEdgeLine.ControlEdge_P != null)
            {
                Vector2f v2 = internalEdgeLine.ControlEdge_P.GetMidPoint();
                //Vector2 cutpoint = internalEdgeLine._ctrlEdge_P_cutAt;
                //painter.Line(
                //    v2.X * _pxscale, v2.Y * _pxscale,
                //    cutpoint.X * _pxscale, cutpoint.Y * _pxscale,
                //    PixelFarm.Drawing.Color.Green); 
                //foundSomePerpendicularEdge = true;
            }
            if (internalEdgeLine.ControlEdge_Q != null)
            {
                Vector2f v2 = internalEdgeLine.ControlEdge_Q.GetMidPoint();
                //Vector2 cutpoint = internalEdgeLine._ctrlEdge_Q_cutAt;
                //painter.Line(
                //    v2.X * _pxscale, v2.Y * _pxscale,
                //    cutpoint.X * _pxscale, cutpoint.Y * _pxscale,
                //    PixelFarm.Drawing.Color.Green);
                //foundSomePerpendicularEdge = true;
            }

            if (!foundSomePerpendicularEdge)
            {
                //TODO: reimplement this again
                //Vector2 midpoint = edge.GetMidPoint();
                //painter.FillRectLBWH(midpoint.X, midpoint.Y, 5, 5, PixelFarm.Drawing.Color.White);
            }
        }
        void DrawBoneJoint(PixelFarm.Drawing.Painter painter, Joint joint)
        {
            //-------------- 
            EdgeLine p_contactEdge = joint.dbugGetEdge_P();
            //mid point
            Vector2f jointPos = joint.OriginalJointPos * _pxscale;//scaled joint pos
            painter.FillRect(jointPos.X, jointPos.Y, 4, 4, PixelFarm.Drawing.Color.Yellow);
            if (joint.TipEdgeP != null)
            {
                EdgeLine tipEdge = joint.TipEdgeP;
                tipEdge.dbugGetScaledXY(out double p_x, out double p_y, out double q_x, out double q_y, _pxscale);
                //
                painter.Line(
                   jointPos.X, jointPos.Y,
                   p_x, p_y,
                   PixelFarm.Drawing.Color.White);
                painter.FillRect(p_x, p_y, 3, 3, PixelFarm.Drawing.Color.Green); //marker

                //
                painter.Line(
                    jointPos.X, jointPos.Y,
                    q_x, q_y,
                    PixelFarm.Drawing.Color.White);
                painter.FillRect(q_x, q_y, 3, 3, PixelFarm.Drawing.Color.Green); //marker
            }
            if (joint.TipEdgeQ != null)
            {
                EdgeLine tipEdge = joint.TipEdgeQ;
                tipEdge.dbugGetScaledXY(out double p_x, out double p_y, out double q_x, out double q_y, _pxscale);
                //
                painter.Line(
                   jointPos.X, jointPos.Y,
                   p_x, p_y,
                   PixelFarm.Drawing.Color.White);
                painter.FillRect(p_x, p_y, 3, 3, PixelFarm.Drawing.Color.Green); //marker

                //
                painter.Line(
                    jointPos.X, jointPos.Y,
                    q_x, q_y,
                    PixelFarm.Drawing.Color.White);
                painter.FillRect(q_x, q_y, 3, 3, PixelFarm.Drawing.Color.Green); //marker
            }

        }

        Vector2f _branchHeadPos;
        protected override void OnBeginDrawingBoneLinks(Vector2f branchHeadPos, int startAt, int endAt)
        {
            _branchHeadPos = branchHeadPos;
        }
        protected override void OnEndDrawingBoneLinks()
        {

        }


        protected override void OnDrawBone(Bone bone, int boneIndex)
        {


            float pxscale = _pxscale;
            Joint jointA = bone.JointA;
            Joint jointB = bone.JointB;

            bool valid = false;
            if (jointA != null && jointB != null)
            {

                Vector2f jointAPoint = jointA.OriginalJointPos;
                Vector2f jointBPoint = jointB.OriginalJointPos;

                _painter.Line(
                      jointAPoint.X * pxscale, jointAPoint.Y * pxscale,
                      jointBPoint.X * pxscale, jointBPoint.Y * pxscale,
                      bone.IsLongBone ? PixelFarm.Drawing.Color.Yellow : PixelFarm.Drawing.Color.Magenta);

                if (this.DrawDynamicOutline)
                {
                    //****
                    _painter.Line(
                        jointA.FitX * pxscale, jointA.FitY * pxscale,
                        jointB.FitX * pxscale, jointB.FitY * pxscale,
                        PixelFarm.Drawing.Color.White);
                }

                valid = true;

                _infoView.ShowBone(bone, jointA, jointB);
            }
            if (jointA != null && bone.TipEdge != null)
            {
                Vector2f jointAPoint = jointA.OriginalJointPos;
                Vector2f mid = bone.TipEdge.GetMidPoint();

                _painter.Line(
                    jointAPoint.X * pxscale, jointAPoint.Y * pxscale,
                    mid.X * pxscale, mid.Y * pxscale,
                    bone.IsLongBone ? PixelFarm.Drawing.Color.Yellow : PixelFarm.Drawing.Color.Magenta);

                valid = true;
                _infoView.ShowBone(bone, jointA, bone.TipEdge);
            }

            if (boneIndex == 0)
            {
                //for first bone 
                _painter.FillRect(_branchHeadPos.X * pxscale, _branchHeadPos.Y * pxscale, 5, 5, PixelFarm.Drawing.Color.DeepPink);
            }
            if (!valid)
            {
                throw new NotSupportedException();
            }
        }


        protected override void OnTriangle(int triangleId, EdgeLine e0, EdgeLine e1, EdgeLine e2, double centroidX, double centroidY)
        {

            DrawEdge(_painter, e0);
            DrawEdge(_painter, e1);
            DrawEdge(_painter, e2);

            _infoView.ShowTriangles(new GlyphTriangleInfo(triangleId, e0, e1, e2, centroidX, centroidY));


        }

        protected override void OnGlyphEdgeN(EdgeLine e)
        {
            float pxscale = _pxscale;

            //TODO: reimplement this again

            //Vector2 cut_p = new Vector2(e.newEdgeCut_P_X, e.newEdgeCut_P_Y) * pxscale;
            //Vector2 cut_q = new Vector2(e.newEdgeCut_Q_X, e.newEdgeCut_Q_Y) * pxscale; 
            //painter.FillRectLBWH(cut_p.X, cut_p.Y, 3, 3, PixelFarm.Drawing.Color.Red);
            //painter.FillRectLBWH(x1 * pxscale, y1 * pxscale, 6, 6, PixelFarm.Drawing.Color.OrangeRed);

            //_infoView.ShowGlyphEdge(e,
            //    e.newEdgeCut_P_X, e.newEdgeCut_P_Y,
            //    e.newEdgeCut_Q_X, e.newEdgeCut_Q_Y);
        }
        protected override void OnCentroidLine(double px, double py, double qx, double qy)
        {

            float pxscale = _pxscale;
            //red centroid line
            _painter.Line(
                px * pxscale, py * pxscale,
                qx * pxscale, qy * pxscale,
                PixelFarm.Drawing.Color.Red);
            ///small yellow marker at p and q point of centroid
            _painter.FillRect(px * pxscale, py * pxscale, 2, 2, PixelFarm.Drawing.Color.Yellow);
            _painter.FillRect(qx * pxscale, qy * pxscale, 2, 2, PixelFarm.Drawing.Color.Yellow);
        }
        protected override void OnCentroidLineTip_P(double px, double py, double tip_px, double tip_py)
        {
            float pxscale = _pxscale;
            _painter.Line(px * pxscale, py * pxscale,
                         tip_px * pxscale, tip_py * pxscale,
                         PixelFarm.Drawing.Color.Blue);
        }
        protected override void OnCentroidLineTip_Q(double qx, double qy, double tip_qx, double tip_qy)
        {
            float pxscale = _pxscale;
            _painter.Line(qx * pxscale, qy * pxscale,
                         tip_qx * pxscale, tip_qy * pxscale,
                         PixelFarm.Drawing.Color.Green);
        }
        protected override void OnBoneJoint(Joint joint)
        {
            DrawBoneJoint(_painter, joint);
            _infoView.ShowJoint(joint);
        }
        //----------------------
        protected override void OnBegingLineHub(float centerX, float centerY)
        {

        }
        protected override void OnEndLineHub(float centerX, float centerY, Joint joint)
        {

            if (DrawEndLineHub)
            {
                //line hub cebter
                _painter.FillRect(centerX * _pxscale, centerY * _pxscale, 7, 7,
                       PixelFarm.Drawing.Color.White);
                //this line hub is connected with other line hub at joint
                if (joint != null)
                {
                    Vector2f joint_pos = joint.OriginalJointPos;
                    _painter.Line(
                            joint_pos.X * _pxscale, joint_pos.Y * _pxscale,
                            centerX * _pxscale, centerY * _pxscale,
                            PixelFarm.Drawing.Color.Magenta);
                }
            }
        } 
#endif 
      
    }

}