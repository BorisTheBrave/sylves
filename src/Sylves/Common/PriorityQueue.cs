using System;
using System.Collections.Generic;

namespace Sylves
{
    // Not actually a priority queue. This is a standin until we get a better impl
    internal class PriorityQueue<T>
    {
        private readonly Func<T, float> extract;
        private readonly Comparison<T> comparer;

        // Ordered with smallest values at end
        private readonly List<T> queue;

        public PriorityQueue(Func<T, float> extract, Comparison<T> comparer = null)
        {
            this.extract = extract;
            this.comparer = comparer ?? ((x, y) => -extract(x).CompareTo(extract(y)));
            this.queue = new List<T>();
        }

        public void Add(T item)
        {
            queue.Add(item);
        }
        public void AddRange(IEnumerable<T> items)
        {
            queue.AddRange(items);
        }
        public int Count => queue.Count;

        public T Pop()
        {
            queue.Sort(comparer);
            var last = queue.Count - 1;
            var t = queue[last];
            queue.RemoveAt(last);
            return t;
        }


        public T Peek()
        {
            queue.Sort(comparer);
            var last = queue.Count - 1;
            return queue[last];
        }

        public IEnumerable<T> Drain(float value)
        {
            queue.Sort(comparer);

            while (queue.Count > 0)
            {
                var last = queue.Count - 1;
                var f = extract(queue[last]);
                if (f < value)
                {
                    yield return queue[last];
                    queue.RemoveAt(last);
                }
                else
                {
                    yield break;
                }
            }
        }

        public IEnumerable<T> Drain()
        {
            queue.Sort(comparer);

            while (queue.Count > 0)
            {
                var last = queue.Count - 1;
                yield return queue[last];
                queue.RemoveAt(last);
            }
        }
    }
}
