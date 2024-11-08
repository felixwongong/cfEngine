using System;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Rx
{
    public static class RtSelectKeyDictionaryExtension
    {
        public static RtSelectKeyDictionary<TOrigKey, TSelectedKey, TValue> SelectKey<TOrigKey, TSelectedKey, TValue>(
            this RtReadOnlyDictionary<TOrigKey, TValue> source, Func<TOrigKey, TSelectedKey> selectFn)
        {
            return new RtSelectKeyDictionary<TOrigKey, TSelectedKey, TValue>(source, selectFn);
        }
    }
    
    public class RtSelectKeyDictionary<TOrigKey, TSelectKey, TValue>: RtReadOnlyDictionary<TSelectKey, TValue>
    {
        private readonly RtReadOnlyDictionary<TOrigKey, TValue> _source;
        private readonly Func<TOrigKey, TSelectKey> _selectFn;

        private readonly Dictionary<TSelectKey, TValue> _selected = new();

        public RtSelectKeyDictionary(RtReadOnlyDictionary<TOrigKey, TValue> source, Func<TOrigKey, TSelectKey> selectFn)
        {
            _source = source;
            _selectFn = selectFn;
            
            _source.OnAdd += OnSourceAdd;
            _source.OnRemove += OnSourceRemove;
            _source.OnUpdate += OnSourceUpdate;
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
        
        private void OnSourceUpdate((TOrigKey key, TValue oldValue, TValue newValue) kvp)
        {
            var (key, oldValue, newValue) = kvp;
            var selectedKey = _selectFn(key);
            
            if (_selected.TryGetValue(selectedKey, out var v) && v.Equals(oldValue))
            {
                _selected[selectedKey] = newValue;
                OnUpdateRelay.Dispatch((selectedKey, v, newValue));
            }
            else
            {
                Log.LogException(new ArgumentException($"Invalid argument ({selectedKey.ToString()}, {v}, {newValue}), cannot update"), nameof(kvp));
            }
        }

        private void OnSourceRemove((TOrigKey key, TValue value) kvp)
        {
            var (key, value) = kvp;
            var selectedKey = _selectFn(key);
            
            if (_selected.TryGetValue(selectedKey, out var v) && v.Equals(value))
            {
                _selected.Remove(selectedKey);
                OnRemoveRelay.Dispatch((selectedKey, v));
            }
            else
            {
                Log.LogException(new ArgumentException($"Invalid argument ({selectedKey.ToString()}, {value.ToString()}), cannot remove"), nameof(kvp));
            }
        }

        private void OnSourceAdd((TOrigKey key, TValue value) kvp)
        {
            var (key, value) = kvp;
            var selectedKey = _selectFn(key);
            if (_selected.TryAdd(selectedKey, value))
            {
                OnAddRelay.Dispatch((selectedKey, value));
            }
            else
            {
                Log.LogException(new ArgumentException($"Invalid argument ({selectedKey.ToString()}, {value.ToString()}), cannot add"), nameof(kvp));
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            
            _source.OnAdd -= OnSourceAdd;
            _source.OnRemove -= OnSourceRemove;
            _source.OnUpdate -= OnSourceUpdate;
        }
    }
}