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
using System.Collections.ObjectModel;

namespace SS3D.Systems.Characters
{
    /// <summary>
    /// Controls every players selected character in the lobby
    /// </summary>
    public class CharacterSubSystem : NetworkSubSystem
    {
        private Dictionary<Player, CharacterProfile> _initialCharacterProfiles = new();

        private List<InGameCharacter> _ingameCharacters = new();

        public ReadOnlyCollection<InGameCharacter> IngameCharacters => _ingameCharacters.AsReadOnly();

        public Dictionary<Player, CharacterProfile> InitialCharacterProfiles => _initialCharacterProfiles;

        public override void OnStartServer()
        {
            base.OnStartServer();

            ServerManager.RegisterBroadcast<ClientSendCharacterMessage>(HandleClientSentCharacter);
        }

        /// <summary>
        /// Clear the list of initial characters
        /// </summary>
        [Server]
        public void ClearInitialCharacterProfiles()
        {
            _initialCharacterProfiles.Clear();
        }

        /// <summary>
        /// Create a character from the list of initial characters
        /// </summary>
        [Server]
        public InGameCharacter CreateInitialCharacter(Player player, CharacterNameType nameType = CharacterNameType.Normal)
        {
            CharacterProfile profile = _initialCharacterProfiles[player];
            _initialCharacterProfiles.Remove(player);

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
        public void SetPlayerCharacter(Entity entity, InGameCharacter character)
        {
            Player player = entity.Mind.player;

            if (character == null)
            {
                Log.Error(this, "Player " + player.Ckey + " does not have a corresponding ingame character");

                character = CreateCharacter(player, new CharacterProfile(), CharacterNameType.Normal);
            }

            entity.Character = character;
            character.Entity = entity;

            UniqueIdentifiers uid = entity.GetComponent<UniqueIdentifiers>();
            if (uid == null) return;

            uid.SetCharacterProfile(character.Profile);
            uid.SetName(character.Name);

            Log.Information(this, "Added character " + uid.Name + " to player " + entity.Ckey);

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
            if (!_initialCharacterProfiles.TryAdd(player, message.Character))
            {
                _initialCharacterProfiles[player] = message.Character;
            }
        }

    }
}