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

namespace SS3D.Systems.CharacterCreation
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
        
        private CharacterCreationSubSystem _characterCreationSubSystem;
        
        protected override void OnAwake()
        {
            base.OnAwake();

            _characterCreationSubSystem = SubSystems.Get<CharacterCreationSubSystem>();

            _characterCreationSubSystem.OnCharacterCustomizationChanged += HandleCustomizationChanged;

            _hairSelection.OnCustomizationGridStarted += HandleCustomizationGridStarted;
            _beardSelection.OnCustomizationGridStarted += HandleCustomizationGridStarted;
            _eyebrowSelection.OnCustomizationGridStarted += HandleCustomizationGridStarted;

            _hairSelection.OnCustomizationGridSelected += HandleHairBeardBrowSelected;
            _beardSelection.OnCustomizationGridSelected += HandleHairBeardBrowSelected;
            _eyebrowSelection.OnCustomizationGridSelected += HandleHairBeardBrowSelected;
            
            _hairColorSelection.onClick.AddListener(() => HandleColorSelectionChanged(CustomizationType.HairColor));
            _eyeColorSelection.onClick.AddListener(() => HandleColorSelectionChanged(CustomizationType.EyeColor));
            _skinColorSelection.onClick.AddListener(() => HandleColorSelectionChanged(CustomizationType.SkinColor));
            
            _saveButton.onClick.AddListener(HandleSaveButtonPressed);
            _loadButton.onClick.AddListener(HandleLoadButtonPressed);
             
            _characterNameSelection.onValueChanged.AddListener(HandleCharacterNameChanged);
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();

            _characterCreationSubSystem.OnCharacterCustomizationChanged -= HandleCustomizationChanged;

            _hairSelection.OnCustomizationGridStarted -= HandleCustomizationGridStarted;
            _beardSelection.OnCustomizationGridStarted -= HandleCustomizationGridStarted;
            _eyebrowSelection.OnCustomizationGridStarted -= HandleCustomizationGridStarted;

            _hairSelection.OnCustomizationGridSelected -= HandleHairBeardBrowSelected;
            _beardSelection.OnCustomizationGridSelected -= HandleHairBeardBrowSelected;
            _eyebrowSelection.OnCustomizationGridSelected -= HandleHairBeardBrowSelected;

            _hairColorSelection.onClick.RemoveListener(() => HandleColorSelectionChanged(CustomizationType.HairColor));
            _eyeColorSelection.onClick.RemoveListener(() => HandleColorSelectionChanged(CustomizationType.EyeColor));
            _skinColorSelection.onClick.RemoveListener(() => HandleColorSelectionChanged(CustomizationType.SkinColor));

            _saveButton.onClick.RemoveListener(HandleSaveButtonPressed);
            _loadButton.onClick.RemoveListener(HandleLoadButtonPressed);

            _characterNameSelection.onValueChanged.RemoveListener(HandleCharacterNameChanged);
        }

        public void HandleSaveButtonPressed()
        {
            _characterCreationSubSystem.HandleSaveButton();
        }

        public void HandleLoadButtonPressed()
        {
            _characterCreationSubSystem.HandleLoadButton();
            SetSelectedStylesFromCurrent();
        }

        public void HandleColorSelectionChanged(CustomizationType type)
        {
            _characterCreationSubSystem.ColorButtonOnClick(type);
        }

        /// <summary>
        /// Method called when a grid is loaded so the correct option can be set as selected in the ui.
        /// </summary>
        public void HandleCustomizationGridStarted(CustomizationType type, CustomizationGrid grid)
        {
            SetSelectedStyleFromCurrent(type, grid);
        }

        /// <summary>
        /// Method called when the character name text field is changed.
        /// </summary>
        public void SetSelectedStylesFromCurrent()
        {
            SetSelectedStyleFromCurrent(CustomizationType.Hairstyle, _hairSelection);
            SetSelectedStyleFromCurrent(CustomizationType.Beardstyle, _beardSelection);
            SetSelectedStyleFromCurrent(CustomizationType.Eyebrows, _eyebrowSelection);
        }
        
        /// <summary>
        /// Method called when the character name text field is changed.
        /// </summary>
        private void SetSelectedStyleFromCurrent(CustomizationType type, CustomizationGrid grid)
        {
            Dictionary<CustomizationType, string> customization = _characterCreationSubSystem.CurrentCustomization;

            // Need to load data and send it
            switch (type)
            {
                case CustomizationType.Hairstyle:
                case CustomizationType.Beardstyle:
                case CustomizationType.Eyebrows:
                    grid.SetSelectedOptionByName(customization[type], false);
                    break;
                default:
                    // error
                    break;
            }
        }

        /// <summary>
        /// Method called when the character name text field is changed.
        /// </summary>
        public void HandleCharacterNameChanged(string name)
        {
            _characterCreationSubSystem.SetCustomizationOption(CustomizationType.CharacterName, name);
        }

        /// <summary>
        /// Method called when a style is selected.
        /// </summary>
        public void HandleHairBeardBrowSelected(CustomizationType type, CustomizationSlot option)
        {
            switch (type)
            {
                case CustomizationType.Hairstyle:
                case CustomizationType.Beardstyle:
                case CustomizationType.Eyebrows:
                    _characterCreationSubSystem.SetCustomizationOption(type, option.CustomizationSO.name);
                    break;
                default:
                    // error
                    break;
            }
        }

        /// <summary>
        /// Method called the players customization is changed, update the preview.
        /// </summary>
        public void HandleCustomizationChanged()
        {
            Dictionary<CustomizationType, string> customization = _characterCreationSubSystem.CurrentCustomization;

            foreach (int i in Enum.GetValues(typeof(CustomizationType)))
            {
                CustomizationType type = (CustomizationType)i;
                switch (type)
                {
                    case CustomizationType.Hairstyle:
                    case CustomizationType.Beardstyle:
                    case CustomizationType.Eyebrows:
                        CustomizationSO option = Assets.Get<CustomizationSO>("Customization", customization[type]);
                        _previewCharacter.SetStyle(type, option);
                        break;

                    case CustomizationType.HairColor:
                    case CustomizationType.EyeColor:
                    case CustomizationType.SkinColor:
                        if (!ColorUtility.TryParseHtmlString("#" + customization[type], out Color color)) break;
                        _previewCharacter.SetColor(type, color);
                        SetColourPickerButton(type, color);
                        break;

                    case CustomizationType.CharacterName:
                        foreach (TMP_Text text in _previewNameTexts)
                        {
                            text.text = customization[type];
                        }
                        _characterNameSelection.text = customization[type];

                        break;

                    default:
                        // no code for other customisation types has been added yet
                        break;
                }
            }
        }
        
        public void SetColourPickerButton(CustomizationType type, Color color)
        {
            switch (type)
            {
                case CustomizationType.HairColor:
                    _hairColorSelection.GetComponent<Image>().color = color;
                    break;
                case CustomizationType.EyeColor:
                    _eyeColorSelection.GetComponent<Image>().color = color;
                    break;
                case CustomizationType.SkinColor:
                    _skinColorSelection.GetComponent<Image>().color = color;
                    break;
                default:
                    // error
                    break;
            }
        }
    }
}
