using DG.Tweening;
using OtsimoColoring.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OtsimoColoring.Main
{
    /// <summary>
    /// Represents a single level tile in the main menu grid.
    /// Displays the level's thumbnail and completion state, raises
    /// an event when tapped, and exposes a "next-up" pulse the menu
    /// controller can drive on the first uncompleted level.
    /// On completion a diagonal line is animated across the cell, as
    /// if the level was crossed off a checklist.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class LevelCell : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RawImage _thumbnailImage;
        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private RectTransform _diagonalLine;
        [SerializeField] private Button _button;
        [Tooltip("The visual root that pulses when this cell is the next level to play.")]
        [SerializeField] private RectTransform _pulseTarget;

        [Header("Animation")]
        [SerializeField] private float _tapPunchScale = 0.08f;
        [SerializeField] private float _tapPunchDuration = 0.25f;
        [SerializeField] private float _diagonalDrawDuration = 0.5f;
        [SerializeField] private Ease _diagonalDrawEase = Ease.OutCubic;
        [SerializeField] private float _nextUpPulseScale = 1.05f;
        [SerializeField] private float _nextUpPulseDuration = 0.85f;

        public System.Action<LevelData> OnSelected;

        private LevelData _levelData;
        private Tween _tapTween;
        private Tween _diagonalTween;
        private Tween _pulseTween;

        public bool IsCompleted { get; private set; }

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
            if (_pulseTarget == null)
            {
                _pulseTarget = transform as RectTransform;
            }
            _button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }
            _tapTween?.Kill();
            _diagonalTween?.Kill();
            _pulseTween?.Kill();
        }

        public void Bind(LevelData levelData, bool isCompleted)
        {
            _levelData = levelData;
            IsCompleted = isCompleted;

            if (_labelText != null)
            {
                _labelText.text = levelData.DisplayName;
            }

            if (_thumbnailImage != null)
            {
                _thumbnailImage.texture = levelData.LineArt;
            }

            ApplyDiagonal(isCompleted, animate: true);
        }

        public void SetNextUp(bool isNextUp)
        {
            _pulseTween?.Kill();
            if (_pulseTarget == null) return;

            if (isNextUp)
            {
                _pulseTween = _pulseTarget
                    .DOScale(Vector3.one * _nextUpPulseScale, _nextUpPulseDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                _pulseTarget.localScale = Vector3.one;
            }
        }

        private void ApplyDiagonal(bool isCompleted, bool animate)
        {
            _diagonalTween?.Kill();

            if (_diagonalLine == null) return;

            _diagonalLine.gameObject.SetActive(isCompleted);

            if (!isCompleted)
            {
                return;
            }

            // The line uses a left-edge pivot so scaling X from 0 to 1 reads
            // as it being drawn from one corner of the cell to the other.
            _diagonalLine.localScale = new Vector3(animate ? 0f : 1f, 1f, 1f);

            if (animate)
            {
                _diagonalTween = _diagonalLine
                    .DOScaleX(1f, _diagonalDrawDuration)
                    .SetEase(_diagonalDrawEase);
            }
        }

        private void HandleClick()
        {
            if (_levelData == null)
            {
                Debug.LogWarning("LevelCell: clicked before Bind() was called.");
                return;
            }

            _tapTween?.Kill();
            _tapTween = transform
                .DOPunchScale(Vector3.one * -_tapPunchScale, _tapPunchDuration, vibrato: 1, elasticity: 0.5f)
                .OnComplete(() => OnSelected?.Invoke(_levelData));
        }
    }
}