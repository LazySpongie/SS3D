using Coimbra.Services.Events;

namespace SS3D.Systems.Screens.Events
{
    public partial struct ChangeGameScreen : IEvent
    {
        public readonly ScreenType Screen;

        public ChangeGameScreen(ScreenType screen)
        {
            Screen = screen;
        }
    }
}