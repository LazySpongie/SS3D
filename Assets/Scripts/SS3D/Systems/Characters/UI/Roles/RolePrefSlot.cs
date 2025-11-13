using SS3D.Systems.Characters.Preferences;
using SS3D.Systems.Roles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SS3D.Systems.Characters.UI
{
    public class RolePrefSlot : MonoBehaviour
    {
        
        public delegate void RolePrefSlotChanged(RolePrefSlot slot, RoleData role, RolePriority type);

        public event RolePrefSlotChanged OnPrefChanged;

        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private Button _never;
        [SerializeField] private Button _low;
        [SerializeField] private Button _medium;
        [SerializeField] private Button _high;
        
        [SerializeField] public bool FallbackJob;

        private RoleData _role;

        public string RoleName => _role.name;

        // methods with enum params cant be selected on the buttons in editor so i have to do this
        public void ChangePrefNever() => ChangePref(RolePriority.Never);

        public void ChangePrefLow() => ChangePref(RolePriority.Low);

        public void ChangePrefMedium() => ChangePref(RolePriority.Medium);

        public void ChangePrefHigh() => ChangePref(RolePriority.High);

        public void ChangePref(RolePriority type)
        {
            SetPref(type);
            OnPrefChanged?.Invoke(this, _role, type);
        }
        
        public void SetRole(RoleData role, RolePriority type)
        {
            _role = role;
            _nameText.text = _role.Name;
            SetPref(type);
        }

        public void SetPref(RolePriority type)
        {
            ResetButtons();

            Button buttonToSelect = _never;

            switch (type)
            {
                case RolePriority.Low:
                    buttonToSelect = _low;
                    break;
                case RolePriority.Medium:
                    buttonToSelect = _medium;
                    break;
                case RolePriority.High:
                    buttonToSelect = _high;
                    break;
            }

            buttonToSelect.interactable = false;
        }

        private void ResetButtons()
        {
            _never.interactable = true;
            _low.interactable = true;
            _medium.interactable = true;
            _high.interactable = true;
        }

    }
}
