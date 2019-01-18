//MIT, 2019-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;

namespace PixelFarm.PathReconstruction
{


    class HSpanColumn
    {
        List<HSpan> _spanList = new List<HSpan>();
        bool _leftSideChecked;
        bool _rightSideChecked;


#if DEBUG
        bool dbugEvalCorners;
        public bool dbugEvalEnd;
#endif
        public HSpanColumn(VerticalGroup owner, int colNumber)
        {
            ColNumber = colNumber;
            OwnerVerticalGroup = owner;
        }
        public VerticalGroup OwnerVerticalGroup { get; }
        public int ColNumber { get; }

        public void AddHSpan(HSpan span)
        {
            _spanList.Add(span);
            XLeftBottom = span.startX;
            XRightBottom = span.endX;
            YBottom = span.y + 1;
        }
        public void EvaluateCorners()
        {
#if DEBUG
            dbugEvalCorners = true;
#endif
            //the column may not be rectangle shape ***
            if (_spanList.Count == 0)
            {
                //??
                throw new System.NotSupportedException();
            }
            //..

            HSpan hspan = _spanList[0];
            XLeftTop = hspan.startX;
            XRightTop = hspan.endX;
            YTop = hspan.y;

            hspan = _spanList[_spanList.Count - 1];
            XLeftBottom = hspan.startX;
            XRightBottom = hspan.endX;
            YBottom = hspan.y + 1;//***

        }

        public int YTop { get; set; }
        public int YBottom { get; set; }

        public int XLeftTop { get; private set; }
        public int XRightTop { get; private set; }
        public int XLeftBottom { get; private set; }
        public int XRightBottom { get; private set; }

        public void ResetRead()
        {
            _leftSideChecked = _rightSideChecked = false;
        }


        public void ReadLeftSide(RawOutline pathW, bool topDown)
        {
            //read once
            if (_leftSideChecked) throw new System.NotSupportedException();

            _leftSideChecked = true;

            if (topDown)
            {
                int count = _spanList.Count;

                RawOutline.BeginLoadSegmentPoints(pathW);
                for (int i = 0; i < count; ++i)
                {
                    HSpan span = _spanList[i];
                    pathW.AppendPoint(span.startX, span.y);
                }
                RawOutline.EndLoadSegmentPoints(pathW);
            }
            else
            {
                RawOutline.BeginLoadSegmentPoints(pathW);
                for (int i = _spanList.Count - 1; i >= 0; --i)
                {
                    HSpan span = _spanList[i];
                    pathW.AppendPoint(span.startX, span.y);
                }
                RawOutline.EndLoadSegmentPoints(pathW);
            }
        }
        public void ReadRightSide(RawOutline pathW, bool topDown)
        {
            if (_rightSideChecked) throw new System.NotSupportedException();

            _rightSideChecked = true;

            if (topDown)
            {
                RawOutline.BeginLoadSegmentPoints(pathW);
                int count = _spanList.Count;
                for (int i = 0; i < count; ++i)
                {
                    HSpan span = _spanList[i];
                    pathW.AppendPoint(span.endX, span.y);
                }
                RawOutline.EndLoadSegmentPoints(pathW);
            }
            else
            {
                RawOutline.BeginLoadSegmentPoints(pathW);
                for (int i = _spanList.Count - 1; i >= 0; --i)
                {
                    HSpan span = _spanList[i];
                    pathW.AppendPoint(span.endX, span.y);
                }
                RawOutline.EndLoadSegmentPoints(pathW);
            }
        }

        public bool HasRightColumn => this.ColNumber < this.OwnerVerticalGroup.ColumnCount - 1; //not the last one
        public bool HasLeftColumn => this.ColNumber > 0;

        public bool LeftSideIsRead => _leftSideChecked;
        public bool RightSideIsRead => _rightSideChecked;


        public HSpanColumn FindLeftColumn() => HasLeftColumn ? OwnerVerticalGroup.GetColumn(ColNumber - 1) : null;
        public HSpanColumn FindRightColumn() => HasRightColumn ? OwnerVerticalGroup.GetColumn(ColNumber + 1) : null;

