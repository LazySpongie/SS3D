using System.Collections.Generic;
using UnityEngine;
using SS3D.Core;
using SS3D.Core.Behaviours;
using FishNet.Object;
using SS3D.Systems.Entities;
using SS3D.Systems.Entities.Events;
using Coimbra.Services.Events;
using SS3D.Logging;
using Coimbra;
using SS3D.Data.Management;

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

	    public const string SavePath = "/Characters";

	    public const string UnnamedCharacterName = "John Beep";

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

        /// <summary>
        /// Dictionary of saved customization to be used in-game
        /// </summary>
        private SerializableDictionary<CustomizationType, string> _savedCustomization = new SerializableDictionary<CustomizationType, string>();

        /// <summary>
        /// Dictionary of currently selected customization in the character creation screen
        /// </summary>
        private SerializableDictionary<CustomizationType, string> _currentCustomization = new SerializableDictionary<CustomizationType, string>();

        /// <summary>
        /// Dictionary of saved customization to be used in-game
        /// </summary>
        public SerializableDictionary<CustomizationType, string> SavedCustomization => _savedCustomization;

        /// <summary>
        /// Dictionary of currently selected customization in the character creation screen
        /// </summary>
        public SerializableDictionary<CustomizationType, string> CurrentCustomization => _currentCustomization;

        protected override void OnAwake()
        {
            base.OnAwake();

            // SetDefault();
            HandleLoadButton();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            OnCharacterCustomizationChanged?.Invoke();

            AddHandle(SpawnedPlayersUpdated.AddListener(HandleSpawnedPlayersUpdated));
        }

        /// <summary>
        /// Method called when the load character button is clicked.
        /// </summary>
        [Client]
        public void HandleLoadButton()
        {
            _savedCustomization = LocalStorage.LoadObject<SerializableDictionary<CustomizationType, string>>(SavePath + "/" + UnnamedCharacterName);

            SetCurrentCustomizationFromSaved();
        }

        /// <summary>
        /// Method called when the save character button is clicked.
        /// </summary>
        [Client]
        public void HandleSaveButton()
        {
            _savedCustomization = new SerializableDictionary<CustomizationType, string>(_currentCustomization);

            bool overwrite = true;
            LocalStorage.SaveObject(SavePath + "/" + UnnamedCharacterName, _savedCustomization, overwrite);
        }

        /// <summary>
        /// Callback when a player entity is spawned to set their appearance
        /// </summary>
        [Client]
        private void HandleSpawnedPlayersUpdated(ref EventContext context, in SpawnedPlayersUpdated e)
        {
            AddCustomizationToPlayer();
        }

        /// <summary>
        /// When the player is spawned get their entity and send their appearance to the server to be applied
        /// </summary>
        [Client]
        private void AddCustomizationToPlayer()
        {
            EntitySubSystem system = SubSystems.Get<EntitySubSystem>();

            if (!system.TryGetSpawnedEntity(LocalConnection, out Entity entity)) return;

            Dictionary<CustomizationType, string> dict = new();
            foreach (KeyValuePair<CustomizationType, string> entry in _currentCustomization)
            {
                dict[entry.Key] = entry.Value;
            }

            // ServerRPC
            entity.GetComponent<UniqueIdentifiers>()?.SetAppearance(dict);
        }

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
        public void SetCurrentCustomizationFromSaved()
        {
            foreach (KeyValuePair<CustomizationType, string> entry in _savedCustomization)
            {
                SetCustomizationOption(entry.Key, entry.Value, false);
            }

            OnCharacterCustomizationChanged?.Invoke();
        }
        
        /// <summary>
        /// Set a customization option in the current character.
        /// </summary>
        [Client]
        public void SetCustomizationOption(CustomizationType type, string option, bool invoke = true)
        {
            _currentCustomization[type] = option;
            // Log.Information(this, type + " has value: " + _currentCustomization[type]);

            if (!invoke) return;
            OnCharacterCustomizationChanged?.Invoke();
        }

        /// <summary>
        /// Cycle through colors when a color selection button is pressed.
        /// </summary>
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
    }
}