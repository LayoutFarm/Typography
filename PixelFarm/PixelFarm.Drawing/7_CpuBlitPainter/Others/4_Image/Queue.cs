//BSD, 2014-present, WinterDev

using System;
namespace PixelFarm.CpuBlit.Imaging
{
    class Queue<T>
    {
        T[] itemArray;
        int size;
        int head;
        int shiftFactor;
        int mask;
        public int Count
        {
            get { return size; }
        }

        public Queue(int shiftFactor)
        {
            this.shiftFactor = shiftFactor;
            mask = (1 << shiftFactor) - 1;
            itemArray = new T[1 << shiftFactor];
            head = 0;
            size = 0;
        }

        public T First
        {
            get { return itemArray[head & mask]; }
        }

        public void Enqueue(T itemToQueue)
        {
            if (size == itemArray.Length)
            {
                int headIndex = head & mask;
                shiftFactor += 1;
                mask = (1 << shiftFactor) - 1;
                T[] newArray = new T[1 << shiftFactor];
                // copy the from head to the end
                Array.Copy(itemArray, headIndex, newArray, 0, size - headIndex);
                // copy form 0 to the size
                Array.Copy(itemArray, 0, newArray, size - headIndex, headIndex);
                itemArray = newArray;
                head = 0;
            }
            itemArray[(head + (size++)) & mask] = itemToQueue;
        }

        public T Dequeue()
        {
            int headIndex = head & mask;
            T firstItem = itemArray[headIndex];
            if (size > 0)
            {
                head++;
                size--;
            }
            return firstItem;
        }
    }
}