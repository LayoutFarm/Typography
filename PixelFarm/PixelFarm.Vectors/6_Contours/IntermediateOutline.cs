//MIT, 2017-present, WinterDev
using System;
using System.Collections.Generic;
using Poly2Tri;
namespace PixelFarm.Contours
{

    public class IntermediateOutline
    {

        List<Contour> _contours;
        List<CentroidLineHub> _lineHubs;
        List<AnalyzedTriangle> _allTriangles;

        float _bounds_minX, _bounds_minY, _bounds_maxX, _bounds_maxY;

        public IntermediateOutline(
            List<Contour> contours,
            List<Polygon> polygons)
        {
            //init value

            _contours = contours;
            //1. create centroid line hubs: 
            CreateCentroidLineHubs(polygons);
            //2. create bone joints (create joint before bone)
            CreateBoneJoints();
            //3. create bones 
            CreateBones();
            //4. create edges          
            CreateEdges();
        }

        void CreateCentroidLineHubs(List<Polygon> polygons)
        {

            _lineHubs = new List<CentroidLineHub>();
#if DEBUG
            EdgeLine.s_dbugTotalId = 0;//reset 
#endif
            _allTriangles = new List<AnalyzedTriangle>();
            //main polygon
            List<AnalyzedTriangle> triangles = new List<AnalyzedTriangle>();
            CreateCentroidLineHubs(polygons[0], triangles, _lineHubs);
            _allTriangles.AddRange(triangles);
            int j = polygons.Count;
            for (int i = 1; i < j; ++i)
            {
                triangles.Clear();//clear, reuse it
                CreateCentroidLineHubs(polygons[i], triangles, _lineHubs);
                _allTriangles.AddRange(triangles);
            }

        }
        static void CreateCentroidLineHubs(Polygon polygon, List<AnalyzedTriangle> triangles, List<CentroidLineHub> outputLineHubs)
        {
             
            int id = 0;
            foreach (DelaunayTriangle delnTri in polygon.Triangles)
            {
                delnTri.MarkAsActualTriangle();
                triangles.Add(new AnalyzedTriangle(id, delnTri)); //all triangles are created from Triangulation process
                id++;
            }

            //----------------------------
            //create centroid line hub
            //----------------------------
            //1.
            var centroidLineHubs = new Dictionary<AnalyzedTriangle, CentroidLineHub>();
            CentroidLineHub currentCentroidLineHub = null;

            //2. temporary list of used triangles
            List<AnalyzedTriangle> usedTriList = new List<AnalyzedTriangle>();
            AnalyzedTriangle latestTri = null;

            //we may walk forward and backward on each tri
            //so we record the used triangle into a usedTriList.
            int triCount = triangles.Count;
            for (int i = 0; i < triCount; ++i)
            {
                AnalyzedTriangle tri = triangles[i];
                if (i == 0)
                {
                    centroidLineHubs[tri] = currentCentroidLineHub = new CentroidLineHub(tri);
                    usedTriList.Add(latestTri = tri);
                }
                else
                {
                    //at a branch 
                    //one tri may connect with 3 NB triangle
                    int foundIndex = FindLatestConnectedTri(usedTriList, tri);
                    if (foundIndex < 0)
                    {
                        //?
                        throw new NotSupportedException();
                    }
                    else
                    {
                        //record used triangle
                        usedTriList.Add(tri);

                        AnalyzedTriangle connectWithPrevTri = usedTriList[foundIndex];
                        if (connectWithPrevTri != latestTri)
                        {
                            //branch
                            CentroidLineHub lineHub;
                            if (!centroidLineHubs.TryGetValue(connectWithPrevTri, out lineHub))
                            {
                                //if not found then=> //start new CentroidLineHub 
                                centroidLineHubs[connectWithPrevTri] = lineHub = new CentroidLineHub(connectWithPrevTri);
                                //create linehub to line hub connection
                                //TODO: review here 
                                //create centroid pair at link point 
                            }
                            else
                            {
                                //this is multiple facets triangle for  CentroidLineHub
                            }

                            currentCentroidLineHub = lineHub;
                            //ensure start triangle of the branch
                            lineHub.SetCurrentCentroidLine(tri);
                            //create centroid line and add to currrent hub 
                            currentCentroidLineHub.AddCentroidPair(new CentroidPair(connectWithPrevTri, tri));
                        }
                        else
                        {
                            //add centroid line to current multifacet joint 
                            if (currentCentroidLineHub.LineCount == 0)
                            {
                                //ensure start triangle of the branch
                                currentCentroidLineHub.SetCurrentCentroidLine(tri);
                            }
                            //create centroid line and add to currrent hub
                            currentCentroidLineHub.AddCentroidPair(new CentroidPair(connectWithPrevTri, tri));
                        }
                        latestTri = tri;
                    }
                }
            }

            //--------------------------------------------------------------
            //copy to list, we not use the centroidLineHubs anymore
            outputLineHubs.AddRange(centroidLineHubs.Values);
        }

