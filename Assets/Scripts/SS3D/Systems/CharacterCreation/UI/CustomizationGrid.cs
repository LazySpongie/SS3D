using Coimbra;
using SS3D.Attributes;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Actor = SS3D.Core.Behaviours.Actor;

namespace SS3D.Systems.CharacterCreation
{

    /// <summary>
    /// Handle the UI and displaying everything related to the tilemap menu building part.
    /// </summary>
    public class CustomizationGrid : Actor
    {
        /// <summary>
        ///  The prefab for a single slot, to display customization options in the menu.
        /// </summary>
        [SerializeField] [NotNull] private GameObject _slotPrefab;

        /// <summary>
        /// List of customization options to load in the menu.
        /// </summary>
        [SerializeField]
        private List<CustomizationSO> _customizationDatabase;

        /// <summary>
        /// List of slots created in the menu.
        /// </summary>
        private List<CustomizationSlot> _customizationSlots = new List<CustomizationSlot>();

        /// <summary>
        /// Game object parent of the area in the menu where the slots will display.
        /// </summary>
        [SerializeField] [NotNull] private GameObject _contentRoot;

        /// <summary>
        /// Currently selected customization option.
        /// </summary>
        private CustomizationSlot _selectedOption;

        protected override void OnStart()
        {
            LoadGrid();
            HandleSlotButtonPressed(_customizationSlots[0]);
        }

        /// <summary>
        /// Show options that contain given string in their name.
        /// </summary>
        public void FilterCustomization(string text)
        {
            ClearGrid();
            foreach (CustomizationSlot slot in _customizationSlots)
            {
                slot.enabled = false;
                if (!slot.CustomizationSO.NameString.Contains(text, StringComparison.OrdinalIgnoreCase)) continue;

                slot.enabled = true;
                // Instantiate(_slotPrefab, _contentRoot.transform, true).GetComponent<CustomizationSlot>().Setup(asset);
            }
        }

        /// <summary>
        /// Load a list of customization options and place them in the UI box grid.
        /// </summary>
        public void LoadGrid()
        {
            ClearGrid();
            // _objectDatabase = _tileSystem.Loader.Assets;
            foreach (CustomizationSO asset in _customizationDatabase)
            {
                CustomizationSlot _slot = Instantiate(_slotPrefab, _contentRoot.transform, true).GetComponent<CustomizationSlot>();
                _slot.gameObject.transform.localScale = Vector3.one;

                _customizationSlots.Add(_slot);

                _slot.Setup(this, asset);

                _slot.Button.onClick.AddListener(() => HandleSlotButtonPressed(_slot));
            }
        }

        /// <summary>
        /// Clear all slots in the content area of the menu.
        /// </summary>
        private void ClearGrid()
        {
            for (int i = _customizationSlots.Count - 1; i >= 0; i--)
            {
                _customizationSlots[i].gameObject.Dispose(true);
                _customizationSlots.RemoveAt(i);
            }
        }

        private void HandleSlotButtonPressed(CustomizationSlot slot)
        {
            if (slot == null) return;
            
            if (_selectedOption) _selectedOption.Button.interactable = true; 
            _selectedOption = slot;
            _selectedOption.Button.interactable = false;
        }

    }
}