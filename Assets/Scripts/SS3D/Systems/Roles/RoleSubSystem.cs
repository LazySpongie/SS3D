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

        // stores every role, its available slots, and which characters are assigned to them
        private Dictionary<string, RoleCounter> _crewManifest = new();
        
        // stores Players and their roles, to avoid issues if multiple characters share the same name
        private Dictionary<Player, RoleData> _rolePlayers = new();

        private List<Player> _playersToAssign = new();

        private List<Player> _overflowPlayers = new();

        public RolesAvailable RolesAvailable => _rolesAvailable;

        public ReadOnlyDictionary<string, RoleCounter> CrewManifest => new ReadOnlyDictionary<string, RoleCounter>(_crewManifest);

        public ReadOnlyDictionary<Player, RoleData> RolePlayers => new ReadOnlyDictionary<Player, RoleData>(_rolePlayers);

        #region Role Assignment

        /// <summary>
        /// Assign roles to every ready player
        /// </summary>
        [Server]
        public void AssignRolesToInitialPlayers()
        {
            // assign major antagonists here
            CreateCrewManifest();
            GetPlayerRolePreferences();
            AssignPriorityRoles();
            // ai and a random command role are picked here?
            AssignRoles();
            AssignOverflow();
            _playersToAssign.Clear();
        }

        /// <summary>
        /// If there are any players left without a job then assign them assistant or send them back to the lobby
        /// </summary>
        [Server]
        private void AssignOverflow()
        {
            if (_playersToAssign.Count == 0) return;
            
            foreach (Player player in _playersToAssign)
            {
                if (_overflowPlayers.Contains(player))
                {
                    Log.Information(this, "Assigning overflow role: " + _rolesAvailable.OverFlowRole.name);
                    AddPlayerToRole(player, _crewManifest[_rolesAvailable.OverFlowRole.name]);
                }
            }

            _overflowPlayers.Clear();
        }

        /// <summary>
        /// Assign the regular roles in a random order
        /// </summary>
        [Server]
        private void AssignRoles()
        {
            if (_playersToAssign.Count == 0) return;

            List<RoleCounter> list = _crewManifest.Values.ToList();

            // list.OrderBy(Rx => Random.value);
            list = RandomiseRoleList(list);

            // loop through every job descending from high priority to low priority players
            foreach (RolePriority priority in Enum.GetValues(typeof(RolePriority)))
            {
                bool allowed =  priority == RolePriority.High || 
                                priority == RolePriority.Medium ||
                                priority == RolePriority.Low;
                if (!allowed) continue;

                foreach (RoleCounter roleCounter in list)
                {
                    if (_playersToAssign.Count == 0) return;
                    roleCounter.AssignPlayersByPriority(priority);
                }
            }
        }

        private List<RoleCounter> RandomiseRoleList(List<RoleCounter> list)
        {
            List<RoleCounter> listCopy = new List<RoleCounter>(list);
            List<RoleCounter> result = new();
            while (listCopy.Count != 0)
            {
                int rand = Random.Range(0, listCopy.Count);
                result.Add(listCopy[rand]);
                listCopy.RemoveAt(rand);
            }
            return result;
        }

        /// <summary>
        /// Assign priority roles like captain even if nobody has them selected
        /// </summary>
        [Server]
        private void AssignPriorityRoles()
        {
            if (_playersToAssign.Count == 0) return;
            if (_rolesAvailable.PriorityRoles.Count == 0) return;

            foreach (RoleData role in _rolesAvailable.PriorityRoles)
            {
                if (role == null) continue;
                RoleCounter roleCounter = _crewManifest[role.name];

                if (!roleCounter.AssignAnyPlayers())
                {
                    // nobody had the role set so we force a random player
                    
                    int i = Random.Range(0, _playersToAssign.Count - 1);

                    Player player = _playersToAssign[i];
                    Log.Information(this, "Required role: " + role.name + " force assigning player: " + player.Ckey);

                    AddPlayerToRole(player, roleCounter);
                }
            }
        }

        /// <summary>
        /// Fill the role counters with players based on their preferred priority
        /// </summary>
        [Server]
        private void GetPlayerRolePreferences()
        {
            foreach (KeyValuePair<Player, CharacterProfile> pair in SubSystems.Get<CharacterSubSystem>().InitialCharacterProfiles)
            {
                Player player = pair.Key;
                
                _playersToAssign.Add(player);

                if (pair.Value.OverflowRole) _overflowPlayers.Add(player);

                Dictionary<string, RolePriority> jobPrefs = pair.Value.Roles;
                
                foreach (KeyValuePair<string, RolePriority> job in jobPrefs)
                {
                    if (!_crewManifest.TryGetValue(job.Key, out RoleCounter roleCounter)) continue;

                    bool overflow = job.Key == _rolesAvailable.OverFlowRole.name;
                    switch (job.Value)
                    {
                        case RolePriority.High:
                            roleCounter.High.Add(player);
                            break;
                        case RolePriority.Medium:
                            if (overflow) break;
                            roleCounter.Medium.Add(player);
                            break;
                        case RolePriority.Low:
                            if (overflow) break;
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
        private void CreateCrewManifest()
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
                    roleCounter.roleSubSystem = this;

                    // _crewManifest.Add(role.Data.name, roleCounter);
                    _crewManifest[role.Data.name] = roleCounter;
                }
            }
        }

        /// <summary>
        /// Called by a RoleCounter, sets a player as having been assigned a role
        /// </summary>
        [Server]
        public void AddPlayerToRole(Player player, RoleCounter rc)
        {
            if (_playersToAssign.Count == 0) return;
            if (_rolePlayers.ContainsKey(player)) return;

            // the ingamecharacter will be added to the manifest later so we just add them to roleplayers

            _rolePlayers.Add(player, rc.Role);
            _playersToAssign.Remove(player);
        }
        
        /// <summary>
        /// Adds a character into the crew manifest
        /// </summary>
        [Server]
        public void AddCharacterToCrewManifest(InGameCharacter character, RoleData role)
        {
            _crewManifest[role.name].AddCharacter(character);
        }

        [Server]
        public void ClearRolePlayers()
        {
            _rolePlayers.Clear();
        }

        #endregion

        #region Getters

        public RoleData GetPlayerRole(Player player)
        {
            return _rolePlayers[player];
        }

        public RoleData GetCharacterRole(InGameCharacter character)
        {
            foreach (KeyValuePair<string, RoleCounter> pair in _crewManifest)
            {
                if (pair.Value.Characters.Contains(character))
                {
                    return pair.Value.Role;
                }
            }

            return null;
        }

        public RoleData GetOverflowRole()
        {
            return _crewManifest[_rolesAvailable.OverFlowRole.name].Role;
        }

        #endregion

        #region Player spawning

        /// <summary>
        /// Checks the role of the player and spawns his items
        /// </summary>
        /// <param name="entity">The player that will receive the items</param>
        [Server]
        public void GiveRoleLoadoutToPlayer(Entity entity, RoleData role)
        {

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
            idCard.OwnerName = entity.GetComponent<UniqueIdentifiers>().Name;

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