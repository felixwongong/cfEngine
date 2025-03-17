using System;
using System.Collections.Generic;

namespace cfEngine.DataStructure
{
    public class ReadOnlyMemoryCharComparer<T> : IEqualityComparer<ReadOnlyMemory<T>>
    {
        private static ReadOnlyMemoryCharComparer<T> _instance;
        public static ReadOnlyMemoryCharComparer<T> Instance => _instance ??= new ReadOnlyMemoryCharComparer<T>();

        public bool Equals(ReadOnlyMemory<T> x, ReadOnlyMemory<T> y) => x.Span == y.Span;

        public int GetHashCode(ReadOnlyMemory<T> obj) => obj.GetHashCode();
    }

    
    public class MemoryDictionary<TMemoryKey, TValue> : Dictionary<ReadOnlyMemory<TMemoryKey>, TValue>
    {
        //Add object pooling
        public static MemoryDictionary<TMemoryKey, TValue> Create() => new MemoryDictionary<TMemoryKey, TValue>();
        
        public MemoryDictionary() : base(ReadOnlyMemoryCharComparer<TMemoryKey>.Instance)
        {
        }
    }
}