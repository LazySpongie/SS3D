using System.Collections.Generic;
using System.Linq;
using SS3D.Core.Behaviours;
using SS3D.Logging;
using SS3D.Systems.Inventory.Items;
using SS3D.Systems.Health;
using SS3D.Data;
using UnityEngine;
using FishNet.Object;
using SS3D.Systems.Characters;
using SS3D.Systems.Inventory.Containers;

namespace SS3D.Systems.Inventory.Clothing
{

    /// <summary>
    /// Client script that displays clothing items on the player
    /// </summary>
    public class ClothingVisualDisplayer : Actor
    {
        /// <summary>
        /// The root containing all the clothing slots.
        /// </summary>
        [SerializeField]
        private Transform _clothingSlotsRoot;

        private ClothingVisualSlot[] _clothingVisualSlots;

        /// <summary>
        /// Health controller to access the body parts so they can be hidden to avoid clipping.
        /// </summary>
        [SerializeField]
        private Transform _bodyPartRoot;

        /// <summary>
        /// AppearanceDisplay to hide hairstyles when hats are worn.
        /// </summary>
        [SerializeField]
        private AppearanceDisplayer _appearanceDisplayer;

        protected override void OnAwake()
        {
            base.OnAwake();
            _clothingVisualSlots = _clothingSlotsRoot.GetComponentsInChildren<ClothingVisualSlot>();
        }

		/// <summary>
        /// Add an item to be displayed.
        /// </summary>
        [Client]
        public void AddItem(ContainerType clothingSlotType, ItemVisualData itemVisualData)
        {
            ClothingVisualSlot newSlot = _clothingVisualSlots.
                Where(x => x.ClothingSlotType == clothingSlotType).First();

            if (newSlot.HasItem) return;

            newSlot.SetItem(itemVisualData);

            ClothingItemCullingData newCullingData = newSlot.CullingData;
            AddCulling(newSlot);
        }

		/// <summary>
        /// Add an item to be displayed.
        /// </summary>
        [Client]
        public void AddItem(ContainerType clothingSlotType, string itemVisualDataName)
        {
            ClothingVisualSlot newSlot = _clothingVisualSlots.
                Where(x => x.ClothingSlotType == clothingSlotType).First();

            if (newSlot.HasItem) return;

            ItemVisualData ItemVisualData = Assets.Get<ItemVisualData>("ItemVisuals", itemVisualDataName);
            newSlot.SetItem(ItemVisualData);

            ClothingItemCullingData newCullingData = newSlot.CullingData;
            AddCulling(newSlot);
        }

		/// <summary>
        /// Remove an item from being displayed.
        /// </summary>
        public void RemoveItem(ContainerType clothingSlotType)
        {
            ClothingVisualSlot oldSlot = _clothingVisualSlots.
                Where(x => x.ClothingSlotType == clothingSlotType).First();

            if (!oldSlot.HasItem) return;

            ClothingItemCullingData oldCullingData = oldSlot.CullingData;
            RemoveCulling(oldSlot);

            oldSlot.RemoveItem();
        }

        /// <summary>
        /// Use culling data provided by the item to set certain clothing slots invisible when equipped
        /// </summary>
        private void AddCulling(ClothingVisualSlot slot)
        {
            ClothingItemCullingData cullingData = slot.CullingData;
            if (cullingData == null) return;

            SetCullingOnClothingVisualSlots(slot, cullingData, true);

            SetCullingOnBodyParts(slot, cullingData, true);
            
            SetCullingOnAppearance(slot, cullingData, true);
        }

		/// <summary>
        /// Use culling data provided by the item to set certain clothing slots visible when unequipped
        /// </summary>
        private void RemoveCulling(ClothingVisualSlot slot)
        {
            ClothingItemCullingData cullingData = slot.CullingData;
            if (cullingData == null) return;

            SetCullingOnClothingVisualSlots(slot, cullingData, false);

            SetCullingOnBodyParts(slot, cullingData, false);
            
            SetCullingOnAppearance(slot, cullingData, false);
        }

        /// <summary>
        /// Get a body part that is referenced in the given culling data
        /// </summary>
        private void SetCullingOnClothingVisualSlots(ClothingVisualSlot slot, ClothingItemCullingData cullingData, bool addCulling)
        {
            foreach (ClothingVisualSlot clothingVisualSlot in _clothingVisualSlots)
            {
                if (cullingData.CulledClothingSlots.Contains(clothingVisualSlot.ClothingSlotType))
                {
                    if (addCulling)
                    {
                        clothingVisualSlot.RendererController?.AddCuller(slot.gameObject);
                    }
                    else
                    {
                        clothingVisualSlot.RendererController?.RemoveCuller(slot.gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Get a body part that is referenced in the given culling data
        /// </summary>
        private void SetCullingOnBodyParts(ClothingVisualSlot slot, ClothingItemCullingData cullingData, bool addCulling)
        {
            foreach (BodyPartRenderer bodyPart in _bodyPartRoot.GetComponentsInChildren<BodyPartRenderer>())
            {
                if (!cullingData.CulledBodyParts.Contains(bodyPart.Type)) continue;
                if (addCulling)
                {
                    bodyPart.AddCuller(slot.gameObject);
                }
                else
                {
                    bodyPart.RemoveCuller(slot.gameObject);
                }
            }
        }

        /// <summary>
        /// Set culling on hairstyles
        /// </summary>
        private void SetCullingOnAppearance(ClothingVisualSlot slot, ClothingItemCullingData cullingData, bool addCulling)
        {
            _appearanceDisplayer?.SetCullingOnAppearance(slot.gameObject, cullingData, addCulling);
        }

    }
}