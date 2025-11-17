using SS3D.Core.Behaviours;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using SS3D.Data;
using SS3D.Attributes;
using SS3D.Systems.Characters.Preferences;
using System;
using System.Collections.Generic;

namespace SS3D.Systems.Characters
{

    /// <summary>
    /// Stores the name and appearance of a humanoid entity
    /// 
    /// TODO: Need to create setters at some point
    /// 
    /// </summary>
    public class UniqueIdentifiers : NetworkActor
    {
        /// <summary>
        /// The name of this character.
        /// </summary>
        [SyncVar(OnChange = nameof(SyncCharacterName))]
        private string _name = string.Empty;

        [SyncVar(OnChange = nameof(SyncFlavorText))]
        private string _flavorText = string.Empty;

        [SyncVar(OnChange = nameof(SyncStyles))]
        private Dictionary<StyleType, string> _styles = new();

        [SyncVar(OnChange = nameof(SyncColors))]
        private Dictionary<ColorType, string> _colors = new();

        [SyncVar(OnChange = nameof(SyncBody))]
        private Dictionary<BodyType, string> _body = new();

        // [SyncVar(OnChange = nameof(SyncTraits))]
        private List<string> _traits = new();

        /// <summary>
        /// Renderer that will display the characters appearance.
        /// </summary>
        [SerializeField] [NotNull] private AppearanceDisplayer _appearanceDisplayer;

        public string Name => _name;

        public string FlavorText => _flavorText;

        #region Setters

        /// <summary>
        /// Set the characters name
        /// </summary>
        [Server]
        public void SetName(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Called when the entity is spawned
        /// </summary>
        [Server]
        public void SetAppearanceFromProfile(CharacterProfile profile)
        {
            _flavorText = profile.FlavorText;
            _styles = profile.Styles;
            _colors = profile.Colors;
            _body = profile.Body;
            _traits = profile.Traits;
        }

        #endregion

        #region Syncing

        /// <summary>
        /// Callback when the characters name is changed
        /// </summary>
        [ServerOrClient]
        private void SyncCharacterName(string oldName, string newName, bool asServer)
        {
            gameObject.name = newName;
        }

        [Client]
        private void SyncFlavorText(string oldChar, string newChar, bool asServer)
        {
            // throw new NotImplementedException();
        }

        [Client]
        private void SyncStyles(Dictionary<StyleType, string> oldChar, Dictionary<StyleType, string> newChar, bool asServer)
        {
            foreach (int i in Enum.GetValues(typeof(StyleType)))
            {
                StyleType type = (StyleType)i;

                SetVisualStyle(type);
            }
        }

        [Client]
        private void SyncColors(Dictionary<ColorType, string> oldChar, Dictionary<ColorType, string> newChar, bool asServer)
        {
            foreach (int i in Enum.GetValues(typeof(ColorType)))
            {
                ColorType type = (ColorType)i;

                SetVisualColor(type);
            }
        }

        [ServerOrClient]
        private void SyncBody(Dictionary<BodyType, string> oldChar, Dictionary<BodyType, string> newChar, bool asServer)
        {
            foreach (int i in Enum.GetValues(typeof(BodyType)))
            {
                BodyType type = (BodyType)i;
                _appearanceDisplayer.SetBody(type, float.Parse(_body[type]));
            }

        }

        #endregion

        #region Set Appearance

        [Client]
        private void SetVisualStyle(StyleType type)
        {
            CustomizationSO option = Assets.Get<CustomizationSO>("Customization", _styles[type]);
            if (option == null) return;

            _appearanceDisplayer.SetStyle(type, option);
        }

        [Client]
        private void SetVisualColor(ColorType type)
        {
            if (!ColorUtility.TryParseHtmlString("#" + _colors[type], out Color color)) return;

            _appearanceDisplayer.SetColor(type, color);
        }

        #endregion
    }
}