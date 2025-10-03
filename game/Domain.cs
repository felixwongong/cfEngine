using System;
using System.Threading;
using cfEngine.Logging;
using cfEngine.Service;

namespace cfEngine.Core
{
    public class Domain: ServiceLocator
    {
        private static Domain _current;
        public static Domain Current => _current;
        public static CancellationToken TaskToken { get; private set; } = CancellationToken.None;
        
        public static void SetCurrent(Domain domain)
        {
            _current?.Dispose();
            _current = domain;
        }

        public virtual void HandleException(Exception ex)
        {
            Log.LogException(ex);
        }
    }
}