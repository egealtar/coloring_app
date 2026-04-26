using OtsimoColoring.Config;
using UnityEngine;

namespace OtsimoColoring.Coloring
{
    /// <summary>
    /// Tracks how many of the canvas's non-outline (paintable) pixels have been
    /// colored at least once, and signals completion when the configured threshold
    /// is crossed. Total paintable pixels are precomputed once per level so
    /// per-fill checks stay O(1).
    /// </summary>
    public class CompletionDetector : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private FillConfig _fillConfig;

        public bool IsCompleted { get; private set; }

        public float CurrentRatio => _totalPaintable > 0
            ? (float)_paintedSoFar / _totalPaintable
            : 0f;

        public System.Action OnCompleted;

        private int _totalPaintable;
        private int _paintedSoFar;

        public void InitializeForCanvas(CanvasController canvas)
        {
            IsCompleted = false;
            _paintedSoFar = 0;
            _totalPaintable = CountPaintablePixels(canvas);
        }

        public void RegisterFilledPixels(int newlyFilled)
        {
            if (IsCompleted || newlyFilled <= 0)
            {
                return;
            }

            _paintedSoFar += newlyFilled;
            float threshold = _fillConfig != null ? _fillConfig.CompletionThreshold : 0.9f;

            if (CurrentRatio >= threshold)
            {
                IsCompleted = true;
                OnCompleted?.Invoke();
            }
        }

        private static int CountPaintablePixels(CanvasController canvas)
        {
            int count = 0;
            int width = canvas.Width;
            int height = canvas.Height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!canvas.IsOutlinePixel(x, y))
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }
}