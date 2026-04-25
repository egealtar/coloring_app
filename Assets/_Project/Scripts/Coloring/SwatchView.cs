using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace OtsimoColoring.Coloring
{
    /// <summary>
    /// A single color swatch in the palette toolbar.
    /// Owns its own selection visuals and selection bounce animation.
    /// Selection state is driven externally by PaletteView.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SwatchView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _colorImage;
        [SerializeField] private Outline _selectionOutline;
        [SerializeField] private Button _button;

        [Header("Animation")]
        [SerializeField] private float _selectScale = 1.2f;
        [SerializeField] private float _bounceDuration = 0.25f;
        [SerializeField] private Ease _bounceEase = Ease.OutBack;

        public System.Action<int> OnSelected;

        private int _index;
        private Tween _scaleTween;

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
            _button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }
            _scaleTween?.Kill();
        }

        public void Bind(int index, Color color)
        {
            _index = index;
            if (_colorImage != null)
            {
                _colorImage.color = color;
            }
            SetSelected(false, animate: false);
        }

        public void SetSelected(bool isSelected, bool animate)
        {
            Debug.Log($"SetSelected called: isSelected={isSelected}, animate={animate}, outline null? {_selectionOutline == null}");

            if (_selectionOutline != null)
            {
                _selectionOutline.enabled = isSelected;
            }

            float targetScale = isSelected ? _selectScale : 1f;

            _scaleTween?.Kill();

            if (animate)
            {
                _scaleTween = transform
                    .DOScale(targetScale, _bounceDuration)
                    .SetEase(_bounceEase);
            }
            else
            {
                transform.localScale = Vector3.one * targetScale;
            }
        }

        private void HandleClick()
        {
            Debug.Log($"SwatchView {_index} clicked");

            OnSelected?.Invoke(_index);
        }
    }
}