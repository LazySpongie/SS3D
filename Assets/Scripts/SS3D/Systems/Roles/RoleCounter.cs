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

        public RoleSubSystem roleSubSystem;

        public void AddCharacter(InGameCharacter character)
        {
            // if (!(CurrentRoles < AvailableRoles || AvailableRoles == 0)) return;
            CurrentRoles++; 
            Characters.Add(character);
        }

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

            // for assistant
            if (AvailableRoles == 0)
            {
                AddAllPlayers(list);
            }
            else
            {
                AddRandomPlayers(list);
            }

            list.Clear();
        }

        /// <summary>
        /// Assign the role to random players from a priority pool
        /// </summary>
        private bool AddRandomPlayers(List<Player> list)
        {
            if (list.Count == 0) return false;
            if (AvailableRoles == 0) return false;

            bool hasAssignedPlayers = false;
            int rolls = AvailableRoles - CurrentRoles;

            for (int i = 1; i < rolls; i++)
            {
                if (list.Count == 0) break;
                int rand = Random.Range(0, list.Count - 1);
                Player player = list[rand];
                
                list.RemoveAt(rand);
                roleSubSystem.AddPlayerToRole(player, this);
            }

            return hasAssignedPlayers;
        }

        /// <summary>
        /// Assign the role to random players from a priority pool
        /// </summary>
        private void AddAllPlayers(List<Player> list)
        {
            if (list.Count == 0) return;

            foreach (Player player in list)
            {
                roleSubSystem.AddPlayerToRole(player, this);
            }
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