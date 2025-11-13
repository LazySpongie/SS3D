using UnityEngine;
using SS3D.Attributes;
using System.Collections.Generic;
using SS3D.Systems.Characters.Preferences;
using SS3D.Systems.Characters.Events;
using Coimbra.Services.Events;
using SS3D.Systems.Roles;
using Coimbra;
using Actor = SS3D.Core.Behaviours.Actor;
using SS3D.Core;
using TMPro;

namespace SS3D.Systems.Characters.UI.View
{
    /// <summary>
    /// Controls the character name and background in the character creation ui
    /// </summary>
    public sealed class RoleTabView : Actor
    {
        [Header("Jobs")]
        [SerializeField] [NotNull] private Transform _rolePrefRoot;
        [SerializeField] [NotNull] private GameObject _roleSlotPrefab;
        [SerializeField] [NotNull] private GameObject _departmentPrefab;
        [SerializeField] [NotNull] private RolePrefSlot _fallbackRoleSlot;

        [Header("Antags")]
        [SerializeField] [NotNull] private Transform _antagPrefRoot;
        [SerializeField] [NotNull] private GameObject _antagSlotPrefab;

        private List<RolePrefSlot> _roleSlots = new();

        private ClientPreferencesSubSystem _preferences;

        #region Setup

        protected override void OnAwake()
        {
            base.OnAwake();

            _preferences = SubSystems.Get<ClientPreferencesSubSystem>();

            _fallbackRoleSlot.OnPrefChanged += HandleJobPrefChanged;

            AddHandle(LocalLobbyCharacterChanged.AddListener(HandleCharacterChanged));
            
            LoadRoles();
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();

            _fallbackRoleSlot.OnPrefChanged -= HandleJobPrefChanged;
        }

        #endregion

        #region Update UI

        /// <summary>
        /// Set the ui when the character is modified.
        /// </summary>
        private void HandleCharacterChanged(ref EventContext context, in LocalLobbyCharacterChanged e)
        {
            switch (e.ChangeType)
            {
                case CharacterChangeType.Load:
                case CharacterChangeType.Jobs:
                    SetRolePrefs(e.Character);
                    break;
            }
        }

        private void LoadRoles()
        {
            ClearRoles();

            RolesAvailable rolesAvailable = SubSystems.Get<RoleSubSystem>().RolesAvailable;

            _fallbackRoleSlot.SetRole(rolesAvailable.OverFlowRole, RolePriority.Never);

            foreach (DepartmentsData department in rolesAvailable.Departments)
            {
                GameObject departmentHeader = Instantiate(_departmentPrefab, _rolePrefRoot, true);

                departmentHeader.transform.localScale = Vector3.one;

                departmentHeader.GetComponentInChildren<TMP_Text>().text = department.Data.Name;


                foreach (RolesData availableRole in department.Roles)
                {
                    RoleData Data = availableRole.Data;

                    RolePrefSlot slot = Instantiate(_roleSlotPrefab, _rolePrefRoot, true).GetComponent<RolePrefSlot>();
                    slot.transform.localScale = Vector3.one;

                    _roleSlots.Add(slot);

                    slot.SetRole(Data, RolePriority.Never);

                    slot.OnPrefChanged += HandleJobPrefChanged;
                }
            }
        }

        private void ClearRoles()
        {
            for (int i = _roleSlots.Count - 1; i >= 0; i--)
            {
                _roleSlots[i].OnPrefChanged -= HandleJobPrefChanged;
                _roleSlots[i].gameObject.Dispose(true);
                _roleSlots.RemoveAt(i);
            }
            _roleSlots.Clear();
        }

        private void ClearRolePrefs()
        {
            if (_roleSlots.Count == 0) return;

            _fallbackRoleSlot.SetPref(RolePriority.Never);

            foreach (RolePrefSlot slot in _roleSlots)
            {
                slot.SetPref(RolePriority.Never);
            }
        }

        private void SetRolePrefs(CharacterProfile character)
        {
            if (_roleSlots.Count == 0) return;

            ClearRolePrefs();
            
            if (character.OverFlowRole) _fallbackRoleSlot.SetPref(RolePriority.High);
            
            foreach (KeyValuePair<string, RolePriority> pair in character.Roles)
            {
                RolePrefSlot slot = _roleSlots.Find(slot => slot.RoleName == pair.Key);

                if (slot == null) continue;

                slot.SetPref(pair.Value);
            }
        }

        #endregion

        #region Set Character

        /// <summary>
        /// Callback when the character name text field is changed.
        /// </summary>
        private void HandleJobPrefChanged(RolePrefSlot slot, RoleData role, RolePriority type)
        {
            string jobName = string.Empty;

            if (!slot.FallbackJob) jobName = role.name;

            _preferences.SetJobPreference(jobName, type);
        }

        #endregion
        
    }
}
