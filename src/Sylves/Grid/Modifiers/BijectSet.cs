using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sylves
{
    internal class BijectSet : ISet<Cell>
    {
        private readonly ISet<Cell> underlying;

        private readonly Func<Cell, Cell> toUnderlying;
        private readonly Func<Cell, Cell> fromUnderlying;

        public BijectSet(ISet<Cell> underlying, Func<Cell, Cell> toUnderlying, Func<Cell, Cell> fromUnderlying)
        {
            this.underlying = underlying;
            this.toUnderlying = toUnderlying;
            this.fromUnderlying = fromUnderlying;
        }

        public int Count => underlying.Count;

        public bool IsReadOnly => true;

        public bool Add(Cell item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(Cell item) => underlying.Contains(toUnderlying(item));

        public void CopyTo(Cell[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public void ExceptWith(IEnumerable<Cell> other)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            return underlying.Select(fromUnderlying).GetEnumerator();
        }

        public void IntersectWith(IEnumerable<Cell> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSubsetOf(IEnumerable<Cell> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<Cell> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<Cell> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSupersetOf(IEnumerable<Cell> other)
        {
            throw new NotImplementedException();
        }

        public bool Overlaps(IEnumerable<Cell> other)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Cell item)
        {
            throw new NotImplementedException();
        }

        public bool SetEquals(IEnumerable<Cell> other)
        {
            throw new NotImplementedException();
        }

        public void SymmetricExceptWith(IEnumerable<Cell> other)
        {
            throw new NotImplementedException();
        }

        public void UnionWith(IEnumerable<Cell> other)
        {
            throw new NotImplementedException();
        }

        void ICollection<Cell>.Add(Cell item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
