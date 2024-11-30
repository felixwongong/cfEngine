using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Rt
{
    public class RtObserverList<T>: RtReadOnlyList<T>
    {
        private readonly List<T> _list;
        private readonly ICollectionEvents<T> _sourceEvents;

        public RtObserverList(IEnumerable<T> sourceItems, ICollectionEvents<T> sourceEvents)
        {
            _list = new List<T>(sourceItems);
            _sourceEvents = sourceEvents;
            
            sourceEvents.Subscribe(OnSourceAdd, OnSourceRemove, OnSourceUpdate, Dispose);
        }

        private void OnSourceUpdate(T oldItem, T newItem)
        {
            var index = _list.IndexOf(oldItem);
            
            if (index == -1)
            {
                Log.LogError($"RtObserverList.OnSourceUpdate: oldItem {oldItem.ToString()} not found in list");
                return;
            }
            
            _list[index] = newItem;
            CollectionEvents.OnUpdateRelay.Dispatch((index, oldItem), (index, newItem));
        }

        private void OnSourceRemove(T item)
        {
            var index = _list.IndexOf(item);
            if (index == -1)
            {
                Log.LogError($"RtObserverList.OnSourceRemove: item {item.ToString()} not found in list");
                return;
            }
            
            CollectionEvents.OnRemoveRelay.Dispatch((index, item));
        }

        private void OnSourceAdd(T item)
        {
            _list.Add(item);
            CollectionEvents.OnAddRelay.Dispatch((_list.Count - 1, item));
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public override int Count => _list.Count;

        public override T this[int index] => _list[index];

        public override void Dispose()
        {
            base.Dispose();
            
            _sourceEvents.Unsubscribe(OnSourceAdd, OnSourceRemove, OnSourceUpdate, Dispose);
            
            _list.Clear();
        }
    }
}