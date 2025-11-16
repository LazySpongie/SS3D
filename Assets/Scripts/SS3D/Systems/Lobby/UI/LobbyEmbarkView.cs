using Coimbra.Services.Events;
using SS3D.Core;
using SS3D.Core.Behaviours;
using SS3D.Systems.Characters.Preferences;
using SS3D.Systems.Entities;
using SS3D.Systems.Entities.Events;
using SS3D.Systems.PlayerControl;
using SS3D.Systems.Rounds;
using SS3D.Systems.Rounds.Messages;
using SS3D.UI.Buttons;
using UnityEngine;
using RoundStateUpdated = SS3D.Systems.Rounds.Events.RoundStateUpdated;

namespace SS3D.Systems.Lobby.UI
{
    public sealed class LobbyEmbarkView : NetworkActor
    {        
        [SerializeField] private LabelButton _embarkButton;

        protected override void OnAwake()           
        {
            base.OnAwake();

            _embarkButton.OnPressedDown += HandleEmbarkButtonPressed;

            // AddHandle(RoundStateUpdated.AddListener(HandleRoundStateUpdated));
            // AddHandle(SpawnedPlayersUpdated.AddListener(HandleSpawnedPlayersUpdated));
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();

            _embarkButton.OnPressedDown -= HandleEmbarkButtonPressed;
        }

        /// <summary>
        /// When the player presses the embark button send their character profile to the server and then spawn their character.
        /// </summary>
        private void HandleEmbarkButtonPressed(bool pressed)
        {
            ClientPreferencesSubSystem preferencesSystem = SubSystems.Get<ClientPreferencesSubSystem>();
            RoundSetupSubSystem roundSetupSubSystem = SubSystems.Get<RoundSetupSubSystem>();

            Player player = preferencesSystem.EmbarkCharacter();
            roundSetupSubSystem.CmdSpawnLatePlayer(player);
        }

    }
}
