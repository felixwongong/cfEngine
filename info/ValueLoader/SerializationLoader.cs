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
    public class SerializationLoader<TInfo> : IValueLoader<TInfo>
    {
        private readonly IStorage _storage;
        private readonly ISerializer _serializer;
        
        public SerializationLoader(IStorage storage, ISerializer serializer)
        {
            _storage = storage;
            _serializer = serializer;
        }
        
        public ListPool<TInfo>.Handle Load(out List<TInfo> values)
        {
            var files = _storage.GetFiles("*");
            if (files.Length <= 0)
            {
                Log.LogWarning($"serialized file not found in loader of type: {typeof(TInfo).Name}, please check the file name and path.");
                values = null;
                return ListPool<TInfo>.Handle.Empty;
            }

            var handle = ListPool<TInfo>.Default.Get(out values);
            foreach (var file in files)
            {
                var byteLoaded = _storage.LoadBytes(file);
                values.AddRange(_serializer.DeserializeAs<List<TInfo>>(byteLoaded));
            }
            
            return handle;
        }

        public Task<List<TInfo>> LoadAsync(CancellationToken cancellationToken)
        {
            var files = _storage.GetFiles("*.json");
            if (files.Length <= 0)
            {
                Log.LogWarning($"serialized file not found in loader of type: {typeof(TInfo).Name}, please check the file name and path.");
                return Task.FromResult<List<TInfo>>(new List<TInfo>(0));
            }

            using var handle = ListPool<Task<byte[]>>.Default.Get(out var byteLoadTasks);
            byteLoadTasks.EnsureCapacity(files.Length);
            
            foreach (var file in files)
            {
                byteLoadTasks.Add(_storage.LoadBytesAsync(file, cancellationToken));
            }

            var byteLoadResult = Task.WhenAll(byteLoadTasks);
            
            return byteLoadResult.ContinueWith(task =>
            {
                var values = new List<TInfo>(task.Result.Length);
                foreach (var bytes in task.Result)
                {
                    values.AddRange(_serializer.DeserializeAs<List<TInfo>>(bytes));
                }

                return values;
            }, cancellationToken);
        }
    }
}