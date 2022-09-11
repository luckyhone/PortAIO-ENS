using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Darius.Misc
{
    using static Bases.ChampionBase;

    class Damage
    {
        public static readonly float[] WBase =
        {
            0f, 0.5f, 0.55f, 0.6f, 0.65f, 0.7f
        };

        public static readonly float[] RBase =
        {
            0f, 100f, 200f, 300f
        };

        private static AIHeroClient LocalPlayer => ObjectManager.Player;

        public static double GetRDamage(AIBaseClient target)
        {
            var damage = 0f;

            damage = (float)LocalPlayer.CalculateDamage(target,
                DamageType.True,
                (RBase[R.Level] +
                 LocalPlayer.GetBonusPhysicalDamage() *
                 0.75f) *
                (1f + 0.2f * target.GetBuffCount("dariushemo")));
            return damage - target.HPRegenRate;
        }

        public static double Sheen(AIBaseClient target)
        {
            var damage = LocalPlayer.CalculateDamage(target,
                                                              DamageType.Physical,
                                                              LocalPlayer.TotalAttackDamage);
            if (LocalPlayer.HasItem(ItemId.Sheen))
            {
                var item = new Items.Item(ItemId.Sheen, 600);
                if (item.IsReady && !LocalPlayer.HasBuff("sheen"))
                {
                    damage = LocalPlayer.CalculateDamage(target,
                                                                  DamageType.Physical,
                                                                  LocalPlayer.TotalAttackDamage) +
                             damage;
                }
            }

            if (LocalPlayer.HasItem(ItemId.Trinity_Force))
            {
                var item = new Items.Item(ItemId.Trinity_Force, 600);
                if (item.IsReady && !LocalPlayer.HasBuff("TrinityForce"))
                {
                    damage = LocalPlayer.CalculateDamage(target,
                                                                  DamageType.Physical,
                                                                  LocalPlayer.BaseAttackDamage) *
                             2f +
                             damage;
                }
            }

            return damage;
        }

        public static double GetWDamage(AIBaseClient target)
        {
            var wLevel      = W.Level;
            var wBaseDamage = LocalPlayer.FlatPhysicalDamageMod * WBase[wLevel];
            var damage      = LocalPlayer.CalculateDamage(target, DamageType.Physical, wBaseDamage) + Sheen(target);

            return damage;
        }
    }
}