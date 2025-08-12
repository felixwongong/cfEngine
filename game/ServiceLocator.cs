using System;
using System.Collections;
using System.Collections.Generic;
using cfEngine.Core;
using cfEngine.Logging;

namespace cfEngine.Service
{
    public interface IServiceModel : IRuntimeSavable
    {
    }
    
    public interface IService : IDisposable
    {
    }

    public interface IModelService : IService
    {
        public IServiceModel GetModel { get; }
    }
    
    public interface IServiceLocator: IEnumerable<IService>, IDisposable
    {
        void Register<T>(T service, string serviceName) where T: IService;
        void Unregister<T>(T service) where T: IService;
        void Unregister(string serviceName);
        T GetService<T>(string serviceName) where T: IService;
    }

    public class ServiceLocator : IServiceLocator
    {
        private Dictionary<string, IService> _serviceMap = new();
        public void Register<T>(T service, string serviceName) where T : IService
        {
            if (!_serviceMap.TryAdd(serviceName, service))
            {
                Log.LogException(new ArgumentException($"Service already registered, serviceName: {serviceName}, service: {service.ToString()}"));
            }
        }

        public void Unregister<T>(T service) where T : IService
        {
            string foundKey = string.Empty;
            foreach (var (key, value) in _serviceMap)
            {
                if (value.Equals(service))
                {
                    foundKey = key;
                }
            }
            
            if(!_serviceMap.Remove(foundKey, out var removedService))
            {
                Log.LogException(new ArgumentException($"Service not found, serviceName: {foundKey}"));
                return;
            } 
            
            removedService.Dispose();
        }

        public void Unregister(string serviceName)
        {
            if(!_serviceMap.Remove(serviceName, out var removedService))
            {
                Log.LogException(new ArgumentException($"Service not found, serviceName: {serviceName}"));
                return;
            }
            
            removedService.Dispose();
        }
        
        public bool HasService(string serviceName)
        {
            return _serviceMap.ContainsKey(serviceName);
        }

        public T GetService<T>(string serviceName) where T : IService
        {
            if(!_serviceMap.TryGetValue(serviceName, out var service))
            {
                throw new KeyNotFoundException($"Service not found, serviceName: {serviceName}");
            }

            return (T)service;
        }

        public void Dispose()
        {
            foreach (var service in _serviceMap.Values)
            {
                service.Dispose();
            }
            
            _serviceMap.Clear();
        }

        public IEnumerator<IService> GetEnumerator()
        {
            return _serviceMap.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}