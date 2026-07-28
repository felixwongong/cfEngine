using System;
using System.Threading.Tasks;

namespace cfEngine.Service.Validation
{
    /// <summary>
    /// Helpers for building intent rules.
    /// </summary>
    public static class IntentRules
    {
        /// <summary>
        /// Wraps a synchronous rule function as an IntentRule.
        /// </summary>
        public static IntentRule<TIntent> Sync<TIntent>(Func<TIntent, ValidationError?> rule)
        {
            return (intent, _) => Task.FromResult(rule(intent));
        }
    }
}