        public HSpanColumn FindLeftLowerColumn()
        {
            VerticalGroup ownerGroup = OwnerVerticalGroup;
            return ownerGroup.IsLastGroup ? null :
                    ownerGroup.GetLowerGroup().TopSideFindFirstTouchColumnFromLeft(this.YBottom, this.XLeftBottom, this.XRightBottom);
        }
        public HSpanColumn FindRightLowerColumn()
        {
            VerticalGroup ownerGroup = OwnerVerticalGroup;
            return ownerGroup.IsLastGroup ? null :
                   ownerGroup.GetLowerGroup().TopSideFindFirstTouchColumnFromRight(this.YBottom, this.XLeftBottom, this.XRightBottom);
        }
        public HSpanColumn FindRightUpperColumn()
        {
            VerticalGroup ownerGroup = OwnerVerticalGroup;
            return ownerGroup.IsFirstGroup ? null :
                   ownerGroup.GetUpperGroup().BottomSideFindFirstTouchColumnFromRight(this.YTop, this.XLeftTop, this.XRightTop);
        }
        public HSpanColumn FindLeftUpperColumn()
        {
            VerticalGroup ownerGroup = OwnerVerticalGroup;
            return ownerGroup.IsFirstGroup ? null :
                   ownerGroup.GetUpperGroup().BottomSideFindFirstTouchColumnFromLeft(this.YTop, this.XLeftTop, this.XRightTop);
        }
        public ReadSide FindUnreadSide()
        {
            ReadSide incompleteSide = ReadSide.None;
            if (!_leftSideChecked)
            {
                incompleteSide |= ReadSide.Left;
            }
            if (!_rightSideChecked)
            {
                incompleteSide |= ReadSide.Right;
            }

            return incompleteSide;
        }

        /// <summary>
        /// check if the bottom side of this group touch with specific range 
        /// </summary>
        /// <param name="lowerGroupTopLeft"></param>
        /// <param name="lowerGroupTopRight"></param>
        /// <returns></returns>
        public bool BottomSideTouchWith(int lowerGroupTop, int lowerGroupTopLeft, int lowerGroupTopRight)
        {
            //[     THIS group    ]
            //---------------------                
            //[ other (lower)group]


            return (lowerGroupTop != this.YBottom) ?
                        false :
                        HSpan.HorizontalTouchWith(
                                 XLeftBottom, XRightBottom,
                                 lowerGroupTopLeft, lowerGroupTopRight);
        }
        /// <summary>
        /// check if the top side of this group touch with specific range)
        /// </summary>
        /// <param name="upperBottomLeft"></param>
        /// <param name="upperBottomRight"></param>
        /// <returns></returns>
        public bool TopSideTouchWith(int upperBottom, int upperBottomLeft, int upperBottomRight)
        {

            //[ other (lower)group]
            //---------------------  
            //[     THIS group    ] 

            //find the first column that its top side touch with 
            //another uppper group   

            return (upperBottom != this.YTop) ?
                        false :
                        HSpan.HorizontalTouchWith(
                                 XLeftTop, XRightTop,
                                 upperBottomLeft, upperBottomRight);
        }
#if DEBUG
        public override string ToString()
        {
            if (dbugEvalCorners)
            {

                return "grp:" + OwnerVerticalGroup.GroupNo + ",col:" + ColNumber +
                    ",Y:" + YTop + "," + YBottom +
                    ",X_top:" + XLeftTop + "," + XRightTop +
                    ",X_bottom:" + XLeftBottom + "," + XRightBottom;
            }
            else
            {
                return "grp:" + OwnerVerticalGroup.GroupNo + ",col:" + ColNumber;
            }

        }
#endif
    }

    [System.Flags]
    enum ReadSide : byte
    {
        None = 0,
        Left = 1,
        Right = 1 << 1,//2 
        LeftAndRight = Left | Right //3
    }
    struct Remaining
    {
        public readonly HSpanColumn column;
        public readonly ReadSide unreadSide;
        public Remaining(HSpanColumn column, ReadSide unreadSide)
        {
            this.column = column;
            this.unreadSide = unreadSide;
        }
    }

