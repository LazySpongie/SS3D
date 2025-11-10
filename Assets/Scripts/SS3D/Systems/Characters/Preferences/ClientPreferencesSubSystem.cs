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
            LoadCharactersFromDisk();
            AddHandle(RoundStateUpdated.AddListener(HandleRoundStateUpdated));
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

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

            if (characterManifest != null || characterManifest.Characters != null)
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

            OnCharactersLoaded?.Invoke();
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
                Log.Warning(this, "Duplicate character name: " + _unsavedCharacter.Name + " - cannot save character.");
                return;
            }

            // if the character is being renamed we need to rename the save file
            if (SelectedCharacter.Name != _unsavedCharacter.Name)
            {
                string oldfile = CharacterSavePath + "/" + SelectedCharacter.Name;
                string newfile = CharacterSavePath + "/" + _unsavedCharacter.Name;
                LocalStorage.RenameFile(oldfile, newfile);
            }

            _characters[_selectedCharacterIndex] = _unsavedCharacter;

            SaveCharacterToDisk(SelectedCharacter);

            SelectCharacter(_selectedCharacterIndex);
        }

        /// <summary>
        /// Reset the unsaved character to the last save.
        /// </summary>
        [Client]
        public void ResetCharacter()
        {
            _unsavedCharacter = new CharacterProfile(_characters[_selectedCharacterIndex]);

            OnCharacterChanged?.Invoke(CharacterChangeType.Load);
        }

        /// <summary>
        /// Create a new character.
        /// </summary>
        [Client]
        public void CreateCharacter(bool select = true)
        {
            CharacterProfile newChar = new CharacterProfile();

            // need to avoid overwriting other characters
            newChar.Name = GetNewCharacterFileName(newChar.Name);

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
            OnCharactersLoaded?.Invoke();

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
        /// Set an appearance option in the current character.
        /// </summary>
        [Client]
        public void SetAppearanceOption(AppearanceType type, string option, bool invoke = true)
        {
            if (option == string.Empty) return;

            _unsavedCharacter.Appearance[type] = option;
            if (!invoke) return;
            OnCharacterChanged?.Invoke(CharacterChangeType.Appearance);
        }

        /// <summary>
        /// Set the current characters name.
        /// </summary>
        public void SetCharacterName(string name)
        {
            if (name == string.Empty || name == _unsavedCharacter.Name) return;

            _unsavedCharacter.Name = name;
            OnCharacterChanged?.Invoke(CharacterChangeType.Name);
        }

        /// <summary>
        /// Set the current characters name.
        /// </summary>
        public void SetColor(AppearanceType type, string color)
        {
            if (color == string.Empty) return;

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