        void CreateBoneJoints()
        {
            int lineHubCount = _lineHubs.Count;
            for (int i = 0; i < lineHubCount; ++i)
            {
                _lineHubs[i].CreateBoneJoints();
            }
            for (int i = 0; i < lineHubCount; ++i)
            {
                //after create bone joint ***                 
                LinkEachLineHubTogether(_lineHubs[i], _lineHubs);
            }
        }
        void CreateBones()
        {

            List<Bone> newBones = new List<Bone>();
            int lineHubCount = _lineHubs.Count;
            for (int i = 0; i < lineHubCount; ++i)
            {
                //create bones and collect back to newBones                
                _lineHubs[i].CreateBones(newBones);
            }
            //----------------------------------------
            //check connection between head of each centroid line
            for (int i = 0; i < lineHubCount; ++i)
            {
                _lineHubs[i].CreateBoneLinkBetweenCentroidLine(newBones);
            }
        }

        void CreateEdges()
        {
            //reset bounds
            _bounds_minX = _bounds_minY = float.MaxValue;
            _bounds_maxX = _bounds_maxY = float.MinValue;

            List<Contour> contours = _contours;
            int j = contours.Count;
            for (int i = 0; i < j; ++i)
            {
                Contour cnt = contours[i];
                cnt.CreateEdges();
                //this is a new found after fitting process
                cnt.FindBounds(ref _bounds_minX, ref _bounds_minY, ref _bounds_maxX, ref _bounds_maxY);
            }
        }
        /// <summary>
        /// min x after fitting process
        /// </summary>
        public float MinX => _bounds_minX;
        /// <summary>
        /// min y after fitting process
        /// </summary>
        public float MinY => _bounds_minY;
        /// <summary>
        /// max x after fitting process
        /// </summary>
        public float MaxX => _bounds_maxX;
        /// <summary>
        ///  max y after fitting process
        /// </summary>
        public float MaxY => _bounds_maxY;

        /// <summary>
        /// find link from main triangle of line-hub to another line hub
        /// </summary>
        /// <param name="analyzingHub"></param>
        /// <param name="hubs"></param>
        static void LinkEachLineHubTogether(CentroidLineHub analyzingHub, List<CentroidLineHub> hubs)
        {
            int j = hubs.Count;
            for (int i = 0; i < j; ++i)
            {
                CentroidLineHub otherHub = hubs[i];
                if (otherHub == analyzingHub)
                {
                    continue;
                }

                //from a given hub,
                //find bone joint that close to the main triangle for of the analyzingHub
                if (otherHub.FindBoneJoint(analyzingHub.StartTriangle, out CentroidLine foundOnBr, out Joint foundOnJoint))
                {
                    //create a new bone joint 
                    // FindNearestEdge(analyzingHub.MainTriangle, foundOnJoint); 
                    //add connection from analyzingHub to otherHub
                    otherHub.AddLineHubConnection(analyzingHub);
                    //also set head connection from joint to this analyzing hub
                    analyzingHub.SetHeadConnnection(foundOnBr, foundOnJoint);
                    //
                    //TODO: review this, why return here?
                    return;
                }
            }
        }

        static int FindLatestConnectedTri(List<AnalyzedTriangle> usedTriList, AnalyzedTriangle tri)
        {
            //search back ***
            for (int i = usedTriList.Count - 1; i >= 0; --i)
            {
                if (usedTriList[i].IsConnectedTo(tri))
                {
                    return i;
                }
            }
            return -1;
        }
        //
        public List<CentroidLineHub> GetCentroidLineHubs() => _lineHubs;
        //
        public List<Contour> GetContours() => _contours;
        // 
        public List<AnalyzedTriangle> GetTriangles()
        {
            return _allTriangles;
        }
    }

}