using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OtsimoColoring.Config
{
    [CreateAssetMenu(
        fileName = "LevelDatabase",
        menuName = "OtsimoColoring/Level Database",
        order = 1)]
    public class LevelDatabase : ScriptableObject
    {
        [SerializeField] private List<LevelData> _levels = new List<LevelData>();

        public IReadOnlyList<LevelData> Levels => _levels;

        public int Count => _levels.Count;

        public LevelData GetByNumber(int levelNumber)
        {
            return _levels.FirstOrDefault(l => l != null && l.LevelNumber == levelNumber);
        }

        public IEnumerable<LevelData> OrderedByNumber()
        {
            return _levels
                .Where(l => l != null)
                .OrderBy(l => l.LevelNumber);
        }
    }
}