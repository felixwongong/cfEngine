using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using cfEngine.Extension;
using cfEngine.IO;
using cfEngine;
using cfEngine.Pooling;
using cfEngine.Serialize;

namespace cfEngine.Info
{
    public class SerializationLoader<TInfo> : IValueLoader<TInfo>, IRevertibleValueLoader
    {
        private readonly IStorage _storage;
        private readonly ISerializer _serializer;
        private readonly Action? _recache;

        public SerializationLoader(IStorage storage, ISerializer serializer)
            : this(storage, serializer, null)
        {
        }

        public SerializationLoader(IStorage storage, ISerializer serializer, Action? recache)
        {
            _storage = storage;
            _serializer = serializer;
            _recache = recache;
        }

        public void Recache()
        {
            if (_recache == null)
            {
                throw new NotSupportedException(
                    $"{typeof(SerializationLoader<TInfo>).Name} was constructed without a recache action");
            }

            _recache();
        }
        
        public ListPool<TInfo>.Handle Load(out List<TInfo> values)
        {
            var files = _storage.GetFiles("*.json");
            if (files == null || files.Length <= 0)
            {
                Log.LogWarning($"serialized file not found in loader of type: {typeof(TInfo).Name}, please check the file name and path.");
                // IValueLoader reports "no data" as a null list; the manager turns that into a
                // failed transaction instead of publishing an empty table over good data.
                values = null!;
                return ListPool<TInfo>.Handle.Empty;
            }

            var handle = ListPool<TInfo>.Default.Get(out values);
            try
            {
                foreach (var file in files)
                {
                    var byteLoaded = _storage.LoadBytes(file);
                    var rows = _serializer.DeserializeAs<List<TInfo>>(byteLoaded);
                    if (rows == null)
                    {
                        throw new InvalidOperationException(
                            $"serialized file '{file}' produced no rows for loader of type: {typeof(TInfo).Name}");
                    }

                    values.AddRange(rows);
                }
            }
            catch
            {
                // Never leak the pooled list when a later file fails: no earlier rows are returned.
                handle.Dispose();
                values = null!;
                throw;
            }
            
            return handle;
        }

        public async Task<List<TInfo>> LoadAsync(CancellationToken cancellationToken)
        {
            var files = _storage.GetFiles("*.json");
            if (files == null || files.Length <= 0)
            {
                Log.LogWarning($"serialized file not found in loader of type: {typeof(TInfo).Name}, please check the file name and path.");
                // Same "no data" contract as Load: null rather than an empty table.
                return null!;
            }

            var loadTasks = new Task<byte[]>[files.Length];
            for (var i = 0; i < files.Length; i++)
            {
                loadTasks[i] = _storage.LoadBytesAsync(files[i], cancellationToken);
            }

            var byteResults = await Task.WhenAll(loadTasks).ConfigureAwait(false);

            var values = new List<TInfo>(byteResults.Length);
            foreach (var bytes in byteResults)
            {
                var rows = _serializer.DeserializeAs<List<TInfo>>(bytes);
                if (rows == null)
                {
                    throw new InvalidOperationException(
                        $"serialized file produced no rows for loader of type: {typeof(TInfo).Name}");
                }

                values.AddRange(rows);
            }

            return values;
        }
    }
}
