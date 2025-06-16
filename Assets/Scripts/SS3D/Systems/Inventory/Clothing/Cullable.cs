using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS3D.Core.Behaviours;
using SS3D.Systems.Inventory.Items;
using FishNet.Object;
using Serilog;

namespace SS3D.Systems.Inventory.Clothing
{
    public class Cullable : Actor
    {
        [SerializeField]
        private SkinnedMeshRenderer _renderer;

        /// <summary>
        /// If there is no item in the slot the renderer should be hidden
        /// </summary>
        private bool _isHidden;

        /// <summary>
        /// List of items culling this slot
        /// </summary>
        private List<Item> _cullers = new();

        /// <summary>
        /// If any items are culling this slot the renderer should be hidden
        /// </summary>
        public bool IsCulled => _cullers.Count > 0;

        /// <summary>
        /// If there is no item in the slot the renderer should be hidden
        /// </summary>
        public bool IsHidden => _isHidden;

        [Client]
        public void SetHidden(bool hidden)
        {
            _isHidden = hidden;
            UpdateRendererEnabled();
        }

        /// <summary>
        /// Add an item to the list of items attempting to hide this clothing slot
        /// </summary>
        [Client]
        public void AddCuller(Item item)
        {
            _cullers.Add(item);
            UpdateRendererEnabled();
        }

        /// <summary>
        /// Remove an item from the list of items attempting to hide this clothing slot
        /// </summary>
        [Client]
        public void RemoveCuller(Item item)
        {
            _cullers.Remove(item);
            UpdateRendererEnabled();
        }

        /// <summary>
        /// Set the renderer's visibility based on the culling and if it has an item assigned
        /// </summary>
        [Client]
        public void UpdateRendererEnabled()
        {
            if (IsCulled || IsHidden)
            {
                _renderer.enabled = false;
                return;
            }

            _renderer.enabled = true;
            Log.Warning("Update Renderer");
        }
    }
}