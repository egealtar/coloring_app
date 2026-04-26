using OtsimoColoring.Config;
using OtsimoColoring.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OtsimoColoring.Main
{
    /// <summary>
    /// Bootstraps the main menu: reads the level database, spawns a LevelCell
    /// for each entry, and loads the ColoringScene when a cell is tapped.
    /// Dependencies are injected via the inspector (no singletons).
    /// </summary>
    public class MainSceneController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private LevelDatabase _levelDatabase;

        [Header("Scene References")]
        [SerializeField] private Transform _gridContainer;
        [SerializeField] private LevelCell _levelCellPrefab;

        [Header("Scene Navigation")]
        [SerializeField] private string _coloringSceneName = "ColoringScene";

        [Header("Transition")]
        [SerializeField] private OtsimoColoring.App.SceneTransition _transition;

        private ProgressStore _progressStore;

        private void Awake()
        {
            _progressStore = new ProgressStore();
        }

        private void Start()
        {
            ValidateReferences();
            BuildGrid();
        }

        private void ValidateReferences()
        {
            if (_levelDatabase == null)
            {
                Debug.LogError("MainSceneController: LevelDatabase is not assigned.");
            }
            if (_gridContainer == null)
            {
                Debug.LogError("MainSceneController: Grid container is not assigned.");
            }
            if (_levelCellPrefab == null)
            {
                Debug.LogError("MainSceneController: LevelCell prefab is not assigned.");
            }
        }

        private void BuildGrid()
        {
            if (_levelDatabase == null || _gridContainer == null || _levelCellPrefab == null)
            {
                return;
            }

            foreach (var level in _levelDatabase.OrderedByNumber())
            {
                var cell = Instantiate(_levelCellPrefab, _gridContainer);
                bool isCompleted = _progressStore.IsLevelCompleted(level.UniqueId);
                cell.Bind(level, isCompleted);
                cell.OnSelected += HandleLevelSelected;
            }
        }

        private void HandleLevelSelected(LevelData level)
        {
            SelectedLevel.Current = level;
            if (_transition != null)
            {
                _transition.FadeOutAndLoad(_coloringSceneName);
            }
            else
            {
                SceneManager.LoadScene(_coloringSceneName);
            }
        }
    }
}