    class VerticalGroup
    {
        HSpanColumn[] _hSpanColumns;
        bool _completeAll;
        VerticalGroupList _ownerVertGroupList;
        public VerticalGroup(VerticalGroupList ownerVertGroupList, int groupNo,
            HSpan[] hspans,
            int startIndex,
            int colCount)
        {
            _ownerVertGroupList = ownerVertGroupList;
            _hSpanColumns = new HSpanColumn[colCount];
            GroupNo = groupNo;
            int index = startIndex;
            for (int i = 0; i < colCount; ++i)
            {
                var col = new HSpanColumn(this, i);
                col.AddHSpan(hspans[index]);
                _hSpanColumns[i] = col;
                index++;
            }
            StartY = hspans[startIndex].y;
        }
        public int ColumnCount => _hSpanColumns.Length;
        public int GroupNo { get; }

        public int StartY { get; }
        public HSpanColumn GetColumn(int index) => _hSpanColumns[index];

        public int CurrentReadColumnIndex { get; set; }
        public HSpanColumn CurrentColumn => _hSpanColumns[CurrentReadColumnIndex];


        HSpanColumn FindTouchColumn(HSpan newspan, ref int colIndex)
        {

            for (int i = colIndex; i < _hSpanColumns.Length; ++i)
            {
                HSpanColumn col = _hSpanColumns[i];
                if (col.BottomSideTouchWith(newspan.y, newspan.startX, newspan.endX))
                {
                    //found
                    colIndex = i;
                    return col;
                }
            }
            //----

            if (colIndex > 0)
            {
                //we didn't start from the first
                for (int i = 0; i < colIndex; ++i)
                {
                    HSpanColumn col = _hSpanColumns[i];
                    if (col.BottomSideTouchWith(newspan.y, newspan.startX, newspan.endX))
                    {
                        //found
                        colIndex = i;
                        return col;
                    }
                }
            }

            //not found
            return null;
        }

        public bool AddHSpans(HSpan[] hspans, int startIndex, int count)
        {

            int index = startIndex;
            //we must ...
            //1. touch one by one
            //and 2. no overlaped column
            for (int i = 0; i < _hSpanColumns.Length; ++i)
            {
                HSpanColumn col = _hSpanColumns[i];
                HSpan hspan = hspans[index];
                if (!col.BottomSideTouchWith(hspan.y, hspan.startX, hspan.endX))
                {
                    //found some 'untouch column'
                    //break all 
                    //need another vertical group
                    return false;
                }
                else if (i > 0 && _hSpanColumns[i - 1].BottomSideTouchWith(hspan.y, hspan.startX, hspan.endX))
                {
                    //see Test/Data/lion_1_v3_2.png for example
                    //check if current hspan dose not touch with prev column                     
                    //in this case => start a new column  
                    return false;
                }
                index++;
            }
            //---
            //pass all
            index = startIndex; //reset
            for (int i = 0; i < _hSpanColumns.Length; ++i)
            {
                HSpanColumn col = _hSpanColumns[i];
                col.AddHSpan(hspans[index]);
                index++;
            }
            return true;
        }


        public bool IsLastGroup => GroupNo == _ownerVertGroupList.Count - 1;
        public bool IsFirstGroup => GroupNo == 0;
        public VerticalGroup GetUpperGroup() => IsFirstGroup ? null : _ownerVertGroupList.GetGroup(GroupNo - 1);
        public VerticalGroup GetLowerGroup() => IsLastGroup ? null : _ownerVertGroupList.GetGroup(GroupNo + 1);

        public void EvaluateColumnCorners()
        {
            //can do this more than 1 times
            for (int i = 0; i < _hSpanColumns.Length; ++i)
            {
                _hSpanColumns[i].EvaluateCorners();
            }
        }

