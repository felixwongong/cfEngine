using System;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Rx
{
    public static partial class RtDictionaryExtension 
    {
        public static RtSelectValueDictionary<TKey, TValue, TSelectValue> SelectValue<TKey, TValue, TSelectValue>(
            this RtReadOnlyDictionary<TKey, TValue> source, Func<TValue, TSelectValue> selectFn)
        {
            return new RtSelectValueDictionary<TKey, TValue, TSelectValue>(source, selectFn);
        }
    }
    
    public class RtSelectValueDictionary<TKey, TOrigValue, TValue>: RtReadOnlyDictionary<TKey, TValue>
    {
        private readonly RtReadOnlyDictionary<TKey,TOrigValue> _source;
        private readonly Func<TOrigValue,TValue> _selectFn;

        private readonly Dictionary<TKey, TValue> _selected = new();

        public RtSelectValueDictionary(RtReadOnlyDictionary<TKey, TOrigValue> source, Func<TOrigValue, TValue> selectFn)
        {
            _source = source;
            _selectFn = selectFn;
            
            _source.OnAdd += OnSourceAdd;
            _source.OnRemove += OnSourceRemove;
            _source.OnUpdate += OnSourceUpdate;
        }

        #region OnSourceCollectionUpdate 

        private void OnSourceUpdate((TKey key, TOrigValue oldValue, TOrigValue newValue) kvp)
        {
            var (key, oldValue, newValue) = kvp;

            if (!_selected.TryGetValue(key, out var oldSelected))
            {
                Log.LogException(new ArgumentException($"Invalid argument ({key}, {oldValue.ToString()}, {newValue.ToString()}), cannot update"), nameof(OnSourceUpdate));
                return;
            }

            var newSelected = _selectFn(newValue);
            _selected[key] = newSelected; 
            OnUpdateRelay.Dispatch((key, oldSelected, newSelected));
        }

        private void OnSourceRemove((TKey key, TOrigValue value) kvp)
        {
            var (key, value) = kvp;
            if (!_selected.TryGetValue(key, out var selectedValue))
            {
                Log.LogException(new ArgumentException($"Invalid argument ({key.ToString()}, {value.ToString()}), cannot remove"), nameof(OnSourceRemove));
                return;
            }

            _selected.Remove(key);
            OnRemoveRelay.Dispatch((key, selectedValue));
        }

        private void OnSourceAdd((TKey key, TOrigValue value) kvp)
        {
            var (key, value) = kvp;
            var selectedValue = _selectFn(value);

            if (!_selected.TryAdd(key, selectedValue))
            {
                Log.LogException(new ArgumentException($"Invalid argument ({key.ToString()}, {selectedValue.ToString()}), cannot add"), nameof(OnSourceAdd));
                return;
            }
            
            OnAddRelay.Dispatch((key, selectedValue));
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
    }
}