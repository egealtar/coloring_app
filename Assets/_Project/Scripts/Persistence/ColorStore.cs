using UnityEngine;

namespace OtsimoColoring.Persistence
{
    /// <summary>
    /// Persists the user's last selected palette color index between sessions.
    /// </summary>
    public class ColorStore
    {
        private const string SelectedColorIndexKey = "OtsimoColoring.LastSelectedColorIndex";
        private const int DefaultIndex = 0;

        public int GetLastSelectedIndex()
        {
            return PlayerPrefs.GetInt(SelectedColorIndexKey, DefaultIndex);
        }

        public void SetLastSelectedIndex(int index)
        {
            PlayerPrefs.SetInt(SelectedColorIndexKey, index);
            PlayerPrefs.Save();
        }

        public void Reset()
        {
            PlayerPrefs.DeleteKey(SelectedColorIndexKey);
            PlayerPrefs.Save();
        }
    }
}