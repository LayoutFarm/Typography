//BSD, 2014-present, WinterDev

using System;
using System.Collections.Generic;
namespace Tesselate
{

    //TODO: review this again....
    //-----
    //design for our tess only
    //not for general use.
    //----- 
    class RefItem<T>
      where T : IComparable<T>
    {
        public RefItem(T data)
        {
            this.Data = data;
        }
        public T Data { get; set; }
        public int NodeNumber { get; set; }
#if DEBUG
        public override string ToString()
        {
            return this.Data.ToString();
        }
#endif
    }

    class MaxFirstList<T>
      where T : IComparable<T>
    {
        List<RefItem<T>> _innerList = new List<RefItem<T>>();
        bool _isSorted = false;
        public MaxFirstList()
        {
        }
        //
        public bool IsEmpty => _innerList.Count == 0;
        //
        static int MaxFirstSort(RefItem<T> t1, RefItem<T> t2)
        {
            return t2.Data.CompareTo(t1.Data);
        }
        void SortData()
        {
            _innerList.Sort(MaxFirstSort);
            for (int i = _innerList.Count - 1; i >= 0; --i)
            {
                _innerList[i].NodeNumber = i;
            }
            _isSorted = true;
        }
        public T DeleteMin()
        {
            //find min and delete 
            if (!_isSorted)
            {
                SortData();
            }
            int last = _innerList.Count - 1;
            var tmp = _innerList[last];
            _innerList.RemoveAt(last);
            return tmp.Data;
        }
        public T FindMin()
        {
            if (!_isSorted)
            {
                SortData();
            }
            return _innerList[_innerList.Count - 1].Data;
        }
        public void Add(out RefItem<T> refItem, T data)
        {
            RefItem<T> item = new RefItem<T>(data);
            _innerList.Add(item);
            _isSorted = false;
            refItem = item;
        }
        public void Add(T data)
        {
            RefItem<T> item = new RefItem<T>(data);
            _innerList.Add(item);
            _isSorted = false;
        }
        int BinSearch(RefItem<T> refItem, int begin, int end)
        {
            int pos = begin + ((end - begin) / 2);
            RefItem<T> sample = _innerList[pos];
            if (refItem == sample)
            {
            }
            else
            {
                if (sample.Data.CompareTo(refItem.Data) <= 0)
                {
                    //search down
                    end = end - ((pos - begin) / 2);
                    if (end == begin)
                    {
                        return -1;
                    }
                    return BinSearch(refItem, begin, end);
                }
                else
                {
                    //search up
                    begin = pos + (end - pos) / 2;
                    if (end == begin)
                    {
                        return -1;
                    }
                    return BinSearch(refItem, begin, end);
                }
            }

            return -1;//not found
        }
        public void Delete(RefItem<T> refItem)
        {
            //delete specfic node 

            if (_isSorted)
            {
                //use binary search to find node 
                //1. find middle point 
                int removeAt = refItem.NodeNumber;
                for (int i = _innerList.Count - 1; i > removeAt; --i)
                {
                    _innerList[i].NodeNumber = i - 1;
                }
                _innerList.RemoveAt(removeAt);
            }
            else
            {
                for (int i = _innerList.Count - 1; i >= 0; --i)
                {
                    if (_innerList[i] == refItem)
                    {
                        _innerList.RemoveAt(i);
                        break;
                    }
                }
            }
            //----------------------------------------------
            //delete that item  
        }
    }
}