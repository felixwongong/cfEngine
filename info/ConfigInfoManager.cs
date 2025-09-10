using cfEngine.Logging;

namespace cfEngine.Info
{
    public abstract class ConfigInfoManager<TKey, TInfo> : InfoManager where TKey : notnull where TInfo: class
    {
        #region Get Values 

        private readonly Dictionary<TKey, TInfo> _valueMap = new();
        public IReadOnlyDictionary<TKey, TInfo> valueMap => _valueMap;
        public override IEnumerable<object> GetAllValue() => valueMap.Values;
        
        #endregion

        protected abstract Func<TInfo, TKey> keyFn { get; }
        public override Type infoType => typeof(TInfo);
        
        private readonly IValueLoader<TInfo> _loader;

        protected ConfigInfoManager(IValueLoader<TInfo> loader) : base()
        {
            _loader = loader;
        }
        
        public override void LoadInfo()
        {
            if (keyFn == null)
            {
                Log.LogError($"keyFn is null on {GetType().Name}, load fail");
                return;
            }
            
            using var handle = _loader.Load(out var values);
            _valueMap.EnsureCapacity(values.Count);
            Log.LogInfo($"{typeof(TInfo).Name} infoCount: {values.Count}");
            
            foreach (var value in values)
            {
                var key = keyFn(value);
                if (!_valueMap.TryAdd(key, value))
                {
                    Log.LogError($"Duplicate key {key} in {GetType().Name}");
                    continue;
                }
            }

            OnLoadCompleted();
        }

        public override async Task LoadInfoAsync(CancellationToken cancellationToken)
        {
            List<TInfo> values = null;
            try
            {
                values = await _loader.LoadAsync(cancellationToken);
            }
            catch (Exception e)
            {
                Log.LogException(e);
                throw;
            }
            
            _valueMap.EnsureCapacity(values.Count);
            foreach (var value in values)
            {
                var key = keyFn(value);
                if (_valueMap.ContainsKey(key))
                {
                    Log.LogError($"Duplicate key {key} in {GetType().Name}");
                    continue;
                }

                _valueMap.Add(key, value);
            }
            
            OnLoadCompleted();
        }
        
        public void AddValue(TInfo value)
        {
            if (value == null)
            {
                Log.LogError($"Value is null in {GetType().Name} for {typeof(TInfo).Name}");
                return;
            }
            
            var key = keyFn(value);
            if (!_valueMap.TryAdd(key, value))
            {
                Log.LogError($"Duplicate key {key} in {GetType().Name}");
            }
        }
        
        public bool TryGetValue(TKey key, out TInfo value)
        {
            value = null;
            if (key == null)
            {
                Log.LogError($"Key is null in {GetType().Name} for {typeof(TInfo).Name}");
                return false;
            }
            return _valueMap.TryGetValue(key, out value);
        }

        public override void Dispose()
        {
            base.Dispose();
            _valueMap.Clear();
        }
    }
}