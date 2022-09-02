using System;
using EnsoulSharp;

namespace HERMES_Kalista.MyInitializer
{
    public static partial class HERMESLoader
    {
        public static void LoadLogic()
        {
            MyLogic.Spells.OnLoad(new EventArgs());
            Game.OnUpdate += MyLogic.Others.SoulboundSaver.OnUpdate;
            AIBaseClient.OnDoCast += MyLogic.Others.SoulboundSaver.OnProcessSpellCast;

            #region Others
            
            AIBaseClient.OnDoCast += MyLogic.Others.Events.OnProcessSpellcast;
            Drawing.OnDraw += MyLogic.Others.Events.OnDraw;
            Game.OnUpdate += MyLogic.Others.SkinHack.OnUpdate;

            #endregion
        }
    }
}