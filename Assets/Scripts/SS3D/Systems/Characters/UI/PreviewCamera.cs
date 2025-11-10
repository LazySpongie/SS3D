using UnityEngine;
using SS3D.Core.Behaviours;
using UnityEngine.EventSystems;

namespace SS3D.Systems.Characters.UI
{
    /// <summary>
    /// Rotates the character preview camera in character creation
    /// </summary>
    public class PreviewCamera : Actor
    {
        [SerializeField] private float _sensitivity = 100f;
        private float _yaw = 0f;
        private float _pitch = 0f;

        private Quaternion _startRotation;

        protected override void OnAwake()
        {
            base.OnAwake();
            _startRotation = transform.rotation;
        }

        public void ResetCamera()
        {
            transform.rotation = _startRotation;
        }
        
        public void MouseInput(PointerEventData eventData)
        {
            HandleInput(eventData.delta);

            Quaternion yawRotation = Quaternion.Euler(_pitch, _yaw, 0f);

            RotateCamera(yawRotation);
        }

        private void RotateCamera(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        private void HandleInput(Vector2 inputDelta)
        {
            _yaw += inputDelta.x * _sensitivity * Time.deltaTime;
            _pitch += inputDelta.y * _sensitivity * Time.deltaTime;
            _pitch = Mathf.Clamp(_pitch, -90, 90);
        }

    }
}