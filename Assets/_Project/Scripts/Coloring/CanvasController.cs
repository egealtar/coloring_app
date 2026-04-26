using OtsimoColoring.Config;
using UnityEngine;
using UnityEngine.UI;

namespace OtsimoColoring.Coloring
{
    /// <summary>
    /// Owns the displayed line-art texture and a separate logic buffer that
    /// flood fill operates on. The logic buffer treats transparent pixels as
    /// solid white so fill regions are well-defined; the display texture keeps
    /// the original art (including transparency) for visual fidelity. When the
    /// user paints, the new color is written to BOTH buffers so the visible
    /// canvas updates and subsequent fills see the painted region correctly.
    /// </summary>
    public class CanvasController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private RawImage _canvasImage;
        [SerializeField] private RectTransform _canvasRect;

        [Header("Config")]
        [SerializeField] private FillConfig _fillConfig;

        // Logic buffer: transparent pixels composited onto white, used by the
        // flood-fill algorithm and outline detection.
        private Color32[] _sourcePixels;
        private Color32[] _workingPixels;

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

            // Build both buffers from the original art:
            // - _displayPixels keeps the raw pixel (with original alpha) for visuals.
            // - _sourcePixels composites onto white so flood fill has a solid surface.
            for (int i = 0; i < len; i++)
            {
                var p = rawPixels[i];
                _displayPixels[i] = p;

                if (p.a == 0)
                {
                    _sourcePixels[i] = new Color32(255, 255, 255, 255);
                }
                else if (p.a < 255)
                {
                    float a = p.a / 255f;
                    byte r = (byte)Mathf.RoundToInt(p.r * a + 255 * (1 - a));
                    byte g = (byte)Mathf.RoundToInt(p.g * a + 255 * (1 - a));
                    byte b = (byte)Mathf.RoundToInt(p.b * a + 255 * (1 - a));
                    _sourcePixels[i] = new Color32(r, g, b, 255);
                }
                else
                {
                    _sourcePixels[i] = new Color32(p.r, p.g, p.b, 255);
                }
            }

            _workingPixels = new Color32[len];
            System.Array.Copy(_sourcePixels, _workingPixels, len);

            // Display texture: starts as the original art. Paint operations
            // overwrite individual pixels here so the user sees the result while
            // the original line-art (with its transparency) shows through everywhere
            // they haven't painted yet.
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

        /// <summary>
        /// Write a paint color to both the logic buffer (so future fills see the
        /// painted region) and the display buffer (so the user actually sees the
        /// new color rather than the original transparent line-art).
        /// </summary>
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
            var p = _sourcePixels[y * _width + x];
            float brightness = (p.r + p.g + p.b) / (3f * 255f);
            return brightness < (_fillConfig != null ? _fillConfig.OutlineBrightnessThreshold : 0.2f);
        }

        public Color32[] WorkingPixelsBuffer => _workingPixels;
        public Color32[] SourcePixelsBuffer => _sourcePixels;
    }
}