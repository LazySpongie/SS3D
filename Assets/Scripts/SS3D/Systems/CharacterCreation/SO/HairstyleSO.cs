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
    /// SO for hairstyles and facial hair
    /// </summary>
    [CreateAssetMenu(menuName = "SS3D/Customization/Hairstyle", fileName = "Hairstyle")]
    public class HairstyleSO : CustomizationSO
    {

        [Header("Models")]

        [Tooltip("Mesh displayed on player model.")]
        public Mesh HairModel;
        
    }
}