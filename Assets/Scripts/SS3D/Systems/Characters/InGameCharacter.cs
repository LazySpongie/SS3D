using Coimbra;
using SS3D.Systems.Characters.Preferences;
using SS3D.Systems.Entities;
using SS3D.Systems.Inventory.Containers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS3D.Systems.Characters
{

    /// <summary>
    /// Stores information about each character that exists in the round.
    /// </summary>
    [Serializable]
    public class InGameCharacter
    {
        /// <summary>
        /// Unique identifier for this character and their entity.
        /// </summary>
        public int ID;

        /// <summary>
        /// The character name and then their ID.
        /// ie, John Beep (23)
        /// </summary>
        public string CombinedID;

        /// <summary>
        /// The player who this character belongs to. Not necessarily the player controlling the entity (mind swapping)
        /// </summary>
        public Player Player;

        /// <summary>
        /// The in-game name of this character
        /// </summary>
        public string Name;

        /// <summary>
        /// The entity assigned to this character
        /// </summary>
        public Entity Entity;

        public InGameCharacter(int id, Player player, string name)
        {
            ID = id;
            Player = player;
            Name = name;
            CombinedID = Name + " (" + ID + ")";
        }

        // TODO: add setters here so admins could change a players name and stuff like that 
    }
}