///*
//Copyright (c) 2014, Lars Brubaker
//All rights reserved.

//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met: 

//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer. 
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 

//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

//The views and conclusions contained in the software and documentation are those
//of the authors and should not be interpreted as representing official policies, 
//either expressed or implied, of the FreeBSD Project.
//*/

//using System;
//using System.Collections;
//using System.Collections.Generic;

//using System.Text;
//using NUnit.Framework;


//using PixelFarm.VectorMath;

//namespace PixelFarm.Agg
//{
//    public interface IKDLeafItem
//    {
//        int Dimensions
//        {
//            get;
//        }

//        double GetPositionForDimension(int dimension);
//        void SetPositionForDimension(int dimension, double position);
//    }

//    public class KDTreeNode<StoredType> where StoredType : IKDLeafItem
//    {
//        public double SplitPosition { get; set; }
//        public int DimensionSplitIsOn { get; set; }

//        public KDTreeNode<StoredType> NodeLessThanSplit { get; set; }
//        public KDTreeNode<StoredType> NodeGreaterThanOrEqualToSplit { get; set; }
//        public StoredType LeafItem { get; set; }

//        public IEnumerable GetDistanceEnumerator(double[] distanceOnEachDimension)
//        {
//            if (LeafItem != null && distanceOnEachDimension.Length != LeafItem.Dimensions)
//            {
//                throw new ArgumentException("You must pass in an array that is the same number of dimensions as the StoredType.");
//            }

//            throw new NotImplementedException();
//        }

//        public IEnumerable<StoredType> UnorderedEnumerator()
//        {
//            if (NodeLessThanSplit != null)
//            {
//                foreach (StoredType item in NodeLessThanSplit.UnorderedEnumerator())
//                {
//                    yield return item;
//                }
//            }

//            if (NodeGreaterThanOrEqualToSplit != null)
//            {
//                foreach (StoredType item in NodeGreaterThanOrEqualToSplit.UnorderedEnumerator())
//                {
//                    yield return item;
//                }
//            }

//            if (LeafItem != null)
//            {
//                yield return LeafItem;
//            }
//        }

//        public static double FindMedianOnDimension(IEnumerable<StoredType> listToCreateFrom, int dimension, out int count)
//        {
//            count = 0;
//            double accumulatedPosition = 0;
//            foreach (StoredType item in listToCreateFrom)
//            {
//                count++;
//                accumulatedPosition += item.GetPositionForDimension(dimension);
//            }

//            if (count > 0)
//            {
//                return accumulatedPosition / count;
//            }

//            return 0;
//        }

//        public static KDTreeNode<StoredType> CreateTree(IEnumerable<StoredType> collectionToCreateFrom, int splitingDimension = 0)
//        {
//            KDTreeUnitTests.Run();

//            int count;
//            double medianOnDimension = FindMedianOnDimension(collectionToCreateFrom, splitingDimension, out count);

//            if (count == 0)
//            {
//                return null;
//            }

//            StoredType firstItemFromCollection = default(StoredType); // if StoredType is a class this will set it to null - if struct, a zeroed struct.
//            foreach (StoredType item in collectionToCreateFrom)
//            {
//                firstItemFromCollection = item;
//                break;
//            }

//            KDTreeNode<StoredType> newNode = new KDTreeNode<StoredType>();
//            newNode.DimensionSplitIsOn = splitingDimension;

//            if (count > 1)
//            {
//                newNode.SplitPosition = medianOnDimension;
//                List<StoredType> lessThanList = new List<StoredType>();
//                List<StoredType> greaterThanOrEqualList = new List<StoredType>();
//                foreach (StoredType item in collectionToCreateFrom)
//                {
//                    double positionOfItemOnDimension = item.GetPositionForDimension(splitingDimension);
//                    if (positionOfItemOnDimension < newNode.SplitPosition)
//                    {
//                        lessThanList.Add(item);
//                    }
//                    else
//                    {
//                        if (positionOfItemOnDimension == newNode.SplitPosition && newNode.LeafItem == null)
//                        {
//                            // if all the points were in exactly the same position we would just get a big linked list.
//                            newNode.LeafItem = item;
//                        }
//                        else
//                        {
//                            greaterThanOrEqualList.Add(item);
//                        }
//                    }
//                }

