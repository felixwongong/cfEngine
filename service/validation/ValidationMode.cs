namespace cfEngine.Service.Validation
{
    /// <summary>
    /// How the intent caller applies authoritative verdicts.
    /// Pessimistic: wait for the verdict before mutating local state.
    /// Optimistic: predict locally, then reconcile (or roll back) on the verdict.
    /// </summary>
    public enum ValidationMode
    {
        Pessimistic,
        Optimistic
    }
}
