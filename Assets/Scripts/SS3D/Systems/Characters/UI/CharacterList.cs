using Coimbra;
using SS3D.Attributes;
using SS3D.Systems.Characters.Preferences;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Actor = SS3D.Core.Behaviours.Actor;

namespace SS3D.Systems.Characters.UI
{

    /// <summary>
    /// Handle the UI and displaying everything related to the tilemap menu building part.
    /// </summary>
    public class CharacterList : Actor
    {
        public delegate void CharacterSlotEventHandler(int index);
        
        public delegate void CharacterListEventHandler();

        // When this script starts it needs to be setup
        public event CharacterListEventHandler OnCharacterListStarted;

        // When the player clicks to select a character
        public event CharacterSlotEventHandler OnCharacterSelected;
        
        // When the player clicks to delete a character
        public event CharacterSlotEventHandler OnCharacterDeleted;

        // When the player clicks to create a character
        public event CharacterListEventHandler OnCreateCharacter;

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
        private List<CharacterSlot> _characterSlots = new();

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
            _selectedIndex = 0;
            // _createCharacterButton.transform.SetSiblingIndex(0);

            foreach (string name in names)
            {
                CharacterSlot _slot = Instantiate(_slotPrefab, _contentRoot.transform, true).GetComponent<CharacterSlot>();

                _slot.transform.localScale = Vector3.one;

                _characterSlots.Add(_slot);

                _slot.SetName(name);

                _slot.DeleteButton.onClick.AddListener(() => HandleDeleteButtonPressed(_slot));
                _slot.CharacterButton.onClick.AddListener(() => HandleSlotButtonPressed(_slot));

            }

            _createCharacterButton.transform.SetSiblingIndex(_contentRoot.transform.childCount);
        }


        /// <summary>
        /// Set the currently selected option.
        /// </summary>
        public void SetSelectedOption(int index)
        {
            if (_characterSlots.Count == 0) return;
            if (_selectedIndex == index) return;
            
            _characterSlots[_selectedIndex]?.SetSelected(false);
            _selectedIndex = index;
            _characterSlots[_selectedIndex]?.SetSelected(true);
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
        /// Load a list of character names and place them in the UI.
        /// </summary>
        public void SetNames(List<string> names)
        {
            if (_characterSlots.Count == 0) return;
            
            int i = 0;
            foreach (CharacterSlot slot in _characterSlots)
            {
                slot.SetName(names[i]);
                i++;
            }
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
        /// Called when the player clicks a character, set the character as the selected option.
        /// </summary>
        private void HandleSlotButtonPressed(CharacterSlot slot)
        {
            int index = _characterSlots.IndexOf(slot);
            OnCharacterSelected?.Invoke(index);
        }

        /// <summary>
        /// Called when the player clicks the delete button on a character.
        /// </summary>
        private void HandleDeleteButtonPressed(CharacterSlot slot)
        {
            int index = _characterSlots.IndexOf(slot);
            OnCharacterDeleted?.Invoke(index);
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