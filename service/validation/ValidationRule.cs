namespace cfEngine.Service.Validation
{
    /// <summary>
    /// A single validation rule. Returns null when the value passes, or a
    /// ValidationError describing the rejection. Rules must be pure: no state
    /// mutation, since a later rule may still reject.
    /// </summary>
    public delegate ValidationError? ValidationRule<in T>(T value);
}
