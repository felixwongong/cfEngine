using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using cfEngine.Pooling;

namespace cfEngine.Info
{
    public interface IValueLoader<TInfo>
    {
        public ListPool<TInfo>.Handle Load(out List<TInfo> values);
        public Task<List<TInfo>> LoadAsync(CancellationToken cancellationToken);
    }
}