        public HSpanColumn BottomSideFindFirstTouchColumnFromLeft(int lowerGroupTop, int lowerGroupTopLeft, int lowerGroupTopRight)
        {
            //[     THIS group    ]
            //---------------------                
            //[ other (lower)group]
            //
            //find the first column that its bottom side touch with 
            //another lower group               

            for (int i = 0; i < _hSpanColumns.Length; ++i)
            {
                HSpanColumn col = _hSpanColumns[i];
                if (col.BottomSideTouchWith(lowerGroupTop, lowerGroupTopLeft, lowerGroupTopRight))
                {
                    return col;
                }
            }
            return null;
        }
        public HSpanColumn BottomSideFindFirstTouchColumnFromRight(int lowerGroupTop, int lowerGroupTopLeft, int lowerGroupTopRight)
        {
            //[     THIS group    ]
            //---------------------                
            //[ other (lower)group]
            //
            //find the first column that its bottom side touch with 
            //another lower group  
            for (int i = _hSpanColumns.Length - 1; i >= 0; --i)
            {
                HSpanColumn col = _hSpanColumns[i];
                if (col.BottomSideTouchWith(lowerGroupTop, lowerGroupTopLeft, lowerGroupTopRight))
                {
                    return col;
                }
            }
            return null;
        }
        public HSpanColumn TopSideFindFirstTouchColumnFromLeft(int upperBottom, int upperBottomLeft, int upperBottomRight)
        {

            //[ other (lower)group]
            //---------------------  
            //[     THIS group    ]


            //find the first column that its top side touch with 
            //another uppper group  
            for (int i = 0; i < _hSpanColumns.Length; ++i)
            {
                HSpanColumn col = _hSpanColumns[i];
                if (col.TopSideTouchWith(upperBottom, upperBottomLeft, upperBottomRight))
                {
                    return col;
                }
            }
            return null;
        }
        public HSpanColumn TopSideFindFirstTouchColumnFromRight(int upperBottom, int upperBottomLeft, int upperBottomRight)
        {
            //[ other (lower)group]
            //---------------------  
            //[     THIS group    ]


            //find the first column that its top side touch with 
            //another uppper group 

            for (int i = _hSpanColumns.Length - 1; i >= 0; --i)
            {
                HSpanColumn col = _hSpanColumns[i];
                if (col.TopSideTouchWith(upperBottom, upperBottomLeft, upperBottomRight))
                {
                    return col;
                }
            }
            return null;
        }


        public void CollectIncompleteRead(List<Remaining> incompleteColumns)
        {
            if (_completeAll) return;
            //
            bool hasSomeIncompleteColumn = false;
            for (int i = 0; i < _hSpanColumns.Length; ++i)
            {
                HSpanColumn hspanCol = _hSpanColumns[i];
                ReadSide incompleteSide = hspanCol.FindUnreadSide();
                if (incompleteSide != ReadSide.None)
                {
                    hasSomeIncompleteColumn = true;
                    incompleteColumns.Add(new Remaining(hspanCol, incompleteSide));
                }
            }
            _completeAll = !hasSomeIncompleteColumn;
        }
#if DEBUG
        public override string ToString()
        {
            return StartY + " ,col=" + ColumnCount;
        }
#endif
    }


    struct VerticalGroupSeparator
    {
        int _lastestLine;
        VerticalGroup _currentVertGroup;
        VerticalGroupList _verticalGroupList;
        public VerticalGroupSeparator(VerticalGroupList verticalGroupList)
        {
            _lastestLine = -1;
            _currentVertGroup = null;
            _verticalGroupList = verticalGroupList;
        }

