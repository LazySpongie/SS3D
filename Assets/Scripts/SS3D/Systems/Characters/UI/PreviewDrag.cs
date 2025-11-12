using UnityEngine;
using SS3D.Core.Behaviours;
using UnityEngine.EventSystems;

namespace SS3D.Systems.Characters.UI
{
    /// <summary>
    /// Handles dragging to rotate the character preview camera in character creation
    /// </summary>
    public class PreviewDrag : Actor, IDragHandler, IScrollHandler
    {
        [SerializeField] 
        private PreviewCamera _previewCamera;

        public void OnDrag(PointerEventData eventData)
        {
            _previewCamera?.MouseInput(eventData.delta);
        }

        public void OnScroll(PointerEventData eventData)
        {
            _previewCamera?.ScrollInput(eventData.scrollDelta.y);
        }
    }
}