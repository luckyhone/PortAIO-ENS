using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Cassiopeia.Misc;

namespace Entropy.AIO.Cassiopeia.Logics
{
    using Bases;
    using static Components;

    static class Harass
    {
        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        public static void ExecuteELast()
        {
            if (!LastHitMenu.EBool.Enabled                       ||
                ChampionBase.E.Level    <= 0                     ||
                LocalPlayer.Mana <= ChampionBase.E.Mana ||
                !ChampionBase.E.IsReady())
            {
                return;
            }

            if (MinionManager.AACheckMinion != null)
            {
                Lasthit.possibleTarget = MinionManager.AACheckMinion;
            }


            if (MinionManager.GetBestLasthitMinion == null)
            {
                return;
            }

            ChampionBase.E.Cast(MinionManager.GetBestLasthitMinion);
        }

        public static void ExecuteQ()
        {
            if (!HarassMenu.QBool.Enabled || !ChampionBase.Q.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(ChampionBase.Q.Range,DamageType.Magical);
            if (target == null)
            {
                return;
            }

            ChampionBase.Q.Cast(target);
        }

        public static void ExecuteW()
        {
            if (!HarassMenu.WBool.Enabled || !ChampionBase.W.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(ChampionBase.W.Range,DamageType.Magical);
            if (target == null || target.DistanceToPlayer() <= 500)
            {
                return;
            }

            ChampionBase.W.Cast(target);
        }

        public static void ExecuteE()
        {
            if (!HarassMenu.EBool.Enabled || !ChampionBase.E.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(ChampionBase.E.Range,DamageType.Magical);
            if (target == null)
            {
                return;
            }

            if (Definitions.CachedPoisoned.ContainsKey((uint)target.NetworkId))
            {
                ChampionBase.E.CastOnUnit(target);
            }

            if (HarassMenu.EPoisonBool.Enabled)
            {
                return;
            }

            ChampionBase.E.CastOnUnit(target);
        }
    }
}