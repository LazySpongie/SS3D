using System.Collections.Generic;
using UnityEngine;
using SS3D.Core;
using SS3D.Core.Behaviours;
using FishNet.Object;
using SS3D.Data.Management;
using SS3D.Systems.PlayerControl;
using SS3D.Systems.Characters.Messages;
using SS3D.Logging;
using SS3D.Systems.Rounds.Events;
using Coimbra.Services.Events;
using System;
using SS3D.Systems.Rounds;

namespace SS3D.Systems.Characters.Preferences
{
    /// <summary>
    /// Controls the players character in character creation
    /// </summary>
    public class ClientPreferencesSubSystem : NetworkSubSystem
    {
        public delegate void CharacterChangedHandler(CharacterChangeType type);

        public delegate void CharacterSelectedHandler(int index);

        public delegate void CharactersLoadedHandler();

        public event CharacterChangedHandler OnCharacterChanged;

        public event CharacterSelectedHandler OnCharacterSelected;
        
        public event CharactersLoadedHandler OnCharactersLoaded;

	    public const string SavePath = "/Characters";

        private Dictionary<int, CharacterProfile> _characters = new();

        private int _selectedCharacterIndex = 0;

        private CharacterProfile _unsavedCharacter;

        public int SelectedCharacterIndex => _selectedCharacterIndex;

        public Dictionary<int, CharacterProfile> Characters => _characters;
        
        public CharacterProfile SelectedCharacter
        {
            get { return _characters[_selectedCharacterIndex]; }
        }

        public CharacterProfile UnsavedCharacter => _unsavedCharacter;

        protected override void OnAwake()
        {
            base.OnAwake();
            if (IsServer) return;
            LoadCharactersFromDisk();
            AddHandle(RoundStateUpdated.AddListener(HandleRoundStateUpdated));
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            SelectCharacter(0);
        }

        #region Character Save/Load

        /// <summary>
        /// Method called when the load character button is clicked.
        /// </summary>
        [Client]
        public void LoadCharactersFromDisk()
        {
            _characters.Clear();
            List<string> savedChars = LocalStorage.GetAllObjectsNameInFolder(SavePath);

            int i = 0;
            while (true)
            {
                string filePath = SavePath + "/Character" + i;

                CharacterProfile loadedcharacter =
                    LocalStorage.LoadObject<CharacterProfile>(filePath);

                // Validate 
                if (loadedcharacter != null)
                {
                    _characters.Add(i, loadedcharacter);
                }
                else
                {
                    break;
                }

                i++;
            }

            // No valid characters so create a default character
            if (_characters.Count == 0)
            {
                CreateCharacter();
            }

            OnCharactersLoaded?.Invoke();
        }

        /// <summary>
        /// Sets a new selected character and sends it to the server.
        /// </summary>
        [Client]
        public void SelectCharacter(int index)
        {
            _selectedCharacterIndex = index;
            ResetCharacter();
        }

        /// <summary>
        /// Method called when the save character button is pressed.
        /// </summary>
        [Client]
        public void SaveCharacter()
        {
            _characters[_selectedCharacterIndex] = _unsavedCharacter;
            bool overwrite = true;
            LocalStorage.SaveObject(SavePath + "/Character" + _selectedCharacterIndex, _unsavedCharacter, overwrite);

            SelectCharacter(_selectedCharacterIndex);
        }

        /// <summary>
        /// Reset the unsaved character to the last save.
        /// </summary>
        [Client]
        public void ResetCharacter()
        {
            _unsavedCharacter = new CharacterProfile(_characters[_selectedCharacterIndex]);
            OnCharacterChanged?.Invoke(CharacterChangeType.Everything);
        }

        /// <summary>
        /// Method called when the create character button is pressed.
        /// </summary>
        [Client]
        public void CreateCharacter()
        {
            int index = _characters.Count;
            CharacterProfile newChar = new CharacterProfile();
            LocalStorage.SaveObject(SavePath + "/Character" + index, newChar, true);

            LoadCharactersFromDisk();
            SelectCharacter(_characters.Count - 1);
        }

        #endregion

        #region Character Setters
        
        /// <summary>
        /// Set an appearance option in the current character.
        /// </summary>
        [Client]
        public void SetAppearanceOption(AppearanceType type, string option, bool invoke = true)
        {
            _unsavedCharacter.Appearance[type] = option;
            if (!invoke) return;
            OnCharacterChanged?.Invoke(CharacterChangeType.Appearance);
        }

        /// <summary>
        /// Set the current characters name.
        /// </summary>
        public void SetCharacterName(string name)
        {
            _unsavedCharacter.Name = name;
            OnCharacterChanged?.Invoke(CharacterChangeType.Name);
        }

        /// <summary>
        /// Set the current characters name.
        /// </summary>
        public void SetColor(AppearanceType type, string color)
        {
            _unsavedCharacter.Appearance[type] = color;
            OnCharacterChanged?.Invoke(CharacterChangeType.Appearance);
        }
        #endregion
        
        #region Networking

        /// <summary>
        /// Callback when the round is starting.
        /// </summary>
        [Client]
        private void HandleRoundStateUpdated(ref EventContext context, in RoundStateUpdated e)
        {
            if (e.RoundState != RoundState.Preparing) return;
            SendSelectedCharacterToServer();
        }

        /// <summary>
        /// Send the character to CharacterSubSystem.
        /// Every client doing this at the same time could cause performance issues
        /// </summary>
        [Client]
        private void SendSelectedCharacterToServer()
        {
            // send character to CharacterSubSystem
            PlayerSubSystem playerSystem = SubSystems.Get<PlayerSubSystem>();
            string ckey = playerSystem.GetCkey(LocalConnection);
            PlayerSelectCharacterMessage selectCharacterMessage = new(ckey, SelectedCharacter);
            ClientManager.Broadcast(selectCharacterMessage);
        }
        #endregion

    }
}