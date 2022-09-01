using Entropy.AIO.Bases;
using Entropy.AIO.General;

namespace Entropy.AIO
{
    static class Bootstrap
    {
        public static void Initialize()
        {
            MenuBase.Initialize();
            ChampionLoader.Initialize();
        }
    }
}