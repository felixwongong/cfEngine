using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using cfEngine;

namespace cfEngine.Info
{
    public abstract class ConfigInfoManager<TKey, TInfo> : InfoManager where TKey : notnull where TInfo: class
    {
        #region Get Values 

        /// <summary>
        /// Serializes every writer (initial load, reload, revert, <see cref="AddValue"/> and
        /// <see cref="Dispose"/>). Held across parsing/recache, released before hooks/events.
        /// </summary>
        private readonly SemaphoreSlim _writeGate = new(1, 1);

        /// <summary>
        /// Published snapshot. Replaced as a whole (<see cref="Interlocked.Exchange{T}"/>) so a
        /// reader can never observe a partially built table; always read it through
        /// <see cref="valueMap"/>. Immutable by convention: never mutate a published dictionary.
        /// </summary>
        private IReadOnlyDictionary<TKey, TInfo> _valueMap = new Dictionary<TKey, TInfo>();

        public IReadOnlyDictionary<TKey, TInfo> valueMap => Volatile.Read(ref _valueMap);
        public override IEnumerable<object> GetAllValue() => valueMap.Values;
        
        #endregion

        protected abstract Func<TInfo, TKey> keyFn { get; }
        public override Type infoType => typeof(TInfo);
        
        private readonly IValueLoader<TInfo> _loader;

        public override event Action? Reloaded;

        protected ConfigInfoManager(IValueLoader<TInfo> loader) : base()
        {
            _loader = loader;
        }
        
        public override void LoadInfo()
        {
            bool published;
            _writeGate.Wait();
            try
            {
                published = TryBuildCandidate(out var candidate);
                if (published)
                {
                    Publish(candidate);
                    // Boot-path observability retained from the pre-slice loader: report the row
                    // count actually published. Explicit Reload/Revert log their own line.
                    Log.LogInfo($"{typeof(TInfo).Name} infoCount: {candidate.Count}");
                }
            }
            finally
            {
                _writeGate.Release();
            }

            if (published)
            {
                OnLoadCompleted();
            }
        }

        public override async Task LoadInfoAsync(CancellationToken cancellationToken)
        {
            await _writeGate.WaitAsync(cancellationToken).ConfigureAwait(false);

            Dictionary<TKey, TInfo>? candidate;
            try
            {
                candidate = await BuildCandidateAsync(cancellationToken).ConfigureAwait(false);
                if (candidate != null)
                {
                    Publish(candidate);
                    // Boot-path observability retained from the pre-slice loader (see LoadInfo).
                    Log.LogInfo($"{typeof(TInfo).Name} infoCount: {candidate.Count}");
                }
            }
            catch (Exception e)
            {
                Log.LogException(e);
                throw;
            }
            finally
            {
                _writeGate.Release();
            }

            if (candidate != null)
            {
                OnLoadCompleted();
            }
        }

        public override bool Reload()
        {
            bool published;
            var count = 0;

            _writeGate.Wait();
            try
            {
                published = TryBuildCandidate(out var candidate);
                if (published)
                {
                    count = candidate.Count;
                    Publish(candidate);
                }
            }
            catch (Exception e)
            {
                Log.LogException(e);
                published = false;
            }
            finally
            {
                _writeGate.Release();
            }

            if (!published)
            {
                Log.LogError($"{typeof(TInfo).Name} reload failed, keeping previous infoCount: {valueMap.Count}");
                return false;
            }

            Log.LogInfo($"{typeof(TInfo).Name} reloaded infoCount: {count}");
            RaiseReloaded();
            return true;
        }

        public override bool Revert()
        {
            if (_loader is not IRevertibleValueLoader revertible)
            {
                Log.LogError($"{GetType().Name}: revert is not supported by loader {_loader?.GetType().Name ?? "null"}");
                return false;
            }

            bool published;
            var count = 0;

            _writeGate.Wait();
            try
            {
                // Recache first: it refreshes the files this manager reads, then the normal
                // all-or-nothing publication decides whether the refresh is accepted.
                revertible.Recache();

                published = TryBuildCandidate(out var candidate);
                if (published)
                {
                    count = candidate.Count;
                    Publish(candidate);
                }
            }
            catch (Exception e)
            {
                Log.LogException(e);
                published = false;
            }
            finally
            {
                _writeGate.Release();
            }

            if (!published)
            {
                Log.LogError($"{typeof(TInfo).Name} revert failed, keeping previous infoCount: {valueMap.Count}");
                return false;
            }

            Log.LogInfo($"{typeof(TInfo).Name} reverted infoCount: {count}");
            RaiseReloaded();
            return true;
        }

