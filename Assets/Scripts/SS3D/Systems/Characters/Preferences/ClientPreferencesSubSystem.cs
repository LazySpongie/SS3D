using System.Collections.Generic;
using SS3D.Core;
using SS3D.Core.Behaviours;
using FishNet.Object;
using SS3D.Data.Management;
using SS3D.Systems.PlayerControl;
using SS3D.Systems.Characters.Messages;
using SS3D.Logging;
using SS3D.Systems.Rounds.Events;
using Coimbra.Services.Events;
using SS3D.Systems.Rounds;
using System.Collections.ObjectModel;
using System.Linq;
using SS3D.Systems.Characters.Events;
using SS3D.Systems.Entities;
using UnityEngine;

namespace SS3D.Systems.Characters.Preferences
{
    /// <summary>
    /// Controls the players character in character creation
    /// </summary>
    public class ClientPreferencesSubSystem : NetworkSubSystem
    {

        public const string CharacterSavePath = "/Characters/Saved";
        
	    public const string CharacterManifestSavePath = "/Characters";

        private List<CharacterProfile> _characters = new();

        private int _selectedCharacterIndex = 0;

        private CharacterProfile _unsavedCharacter;

        public int SelectedCharacterIndex => _selectedCharacterIndex;

        public ReadOnlyCollection<CharacterProfile> Characters => _characters.AsReadOnly();

        public ReadOnlyCollection<string> CharacterNames
        {
            get
            {
                List<string> list = new();
                _characters.ForEach(profile => list.Add(profile.Name));

                return list.AsReadOnly();
            }
        }
        
        public CharacterProfile SelectedCharacter
        {
            get { return _characters[_selectedCharacterIndex]; }
        }

        public CharacterProfile UnsavedCharacter => _unsavedCharacter;

        protected override void OnAwake()
        {
            base.OnAwake();
            if (IsServer) return;
            AddHandle(RoundStateUpdated.AddListener(SendCharacterIfReady));
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            LoadCharactersFromDisk();
            SelectCharacter(_selectedCharacterIndex);
        }

        #region Character Save/Load

        /// <summary>
        /// Load saved characters from the save folder.
        /// </summary>
        [Client]
        public void LoadCharactersFromDisk()
        {
            _characters.Clear();

            _selectedCharacterIndex = 0;

            // need to make a function to validate or repair the char manifest
            
            CharacterProfileManifest characterManifest = LocalStorage.LoadObject<CharacterProfileManifest>(CharacterManifestSavePath + "/CharacterManifest");

            if (characterManifest != null)
            {
                _selectedCharacterIndex = characterManifest.LastSelected;

                foreach (string name in characterManifest.Characters)
                {
                    string filePath = CharacterSavePath + "/" + name;

                    CharacterProfile loadedcharacter =
                        LocalStorage.LoadObject<CharacterProfile>(filePath);

                    if (loadedcharacter == null) continue;

                    _characters.Add(loadedcharacter);
                }
            }

            // No valid characters so create a default character
            if (_characters.Count == 0)
            {
                CreateCharacter(false);
            }
            
            InvokeCharacterListChanged();
        }

        /// <summary>
        /// Select a character from the list of characters.
        /// </summary>
        [Client]
        public void SelectCharacter(int index)
        {
            _selectedCharacterIndex = index;
            ResetCharacter();
        }

        /// <summary>
        /// Save the changes to the current character.
        /// </summary>
        [Client]
        public void SaveCharacter()
        {
            // To prevent overwriting save files we have to make sure the name is unique
            if (CharacterNames.Contains(_unsavedCharacter.Name) & SelectedCharacter.Name != _unsavedCharacter.Name)
            {
                Log.Warning(this, "Duplicate character name: " + _unsavedCharacter.Name + " - cannot save.");
                return;
            }

            // if the character is being renamed we need to rename the save file
            if (SelectedCharacter.Names[CharacterNameType.Normal] != _unsavedCharacter.Names[CharacterNameType.Normal])
            {
                string oldfile = CharacterSavePath + "/" + SelectedCharacter.Name;
                string newfile = CharacterSavePath + "/" + _unsavedCharacter.Name;
                LocalStorage.RenameFile(oldfile, newfile);
            }

            _characters[_selectedCharacterIndex] = _unsavedCharacter;

            SaveCharacterToDisk(SelectedCharacter);

            // the characters name may have been changed so the list has changed as well
            InvokeCharacterListChanged();

            SelectCharacter(_selectedCharacterIndex);
        }

