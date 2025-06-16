using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS3D.Core.Behaviours;

namespace SS3D.Systems.Inventory.Clothing
{
    /// <summary>
    /// Simple class to mark that a container should be treated as the kind that can contain clothes only.
    /// </summary>
    /// 
    /// TODO: Make this inherit from attachedcontainer instead?
    public class ClothingContainer : Actor
    {
        [SerializeField]
        private ClothingSlotType _clothingSlotType;

        public ClothingSlotType ClothingSlotType => _clothingSlotType;
	}
}
