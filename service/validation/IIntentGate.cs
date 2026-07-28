using System.Threading;
using System.Threading.Tasks;

namespace cfEngine.Service.Validation
{
    /// <summary>
    /// Submits intents for authoritative validation and application. Callers
    /// reconcile local state from the returned snapshot; they never mutate
    /// authoritative state directly.
    /// </summary>
    public interface IIntentGate<TIntent, TSnapshot> where TIntent : IIntent
    {
        /// <summary>
        /// Hints to callers how verdicts should be applied (predict + reconcile,
        /// or wait). The gate itself always applies through Submit.
        /// </summary>
        ValidationMode Mode { get; }

        Task<Res<TSnapshot, ValidationError>> Submit(TIntent intent, CancellationToken ct = default);
    }
}
