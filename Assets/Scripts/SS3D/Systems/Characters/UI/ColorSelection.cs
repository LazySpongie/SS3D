using TMPro;
using SS3D.Attributes;
using UnityEngine;
using UnityEngine.UI;
using SS3D.Core.Behaviours;
using UnityEngine.Events;
using SS3D.Systems.Characters.Preferences;

namespace SS3D.Systems.Characters.UI
{
    public class ColorSelection : Actor
    {
        public delegate void ColorChangedHandler (ColorSelection select, Color color);
        
        public event ColorChangedHandler OnColorSelected;

        [SerializeField] private AppearanceType _type;
        [SerializeField] private ColorOptionsSO _colors;
        [SerializeField] private Button _button;

        private int _currentIndex = 0;

        public AppearanceType Type => _type;

        protected override void OnAwake()
        {
            base.OnAwake();
            _button.onClick.AddListener(HandleButtonPressed);
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();
            _button.onClick.RemoveListener(HandleButtonPressed);
        }

        public void SetColor(Color color)
        {
            GetComponent<Image>().color = color;
        }
        
        public void HandleButtonPressed()
        {
            _currentIndex++;
            if (_currentIndex >= _colors.Colors.Count) _currentIndex = 0;
            SetColor(_colors.Colors[_currentIndex]);
            OnColorSelected.Invoke(this, _colors.Colors[_currentIndex]);
        }

    }
}