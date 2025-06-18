using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS3D.Systems.Inventory.Items;

namespace SS3D.Systems.Inventory.Clothing
{
    /// <summary>
    /// Data for clothing items to decide what models are shown on the player as well as what bodyparts will be hidden
    /// </summary>
    [CreateAssetMenu(menuName = "Inventory/Items/ClothingItemVisualData", fileName = "ClothingItemVisualData")]
    public class ClothingItemVisualData : ItemVisualData
    {
        [Header("Clothing Models")]

        [Tooltip("Mesh displayed on player model.")]
        public Mesh ClothingModel;

        [Tooltip("Right handed mesh for shoes, headsets, gloves.")]
        public Mesh AltClothingModel;

        // Blend shapes applied to the clothing model here?


        [Header("Culling")]
        [Tooltip("Clothing and bodyparts that should be hidden when this is worn.")]
        public ClothingItemCullingData CullingData;
        
        [Tooltip("Right handed culling data for shoes, headsets, gloves.")]
        public ClothingItemCullingData AltCullingData;

    }
}