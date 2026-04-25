using OtsimoColoring.Config;
using UnityEngine;
using UnityEngine.UI;

namespace OtsimoColoring.Coloring
{
    /// <summary>
    /// Owns the source line-art texture and a separate working canvas texture
    /// that the user paints onto. Provides pixel coordinate translation from
    /// screen-space taps and applies fill operations to the working texture.
    /// The source is kept untouched so outline checks always reflect the original art.
    /// </summary>
    public class CanvasController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private RawImage _canvasImage;
        [SerializeField] private RectTransform _canvasRect;

        [Header("Config")]
        [SerializeField] private FillConfig _fillConfig;

        private Texture2D _sourceTexture;
        private Texture2D _workingTexture;
        private Color32[] _sourcePixels;
        private Color32[] _workingPixels;
        private int _width;
        private int _height;

        public int Width => _width;
        public int Height => _height;
        public FillConfig FillConfig => _fillConfig;

        public void Initialize(Texture2D sourceLineArt)
        {
            if (sourceLineArt == null)
            {
                Debug.LogError("CanvasController: source line-art is null.");
                return;
            }

            // Keep source as-is; create a writable working copy.
            _sourceTexture = sourceLineArt;
            _sourcePixels = _sourceTexture.GetPixels32();

            _width = _sourceTexture.width;
            _height = _sourceTexture.height;

            _workingTexture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
            _workingTexture.filterMode = FilterMode.Point;
            _workingTexture.wrapMode = TextureWrapMode.Clamp;
            _workingPixels = new Color32[_sourcePixels.Length];
            System.Array.Copy(_sourcePixels, _workingPixels, _sourcePixels.Length);
            _workingTexture.SetPixels32(_workingPixels);
            _workingTexture.Apply();

            if (_canvasImage != null)
            {
                _canvasImage.texture = _workingTexture;
            }
        }

        /// <summary>
        /// Convert a screen-space pointer position into texture pixel coordinates.
        /// Returns false if the pointer is outside the canvas image bounds.
        /// </summary>
        public bool TryScreenToPixel(Vector2 screenPos, Camera eventCamera, out int px, out int py)
        {
            px = 0;
            py = 0;

            if (_canvasRect == null) return false;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvasRect, screenPos, eventCamera, out var localPoint))
            {
                return false;
            }

            var rect = _canvasRect.rect;
            float u = (localPoint.x - rect.x) / rect.width;   // 0..1
            float v = (localPoint.y - rect.y) / rect.height;  // 0..1

            if (u < 0f || u > 1f || v < 0f || v > 1f)
            {
                return false;
            }

            px = Mathf.Clamp(Mathf.FloorToInt(u * _width), 0, _width - 1);
            py = Mathf.Clamp(Mathf.FloorToInt(v * _height), 0, _height - 1);
            return true;
        }

        public Color32 GetSourcePixel(int x, int y)
        {
            return _sourcePixels[y * _width + x];
        }

        public Color32 GetWorkingPixel(int x, int y)
        {
            return _workingPixels[y * _width + x];
        }

        public void SetWorkingPixel(int x, int y, Color32 color)
        {
            _workingPixels[y * _width + x] = color;
        }

        /// <summary>
        /// Push the in-memory pixel buffer to the GPU. Call sparingly — once per
        /// fill completion, or throttled during long fills, since Apply() is expensive.
        /// </summary>
        public void ApplyChanges()
        {
            if (_workingTexture == null) return;
            _workingTexture.SetPixels32(_workingPixels);
            _workingTexture.Apply();
        }

        public bool IsOutlinePixel(int x, int y)
        {
            var p = _sourcePixels[y * _width + x];
            // Brightness on 0..1 scale; treat near-black as outline.
            float brightness = (p.r + p.g + p.b) / (3f * 255f);
            return brightness < (_fillConfig != null ? _fillConfig.OutlineBrightnessThreshold : 0.2f);
        }

        public Color32[] WorkingPixelsBuffer => _workingPixels;
        public Color32[] SourcePixelsBuffer => _sourcePixels;
    }
}