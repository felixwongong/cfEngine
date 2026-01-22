using System.Collections.Generic;
using cfEngine.Pooling;

namespace cfEngine.DataStructure
{
    public interface IPropertyMap
    {
        public bool Get<T>(string key, out T? value);
        public void RegisterPropertyChange(Action<string> callback);
        public void UnregisterPropertyChange(Action<string> callback);
    }

    public class PropertyMap: IPropertyMap
    {
        private readonly Dictionary<string, object?> properties;
    
        private WeakReferenceList<Action<string>> propertyChangedCallbacks;

        public PropertyMap()
        {
            properties = DictionaryPool<string, object?>.Default.Get();
            propertyChangedCallbacks = new WeakReferenceList<Action<string>>();
        }

        public void RegisterPropertyChange(Action<string> callback)
        {
            propertyChangedCallbacks.Add(callback);
        }
    
        public void UnregisterPropertyChange(Action<string> callback)
        {
            propertyChangedCallbacks.Remove(callback);
        }
    
        public bool Get<T>(string key, out T? value)
        {
            if (properties.TryGetValue(key, out var obj) && obj is T t)
            {
                value = t;
                return true;
            }

            value = default;
            return false;
        }
    
        public void Set<T>(string key, T? value)
        {
            properties[key] = value;
            foreach (var cb in propertyChangedCallbacks)
            {
                cb?.Invoke(key);
            }
        }
    }
}