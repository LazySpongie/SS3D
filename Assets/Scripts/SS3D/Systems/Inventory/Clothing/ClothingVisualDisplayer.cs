using System.Collections.Generic;
using System.Linq;
using SS3D.Core.Behaviours;
using SS3D.Logging;
using SS3D.Systems.Inventory.Items;
using SS3D.Systems.Inventory.Containers;
using SS3D.Systems.Health;
using UnityEngine;
using FishNet.Object.Synchronizing;
using FishNet.Object;

namespace SS3D.Systems.Inventory.Clothing
{
    /// <summary>
    /// Display clothes on player for all clients.
    /// </summary>
    public class ClothingVisualDisplayer : NetworkActor
    {
        /// <summary>
        /// A small structure containing information regarding clothes on player, to help syncing them over the network.
        /// For each bodypart that can have clothing, it also contains information on the item to display, if it should show or not.
        /// </summary>
        private struct ClothDisplayData
        {
            public NetworkObject _clothingVisualSlot;
            public Item _itemToDisplay;

            public ClothDisplayData(NetworkObject clothingVisualSlot, Item itemToDisplay)
            {
                _clothingVisualSlot = clothingVisualSlot;
                _itemToDisplay = itemToDisplay;
            }
        }

        /// <summary>
        /// Synced list of clothing slots and the items worn on them.
        /// </summary>
        [SyncObject]
        private readonly SyncList<ClothDisplayData> _clothDisplayDataList = new SyncList<ClothDisplayData>();

        /// <summary>
        /// The inventory containing the player's clothing slots.
        /// </summary>
        [SerializeField]
        private HumanInventory _inventory;
        
        /// <summary>
        /// All of the clothing slots that an item can be displayed on.
        /// </summary>
        [SerializeField]
        private ClothingVisualSlot[] _clothingVisualSlots;

        /// <summary>
        /// Health controller to access the body parts.
        /// </summary>
        [SerializeField]
        private HealthController _healthController;

        public ClothingVisualSlot[] ClothingVisualSlots => _clothingVisualSlots;

        public override void OnStartServer()
        {
            base.OnStartServer();
            _inventory.OnContainerContentChanged += HandleContainerContentChanged;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _clothDisplayDataList.OnChange += ClothingVisualSlotsOnChange;

            foreach (ClothingVisualSlot slot in _clothingVisualSlots)
            {
                slot.OnItemCullingChanged += ItemCullingOnChange;
            }
        }

        /// <summary>
        /// Use culling data provided by the item to set certain clothing slots invisible when equipped
        /// </summary>
        [Client]
        public void AddCulling(ClothingItemCullingData cullingData)
        {
            if (cullingData == null) return;

            SetCullingOnClothingVisualSlots(cullingData, true);

            SetCullingOnBodyParts(cullingData, true);
        }

		/// <summary>
        /// Use culling data provided by the item to set certain clothing slots visible when unequipped
        /// </summary>
        [Client]
        public void RemoveCulling(ClothingItemCullingData cullingData)
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
            foreach (ClothingVisualSlot clothingVisualSlot in ClothingVisualSlots)
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


		/// <summary>
        /// When the content of a container change, check if it should display or remove display of some clothes.
        /// </summary>
        [Server]
        public void HandleContainerContentChanged(AttachedContainer container, Item oldItem, Item newItem, ContainerChangeType type)
        {

            // this is running 2 times when a single item is equipped and i have no fucking idea why

            // If it's not a cloth type container.
            // It'd be probably better to just create "cloth container" inheriting from container to easily test that.
            if (container.GetComponent<ClothingContainer>() == null)
            {
                return;
            }

            // Log.Debug(this, $"HandleContainerContentChanged {newItem}");

            switch (type)
            {
                case ContainerChangeType.Add:
					AddCloth(container, newItem);
                    break;
                    
                case ContainerChangeType.Remove:
					RemoveCloth(container, oldItem);
                    break;
            }
        }

        /// <summary>
        /// Adds a cloth to the synced list, making a few checks to find where to add it, if possible.
        /// </summary>
        /// <param name="item"> The item to add, it should have a Cloth component on it.</param>
        [Server]
        private void AddCloth(AttachedContainer container, Item item)
        {
            if (item == null)
            {
                return;
            }

            ClothingSlotType clothingSlotType = container.gameObject.GetComponent<ClothingContainer>().ClothingSlotType;

            ClothingVisualSlot clothingVisualSlot = _clothingVisualSlots.
                Where(x => x.ClothingSlotType == clothingSlotType).First();

            // Something keeps calling HandleContainerContentChanged multiple times so this is a workaround
            if (clothingVisualSlot.HasItem) return;

            NetworkObject NetworkedClothingVisualSlot = clothingVisualSlot.gameObject.GetComponent<NetworkObject>();
            if (NetworkedClothingVisualSlot != null)
            {
                _clothDisplayDataList.Add(new ClothDisplayData(NetworkedClothingVisualSlot, item));
            }
        }

        /// <summary>
        /// Remove a cloth from the synced list, check if it's there before removing.
        /// </summary>
        /// <param name="item">The item to add, it should have a Cloth component on it.</param>
        [Server]
        private void RemoveCloth(AttachedContainer container, Item item)
        {
            if (item == null)
            {
                return;
            }

            // ContainerType containerType = container.Type;
            ClothingSlotType clothingSlotType = container.gameObject.GetComponent<ClothingContainer>().ClothingSlotType;

            // ClothType itemClothType = cloth.Type;
            ClothDisplayData clothData = _clothDisplayDataList.Find(
                x => x._clothingVisualSlot.gameObject.GetComponent<ClothingVisualSlot>().ClothingSlotType == clothingSlotType);
            
            _clothDisplayDataList.Remove(clothData);
        }
        
        /// <summary>
        /// Callback when the syncedList _clothDisplayDataList changes. Update the displayed clothes on the player.
        /// </summary>
        private void ClothingVisualSlotsOnChange(SyncListOperation op, int index, ClothDisplayData oldData, ClothDisplayData newData, bool asServer)
        {
            if (asServer) return;

            // this is running 4 times when a single item is equipped and i have no fucking idea why

            // Log.Debug(this, $"ClothingVisualSlotsOnChange");
            
            switch (op)
            {
                // Show the new cloth on the player
                case SyncListOperation.Add:

                    Item newItem = newData._itemToDisplay;

                    ClothingVisualSlot newSlot = newData._clothingVisualSlot.GetComponent<ClothingVisualSlot>();
                    if (newSlot.HasItem) break;

                    newSlot.SetItem(newItem);

                    ClothingItemCullingData newCullingData = newSlot.CullingData;
                    AddCulling(newCullingData);

                    break;

                // Stop displaying cloth on the player
                case SyncListOperation.RemoveAt:

                    
                    ClothingVisualSlot oldSlot = oldData._clothingVisualSlot.GetComponent<ClothingVisualSlot>();
                    if (!oldSlot.HasItem) break;

                    ClothingItemCullingData oldCullingData = oldSlot.CullingData;
                    RemoveCulling(oldCullingData);

                    oldSlot.RemoveItem();

                    break;
            }
        }

        /// <summary>
        /// Callback when an item's visual is changed 
        /// </summary>
        [Client]
        private void ItemCullingOnChange(ClothingVisualSlot slot, ClothingItemCullingData oldData, ClothingItemCullingData newData)
        {
            RemoveCulling(oldData);
            AddCulling(newData);
        }
    }
}