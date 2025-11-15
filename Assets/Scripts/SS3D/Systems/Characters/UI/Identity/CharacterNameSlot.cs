using TMPro;
using SS3D.Attributes;
using UnityEngine;
using SS3D.Core.Behaviours;

namespace SS3D.Systems.Characters.UI
{
    public class CharacterNameSlot : Actor
    {
        public delegate void OnValueChangedEventHandler(CharacterNameType type, string value);
        
        public delegate void OnStartedEventHandler(CharacterNameSlot slot);

        // When the selection changes
        public event OnValueChangedEventHandler OnValueChanged;
        
        // When this script starts
        public event OnStartedEventHandler OnStarted;

        [SerializeField] [NotNull] private CharacterNameType _type;
        
        [SerializeField] [NotNull] private TMP_InputField _inputField;

        public CharacterNameType Type => _type;

        public TMP_InputField InputField => _inputField;

        protected override void OnAwake()
        {
            base.OnAwake();
            _inputField.onEndEdit.AddListener(valueChanged);
        }
        
        protected override void OnStart()
        {
            base.OnStart();
            OnStarted?.Invoke(this);
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();
            _inputField.onValueChanged.RemoveListener(valueChanged);
        }

        private void valueChanged(string value)
        {
            OnValueChanged?.Invoke(_type, value);
        }
        
        public void SetValue(string value)
        {
            _inputField.SetTextWithoutNotify(value); 
        }
        
    }
}