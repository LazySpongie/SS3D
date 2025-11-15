using System.Collections.Generic;
using SS3D.Core;
using SS3D.Core.Behaviours;
using FishNet.Object;
using SS3D.Systems.Entities;
using SS3D.Systems.Characters.Preferences;
using SS3D.Systems.Characters.Messages;
using FishNet.Connection;
using SS3D.Systems.PlayerControl;
using SS3D.Logging;
using SS3D.Systems.Roles;
using Codice.CM.Common;

namespace SS3D.Systems.Characters
{
    /// <summary>
    /// Controls every players selected character in the lobby
    /// </summary>
    public class CharacterSubSystem : NetworkSubSystem
    {
        private Dictionary<Player, CharacterProfile> _initialCharacters = new();

        private List<InGameCharacter> _ingameCharacters = new();

        public Dictionary<Player, CharacterProfile> InitialCharacters => _initialCharacters;
        // public ReadOnlyDictionary<Player, CharacterProfile> Characters => new ReadOnlyDictionary<Player, CharacterProfile>(_characters);

        public override void OnStartServer()
        {
            base.OnStartServer();

            ServerManager.RegisterBroadcast<ClientSendCharacterMessage>(HandleClientSentCharacter);
        }

        /// <summary>
        /// Clear the list of initial characters
        /// </summary>
        [Server]
        public void ClearInitialCharacters()
        {
            _initialCharacters.Clear();
        }

        /// <summary>
        /// Create characters from the list of players with assigned jobs and add them to the crew manifest
        /// </summary>
        [Server]
        public void CreateCharactersFromCrew(RoleSubSystem roleSubSystem)
        {
            foreach (KeyValuePair<Player, RoleData> pair in roleSubSystem.RolePlayers)
            {
                InGameCharacter character = CreateInitialCharacter(pair.Key, pair.Value.NameType);
                roleSubSystem.AddCharacterToCrewManifest(character, pair.Value);
            }
        }

        /// <summary>
        /// Create a character from the list of initial characters
        /// </summary>
        [Server]
        public InGameCharacter CreateInitialCharacter(Player player, CharacterNameType nameType = CharacterNameType.Normal)
        {
            CharacterProfile profile = _initialCharacters[player];
            _initialCharacters.Remove(player);

            return CreateCharacter(player, profile, nameType);
        }

        /// <summary>
        /// Create an InGameCharacter and add it to the list of characters in the round
        /// </summary>
        [Server]
        public InGameCharacter CreateCharacter(Player player, CharacterProfile profile, CharacterNameType nameType = CharacterNameType.Normal)
        {
            // When spawning new characters mid-round this will probably cause issues so it needs to be adjusted
            if (profile == null)
            {
                profile = new CharacterProfile();
            }

            string name = profile.Names[nameType];
            
            if (name == string.Empty)
            {
                // need to set a random name here 
                name = "missing name";
            }

            InGameCharacter character = new(player, name, profile);
            _ingameCharacters.Add(character);
            return character;
        }

        /// <summary>
        /// When a player is spawned set their name and appearance from their InGameCharacter
        /// </summary>
        [Server]
        public InGameCharacter SetPlayerCharacter(Entity entity)
        {
            Player player = entity.Mind.player;

            InGameCharacter character = _ingameCharacters.Find(ig => ig.Player == player && ig.Entity == null);

            if (character == null)
            {
                Log.Error(this, "Player " + player.Ckey + " does not have a corresponding ingame character");

                character = CreateCharacter(player, new CharacterProfile(), CharacterNameType.Normal);
            }

            // save this entity to the character
            character.Entity = entity;
            string name = character.Name;
            CharacterProfile profile = character.Profile;

            UniqueIdentifiers uid = entity.GetComponent<UniqueIdentifiers>();
            if (uid == null) return character;

            uid.SetCharacterProfile(profile);
            uid.SetName(name);

            Log.Information(this, "Added character " + uid.Name + " to player " + entity.Ckey);

            return character;
        }

        /// <summary>
        /// Save the character profile sent by each client to initialCharacters.
        /// Message sent by ClientPreferencesSubSystem.
        /// </summary>
        [Server]
        private void HandleClientSentCharacter(NetworkConnection connection, ClientSendCharacterMessage message)
        {
            Log.Information(this, message.Ckey + " assigned character: " + message.Character.Name);
            
            PlayerSubSystem _playerSystem = SubSystems.Get<PlayerSubSystem>();
            Player player = _playerSystem.GetPlayer(message.Ckey);
            if (!_initialCharacters.TryAdd(player, message.Character))
            {
                _initialCharacters[player] = message.Character;
            }
        }

    }
}