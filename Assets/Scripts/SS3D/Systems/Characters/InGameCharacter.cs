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
    /// Stores information about each character that exists in the round
    /// </summary>
    public class InGameCharacter
    {
        public Player Player;

        public string Name;

        public Entity Entity;

        public CharacterProfile Profile;

        public InGameCharacter(Player player, string name, CharacterProfile profile)
        {
            Player = player;
            Name = name;
            Profile = profile;
            // Entity = entity;
        }
    }
}