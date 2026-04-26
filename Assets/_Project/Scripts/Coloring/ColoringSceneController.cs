using OtsimoColoring.Persistence;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OtsimoColoring.Coloring
{
    /// <summary>
    /// Top-level orchestrator for the coloring scene. Wires together the canvas,
    /// flood-fill service, palette, completion detection and celebration overlay.
    /// All dependencies are inspector-injected. No singletons.
    /// </summary>
    public class ColoringSceneController : MonoBehaviour
    {
        [Header("Sub-systems")]
        [SerializeField] private CanvasController _canvas;
        [SerializeField] private FloodFillService _floodFill;
        [SerializeField] private PaletteView _palette;
        [SerializeField] private CompletionDetector _completionDetector;
        [SerializeField] private CompletionView _completionView;

        [Header("UI")]
        [SerializeField] private Button _backButton;
        [SerializeField] private RawImage _canvasImage;

        [Header("Scene Navigation")]
        [SerializeField] private string _mainSceneName = "MainScene";

        [Header("Transition")]
        [SerializeField] private OtsimoColoring.App.SceneTransition _transition;

        private ProgressStore _progressStore;
        private ColorStore _colorStore;
        private Color _currentColor = Color.black;
        private bool _inputLocked;

        private void Awake()
        {
            _progressStore = new ProgressStore();
            _colorStore = new ColorStore();
        }

        private void Start()
        {
            ValidateReferences();
            BootstrapLevel();
        }

        private void OnEnable()
        {
            if (_palette != null)
            {
                _palette.OnColorSelected += HandleColorSelected;
            }
            if (_completionDetector != null)
            {
                _completionDetector.OnCompleted += HandleLevelCompleted;
            }
            if (_completionView != null)
            {
                _completionView.OnBackPressed += GoToMainScene;
            }
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(GoToMainScene);
            }
        }

        private void OnDisable()
        {
            if (_palette != null)
            {
                _palette.OnColorSelected -= HandleColorSelected;
            }
            if (_completionDetector != null)
            {
                _completionDetector.OnCompleted -= HandleLevelCompleted;
            }
            if (_completionView != null)
            {
                _completionView.OnBackPressed -= GoToMainScene;
            }
            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(GoToMainScene);
            }
        }

        private void ValidateReferences()
        {
            if (_canvas == null) Debug.LogError("ColoringSceneController: CanvasController missing.");
            if (_floodFill == null) Debug.LogError("ColoringSceneController: FloodFillService missing.");
            if (_palette == null) Debug.LogError("ColoringSceneController: PaletteView missing.");
            if (_completionDetector == null) Debug.LogError("ColoringSceneController: CompletionDetector missing.");
            if (_completionView == null) Debug.LogError("ColoringSceneController: CompletionView missing.");
        }

        private void BootstrapLevel()
        {
            var level = SelectedLevel.Current;
            if (level == null || level.LineArt == null)
            {
                Debug.LogError("ColoringSceneController: no SelectedLevel.Current — go through MainScene first.");
                return;
            }

            _canvas.Initialize(level.LineArt);
            _completionDetector.InitializeForCanvas(_canvas);

            int savedColorIndex = _colorStore.GetLastSelectedIndex();
            _palette.Build(savedColorIndex);

            _completionView.HideImmediate();
            _inputLocked = false;
        }

        private void HandleColorSelected(int index, Color color)
        {
            _currentColor = color;
            _colorStore.SetLastSelectedIndex(index);
        }

        public void HandleCanvasTap(PointerEventData eventData)
        {
            if (_inputLocked || _floodFill.IsFilling || _canvas == null)
            {
                return;
            }

            if (!_canvas.TryScreenToPixel(eventData.position, eventData.pressEventCamera, out int px, out int py))
            {
                return;
            }

            StartCoroutine(_floodFill.FillCoroutine(
                _canvas, px, py, _currentColor,
                onPixelsFilled: HandlePixelsFilled));
        }

        private void HandlePixelsFilled(int newlyFilled)
        {
            _completionDetector.RegisterFilledPixels(newlyFilled);
        }

        private void HandleLevelCompleted()
        {
            _inputLocked = true;
            var level = SelectedLevel.Current;
            if (level != null)
            {
                _progressStore.MarkLevelCompleted(level.UniqueId);
            }
            _completionView.Show();
        }

        private void GoToMainScene()
        {
            if (_transition != null)
            {
                _transition.FadeOutAndLoad(_mainSceneName);
            }
            else
            {
                SceneManager.LoadScene(_mainSceneName);
            }
        }
    }
}