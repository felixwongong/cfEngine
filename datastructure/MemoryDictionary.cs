using System;
using System.Collections.Generic;

namespace cfEngine.DataStructure
{
    public class ReadOnlyMemoryComparer<T> : IEqualityComparer<ReadOnlyMemory<T>>
    {
        private static ReadOnlyMemoryComparer<T> _instance;
        public static ReadOnlyMemoryComparer<T> Instance => _instance ??= new ReadOnlyMemoryComparer<T>();

        public bool Equals(ReadOnlyMemory<T> x, ReadOnlyMemory<T> y) => x.Span.SequenceEqual(y.Span);

        public int GetHashCode(ReadOnlyMemory<T> obj)
        {
            var hashCode = new HashCode();
            foreach (var item in obj.Span)
            {
                hashCode.Add(item);
            }

            return hashCode.ToHashCode();
        }
    }

    
    public class MemoryDictionary<TMemoryKey, TValue> : Dictionary<ReadOnlyMemory<TMemoryKey>, TValue>
    {
        //Add object pooling
        public static MemoryDictionary<TMemoryKey, TValue> Create() => new MemoryDictionary<TMemoryKey, TValue>();
        
        public MemoryDictionary() : base(ReadOnlyMemoryComparer<TMemoryKey>.Instance)
        {
        }
    }
}
