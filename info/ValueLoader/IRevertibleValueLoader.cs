namespace cfEngine.Info
{
    /// <summary>
    /// Optional capability for <see cref="IValueLoader{TInfo}"/> implementations that can refresh
    /// their backing data files from the canonical source. Managers expose it as
    /// <see cref="IInfoManager.Revert"/>.
    /// </summary>
    public interface IRevertibleValueLoader
    {
        /// <summary>
        /// Refreshes the loader's backing data from its canonical source. Throws
        /// <see cref="System.NotSupportedException"/> when the loader has no recache capability.
        /// </summary>
        void Recache();
    }
}
