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
        [SerializeField][NotNull] private TMP_InputField _characterNameSelection;

        #region Setup

        protected override void OnAwake()
        {
            base.OnAwake();

            AddHandle(LocalLobbyCharacterChanged.AddListener(HandleCharacterChanged));
            
            _characterNameSelection.onValueChanged.AddListener(HandleNameFieldChanged);
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();

            _characterNameSelection.onValueChanged.RemoveListener(HandleNameFieldChanged);
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
                    SetNameTextField(e.Character.Name);
                    break;
                case CharacterChangeType.Name:
                    // SetNameTextField(e.Character.Name);
                    break;
            }
        }

        /// <summary>
        /// Update the characters name in the ui.
        /// </summary>
        private void SetNameTextField(string name)
        {
            _characterNameSelection.SetTextWithoutNotify(name);
        }

        #endregion

        #region Set Character

        /// <summary>
        /// Callback when the character name text field is changed.
        /// </summary>
        private void HandleNameFieldChanged(string name)
        {
            SubSystems.Get<ClientPreferencesSubSystem>().SetCharacterName(name);
        }

        #endregion
        
    }
}
