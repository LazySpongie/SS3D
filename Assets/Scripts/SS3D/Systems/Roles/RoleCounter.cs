using SS3D.Logging;
using SS3D.Systems.Entities;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

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
        public List<Player> Players = new();

        // players with these as their job priority
        public List<Player> Low = new();
        public List<Player> Medium = new();
        public List<Player> High = new();

        public Dictionary<Player, RoleData> rolePlayers;
        public List<Player> playersToAssign;

        public delegate void OnPlayerAssigned(Player player, RoleData role); 

        /// <summary>
        /// Assign a player to this role
        /// </summary>
        public void AddPlayer(Player player)
        {
            if (CurrentRoles < AvailableRoles || AvailableRoles == 0)
            {
                Log.Information(this, player.Ckey + " assigned role: " + Role.name);

                Players.Add(player);

                rolePlayers.Add(player, Role);
                playersToAssign.Remove(player);

                CurrentRoles++;
            }
        }

        /// <summary>
        /// Remove a player from this role
        /// </summary>
        public void RemovePlayer(Player player)
        {
            Players.Remove(player);
            CurrentRoles--;
        }


        /// <summary>
        /// Pick random players to assign this role based on their preferences 
        /// </summary>
        public bool AssignPlayers()
        {
            // TODO: check if they are an antagonist first
            if (playersToAssign.Count == 0) return false;
            
            bool hasAssignedAllRoles = 
                AssignRandomPlayers(High) ||
                AssignRandomPlayers(Medium) ||
                AssignRandomPlayers(Low);

            // _rolePlayers = null;

            CleanupLists();

            return hasAssignedAllRoles;
        }

        /// <summary>
        /// Assign the role to random players from the high, medium, or low priority pools
        /// </summary>
        private bool AssignRandomPlayers(List<Player> list)
        {
            if (list.Count == 0) return false;
            if (AvailableRoles == 0) return false;

            while (CurrentRoles < AvailableRoles)
            {
                int i = Random.Range(0, list.Count - 1);
                
                if (rolePlayers.ContainsKey(list[i]))
                {
                    // player already has role
                    list.RemoveAt(i);
                    continue;
                }
                AddPlayer(list[i]);
                list.RemoveAt(i);
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