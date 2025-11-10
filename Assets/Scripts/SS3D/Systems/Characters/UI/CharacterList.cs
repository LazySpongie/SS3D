using Coimbra;
using SS3D.Attributes;
using SS3D.Systems.Characters.Preferences;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Actor = SS3D.Core.Behaviours.Actor;

namespace SS3D.Systems.Characters
{

    /// <summary>
    /// Handle the UI and displaying everything related to the tilemap menu building part.
    /// </summary>
    public class CharacterList : Actor
    {
        public delegate void CharacterSelectionEventHandler(int index);
        
        public delegate void CharacterListStartEventHandler();

        public delegate void CreateCharacterEventHandler();

        // When the selection changes
        public event CharacterSelectionEventHandler OnCharacterSelected;

        // When this script starts
        public event CharacterListStartEventHandler OnCharacterListStarted;
        
        // When a character is created
        public event CreateCharacterEventHandler OnCreateCharacter;

        /// <summary>
        ///  The prefab for a single character slot UI.
        /// </summary>
        [SerializeField][NotNull] private GameObject _slotPrefab;
        
        /// <summary>
        /// Game object parent of the area in the menu where the slots will display.
        /// </summary>
        [SerializeField] [NotNull] private GameObject _contentRoot;
        
        /// <summary>
        /// Game object new character button.
        /// </summary>
        [SerializeField] [NotNull] private Button _createCharacterButton;

        /// <summary>
        /// List of slots created in the menu.
        /// </summary>
        private List<Button> _characterSlots = new();

        /// <summary>
        /// Currently selected option.
        /// </summary>
        private int _selectedIndex;

        /// <summary>
        /// Currently selected option.
        /// </summary>
        public int SelectedOption => _selectedIndex;

        protected override void OnAwake()
        {
            _createCharacterButton.onClick.AddListener(HandleNewCharacterButtonPressed);
        }

        protected override void OnStart()
        {
            OnCharacterListStarted?.Invoke();
        }

        /// <summary>
        /// Load a list of character names and place them in the UI.
        /// </summary>
        public void LoadList(List<string> names)
        {
            ClearList();
            
            // _createCharacterButton.transform.SetSiblingIndex(0);

            foreach (string name in names)
            {
                Button _slot = Instantiate(_slotPrefab, _contentRoot.transform, true).GetComponent<Button>();

                _slot.transform.localScale = Vector3.one;

                _characterSlots.Add(_slot);

                _slot.GetComponentInChildren<TMP_Text>().text = name;

                _slot.onClick.AddListener(() => HandleSlotButtonPressed(_slot));

            }

            _createCharacterButton.transform.SetSiblingIndex(_contentRoot.transform.childCount);
        }

        /// <summary>
        /// Set the currently selected option.
        /// </summary>
        public void SetSelectedOption(int index)
        {
            if (_characterSlots.Count == 0) return;
            _characterSlots[_selectedIndex].interactable = true;
            _selectedIndex = index;
            _characterSlots[_selectedIndex].interactable = false;
        }
        
        /// <summary>
        /// Set the currently selected option.
        /// </summary>
        public void SetName(string name, int index)
        {
            if (_characterSlots.Count == 0) return;
            _characterSlots[index].GetComponentInChildren<TMP_Text>().text = name;
        }

        /// <summary>
        /// Clear all slots in the content area of the menu.
        /// </summary>
        private void ClearList()
        {
            for (int i = _characterSlots.Count - 1; i >= 0; i--)
            {
                _characterSlots[i].gameObject.Dispose(true);
            }
            _characterSlots.Clear();
        }

        /// <summary>
        /// Called when an option in the grid is selected, set the option as the selected option.
        /// </summary>
        private void HandleSlotButtonPressed(Button slot)
        {
            int index = _characterSlots.IndexOf(slot);
            OnCharacterSelected?.Invoke(index);
        }

        /// <summary>
        /// Called when the create character button is pressed.
        /// </summary>
        private void HandleNewCharacterButtonPressed()
        {
            OnCreateCharacter?.Invoke();
        }

    }
}