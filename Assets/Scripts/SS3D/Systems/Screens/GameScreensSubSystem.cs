using Coimbra.Services.Events;
using FishNet.Connection;
using SS3D.Core;
using SS3D.Core.Behaviours;
using SS3D.Systems.Entities.Events;
using SS3D.Systems.Inputs;
using SS3D.Systems.PlayerControl;
using SS3D.Systems.PlayerControl.Events;
using SS3D.Systems.Rounds.Events;
using SS3D.Systems.Screens.Events;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using InputSubSystem = SS3D.Systems.Inputs.InputSubSystem;

namespace SS3D.Systems.Screens
{
    public class GameScreensSubSystem : NetworkSubSystem
    {
        [SerializeField] private bool _blockSwitchToNone;

        private PlayerSpawnedState _spawnedState;
        private Controls.OtherActions _controls;

        protected override void OnAwake()
        {
            base.OnAwake();

            _blockSwitchToNone = true;
            _spawnedState = PlayerSpawnedState.IsNotSpawned;

            AddHandle(ChangeGameScreen.AddListener(HandleChangeGameScreen));
            AddHandle(SpawnedPlayersUpdated.AddListener(HandleSpawnedPlayersUpdated));
            AddHandle(RoundStateUpdated.AddListener(HandleRoundStateUpdated));

            _controls = SubSystems.Get<InputSubSystem>().Inputs.Other;
            _controls.ToggleMenu.performed += HandleToggleMenu;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            SwitchScreen(ScreenType.Lobby);
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();

            _controls.ToggleMenu.performed -= HandleToggleMenu;
        }

        private void HandleToggleMenu(InputAction.CallbackContext context)
        {
            if (_blockSwitchToNone)
            {
                return;
            }

            ScreenType screenToSwitchTo = GameScreens.ActiveScreen == ScreenType.Lobby ? ScreenType.None : ScreenType.Lobby;
            SwitchScreen(screenToSwitchTo);
        }

        /// <summary>
        /// Called when another script requests to change the gamescreen.
        /// </summary>
        private void HandleChangeGameScreen(ref EventContext context, in ChangeGameScreen e)
        {
            ScreenType screenType = e.Screen;

            switch (screenType)
            {
                case ScreenType.None:
                    MarkNewlySpawnedPlayerAsAwaitingConfirmation();
                    break;
                case ScreenType.Lobby:
                    break;
                case ScreenType.CharacterCreation:
                    // dont want the player to be able to open char creation while ingame
                    if (_spawnedState == PlayerSpawnedState.ConfirmedSpawned) return;
                    break;
                default:
                    break;
            }
            
            SwitchScreen(screenType);
        }

        private void HandleSpawnedPlayersUpdated(ref EventContext context, in SpawnedPlayersUpdated e)
        {
            bool isPlayerSpawned = e.SpawnedPlayers.Find(controllable => controllable.Owner == LocalConnection);

            if (!isPlayerSpawned && _spawnedState == PlayerSpawnedState.ConfirmedSpawned)
            {
                LockToMenuScreen();
            }

            if (isPlayerSpawned)
            {
                GivePlayerAccessToGame();
            }

            UpdateScreenBasedOnSpawnState();
        }

        private void HandleRoundStateUpdated(ref EventContext context, in RoundStateUpdated e)
        {
            switch (e.RoundState)
            {
                case Rounds.RoundState.Ongoing:
                case Rounds.RoundState.Ending:
                    break;
                default:
                    LockToMenuScreen();
                    break;
            }
        }

        private void UpdateScreenBasedOnSpawnState()
        {
            switch (_spawnedState)
            {
                case PlayerSpawnedState.IsNotSpawned:
                case PlayerSpawnedState.AwaitingConfirmationOfSpawn:
                    SwitchScreen(ScreenType.Lobby);
                    break;
                case PlayerSpawnedState.ConfirmedSpawned:
                    SwitchScreen(ScreenType.None);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Prevents the player from leaving the menu screen.
        /// </summary>
        private void LockToMenuScreen()
        {
            _blockSwitchToNone = true;
            _spawnedState = PlayerSpawnedState.IsNotSpawned;

            UpdateScreenBasedOnSpawnState();
        }

        /// <summary>
        /// Switch the game screen.
        /// </summary>
        private void SwitchScreen(ScreenType type)
        {
            GameScreens.SwitchTo(type);
            new GameScreenChanged(GameScreens.ActiveScreen, GameScreens.LastScreen).Invoke(this);
        }

        /// <summary>
        /// Gives the player the ability to toggle in and out of the
        /// menu, and records that they have been added to the Spawned
        /// Players list.
        /// </summary>
        private void GivePlayerAccessToGame()
        {
            _blockSwitchToNone = false;
            _spawnedState = PlayerSpawnedState.ConfirmedSpawned;

            UpdateScreenBasedOnSpawnState();
        }

        /// <summary>
        /// Identifies that the entity may have spawned recently, and
        /// may not yet been reflected in the Spawned Players list.
        /// </summary>
        private void MarkNewlySpawnedPlayerAsAwaitingConfirmation()
        {
            if (_spawnedState == PlayerSpawnedState.IsNotSpawned)
            {
                _spawnedState = PlayerSpawnedState.AwaitingConfirmationOfSpawn;
            }

            UpdateScreenBasedOnSpawnState();
        }

        /// <summary>
        /// Internal enum to describe player spawn state.
        /// </summary>
        private enum PlayerSpawnedState
        {
            IsNotSpawned,
            AwaitingConfirmationOfSpawn,
            ConfirmedSpawned
        }
    }
}
