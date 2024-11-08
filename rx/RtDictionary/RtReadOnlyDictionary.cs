using System;
using System.Collections;
using System.Collections.Generic;
using cfEngine.Util;

namespace cfEngine.Rx
{
    public abstract class RtReadOnlyDictionary<TKey, TValue>: IReadOnlyDictionary<TKey, TValue>, IDisposable
    {
        #region Rx Events
        protected Relay<(TKey key, TValue value)> OnAddRelay;
        protected Relay<(TKey key, TValue value)> OnRemoveRelay;
        protected Relay<(TKey key, TValue oldValue, TValue newValue)> OnUpdateRelay;

        public event Action<(TKey key, TValue value)> OnAdd
        {
            add => OnAddRelay.AddListener(value);
            remove => OnAddRelay.RemoveListener(value);
        }
        
        public event Action<(TKey key, TValue value)> OnRemove 
        {
            add => OnRemoveRelay.AddListener(value);
            remove => OnRemoveRelay.RemoveListener(value);
        }
        
        public event Action<(TKey key, TValue oldValue, TValue newValue)> OnUpdate
        {
            add => OnUpdateRelay.AddListener(value);
            remove => OnUpdateRelay.AddListener(value);
        } 
        #endregion

        #region abstract IDictionary Implementation 


        #endregion

        public virtual void Dispose()
        {
            OnRemoveRelay.RemoveAll();
            OnAddRelay.RemoveAll();
            OnUpdateRelay.RemoveAll();
        }

        public abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract int Count { get; }
        public abstract bool ContainsKey(TKey key);

        public abstract bool TryGetValue(TKey key, out TValue value);

        public abstract TValue this[TKey key] { get; }

        public abstract IEnumerable<TKey> Keys { get; }
        public abstract IEnumerable<TValue> Values { get; }
    }
}