using System;
using System.Collections.Generic;
using cfEngine;
using cfEngine.Pooling;

namespace cfEngine.Command
{
    public class Parameters: IDisposable
    {
        private static ObjectPool<Parameters> _pool = new(
            static () => new Parameters(),
            static map =>
            {
                if (!map._isDisposed)
                {
                    Log.LogException(new InvalidOperationException("Get a non disposed ParameterMap from pool"));
                    return;
                }
            
                map.Clear();
                map._isDisposed = false;
            },
            static map =>
            {
                map.Clear();
                map._isDisposed = true;
            }, static _ => {});
    
        public static Parameters Get()
        {
            return _pool.Get();
        }

        private readonly Dictionary<string, string> _map;
        private bool _isDisposed;
    
        public int Count => _map.Count;
    
        private Parameters()
        {
            _map = new Dictionary<string, string>();
        }
    
        public void Add(string name, string param)
        {
            _map.Add(name, param);
        }

        #region Get

        public string GetString(string name)
        {
            if (!_map.TryGetValue(name, out var value))
            {
                return string.Empty;
            }

            return value;
        }

        public int GetInt(string name)
        {
            var str = GetString(name);
            if (int.TryParse(str, out var value))
                return value;

            return 0;
        }
    
        public float GetFloat(string name)
        {
            var str = GetString(name);
            if (float.TryParse(str, out var value))
                return value;
        
            return 0f;
        }
    
        public bool GetBool(string name)
        {
            var str = GetString(name);
            if (bool.TryParse(str, out var value))
                return value;
        
            return false;
        }

        #endregion

        public void Clear()
        {
            _map.Clear();
        }

        public void Dispose()
        {
            _pool.Release(this);
        }
    }
}