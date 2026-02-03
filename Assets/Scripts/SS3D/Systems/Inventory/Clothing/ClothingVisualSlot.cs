using UnityEngine;
using SS3D.Logging;
using SS3D.Systems.Inventory.Items;
using FishNet.Object;
using VisualSlot = SS3D.Systems.Characters.VisualSlot;
using SS3D.Systems.Inventory.Containers;

namespace SS3D.Systems.Inventory.Clothing
{
    /// <summary>
    /// Networked script that syncs clothing in the characters inventory and sends it to the ClothingVisualDisplayer to be displayed.
    /// </summary>
    /// 
    public class ClothingVisualSlot : VisualSlot
    {
        [Tooltip("Set which clothing container in the inventory is using this slot.")]
        [SerializeField]
        private ContainerType _clothingSlotType;

        [Tooltip("If this is a right-sided slot like right glove, right shoe.")]
        [SerializeField]
        private bool _useAltClothingModel;

        /// <summary>
        /// ClothingItemVisualData used in this slot
        /// </summary>
        private ClothingVisualData _visualData;

        /// <summary>
        /// ClothingItemCullingData used by this item
        /// </summary>
        private ClothingCullingData _cullingData;

        /// <summary>
        /// ClothingItemVisualData used in this slot
        /// </summary>
        public ClothingVisualData VisualData => _visualData;

        /// <summary>
        /// ClothingItemCullingData used by this item
        /// </summary>
        public ClothingCullingData CullingData => _cullingData;

        /// <summary>
        /// If there is an item displayed in this slot
        /// </summary>
        public bool HasItem => _visualData;

        /// <summary>
        /// Used to connect a clothing container in the inventory to this slot
        /// </summary>
        public ContainerType ClothingSlotType => _clothingSlotType;

        /// <summary>
        /// If this is a right-sided slot like right glove, right shoe
        /// </summary>
        public bool UseAltClothingModel => _useAltClothingModel;

        /// <summary>
        /// Assign a clothing item to be displayed on this slot.
        /// </summary>
        [Client]
        public void SetItem(ClothingVisualData data)
        {
            _visualData = data;
            SetupItem();
        }

        /// <summary>
        /// Remove the clothing item displayed on this slot.
        /// </summary>
        [Client]
        public void RemoveItem()
        {
            _visualData = null;
            _cullingData = null;
            RemoveClothingMesh();
            RendererController.SetHidden(true);
        }

        /// <summary>
        /// Set item data
        /// </summary>
        [Client]
        private void SetupItem()
        {
            SetClothingMesh();
            SetClothingCullingData();

            RendererController.SetHidden(false);
        }

        /// <summary>
        /// Find the mesh assigned to the item and use it 
        /// </summary>
        [Client]
        private void SetClothingMesh()
        {
            // In the future this needs to be changed to support species
            Mesh newMesh = _visualData.Human.ClothingModel;
            if (_useAltClothingModel & (_visualData.Human.AltClothingModel != null))
            {
                newMesh = _visualData.Human.AltClothingModel;
            }

            SetRendererMesh(newMesh);

            // This will need to be changed to the instanced materials of the item later
            SetRendererMaterials(_visualData.Materials);
        }

        /// <summary>
        /// Set mesh and materials to blank 
        /// </summary>
        [Client]
        private void RemoveClothingMesh()
        {
            SetRendererMesh(null);
            Material[] materials = { };
            SetRendererMaterials(materials);
        }

        /// <summary>
        /// Find the culling data that will be used for the item
        /// </summary>
        [Client]
        private void SetClothingCullingData()
        {
            // Gloves need to hide the correct hand
            ClothingCullingData cullingData = _visualData.CullingData;
            if (_useAltClothingModel & _visualData.AltCullingData != null)
            {
                cullingData = _visualData.AltCullingData;
            }

            _cullingData = cullingData;
        }
    }
}