        public void Separate(HSpan[] hspans)
        {
            int count = hspans.Length;
            if (count == 0) return;

            int startCollectIndex = 0;
            int colCount = 0;
            _lastestLine = hspans[0].y;

            for (int i = 0; i < count; ++i)
            {
                HSpan sp = hspans[i];
                int lineDiff = sp.y - _lastestLine;
                switch (lineDiff)
                {
                    case 1:
                        {
                            //go next lower line
                            //flush current collected columns 
                            FlushCollectedColumns(hspans, startCollectIndex, colCount);
                            //
                            startCollectIndex = i;
                            colCount = 1;
                            _lastestLine = sp.y;
                        }
                        break;
                    case 0:
                        {
                            //sameline
                            colCount++;
                        }
                        break;
                    default:
                        throw new System.NotSupportedException();
                }
            }

            if (startCollectIndex < count - 1)
            {
                //flush remaining 
                FlushCollectedColumns(hspans, startCollectIndex, colCount);
            }
        }
        void FlushCollectedColumns(HSpan[] hspans, int start, int colCount)
        {
            if (_currentVertGroup == null ||
                _currentVertGroup.ColumnCount != colCount)
            {
                //start new group
                //create new                     
                _verticalGroupList.Append(
                    _currentVertGroup = new VerticalGroup(_verticalGroupList, _verticalGroupList.Count, hspans, start, colCount));
                return;
            }

            if (_currentVertGroup.AddHSpans(hspans, start, colCount))
            {
                //pass
                return;
            }

            //create and add to a new vertical group
            _verticalGroupList.Append(
                    _currentVertGroup = new VerticalGroup(_verticalGroupList, _verticalGroupList.Count, hspans, start, colCount));
        }
    }

    class VerticalGroupList
    {
        List<VerticalGroup> _verticalGroupList = new List<VerticalGroup>();

        public int Count => _verticalGroupList.Count;

        public VerticalGroup GetGroup(int index) => _verticalGroupList[index];

        public void Append(VerticalGroup vertGtoup)
        {
            _verticalGroupList.Add(vertGtoup);
        }
        public void EvaluateCorners()
        {
            int j = _verticalGroupList.Count;
            for (int i = 0; i < j; ++i)
            {
                _verticalGroupList[i].EvaluateColumnCorners();
            }
        }
        public void CollectIncompleteColumns(List<Remaining> incompleteReadList)
        {
            int j = _verticalGroupList.Count;
            for (int i = 0; i < j; ++i)
            {
                _verticalGroupList[i].CollectIncompleteRead(incompleteReadList);
            }
        }
    }

    struct ColumnWalkerCcw
    {

        int _vertGroupCount;
        RawOutline _pathWriter;
        HSpanColumn _currentCol;
        bool _latestReadOnRightSide;
        VerticalGroupList _vertGroupList;
        public ColumnWalkerCcw(VerticalGroupList verticalGroupList)
        {
            _pathWriter = null;
            _latestReadOnRightSide = false;
            _vertGroupList = verticalGroupList;
            _vertGroupCount = verticalGroupList.Count;
            _currentCol = null;
        }
        public void Bind(RawOutline pathW)
        {
            _pathWriter = pathW;
        }
        public void Bind(HSpanColumn hspanCol)
        {
            _currentCol = hspanCol;
        }

