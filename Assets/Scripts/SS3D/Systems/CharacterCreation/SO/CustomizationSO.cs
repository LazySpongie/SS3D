using System;
using Coimbra;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using SS3D.Core.Behaviours;
using SS3D.Systems.Entities.Events;
using SS3D.Systems.Entities.Humanoid;
using SS3D.Systems.Health;
using SS3D.Systems.Interactions;
using SS3D.Systems.Inventory.Containers;
using UnityEngine;

namespace SS3D.Systems.CharacterCreation
{
    
    /// <summary>
    /// Base SO for all customization options that can be selected in character creation (hairstyles, loadout item.)
    /// </summary>
    public class CustomizationSO : ScriptableObject
    {
        public string NameString;
        public Sprite icon;
        
    }
}