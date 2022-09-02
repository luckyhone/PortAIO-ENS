using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Camille
{
    #region


    #endregion

    class Methods
    {
        public Methods()
        {
            Initialize();
        }

        private static void Initialize()
        {
            GameEvent.OnGameTick                     += Camille.OnTick;
            GameEvent.OnGameTick      += Camille.OnCustomTick;
            Orbwalker.OnBeforeMove             += Camille.OnPreMove;
            Orbwalker.OnAfterAttack          += Camille.OnPostAttack;
            AIBaseClient.OnDoCast += Camille.OnProcessSpellCast;
            AIBaseClient.OnBuffAdd          += Camille.OnGainBuff;
            Drawing.OnDraw += Drawings.OnRender;
        }
    }
}