namespace Oerfi
{
    /// <summary>
    /// Project-wide constants.
    /// </summary>
    public static class GameConstants
    {
        /// <summary>
        /// Version of the save-data format. Must be incremented together with any
        /// change to a persisted data structure (save-compatibility rule in CLAUDE.md;
        /// save format ADR planned for the save/load milestone).
        /// </summary>
        public const int SAVE_DATA_VERSION = 1;
    }
}