        public void ReadLeftSide()
        {
            _latestReadOnRightSide = false;
            _currentCol.ReadLeftSide(_pathWriter, true);
        }
        public void ReadRightSide()
        {
            _latestReadOnRightSide = true;
            _currentCol.ReadRightSide(_pathWriter, false);
        }
        public Remaining FindReadNextColumn()
        {
            if (!_latestReadOnRightSide)
            {
                //latest state is on LEFT side of the column
                HSpanColumn leftLowerCol = _currentCol.FindLeftLowerColumn();
                HSpanColumn leftCol = _currentCol.FindLeftColumn();
                if (leftLowerCol != null)
                {
                    if (leftCol != null)
                    {
                        HSpanColumn rightLowerCol = leftCol.FindRightLowerColumn();
                        if (leftLowerCol == rightLowerCol)
                        {
                            //if they share the same 
                            return leftCol.RightSideIsRead ?
                                        new Remaining() : //complete
                                        new Remaining(leftCol, ReadSide.Right);
                        }
                        else
                        {

                            if (leftLowerCol.LeftSideIsRead && !leftCol.RightSideIsRead)
                            {

                                return new Remaining(leftCol, ReadSide.Right);
                            }
                        }
                    }

                    return leftLowerCol.LeftSideIsRead ?
                            new Remaining() : //complete
                            new Remaining(leftLowerCol, ReadSide.Left);
                }
                else
                {    //no lower column => this is Bottom-End

                    if (!_currentCol.RightSideIsRead)
                    {
                        return new Remaining(_currentCol, ReadSide.Right);
                    }
                    else
                    {

                    }

                    return new Remaining(); //complete
                }
            }
            else
            {
                //latest state is on RIGHT side of the column  
                HSpanColumn rightUpperCol = _currentCol.FindRightUpperColumn();
                HSpanColumn rightCol = _currentCol.FindRightColumn();

                if (rightUpperCol != null)
                {
                    if (rightCol != null)
                    {
                        HSpanColumn leftUpperCol = rightCol.FindLeftUpperColumn();
                        if (rightUpperCol == leftUpperCol)
                        {
                            return rightCol.LeftSideIsRead ?
                                        new Remaining() : //complete
                                        new Remaining(rightCol, ReadSide.Left);
                        }
                        else
                        {
                            if (rightUpperCol.RightSideIsRead && !rightCol.LeftSideIsRead)
                            {
                                //???
                                return new Remaining(rightCol, ReadSide.Left);
                            }
                        }
                    }
                    return rightUpperCol.RightSideIsRead ?
                            new Remaining() :
                            new Remaining(rightUpperCol, ReadSide.Right);
                }
                else
                {
                    if (rightCol != null && !rightCol.LeftSideIsRead)
                    {

                    }

                    //no upper column => this is Top-End
                    return _currentCol.LeftSideIsRead ?
                             new Remaining() :
                             new Remaining(_currentCol, ReadSide.Left);
                }
            }
        }
    }

    public class OutlineTracer
    {
        VerticalGroupList _verticalGroupList = new VerticalGroupList();

        void TraceOutlineCcw(Remaining toReadNext, RawOutline output, bool outside)
        {
            output.BeginContour(outside);
            //if we starts on left-side of the column                
            ColumnWalkerCcw ccw = new ColumnWalkerCcw(_verticalGroupList);
            ccw.Bind(output);
            //-------------------------------
            for (; ; )
            {
                switch (toReadNext.unreadSide)
                {
                    default:
                        throw new System.NotSupportedException();
                    case ReadSide.Left:
                        ccw.Bind(toReadNext.column);
                        ccw.ReadLeftSide();
                        break;
                    case ReadSide.Right:
                        ccw.Bind(toReadNext.column);
                        ccw.ReadRightSide();
                        break;
                    case ReadSide.None:
                        //complete
                        output.EndContour();
                        return;
                }
                toReadNext = ccw.FindReadNextColumn();
            }

        }
        /// <summary>
        /// trace outline counter-clockwise
        /// </summary>
        /// <param name="output"></param>
        void TraceOutline(HSpan[] sortedHSpans, RawOutline output)
        {
            if (sortedHSpans == null) return;
            //
            var sep = new VerticalGroupSeparator(_verticalGroupList);
            sep.Separate(sortedHSpans);

            int vertGroupCount = _verticalGroupList.Count;
            if (vertGroupCount == 0) return;

            _verticalGroupList.EvaluateCorners();


            List<Remaining> incompleteReadList = new List<Remaining>();
            TraceOutlineCcw(new Remaining(_verticalGroupList.GetGroup(0).GetColumn(0), ReadSide.Left), output, true);

            TRACE_AGAIN://**

            //check if the shape have hole(s) 
            _verticalGroupList.CollectIncompleteColumns(incompleteReadList);

            if (incompleteReadList.Count > 0)
            {
                //this should be a hole
                Remaining incompleteRead = incompleteReadList[0];
                switch (incompleteRead.unreadSide)
                {
                    //?should not occur
                    case ReadSide.LeftAndRight:
                        {
                            TraceOutlineCcw(new Remaining(incompleteRead.column, ReadSide.Left), output, false);
                            incompleteReadList.Clear();

                            goto TRACE_AGAIN;
                        }
                    case ReadSide.Left:
                    case ReadSide.Right:
                        {
                            TraceOutlineCcw(incompleteRead, output, false);
                            incompleteReadList.Clear();
                            goto TRACE_AGAIN;

                        }
                }
            }
            else
            {
                //complete all
            }
        }
        public void TraceOutline(ReconstructedRegionData rgnData, RawOutline output)
        {
            TraceOutline(rgnData.HSpans, output);
        }
    }


