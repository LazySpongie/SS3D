using SS3D.Systems.Characters.Preferences;
using SS3D.Systems.Roles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SS3D.Systems.Characters.UI
{
    public class EmbarkRoleSlot : MonoBehaviour
    {
        public delegate void EmbarkRolePressed(RoleData role);

        public event EmbarkRolePressed OnPressed;

        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _current;
        [SerializeField] private TMP_Text _available;
        [SerializeField] private Button _button;
        
        private RoleData _role;

        public string RoleName => _role.name;

        public void Embark()
        {
            OnPressed?.Invoke(_role);
        }
        
        public void SetRole(RoleData role)
        {
            _role = role;
            _name.text = _role.Name;
        }

        public void SetCount(int current, int available)
        {
            _current.text = current + " current";
            _available.text = available + " available";
        }

    }
}
