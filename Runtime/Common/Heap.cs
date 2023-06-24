using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{

    /// <summary>
    /// Standard binary min-heap.
    /// </summary>
    internal class Heap<T, TKey> where TKey : IComparable<TKey>
    {
        (T Value, TKey Key)[] data;
        int size;

        private static int Parent(int i) => (i - 1) >> 1;
        private static int Left(int i) => (i << 1) + 1;
        private static int Right(int i) => (i << 1) + 2;

        public Heap()
        {
            data = new (T value, TKey key)[0];
            size = 0;
        }

        public Heap(int capacity)
        {
            data = new (T value, TKey key)[capacity];
            size = 0;
        }

        public int Count => size;

        public T Peek()
        {
            if (size == 0)
                throw new Exception("Heap is empty");
            return data[0].Value;
        }
        public TKey PeekKey()
        {
            if (size == 0)
                throw new Exception("Heap is empty");
            return data[0].Key;
        }

        public T Pop()
        {
            if (size == 0)
                throw new Exception("Heap is empty");
            var r = data[0].Value;
            data[0] = data[size - 1];
            size--;
            Heapify(0);
            return r;
        }

        public void Heapify()
        {
            for (var i = Parent(size); i >= 0; i--)
            {
                Heapify(i);
            }
        }

        private void DecreasedKey(int i)
        {
            var priority = data[i].Key;
            while (true)
            {
                if (i == 0)
                {
                    return;
                }

                var p = Parent(i);
                var parent = data[p];
                var parentP = parent.Key;

                if (parentP.CompareTo(priority) > 0)
                {
                    (data[p], data[i]) = (data[i], data[p]);
                    i = p;
                    continue;
                }
                else
                {
                    return;
                }
            }
        }

        private void Heapify(int i)
        {
            var ip = data[i].Key;
            var smallest = i;
            var smallestP = ip;
            var l = Left(i);
            if (l < size)
            {
                var lp = data[l].Key;
                if (lp.CompareTo(smallestP) < 0)
                {
                    smallest = l;
                    smallestP = lp;
                }
            }
            var r = Right(i);
            if (r < size)
            {
                var rp = data[r].Key;
                if (rp.CompareTo(smallestP) < 0)
                {
                    smallest = r;
                    smallestP = rp;
                }
            }
            if (i != smallest)
            {
                (data[i], data[smallest]) = (data[smallest], data[i]);
                Heapify(smallest);
            }
        }

        public void Insert(T item, TKey key)
        {
            if(data.Length == 0)
            {
                data = new (T value, TKey key)[4];
            }
            if (data.Length == size)
            {
                var data2 = new (T value, TKey key)[size * 2];
                Array.Copy(data, data2, size);
                data = data2;
            }
            data[size].Value = item;
            data[size].Key = key;
            size++;
            DecreasedKey(size - 1);
        }

        public void Clear()
        {
            size = 0;
        }
    }
}
