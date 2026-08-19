namespace cfEngine.Service.Validation
{
    /// <summary>
    /// Named validator for reuse via ValidateWith when rules outgrow lambdas.
    /// </summary>
    public interface IValidator<T>
    {
        ValidationError? Validate(T value);
    }
}
