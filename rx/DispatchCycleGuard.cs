namespace cfEngine.Rx
{
    /// <summary>
    /// Shared per-thread dispatch guard used by all Relay implementations to prevent
    /// synchronous infinite dispatch cycles. Non-generic so the depth counter is global
    /// across all relay type instantiations on the same thread.
    /// </summary>
    internal static class DispatchCycleGuard
    {
        [System.ThreadStatic]
        internal static int DispatchDepth;

        internal const int MaxDepth = 64;
    }
}
