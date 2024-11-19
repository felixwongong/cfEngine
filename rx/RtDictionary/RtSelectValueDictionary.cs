using System;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Rt
{
    public class RtSelectValueDictionary<TKey, TOrigValue, TValue>: RtMutatedDictionaryBase<TKey, TOrigValue, TKey, TValue>
    {
        private readonly Func<TOrigValue,TValue> _selectFn;
        private readonly Dictionary<TKey, TValue> _selected = new();

        public RtSelectValueDictionary(RtReadOnlyDictionary<TKey, TOrigValue> source, Func<TOrigValue, TValue> selectFn): base(source.Events)
        {
            _selectFn = selectFn;
            _selected.EnsureCapacity(source.Count);
            foreach (var (key, origValue) in source)
            {
                _selected[key] = _selectFn(origValue);
            }
        }

        #region OnSourceCollectionUpdate

        protected override void OnSourceUpdate(KeyValuePair<TKey, TOrigValue> oldPair, KeyValuePair<TKey, TOrigValue> newPair)
        {
            var key = oldPair.Key;
            var oldValue = oldPair.Value;
            var newValue = newPair.Value;
            
            if (!_selected.TryGetValue(key, out var oldSelected))
            {
                Log.LogException(new ArgumentException($"Invalid argument ({key}, {oldValue.ToString()}, {newValue.ToString()}), cannot update"), nameof(OnSourceUpdate));
                return;
            }

            var newSelected = _selectFn(newValue);
            _selected[key] = newSelected; 
            CollectionEvents.OnUpdateRelay.Dispatch(
                new KeyValuePair<TKey, TValue>(key, oldSelected),
                new KeyValuePair<TKey, TValue>(key, newSelected)
                );
        }

        protected override void OnSourceRemove(KeyValuePair<TKey, TOrigValue> kvp)
        {
            var (key, value) = kvp;
            if (!_selected.TryGetValue(key, out var selectedValue))
            {
                Log.LogException(new ArgumentException($"Invalid argument ({key.ToString()}, {value.ToString()}), cannot remove"), nameof(OnSourceRemove));
                return;
            }

            _selected.Remove(key);
            CollectionEvents.OnRemoveRelay.Dispatch(new (key, selectedValue));
        }

        protected override void OnSourceAdd(KeyValuePair<TKey, TOrigValue> kvp)
        {
            var (key, value) = kvp;
            var selectedValue = _selectFn(value);

            if (!_selected.TryAdd(key, selectedValue))
            {
                Log.LogException(new ArgumentException($"Invalid argument ({key.ToString()}, {selectedValue.ToString()}), cannot add"), nameof(OnSourceAdd));
                return;
            }
            
            CollectionEvents.OnAddRelay.Dispatch(new KeyValuePair<TKey, TValue>(key, selectedValue));
        }


        #endregion
        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _selected.GetEnumerator();
        }

        public override int Count => _selected.Count;
        public override bool ContainsKey(TKey key)
        {
            return _selected.ContainsKey(key);
        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            return _selected.TryGetValue(key, out value);
        }

        public override TValue this[TKey key] => _selected[key];

        public override IEnumerable<TKey> Keys => _selected.Keys;
        public override IEnumerable<TValue> Values => _selected.Values;

        public override void Dispose()
        {
            base.Dispose();
            _selected.Clear();
        }
    }
}