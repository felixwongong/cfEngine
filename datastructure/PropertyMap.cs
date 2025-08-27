using System.Collections.Generic;
using cfEngine.Pooling;

namespace cfEngine.DataStructure;

public delegate void PropertyChangedCallback(string propertyName, object propertyValue);
    
public interface IPropertyMap
{
    public void RegisterPropertyChange(PropertyChangedCallback callback);
    public void UnregisterPropertyChange(PropertyChangedCallback callback);
}

public class PropertyMap: IPropertyMap
{
    private Dictionary<string, object> properties;
    
    private WeakReferenceList<PropertyChangedCallback> propertyChangedCallbacks;

    public PropertyMap()
    {
        properties = DictionaryPool<string, object>.Default.Get();
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
    
    public void Set<T>(string key, T value)
    {
        properties[key] = value;
        foreach (var cb in propertyChangedCallbacks)
        {
            cb?.Invoke(key, value);
        }
    }
}