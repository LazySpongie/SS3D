using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS3D.Systems.Inventory.Items;

namespace SS3D.Systems.Inventory.Clothing
{
    /// <summary>
    /// Used to store meshes for different item states
    /// </summary>
    [CreateAssetMenu(menuName = "Inventory/Items/ClothingItemVisualData", fileName = "ClothingItemVisualData")]
    public class ClothingItemVisualData : ItemVisualData
    {
        [Header("Clothing Models")]

        [Tooltip("Mesh displayed on player model.")]
        public Mesh ClothingModel;

        [Tooltip("Right handed mesh for shoes, headsets, gloves.")]
        public Mesh AltClothingModel;

        // TODO: Add blend shapes

        [Header("Culling")]
        [Tooltip("Clothing slots that should be hidden when this is worn.")]
        public ClothingItemCullingData CullingData;

        [Tooltip("Right handed culling data.")]
        public ClothingItemCullingData AltCullingData;
    }
}