using System;
using BadaoGP;

namespace BadaoKingdom.BadaoChampion.BadaoGangplank
{
    using static BadaoMainVariables;
    using static BadaoGangplankVariables;
    public static class BadaoGangplank
    {
        public static void BadaoActivate()
        {
            BadaoGangplankConfig.BadaoActivate();
            BadaoGangplankBarrels.BadaoActivate();
            BadaoGangplankCombo.BadaoActivate();
            BadaoGangplankHarass.BadaoActivate();
            BadaoGangplankLaneClear.BadaoActivate();
            BadaoGangplankJungleClear.BadaoActivate();
            BadaoGangplankAuto.BadaoActivate();
            //Game.OnUpdate += Game_OnUpdate;
        }
        private static void Game_OnUpdate(EventArgs args)
        {

        }
    }
}