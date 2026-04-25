using UnityEngine;

namespace OtsimoColoring.Persistence
{
    /// <summary>
    /// Persists per-level completion state using PlayerPrefs.
    /// Simple wrapper so the persistence mechanism can be swapped later
    /// (e.g. JSON on disk) without touching callers.
    /// </summary>
    public class ProgressStore
    {
        private const string CompletedKeyPrefix = "OtsimoColoring.LevelCompleted.";
        private const string AllKeysListKey = "OtsimoColoring.LevelCompletedKeys";

        public bool IsLevelCompleted(string levelUniqueId)
        {
            if (string.IsNullOrEmpty(levelUniqueId))
            {
                return false;
            }
            return PlayerPrefs.GetInt(CompletedKeyPrefix + levelUniqueId, 0) == 1;
        }

        public void MarkLevelCompleted(string levelUniqueId)
        {
            if (string.IsNullOrEmpty(levelUniqueId))
            {
                return;
            }
            PlayerPrefs.SetInt(CompletedKeyPrefix + levelUniqueId, 1);
            AppendKnownKey(levelUniqueId);
            PlayerPrefs.Save();
        }

        public void ResetAll()
        {
            string knownKeysCsv = PlayerPrefs.GetString(AllKeysListKey, string.Empty);
            if (!string.IsNullOrEmpty(knownKeysCsv))
            {
                foreach (var key in knownKeysCsv.Split(';'))
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        PlayerPrefs.DeleteKey(CompletedKeyPrefix + key);
                    }
                }
            }
            PlayerPrefs.DeleteKey(AllKeysListKey);
            PlayerPrefs.Save();
        }

        private void AppendKnownKey(string levelUniqueId)
        {
            string existing = PlayerPrefs.GetString(AllKeysListKey, string.Empty);
            if (!string.IsNullOrEmpty(existing))
            {
                foreach (var key in existing.Split(';'))
                {
                    if (key == levelUniqueId)
                    {
                        return; // already tracked
                    }
                }
                existing += ";" + levelUniqueId;
            }
            else
            {
                existing = levelUniqueId;
            }
            PlayerPrefs.SetString(AllKeysListKey, existing);
        }
    }
}