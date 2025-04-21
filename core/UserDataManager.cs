using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using cfEngine.IO;
using cfEngine.Logging;
using cfEngine.Serialize;
using System.Text.Json.Nodes;
using cfEngine.Service;

namespace cfEngine.Core
{
    public partial class UserDataKey
    {
        public const string SaveVersion = "SaveVersion";
    }
    
    public static partial class ServiceName
    {
        public const string UserDataManager = "UserDataManager";
    }
    
    public static partial class GameExtension
    {
        public static Game WithUserData(this Game game, UserDataManager service)
        {
            game.Register(service, ServiceName.UserDataManager);
            return game;
        }
        
        public static UserDataManager GetUserData(this Game game) => game.GetService<UserDataManager>(ServiceName.UserDataManager);
    }
}

namespace cfEngine.Core
{
    public interface IRuntimeSavable
    {
        public void Initialize(IReadOnlyDictionary<string, JsonObject> dataMap);
        public void SetSaveData(Dictionary<string, object> dataMap);
    }
    
    public class UserDataManager: IService
    {
        private readonly IStorage _storage;
        private readonly ISerializer _serializer;

        private readonly List<IRuntimeSavable> _savables = new();

        private const string dataFileName = "data";
        private const string backupFileName = dataFileName + ".backup";
        
        private IReadOnlyDictionary<string, JsonObject> _cachedDataMap;

        public UserDataManager(IStorage storage, ISerializer serializer)
        {
            _storage = storage;
            _serializer = serializer;
        }

        public void Register(IRuntimeSavable savable)
        {
            _savables.Add(savable);
        }
        
        public void RegisterAndInitialize(IRuntimeSavable savable)
        {
            Register(savable);
            savable.Initialize(_cachedDataMap);
        }

        public async Task<IReadOnlyDictionary<string, JsonObject>> LoadDataMap(CancellationToken token = default)
        {
            try
            {
                if (!_storage.IsFileExist(dataFileName))
                {
                    return new Dictionary<string, JsonObject>();
                }
                
                var userDataBytes = await _storage.LoadBytesAsync(dataFileName, token);
                _cachedDataMap = await _serializer.DeserializeAsAsync<Dictionary<string, JsonObject>>(userDataBytes, token: token);
                return _cachedDataMap;
            }
            catch (Exception ex)
            {
                Log.LogException(ex, "Exception occurs saving, Loading cancelled.");
                return new Dictionary<string, JsonObject>();
            }
        }

        public async Task SaveAsync(CancellationToken token = default)
        {
            Dictionary<string, object> dataMap = new();

            try
            {
                foreach (var savable in _savables)
                {
                    savable.SetSaveData(dataMap);
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

        public void InitializeSavables(IReadOnlyDictionary<string, JsonObject> dataMap)
        {
            foreach (var savable in _savables)
            {
                savable.Initialize(dataMap);
            }
        }

        public void Dispose()
        {
            _storage?.Dispose();
        }
    }
}