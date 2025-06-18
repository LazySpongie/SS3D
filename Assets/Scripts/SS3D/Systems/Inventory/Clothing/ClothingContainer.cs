using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS3D.Core.Behaviours;

namespace SS3D.Systems.Inventory.Clothing
{
    /// <summary>
    /// Class to store information about how a clothing item equipped in the container should be displayed
    /// TODO: Make this inherit from attachedcontainer instead?
    /// </summary>
    public class ClothingContainer : Actor
    {
        [Tooltip("Set which ClothingVisualSlot the item will be displayed on.")]
        [SerializeField]
        private ClothingSlotType _clothingSlotType;

        /// <summary>
        /// Set which Clothing Visual Slot the item will be displayed on
        /// </summary>
        public ClothingSlotType ClothingSlotType => _clothingSlotType;
	}
}
