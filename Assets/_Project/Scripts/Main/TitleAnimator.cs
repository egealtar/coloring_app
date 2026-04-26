using DG.Tweening;
using UnityEngine;

namespace OtsimoColoring.Main
{
    /// <summary>
    /// Drops the menu title in from above the screen on Start, with a small
    /// bounce on landing. Uses anchoredPosition so it works in any UI canvas.
    /// </summary>
    public class TitleAnimator : MonoBehaviour
    {
        [Header("Animation")]
        [Tooltip("How far above its resting position the title starts (Y offset).")]
        [SerializeField] private float _startYOffset = 600f;
        [SerializeField] private float _dropDuration = 0.7f;
        [SerializeField] private Ease _dropEase = Ease.OutBounce;
        [SerializeField] private float _startDelay = 0.1f;

        private RectTransform _rect;
        private Vector2 _restingPosition;
        private Tween _dropTween;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _restingPosition = _rect.anchoredPosition;
        }

        private void OnEnable()
        {
            PlayDrop();
        }

        private void OnDestroy()
        {
            _dropTween?.Kill();
        }

        public void PlayDrop()
        {
            _dropTween?.Kill();

            // Start above the resting position, then fall to it. OutBounce
            // gives the title a small "settle" wobble when it arrives.
            _rect.anchoredPosition = _restingPosition + new Vector2(0f, _startYOffset);

            _dropTween = _rect
                .DOAnchorPos(_restingPosition, _dropDuration)
                .SetEase(_dropEase)
                .SetDelay(_startDelay);
        }
    }
}