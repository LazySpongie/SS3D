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
    /// this demon controls the character creation ui and sends the players changes to ClientPreferencesSubSystem
    /// </summary>
    public sealed class CharacterSelectionView : Actor
    {
        [Header("Selection")]
        [SerializeField] [NotNull] private CharacterList _characterSelection;
        
        private ClientPreferencesSubSystem _preferences;

        private List<string> _characterNames = new();

        private int _characterIndex;

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
            AddHandle(LocalLobbyCharacterListChanged.AddListener(HandleCharactersLoaded));
            
            _characterSelection.OnCharacterListStarted += HandleCharacterListStarted;
            _characterSelection.OnCharacterSelected += HandleCharacterSelectionChanged;
            _characterSelection.OnCharacterDeleted += HandleDeleteButtonPressed;
            _characterSelection.OnCreateCharacter += HandleCharacterCreated;
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();

            _characterSelection.OnCharacterListStarted -= HandleCharacterListStarted;
            _characterSelection.OnCharacterSelected -= HandleCharacterSelectionChanged;
            _characterSelection.OnCharacterDeleted -= HandleDeleteButtonPressed;
            _characterSelection.OnCreateCharacter -= HandleCharacterCreated;
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
                case ScreenType.CharacterCreation:
                    // switched to char creation
                    _characterSelection.SetSelectedOption(_characterIndex);
                    break;
            }
        }

        /// <summary>
        /// Set the ui when the character is modified.
        /// </summary>
        private void HandleCharacterChanged(ref EventContext context, in LocalLobbyCharacterChanged e)
        {
            _characterIndex = e.Index;
            switch (e.ChangeType)
            {
                case CharacterChangeType.Load:
                    _characterSelection.SetSelectedOption(_characterIndex);
                    break;
            }
        }

        /// <summary>
        /// Method called when the character list object is loaded.
        /// </summary>
        private void HandleCharacterListStarted()
        {
            _characterSelection.LoadList(_characterNames);
            _characterSelection.SetSelectedOption(_characterIndex);
        }

        /// <summary>
        /// Fill the character selection list with character names.
        /// </summary>
        private void HandleCharactersLoaded(ref EventContext context, in LocalLobbyCharacterListChanged loaded)
        {
            _characterNames = loaded.Names;
            _characterSelection.LoadList(_characterNames);
        }

        #endregion

        #region Set Character

        /// <summary>
        /// Callback when the player clicks to create a new character.
        /// </summary>
        private void HandleCharacterCreated()
        {
            _preferences.CreateCharacter();
        }

        /// <summary>
        /// Callback when the player clicks to delete a character.
        /// </summary>
        private void HandleDeleteButtonPressed(int index)
        {
            _preferences.DeleteCharacter(index);
        }

        /// <summary>
        /// Callback when the player clicks to select a different character.
        /// </summary>
        private void HandleCharacterSelectionChanged(int index)
        {
            _preferences.SelectCharacter(index);
        }
        
        #endregion
        
    }
}
