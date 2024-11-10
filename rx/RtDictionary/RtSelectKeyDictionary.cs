using System;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Rt
{
    public class RtSelectKeyDictionary<TOrigKey, TSelectKey, TValue>: RtReadOnlyDictionary<TSelectKey, TValue>
    {
        private readonly ICollectionEvents<KeyValuePair<TOrigKey, TValue>> _sourceEvents;
        private readonly Func<TOrigKey, TSelectKey> _selectFn;

        private readonly Dictionary<TSelectKey, TValue> _selected = new();

        public RtSelectKeyDictionary(RtReadOnlyDictionary<TOrigKey, TValue> source, Func<TOrigKey, TSelectKey> selectFn)
        {
            _sourceEvents = source.Events;
            _selectFn = selectFn;

            _selected.EnsureCapacity(source.Count);
            foreach (var (origKey, value) in source)
            {
                _selected[selectFn(origKey)] = value;
            }
            
            _sourceEvents.Subscribe(OnSourceAdd, OnSourceRemove, OnSourceUpdate, Dispose);
        }
        public override IEnumerator<KeyValuePair<TSelectKey, TValue>> GetEnumerator()
        {
            return _selected.GetEnumerator();
        }

        public override int Count => _selected.Count;
        public override bool ContainsKey(TSelectKey key)
        {
            return _selected.ContainsKey(key);
        }

        public override bool TryGetValue(TSelectKey key, out TValue value)
        {
            return _selected.TryGetValue(key, out value);
        }

        public override TValue this[TSelectKey key] => _selected[key];

        public override IEnumerable<TSelectKey> Keys => _selected.Keys;
        public override IEnumerable<TValue> Values => _selected.Values;
        
        private void OnSourceUpdate(KeyValuePair<TOrigKey, TValue> oldPair, KeyValuePair<TOrigKey, TValue> newPair)
        {
            var key = oldPair.Key;
            var oldValue = oldPair.Value;
            var newValue = newPair.Value;
            var selectedKey = _selectFn(key);
            
            if (_selected.TryGetValue(selectedKey, out var v) && v.Equals(oldValue))
            {
                _selected[selectedKey] = newValue;
                CollectionEvents.OnUpdateRelay.Dispatch(
                    new KeyValuePair<TSelectKey, TValue>(selectedKey, v),
                    new KeyValuePair<TSelectKey, TValue>(selectedKey, newValue)
                    );
            }
            else
            {
                Log.LogException(new ArgumentException($"Invalid argument ({selectedKey.ToString()}, {v}, {newValue}), cannot update"), nameof(OnSourceUpdate));
            }
        }

        private void OnSourceRemove(KeyValuePair<TOrigKey, TValue> kvp)
        {
            var (key, value) = kvp;
            var selectedKey = _selectFn(key);
            
            if (_selected.TryGetValue(selectedKey, out var v) && v.Equals(value))
            {
                _selected.Remove(selectedKey);
                CollectionEvents.OnRemoveRelay.Dispatch(new KeyValuePair<TSelectKey, TValue>(selectedKey, v));
            }
            else
            {
                Log.LogException(new ArgumentException($"Invalid argument ({selectedKey.ToString()}, {value.ToString()}), cannot remove"), nameof(OnSourceRemove));
            }
        }

        private void OnSourceAdd(KeyValuePair<TOrigKey, TValue> kvp)
        {
            var (key, value) = kvp;
            var selectedKey = _selectFn(key);
            if (_selected.TryAdd(selectedKey, value))
            {
                CollectionEvents.OnAddRelay.Dispatch(new KeyValuePair<TSelectKey, TValue>(selectedKey, value));
            }
            else
            {
                Log.LogException(new ArgumentException($"Invalid argument ({selectedKey.ToString()}, {value.ToString()}), cannot add"), nameof(OnSourceAdd));
            }
        }

        public override void Dispose()
        {
            _selected.Clear();
            CollectionEvents.OnDisposeRelay.Dispatch();
            
            _sourceEvents.Unsubscribe(OnSourceAdd, OnSourceRemove, OnSourceUpdate, Dispose);
            
            base.Dispose();
        }
    }
}