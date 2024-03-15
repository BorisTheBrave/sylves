using System.Collections;
using System.Collections.Generic;

namespace Sylves
{
    public struct Triple<T> : IEnumerable<T>
    {
        public T Item1;
        public T Item2;
        public T Item3;

        public static Triple<T> Create(T item1, T item2, T item3) => new Triple<T>(item1, item2, item3);

        public Triple(T item1, T item2, T item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return Item1;
            yield return Item2;
            yield return Item3;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return Item1;
            yield return Item2;
            yield return Item3;
        }

        public static implicit operator (T, T, T)(Triple<T> t) => (t.Item1, t.Item2, t.Item3);
        public static implicit operator Triple<T>((T, T, T) t) => Create(t.Item1, t.Item2, t.Item3);
    }
}
