using System.Collections.Generic;

namespace cfEngine.DataStructure
{
    public interface IPropertyMap
    {
        public bool Get<T>(string key, out T? value);
        public void RegisterPropertyChange(Action<string> callback);
        public void UnregisterPropertyChange(Action<string> callback);
    }
}
