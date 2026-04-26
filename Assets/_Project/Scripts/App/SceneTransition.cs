using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OtsimoColoring.App
{
    /// <summary>
    /// Drives a full-screen fade in/out using DOTween. Lives on a small overlay
    /// canvas in each scene; on Start it fades the cover from opaque to clear,
    /// and FadeOutAndLoad fades back to opaque before loading the next scene.
    /// Replaces the previous instant SceneManager.LoadScene calls so transitions
    /// feel continuous and use the same animation library as the rest of the UI.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class SceneTransition : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private float _fadeInDuration = 0.35f;
        [SerializeField] private float _fadeOutDuration = 0.3f;
        [SerializeField] private Ease _fadeEase = Ease.OutQuad;

        [Header("References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _coverImage;

        private bool _isTransitioning;

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
            // Start fully covered so the fade-in masks the scene swap.
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        private void Start()
        {
            FadeIn();
        }

        public void FadeIn()
        {
            _canvasGroup.DOFade(0f, _fadeInDuration)
                .SetEase(_fadeEase)
                .OnComplete(() => _canvasGroup.blocksRaycasts = false);
        }

        /// <summary>
        /// Fade the cover back to opaque, then load the requested scene.
        /// Multiple calls while a transition is active are ignored so a
        /// double-tap on the back button can't queue two loads.
        /// </summary>
        public void FadeOutAndLoad(string sceneName)
        {
            if (_isTransitioning)
            {
                return;
            }
            _isTransitioning = true;
            _canvasGroup.blocksRaycasts = true;

            _canvasGroup.DOFade(1f, _fadeOutDuration)
                .SetEase(_fadeEase)
                .OnComplete(() => SceneManager.LoadScene(sceneName));
        }
    }
}