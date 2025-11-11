using Coimbra;
using SS3D.Attributes;
using SS3D.Systems.Characters.Preferences;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Actor = SS3D.Core.Behaviours.Actor;

namespace SS3D.Systems.Characters.UI
{

    /// <summary>
    /// Handle the UI and displaying everything related to the tilemap menu building part.
    /// </summary>
    public class CustomizationGrid : Actor
    {
        public delegate void CustomizationGridSelectionEventHandler(CustomizationGrid grid, CustomizationSlot option);
        
        public delegate void CustomizationGridStartEventHandler(CustomizationGrid grid);

        // When the selection changes
        public event CustomizationGridSelectionEventHandler OnSelected;
        
        // When this script starts
        public event CustomizationGridStartEventHandler OnStarted;

        /// <summary>
        ///  The search bar for the menu.
        /// </summary>
        [SerializeField][NotNull] private TMP_InputField _searchBar;

        /// <summary>
        ///  The prefab for a single slot, to display customization options in the menu.
        /// </summary>
        [SerializeField][NotNull] private GameObject _slotPrefab;
        
        /// <summary>
        /// Game object parent of the area in the menu where the slots will display.
        /// </summary>
        [SerializeField] [NotNull] private GameObject _contentRoot;

        /// <summary>
        /// List of customization options to load in the menu.
        /// </summary>
        [SerializeField] private CustomizationOptionsSO _customizationDatabase;

        /// <summary>
        /// List of slots created in the menu.
        /// </summary>
        private List<CustomizationSlot> _customizationSlots = new List<CustomizationSlot>();

        /// <summary>
        /// Currently selected option.
        /// </summary>
        private CustomizationSlot _selectedOption;

        /// <summary>
        /// Currently selected option.
        /// </summary>
        public CustomizationSlot SelectedOption => _selectedOption;

        protected override void OnStart()
        {
            LoadGrid();

            _searchBar.onValueChanged.AddListener(HandleSearchFieldChanged);
            OnStarted?.Invoke(this);
        }

        protected override void OnDestroyed()
        {
            _searchBar.onValueChanged.RemoveListener(HandleSearchFieldChanged);
        }

        /// <summary>
        /// Show options that contain given string in their name.
        /// </summary>
        public void FilterCustomization(string text)
        {
            // ClearGrid();
            foreach (CustomizationSlot slot in _customizationSlots)
            {
                // default (bald) option should always be visible maybe?
                slot.gameObject.SetActive(slot.CustomizationSO.NameString.Contains(text, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Load a list of customization options and place them in the UI box grid.
        /// </summary>
        public void LoadGrid()
        {
            ClearGrid();
            // _objectDatabase = _tileSystem.Loader.Assets;
            foreach (CustomizationSO asset in _customizationDatabase.Options)
            {
                CustomizationSlot _slot = Instantiate(_slotPrefab, _contentRoot.transform, true).GetComponent<CustomizationSlot>();
                _slot.gameObject.transform.localScale = Vector3.one;

                _customizationSlots.Add(_slot);

                _slot.Setup(this, asset);

                _slot.Button.onClick.AddListener(() => HandleSlotButtonPressed(_slot));
            }
        }

        /// <summary>
        /// Set the currently selected option by name.
        /// </summary>
        public void SetSelectedOptionByName(string name, bool invoke = true)
        {
            foreach (CustomizationSlot option in _customizationSlots)
            {
                if (option.CustomizationSO.name == name)
                {
                    HandleSlotButtonPressed(option, invoke);
                }
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

        /// <summary>
        /// Called when an option in the grid is selected, set the option as the selected option.
        /// </summary>
        private void HandleSlotButtonPressed(CustomizationSlot slot, bool invoke = true)
        {
            if (slot == null) slot = _customizationSlots[0];
            if (slot == null) return;
            if (_selectedOption == slot) return;
            
            if (_selectedOption) _selectedOption.Button.interactable = true; 
            _selectedOption = slot;
            _selectedOption.Button.interactable = false;

            if (!invoke) return;
            OnSelected?.Invoke(this, _selectedOption);
        }

        /// <summary>
        /// Called when the text in the search box is changed.
        /// </summary>
        public void HandleSearchFieldChanged(string text)
        {
            FilterCustomization(text);
        }
    }
}