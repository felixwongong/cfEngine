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

        /// <summary>
        /// Re-reads every backing file into a fresh table and publishes it in one swap.
        /// Returns false (leaving the previous table and instances untouched) when any file
        /// is missing, malformed or fails validation. Never recaches.
        /// </summary>
        bool Reload();

        /// <summary>
        /// Refreshes the backing files from their canonical source, then runs the same
        /// all-or-nothing publication as <see cref="Reload"/>. Returns false when the loader
        /// has no recache capability or the recache/parse failed.
        /// </summary>
        bool Revert();

        /// <summary>
        /// Raised synchronously after an explicit <see cref="Reload"/>/<see cref="Revert"/>
        /// publishes a new table. Never raised by the initial load.
        /// </summary>
        event Action? Reloaded;
    }
    
    public abstract class InfoManager: IInfoManager
    {
        public abstract Type infoType { get; }
        public abstract IEnumerable<object> GetAllValue();
        public abstract void LoadInfo();
        public abstract Task LoadInfoAsync(CancellationToken cancellationToken);
        public abstract bool Reload();
        public abstract bool Revert();
        public abstract event Action? Reloaded;
        protected virtual void OnLoadCompleted() {}

        public virtual void Dispose()
        {
        }
    }
}