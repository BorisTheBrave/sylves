using System.Collections;
using System.Collections.Generic;

namespace Sylves
{
    /// <summary>
    /// Represents a 1:1 mapping between two types
    /// </summary>
    public class BiMap<U, V> : IEnumerable<(U, V)>
    {
        private readonly Dictionary<U, V> uToV;
        private readonly Dictionary<V, U> vToU;

        public BiMap(IEnumerable<(U, V)> data)
        {
            uToV = new Dictionary<U, V>();
            vToU = new Dictionary<V, U>();
            foreach (var (u, v) in data)
            {
                uToV.Add(u, v);
                vToU.Add(v, u);
            }
        }

        public V this[U u]
        {
            get { return uToV[u]; }
        }

        public U this[V v]
        {
            get { return vToU[v]; }
        }

        public int Count => uToV.Count;

        public IEnumerator<(U, V)> GetEnumerator()
        {
            foreach(var kv in uToV)
            {
                yield return (kv.Key, kv.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
