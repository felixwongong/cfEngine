using System;
using System.Threading;
using cfEngine.Logging;
using cfEngine.Service;

namespace cfEngine.Core
{
    public partial class DomainExtension
    {
        public static Domain With<TService>(this Domain domain, TService service)
            where TService : IService
        {
            var serviceName = typeof(TService).FullName;
            domain.Register(service, serviceName);
            return domain;
        }

        public static TService Get<TService>(this Domain domain) where TService : IService
        {
            var serviceName = typeof(TService).FullName;
            var res = domain.GetService<TService>(serviceName);
            if (res.TryGetError(out var err))
            {
                Log.LogException(err);
                var findRes = domain.FindService<TService>();
                if (findRes.TryGetError(out err))
                    throw err;
                else
                    return findRes.value;
            }
            else
                return res.value;
        }
    }
    
    public class Domain: ServiceLocator
    {
        private static Domain _current;
        public static Domain Current => _current;
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