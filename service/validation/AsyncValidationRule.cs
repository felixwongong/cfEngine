using System.Threading;
using System.Threading.Tasks;

namespace cfEngine.Service.Validation
{
    /// <summary>
    /// Async sibling of ValidationRule — e.g. a rule that asks a server.
    /// Same purity contract as ValidationRule.
    /// </summary>
    public delegate Task<ValidationError?> AsyncValidationRule<in T>(T value, CancellationToken ct);
}