        /// <summary>
        /// Reset the unsaved character to the last save.
        /// </summary>
        [Client]
        public void ResetCharacter()
        {
            _unsavedCharacter = new CharacterProfile(SelectedCharacter);

            InvokeCharacterChanged(CharacterChangeType.Load);
        }

        /// <summary>
        /// Create a new character.
        /// </summary>
        [Client]
        public void CreateCharacter(bool select = true)
        {
            CharacterProfile newChar = new CharacterProfile();

            // need to avoid overwriting other characters
            newChar.Names[CharacterNameType.Normal] = GetNewCharacterFileName(newChar.Name);

            _characters.Add(newChar);
            _selectedCharacterIndex = _characters.Count - 1;
            
            SaveCharacterToDisk(newChar);

            LoadCharactersFromDisk();

            if (!select) return;
            SelectCharacter(_characters.Count - 1);
        }

        /// <summary>
        /// Delete a specified character.
        /// </summary>
        [Client]
        public void DeleteCharacter(int index)
        {
            if (index == _selectedCharacterIndex) return;

            LocalStorage.DeleteFile(CharacterSavePath + "/" + _characters[index].Name);

            CharacterProfile selectedChar = SelectedCharacter;
            _characters.RemoveAt(index);
            _selectedCharacterIndex = _characters.IndexOf(selectedChar);

            UpdateCharacterProfileManifest();

            // LoadCharactersFromDisk();
            InvokeCharacterListChanged();

            SelectCharacter(_selectedCharacterIndex);
        }

        /// <summary>
        /// Save the character profile to disk and update the manifest.
        /// </summary>
        [Client]
        private void SaveCharacterToDisk(CharacterProfile character)
        {
            UpdateCharacterProfileManifest();

            LocalStorage.SaveObject(CharacterSavePath + "/" + character.Name, character, true);
            character.AfterSerialize();
        }

        /// <summary>
        /// Return a unique name if a file with the same name already exists.
        /// </summary>
        [Client]
        private string GetNewCharacterFileName(string name)
        {
            string newName = name;
            int i = 1;
            while (true)
            {
                if (LocalStorage.FolderAlreadyContainsName(CharacterSavePath, newName))
                {
                    // character1, character2, character3, etc
                    newName = name + i;
                    i++;
                }
                else
                {
                    return newName;
                }
            }
        }

        /// <summary>
        /// Keep track of every saved character
        /// </summary>
        [Client]
        private void UpdateCharacterProfileManifest()
        {
            CharacterProfileManifest characterManifest = new CharacterProfileManifest(_selectedCharacterIndex, CharacterNames.ToList());
            
            LocalStorage.SaveObject(CharacterManifestSavePath + "/CharacterManifest", characterManifest, true);
        }

        #endregion

        #region Character Setters

        /// <summary>
        /// Set a style option in the current character.
        /// </summary>
        [Client]
        public void SetStyle(StyleType type, string option)
        {
            if (option == string.Empty) return;

            _unsavedCharacter.Styles[type] = option;

            InvokeCharacterChanged(CharacterChangeType.Appearance);
        }

        /// <summary>
        /// Set the characters skintone.
        /// </summary>
        [Client]
        public void SetSkinTone(float tone, string color)
        {
            _unsavedCharacter.SkinTone = tone;
            _unsavedCharacter.Colors[ColorType.SkinColor] = color;

            InvokeCharacterChanged(CharacterChangeType.Appearance);
        }

        /// <summary>
        /// Set a color in the current character.
        /// </summary>
        [Client]
        public void SetColor(ColorType type, string option)
        {
            if (option == string.Empty) return;

            _unsavedCharacter.Colors[type] = option;

            InvokeCharacterChanged(CharacterChangeType.Appearance);
        }
        
