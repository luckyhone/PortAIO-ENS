using System;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.LeBlanc
{
    class Methods
    {
        public Methods()
        {
            Initialize();
        }

        private static void Initialize()
        {
            GameEvent.OnGameTick    += LeBlanc.OnTick;
            GameEvent.OnGameWndProc += LeBlanc.OnWndProc;
            //GameEvent.OnGameEnd     += OnEnd;
            Drawing.OnDraw += Drawings.OnRender;
        }
        
    }
}