//                int nextSplitDimension = (splitingDimension + 1) % firstItemFromCollection.Dimensions;
//                newNode.NodeLessThanSplit = CreateTree(lessThanList, nextSplitDimension);
//                newNode.NodeGreaterThanOrEqualToSplit = CreateTree(greaterThanOrEqualList, nextSplitDimension);

//                return newNode;
//            }

//            newNode.LeafItem = firstItemFromCollection;

//            return newNode;
//        }
//    }

//    public class Vector2DLeafItem : IKDLeafItem
//    {
//        Vector2 position;

//        public Vector2DLeafItem() { }

//        public Vector2DLeafItem(double x, double y)
//        {
//            position.x = x;
//            position.y = y;
//        }

//        public int Dimensions { get { return 2; } }
//        public double GetPositionForDimension(int dimension) { return position[dimension]; }
//        public void SetPositionForDimension(int dimension, double position) { this.position[dimension] = position; }
//    }

//    public class Vector3DLeafItem : IKDLeafItem
//    {
//        Vector3 position;

//        public Vector3DLeafItem() { }

//        public Vector3DLeafItem(double x, double y, double z)
//        {
//            position.x = x;
//            position.y = y;
//            position.z = z;
//        }

//        public int Dimensions { get { return 3; } }
//        public double GetPositionForDimension(int dimension) { return position[dimension]; }
//        public void SetPositionForDimension(int dimension, double position) { this.position[dimension] = position; }
//    }

//    [TestFixture]
//    public class KDTreeTests
//    {
//        [Test]
//        public void SamePointTest2D()
//        {
//            Vector2DLeafItem item1 = new Vector2DLeafItem(5, 5);
//            Vector2DLeafItem item2 = new Vector2DLeafItem(5, 5);
//            Vector2DLeafItem item3 = new Vector2DLeafItem(5, 5);
//            var itemList = new Vector2DLeafItem[] { item1, item2, item3 };

//            // IEnumerable<Vector2DLeafItem> enumerable = new Vector2DLeafItem[] { item1, item2, item3 }.AsEnumerable<Vector2DLeafItem>();
//            KDTreeNode<Vector2DLeafItem> rootNode = KDTreeNode<Vector2DLeafItem>.CreateTree(itemList);

//            KDTreeNode<Vector2DLeafItem> testNode = rootNode;
//            Assert.IsTrue(testNode.DimensionSplitIsOn == 0);
//            Assert.IsTrue(testNode.NodeLessThanSplit == null);
//            Assert.IsTrue(testNode.NodeGreaterThanOrEqualToSplit != null);
//            Assert.IsTrue(testNode.LeafItem == item1);

//            testNode = testNode.NodeGreaterThanOrEqualToSplit;
//            Assert.IsTrue(testNode.DimensionSplitIsOn == 1);
//            Assert.IsTrue(testNode.NodeLessThanSplit == null);
//            Assert.IsTrue(testNode.NodeGreaterThanOrEqualToSplit != null);
//            Assert.IsTrue(testNode.LeafItem == item2);

//            testNode = testNode.NodeGreaterThanOrEqualToSplit;
//            Assert.IsTrue(testNode.DimensionSplitIsOn == 0);
//            Assert.IsTrue(testNode.NodeLessThanSplit == null);
//            Assert.IsTrue(testNode.NodeGreaterThanOrEqualToSplit == null);
//            Assert.IsTrue(testNode.LeafItem == item3);
//        }

//        private static void RunTestOnNode3D(Vector3DLeafItem item1, Vector3DLeafItem item2, Vector3DLeafItem item3, KDTreeNode<Vector3DLeafItem> rootNode)
//        {
//            KDTreeNode<Vector3DLeafItem> testNode = rootNode;
//            Assert.IsTrue(testNode.DimensionSplitIsOn == 0);
//            Assert.IsTrue(testNode.NodeLessThanSplit == null);
//            Assert.IsTrue(testNode.NodeGreaterThanOrEqualToSplit != null);
//            Assert.IsTrue(testNode.LeafItem == item1);

//            testNode = testNode.NodeGreaterThanOrEqualToSplit;
//            Assert.IsTrue(testNode.DimensionSplitIsOn == 1);
//            Assert.IsTrue(testNode.NodeLessThanSplit == null);
//            Assert.IsTrue(testNode.NodeGreaterThanOrEqualToSplit != null);
//            Assert.IsTrue(testNode.LeafItem == item2);

