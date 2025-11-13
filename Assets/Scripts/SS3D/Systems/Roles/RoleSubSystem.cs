using FishNet.Object;
using SS3D.Core.Behaviours;
using SS3D.Systems.PlayerControl;
using UnityEngine;
using SS3D.Logging;
using System.Collections.Generic;
using System.Linq;
using SS3D.Systems.Entities;
using SS3D.Systems.Inventory.Containers;
using SS3D.Systems.Inventory.Items;
using SS3D.Core;
using SS3D.Systems.Inventory.Items.Generic;
using SS3D.Systems.Characters;
using System.Collections.ObjectModel;
using SS3D.Systems.Characters.Preferences;
using Coimbra;
using System;
using Random = UnityEngine.Random;
using SS3D.Systems.Rounds;
using SS3D.Systems.Rounds.Events;
using Coimbra.Services.Events;

namespace SS3D.Systems.Roles
{
    /// <summary>
    /// Controls the assignment of roles when the round prepares
    /// </summary>
    public class RoleSubSystem : NetworkSubSystem
    {
        [SerializeField] private RolesAvailable _rolesAvailable;

        private Dictionary<string, RoleCounter> _roleCounters = new();

        private Dictionary<Player, RoleData> _rolePlayers = new();

        private List<Player> _playersToAssign = new();

        public RolesAvailable RolesAvailable => _rolesAvailable;

        #region Setup

        public override void OnStartServer()
        {
            base.OnStartServer();

            AddHandle(RoundStateUpdated.AddListener(HandleRoundStateUpdated));
        }

        [Server]
        private void HandleRoundStateUpdated(ref EventContext context, in RoundStateUpdated e)
        {
            if (e.RoundState != RoundState.WarmingUp) return;

            AssignRolesToPlayers();
        }

        #endregion

        #region Role Assignment
        
        /// <summary>
        /// Assign roles to every ready player
        /// </summary>
        [Server]
        private void AssignRolesToPlayers()
        {
            CharacterSubSystem characterSubSystem = SubSystems.Get<CharacterSubSystem>();
            Dictionary<Player, CharacterProfile> characters = characterSubSystem.Characters;

            GetAvailableRoles();
            // create crew manifest here
            GetPlayerRolePreferences(characters);
            AssignPriorityRoles();
            AssignRoles();
            AssignOverflow(characters);

            // cleanup rolecounters?
            _roleCounters.Clear();
            _playersToAssign.Clear();
        }

        /// <summary>
        /// If there are any players left without a job then assign them assistant or send them back to the lobby
        /// </summary>
        [Server]
        private void AssignOverflow(Dictionary<Player, CharacterProfile> characters)
        {
            ReadyPlayersSubSystem readyPlayers = SubSystems.Get<ReadyPlayersSubSystem>();

            foreach (Player player in _playersToAssign)
            {
                if (characters[player].OverFlowRole)
                {
                    Log.Information(this, "Assigning overflow role: " + _rolesAvailable.OverFlowRole.name);
                    _rolePlayers.Add(player, _rolesAvailable.OverFlowRole);
                    return;
                }

                readyPlayers.RemoveReadyPlayer(player);
            }
        }

        /// <summary>
        /// Assign the regular roles in a random order
        /// </summary>
        [Server]
        private void AssignRoles()
        {
            List<RoleCounter> list = _roleCounters.Values.ToList();

            list.OrderBy(Rx => Random.value);

            foreach (RoleCounter roleCounter in list)
            {
                roleCounter.AssignPlayers();
                _roleCounters.Remove(roleCounter.Role.name);
            }
            
        }

        /// <summary>
        /// Assign priority roles like captain even if nobody has them selected
        /// </summary>
        [Server]
        private void AssignPriorityRoles()
        {
            if (_rolesAvailable.PriorityRoles.Count == 0) return;

            foreach (RoleData role in _rolesAvailable.PriorityRoles)
            {
                if (role == null) continue;
                RoleCounter roleCounter = _roleCounters[role.name];

                if ((!roleCounter.AssignPlayers()) && _playersToAssign.Count > 0)
                {
                    // nobody had the role set so we force a random player
                    
                    int i = Random.Range(0, _playersToAssign.Count - 1);

                    Player player = _playersToAssign[i];
                    Log.Information(this, "Required role: " + role.name + " force assigning player: " + player.Ckey);
                    roleCounter.AddPlayer(player);
                }
                
                // not sure if i need to do this
                _roleCounters.Remove(roleCounter.Role.name);
            }
        }

