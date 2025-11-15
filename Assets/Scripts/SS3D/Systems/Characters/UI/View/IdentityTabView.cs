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
    /// Controls the character name and background in the character creation ui
    /// </summary>
    public sealed class IdentityTabView : Actor
    {
        [Header("Selections")]
        [SerializeField][NotNull] private List<CharacterNameSlot> _nameSlots;
        
        [SerializeField][NotNull] private TMP_InputField _flavorTextField;

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

            AddHandle(LocalLobbyCharacterChanged.AddListener(HandleCharacterChanged));
            
            _nameSlots.ForEach(slot => slot.OnValueChanged += HandleNameChanged);
            _flavorTextField.onEndEdit.AddListener(HandleFlavorTextChanged);
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();

            _nameSlots.ForEach(slot => slot.OnValueChanged -= HandleNameChanged);
        }

        #endregion

        #region Update UI

        /// <summary>
        /// Set the ui when the character is modified.
        /// </summary>
        private void HandleCharacterChanged(ref EventContext context, in LocalLobbyCharacterChanged e)
        {
            switch (e.ChangeType)
            {
                case CharacterChangeType.Load:
                    SetNameSlots(e.Character.Names);
                    break;
                case CharacterChangeType.Names:
                    // SetNameTextField(e.Character.Name);
                    break;
            }
        }

        /// <summary>
        /// Update the characters name in the ui.
        /// </summary>
        private void SetNameSlots(Dictionary<CharacterNameType, string> names)
        {
            _nameSlots.ForEach(slot => {
                slot.SetValue(names[slot.Type]);
            });
        }

        #endregion

        #region Set Character

        /// <summary>
        /// Callback when the character name text field is changed.
        /// </summary>
        private void HandleNameChanged(CharacterNameType type, string name)
        {
            _preferences.SetName(type, name);
        }

        /// <summary>
        /// Callback when the flavor text field is changed.
        /// </summary>
        private void HandleFlavorTextChanged(string text)
        {
            _preferences.SetFlavorText(text);
        }


        #endregion
        
    }
}
