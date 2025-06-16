using SS3D.Systems.Health;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS3D.Systems.Inventory.Clothing
{
    /// <summary>
    /// Used to store meshes for different item states
    /// </summary>
    [CreateAssetMenu(menuName = "Inventory/Items/ClothingItemCullingData", fileName = "ClothingItemCullingData")]
    public class ClothingItemCullingData : ScriptableObject
    {
        public ClothingSlotType[] CulledClothingSlots;
        
        public BodyPartType[] CulledBodyParts;
    }
}