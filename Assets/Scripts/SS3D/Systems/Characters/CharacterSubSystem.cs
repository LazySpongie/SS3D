using System.Collections.Generic;
using UnityEngine;
using SS3D.Core;
using SS3D.Core.Behaviours;
using FishNet.Object;
using SS3D.Systems.Entities;
using SS3D.Systems.Entities.Events;
using Coimbra.Services.Events;
using SS3D.Systems.Characters.Preferences;
using System;
using SS3D.Systems.Characters.Messages;
using FishNet.Connection;
using SS3D.Systems.PlayerControl.Events;
using SS3D.Systems.PlayerControl;
using SS3D.Logging;
using SS3D.Systems.Rounds;
using System.Collections.ObjectModel;

namespace SS3D.Systems.Characters
{
    /// <summary>
    /// Controls every players selected character in the lobby
    /// </summary>
    public class CharacterSubSystem : NetworkSubSystem
    {
        private Dictionary<Player, CharacterProfile> _characters = new();

        public Dictionary<Player, CharacterProfile> Characters => _characters;
        // public ReadOnlyDictionary<Player, CharacterProfile> Characters => new ReadOnlyDictionary<Player, CharacterProfile>(_characters);

        public override void OnStartServer()
        {
            base.OnStartServer();

            ServerManager.RegisterBroadcast<ClientSendCharacterMessage>(HandleClientSentCharacter);

            AddHandle(OnlinePlayersChanged.AddListener(HandleUserLeftServer));
        }

        /// <summary>
        /// When a player is spawned set their character profile
        /// </summary>
        [Server]
        public void SetPlayerCharacter(Entity entity)
        {
            _characters.TryGetValue(entity.Mind.player, out CharacterProfile character);
            
            // returns a blank character profile if character is null
            character = new CharacterProfile(character);

            entity.GetComponent<UniqueIdentifiers>()?.SetFromCharacterProfile(character);

            Log.Information(this, "Added character profile " + character.Name + " to player " + entity.Ckey);
        }

        /// <summary>
        /// When a user leaves the server delete their character from the list.
        /// 
        /// This will have to be changed at some point because it could potentially cause issues if leaving mid round
        /// </summary>
        [Server]
        private void HandleUserLeftServer(ref EventContext context, in OnlinePlayersChanged e)
        {
            if (e.ChangeType != ChangeType.Removal) return;
            _characters.Remove(e.ChangedPlayer);
        }

        /// <summary>
        /// Save the character profile sent by each client to a dictionary.
        /// Message sent by ClientPreferencesSubSystem.
        /// </summary>
        [Server]
        private void HandleClientSentCharacter(NetworkConnection connection, ClientSendCharacterMessage message)
        {
            Log.Information(this, message.Ckey + " assigned character: " + message.Character.Name);
            
            PlayerSubSystem _playerSystem = SubSystems.Get<PlayerSubSystem>();
            Player player = _playerSystem.GetPlayer(message.Ckey);
            if (!_characters.TryAdd(player, message.Character))
            {
                _characters[player] = message.Character;
            }
        }

    }
}