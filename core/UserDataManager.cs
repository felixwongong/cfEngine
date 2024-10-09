using System.Collections.Generic;
using cfEngine.IO;
using cfEngine.Serialize;

namespace cfEngine.Core
{
    public interface IRuntimeSavable
    {
        public void Initialize(IReadOnlyDictionary<string, object> dataMap);
        public void Save(Dictionary<string, object> dataMap);
    }

    public partial class UserDataKey
    {
        public const string SaveVersion = "SaveVersion";
    }
    
    public class UserDataManager
    {
        private readonly Storage _datastore;
        private readonly Serializer _serializer;

        private readonly List<IRuntimeSavable> _savables = new();

        public UserDataManager(Storage datastore, Serializer serializer)
        {
            _datastore = datastore;
            _serializer = serializer;
        }

        public void Register(IRuntimeSavable savable)
        {
            _savables.Add(savable);
        }

        public void Save()
        {
            Dictionary<string, object> dataMap = new();
            
            foreach (var savable in _savables)
            {
                savable.Save(dataMap);
            }
        }
    }
}