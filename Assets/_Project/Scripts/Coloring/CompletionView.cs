using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace OtsimoColoring.Coloring
{
    /// <summary>
    /// Renders the celebration overlay shown when a level is completed.
    /// Animates in via DOTween, blocks all underlying input through its
    /// CanvasGroup, and exposes a Back button hook for the scene controller.
    /// </summary>
    public class CompletionView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _root;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _content;
        [SerializeField] private Button _backToLevelsButton;

        [Header("Animation")]
        [SerializeField] private float _fadeDuration = 0.3f;
        [SerializeField] private float _scaleDuration = 0.45f;
        [SerializeField] private Ease _scaleEase = Ease.OutBack;

        public System.Action OnBackPressed;

        private Sequence _showSequence;

        private void Awake()
        {
            if (_backToLevelsButton != null)
            {
                _backToLevelsButton.onClick.AddListener(HandleBackClick);
            }
            HideImmediate();
        }

        private void OnDestroy()
        {
            if (_backToLevelsButton != null)
            {
                _backToLevelsButton.onClick.RemoveListener(HandleBackClick);
            }
            _showSequence?.Kill();
        }

        public void HideImmediate()
        {
            _showSequence?.Kill();
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }

        public void Show()
        {
            if (_root != null)
            {
                _root.SetActive(true);
            }
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }
            if (_content != null)
            {
                _content.localScale = Vector3.zero;
            }

            _showSequence?.Kill();
            _showSequence = DOTween.Sequence();

            if (_canvasGroup != null)
            {
                _showSequence.Join(_canvasGroup.DOFade(1f, _fadeDuration));
            }
            if (_content != null)
            {
                _showSequence.Join(
                    _content.DOScale(Vector3.one, _scaleDuration).SetEase(_scaleEase));
            }
        }

        private void HandleBackClick()
        {
            OnBackPressed?.Invoke();
        }
    }
}