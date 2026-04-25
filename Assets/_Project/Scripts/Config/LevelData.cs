using UnityEngine;

namespace OtsimoColoring.Config
{
    [CreateAssetMenu(
        fileName = "LevelData",
        menuName = "OtsimoColoring/Level Data",
        order = 0)]
    public class LevelData : ScriptableObject
    {
        [SerializeField] private int _levelNumber;
        [SerializeField] private string _displayName;
        [SerializeField] private Texture2D _lineArt;

        public int LevelNumber => _levelNumber;
        public string DisplayName => _displayName;
        public Texture2D LineArt => _lineArt;

        public string UniqueId => $"level_{_levelNumber}";
    }
}