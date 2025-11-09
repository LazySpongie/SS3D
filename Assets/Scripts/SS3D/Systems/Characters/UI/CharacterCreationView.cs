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

namespace SS3D.Systems.Characters
{
    /// <summary>
    /// Sends selected customization to CharacterCreationSubSystem and previews customization on the lobby avatar
    /// </summary>
    public sealed class CharacterCreationView : Actor
    {
        [SerializeField] [NotNull] private AppearanceDisplayer _previewCharacter;
        [SerializeField] [NotNull] private List<TMP_Text> _previewNameTexts;

        [Header("Save/Load Buttons")]
        [SerializeField][NotNull] private Button _saveButton;
        [SerializeField][NotNull] private Button _loadButton;
        
        [Header("Name Selection")]
        [SerializeField] [NotNull] private TMP_InputField _characterNameSelection;

        [Header("Style Selection")]
        [SerializeField] [NotNull] private CustomizationGrid _hairSelection;
        [SerializeField] [NotNull] private CustomizationGrid _beardSelection;
        [SerializeField] [NotNull] private CustomizationGrid _eyebrowSelection;

        [Header("Color Selection")]
        [SerializeField][NotNull] private Button _hairColorSelection;
        [SerializeField][NotNull] private Button _eyeColorSelection;
        [SerializeField][NotNull] private Button _skinColorSelection;
        
        [Header("Colors")]
        [SerializeField] public List<Color> hairColours;
        [SerializeField] public List<Color> eyeColors;
        [SerializeField] public List<Color> skinColors;

        private int _currentHairColor = 0;
        private int _currentEyeColor = 0;
        private int _currentSkinColor = 0;

        private ClientPreferencesSubSystem _preferences;
        
