using UnityEngine;

namespace OtsimoColoring.Config
{
    [CreateAssetMenu(
        fileName = "PaletteConfig",
        menuName = "OtsimoColoring/Palette Config",
        order = 2)]
    public class PaletteConfig : ScriptableObject
    {
        [SerializeField] private Color[] _colors = new Color[6];

        public int Count => _colors.Length;

        public Color GetColor(int index)
        {
            if (index < 0 || index >= _colors.Length)
            {
                Debug.LogError($"PaletteConfig: index {index} out of range (0..{_colors.Length - 1})");
                return Color.magenta;
            }
            return _colors[index];
        }

        public bool IsValidIndex(int index)
        {
            return index >= 0 && index < _colors.Length;
        }
    }
}