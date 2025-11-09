using System.Collections.Generic;
using UnityEngine;
using SS3D.Core;
using SS3D.Core.Behaviours;
using FishNet.Object;
using SS3D.Data.Management;

namespace SS3D.Systems.Characters.Preferences
{
    /// <summary>
    /// </summary>
    public class ClientPreferencesSubSystem : SubSystem
    {
        public delegate void CharacterChangedHandler(CharacterChangeType type);

        public event CharacterChangedHandler OnCharacterChanged;

	    public const string SavePath = "/Characters";

        private Dictionary<int, CharacterProfile> _characters = new();

        private int _selectedCharacterIndex = 0;

        private CharacterProfile _unsavedCharacter;

        public int SelectedCharacterIndex => _selectedCharacterIndex;

        public CharacterProfile SelectedCharacter
        {
            get { return _characters[_selectedCharacterIndex]; }
        }

        public CharacterProfile UnsavedCharacter => _unsavedCharacter;

        protected override void OnAwake()
        {
            base.OnAwake();

            LoadCharactersFromDisk();
        }

        protected override void OnStart()
        {
            base.OnStart();

            SelectCharacter(0);
            // OnCharacterChanged?.Invoke();
        }

        // public override void OnStartClient()
        // {
        //     base.OnStartClient();

        //     // OnCharacterChanged?.Invoke();

        //     AddHandle(SpawnedPlayersUpdated.AddListener(HandleSpawnedPlayersUpdated));
        // }

        #region Load and Save

        /// <summary>
        /// Method called when the load character button is clicked.
        /// </summary>
        [Client]
        public void LoadCharactersFromDisk()
        {
            Debug.Log("LoadCharactersFromDisk");

            _characters.Clear();
            List<string> savedChars = LocalStorage.GetAllObjectsNameInFolder(SavePath);

            // Find valid saved characters
            int i = 0;
            foreach (string name in savedChars)
            {
                // need to remove the .json from filename
                string newName = name.Remove(name.Length - 5);

                string expectedName = "Character" + i;

                if (newName != expectedName) continue;

                string filePath = SavePath + "/" + newName;
                CharacterProfile loadedcharacter =
                    LocalStorage.LoadObject<CharacterProfile>(filePath);

                // Validate 
                if (loadedcharacter != null)
                {
                    _characters.Add(i, loadedcharacter);
                    // need to validate character here
                }

                i++;
            }

            // No valid characters so create a default character
            if (_characters.Count == 0)
            {
                CharacterProfile newChar = new CharacterProfile();
                _characters.Add(0, newChar);
                LocalStorage.SaveObject(SavePath + "/Character0", newChar, true);
            }
        }

        /// <summary>
        /// Method called when a character is selected in the menu.
        /// </summary>
        [Client]
        public void SelectCharacter(int index)
        {
            _selectedCharacterIndex = index;
            _unsavedCharacter = new CharacterProfile(_characters[_selectedCharacterIndex]);

            //send to server
            OnCharacterChanged?.Invoke(CharacterChangeType.Everything);
        }

        /// <summary>
        /// Method called when the save character button is clicked.
        /// </summary>
        [Client]
        public void SaveCharacter()
        {
            _characters[_selectedCharacterIndex] = _unsavedCharacter;
            bool overwrite = true;
            LocalStorage.SaveObject(SavePath + "/Character" + _selectedCharacterIndex, _unsavedCharacter, overwrite);
        }
        #endregion

        // #region Spawn Player

        // /// <summary>
        // /// Callback when a player entity is spawned to set their appearance
        // /// </summary>
        // [Client]
        // private void HandleSpawnedPlayersUpdated(ref EventContext context, in SpawnedPlayersUpdated e)
        // {
        //     AddCustomizationToPlayer();
        // }

        // /// <summary>
        // /// When the player is spawned get their entity and send their appearance to the server to be applied
        // /// </summary>
        // [Client]
        // private void AddCustomizationToPlayer()
        // {
        //     EntitySubSystem system = SubSystems.Get<EntitySubSystem>();

        //     if (!system.TryGetSpawnedEntity(LocalConnection, out Entity entity)) return;

        //     Dictionary<AppearanceType, string> dict = new();
        //     foreach (KeyValuePair<AppearanceType, string> entry in SelectedCharacter.Appearance)
        //     {
        //         dict[entry.Key] = entry.Value;
        //     }

        //     // ServerRPC
        //     entity.GetComponent<UniqueIdentifiers>()?.SetAppearance(SelectedCharacter);
        // }
        // #endregion
        
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
        
    }
}