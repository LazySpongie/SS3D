using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS3D.Systems.Inventory.Items;

namespace SS3D.Systems.Inventory.Clothing
{
    /// <summary>
    /// Data for clothing items to decide what models are shown on the player as well as what bodyparts will be hidden
    /// </summary>
    [CreateAssetMenu(menuName = "Inventory/Items/ClothingVisualData", fileName = "ClothingVisualData")]
    public class ClothingVisualData : ScriptableObject
    {
        [Header("Materials")]
        public Material[] Materials;

        [Header("Clothing Models")]
        public SpeciesClothingData Human;

        // Blend shapes applied to the clothing model here?

        [Header("Culling")]
        [Tooltip("Clothing and bodyparts that should be hidden when this is worn.")]
        public ClothingCullingData CullingData;
        
        [Tooltip("Right handed culling data for shoes, headsets, gloves.")]
        public ClothingCullingData AltCullingData;

        /// <summary>
        /// Struct used to hold meshes for a particular species.
        /// Future proofing.
        /// </summary>
        [System.Serializable]
        public struct SpeciesClothingData
        {
            [Tooltip("Mesh displayed on player model.")]
            public Mesh ClothingModel;

            [Tooltip("Right handed mesh for shoes, headsets, gloves.")]
            public Mesh AltClothingModel;

            public readonly bool Exists => ClothingModel != null || AltClothingModel != null;
        }
    }
}