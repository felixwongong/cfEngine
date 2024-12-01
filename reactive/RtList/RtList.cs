using System;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Rt
{
    public class RtList<T>: RtReadOnlyList<T>
    {
        private readonly List<T> _list;

        public RtList() : base()
        {
            _list = new List<T>();
        }

        public RtList(int capacity): base()
        {
            _list = new List<T>(capacity);
        }

        public RtList(IEnumerable<T> defaultItems)
        {
            _list = new List<T>();
            _list.AddRange(defaultItems);
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public void Add(T item)
        {
            _list.Add(item);
            
            CollectionEvents.OnAddRelay.Dispatch((_list.Count - 1, item));
        }

        public void Clear()
        {
            for (var i = _list.Count - 1; i >= 0; i--)
            {
                var item = _list[i];
                _list.RemoveAt(i);
                
                CollectionEvents.OnRemoveRelay.Dispatch((i, item));
            }
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            var index = _list.IndexOf(item);
            if (index == -1)
            {
                Log.LogException(new ArgumentException($"RtList item not found: {item?.ToString()}"));
                return false;
            }
            
            _list.RemoveAt(index);
            CollectionEvents.OnRemoveRelay.Dispatch((index, item));
            return true;
        }

        public override int Count => _list.Count;
        public bool IsReadOnly => false;
        public int IndexOf(T item)
        {
            var index = -1;
            for (var i = 0; i < _list.Count; i++)
            {
                if (_list[i].Equals(item))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public void Update(int index, T item)
        {
            var existing = _list[index];
            _list[index] = item;
            
            CollectionEvents.OnUpdateRelay.Dispatch(
                (index, existing),
                (index, item)
                );
        }

        public override T this[int index] => _list[index];

        public override void Dispose()
        {
            base.Dispose();
            _list.Clear();
        }
    }
}