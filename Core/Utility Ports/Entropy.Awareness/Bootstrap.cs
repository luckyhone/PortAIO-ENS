using Entropy.Awareness.Bases;
using Entropy.Awareness.Managers;

namespace Entropy.Awareness
{
    public static class Bootstrap
    {
        public static void Initialize()
        {
            MenuBase.Initialize();

            new Menus();

            TrackersCommon.Initialize();

            InformationManager.Initialize();

            RenderingManager.Initialize();
        }
    }
}