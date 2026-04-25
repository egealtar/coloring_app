using OtsimoColoring.Config;

namespace OtsimoColoring.Persistence
{
    /// <summary>
    /// Tiny static handoff used to pass the chosen LevelData from the main
    /// menu into the coloring scene. Kept as a plain static field so both
    /// scenes can reference it without any singleton or DontDestroyOnLoad plumbing.
    /// </summary>
    public static class SelectedLevel
    {
        public static LevelData Current;
    }
}