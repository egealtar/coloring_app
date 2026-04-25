using UnityEngine;
using UnityEngine.EventSystems;

namespace OtsimoColoring.Coloring
{
    /// <summary>
    /// Lives on the canvas RawImage so the EventSystem can route taps here,
    /// then forwards them to the scene controller. Keeps controller free of
    /// requiring a Graphic component on the same GameObject.
    /// </summary>
    public class CanvasInputRelay : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private ColoringSceneController _controller;

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"CanvasInputRelay clicked at {eventData.position}");

            if (_controller != null)
            {
                _controller.HandleCanvasTap(eventData);
            }
        }
    }
}