//            testNode = testNode.NodeGreaterThanOrEqualToSplit;
//            Assert.IsTrue(testNode.DimensionSplitIsOn == 2);
//            Assert.IsTrue(testNode.NodeLessThanSplit == null);
//            Assert.IsTrue(testNode.NodeGreaterThanOrEqualToSplit == null);
//            Assert.IsTrue(testNode.LeafItem == item3);
//        }

//        [Test]
//        public void SamePointTest3D()
//        {
//            Vector3DLeafItem item1 = new Vector3DLeafItem(5, 5, 5);
//            Vector3DLeafItem item2 = new Vector3DLeafItem(5, 5, 5);
//            Vector3DLeafItem item3 = new Vector3DLeafItem(5, 5, 5);
//            var itemList = new Vector3DLeafItem[] { item1, item2, item3 };
//            KDTreeNode<Vector3DLeafItem> rootNode = KDTreeNode<Vector3DLeafItem>.CreateTree(itemList);

//            RunTestOnNode3D(item1, item2, item3, rootNode);
//        }

//        [Test]
//        public void CreateFromKDTree()
//        {
//            Vector3DLeafItem item1 = new Vector3DLeafItem(5, 5, 5);
//            Vector3DLeafItem item2 = new Vector3DLeafItem(5, 5, 5);
//            Vector3DLeafItem item3 = new Vector3DLeafItem(5, 5, 5);
//            var itemlist = new Vector3DLeafItem[] { item1, item2, item3 };
//            KDTreeNode<Vector3DLeafItem> rootNode = KDTreeNode<Vector3DLeafItem>.CreateTree(itemlist);
//            RunTestOnNode3D(item1, item2, item3, rootNode);

//            KDTreeNode<Vector3DLeafItem> fromRootNode = KDTreeNode<Vector3DLeafItem>.CreateTree(rootNode.UnorderedEnumerator());

//            KDTreeNode<Vector3DLeafItem> testNode = fromRootNode;
//            Assert.IsTrue(testNode.DimensionSplitIsOn == 0);
//            Assert.IsTrue(testNode.NodeLessThanSplit == null);
//            Assert.IsTrue(testNode.NodeGreaterThanOrEqualToSplit != null);

//            testNode = testNode.NodeGreaterThanOrEqualToSplit;
//            Assert.IsTrue(testNode.DimensionSplitIsOn == 1);
//            Assert.IsTrue(testNode.NodeLessThanSplit == null);
//            Assert.IsTrue(testNode.NodeGreaterThanOrEqualToSplit != null);

//            testNode = testNode.NodeGreaterThanOrEqualToSplit;
//            Assert.IsTrue(testNode.DimensionSplitIsOn == 2);
//            Assert.IsTrue(testNode.NodeLessThanSplit == null);
//            Assert.IsTrue(testNode.NodeGreaterThanOrEqualToSplit == null);
//        }

//        [Test]
//        public void EnumerateFromPoint()
//        {
//            Vector3DLeafItem item1 = new Vector3DLeafItem(1, 0, 0);
//            Vector3DLeafItem item2 = new Vector3DLeafItem(2, 0, 0);
//            Vector3DLeafItem item3 = new Vector3DLeafItem(3, 0, 0);
//            var itemList = new Vector3DLeafItem[] { item1, item2, item3 };
//            KDTreeNode<Vector3DLeafItem> rootNode = KDTreeNode<Vector3DLeafItem>.CreateTree(itemList);

//            int index = 0;
//            foreach (Vector3DLeafItem item in rootNode.GetDistanceEnumerator(new double[] { 2.1, 0, 0 }))
//            {
//                switch (index++)
//                {
//                    case 0:
//                        Assert.IsTrue(item == item2);
//                        break;

//                    case 1:
//                        Assert.IsTrue(item == item3);
//                        break;

//                    case 2:
//                        Assert.IsTrue(item == item1);
//                        break;
//                }
//            }
//        }
//    }

//    public static class KDTreeUnitTests
//    {
//        static bool ranTests = false;

//        public static bool RanTests { get { return ranTests; } }
//        public static void Run()
//        {
//            if (!ranTests)
//            {
//                ranTests = true;
//                KDTreeTests test = new KDTreeTests();
//                test.SamePointTest2D();
//                test.SamePointTest3D();
//                test.CreateFromKDTree();
//                test.EnumerateFromPoint();
//            }
//        }
//    }
//}
