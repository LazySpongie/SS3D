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
    /// Networked script that syncs clothing in the characters inventory and sends it to the ClothingVisualDisplayer to be displayed.
    /// </summary>
    /// 
    public class ClothingVisualSlot : Actor
    {
        [Tooltip("Set which clothing container in the inventory is using this slot.")]
        [SerializeField]
        private ClothingSlotType _clothingSlotType;

        [Tooltip("If this is a right-sided slot like right glove, right shoe.")]
        [SerializeField]
        private bool _useAltClothingModel;

        /// <summary>
        /// ClothingItemVisualData used in this slot
        /// </summary>
        private ClothingItemVisualData _visualData;

        /// <summary>
        /// ClothingItemCullingData used by this item
        /// </summary>
        private ClothingItemCullingData _cullingData;

        /// <summary>
        /// Cullable script on this object
        /// </summary>
        private Cullable _cullable;

        private SkinnedMeshRenderer _renderer;

        /// <summary>
        /// Cullable script on this object
        /// </summary>
        public Cullable Cullable => _cullable;

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
        public bool HasItem => _visualData;

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
        public void SetItem(ItemVisualData data)
        {
            SetupVisualData(data);
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
            Cullable.SetHidden(true);
        }

        [Client]
        private void SetupVisualData(ItemVisualData data)
        {
            if (data is not ClothingItemVisualData visualData)
            {
                Log.Warning(this, $" {data} is not ClothingItemVisualData, can't display cloth");
                _visualData = null;
                return;
            }
            _visualData = visualData;
        }

        /// <summary>
        /// Set item data
        /// </summary>
        [Client]
        private void SetupItem()
        {
            SetClothingMesh();
            SetClothingCullingData();

            Cullable.SetHidden(false);
        }

        /// <summary>
        /// Find the mesh assigned to the item and use it 
        /// </summary>
        [Client]
        private void SetClothingMesh()
        {
            // Set mesh
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
    }
}