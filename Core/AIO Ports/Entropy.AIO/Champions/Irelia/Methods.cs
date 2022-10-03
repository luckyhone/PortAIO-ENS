using System;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Irelia
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
            GameEvent.OnGameTick                      += Irelia.OnTick;
            Interrupter.OnInterrupterSpell += Irelia.OnInterruptableSpell;
            AIBaseClient.OnDoCast  += Irelia.OnProcessSpellCast;
            GameObject.OnDelete              += Irelia.OnDelete;
            AIBaseClient.OnBuffRemove           += Irelia.BuffManagerOnOnLoseBuff;
            Game.OnWndProc                   += Irelia.OnWndProc;
            Dash.OnDash                   += Irelia.OnDash;
            GameEvent.OnGameEnd                       += OnEnd;
            Drawing.OnDraw += Drawings.OnRender;
        }

        private static void OnEnd()
        {
            Game.OnWndProc -= Irelia.OnWndProc;
        }
    }
}