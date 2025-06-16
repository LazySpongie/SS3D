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
        [SerializeField]
        private ClothingSlotType _clothingSlotType;

        [SerializeField]
        private bool _useAltClothingModel;
        
        /// <summary>
        /// Item displayed in this slot
        /// </summary>
        private Item _item;

        private SkinnedMeshRenderer _renderer;

        /// <summary>
        /// Reference to the Cullable script on this object
        /// </summary>
        private Cullable _cullable;

        /// <summary>
        /// Reference to the Cullable script on this object
        /// </summary>
        public Cullable Cullable => _cullable;

        /// <summary>
        /// Item displayed in this slot
        /// </summary>
        public Item Item => _item;

        /// <summary>
        /// If there is an item displayed in this slot
        /// </summary>
        public bool HasItem => Item != null;

        public ClothingSlotType ClothingSlotType => _clothingSlotType;

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
            SetClothingMesh();
            Cullable.SetHidden(false);

            // TODO: Subscribe to OnItemVisualChanged event on the item here
        }

        /// <summary>
        /// Remove the clothing item displayed on this slot.
        /// </summary>
        [Client]
        public void RemoveItem()
        {
            _item = null;
            SetRendererMesh(null);
            Material[] materials = { };
            SetRendererMaterials(materials);
            Cullable.SetHidden(true);

            // TODO: Unsubscribe from OnItemVisualChanged event on the item here
        }

        /// <summary>
        /// Find the mesh assigned to the item and use it 
        /// </summary>
        [Client]
        private void SetClothingMesh()
        {
            if (_item.ItemVisualData is not ClothingItemVisualData visualData)
            {
                Log.Warning(this, $" item {_item.gameObject} does not have ClothingItemVisualData, can't display cloth");
                return;
            }

            // Set mesh
            Mesh newMesh = visualData.ClothingModel;
            if (_useAltClothingModel & (visualData.AltClothingModel != null))
            {
                newMesh = visualData.AltClothingModel;
            }

            SetRendererMesh(newMesh);

            // This will need to be changed to the instanced materials of the item later
            SetRendererMaterials(visualData.Materials);
        }

        /// <summary>
        /// Callback when the item's visual is modified 
        /// </summary>
        [Client]
        private void OnItemVisualChanged()
        {
            SetClothingMesh();
        }

        /// <summary>
        /// Assign a mesh to the renderer 
        /// </summary>
        [Client]
        private void SetRendererMesh(Mesh mesh)
        {
            _renderer.sharedMesh = mesh;
        }

        [Client]
        private void SetRendererMaterials(Material[] materials)
        {
            _renderer.sharedMaterials = materials;
        }
        
    }
}