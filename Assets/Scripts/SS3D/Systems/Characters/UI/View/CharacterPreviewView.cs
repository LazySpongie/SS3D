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
using SS3D.Systems.Roles;
using SS3D.Systems.Inventory.Clothing;
using SS3D.Systems.Inventory.Containers;
using SS3D.Systems.Inventory.Items;
using SS3D.UI.Buttons;

namespace SS3D.Systems.Characters.UI.View
{
    /// <summary>
    /// Controls the preview screen, save button, and reset button
    /// </summary>
    public sealed class CharacterPreviewView : Actor
    {
        [Header("Preview")]
        [SerializeField] [NotNull] private AppearanceDisplayer _previewAppearance;
        [SerializeField] [NotNull] private ClothingVisualDisplayer _previewClothing;
        [SerializeField] [NotNull] private PreviewCamera _previewCamera;
        [SerializeField] [NotNull] private List<TMP_Text> _previewNameTexts;
        [SerializeField] [NotNull] private ToggleLabelButton _showClothingButton;


        [Header("Save/Load Buttons")]
        [SerializeField] [NotNull] private Button _saveButton;
        [SerializeField] [NotNull] private Button _resetButton;

        private ClientPreferencesSubSystem _preferences;

        private RoleData _currentRole;
        private Dictionary<ContainerType, ItemVisualData> _currentClothing = new();
        private bool _showClothing = true;

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
            _resetButton.onClick.AddListener(HandleResetButtonPressed);
            _showClothingButton.OnPressedDown += HandleClothingButtonPressed;
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();

            _saveButton.onClick.RemoveListener(HandleSaveButtonPressed);
            _resetButton.onClick.RemoveListener(HandleResetButtonPressed);
            _showClothingButton.OnPressedDown -= HandleClothingButtonPressed;
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
                    if (!_showClothing) _showClothingButton.Press();
                    break;
            }
        }

        /// <summary>
        /// Method called when something about the character profile has been changed.
        /// </summary>
        private void HandleCharacterChanged(ref EventContext context, in LocalLobbyCharacterChanged e)
        {
            SetSaveAndResetButtonsActive(true);
            switch (e.ChangeType)
            {
                case CharacterChangeType.Load:
                    HandleCharacterNameChanged(e.Character);
                    HandleAppearanceChanged(e.Character);
                    SetSaveAndResetButtonsActive(false);
                    HandleRolesChanged(e.Character);
                    break;
                case CharacterChangeType.Names:
                    HandleCharacterNameChanged(e.Character);
                    SetSaveAndResetButtonsActive(true);
                    break;
                case CharacterChangeType.Appearance:
                    HandleAppearanceChanged(e.Character);
                    SetSaveAndResetButtonsActive(true);
                    break;
                case CharacterChangeType.Roles:
                    SetSaveAndResetButtonsActive(true);
                    
                    HandleRolesChanged(e.Character);
                    break;
                case CharacterChangeType.Antags:
                case CharacterChangeType.Loadout:
                case CharacterChangeType.Background:
                    SetSaveAndResetButtonsActive(true);
                    break;
            }
        }

        private void HandleRolesChanged(CharacterProfile character)
        {
            if (!Assets.TryGet("Roles", character.FavoriteRole, out RoleData role)) return;
            if (_currentRole == role) return;
            _currentRole = role;

            ClearClothing();
            if (!_showClothing) return;
            EquipRoleLoadout();
        }

        private void HandleClothingButtonPressed(bool pressed)
        {
            _showClothing = pressed;
            if (_showClothing)
            {
                EquipRoleLoadout();
                return;
            }
            ClearClothing();
        }

        private void EquipRoleLoadout()
        {
            RoleLoadout loadout = _currentRole.Loadout;

            foreach (KeyValuePair<ContainerType, GameObject> pair in loadout.Equipment)
            {
                ItemVisualData data = pair.Value.GetComponent<Item>().StartingItemVisualData;
                if (data == null) continue;
                _previewClothing.AddItem(pair.Key, data);
                _currentClothing.Add(pair.Key, data);
            }
        }

        private void ClearClothing()
        {
            if (_currentClothing.Count == 0) return;
            foreach (KeyValuePair<ContainerType, ItemVisualData> pair in _currentClothing)
            {
                _previewClothing.RemoveItem(pair.Key);
            }
            _currentClothing.Clear();
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
                CustomizationSO option = Assets.Get<CustomizationSO>("Customization", character.GetStyle(type));
                _previewAppearance.SetStyle(type, option);
            }

            foreach (int i in Enum.GetValues(typeof(ColorType)))
            {
                ColorType type = (ColorType)i;
                if (!ColorUtility.TryParseHtmlString("#" + character.GetColor(type), out Color color)) break;
                _previewAppearance.SetColor(type, color); 
            }

            foreach (int i in Enum.GetValues(typeof(BodyType)))
            {
                BodyType type = (BodyType)i;
                _previewAppearance.SetBody(type, float.Parse(character.GetBody(type)));
                
            }
        }

        private void SetSaveAndResetButtonsActive(bool active)
        {
            _saveButton.interactable = active;
            _resetButton.interactable = active;
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
