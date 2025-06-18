using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SS3D.Systems.Inventory.Items
{
    /// <summary>
    /// Data used to decide what materials and meshes are used for an item. 
    /// Different item visual data can be assigned to an item during runtime such as from chameleon jumpsuits
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