using EnsoulSharp.SDK;

namespace Entropy.AIO.Neeko
{
    class Methods
    {

        public Methods()
        {
            Initialize();
        }

        private static void Initialize()
        {
            Dash.OnDash                 += Neeko.OnDash;
            GameEvent.OnGameTick        += Neeko.OnTick;
            GameEvent.OnGameTick        += Neeko.OnCustomTick;
            Orbwalker.OnBeforeAttack    += Neeko.OnPreAttack;
            AntiGapcloser.OnGapcloser   += Neeko.GapCloser;
        }
    }
}