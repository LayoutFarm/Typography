//BSD, 2014-present, WinterDev

using System;
namespace PixelFarm.CpuBlit.Imaging
{
    class Queue<T>
    {
        T[] _itemArray;
        int _size;
        int _head;
        int _shiftFactor;
        int _mask;
        //

        public Queue(int shiftFactor)
        {
            _shiftFactor = shiftFactor;
            _mask = (1 << shiftFactor) - 1;
            _itemArray = new T[1 << shiftFactor];
            _head = 0;
            _size = 0;
        }
        public int Count => _size;
        public T First => _itemArray[_head & _mask];


        public void Enqueue(T itemToQueue)
        {
            if (_size == _itemArray.Length)
            {
                int headIndex = _head & _mask;
                _shiftFactor += 1;
                _mask = (1 << _shiftFactor) - 1;
                T[] newArray = new T[1 << _shiftFactor];
                // copy the from head to the end
                Array.Copy(_itemArray, headIndex, newArray, 0, _size - headIndex);
                // copy form 0 to the size
                Array.Copy(_itemArray, 0, newArray, _size - headIndex, headIndex);
                _itemArray = newArray;
                _head = 0;
            }
            _itemArray[(_head + (_size++)) & _mask] = itemToQueue;
        }

        public T Dequeue()
        {
            int headIndex = _head & _mask;
            T firstItem = _itemArray[headIndex];
            if (_size > 0)
            {
                _head++;
                _size--;
            }
            return firstItem;
        }
    }
}