using SS3D.Core.Behaviours;
using UnityEngine;
using FishNet.Object;
using System.Collections.Generic;
using FishNet.Object.Synchronizing;
using SS3D.Data;
using SS3D.Attributes;
using SS3D.Systems.Characters.Preferences;
using System;

namespace SS3D.Systems.Characters
{

    /// <summary>
    /// Script on an entity that stores the current appearance and sends it to AppearanceDisplayer when changed
    /// </summary>
    public class UniqueIdentifiers : NetworkActor
    {

        /// <summary>
        /// Renderer that will display the characters appearance.
        /// </summary>
        [SerializeField] [NotNull] private AppearanceDisplayer _appearanceDisplayer;
        
        [SyncObject]
        private readonly SyncVar<string> _characterName;
        
        [SyncObject]
        private readonly SyncDictionary<AppearanceType, string> _currentAppearance = new();

        protected override void OnAwake()
        {
            base.OnAwake();
            _currentAppearance.OnChange += SyncAppearance;
            _characterName.OnChange += SyncCharacterName;
        }

        #region Syncing

        /// <summary>
        /// Callback when the players appearance is changed
        /// </summary>
        [Client]
        public void SyncAppearance(SyncDictionaryOperation op, AppearanceType type, string value, bool asServer)
        {
            if (asServer) return;
            
            if (op != SyncDictionaryOperation.Set) return;

            switch (type)
            {
                case AppearanceType.Hairstyle:
                case AppearanceType.Beardstyle:
                case AppearanceType.Eyebrows:
                    CustomizationSO option = Assets.Get<CustomizationSO>("Customization", value);
                    _appearanceDisplayer.SetStyle(type, option);
                    break;

                case AppearanceType.HairColor:
                case AppearanceType.EyeColor:
                case AppearanceType.SkinColor:
                    if (!ColorUtility.TryParseHtmlString("#" + value, out Color color)) break;
                    _appearanceDisplayer.SetColor(type, color);
                    break;
                default:
                    // no code for other customisation types has been added yet
                    break;
            }
        }

        /// <summary>
        /// Callback when the characters name is changed
        /// </summary>
        [ServerOrClient]
        private void SyncCharacterName(string oldName, string newName, bool asServer)
        {
            gameObject.name = newName;
        }

        #endregion

        /// <summary>
        /// Called when the entity is spawned
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SetAppearance(CharacterProfile character)
        {
            _characterName.SetValue(character.Name, false);

            foreach (KeyValuePair<AppearanceType, string> entry in character.Appearance)
            {
                _currentAppearance[entry.Key] = entry.Value;
            }
        }

        /// <summary>
        /// Sets customization option
        /// </summary>
        [Server]
        public void SetCustomizationOption(AppearanceType type, string option)
        {
            _currentAppearance[type] = option;
        }
    }
}