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

namespace SS3D.Systems.Characters
{
    /// <summary>
    /// Controls every players selected character in the lobby
    /// </summary>
    public class CharacterSubSystem : NetworkSubSystem
    {
        private Dictionary<string, CharacterProfile> _characters = new();

        public override void OnStartServer()
        {
            base.OnStartServer();

            ServerManager.RegisterBroadcast<PlayerSelectCharacterMessage>(HandlePlayerSelectCharacter);

            AddHandle(OnlinePlayersChanged.AddListener(HandleUserLeftServer));
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
            _characters.Remove(e.ChangedCkey);
        }

        /// <summary>
        /// When a user selects a character save it to the list.
        /// Message sent by ClientPreferencesSubSystem.
        /// </summary>
        [Server]
        private void HandlePlayerSelectCharacter(NetworkConnection connection, PlayerSelectCharacterMessage message)
        {
            Log.Information(this, message.Ckey + "selected character: " + message.Character.Name);
            if (!_characters.TryAdd(message.Ckey, message.Character))
            {
                _characters[message.Ckey] = message.Character;
            }
        }
        
        /// <summary>
        /// When a player is spawned send their character profile to their UniqueIdentifiers
        /// </summary>
        [Server]
        public void SetPlayerCharacter(Entity entity)
        {
            _characters.TryGetValue(entity.Ckey, out CharacterProfile character);
            character = new CharacterProfile(character);

            if (character == null) character = new CharacterProfile();

            entity.GetComponent<UniqueIdentifiers>()?.SetFromCharacterProfile(character);

            Log.Information(this, "Added character profile " + _characters[entity.Ckey].Name + " to player " + entity.Ckey);
        }
    }
}