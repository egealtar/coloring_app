#if UNITY_EDITOR
using OtsimoColoring.Persistence;
using UnityEditor;
using UnityEngine;

namespace OtsimoColoring.EditorTools
{
    /// <summary>
    /// Editor-only menu item that wipes all locally persisted progress
    /// (level completion + last selected color). Mirrors what a user-facing
    /// "reset" button would do, but available from the Unity menu so QA can
    /// quickly retest first-time flows.
    /// </summary>
    public static class ResetProgressMenuItem
    {
        [MenuItem("Tools/OtsimoColoring/Reset All Progress")]
        private static void ResetAll()
        {
            new ProgressStore().ResetAll();
            new ColorStore().Reset();
            PlayerPrefs.Save();

            Debug.Log("[OtsimoColoring] All progress and color preferences cleared.");
        }
    }
}
#endif