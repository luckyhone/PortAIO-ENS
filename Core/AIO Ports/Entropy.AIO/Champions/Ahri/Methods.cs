using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Ahri
{
    class Methods
    {
        public Methods()
        {
            Initialize();
        }

        private static void Initialize()
        {
            Game.OnUpdate               += Ahri.OnTick;
            GameEvent.OnGameTick        += Ahri.OnCustomTick;
            AntiGapcloser.OnGapcloser   += Ahri.OnNewGapcloser;
        }
    }
}