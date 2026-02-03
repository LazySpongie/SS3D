using Coimbra;
using Coimbra.Services.Events;
using FishNet.Connection;
using FishNet.Object;
using SS3D.Core;
using SS3D.Core.Behaviours;
using SS3D.Engine.Chat;
using SS3D.Logging;
using SS3D.Systems.Characters;
using SS3D.Systems.Characters.Preferences;
using SS3D.Systems.Entities;
using SS3D.Systems.Entities.Events;
using SS3D.Systems.Roles;
using System.Collections.Generic;
using System.Security;
using RoundStateUpdated = SS3D.Systems.Rounds.Events.RoundStateUpdated;

namespace SS3D.Systems.Rounds
{
    /// <summary>
    /// Handles assigning roles, antagonists, and creating characters
    /// Sets up player entities with the correct appearance and loadout when spawned
    /// 
    /// TODO: this script should also handle latejoining players
    /// 
    /// </summary>
    public class RoundSetupSubSystem : NetworkSubSystem
    {
        private CharacterSubSystem _characterSubSystem;
        private RoleSubSystem _roleSubSystem;
        private EntitySubSystem _entitySubSystem;
        
        public override void OnStartServer()
        {
            base.OnStartServer();

            AddHandle(RoundStateUpdated.AddListener(HandleRoundStateUpdated));

            _characterSubSystem = SubSystems.Get<CharacterSubSystem>();
            _roleSubSystem = SubSystems.Get<RoleSubSystem>();
            _entitySubSystem = SubSystems.Get<EntitySubSystem>();
            
        }

        /// <summary>
        /// Assign roles, antags, and create characters for the players that will spawn at roundstart
        /// </summary>
        [Server]
        private void HandleRoundStateUpdated(ref EventContext context, in RoundStateUpdated e)
        {
            switch (e.RoundState)
            {
                case RoundState.WarmingUp:

                    // assign major antagonists here
                    _roleSubSystem.AssignRolesToInitialPlayers();
                    CreateInitialCharacters();
                    // assign minor antags here
                    
                    SubSystems.Get<ReadyPlayersSubSystem>().ClearReadyPlayers();
                    break;
                case RoundState.Ongoing:

                    SpawnInitialPlayers();
            
                    new InitialPlayersSpawned(_entitySubSystem.SpawnedPlayers).Invoke(this);

                    break;
            }
        }
        
        /// <summary>
        /// Create characters from the list of players with assigned roles and add them to the crew manifest
        /// </summary>
        [Server]
        private void CreateInitialCharacters()
        {
            if (_roleSubSystem.RolePlayers.Count == 0) return;

            foreach (KeyValuePair<Player, RoleData> pair in _roleSubSystem.RolePlayers)
            {
                InGameCharacter character = _characterSubSystem.CreateInitialCharacter(pair.Key, pair.Value.NameType);
                if (pair.Value.IsStationCrew)
                {
                    _roleSubSystem.AddCharacterToCrewManifest(character, pair.Value.name);
                }
            }
            _roleSubSystem.ClearRolePlayers();
        }

        /// <summary>
        /// Spawn the starting players from their characters
        /// </summary>
        [Server]
        private void SpawnInitialPlayers()
        {
            if (_characterSubSystem.IngameCharacters.Count == 0) return;

            foreach (KeyValuePair<int, InGameCharacter> pair in _characterSubSystem.IngameCharacters)
            {
                SpawnPlayer(pair.Value);
            }

            _characterSubSystem.ClearInitialCharacterProfiles();
        }

        /// <summary>
        /// Asks the server to spawn a player.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void CmdSpawnLatePlayer(Player player, string role)
        {
            SpawnLatePlayer(player, role);
        }
        
        /// <summary>
        /// Spawns a player after the round has started
        /// </summary>
        [Server]
        private void SpawnLatePlayer(Player player, string role)
        {
            InGameCharacter character = _characterSubSystem.CreateInitialCharacter(player, CharacterNameType.Normal);

            if (character == null)
            {
                Log.Warning(this, player.Ckey + " does not have an initial character profile. Using default character.");

                character = _characterSubSystem.CreateCharacter(player, new CharacterProfile());
            }

            if (role == string.Empty) role = _roleSubSystem.GetOverflowRole().name;
            
            _roleSubSystem.AddCharacterToCrewManifest(character, role);

            ChatSubSystem chatSystem = SubSystems.Get<ChatSubSystem>();
            ChatChannels chatChannels = ScriptableSettings.GetOrFind<ChatChannels>();
            chatSystem.SendServerMessage(chatChannels.stationAlertsChannel, $"{character.Name}, assistant, has joined the ship");

            Log.Information(this, player.Ckey + " has joined the ship as " + character.Name);

            SpawnPlayer(character);
        }

        /// <summary>
        /// Spawn a player and set them up 
        /// </summary>
        [Server]
        private Entity SpawnPlayer(InGameCharacter character)
        {
            CharacterProfile profile = _characterSubSystem.InitialCharacterProfiles[character.Player];
            _characterSubSystem.InitialCharacterProfiles.Remove(character.Player);

            Entity entity = _entitySubSystem.SpawnPlayer(character.Player, character);
            SetupSpawnedPlayer(entity, character, profile);
            return entity;
        }

        /// <summary>
        /// Sets up a spawned player entity
        /// </summary>
        [Server]
        private void SetupSpawnedPlayer(Entity entity, InGameCharacter character, CharacterProfile profile)
        {
            _characterSubSystem.SetPlayerCharacter(entity, character, profile);

            // TODO: needs to account for if the player is not a member of the crew

            RoleData role = _roleSubSystem.GetCharacterRole(character);

            if (role == null)
            {
                role = _roleSubSystem.GetOverflowRole();
            }

            // add starting role loadout
            _roleSubSystem.GiveRoleLoadoutToPlayer(entity, role);

            // add the players loadout
            if (profile.Loadout.Count != 0)
            {
                
            }
        }
    }
}
