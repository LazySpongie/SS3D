using Coimbra;
using Coimbra.Services.Events;
using FishNet.Object;
using SS3D.Core;
using SS3D.Core.Behaviours;
using SS3D.Systems.Characters.Preferences;
using SS3D.Systems.Characters.UI;
using SS3D.Systems.Entities;
using SS3D.Systems.Entities.Events;
using SS3D.Systems.Roles;
using SS3D.Systems.Rounds;
using SS3D.Systems.Rounds.Events;
using SS3D.UI.Buttons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using RoundStateUpdated = SS3D.Systems.Rounds.Events.RoundStateUpdated;

namespace SS3D.Systems.Lobby.UI
{
    public sealed class LobbyEmbarkView : NetworkActor
    {        
        [SerializeField] private LabelButton _embarkButton;
        [SerializeField] private GameObject _embarkMenu;
        [SerializeField] private Transform _embarkMenuContentRoot;
        [SerializeField] private GameObject _jobSlotPrefab;
        [SerializeField] private GameObject _departmentPrefab;

        private List<EmbarkRoleSlot> _roles = new();

        private RoundState _roundState;
        private bool _isPlayerSpawned;

        private RoleSubSystem _roleSubSystem;

        [Client]
        protected override void OnAwake()           
        {
            base.OnAwake();

            _embarkButton.OnPressedDown += HandleEmbarkButtonPressed;

            AddHandle(RoundStateUpdated.AddListener(HandleRoundStateUpdated));
            AddHandle(CrewManifestUpdated.AddListener(HandleCrewManifestUpdated));
            AddHandle(SpawnedPlayersUpdated.AddListener(HandleSpawnedPlayersUpdated));
        }

        [Client]
        public override void OnStartClient()           
        {
            base.OnStartClient();
            _roleSubSystem = SubSystems.Get<RoleSubSystem>();
        }

        [Client]
        protected override void OnDestroyed()
        {
            base.OnDestroyed();

            _embarkButton.OnPressedDown -= HandleEmbarkButtonPressed;
        }
        
        [Client]
        private void HandleSpawnedPlayersUpdated(ref EventContext context, in SpawnedPlayersUpdated e)
        {
            if (_isPlayerSpawned) return;

            EntitySubSystem system = SubSystems.Get<EntitySubSystem>();

            _isPlayerSpawned = system.IsPlayerSpawned(LocalConnection);
        }

        [Client]
        private void HandleRoundStateUpdated(ref EventContext context, in RoundStateUpdated e)
        {
            _roundState = e.RoundState;
            if (_roundState == RoundState.Preparing) _isPlayerSpawned = false; 
        }

        [Client]
        private void HandleEmbarkButtonPressed(bool pressed)
        {
            LoadList(_roleSubSystem.CrewManifest);
            _embarkMenu.SetActive(true);
        }

        [Client]
        private void HandleCrewManifestUpdated(ref EventContext context, in CrewManifestUpdated e)
        {
            if (_roundState != RoundState.Ongoing || _isPlayerSpawned) return;
            
            LoadList(e.CrewManifest);
        }

        /// <summary>
        /// Fill the menu with role options and department headers.
        /// </summary>
        [Client]
        public void LoadList(ReadOnlyDictionary<string, RoleCounter> crewManifest)
        {
            ClearList();
            string lastDept = string.Empty;
            foreach (KeyValuePair<string, RoleCounter> pair in crewManifest)
            {
                // spawn department header
                string dept = pair.Value.Department.name;
                if (lastDept != dept)
                {
                    lastDept = dept;
                    GameObject departmentHeader = Instantiate(_departmentPrefab, _embarkMenuContentRoot, true);
                    departmentHeader.transform.localScale = Vector3.one;
                    departmentHeader.GetComponentInChildren<TMP_Text>().text = pair.Value.Department.Name;
                }

                int current = pair.Value.CurrentRoles;
                int available = pair.Value.AvailableRoles;

                if (current >= available) return;
                EmbarkRoleSlot slot = Instantiate(_jobSlotPrefab, _embarkMenuContentRoot, true).GetComponent<EmbarkRoleSlot>();
                slot.transform.localScale = Vector3.one;
                _roles.Add(slot);
                slot.SetRole(pair.Value.Role);
                slot.SetCount(current, available);
                slot.OnPressed += HandleRoleSelected;
            }
        }

        /// <summary>
        /// Clear all roles.
        /// </summary>
        [Client]
        private void ClearList()
        {
            for (int i = _roles.Count - 1; i >= 0; i--)
            {
                _roles[i].OnPressed -= HandleRoleSelected;
                _roles[i].gameObject.Dispose(true);
            }
            _roles.Clear();
        }

        /// <summary>
        /// When the player presses the embark button send their character profile to the server and then spawn their character.
        /// </summary>
        [Client]
        private void HandleRoleSelected(RoleData role)
        {
            _embarkMenu.SetActive(false);
            ClearList();
            
            ClientPreferencesSubSystem preferencesSystem = SubSystems.Get<ClientPreferencesSubSystem>();
            RoundSetupSubSystem roundSetupSubSystem = SubSystems.Get<RoundSetupSubSystem>();

            Player player = preferencesSystem.EmbarkCharacter();
            roundSetupSubSystem.CmdSpawnLatePlayer(player, role.name);
        }

    }
}
