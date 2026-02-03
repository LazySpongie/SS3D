using System.Linq;
using SS3D.Core.Behaviours;
using SS3D.Logging;
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

        [Client]
        protected override void OnAwake()
        {
            base.OnAwake();
            _clothingVisualSlots = _clothingSlotsRoot.GetComponentsInChildren<ClothingVisualSlot>();
        }

		/// <summary>
        /// Add an item to be displayed.
        /// </summary>
        [Client]
        public void AddItem(ContainerType clothingSlotType, ClothingVisualData clothingVisual)
        {
            ClothingVisualSlot slot = GetSlotFromContainerType(clothingSlotType);

            AddVisualToSlot(slot, clothingVisual);
        }

		/// <summary>
        /// Add an item to be displayed.
        /// </summary>
        [Client]
        public void AddItem(ContainerType clothingSlotType, string visualName)
        {
            ClothingVisualSlot slot = GetSlotFromContainerType(clothingSlotType);

            ClothingVisualData clothingVisual = Assets.Get<ClothingVisualData>("Clothing", visualName);

            if (clothingVisual == null) 
            {
                Log.Error(this, $"Clothing Visual Asset {visualName} is not found in the database.");
                return;
            }

            AddVisualToSlot(slot, clothingVisual);
        }

		/// <summary>
        /// Remove an item from being displayed.
        /// </summary>
        [Client]
        public void RemoveItem(ContainerType clothingSlotType)
        {
            ClothingVisualSlot oldSlot = _clothingVisualSlots.
                First(x => x.ClothingSlotType == clothingSlotType);

            if (!oldSlot.HasItem) return;

            ClothingCullingData oldCullingData = oldSlot.CullingData;
            RemoveCulling(oldSlot);

            oldSlot.RemoveItem();
        }

		/// <summary>
        /// Displays a clothing visual in a slot.
        /// </summary>
        [Client]
        private void AddVisualToSlot(ClothingVisualSlot slot, ClothingVisualData clothingVisual)
        {
            if (slot.HasItem) return;
            slot.SetItem(clothingVisual);
            AddCulling(slot);
        }

        /// <summary>
        /// Use culling data provided by the item to set certain clothing slots invisible when equipped
        /// </summary>
        [Client]
        private void AddCulling(ClothingVisualSlot slot)
        {
            ClothingCullingData cullingData = slot.CullingData;
            if (cullingData == null) return;

            SetCullingOnClothingVisualSlots(slot, cullingData, true);

            SetCullingOnBodyParts(slot, cullingData, true);
            
            SetCullingOnAppearance(slot, cullingData, true);
        }

		/// <summary>
        /// Use culling data provided by the item to set certain clothing slots visible when unequipped
        /// </summary>
        [Client]
        private void RemoveCulling(ClothingVisualSlot slot)
        {
            ClothingCullingData cullingData = slot.CullingData;
            if (cullingData == null) return;

            SetCullingOnClothingVisualSlots(slot, cullingData, false);

            SetCullingOnBodyParts(slot, cullingData, false);
            
            SetCullingOnAppearance(slot, cullingData, false);
        }

        /// <summary>
        /// Get a body part that is referenced in the given culling data
        /// </summary>
        [Client]
        private void SetCullingOnClothingVisualSlots(ClothingVisualSlot slot, ClothingCullingData cullingData, bool addCulling)
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
        [Client]
        private void SetCullingOnBodyParts(ClothingVisualSlot slot, ClothingCullingData cullingData, bool addCulling)
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
        [Client]
        private void SetCullingOnAppearance(ClothingVisualSlot slot, ClothingCullingData cullingData, bool addCulling)
        {
            _appearanceDisplayer?.SetCullingOnAppearance(slot.gameObject, cullingData, addCulling);
        }

		/// <summary>
        /// Get a ClothingVisualSlot from a ContainerType.
        /// </summary>
        [Client]
        private ClothingVisualSlot GetSlotFromContainerType(ContainerType clothingSlotType)
        {
            return _clothingVisualSlots.
                First(x => x.ClothingSlotType == clothingSlotType);
        }

    }
}