using System;
using System.Threading;
using cfEngine;
using cfEngine.Service;

namespace cfEngine.Core
{
    public static class DomainExtension
    {
        public static TDomain With<TDomain, TService>(this TDomain domain, TService service)
            where TDomain: Domain 
            where TService : IService
        {
            return With<TDomain>(domain, service, typeof(TService));
        }

        public static TDomain With<TDomain>(this TDomain domain, IService service, Type serviceType)
            where TDomain: Domain
        {
            if (!serviceType.IsInstanceOfType(service))
            {
                throw new Exception($"Service type {serviceType} is not assignable to {typeof(TDomain)}");
            }
            
            var serviceName = serviceType.FullName;
            if (serviceName == null)
                throw new Exception("Service name is empty");
            
            domain.Register(service, serviceName);
            return domain;
        }

        public static Res<TService, Exception> Get<TService>(this Domain domain) where TService : IService
        {
            var serviceName = typeof(TService).FullName;
            if (string.IsNullOrEmpty(serviceName))
                return new InvalidOperationException("Service name is empty");
            
            var res = domain.Get<TService>(serviceName);
            if (res.HasError(out var err))
            {
                Log.LogException(err);
                var findRes = domain.FindService<TService>();
                if (findRes.HasError(out err))
                    throw err;
                else
                    return findRes.value;
            }

            return res.value;
        }
    }
    
    public class Domain: ServiceLocator
    {
        private static Domain? _current;
        public static Domain Current
        {
            get
            {
                if (_current == null)
                {
                    SetCurrent(new Domain());
                    Log.LogInfo($"[{nameof(Domain)}] Current not set, using default.");
                }

                return _current!;
            }
        }

        public static CancellationToken TaskToken { get; private set; } = CancellationToken.None;
        
        public static void SetCurrent(Domain domain)
        {
            if (_current != null)
            {
                Log.LogInfo($"Domain disposed: {_current}");
                _current?.Dispose();
                _current = domain;
                Log.LogInfo($"Domain switched from domain ({_current}) to ({domain})");
            }
            else
            {
                _current = domain;
                Log.LogInfo($"Domain set to: {domain}");
            }
        }

        public virtual void HandleException(Exception ex)
        {
            Log.LogException(ex);
        }

        public static void CLear()
        {
            _current?.Dispose();
            _current = null;
        }
    }
}