using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace OtsimoColoring.Coloring
{
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
            Debug.Log($"[Swatch] Awake called on {gameObject.name}. Outline component on this GameObject? {GetComponent<Outline>() != null}. _selectionOutline assigned? {_selectionOutline != null}. _colorImage assigned? {_colorImage != null}");

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
            Debug.Log($"[Swatch_{_index}] Bind called. Color={color} (a={color.a}). gameObject={gameObject.name}");
            
            if (_colorImage != null)
            {
                _colorImage.color = color;
                Debug.Log($"[Swatch_{_index}] After setting color, _colorImage.color={_colorImage.color}");
            }
            else
            {
                Debug.LogWarning($"[Swatch_{_index}] _colorImage is NULL!");
            }
            
            SetSelected(false, animate: false);
        }

        public void SetSelected(bool isSelected, bool animate)
        {
            Debug.Log($"[Swatch_{_index}] SetSelected: isSel={isSelected}, animate={animate}, outlineNull={_selectionOutline == null}, currentScale={transform.localScale.x}");

            if (_selectionOutline != null)
            {
                bool wasEnabled = _selectionOutline.enabled;
                _selectionOutline.enabled = isSelected;
                Debug.Log($"[Swatch_{_index}] Outline.enabled: {wasEnabled} -> {_selectionOutline.enabled}");
            }
            else
            {
                Debug.LogWarning($"[Swatch_{_index}] _selectionOutline is NULL, can't toggle outline!");
            }

            float targetScale = isSelected ? _selectScale : 1f;
            Debug.Log($"[Swatch_{_index}] targetScale={targetScale}, _selectScale={_selectScale}");

            _scaleTween?.Kill();
            transform.localScale = Vector3.one * targetScale;
            
            Debug.Log($"[Swatch_{_index}] After set, localScale={transform.localScale.x}");
        }

        private void HandleClick()
        {
            Debug.Log($"[Swatch_{_index}] HandleClick! gameObject={gameObject.name}, OnSelected null? {OnSelected == null}");
            OnSelected?.Invoke(_index);
        }
    }
}