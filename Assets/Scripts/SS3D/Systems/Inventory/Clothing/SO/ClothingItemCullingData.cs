using SS3D.Systems.Characters;
using SS3D.Systems.Health;
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

        [Header("Character Appearance")]
        public bool HideHair;
        public bool HideBeard;
        public bool HideEyebrows;
        public bool HideEyes;

        [Header("Corrective Blendshapes")]

        [Range(0f, 100f)]
        public float Hat;

        [Range(0f, 100f)]
        public float Helmet;

        [Range(0f, 100f)]
        public float Mask;

        [Range(0f, 100f)]
        public float Hood;
        
    }
}