        /// <summary>
        /// Builds a complete candidate table. Returns false (and leaves the published snapshot
        /// untouched) for any invalid input: null loader list, null row, null/duplicate key.
        /// Loader exceptions propagate to the caller.
        /// </summary>
        private bool TryBuildCandidate(out Dictionary<TKey, TInfo> candidate)
        {
            candidate = null!;

            if (!HasKeyFn())
            {
                return false;
            }

            using var handle = _loader.Load(out var values);
            return TryMapValues(values, out candidate);
        }

        private async Task<Dictionary<TKey, TInfo>?> BuildCandidateAsync(CancellationToken cancellationToken)
        {
            if (!HasKeyFn())
            {
                return null;
            }

            var values = await _loader.LoadAsync(cancellationToken).ConfigureAwait(false);
            return TryMapValues(values, out var candidate) ? candidate : null;
        }

        private bool HasKeyFn()
        {
            if (keyFn == null)
            {
                Log.LogError($"keyFn is null on {GetType().Name}, load fail");
                return false;
            }

            return true;
        }

        private bool TryMapValues(List<TInfo> values, out Dictionary<TKey, TInfo> candidate)
        {
            candidate = null!;
            if (values == null)
            {
                Log.LogError($"no serialized data for {typeof(TInfo).Name} on {GetType().Name}, load fail");
                return false;
            }

            var map = new Dictionary<TKey, TInfo>(values.Count);
            foreach (var value in values)
            {
                if (value == null)
                {
                    Log.LogError($"Null row in {GetType().Name} for {typeof(TInfo).Name}, load fail");
                    return false;
                }

                var key = keyFn(value);
                if (key is null)
                {
                    Log.LogError($"Null key in {GetType().Name} for {typeof(TInfo).Name}, load fail");
                    return false;
                }

                if (!map.TryAdd(key, value))
                {
                    Log.LogError($"Duplicate key {key} in {GetType().Name}, load fail");
                    return false;
                }
            }

            candidate = map;
            return true;
        }

        private void Publish(Dictionary<TKey, TInfo> map) => Interlocked.Exchange(ref _valueMap, map);

        private void RaiseReloaded()
        {
            var handler = Reloaded;
            if (handler == null)
            {
                return;
            }

            // Each subscriber is isolated: one failing listener must not stop the others and
            // must never retroactively mark the already-published transaction as failed.
            foreach (var subscriber in handler.GetInvocationList())
            {
                try
                {
                    ((Action)subscriber)();
                }
                catch (Exception e)
                {
                    Log.LogException(e, $"{typeof(TInfo).Name} Reloaded subscriber failed");
                }
            }
        }
        
        public void AddValue(TInfo value)
        {
            if (value == null)
            {
                Log.LogError($"Value is null in {GetType().Name} for {typeof(TInfo).Name}");
                return;
            }

            _writeGate.Wait();
            try
            {
                if (!HasKeyFn())
                {
                    return;
                }

                var key = keyFn(value);
                if (key is null)
                {
                    Log.LogError($"Null key in {GetType().Name}, AddValue fail");
                    return;
                }

                var current = Volatile.Read(ref _valueMap);
                if (current.ContainsKey(key))
                {
                    Log.LogError($"Duplicate key {key} in {GetType().Name}");
                    return;
                }

                // Copy-on-write: the previously published snapshot stays valid for held references.
                Publish(new Dictionary<TKey, TInfo>(current) { { key, value } });
            }
            finally
            {
                _writeGate.Release();
            }
        }
        
        public bool TryGetValue(TKey key, out TInfo? value)
        {
            return valueMap.TryGetValue(key, out value);
        }

        public Res<TInfo, Exception> GetValue(TKey key)
        {
            if (!valueMap.TryGetValue(key, out var value))
            {
                return Res.Err<TInfo>(new KeyNotFoundException($"Info [{typeof(TInfo).Name}]  not found: {key}"));
            }

            return value;
        }

        public override void Dispose()
        {
            base.Dispose();

            // Replace rather than clear: readers holding the previous snapshot keep their data.
            _writeGate.Wait();
            try
            {
                Publish(new Dictionary<TKey, TInfo>());
            }
            finally
            {
                _writeGate.Release();
            }

            if (_loader is IDisposable disposableLoader)
            {
                disposableLoader.Dispose();
            }

            // _writeGate is intentionally never disposed: a concurrent writer may still be
            // waiting on it, and disposing it would fault that writer.
        }
    }
}
