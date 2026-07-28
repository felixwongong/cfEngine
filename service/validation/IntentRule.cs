using System.Threading;
using System.Threading.Tasks;

namespace cfEngine.Service.Validation
{
    /// <summary>
    /// A single validation rule for an intent. Returns null when the intent
    /// passes, or a ValidationError describing the rejection. Rules must be pure:
    /// no state mutation, since a later rule may still reject the intent.
    /// </summary>
    public delegate Task<ValidationError?> IntentRule<in TIntent>(TIntent intent, CancellationToken ct);
}
