using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using cfEngine.Logging;

namespace cfEngine.Core
{
    public partial class UserDataManager
    {
        public class JsonContextMap : IUserData
        {
            public static JsonContextMap Empty = new(new Dictionary<string, JsonObject>());
            
            private readonly IReadOnlyDictionary<string, JsonObject> _dataMap;
            public JsonContextMap(IReadOnlyDictionary<string, JsonObject> dataMap)
            {
                _dataMap = dataMap;
            }
            
            public bool TryGetContext<T>(string contextKey, out T context)
            {
                context = default;
                if (_dataMap.TryGetValue(contextKey, out var value))
                {
                    try
                    {
                        context = value.Deserialize<T>();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Log.LogException(ex, $"Failed to deserialize context for key: {contextKey}");
                    }
                }
                return false;
            }

            public T GetContext<T>(string contextKey)
            {
                return default;
            }
        }
    }
}