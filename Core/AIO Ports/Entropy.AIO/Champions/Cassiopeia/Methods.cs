using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Cassiopeia
{
    using Misc;
    class Methods
    {

        public Methods()
        {
            Initialize();
        }

        private static void Initialize()
        {
            Game.OnUpdate                    += Cassiopeia.OnTick;
            AntiGapcloser.OnGapcloser         += Cassiopeia.OnNewGapcloser;
            Dash.OnDash                   += Cassiopeia.OnDash;
            AIBaseClient.OnBuffAdd           += Definitions.BuffManagerOnOnGainBuff;
            AIBaseClient.OnBuffRemove           += Definitions.BuffManagerOnOnLoseBuff;
            Orbwalker.OnBeforeAttack            += Cassiopeia.OnPreAttack;
            Interrupter.OnInterrupterSpell += Cassiopeia.OnInterruptableSpell;
        }
    }
}