        /// <summary>
        /// Fill the role counters with players based on their preferred priority
        /// </summary>
        [Server]
        private void GetPlayerRolePreferences(Dictionary<Player, CharacterProfile> characters)
        {
            foreach (KeyValuePair<Player, CharacterProfile> pair in characters)
            {
                Player player = pair.Key;
                
                _playersToAssign.Add(player);

                Dictionary<string, RolePriority> jobPrefs = pair.Value.Roles;
                
                foreach (KeyValuePair<string, RolePriority> job in jobPrefs)
                {
                    if (!_roleCounters.TryGetValue(job.Key, out RoleCounter roleCounter)) continue;
                    
                    switch (job.Value)
                    {
                        case RolePriority.High:
                            roleCounter.High.Add(player);
                            break;
                        case RolePriority.Medium:
                            roleCounter.Medium.Add(player);
                            break;
                        case RolePriority.Low:
                            roleCounter.Low.Add(player);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Get all roles in the current AvailableRoles class and sets up
        /// the Role Counters for them
        /// </summary>
        [Server]
        private void GetAvailableRoles()
        {
            if (_rolesAvailable == null)
            {
                Log.Error(this, "Initial Available Roles not set!");
            }

            foreach (DepartmentsData department in _rolesAvailable.Departments)
            {
                if (department == null) continue;
                
                foreach (RolesData role in department.Roles)
                {
                    if (role == null) continue;
                    RoleCounter roleCounter = new RoleCounter();
                    roleCounter.Role = role.Data;
                    roleCounter.AvailableRoles = role.AvailableRoles;

                    roleCounter.rolePlayers = _rolePlayers;
                    roleCounter.playersToAssign = _playersToAssign;

                    _roleCounters.Add(role.Data.name, roleCounter);
                }
            }
        }

        #endregion

        #region Player spawning

        /// <summary>
        /// Checks the role of the player and spawns his items
        /// </summary>
        /// <param name="entity">The player that will receive the items</param>
        [ServerRpc(RequireOwnership = false)]
        public void GiveRoleLoadoutToPlayer(Entity entity)
        {
            if (!_rolePlayers.TryGetValue(entity.Mind.player, out RoleData role))
            {
                role = _rolesAvailable.OverFlowRole;
            }

            // TODO: add embark job selection screen

            Log.Information(this, entity.Ckey + " embarked with role " + role.Name);
            SpawnIdentificationItems(entity, role);

            if (role.Loadout != null)
            {
                SpawnLoadoutItems(entity, role.Loadout);
            }
        }

        /// <summary>
        /// Spawn the player's PDA and IDCard with the proper permissions
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="role"></param>
        private void SpawnIdentificationItems(Entity entity, RoleData role)
        {
            ItemSubSystem itemSystem = SubSystems.Get<ItemSubSystem>();
            HumanInventory inventory = entity.GetComponent<HumanInventory>();

            if (!inventory.TryGetTypeContainer(ContainerType.Identification, 0, out AttachedContainer container)) return;

            Item pdaItem = itemSystem.SpawnItemInContainer(role.PDAPrefab, container);
            Item idCardItem = itemSystem.SpawnItem(role.IDCardPrefab.name, Vector3.zero, Quaternion.identity);

            PDA pda = (PDA)pdaItem;
            IDCard idCard = (IDCard)idCardItem;

            // Set up ID Card data
            string name = entity.GetComponent<UniqueIdentifiers>().Name;
            idCard.OwnerName = name;
            idCard.RoleName = role.Name;
            foreach (IDPermission permission in role.Permissions)
            {
                idCard.AddPermission(permission);
                Log.Information(this, "Added " + permission.Name + " permission to IDCard of " + name);
            }

            pda.StartingIDCard = idCardItem;
        }

        /// <summary>
        /// Spawn all the role items for the player
        /// </summary>
        /// <param name="entity">The player that will receive the items</param>
        /// <param name="loadout">The loadout of items he will receive</param>
        private void SpawnLoadoutItems(Entity entity, RoleLoadout loadout)
        {
            Hands hands = entity.GetComponent<Hands>();
            HumanInventory inventory = entity.GetComponent<HumanInventory>();

            Dictionary<ContainerType, AttachedContainer> containers = new Dictionary<ContainerType, AttachedContainer>();
            List<AttachedContainer> handContainers = hands.HandContainers;

            foreach (AttachedContainer inventoryContainer in inventory.Containers)
            {
                if (inventoryContainer.ContainerType == ContainerType.Hand)
                {
                    continue;
                }

                containers.Add(inventoryContainer.ContainerType, inventoryContainer);
            }

            foreach (KeyValuePair<ContainerType, GameObject> itemToEquip in loadout.Equipment)
            {
                if (itemToEquip.Value == null)
                {
                    continue;
                }

                if (containers.TryGetValue(itemToEquip.Key, out AttachedContainer container))
                {
                    SpawnItemInSlot(itemToEquip.Value, true, container);
                }
            }

            if (loadout.HandLeft != null)
            {
                SpawnItemInSlot(loadout.HandLeft, true, handContainers[0]);
            }

            if (loadout.HandRight != null)
            {
                SpawnItemInSlot(loadout.HandRight, true, handContainers[1]);
            }

            inventory.TriggerInventorySetup();
        }

        /// <summary>
        /// Spawns an item inside a container slot after checking for boolean
        /// </summary>
        /// <param name="itemId">The id of the item to be spawned</param>
        /// <param name="shouldSpawn">Condition indicating if the item should be spawned</param>
        /// <param name="container">Container the item will be spawned in</param>
        private void SpawnItemInSlot(GameObject itemId, bool shouldSpawn, AttachedContainer container)
        {
            if (!shouldSpawn)
            {
                return;
            }

            ItemSubSystem itemSystem = SubSystems.Get<ItemSubSystem>();
            itemSystem.SpawnItemInContainer(itemId, container);
        }

        #endregion

    }
}