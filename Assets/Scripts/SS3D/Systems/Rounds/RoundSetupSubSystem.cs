using Coimbra.Services.Events;
using FishNet.Object;
using SS3D.Core;
using SS3D.Core.Behaviours;
using SS3D.Systems.Characters;
using SS3D.Systems.Entities;
using SS3D.Systems.Roles;
using RoundStateUpdated = SS3D.Systems.Rounds.Events.RoundStateUpdated;

namespace SS3D.Systems.Rounds
{
    /// <summary>
    /// Handles assigning roles, antagonists, and creating characters
    /// 
    /// TODO: this script should also handle latejoining players
    /// 
    /// </summary>
    public class RoundSetupSubSystem : NetworkSubSystem
    {
        private CharacterSubSystem _characterSubSystem;
        private RoleSubSystem _roleSubSystem;
        
        public override void OnStartServer()
        {
            base.OnStartServer();

            AddHandle(RoundStateUpdated.AddListener(HandleRoundStateUpdated));

            _characterSubSystem = SubSystems.Get<CharacterSubSystem>();
            _roleSubSystem = SubSystems.Get<RoleSubSystem>();
        }

        /// <summary>
        /// Assign roles, antags, and create characters for the players that will spawn at roundstart
        /// </summary>
        [Server]
        private void HandleRoundStateUpdated(ref EventContext context, in RoundStateUpdated e)
        {
            if (e.RoundState != RoundState.WarmingUp) return;

            // assign major antagonists here

            _roleSubSystem.AssignRolesToReadyPlayers();
            _characterSubSystem.CreateCharactersFromCrew(_roleSubSystem);
            _characterSubSystem.ClearInitialCharacters();
            _roleSubSystem.ClearRolePlayers();
            // assign minor antags here
        }
        
        /// <summary>
        /// Sets the character and role loadout of an entity
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SetupSpawnedPlayer(Entity entity)
        {
            InGameCharacter character = _characterSubSystem.SetPlayerCharacter(entity);

            // TODO: needs to account for if the player is not a member of the crew

            RoleData role = _roleSubSystem.GetCharacterRole(character);

            if (role == null)
            {
                role = _roleSubSystem.GetOverflowRole();
            }

            // add starting role loadout
            _roleSubSystem.GiveRoleLoadoutToPlayer(entity, role);

            // add the players loadout
            if (character.Profile.Loadout.Count != 0)
            {
                
            }
        }

    }
}
