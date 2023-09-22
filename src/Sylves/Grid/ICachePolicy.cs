using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Sylves
{

    /// <summary>
    /// Describes how long data should be cached.
    /// </summary>
    public interface ICachePolicy
    {
        IDictionary<Cell, Value> GetDictionary<Value>(IGrid grid);
    }

    public static class CachePolicy
    {
        /// <summary>
        /// The default policy, caches items indefinitely.
        /// </summary>
        public static ICachePolicy Always => new AlwaysCachePolicy();


        /// <summary>
        /// Caches items indefinitely, threadsafe.
        /// </summary>
        public static ICachePolicy ConcurrentAlways => new ConcurrentAlwaysCachePolicy();
    }

    internal class AlwaysCachePolicy : ICachePolicy
    {
        public IDictionary<Cell, Value> GetDictionary<Value>(IGrid grid)
        {
            return new Dictionary<Cell, Value>();
        }
    }

    internal class ConcurrentAlwaysCachePolicy : ICachePolicy
    {
        public IDictionary<Cell, Value> GetDictionary<Value>(IGrid grid)
        {
            return new ConcurrentDictionary<Cell, Value>();
        }
    }
}
