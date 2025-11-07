using Coimbra.Services.Events;
using SS3D.Core;
using SS3D.Core.Behaviours;
using SS3D.Systems.Entities;
using SS3D.Systems.Entities.Events;
using SS3D.Systems.PlayerControl;
using SS3D.Systems.Rounds;
using SS3D.Systems.Rounds.Messages;
using SS3D.UI.Buttons;
using UnityEngine;
using SS3D.Attributes;
using TMPro;

namespace SS3D.Systems.CharacterCreation
{
    /// <summary>
    /// Sends selected customization to CharacterCreationSubSystem and previews customization on the lobby avatar
    /// </summary>
    public sealed class CharacterCreationView : NetworkActor
    {
        [SerializeField] [NotNull] private AppearanceDisplayer _previewCharacter;
        [SerializeField] [NotNull] private TMP_Text _previewNameText;
        
        [Header("Name Selection")]
        [SerializeField] [NotNull] private TMP_InputField _characterNameSelection;

        [Header("Style Selection")]
        [SerializeField] [NotNull] private CustomizationGrid _hairSelection;
        [SerializeField] [NotNull] private CustomizationGrid _beardSelection;
        [SerializeField] [NotNull] private CustomizationGrid _eyebrowSelection;

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

            _characterNameSelection.onValueChanged.RemoveListener(HandleCharacterNameChanged);
        }
        
        protected override void OnStart()
        {
            base.OnStart();
            
            _characterNameSelection.text = _characterCreationSubSystem.CharacterName;
        }

        /// <summary>
        /// Method called when a grid is loaded so the correct option can be set as selected in the ui.
        /// </summary>
        public void HandleCustomizationGridStarted(CustomizationType type, CustomizationGrid grid)
        {
            // Need to load data and send it
            switch (type)
            {
                case CustomizationType.Hairstyle:
                    // code
                    grid.SetSelectedOptionByName(_characterCreationSubSystem.HairStyle.name);
                    break;
                case CustomizationType.Beardstyle:
                    // code
                    grid.SetSelectedOptionByName(_characterCreationSubSystem.Beardstyle.name);
                    break;
                case CustomizationType.Eyebrows:
                    // code
                    grid.SetSelectedOptionByName(_characterCreationSubSystem.Eyebrows.name);
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
            _characterCreationSubSystem.SetCharacterName(name);
        }

        /// <summary>
        /// Method called when a style is selected.
        /// </summary>
        public void HandleHairBeardBrowSelected(CustomizationType type, CustomizationSlot option)
        {
            switch (type)
            {
                case CustomizationType.Hairstyle:
                    _characterCreationSubSystem.SetHairstyle(option.CustomizationSO);
                    break;
                case CustomizationType.Beardstyle:
                    _characterCreationSubSystem.SetBeardstyle(option.CustomizationSO);
                    break;
                case CustomizationType.Eyebrows:
                    _characterCreationSubSystem.SetEyebrows(option.CustomizationSO);
                    break;
                default:
                    // error
                    break;
            }
        }
        
        /// <summary>
        /// Method called the players customization is changed, update the preview.
        /// </summary>
        public void HandleCustomizationChanged(CustomizationType type)
        {
            switch (type)
            {
                case CustomizationType.Hairstyle:
                    _previewCharacter.SetHairstyle(_characterCreationSubSystem.HairStyle);
                    break;
                case CustomizationType.Beardstyle:
                    _previewCharacter.SetBeardstyle(_characterCreationSubSystem.Beardstyle);
                    break;
                case CustomizationType.Eyebrows:
                    _previewCharacter.SetEyebrows(_characterCreationSubSystem.Eyebrows);
                    break;
                case CustomizationType.HairColor:
                    _previewCharacter.SetHairColor(_characterCreationSubSystem.HairColor);
                    break;
                case CustomizationType.EyeColor:
                    _previewCharacter.SetEyeColor(_characterCreationSubSystem.EyeColor);
                    break;
                case CustomizationType.SkinColor:
                    _previewCharacter.SetSkinColor(_characterCreationSubSystem.SkinColor);
                    break;
                case CustomizationType.CharacterName:
                    _previewNameText.text = _characterCreationSubSystem.CharacterName;
                    break;
                default:
                    // no code for other customisation types has been added yet
                    break;
            }
        }

    }
}
