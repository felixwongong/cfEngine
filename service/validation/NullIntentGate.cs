using System;
using System.Threading;
using System.Threading.Tasks;

namespace cfEngine.Service.Validation
{
    /// <summary>
    /// A gate with no validation: no rules, no sequence enforcement. Submit runs
    /// the apply delegate directly and returns its snapshot. Use when a service
    /// wants the uniform intent-submit flow without an authoritative check —
    /// callers never deal with a null gate. Mode is always Pessimistic since
    /// there is no authority to disagree with a prediction.
    /// </summary>
    public class NullIntentGate<TIntent, TSnapshot> : IIntentGate<TIntent, TSnapshot> where TIntent : IIntent
    {
        private readonly Func<TIntent, TSnapshot> _apply;

        public ValidationMode Mode => ValidationMode.Pessimistic;

        public NullIntentGate(Func<TIntent, TSnapshot> apply)
        {
            _apply = apply ?? throw new ArgumentNullException(nameof(apply));
        }

        public Task<Res<TSnapshot, ValidationError>> Submit(TIntent intent, CancellationToken ct = default)
        {
            return Task.FromResult(Res<TSnapshot, ValidationError>.Ok(_apply(intent)));
        }
    }
}
