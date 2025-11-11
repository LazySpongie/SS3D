using SS3D.Core;
using SS3D.Core.Behaviours;
using UnityEngine;
using SS3D.Attributes;
using TMPro;
using System.Collections.Generic;
using SS3D.Data;
using System;
using UnityEngine.UI;
using SS3D.Systems.Characters.Preferences;
using SS3D.Systems.Screens.Events;
using SS3D.Systems.Screens;
using SS3D.Systems.Characters.Events;
using Coimbra.Services.Events;

namespace SS3D.Systems.Characters.UI.View
{
    /// <summary>
    /// Controls the preview screen, save button, and reset button
    /// </summary>
    public sealed class CharacterPreviewView : Actor
    {
        [Header("Preview")]
        [SerializeField] [NotNull] private AppearanceDisplayer _previewCharacter;
        [SerializeField] [NotNull] private PreviewCamera _previewCamera;
        [SerializeField] [NotNull] private List<TMP_Text> _previewNameTexts;

        [Header("Save/Load Buttons")]
        [SerializeField] [NotNull] private Button _saveButton;
        [SerializeField] [NotNull] private Button _ResetButton;

        private ClientPreferencesSubSystem _preferences;

        #region Setup

        protected override void OnStart()
        {
            base.OnStart();
            _preferences = SubSystems.Get<ClientPreferencesSubSystem>();
        }

        protected override void OnAwake()
        {
            base.OnAwake();

            AddHandle(GameScreenChanged.AddListener(HandleChangeGameScreen));

            AddHandle(LocalLobbyCharacterChanged.AddListener(HandleCharacterChanged));
            
            _saveButton.onClick.AddListener(HandleSaveButtonPressed);
            _ResetButton.onClick.AddListener(HandleResetButtonPressed);
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();

            _saveButton.onClick.RemoveListener(HandleSaveButtonPressed);
            _ResetButton.onClick.RemoveListener(HandleResetButtonPressed);
        }

        #endregion

        #region Update UI

        /// <summary>
        /// Method called when the game screen is changed
        /// </summary>
        private void HandleChangeGameScreen(ref EventContext context, in GameScreenChanged e)
        {
            ScreenType screenType = e.ActiveScreen;
            switch (screenType)
            {
                case ScreenType.Lobby:
                    // switched to lobby from character creation so reset the preview
                    if (e.LastScreen != ScreenType.CharacterCreation) break;
                    _previewCamera.ResetCamera();
                    _preferences.ResetCharacter();
                    break;
            }
        }

        /// <summary>
        /// Method called when something about the character profile has been changed.
        /// </summary>
        private void HandleCharacterChanged(ref EventContext context, in LocalLobbyCharacterChanged e)
        {
            switch (e.ChangeType)
            {
                case CharacterChangeType.Load:
                    HandleCharacterNameChanged(e.Character);
                    HandleAppearanceChanged(e.Character);
                    break;
                case CharacterChangeType.Name:
                    HandleCharacterNameChanged(e.Character);
                    break;
                case CharacterChangeType.Appearance:
                    HandleAppearanceChanged(e.Character);
                    break;
            }
        }

        /// <summary>
        /// Update the characters name in the ui.
        /// </summary>
        private void HandleCharacterNameChanged(CharacterProfile character)
        {
            foreach (TMP_Text text in _previewNameTexts)
            {
                text.text = character.Name;
            }
        }

        /// <summary>
        /// Update the preview appearance.
        /// </summary>
        private void HandleAppearanceChanged(CharacterProfile character)
        {
            foreach (int i in Enum.GetValues(typeof(StyleType)))
            {
                StyleType type = (StyleType)i;
                switch (type)
                {
                    case StyleType.Hairstyle:
                    case StyleType.Beardstyle:
                    case StyleType.Eyebrows:
                        CustomizationSO option = Assets.Get<CustomizationSO>("Customization", character.GetStyle(type));
                        _previewCharacter.SetStyle(type, option);
                        break;
                }
            }

            foreach (int i in Enum.GetValues(typeof(ColorType)))
            {
                ColorType type = (ColorType)i;
                switch (type)
                {
                    case ColorType.HairColor:
                    case ColorType.EyeColor:
                    case ColorType.SkinColor:
                        if (!ColorUtility.TryParseHtmlString("#" + character.GetColor(type), out Color color)) break;
                        _previewCharacter.SetColor(type, color);
                        break;
                }
            }
        }

        #endregion

        #region Set Character

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
        public void HandleResetButtonPressed()
        {
            _preferences.ResetCharacter();
        }

        #endregion
        
    }
}
