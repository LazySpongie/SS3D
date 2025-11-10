using SS3D.Core;
using SS3D.Core.Behaviours;
using UnityEngine;
using SS3D.Attributes;
using TMPro;
using System.Collections.Generic;
using SS3D.Data;
using SS3D.Logging;
using System;
using UnityEngine.UI;
using SS3D.Systems.Characters.Preferences;
using System.Linq;

namespace SS3D.Systems.Characters
{
    /// <summary>
    /// this demon controls the character creation ui and sends the players changes to ClientPreferencesSubSystem
    /// </summary>
    public sealed class CharacterCreationView : Actor
    {
        [SerializeField] [NotNull] private AppearanceDisplayer _previewCharacter;
        [SerializeField] [NotNull] private List<TMP_Text> _previewNameTexts;

        [Header("Save/Load Buttons")]
        [SerializeField] [NotNull] private Button _saveButton;
        [SerializeField] [NotNull] private Button _ResetButton;

        [Header("Character Selection")]
        [SerializeField] [NotNull] private CharacterList _characterSelection;
        
        [Header("Name Selection")]
        [SerializeField] [NotNull] private TMP_InputField _characterNameSelection;

        [Header("Style Selection")]
        [SerializeField] [NotNull] private List<AppearanceGrid> _styleSelections = new();

        [Header("Color Selection")]
        [SerializeField] [NotNull] private List<ColorSelection> _colorSelections;
        
        private ClientPreferencesSubSystem _preferences;

        #region Setup
        
        protected override void OnAwake()
        {
            base.OnAwake();

            _preferences = SubSystems.Get<ClientPreferencesSubSystem>();

            _preferences.OnCharacterChanged += HandleCharacterChanged;
            _preferences.OnCharactersLoaded += HandleCharactersLoaded;

            _characterSelection.OnCharacterListStarted += HandleCharacterListStarted;
            _characterSelection.OnCharacterSelected += HandleCharacterSelectionChanged;
            _characterSelection.OnCharacterDeleted += HandleDeleteButtonPressed;
            _characterSelection.OnCreateCharacter += HandleCharacterCreated;

            _styleSelections.ForEach(grid =>
            {
                grid.OnCustomizationGridStarted += HandleCustomizationGridStarted;
                grid.OnCustomizationGridSelected += HandleAppearanceSelected;
            });

            _colorSelections.ForEach(colorSelection =>
            {
                colorSelection.OnColorSelected += HandleColorSelectionChanged;
            });

            _saveButton.onClick.AddListener(HandleSaveButtonPressed);
            _ResetButton.onClick.AddListener(HandleResetButtonPressed);

            _characterNameSelection.onValueChanged.AddListener(HandleNameFieldChanged);
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();

            _preferences.OnCharacterChanged -= HandleCharacterChanged;
            _preferences.OnCharactersLoaded -= HandleCharactersLoaded;

            _characterSelection.OnCharacterListStarted -= HandleCharacterListStarted;
            _characterSelection.OnCharacterSelected -= HandleCharacterSelectionChanged;
            _characterSelection.OnCharacterDeleted -= HandleDeleteButtonPressed;
            _characterSelection.OnCreateCharacter -= HandleCharacterCreated;

            _styleSelections.ForEach(grid =>
            {
                grid.OnCustomizationGridStarted -= HandleCustomizationGridStarted;
                grid.OnCustomizationGridSelected -= HandleAppearanceSelected;
            });

            _colorSelections.ForEach(colorSelection =>
            {
                colorSelection.OnColorSelected -= HandleColorSelectionChanged;
            });

            _saveButton.onClick.RemoveListener(HandleSaveButtonPressed);
            _ResetButton.onClick.RemoveListener(HandleResetButtonPressed);

            _characterNameSelection.onValueChanged.RemoveListener(HandleNameFieldChanged);
        }

        #endregion

        #region Update UI
        
        /// <summary>
        /// Method called when something about the character profile has been changed.
        /// </summary>
        private void HandleCharacterChanged(CharacterChangeType type)
        {
            switch (type)
            {
                case CharacterChangeType.Load:
                    HandleCharacterSelected();
                    SetSelectedStylesFromCurrent();
                    HandleCharacterNameChanged(_preferences.UnsavedCharacter.Name);

                    // need to update the characters name in the character selection list when a name has been saved
                    _characterSelection.SetNames(_preferences.CharacterNames.ToList());

                    HandleAppearanceChanged();
                    break;
                case CharacterChangeType.Name:
                    HandleCharacterNameChanged(_preferences.UnsavedCharacter.Name);
                    break;
                case CharacterChangeType.Appearance:
                    HandleAppearanceChanged();
                    break;
            }
        }

