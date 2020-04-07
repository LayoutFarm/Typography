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
        public T Data { get; private set; }
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
        readonly List<RefItem<T>> _innerList = new List<RefItem<T>>();
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
        public void Add(T data, out RefItem<T> refItem)
        {
            RefItem<T> item = new RefItem<T>(data);
            refItem = item;

            if (_isSorted)
            {
                int pos = FindProperInsertPos(data);
                if (pos >= _innerList.Count)
                {
                    _innerList.Add(item);
                }
                else
                {
                    _innerList.Insert(pos, item);
                }

                //SortData();
                //int actualPos = BinSearch(item, 0, _innerList.Count - 1);
                //if (actualPos != pos)
                //{

                //}
            }
            else
            {
                _innerList.Add(item);
            }
        }
        internal int FindProperInsertPos(T data)
        {
            int begin = 0;
            int end = _innerList.Count - 1;
        TRY_AGAIN:
            int pos = begin + ((end - begin) / 2);
            T sample = _innerList[pos].Data;
            int compare = sample.CompareTo(data);

            if (compare == 0)
            {
                return pos + 1;
            }
            else
            {
                if (begin >= end)
                {
                    //stop
                    if (compare >= 0)
                    {
                        return pos + 1;
                    }

                    return pos;
                }
                if (compare < 0)
                {
                    //this is MaxFirst list
                    //data at this pos is lesser than refItem.Data
                    //we need to move to the begin side of the list                      
                    end = pos - 1;
                    goto TRY_AGAIN;
                    //return BinSearch(refItem, begin, pos - 1);
                }
                else
                {
                    //this is MaxFirst list
                    //data at this pos is greater than refItem.Data
                    //we need to move to the end of this list                     
                    begin = pos + 1;
                    goto TRY_AGAIN;
                    //return BinSearch(refItem, pos + 1, end);
                }
            }
        }
        int BinSearch(RefItem<T> refItem, int begin, int end)
        {
        TRY_AGAIN:
            int pos = begin + ((end - begin) / 2);
            RefItem<T> sample = _innerList[pos];

            if (refItem == sample)
            {
                return pos;
            }
            else
            {
                if (begin == end)
                {
                    return -1;//not found
                }
                if (sample.Data.CompareTo(refItem.Data) < 0)
                {
                    //this is MaxFirst list
                    //data at this pos is lesser than refItem.Data
                    //we need to move to the begin side of the list                      
                    end = pos - 1;
                    goto TRY_AGAIN;
                    //return BinSearch(refItem, begin, pos - 1);
                }
                else
                {
                    //this is MaxFirst list
                    //data at this pos is greater than refItem.Data
                    //we need to move to the end of this list                     
                    begin = pos + 1;
                    goto TRY_AGAIN;
                    //return BinSearch(refItem, pos + 1, end);
                }
            }
        }
        public int Search(RefItem<T> refItem)
        {
            if (!_isSorted)
            {
                SortData();
            }
            return BinSearch(refItem, 0, _innerList.Count - 1);
        }
        public void Delete(RefItem<T> refItem)
        {
            //delete specfic node 

            if (_isSorted)
            {
                int pos = BinSearch(refItem, 0, _innerList.Count - 1);
                if (pos > -1)
                {
                    _innerList.RemoveAt(pos);
                }
                //int actualPos = -1;
                //for (int i = _innerList.Count - 1; i >= 0; --i)
                //{
                //    if (_innerList[i] == refItem)
                //    {
                //        actualPos = i;
                //        break;
                //    }
                //}

                //if (pos != actualPos)
                //{

                //}
                //for (int i = _innerList.Count - 1; i >= 0; --i)
                //{
                //    if (_innerList[i] == refItem)
                //    {
                //        _innerList.RemoveAt(i);
                //        break;
                //    }
                //}
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