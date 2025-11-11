using Coimbra.Services.Events;

namespace SS3D.Systems.Screens.Events
{
    public partial struct GameScreenChanged : IEvent
    {
        public readonly ScreenType ActiveScreen;
        public readonly ScreenType LastScreen;

        public GameScreenChanged(ScreenType activeScreen, ScreenType lastScreen)
        {
            ActiveScreen = activeScreen;
            LastScreen = lastScreen;
        }
    }
}