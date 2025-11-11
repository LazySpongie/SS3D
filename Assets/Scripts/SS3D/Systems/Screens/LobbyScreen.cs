using SS3D.Attributes;
using SS3D.Systems.Screens.Events;
using UnityEngine;
using UnityEngine.UI;

namespace SS3D.Systems.Screens
{
    public class LobbyScreen : GameScreen
    {
        [Header("Buttons")]
        [SerializeField] [NotNull] private Button _characterCreationButton;
        
        protected override void OnAwake()
        {
            base.OnAwake();
            _characterCreationButton.onClick.AddListener(HandleOpenMenuButton);
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();
            _characterCreationButton.onClick.RemoveListener(HandleOpenMenuButton);
        }

        /// <summary>
        /// Method called when the player clicks to character creation.
        /// </summary>
        private void HandleOpenMenuButton()
        {
            new ChangeGameScreen(ScreenType.CharacterCreation).Invoke(this);
        }
    }
}