
using SS3D.Systems.Inventory.Items;
using System;
using UnityEngine;

namespace SS3D.Systems.Inventory.Clothing
{

    /// <summary>
    /// A small structure containing information regarding clothes on player, to help syncing them over the network.
    /// For each bodypart that can have clothing, it also contains information on the item to display, if it should show or not.
    /// </summary>
    public readonly struct DisplayedClothing
    {
        public readonly ClothingSlotType ClothingSlotType;
        public readonly string ItemVisualData;

        public DisplayedClothing(ClothingSlotType clothingSlotType, string itemToDisplay)
        {
            ClothingSlotType = clothingSlotType;
            ItemVisualData = itemToDisplay;
        }
    }
}