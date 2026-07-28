namespace cfEngine.Service.Validation
{
    /// <summary>
    /// Typed validation failure, flows as the error side of Res across service
    /// boundaries (R6). Code is stable/machine-readable; Message is for logs.
    /// </summary>
    public sealed record ValidationError(string Code, string Message);
}
