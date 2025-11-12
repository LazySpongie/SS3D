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
        [SerializeField] public float Sensitivity = 100f;
        [SerializeField] public float MinPitch = -90f;
        [SerializeField] public float MaxPitch = 90f;
        [SerializeField] public float ZoomSensitivity = 100f;
        [SerializeField] public float MinDistance = 1.2f;
        [SerializeField] public float MaxDistance = 5f;
        
        private float _yaw = 0f;
        private float _pitch = 0f;

        private Quaternion _startRotation;
        private Transform _camera;

        protected override void OnAwake()
        {
            base.OnAwake();
            _startRotation = transform.rotation;
            _camera = GetComponentInChildren<Camera>().transform;
        }

        public void ResetCamera()
        {
            transform.rotation = _startRotation;
        }

        public void ScrollInput(float delta)
        {
            Vector3 pos = _camera.localPosition;
            pos.z -= delta * ZoomSensitivity * Time.deltaTime;
            pos.z = Mathf.Clamp(pos.z, MinDistance, MaxDistance);
            _camera.localPosition = pos;

        }
        
        public void MouseInput(Vector2 inputDelta)
        {
            HandleInput(inputDelta);

            Quaternion yawRotation = Quaternion.Euler(_pitch, _yaw, 0f);

            RotateCamera(yawRotation);
        }

        private void RotateCamera(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        private void HandleInput(Vector2 inputDelta)
        {
            _yaw += inputDelta.x * Sensitivity * Time.deltaTime;
            _pitch += inputDelta.y * Sensitivity * Time.deltaTime;
            _pitch = Mathf.Clamp(_pitch, MinPitch, MaxPitch);
        }

    }
}