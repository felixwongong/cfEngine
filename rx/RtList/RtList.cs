using System;
using System.Collections.Generic;

namespace cfEngine.Rx
{
    public class RtList<T>: RtReadOnlyList<T>
    {
        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }
        
        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public override IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override int Count { get; }

        public override T this[int index] => throw new NotImplementedException();
    }
}