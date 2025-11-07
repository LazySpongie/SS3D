using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using SS3D.Core;
using SS3D.Core.Behaviours;
using SS3D.Attributes;

namespace SS3D.Systems.CharacterCreation
{
    /// <summary>
    /// 
    /// store current character and their selected roles
    /// 
    /// saved character which will be sent over network when spawning
    /// local unsaved character when making changes in the menu
    /// 
    /// save character button - save character to file and store networked version
    /// load character button - menu of character slots which you can switch between
    /// 
    /// when a round is started the server should ask this script to give it the players appearance and selected jobs
    ///  
    /// character creation view should read from this script to preview character
    /// 
    /// </summary>
    public class CharacterCreationSubSystem : NetworkSubSystem
    {
        public delegate void CharacterCustomizationChangedHandler(CustomizationType type);

        public event CharacterCustomizationChangedHandler OnCharacterCustomizationChanged;

        [Header("Default Names")]
        [SerializeField] public string defaultCharacterName;

        [Header("Default Styles")]
        [SerializeField] public CustomizationSO defaultHair;
        [SerializeField] public CustomizationSO defaultBeard;
        [SerializeField] public CustomizationSO defaultEyebrows;

        [Header("Default Colors")]
        [SerializeField] public Color defaultHairColor;
        [SerializeField] public Color defaultEyeColor;
        [SerializeField] public Color defaultSkinColor;

        private string _characterName;
        private CustomizationSO _hairStyle;
        private CustomizationSO _beardStyle;
        private CustomizationSO _eyebrows;
        private Color _hairColor;
        private Color _eyeColor;
        private Color _skinColor;

        public string CharacterName => _characterName;

        public CustomizationSO HairStyle => _hairStyle;

        public CustomizationSO Beardstyle => _beardStyle;

        public CustomizationSO Eyebrows => _eyebrows;
        
        public Color HairColor => _hairColor;
        
        public Color EyeColor => _eyeColor;
        
        public Color SkinColor => _skinColor;

        protected override void OnStart()
        {
            base.OnStart();

            SetDefault();
        }

        /// <summary>
        /// Method called when the save character button is clicked.
        /// </summary>
        public void SetDefault()
        {
            SetCharacterName(defaultCharacterName);
            SetHairstyle(defaultHair);
            SetBeardstyle(defaultBeard);
            SetEyebrows(defaultEyebrows);
            SetHairColor(defaultHairColor);
            SetEyeColor(defaultEyeColor);
            SetSkinColor(defaultSkinColor);
        }
        
        /// <summary>
        /// Method called when the load character button is clicked.
        /// </summary>
        public void HandleLoadButton()
        {
            // set hairstyle selection here
        }

        /// <summary>
        /// Method called when the save character button is clicked.
        /// </summary>
        public void HandleSaveButton()
        {

        }

        public void SetCharacterName(string option)
        {
            _characterName = option;
            OnCharacterCustomizationChanged?.Invoke(CustomizationType.CharacterName);
        }

        public void SetHairstyle(CustomizationSO option)
        {
            _hairStyle = option;
            OnCharacterCustomizationChanged?.Invoke(CustomizationType.Hairstyle);
        }
        
        public void SetBeardstyle(CustomizationSO option)
        {
            _beardStyle = option;
            OnCharacterCustomizationChanged?.Invoke(CustomizationType.Beardstyle);
        }

        public void SetEyebrows(CustomizationSO option)
        {
            _eyebrows = option;
            OnCharacterCustomizationChanged?.Invoke(CustomizationType.Eyebrows);
        }

        public void SetHairColor(Color option)
        {
            _hairColor = option;
            OnCharacterCustomizationChanged?.Invoke(CustomizationType.HairColor);
        }

        public void SetEyeColor(Color option)
        {
            _eyeColor = option;
            OnCharacterCustomizationChanged?.Invoke(CustomizationType.EyeColor);
        }

        public void SetSkinColor(Color option)
        {
            _skinColor = option;
            OnCharacterCustomizationChanged?.Invoke(CustomizationType.SkinColor);
        }

    }
}