using SS3D.Core.Behaviours;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using SS3D.Data;
using SS3D.Systems.Characters.Preferences;
using System;
using System.Collections.Generic;

namespace SS3D.Systems.Characters
{

    /// <summary>
    /// Stores the appearance of a humanoid entity
    /// 
    /// TODO: Need to create setters at some point
    /// 
    /// </summary>
    public class CharacterAppearance : NetworkActor
    {
        /// <summary>
        /// The hairstyle, facial hair, and eyebrows.
        /// </summary>
        [SyncVar(OnChange = nameof(SyncStyles))]
        private Dictionary<StyleType, string> _styles = new();

        /// <summary>
        /// Hair, eye, skin color.
        /// </summary>
        [SyncVar(OnChange = nameof(SyncColors))]
        private Dictionary<ColorType, string> _colors = new();

        /// <summary>
        /// Body part sizes.
        /// </summary>
        [SyncVar(OnChange = nameof(SyncBody))]
        private Dictionary<BodyType, string> _body = new();

        private AppearanceDisplayer _appearanceDisplayer;
        
        protected override void OnAwake()
        {
            base.OnAwake();
            _appearanceDisplayer = GetComponent<AppearanceDisplayer>();
        }

        #region Setters

        /// <summary>
        /// Called when the entity is spawned
        /// </summary>
        [Server]
        public void SetAppearanceFromProfile(CharacterProfile profile)
        {
            _styles = profile.Styles;
            _colors = profile.Colors;
            _body = profile.Body;
        }

        #endregion

        #region Syncing

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