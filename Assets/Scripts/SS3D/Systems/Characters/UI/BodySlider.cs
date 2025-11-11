using TMPro;
using SS3D.Attributes;
using UnityEngine;
using UnityEngine.UI;
using SS3D.Core.Behaviours;
using SS3D.Systems.Characters.Preferences;

namespace SS3D.Systems.Characters.UI
{
    public class BodySlider : Actor
    {
        public delegate void OnValueChangedEventHandler(BodyType type, float value);
        
        public delegate void OnStartedEventHandler(BodySlider slider);

        // When the selection changes
        public event OnValueChangedEventHandler OnValueChanged;
        
        // When this script starts
        public event OnStartedEventHandler OnStarted;

        [SerializeField] [NotNull] private BodyType _type;
        
        [SerializeField] [NotNull] private Slider _slider;

        public BodyType Type => _type;

        public Slider Slider => _slider;

        protected override void OnAwake()
        {
            base.OnAwake();
            _slider.onValueChanged.AddListener(valueChanged);
        }
        
        protected override void OnStart()
        {
            base.OnStart();
            OnStarted?.Invoke(this);
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();
            _slider.onValueChanged.RemoveListener(valueChanged);
        }

        private void valueChanged(float value)
        {
            OnValueChanged?.Invoke(_type, value);
        }
        
        public void SetValue(float value)
        {
            _slider.SetValueWithoutNotify(value); 
        }
        
    }
}