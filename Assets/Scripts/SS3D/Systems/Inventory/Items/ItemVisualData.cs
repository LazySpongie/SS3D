using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SS3D.Systems.Inventory.Items
{
    /// <summary>
    /// Used to store meshes for different item states
    /// </summary>
    [CreateAssetMenu(menuName = "Inventory/Items/ItemVisualData", fileName = "ItemVisualData")]
    public class ItemVisualData : ScriptableObject
    {
        public Material[] Materials;

        [Header("Item Models")]
        public Mesh DefaultModel;
        
        public Mesh HandModel;
    }
}