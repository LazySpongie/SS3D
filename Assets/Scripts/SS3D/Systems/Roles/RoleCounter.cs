using SS3D.Logging;
using SS3D.Systems.Entities;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;
using SS3D.Systems.Characters.Preferences;
using SS3D.Systems.Characters;
using UnityEngine;

namespace SS3D.Systems.Roles
{
    /// <summary>
    /// A counter of how many players there are in a Role and how many slots are left
    /// </summary>
    public class RoleCounter
    {
        public RoleData Role;
        public int CurrentRoles;
        public int AvailableRoles;
        public List<InGameCharacter> Characters = new();

        // players with these as their job priority
        public List<Player> Low = new();
        public List<Player> Medium = new();
        public List<Player> High = new();

        public Dictionary<Player, RoleData> rolePlayers;

        public RoleSubSystem roleSubSystem;


        /// <summary>
        /// Pick random players for this role from any priority preference (High to Low)
        /// </summary>
        public bool AssignAnyPlayers()
        {
            // TODO: check if they are an antagonist first
            
            bool hasAssignedAllRoles = 
                AddRandomPlayers(High) ||
                AddRandomPlayers(Medium) ||
                AddRandomPlayers(Low);

            CleanupLists();

            return hasAssignedAllRoles;
        }

        /// <summary>
        /// Pick random players for this role from a specific priority level 
        /// </summary>
        public void AssignPlayersByPriority(RolePriority priority)
        {
            // TODO: check if they are an antagonist first

            List<Player> list = new();

            switch (priority)
            {
                case RolePriority.High:
                    list = High;
                    break;
                case RolePriority.Medium:
                    list = Medium;
                    break;
                case RolePriority.Low:
                    list = Low;
                    break;
            }

            AddRandomPlayers(list);
            list.Clear();
        }

        /// <summary>
        /// Assign the role to random players from a priority pool
        /// </summary>
        private bool AddRandomPlayers(List<Player> list)
        {
            if (list.Count == 0) return false;
            if (AvailableRoles == 0) return false;

            while (CurrentRoles < AvailableRoles)
            {
                int i = Random.Range(0, list.Count - 1);
                Player player = list[i];
                if (rolePlayers.ContainsKey(player))
                {
                    // player already has a role
                    list.RemoveAt(i);
                    continue;
                }

                list.RemoveAt(i);
                roleSubSystem.AddPlayerToRole(player, this);
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Cleanup the lists after we're done
        /// </summary>
        private void CleanupLists()
        {
            Low.Clear();
            Medium.Clear();
            High.Clear();
        }
    }
}