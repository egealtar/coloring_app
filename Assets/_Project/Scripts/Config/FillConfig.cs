using UnityEngine;

namespace OtsimoColoring.Config
{
    [CreateAssetMenu(
        fileName = "FillConfig",
        menuName = "OtsimoColoring/Fill Config",
        order = 3)]
    public class FillConfig : ScriptableObject
    {
        [Header("Fill Matching")]
        [Tooltip("Two pixels are considered 'same region' if their color channels differ by less than this (0..1).")]
        [Range(0f, 1f)]
        [SerializeField] private float _colorTolerance = 0.1f;

        [Header("Outline Detection")]
        [Tooltip("A pixel is treated as outline if its brightness is below this threshold (0..1). Near-black pixels are blocked.")]
        [Range(0f, 1f)]
        [SerializeField] private float _outlineBrightnessThreshold = 0.2f;

        [Header("Completion")]
        [Tooltip("Percentage of non-outline pixels that must be colored before the level is considered complete (0..1).")]
        [Range(0f, 1f)]
        [SerializeField] private float _completionThreshold = 0.9f;

        [Header("Performance")]
        [Tooltip("Max pixels processed per frame during flood fill. Lower = smoother, higher = faster fill.")]
        [Min(100)]
        [SerializeField] private int _pixelsPerFrame = 4000;

        public float ColorTolerance => _colorTolerance;
        public float OutlineBrightnessThreshold => _outlineBrightnessThreshold;
        public float CompletionThreshold => _completionThreshold;
        public int PixelsPerFrame => _pixelsPerFrame;
    }
}