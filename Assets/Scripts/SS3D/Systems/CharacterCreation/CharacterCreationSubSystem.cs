using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using SS3D.Core;
using SS3D.Core.Behaviours;
using SS3D.Attributes;
using FishNet.Object;
using SS3D.Systems.Entities;
using SS3D.Systems.Entities.Events;
using Coimbra.Services.Events;
using SS3D.Logging;
using System;

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
        public delegate void CharacterCustomizationChangedHandler();

        public event CharacterCustomizationChangedHandler OnCharacterCustomizationChanged;

        [Header("Default Names")]
        [SerializeField] public string defaultCharacterName;

        [Header("Default Styles")]
        [SerializeField] public CustomizationSO defaultHair;
        [SerializeField] public CustomizationSO defaultBeard;
        [SerializeField] public CustomizationSO defaultEyebrows;

        [Header("Default Colors")]
        [SerializeField] public List<Color> hairColours;
        [SerializeField] public List<Color> eyeColors;
        [SerializeField] public List<Color> skinColors;

        private int _currentHairColor = 0;
        private int _currentEyeColor = 0;
        private int _currentSkinColor = 0;

        private Dictionary<CustomizationType, string> _currentCustomization = new Dictionary<CustomizationType, string>();

        public Dictionary<CustomizationType, string> CurrentCustomization => _currentCustomization;

        protected override void OnAwake()
        {
            base.OnAwake();

            SetDefault();

        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            OnCharacterCustomizationChanged?.Invoke();
            
            AddHandle(SpawnedPlayersUpdated.AddListener(HandleSpawnedPlayersUpdated));
        }

        [Client]
        private void HandleSpawnedPlayersUpdated(ref EventContext context, in SpawnedPlayersUpdated e)
        {
            AddCustomizationToPlayer();
        }

        [Client]
        private void AddCustomizationToPlayer()
        {
            EntitySubSystem system = SubSystems.Get<EntitySubSystem>();

            if (!system.TryGetSpawnedEntity(LocalConnection, out Entity entity)) return;

            entity.GetComponent<UniqueIdentifiers>()?.SetCustomization(_currentCustomization);
            //add customization here
        }

        /// <summary>
        /// Method called when the save character button is clicked.
        /// </summary>
        [Client]
        public void SetDefault()
        {
            SetCustomizationOption(CustomizationType.CharacterName, defaultCharacterName, false);
            SetCustomizationOption(CustomizationType.Hairstyle, defaultHair.name, false);
            SetCustomizationOption(CustomizationType.Beardstyle, defaultBeard.name, false);
            SetCustomizationOption(CustomizationType.Eyebrows, defaultEyebrows.name, false);

            SetCustomizationOption(CustomizationType.HairColor, ColorUtility.ToHtmlStringRGB(hairColours[_currentHairColor]), false);
            SetCustomizationOption(CustomizationType.EyeColor, ColorUtility.ToHtmlStringRGB(eyeColors[_currentEyeColor]), false);
            SetCustomizationOption(CustomizationType.SkinColor, ColorUtility.ToHtmlStringRGB(skinColors[_currentSkinColor]), false);
        }

        [Client]
        public void SetCustomizationOption(CustomizationType type, string option, bool invoke = true)
        {
            _currentCustomization[type] = option;
            // Log.Information(this, type + " has value: " + _currentCustomization[type]);

            if (!invoke) return;
            OnCharacterCustomizationChanged?.Invoke();
        }

        [Client]
        public void ColorButtonOnClick(CustomizationType type)
        {
            // placeholder
            switch (type)
            {
                case CustomizationType.HairColor:
                    _currentHairColor++;
                    if (_currentHairColor >= hairColours.Count) _currentHairColor = 0;
                    SetCustomizationOption(type, ColorUtility.ToHtmlStringRGB(hairColours[_currentHairColor]));
                    break;

                case CustomizationType.EyeColor:
                    _currentEyeColor++;
                    if (_currentEyeColor >= eyeColors.Count) _currentEyeColor = 0;
                    SetCustomizationOption(type, ColorUtility.ToHtmlStringRGB(eyeColors[_currentEyeColor]));
                    break;

                case CustomizationType.SkinColor:
                    _currentSkinColor++;
                    if (_currentSkinColor >= skinColors.Count) _currentSkinColor = 0;
                    SetCustomizationOption(type, ColorUtility.ToHtmlStringRGB(skinColors[_currentSkinColor]));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Method called when the load character button is clicked.
        /// </summary>
        [Client]
        public void HandleLoadButton()
        {
            // set hairstyle selection here
        }

        /// <summary>
        /// Method called when the save character button is clicked.
        /// </summary>
        [Client]
        public void HandleSaveButton()
        {

        }

    }
}