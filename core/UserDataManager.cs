using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using cfEngine.IO;
using cfEngine.Logging;
using cfEngine.Serialize;
using System.Text.Json.Nodes;

namespace cfEngine.Core
{
    public interface IRuntimeSavable
    {
        public void Initialize(IReadOnlyDictionary<string, JsonObject> dataMap);
        public void Save(Dictionary<string, object> dataMap);
    }

    public partial class UserDataKey
    {
        public const string SaveVersion = "SaveVersion";
    }
    
    public partial class UserDataManager
    {
        private readonly Storage _storage;
        private readonly Serializer _serializer;

        private readonly List<IRuntimeSavable> _savables = new();

        private const string dataFileName = "data";
        private const string backupFileName = dataFileName + ".backup";

        public UserDataManager(Storage storage, Serializer serializer)
        {
            _storage = storage;
            _serializer = serializer;
        }

        public void Register(IRuntimeSavable savable)
        {
            _savables.Add(savable);
        }

        public async Task LoadInitializeAsync(CancellationToken token = default)
        {
            try
            {
                if (!_storage.IsFileExist(dataFileName))
                {
                    return;
                }
                
                var userDataBytes = await _storage.LoadBytesAsync(string.Empty, dataFileName, token);
                var dataMap =
                    await _serializer.DeserializeAsAsync<Dictionary<string, JsonObject>>(userDataBytes, token: token);

                foreach (var savable in _savables)
                {
                    savable.Initialize(dataMap);
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex, "Exception occurs saving, Loading cancelled.");
            }
        }

        public async Task SaveAsync(CancellationToken token = default)
        {
            Dictionary<string, object> dataMap = new();

            try
            {
                foreach (var savable in _savables)
                {
                    savable.Save(dataMap);
                }

                if (_storage.IsFileExist(dataFileName))
                {
                    _storage.CopyFile(dataFileName, backupFileName);
                }

                var data = await _serializer.SerializeAsync(dataMap, token: token);
                await _storage.SaveAsync(dataFileName, data, token);

                _storage.DeleteFile(backupFileName);
            }
            catch (Exception ex)
            {
                Log.LogException(ex, "Exception occurs saving, Saving cancelled.");
                return;
            }
        }
    }
}