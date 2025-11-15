using TMPro;
using SS3D.Attributes;
using UnityEngine;
using UnityEngine.UI;
using SS3D.Core.Behaviours;
using SS3D.Systems.Characters.Preferences;
using System;

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
        [SerializeField] [NotNull] private TMP_InputField _floatInput;

        public BodyType Type => _type;

        public Slider Slider => _slider;

        // protected override void OnAwake()
        // {
        //     base.OnAwake();
        //     _slider.onValueChanged.AddListener(valueChanged);
        // }
        
        protected override void OnStart()
        {
            base.OnStart();
            OnStarted?.Invoke(this);
        }

        // protected override void OnDestroyed()
        // {
        //     base.OnDestroyed();
        //     _slider.onValueChanged.RemoveListener(valueChanged);
        // }

        public void OnFloatInputChanged()
        {
            float value = float.Parse(_floatInput.text);
            value = Mathf.Clamp(value, _slider.minValue, _slider.maxValue);
            value = (float)Math.Round((double)value, 2);
            _slider.value = value;
        }
        
        public void OnSliderChanged()
        {
            float value = _slider.value;
            value = (float)Math.Round((double)value, 2);
            _floatInput.SetTextWithoutNotify(value.ToString());
            OnValueChanged?.Invoke(_type, value);
        }
        
        public void SetValue(float value)
        {
            value = Mathf.Clamp(value, _slider.minValue, _slider.maxValue);
            
            value = (float)Math.Round((double)value, 2);

            _slider.SetValueWithoutNotify(value); 
            _floatInput.SetTextWithoutNotify(value.ToString());
        }
        
    }
}