        /// <summary>
        /// Set the selection of the style grids to the current option.
        /// </summary>
        private void SetSelectedStylesFromCurrent()
        {
            _styleSelections.ForEach(grid =>
            {
                grid.SetSelectedOptionByName(_preferences.UnsavedCharacter.Appearance[grid.AppearanceType], false);
            });
        }

        /// <summary>
        /// Update the characters name in the ui.
        /// </summary>
        private void HandleCharacterNameChanged(string name)
        {
            foreach (TMP_Text text in _previewNameTexts)
            {
                text.text = name;
            }
            _characterNameSelection.text = name;
        }

        /// <summary>
        /// Update character list to select the correct character.
        /// </summary>
        private void HandleCharacterSelected()
        {
            _characterSelection.SetSelectedOption(_preferences.SelectedCharacterIndex);
        }

        /// <summary>
        /// Update the preview appearance.
        /// </summary>
        private void HandleAppearanceChanged()
        {
            foreach (int i in Enum.GetValues(typeof(AppearanceType)))
            {
                AppearanceType type = (AppearanceType)i;
                switch (type)
                {
                    case AppearanceType.Hairstyle:
                    case AppearanceType.Beardstyle:
                    case AppearanceType.Eyebrows:
                        CustomizationSO option = Assets.Get<CustomizationSO>("Customization", _preferences.UnsavedCharacter.Appearance[type]);
                        _previewCharacter.SetStyle(type, option);
                        break;

                    case AppearanceType.HairColor:
                    case AppearanceType.EyeColor:
                    case AppearanceType.SkinColor:
                        if (!ColorUtility.TryParseHtmlString("#" + _preferences.UnsavedCharacter.Appearance[type], out Color color)) break;
                        _previewCharacter.SetColor(type, color);
                        SetColorPickerButton(type, color);
                        break;
                }
            }
        }

        /// <summary>
        /// Set the color pickers to the selected color.
        /// </summary>
        private void SetColorPickerButton(AppearanceType type, Color color)
        {
            _colorSelections.ForEach(colorSelection =>
            {
                if (colorSelection.Type != type) return;
                colorSelection.SetColor(color);
            });
        }

        /// <summary>
        /// Method called when the character list object is loaded.
        /// </summary>
        private void HandleCharacterListStarted()
        {
            HandleCharactersLoaded();
            _characterSelection.SetSelectedOption(_preferences.SelectedCharacterIndex);
        }

        /// <summary>
        /// Fill the character selection list with character names.
        /// </summary>
        private void HandleCharactersLoaded()
        {
            _characterSelection.LoadList(_preferences.CharacterNames.ToList());
        }

        /// <summary>
        /// Method called when a grid is loaded so the correct option can be set as selected in the ui.
        /// </summary>
        private void HandleCustomizationGridStarted(CustomizationGrid grid)
        {
            if (_preferences.UnsavedCharacter == null) return;
            if (grid is AppearanceGrid appearanceGrid)
            {
                AppearanceType type = appearanceGrid.AppearanceType;
                grid.SetSelectedOptionByName(_preferences.UnsavedCharacter.Appearance[type], false);
            }
        }

        #endregion

        #region Update Character

        /// <summary>
        /// Callback when the save button is pressed.
        /// </summary>
        public void HandleSaveButtonPressed()
        {
            _preferences.SaveCharacter();
        }

        /// <summary>
        /// Callback when a character is selected.
        /// </summary>
        public void HandleResetButtonPressed()
        {
            _preferences.ResetCharacter();
        }

        /// <summary>
        /// Callback when the player clicks to create a new character.
        /// </summary>
        private void HandleCharacterCreated()
        {
            _preferences.CreateCharacter();
        }

        /// <summary>
        /// Callback when the player clicks to delete a character.
        /// </summary>
        private void HandleDeleteButtonPressed(int index)
        {
            _preferences.DeleteCharacter(index);
        }
        
        /// <summary>
        /// Callback when the player clicks to select a different character.
        /// </summary>
        private void HandleCharacterSelectionChanged(int index)
        {
            _preferences.SelectCharacter(index);
        }
        
        /// <summary>
        /// Callback when the character name text field is changed.
        /// </summary>
        public void HandleNameFieldChanged(string name)
        {
            _preferences.SetCharacterName(name);
        }

        /// <summary>
        /// Callback when an appearance option is selected.
        /// </summary>
        public void HandleAppearanceSelected(CustomizationGrid grid, CustomizationSlot option)
        {
            if (grid is not AppearanceGrid appearanceGrid) return;

            AppearanceType type = appearanceGrid.AppearanceType;
            _preferences.SetAppearanceOption(type, option.CustomizationSO.name);
        }
        
        /// <summary>
        /// Callback when a color button is pressed.
        /// </summary>
        public void HandleColorSelectionChanged(ColorSelection colorSelection, Color color)
        {
            _preferences.SetColor(colorSelection.Type, ColorUtility.ToHtmlStringRGB(color));
        }
        #endregion
        
    }
}