    //------------------------------------------------------------------------------
    public static class RawPathExtensions
    {
        public static void Simplify(this RawOutline rgnOutline, float tolerance = 0.5f, bool heighQualityEnable = false)
        {

            int j = rgnOutline._contours.Count;
            for (int i = 0; i < j; ++i)
            {
                RawContour contour = rgnOutline._contours[i];
                var simplifiedPoints = PixelFarm.CpuBlit.VertexProcessing.SimplificationHelpers.Simplify(
                     contour._xyCoords,
                     (p1, p2) => p1 == p2,
                     p => p.x,
                     p => p.y,
                     tolerance,
                     heighQualityEnable);
                //replace current raw contour with the new one
#if DEBUG
                System.Diagnostics.Debug.WriteLine("simplification before:" + contour._xyCoords.Count + ",after" + simplifiedPoints.Count);
#endif

                //create a new raw contour, 
                //but you can replace internal data of the old contour too,
                RawContour newContour = new RawContour();
                newContour.IsOutside = contour.IsOutside;
                foreach (var point in simplifiedPoints)
                {
                    newContour.AddPoint(point);
                }
                rgnOutline._contours[i] = newContour;
            }
        }
    }



    /// <summary>
    /// outline of the region
    /// </summary>
    public class RawOutline
    {
        internal List<RawContour> _contours = new List<RawContour>();
        RawContour _currentContour;
        public RawOutline() { }
        internal void BeginContour(bool outside)
        {
            _currentContour = new RawContour();
            _currentContour.IsOutside = outside;
            _contours.Add(_currentContour);
        }
        internal void EndContour()
        {

        }

        internal void AppendPoint(int x, int y) => _currentContour.AddPoint(x, y);
        internal int ContourCount => _contours.Count;

        internal RawContour GetContour(int index) => _contours[index];

        internal static void BeginLoadSegmentPoints(RawOutline rawPath) => rawPath.OnBeginLoadSegmentPoints();

        internal static void EndLoadSegmentPoints(RawOutline rawPath) => rawPath.OnEndLoadSegmentPoints();


        protected virtual void OnBeginLoadSegmentPoints()
        {
            //for hinting
            //that the following AppendPoints come from the same vertical column side
        }
        protected virtual void OnEndLoadSegmentPoints()
        {
            //for hinting
            //that the following AppendPoints come from the same vertical column side
        }

        public void MakeVxs(VertexStore vxs)
        {
            int contourCount = _contours.Count;

            for (int i = 0; i < contourCount; ++i)
            {
                //each contour
                RawContour contour = _contours[i];
                List<Point> xyCoords = contour._xyCoords;
                int count = xyCoords.Count;

                if (count > 1)
                {
                    if (contour.IsOutside)
                    {
                        Point p = xyCoords[0];
                        vxs.AddMoveTo(p.x, p.y);
                        for (int n = 1; n < count; ++n)
                        {
                            p = xyCoords[n];
                            vxs.AddLineTo(p.x, p.y);
                        }
                        vxs.AddCloseFigure();
                    }
                    else
                    {
                        Point p = xyCoords[count - 1];
                        vxs.AddMoveTo(p.x, p.y);
                        for (int n = count - 1; n >= 0; --n)
                        {
                            p = xyCoords[n];
                            vxs.AddLineTo(p.x, p.y);
                        }
                        vxs.AddCloseFigure();
                    }

                }
            }

        }
    }

    class RawContour
    {
        internal List<Point> _xyCoords = new List<Point>();
        public RawContour() { }

        public bool IsOutside { get; set; }
        public virtual void AddPoint(int x, int y)
        {
            _xyCoords.Add(new Point(x, y));
        }
        public virtual void AddPoint(Point p)
        {
            _xyCoords.Add(p);
        }
    }



}