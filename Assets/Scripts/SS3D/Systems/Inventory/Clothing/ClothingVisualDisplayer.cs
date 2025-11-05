using System.Collections.Generic;
using System.Linq;
using SS3D.Core.Behaviours;
using SS3D.Logging;
using SS3D.Systems.Inventory.Items;
using SS3D.Systems.Health;
using SS3D.Data;
using UnityEngine;
using FishNet.Object;

namespace SS3D.Systems.Inventory.Clothing
{

    /// <summary>
    /// Client script that displays clothing items on the player
    /// </summary>
    public class ClothingVisualDisplayer : Actor
    {
        /// <summary>
        /// The clothing slots an item can be displayed on.
        /// </summary>
        [SerializeField]
        private ClothingVisualSlot[] _clothingVisualSlots;

        /// <summary>
        /// Health controller to access the body parts so they can be hidden to avoid clipping.
        /// </summary>
        [SerializeField]
        private HealthController _healthController;

		/// <summary>
        /// Add an item to be displayed.
        /// </summary>
        [Client]
        public void AddItem(ClothingSlotType clothingSlotType, string itemVisualDataName)
        {
            ClothingVisualSlot newSlot = _clothingVisualSlots.
                Where(x => x.ClothingSlotType == clothingSlotType).First();

            if (newSlot.HasItem) return;

            ItemVisualData ItemVisualData = Assets.Get<ItemVisualData>("ItemVisuals", itemVisualDataName);
            newSlot.SetItem(ItemVisualData);

            ClothingItemCullingData newCullingData = newSlot.CullingData;
            AddCulling(newCullingData);
        }

		/// <summary>
        /// Remove an item from being displayed.
        /// </summary>
        [Client]
        public void RemoveItem(ClothingSlotType clothingSlotType)
        {
            ClothingVisualSlot oldSlot = _clothingVisualSlots.
                Where(x => x.ClothingSlotType == clothingSlotType).First();

            if (!oldSlot.HasItem) return;

            ClothingItemCullingData oldCullingData = oldSlot.CullingData;
            RemoveCulling(oldCullingData);

            oldSlot.RemoveItem();
        }

        /// <summary>
        /// Use culling data provided by the item to set certain clothing slots invisible when equipped
        /// </summary>
        [Client]
        private void AddCulling(ClothingItemCullingData cullingData)
        {
            if (cullingData == null) return;

            SetCullingOnClothingVisualSlots(cullingData, true);

            SetCullingOnBodyParts(cullingData, true);
        }

		/// <summary>
        /// Use culling data provided by the item to set certain clothing slots visible when unequipped
        /// </summary>
        [Client]
        private void RemoveCulling(ClothingItemCullingData cullingData)
        {
            if (cullingData == null) return;

            SetCullingOnClothingVisualSlots(cullingData, false);

            SetCullingOnBodyParts(cullingData, false);
        }

        /// <summary>
        /// Get a body part that is referenced in the given culling data
        /// </summary>
        [Client]
        private void SetCullingOnClothingVisualSlots(ClothingItemCullingData cullingData, bool addCulling)
        {
            foreach (ClothingVisualSlot clothingVisualSlot in _clothingVisualSlots)
            {
                if (cullingData.CulledClothingSlots.Contains(clothingVisualSlot.ClothingSlotType))
                {
                    if (addCulling)
                    {
                        clothingVisualSlot.Cullable?.AddCuller(gameObject);
                    }
                    else
                    {
                        clothingVisualSlot.Cullable?.RemoveCuller(gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Get a body part that is referenced in the given culling data
        /// </summary>
        [Client]
        private void SetCullingOnBodyParts(ClothingItemCullingData cullingData, bool addCulling)
        {
            foreach (BodyPart bodyPart in _healthController.BodyPartsOnEntity)
            {
                if (cullingData.CulledBodyParts.Contains(bodyPart.BodyPartType))
                {
                    if (addCulling)
                    {
                        bodyPart.GetComponent<Cullable>()?.AddCuller(gameObject);
                    }
                    else
                    {
                        bodyPart.GetComponent<Cullable>()?.RemoveCuller(gameObject);
                    }
                }
            }
        }
    }
}