using System;
using System.Collections.Generic;

namespace cfEngine.Rx
{
    /// <summary>
    /// Thrown under CF_RX_INSTRUMENTED when a synchronous Relay dispatch cycle is detected
    /// (e.g. ReactiveProperty A → listener → ReactiveProperty B → listener → ReactiveProperty A).
    /// Carries the owner chain so the overlay/log can show the cycle path.
    ///
    /// In release builds (without CF_RX_INSTRUMENTED) this exception is never thrown —
    /// the per-relay re-entrancy guard and depth limit prevent the cycle by skipping
    /// + logging, without throwing. The type is still defined so Relay.cs catch clauses
    /// can reference it unconditionally.
    /// </summary>
    public sealed class RelayCycleException : Exception
    {
        public IReadOnlyList<string> OwnerChain { get; }

        public RelayCycleException(IReadOnlyList<string> ownerChain, string message)
            : base(message)
        {
            OwnerChain = ownerChain ?? Array.Empty<string>();
        }
    }
}
