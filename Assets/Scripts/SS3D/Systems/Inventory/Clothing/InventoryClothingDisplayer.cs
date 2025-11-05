using System.Collections.Generic;
using System.Linq;
using SS3D.Core.Behaviours;
using SS3D.Logging;
using SS3D.Systems.Inventory.Items;
using SS3D.Systems.Inventory.Containers;
using UnityEngine;
using FishNet.Object.Synchronizing;
using FishNet.Object;

namespace SS3D.Systems.Inventory.Clothing
{
    /// <summary>
    /// Get equipped items from a characters inventory and send them to ClothingVisualDisplayer to be displayed.
    /// </summary>
    public class InventoryClothingDisplayer : NetworkActor
    {

        /// <summary>
        /// Synced list of clothing to be displayed on the player.
        /// </summary>
        [SyncObject]
        private readonly SyncList<DisplayedClothing> _displayedClothingList = new SyncList<DisplayedClothing>();

        /// <summary>
        /// The inventory containing the players clothing slots.
        /// </summary>
        [SerializeField]
        private HumanInventory _inventory;

        /// <summary>
        /// Handles displaying the clothing.
        /// </summary>
        [SerializeField]
        private ClothingVisualDisplayer _clothingVisualDisplayer;

        public override void OnStartServer()
        {
            base.OnStartServer();
            _inventory.OnContainerContentChanged += HandleContainerContentChanged;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _displayedClothingList.OnChange += ClothingVisualSlotsOnChange;
        }

        /// <summary>
        /// Client updates displayed clothes when the syncList of clothing is changed by the server.
        /// </summary>
        [Client]
        private void ClothingVisualSlotsOnChange(SyncListOperation op, int index, DisplayedClothing oldData, DisplayedClothing newData, bool asServer)
        {
            // if (asServer) return;

            // Log.Debug(this, $"ClothingVisualSlotsOnChange");

            switch (op)
            {
                // Show the new cloth on the player
                case SyncListOperation.Add:
                    _clothingVisualDisplayer.AddItem(newData.ClothingSlotType, newData.ItemVisualData);
                    break;

                // Stop displaying cloth on the player
                case SyncListOperation.RemoveAt:
                    _clothingVisualDisplayer.RemoveItem(oldData.ClothingSlotType);
                    break;
            }
        }
        
		/// <summary>
        /// Server adds equipped clothing items to a synclist for the client to then display.
        /// </summary>
        [Server]
        public void HandleContainerContentChanged(AttachedContainer container, Item oldItem, Item newItem, ContainerChangeType type)
        {
            ClothingContainer clothingContainer = container.GetComponent<ClothingContainer>();
            if (clothingContainer == null) return;

            // Log.Debug(this, $"HandleContainerContentChanged {newItem}");

            switch (type)
            {
                case ContainerChangeType.Add:
					AddClothingItem(clothingContainer.ClothingSlotType, newItem);
                    break;
                    
                case ContainerChangeType.Remove:
					RemoveClothingItem(clothingContainer.ClothingSlotType, oldItem);
                    break;
            }
        }

        /// <summary>
        /// Adds an item to be displayed.
        /// </summary>
        [Server]
        private void AddClothingItem(ClothingSlotType clothingSlotType, Item item)
        {
            if (item == null || item.ItemVisualData == null) return;

            // Need to convert item visual to its ID so it can be sent over network
            string itemVisualDataName = item.ItemVisualData.name;

            _displayedClothingList.Add(new DisplayedClothing(clothingSlotType, itemVisualDataName));
        }

        /// <summary>
        /// Removes an item from being displayed.
        /// </summary>
        [Server]
        private void RemoveClothingItem(ClothingSlotType clothingSlotType, Item item)
        {
            if (item == null) return;

            // ClothType itemClothType = cloth.Type;
            DisplayedClothing clothData = _displayedClothingList.Find(
                x => x.ClothingSlotType == clothingSlotType);
            
            _displayedClothingList.Remove(clothData);
        }
    }
}