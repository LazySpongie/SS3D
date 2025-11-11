using SS3D.Attributes;
using SS3D.Systems.Characters.UI;
using SS3D.Systems.Screens.Events;
using UnityEngine;
using UnityEngine.UI;

namespace SS3D.Systems.Screens
{
    public class CharacterCreationScreen : GameScreen
    {

        [Header("Buttons")]
        [SerializeField] [NotNull] private Button _lobbyButton;
        
        protected override void OnAwake()
        {
            base.OnAwake();
            _lobbyButton.onClick.AddListener(HandleLobbyButton);
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();
            _lobbyButton.onClick.RemoveListener(HandleLobbyButton);
        }

        /// <summary>
        /// Method called when the player clicks to lobby.
        /// </summary>
        private void HandleLobbyButton()
        {
            new ChangeGameScreen(ScreenType.Lobby).Invoke(this);
        }

    }
}