using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using cfEngine.Logging;

namespace cfEngine.Info
{
    public abstract class ConfigInfoManager<TKey, TInfo> : InfoManager where TKey : notnull where TInfo: class
    {
        private readonly IValueLoader<TInfo> _loader;

        protected readonly Dictionary<TKey, TInfo> _valueMap = new();
        public IReadOnlyDictionary<TKey, TInfo> ValueMap => _valueMap;
        public override IEnumerable<object> GetAllValue() => ValueMap.Values;

        private List<TInfo> _allValues;
        public IReadOnlyList<TInfo> allValues => _allValues ??= _valueMap.Values.ToList();

        protected abstract Func<TInfo, TKey> keyFn { get; }

        protected ConfigInfoManager(IValueLoader<TInfo> loader) : base()
        {
            _loader = loader;
        }

        public override void LoadInfo()
        {
            using var handle = _loader.Load(out var values);
            _valueMap.EnsureCapacity(values.Count);
            
            foreach (var value in values)
            {
                var key = keyFn(value);
                if (!_valueMap.TryAdd(key, value))
                {
                    Log.LogError($"Duplicate key {key} in {infoDirectory}");
                    continue;
                }
            }

            OnLoadCompleted();
        }

        public override async Task LoadInfoAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(infoDirectory))
            {
                throw new ArgumentNullException(nameof(infoDirectory), "info key is unset");
            }

            var values = await _loader.LoadAsync(cancellationToken);
            
            _valueMap.EnsureCapacity(values.Count);
            foreach (var value in values)
            {
                var key = keyFn(value);
                if (_valueMap.ContainsKey(key))
                {
                    Log.LogError($"Duplicate key {key} in {infoDirectory}");
                    continue;
                }

                _valueMap.Add(key, value);
            }
            
            OnLoadCompleted();
        }

        public override void Dispose()
        {
            base.Dispose();
            _valueMap.Clear();
        }
    }
}