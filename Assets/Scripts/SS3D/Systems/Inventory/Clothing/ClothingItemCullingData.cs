using SS3D.Systems.Health;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS3D.Systems.Inventory.Clothing
{
    /// <summary>
    /// Data containing clothing slots and bodyparts that should be hidden when a clothing item is worn
    /// </summary>
    [CreateAssetMenu(menuName = "Inventory/Items/ClothingItemCullingData", fileName = "ClothingItemCullingData")]
    public class ClothingItemCullingData : ScriptableObject
    {
        /// <summary>
        /// The clothing slots that will be hidden
        /// </summary>
        public ClothingSlotType[] CulledClothingSlots;

        /// <summary>
        /// The body parts that will be hidden
        /// </summary>
        public BodyPartType[] CulledBodyParts;
        
        // Need to add data to decide if hair should be hidden or have corrective blendshapes applied here
    }
}