        /// <summary>
        /// Set a body slider in the current character.
        /// </summary>
        [Client]
        public void SetBody(BodyType type, string option)
        {
            if (option == string.Empty) return;

            _unsavedCharacter.Body[type] = option;

            InvokeCharacterChanged(CharacterChangeType.Appearance);
        }

        /// <summary>
        /// Set the current characters name.
        /// </summary>
        [Client]
        public void SetName(CharacterNameType type, string name)
        {
            if (name == _unsavedCharacter.Names[type]) return;
            if (name == string.Empty && type == CharacterNameType.Normal) return;

            _unsavedCharacter.Names[type] = name;

            InvokeCharacterChanged(CharacterChangeType.Names);
        }

        /// <summary>
        /// Set the current characters name.
        /// </summary>
        [Client]
        public void SetFlavorText(string text)
        {
            _unsavedCharacter.FlavorText = text;

            InvokeCharacterChanged(CharacterChangeType.Background);
        }

        /// <summary>
        /// Set the current characters name.
        /// </summary>
        [Client]
        public void SetRolePreference(string role, RolePriority priority)
        {
            // overflow role selected
            if (role == string.Empty)
            {
                _unsavedCharacter.OverFlowRole = priority == RolePriority.High;
                InvokeCharacterChanged(CharacterChangeType.Roles);
                return;
            }

            // if the high priority role is being changed we clear the fav role
            if (_unsavedCharacter.FavoriteRole == role)
            {
                _unsavedCharacter.FavoriteRole = string.Empty;
            }

            switch (priority)
            {
                case RolePriority.Never:
                    _unsavedCharacter.Roles.Remove(role);
                    break;
                case RolePriority.Low:
                case RolePriority.Medium:

                    _unsavedCharacter.Roles[role] = priority;
                    break;

                case RolePriority.High:
                
                    string oldFav = _unsavedCharacter.FavoriteRole;
                    if (oldFav != string.Empty)
                    {
                        _unsavedCharacter.Roles[oldFav] = RolePriority.Medium;
                    }

                    _unsavedCharacter.FavoriteRole = role;
                    _unsavedCharacter.Roles[role] = priority;
                    break;
            }

            InvokeCharacterChanged(CharacterChangeType.Roles);
        }

        #endregion

        #region Networking

        /// <summary>
        /// When pressing the embark button
        /// </summary>
        [Client]
        public Player EmbarkCharacter()
        {
            PlayerSubSystem playerSystem = SubSystems.Get<PlayerSubSystem>();
            Player player = playerSystem.GetPlayer(LocalConnection);

            SendCharacter(player.Ckey);
            return player;
        }
        
        /// <summary>
        /// When the round is preparing and the player is ready send their character to server.
        /// </summary>
        [Client]
        private void SendCharacterIfReady(ref EventContext context, in RoundStateUpdated e)
        {
            if (e.RoundState != RoundState.Preparing) return;

            PlayerSubSystem playerSystem = SubSystems.Get<PlayerSubSystem>();
            Player player = playerSystem.GetPlayer(LocalConnection);
            
            ReadyPlayersSubSystem readyPlayersSystem = SubSystems.Get<ReadyPlayersSubSystem>();
            if (!readyPlayersSystem.ReadyPlayers.Contains(player)) return;

            SendCharacter(player.Ckey);
        }

        /// <summary>
        /// Send the character to CharacterSubSystem.
        /// Has to be public so it can be called when pressing the embark button
        /// </summary>
        [Client]
        private void SendCharacter(string ckey)
        {
            ClientSendCharacterMessage selectCharacterMessage = new(ckey, new CharacterProfile(SelectedCharacter));
            ClientManager.Broadcast(selectCharacterMessage);
        }

        #endregion

        #region Events

        [Client]
        private void InvokeCharacterChanged(CharacterChangeType type)
        {
            new LocalLobbyCharacterChanged(_selectedCharacterIndex, _unsavedCharacter, type).Invoke(this);
        }

        [Client]
        private void InvokeCharacterListChanged()
        {
            new LocalLobbyCharacterListChanged(_characters, CharacterNames.ToList()).Invoke(this);
        }
        
        #endregion
    }
}