using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using SS3D.Data;
using SS3D.Systems.Inventory.Items;

namespace SS3D.Systems.Inventory.Clothing
{
    /// <summary>
    /// An item with data that lets it be displayed on a player.
    /// 
    /// TODO: Add extra clothing item states at some point (hoods up and down etc)
    /// </summary>
    public class ClothingItem : Item
    {
        [Header("Clothing Settings")]

        [SerializeField] private ClothingVisualData _startingClothingVisual;
        
        [SerializeField] private bool _hidesIdentity;

        /// <summary>
        /// Name of the current clothing visual so it can be networked
        /// </summary>
        [SyncVar(OnChange = nameof(SyncClothingVisual))]
        private string _currentClothingVisualName;

        private ClothingVisualData _currentClothingVisual;

        public ClothingVisualData StartingClothingVisual => _startingClothingVisual;

        public ClothingVisualData CurrentClothingVisual => _currentClothingVisual;

        public bool HidesIdentity => _hidesIdentity;

        protected override void OnAwake()
        {
            base.OnAwake();

            // Set the correct visual data for the item when it is first spawned
            SetClothingVisual(_startingClothingVisual);
        }

        [Server]
        public void SetClothingVisual(ClothingVisualData visual)
        {
            if (visual == null) return;
            
            _currentClothingVisualName = visual.name;
        }

        /// <summary>
        /// Callback when the SyncVar _currentClothingVisualName is changed 
        /// </summary>
        private void SyncClothingVisual(string oldName, string newName, bool asServer)
        {
            _currentClothingVisual = Assets.Get<ClothingVisualData>("Clothing", newName);

            RefreshClothingVisual();
        }

        /// <summary>
        /// Re-add the item to its container to refresh the visuals for worn clothing items.
        /// </summary>
        [Server]
        private void RefreshClothingVisual()
        {
            if (!Container) return; 

            // This is jank as fuck but removing and readding the item to the container is the easiest way to refresh clothing
            Container?.TransferItemToOther(this, Container.PositionOf(this), Container);
        }
    }
}
