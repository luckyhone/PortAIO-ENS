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
            AIBaseClient.OnProcessSpellCast += MyLogic.Others.SoulboundSaver.OnProcessSpellCast;

            #region Others
            
            AIBaseClient.OnProcessSpellCast += MyLogic.Others.Events.OnProcessSpellcast;
            Drawing.OnDraw += MyLogic.Others.Events.OnDraw;
            Game.OnUpdate += MyLogic.Others.SkinHack.OnUpdate;

            #endregion
        }
    }
}