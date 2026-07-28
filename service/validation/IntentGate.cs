using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace cfEngine.Service.Validation
{
    /// <summary>
    /// Orchestrates intent validation for a service: runs the ordered rule chain,
    /// and on full pass applies the intent to service-owned authoritative state,
    /// returning the authoritative snapshot. Callers submit intents and reconcile
    /// local state from the verdict; nobody mutates authoritative state directly.
    ///
    /// The signature is async from day one so a network transport can replace the
    /// local execution path later without changing intents, rules, or callers.
    /// </summary>
    public class IntentGate<TIntent, TSnapshot> : IIntentGate<TIntent, TSnapshot> where TIntent : IIntent
    {
        private readonly IReadOnlyList<IntentRule<TIntent>> _rules;
        private readonly Func<TIntent, TSnapshot> _apply;
        private readonly bool _enforceSequenceOrder;
        private long _lastAcceptedSequence = -1;

        /// <summary>
        /// Hints to callers how verdicts should be applied (predict + reconcile,
        /// or wait). The gate itself always validates before applying.
        /// </summary>
        public ValidationMode Mode { get; set; } = ValidationMode.Pessimistic;

        public IntentGate(
            IReadOnlyList<IntentRule<TIntent>> rules,
            Func<TIntent, TSnapshot> apply,
            bool enforceSequenceOrder = false)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _apply = apply ?? throw new ArgumentNullException(nameof(apply));
            _enforceSequenceOrder = enforceSequenceOrder;
        }

        public async Task<Res<TSnapshot, ValidationError>> Submit(TIntent intent, CancellationToken ct = default)
        {
            if (_enforceSequenceOrder && intent.Sequence <= _lastAcceptedSequence)
            {
                return Res<TSnapshot, ValidationError>.Err(new ValidationError(
                    "out_of_order",
                    $"Intent sequence {intent.Sequence} rejected; last accepted sequence is {_lastAcceptedSequence}"));
            }

            foreach (var rule in _rules)
            {
                var error = await rule(intent, ct).ConfigureAwait(false);
                if (error != null)
                    return Res<TSnapshot, ValidationError>.Err(error);
            }

            var snapshot = _apply(intent);
            _lastAcceptedSequence = intent.Sequence;
            return Res<TSnapshot, ValidationError>.Ok(snapshot);
        }
    }
}
