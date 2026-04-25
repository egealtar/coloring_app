using System.Collections.Generic;
using OtsimoColoring.Config;
using UnityEngine;

namespace OtsimoColoring.Coloring
{
    /// <summary>
    /// Spawns swatch instances based on the palette config and tracks
    /// which one is currently selected. Notifies listeners (the scene
    /// controller) whenever the selection changes.
    /// </summary>
    public class PaletteView : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PaletteConfig _paletteConfig;

        [Header("Scene References")]
        [SerializeField] private RectTransform _swatchContainer;
        [SerializeField] private SwatchView _swatchPrefab;

        [Header("Layout")]
        [Tooltip("Width and height of each swatch, in pixels.")]
        [SerializeField] private float _swatchSize = 120f;

        [Tooltip("Vertical distance between swatch centers.")]
        [SerializeField] private float _verticalSpacing = 160f;

        [Tooltip("Vertical offset of the first swatch from the top of the container.")]
        [SerializeField] private float _topPadding = 100f;

        public System.Action<int, Color> OnColorSelected;

        private readonly List<SwatchView> _swatches = new List<SwatchView>();
        private int _selectedIndex = -1;

        public int SelectedIndex => _selectedIndex;

        public Color SelectedColor =>
            _paletteConfig != null && _paletteConfig.IsValidIndex(_selectedIndex)
                ? _paletteConfig.GetColor(_selectedIndex)
                : Color.black;

        // Temporary auto-build for testing - will be removed when ColoringSceneController is ready
        private void Start()
        {
            Build(0);
        }

        public void Build(int initialSelectedIndex)
        {
            ClearExisting();

            if (_paletteConfig == null || _swatchContainer == null || _swatchPrefab == null)
            {
                Debug.LogError("PaletteView: missing references.");
                return;
            }

            for (int i = 0; i < _paletteConfig.Count; i++)
            {
                SpawnSwatch(i);
            }

            int safeIndex = _paletteConfig.IsValidIndex(initialSelectedIndex)
                ? initialSelectedIndex
                : 0;

            ApplySelection(safeIndex, animate: false);
        }

        private void SpawnSwatch(int index)
        {
            // Instantiate as child of container, preserving local-space transform
            var swatch = Instantiate(_swatchPrefab, _swatchContainer, false);
            swatch.gameObject.name = $"Swatch_{index}";
            swatch.Bind(index, _paletteConfig.GetColor(index));
            //swatch.OnSelected += HandleSwatchSelected;

            // Force a clean RectTransform regardless of what the prefab held
            var rt = swatch.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
            rt.sizeDelta = new Vector2(_swatchSize, _swatchSize);

            float yOffset = -_topPadding - (index * _verticalSpacing);
            rt.anchoredPosition3D = new Vector3(0f, yOffset, 0f);

            _swatches.Add(swatch);
        }

        private void ClearExisting()
        {
            foreach (var swatch in _swatches)
            {
                if (swatch != null)
                {
                    Destroy(swatch.gameObject);
                }
            }
            _swatches.Clear();
            _selectedIndex = -1;
        }

        private void HandleSwatchSelected(int index)
        {
            if (index == _selectedIndex)
            {
                return;
            }
            ApplySelection(index, animate: true);
        }

        private void ApplySelection(int index, bool animate)
        {
            for (int i = 0; i < _swatches.Count; i++)
            {
                _swatches[i].SetSelected(i == index, animate);
            }

            _selectedIndex = index;
            OnColorSelected?.Invoke(index, _paletteConfig.GetColor(index));
        }
    }
}