        #region Setup
        protected override void OnAwake()
        {
            base.OnAwake();

            _preferences = SubSystems.Get<ClientPreferencesSubSystem>();

            _preferences.OnCharacterChanged += HandleCharacterChanged;

            _hairSelection.OnCustomizationGridStarted += HandleCustomizationGridStarted;
            _beardSelection.OnCustomizationGridStarted += HandleCustomizationGridStarted;
            _eyebrowSelection.OnCustomizationGridStarted += HandleCustomizationGridStarted;

            _hairSelection.OnCustomizationGridSelected += HandleAppearanceSelected;
            _beardSelection.OnCustomizationGridSelected += HandleAppearanceSelected;
            _eyebrowSelection.OnCustomizationGridSelected += HandleAppearanceSelected;
            
            _hairColorSelection.onClick.AddListener(() => HandleColorSelectionChanged(AppearanceType.HairColor));
            _eyeColorSelection.onClick.AddListener(() => HandleColorSelectionChanged(AppearanceType.EyeColor));
            _skinColorSelection.onClick.AddListener(() => HandleColorSelectionChanged(AppearanceType.SkinColor));
            
            _saveButton.onClick.AddListener(HandleSaveButtonPressed);
            _loadButton.onClick.AddListener(HandleLoadButtonPressed);
             
            _characterNameSelection.onValueChanged.AddListener(HandleNameFieldChanged);
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();

            _preferences.OnCharacterChanged -= HandleCharacterChanged;

            _hairSelection.OnCustomizationGridStarted -= HandleCustomizationGridStarted;
            _beardSelection.OnCustomizationGridStarted -= HandleCustomizationGridStarted;
            _eyebrowSelection.OnCustomizationGridStarted -= HandleCustomizationGridStarted;

            _hairSelection.OnCustomizationGridSelected -= HandleAppearanceSelected;
            _beardSelection.OnCustomizationGridSelected -= HandleAppearanceSelected;
            _eyebrowSelection.OnCustomizationGridSelected -= HandleAppearanceSelected;

            _hairColorSelection.onClick.RemoveListener(() => HandleColorSelectionChanged(AppearanceType.HairColor));
            _eyeColorSelection.onClick.RemoveListener(() => HandleColorSelectionChanged(AppearanceType.EyeColor));
            _skinColorSelection.onClick.RemoveListener(() => HandleColorSelectionChanged(AppearanceType.SkinColor));

            _saveButton.onClick.RemoveListener(HandleSaveButtonPressed);
            _loadButton.onClick.RemoveListener(HandleLoadButtonPressed);

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
                case CharacterChangeType.Everything:
                    SetSelectedStylesFromCurrent();
                    HandleCharacterNameChanged(_preferences.UnsavedCharacter.Name);
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
        /// Method called the players customization is changed, update the preview.
        /// </summary>
        public void HandleAppearanceChanged()
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
                        SetColourPickerButton(type, color);
                        break;
                }
            }
        }

        public void SetColourPickerButton(AppearanceType type, Color color)
        {
            switch (type)
            {
                case AppearanceType.HairColor:
                    _hairColorSelection.GetComponent<Image>().color = color;
                    break;
                case AppearanceType.EyeColor:
                    _eyeColorSelection.GetComponent<Image>().color = color;
                    break;
                case AppearanceType.SkinColor:
                    _skinColorSelection.GetComponent<Image>().color = color;
                    break;
                default:
                    // error
                    break;
            }
        }

        /// <summary>
        /// Method called when a grid is loaded so the correct option can be set as selected in the ui.
        /// </summary>
        public void HandleCustomizationGridStarted(CustomizationGrid grid)
        {
            if (_preferences.UnsavedCharacter == null) return;
            if (grid is AppearanceGrid appearanceGrid)
            {
                AppearanceType type = appearanceGrid.AppearanceType;

                SetSelectedStyleFromCurrent(type, grid);
            }
        }

        /// <summary>
        /// Set the selection of all grids to the current option from the character profile.
        /// </summary>
        public void SetSelectedStylesFromCurrent()
        {
            SetSelectedStyleFromCurrent(AppearanceType.Hairstyle, _hairSelection);
            SetSelectedStyleFromCurrent(AppearanceType.Beardstyle, _beardSelection);
            SetSelectedStyleFromCurrent(AppearanceType.Eyebrows, _eyebrowSelection);
        }

        /// <summary>
        /// Set the selection of an appearance grid to the current option from the character profile.
        /// </summary>
        private void SetSelectedStyleFromCurrent(AppearanceType type, CustomizationGrid grid)
        {
            // Need to load data and send it
            switch (type)
            {
                case AppearanceType.Hairstyle:
                case AppearanceType.Beardstyle:
                case AppearanceType.Eyebrows:
                    grid.SetSelectedOptionByName(_preferences.UnsavedCharacter.Appearance[type], false);
                    break;
            }
        }
        #endregion

        #region Update Character
        
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
            switch (type)
            {
                case AppearanceType.Hairstyle:
                case AppearanceType.Beardstyle:
                case AppearanceType.Eyebrows:
                    _preferences.SetAppearanceOption(type, option.CustomizationSO.name);
                    break;
                default:
                    // error
                    break;
            }
        }

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
        public void HandleLoadButtonPressed()
        {
            //todo: make character list
            _preferences.LoadCharactersFromDisk();
            _preferences.SelectCharacter(0);
        }

        /// <summary>
        /// Callback when a color button is pressed.
        /// </summary>
        public void HandleColorSelectionChanged(AppearanceType type)
        {
            // placeholder
            switch (type)
            {
                case AppearanceType.HairColor:
                    _currentHairColor++;
                    if (_currentHairColor >= hairColours.Count) _currentHairColor = 0;
                    _preferences.SetColor(type, ColorUtility.ToHtmlStringRGB(hairColours[_currentHairColor]));
                    break;

                case AppearanceType.EyeColor:
                    _currentEyeColor++;
                    if (_currentEyeColor >= eyeColors.Count) _currentEyeColor = 0;
                    _preferences.SetColor(type, ColorUtility.ToHtmlStringRGB(eyeColors[_currentEyeColor]));
                    break;

                case AppearanceType.SkinColor:
                    _currentSkinColor++;
                    if (_currentSkinColor >= skinColors.Count) _currentSkinColor = 0;
                    _preferences.SetColor(type, ColorUtility.ToHtmlStringRGB(skinColors[_currentSkinColor]));
                    break;
                default:
                    break;
            }
        }
        #endregion
        
    }
}
