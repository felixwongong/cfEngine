namespace cfEngine.Service.Validation
{
    /// <summary>
    /// An action the client (gameplay or user input) wants the service to perform.
    /// Intents never carry client-computed results; the authoritative side
    /// recomputes outcomes. Sequence orders intents per client for reconciliation.
    /// </summary>
    public interface IIntent
    {
        long Sequence { get; }
    }
}
