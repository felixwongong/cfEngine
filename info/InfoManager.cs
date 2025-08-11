using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using cfEngine.IO;
using cfEngine.Serialize;

namespace cfEngine.Info
{
    public interface IInfoManager: IDisposable
    {
        Type infoType { get; }
        IEnumerable<object> GetAllValue();
        void LoadInfo();
        Task LoadInfoAsync(CancellationToken cancellationToken);
    }
    
    public abstract class InfoManager: IInfoManager
    {
        public abstract Type infoType { get; }
        public abstract IEnumerable<object> GetAllValue();
        public abstract void LoadInfo();
        public abstract Task LoadInfoAsync(CancellationToken cancellationToken);
        protected virtual void OnLoadCompleted() {}

        public virtual void Dispose()
        {
        }
    }
}