using System;
using System.Threading;
using cfEngine;
using cfEngine.Service;

namespace cfEngine.Core
{
    public static partial class DomainExtension
    {
        public static Domain With<TService>(this Domain domain, TService service)
            where TService : IService
        {
            var serviceName = typeof(TService).FullName;
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
        private static Domain _current;
        public static Domain Current
        {
            get
            {
                if (_current == null)
                {
                    SetCurrent(new Domain());
                    Log.LogInfo($"[{nameof(Domain)}] Current not set, using default.");
                }

                return _current;
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
    }
}