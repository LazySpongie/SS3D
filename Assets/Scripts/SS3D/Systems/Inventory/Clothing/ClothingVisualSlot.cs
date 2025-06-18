using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS3D.Logging;
using SS3D.Core.Behaviours;
using SS3D.Systems.Inventory.Items;
using FishNet.Object;

namespace SS3D.Systems.Inventory.Clothing
{
    /// <summary>
    /// Used to display clothing worn in a particular clothing slot.
    /// </summary>
    /// 
    public class ClothingVisualSlot : Actor
    {
        public delegate void ItemCullingEventHandler(ClothingVisualSlot clothingVisualSlot, ClothingItemCullingData oldData, ClothingItemCullingData newData);

        // When the visual of the item is changed
        public event ItemCullingEventHandler OnItemCullingChanged;

        [Tooltip("Set which clothing container in the inventory is using this slot.")]
        [SerializeField]
        private ClothingSlotType _clothingSlotType;

        [Tooltip("If this is a right-sided slot like right glove, right shoe.")]
        [SerializeField]
        private bool _useAltClothingModel;

        // Item displayed in this slot
        private Item _item;

        // ClothingItemVisualData used in this slot
        private ClothingItemVisualData _visualData;

        // ClothingItemCullingData used by this item
        private ClothingItemCullingData _cullingData;

        // Reference to the Cullable script on this object
        private Cullable _cullable;

        private bool _hasItem;

        private SkinnedMeshRenderer _renderer;

        /// <summary>
        /// Reference to the Cullable script on this object
        /// </summary>
        public Cullable Cullable => _cullable;

        /// <summary>
        /// Item displayed in this slot
        /// </summary>
        public Item Item => _item;
        
        /// <summary>
        /// ClothingItemVisualData used in this slot
        /// </summary>
        public ClothingItemVisualData VisualData => _visualData;
        
        /// <summary>
        /// ClothingItemCullingData used by this item
        /// </summary>
        public ClothingItemCullingData CullingData => _cullingData;

        /// <summary>
        /// If there is an item displayed in this slot
        /// </summary>
        public bool HasItem => _hasItem;

        /// <summary>
        /// Used to connect a clothing container in the inventory to this slot
        /// </summary>
        public ClothingSlotType ClothingSlotType => _clothingSlotType;

        /// <summary>
        /// If this is a right-sided slot like right glove, right shoe
        /// </summary>
        public bool UseAltClothingModel => _useAltClothingModel;

        protected override void OnAwake()
        {
            base.OnAwake();
            _renderer = GetComponent<SkinnedMeshRenderer>();
            _cullable = GetComponent<Cullable>();
        }

        /// <summary>
        /// Assign a clothing item to be displayed on this slot.
        /// </summary>
        [Client]
        public void SetItem(Item item)
        {
            _item = item;
            _hasItem = true;

            SetupItem();

            Cullable.SetHidden(false);

            if (_item)
            {
                _item.OnItemVisualChanged += ItemVisualOnChange;
            }
        }

        /// <summary>
        /// Remove the clothing item displayed on this slot.
        /// </summary>
        [Client]
        public void RemoveItem()
        {
            if (_item)
            {
                _item.OnItemVisualChanged -= ItemVisualOnChange;
            }

            _item = null;
            _hasItem = false;
            _cullingData = null;
            RemoveClothingMesh();
            Cullable.SetHidden(true);
        }

        /// <summary>
        /// Set item data
        /// </summary>
        [Client]
        private void SetupItem()
        {
            if (_item.ItemVisualData is not ClothingItemVisualData visualData)
            {
                Log.Warning(this, $" item {_item.gameObject} does not have ClothingItemVisualData, can't display cloth");
                _visualData = null;
                return;
            }
            _visualData = visualData;

            SetClothingMesh();
            SetClothingCullingData();
        }

        /// <summary>
        /// Find the mesh assigned to the item and use it 
        /// </summary>
        [Client]
        private void SetClothingMesh()
        {
            // Set mesh
            Mesh newMesh = _visualData.ClothingModel;
            if (_useAltClothingModel & (_visualData.AltClothingModel != null))
            {
                newMesh = _visualData.AltClothingModel;
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
            ClothingItemCullingData cullingData = _visualData.CullingData;
            if (_useAltClothingModel & _visualData.AltCullingData != null)
            {
                cullingData = _visualData.AltCullingData;
            }

            _cullingData = cullingData;
        }

        /// <summary>
        /// Assign a mesh to the renderer 
        /// </summary>
        [Client]
        private void SetRendererMesh(Mesh mesh)
        {
            _renderer.sharedMesh = mesh;
        }

        /// <summary>
        /// Assign materials to the renderer
        /// </summary>
        [Client]
        private void SetRendererMaterials(Material[] materials)
        {
            _renderer.sharedMaterials = materials;
        }

        /// <summary>
        /// Callback when the item's visual is changed 
        /// </summary>
        [Client]
        private void ItemVisualOnChange(Item item, ItemVisualData oldData, ItemVisualData newData)
        {
            ClothingItemCullingData oldCullingData = _cullingData;

            SetupItem();

            ClothingItemCullingData newCullingData = _cullingData;

            if (oldCullingData == newCullingData) return;

            // TODO: Signal back to the clothing displayer so the culling can be changed
            OnItemCullingChanged?.Invoke(this, oldCullingData, newCullingData);
        }
    }
}