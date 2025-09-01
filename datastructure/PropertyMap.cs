using System.Collections.Generic;
using cfEngine.Pooling;

namespace cfEngine.DataStructure;

public delegate void PropertyChangedCallback(string propertyName, object? propertyValue);
    
public interface IPropertyMap
{
    public void Get<T>(string key, out T? value);
    public void Restore(PropertyMap other);
    public void RegisterPropertyChange(PropertyChangedCallback callback);
    public void UnregisterPropertyChange(PropertyChangedCallback callback);
}

public class PropertyMap: IPropertyMap
{
    private readonly Dictionary<string, object?> properties;
    
    private WeakReferenceList<PropertyChangedCallback> propertyChangedCallbacks;

    public PropertyMap()
    {
        properties = DictionaryPool<string, object?>.Default.Get();
        propertyChangedCallbacks = new WeakReferenceList<PropertyChangedCallback>();
    }

    public void RegisterPropertyChange(PropertyChangedCallback callback)
    {
        propertyChangedCallbacks.Add(callback);
    }
    
    public void UnregisterPropertyChange(PropertyChangedCallback callback)
    {
        propertyChangedCallbacks.Remove(callback);
    }
    
    public void Restore(PropertyMap other)
    {
        properties.Clear();
        foreach (var kv in other.properties)
        {
            properties[kv.Key] = kv.Value;
        }
    }
    
    public void Get<T>(string key, out T? value)
    {
        if (properties.TryGetValue(key, out var obj) && obj is T t)
        {
            value = t;
            return;
        }

        value = default;
    }
    
    public void Set<T>(string key, T? value)
    {
        properties[key] = value;
        foreach (var cb in propertyChangedCallbacks)
        {
            cb?.Invoke(key, value);
        }
    }
}