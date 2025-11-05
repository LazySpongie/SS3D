using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SS3D.Systems.Inventory.Items
{
    /// <summary>
    /// Data used to decide what materials and meshes are used for an item. 
    /// This should only be used for items that need to be able to change visuals during runtime such as from chameleon jumpsuits
    /// This feature will break if an item prefab uses multiple meshes like toolboxes (body and lid are separate and animated)
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