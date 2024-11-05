namespace cfEngine.Util
{
    public enum ValidationState
    {
        Success,
        Failed,
    }

    public sealed class Validation
    {
        public readonly ValidationState State;
        public readonly object Result;
    }
}