using EnsoulSharp;

namespace Entropy.AIO.Cassiopeia.Logics
{
    using Misc;
    using static Bases.ChampionBase;
    using static Components;

    static class Lasthit
    {
        public static GameObject possibleTarget;

        public static void ExecuteE()
        {
            if (!LastHitMenu.EBool.Enabled          ||
                E.Level                 <= 0        ||
                ObjectManager.Player.Mana <= E.Mana ||
                !E.IsReady())
            {
                return;
            }

            if (MinionManager.AACheckMinion != null)
            {
                possibleTarget = MinionManager.AACheckMinion;
            }


            if (MinionManager.GetBestLasthitMinion == null)
            {
                return;
            }

            E.Cast(MinionManager.GetBestLasthitMinion);
        }
    }
}