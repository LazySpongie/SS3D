using SS3D.Core.Behaviours;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using SS3D.Data;
using SS3D.Attributes;
using SS3D.Systems.Characters.Preferences;
using System;
using SS3D.Logging;

namespace SS3D.Systems.Characters
{

    /// <summary>
    /// Script on an entity that stores the current appearance and sends it to AppearanceDisplayer when changed
    /// 
    /// TODO: Need to create setters at some point
    /// 
    /// </summary>
    public class UniqueIdentifiers : NetworkActor
    {
        /// <summary>
        /// The name and appearance of this character.
        /// </summary>
        [SyncVar(OnChange = nameof(SyncCharacterProfile))]
        private CharacterProfile _profile;

        /// <summary>
        /// Renderer that will display the characters appearance.
        /// </summary>
        [SerializeField] [NotNull] private AppearanceDisplayer _appearanceDisplayer;

        /// <summary>
        /// The name of this character.
        /// </summary>
        public string Name => _profile.Name;

        /// <summary>
        /// Called when the entity is spawned
        /// </summary>
        [Server]
        public void SetFromCharacterProfile(CharacterProfile character)
        {
            _profile = new CharacterProfile(character);
        }

        #region Syncing

        /// <summary>
        /// Callback when the characters profile is modified
        /// </summary>
        [Client]
        private void SyncCharacterProfile(CharacterProfile oldChar, CharacterProfile newChar, bool asServer)
        {

            gameObject.name = newChar.Name;

            foreach (int i in Enum.GetValues(typeof(StyleType)))
            {
                StyleType type = (StyleType)i;

                SetVisualStyle(type);
            }

            foreach (int i in Enum.GetValues(typeof(ColorType)))
            {
                ColorType type = (ColorType)i;

                SetVisualColor(type);
            }

            foreach (int i in Enum.GetValues(typeof(BodyType)))
            {
                BodyType type = (BodyType)i;
                _appearanceDisplayer.SetBody(type, float.Parse(newChar.GetBody(type)));
            }

            // body sliders here
        }

        #endregion

        #region Set Visuals

        private void SetVisualStyle(StyleType type)
        {
            CustomizationSO option = Assets.Get<CustomizationSO>("Customization", _profile.GetStyle(type));
            if (option == null) return;

            _appearanceDisplayer.SetStyle(type, option);
        }

        private void SetVisualColor(ColorType type)
        {
            if (!ColorUtility.TryParseHtmlString("#" + _profile.GetColor(type), out Color color)) return;

            _appearanceDisplayer.SetColor(type, color);
        }

        #endregion
    }
}