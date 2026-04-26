using OtsimoColoring.Config;
using UnityEngine;
using UnityEngine.UI;

namespace OtsimoColoring.Coloring
{
    /// <summary>
    /// Owns the displayed line-art texture and a separate logic buffer that
    /// flood fill operates on. Transparent pixels in the source are treated
    /// as paintable white in the logic buffer, while any pixel that the
    /// artist actually drew (alpha > 0) is preserved and used to detect
    /// outlines, so even faint AA strokes block the fill from leaking.
    /// </summary>
    public class CanvasController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private RawImage _canvasImage;
        [SerializeField] private RectTransform _canvasRect;

        [Header("Config")]
        [SerializeField] private FillConfig _fillConfig;

        // Logic buffers used by the flood-fill algorithm.
        private Color32[] _sourcePixels;
        private Color32[] _workingPixels;

        // True for pixels the artist actually drew (alpha > 0 in the original).
        // Anything in this set blocks the fill, regardless of brightness.
        private bool[] _isInkMask;

        // Display buffer + texture: original art preserved, painted on top.
        private Color32[] _displayPixels;
        private Texture2D _displayTexture;

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

            _width = sourceLineArt.width;
            _height = sourceLineArt.height;

            var rawPixels = sourceLineArt.GetPixels32();
            int len = rawPixels.Length;

            _sourcePixels = new Color32[len];
            _displayPixels = new Color32[len];
            _isInkMask = new bool[len];

            // Treat any drawn pixel (alpha > 0) as ink. The flood fill respects
            // this mask so faint anti-aliased line ends still block the fill.
            const byte inkAlphaCutoff = 16;

            for (int i = 0; i < len; i++)
            {
                var p = rawPixels[i];
                _displayPixels[i] = p;

                if (p.a < inkAlphaCutoff)
                {
                    // Effectively transparent → paintable white in logic buffer.
                    _sourcePixels[i] = new Color32(255, 255, 255, 255);
                    _isInkMask[i] = false;
                }
                else
                {
                    // Drawn pixel → composite onto white for visual fidelity but
                    // mark it as ink so the fill won't cross it.
                    float a = p.a / 255f;
                    byte r = (byte)Mathf.RoundToInt(p.r * a + 255 * (1 - a));
                    byte g = (byte)Mathf.RoundToInt(p.g * a + 255 * (1 - a));
                    byte b = (byte)Mathf.RoundToInt(p.b * a + 255 * (1 - a));
                    _sourcePixels[i] = new Color32(r, g, b, 255);
                    _isInkMask[i] = true;
                }
            }

            _workingPixels = new Color32[len];
            System.Array.Copy(_sourcePixels, _workingPixels, len);

            _displayTexture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
            _displayTexture.filterMode = FilterMode.Point;
            _displayTexture.wrapMode = TextureWrapMode.Clamp;
            _displayTexture.SetPixels32(_displayPixels);
            _displayTexture.Apply();

            if (_canvasImage != null)
            {
                _canvasImage.texture = _displayTexture;
            }
        }

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
            float u = (localPoint.x - rect.x) / rect.width;
            float v = (localPoint.y - rect.y) / rect.height;

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
            int idx = y * _width + x;
            _workingPixels[idx] = color;
            _displayPixels[idx] = color;
        }

        public void ApplyChanges()
        {
            if (_displayTexture == null) return;
            _displayTexture.SetPixels32(_displayPixels);
            _displayTexture.Apply();
        }

        public bool IsOutlinePixel(int x, int y)
        {
            // Primary check: the artist drew here (any alpha above the cutoff).
            // This catches faint AA strokes that brightness alone would miss.
            if (_isInkMask != null && _isInkMask[y * _width + x])
            {
                return true;
            }

            // Secondary check: brightness, kept as a fallback for line-art that
            // ships without alpha (e.g., a fully opaque white background).
            var p = _sourcePixels[y * _width + x];
            float brightness = (p.r + p.g + p.b) / (3f * 255f);
            float threshold = _fillConfig != null ? _fillConfig.OutlineBrightnessThreshold : 0.5f;
            return brightness < threshold;
        }

        public Color32[] WorkingPixelsBuffer => _workingPixels;
        public Color32[] SourcePixelsBuffer => _sourcePixels;
    }
}