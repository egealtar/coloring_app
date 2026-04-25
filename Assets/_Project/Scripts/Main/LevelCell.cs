using OtsimoColoring.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OtsimoColoring.Main
{
    /// <summary>
    /// Represents a single level tile in the main menu grid.
    /// Displays the level's thumbnail and completion state, and raises
    /// an event when tapped so the MainSceneController can handle the load.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class LevelCell : MonoBehaviour
    {
        [SerializeField] private RawImage _thumbnailImage;
        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private GameObject _completionMark;
        [SerializeField] private Button _button;

        private LevelData _levelData;

        public System.Action<LevelData> OnSelected;

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
        }

        public void Bind(LevelData levelData, bool isCompleted)
        {
            _levelData = levelData;

            if (_labelText != null)
            {
                _labelText.text = levelData.DisplayName;
            }

            if (_thumbnailImage != null)
            {
                _thumbnailImage.texture = levelData.LineArt;
            }

            if (_completionMark != null)
            {
                _completionMark.SetActive(isCompleted);
            }
        }

        private void HandleClick()
        {
            if (_levelData == null)
            {
                Debug.LogWarning("LevelCell: clicked before Bind() was called.");
                return;
            }

            OnSelected?.Invoke(_levelData);
        }
    }
}