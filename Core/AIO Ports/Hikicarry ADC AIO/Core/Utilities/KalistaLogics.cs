using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using HikiCarry.Champions;
using HikiCarry.Core.Predictions;
using LeagueSharpCommon;
using SPrediction;

namespace HikiCarry.Core.Utilities
{
    class KalistaLogics
    {
        private static readonly float[] RRD = { 20, 30, 40, 50, 60 };
        private static readonly float[] RRDM = { 0.6f, 0.6f, 0.6f, 0.6f, 0.6f };
        private static readonly float[] RRPS = { 10, 14, 19, 25, 32 };
        private static readonly float[] RRPSM = { 0.2f, 0.225f, 0.25f, 0.275f, 0.3f };

        public static void PierceCombo()
        {
            foreach (var enemy in GameObjects.EnemyHeroes.Where(hero => hero.IsValidTarget(Kalista.Q.Range) && hero.IsVisible))
            {
                Kalista.Q.Do(enemy,Utilities.HikiChance("hitchance"),true);
            }
        }
        public static void PierceJungleClear(Spell spell)
        {
            var mob = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth);
            if (mob == null || mob.Count == 0)
            {
                return;
            }

            Kalista.Q.Do(mob[0], Utilities.HikiChance("hitchance"));

        }
        public static void RendCombo()
        {
            foreach (var enemy in GameObjects.EnemyHeroes.Where(hero => hero.IsValidTarget(Kalista.E.Range) && !hero.HasBuffOfType(BuffType.SpellShield)))
            {
                if (enemy.Health < ChampionTotalDamage(enemy))
                {
                    Kalista.E.Cast();
                }
            }
        }
        public static void RendHarass(int spearcount)
        {
            foreach (var enemy in GameObjects.EnemyHeroes.Where(hero => hero.IsValidTarget(Kalista.E.Range)))
            {
                int enemyStack = enemy.Buffs.FirstOrDefault(x => x.Name == "KalistaExpungeMarker").Count;
                if (enemyStack > 0 && enemyStack > spearcount)
                {
                    Kalista.E.Cast();
                }
            }  
        }
        public static void RendClear(int minionCount)
        {
            var mns = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Kalista.E.Range);
            var mkc = mns.Count(x => Kalista.E.CanCast(x) && x.Health <= Kalista.E.GetDamage(x));
            if (mkc >= minionCount)
            {
                Kalista.E.Cast();
            }
        }
        public static void RendSiegeMinions()
        {
            foreach (var lasthit in MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Kalista.E.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Enemy).Where(x => x.Name == "MinionSiege"))
            {
                if (MinionCalculator(lasthit) > lasthit.Health)
                {
                    Kalista.E.Cast();
                }
            }
        }
        public static void RendJungleClear()
        {
            var mob = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth);
            if (mob == null || (mob != null && mob.Count == 0))
            {
                return;
            }
            if (mob[0].Health <= (Kalista.E.GetDamage(mob[0])/2))
            {
                Kalista.E.Cast();
            }
        }
        public static void FatesCall(int healthpercent)
        {
            foreach (var support in ObjectManager.Get<AIHeroClient>().Where(x=> x.IsAlly && !x.IsMe && x.Distance(ObjectManager.Player.Position) < Kalista.R.Range &&
                x.HasBuff("kalistacoopstrikeally")))
            {
                if (support.Health < healthpercent)
                {
                    Kalista.R.Cast();
                }
            }
        }
        public static void SentinelCombo() // will be come soon
        {
            
        }
        public static void BlueOrb(int level)
        {
            if (ObjectManager.Player.Level >= level && ObjectManager.Player.InShop() && !(Items.HasItem(ObjectManager.Player,3342) || Items.HasItem(ObjectManager.Player,3363)))
            {
                ObjectManager.Player.BuyItem(ItemId.Farsight_Alteration);
            }
        }
        public static void KillStealWithPierce()
        {
            foreach (var target in GameObjects.EnemyHeroes.Where(x=>x.IsValidTarget(Kalista.Q.Range)).OrderByDescending(x => x.Health))
            {
                if (target != null)
                {
                    Kalista.Q.Do(target, HitChance.High);
                }
            }
        }
        public static void KillStealWithRend()
        {
            foreach (var target in GameObjects.EnemyHeroes.OrderByDescending(x => x.Health))
            {
                if (target.Health < ChampionTotalDamage(target))
                {
                    Kalista.E.Cast();
                }
            }
        }
        public static void Bitterlogic(int hppercent)
        {
            foreach (var enemy in GameObjects.EnemyHeroes.Where(o => o.IsValidTarget(Kalista.E.Range)))
            {
                float spearDamage = ChampionTotalDamage(enemy);
                float killableSpearCount = enemy.Health / spearDamage;
                int totalSpear = (int)Math.Ceiling(killableSpearCount);
                if (ObjectManager.Player.Health < hppercent && KillableSpearCount(enemy) - 1 < totalSpear)
                {
                    Kalista.E.Cast();
                }
            }
        }
        public static bool ImmobileDetector(AIHeroClient target)
        {
            if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup) ||
                target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockback) ||
                target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Suppression) ||
                target.IsStunned)
            {
                return true;
            }
            else
                return false;
        }
        public static void ImmobilePierce()
        {
            foreach (var enemy in GameObjects.EnemyHeroes.Where(hero => hero.IsValidTarget(Kalista.Q.Range)))
            {
                if (ImmobileDetector(enemy))
                {
                    Kalista.Q.Do(enemy, Utilities.HikiChance("hitchance"));
                }
            }
        }
        public static void Balista(int minrange,int maxrange, Spell spell)
        {
            if (!spell.IsReady())
            {
                return;
            }
            var blitz = ObjectManager.Get<AIBaseClient>().FirstOrDefault(x => x.IsAlly && x.CharacterName == "Blitzcrank" && x.HasBuff("kalistacoopstrikeally"));
            if (blitz != null && spell.IsReady())
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(o => o.IsEnemy && o.IsValidTarget(2450f)))
                {
                    if (blitz.Distance(enemy.Position) <= 950f &&
                        blitz.Distance(ObjectManager.Player.Position) >= minrange &&
                        blitz.Distance(ObjectManager.Player.Position) <= maxrange)
                    {
                        if (enemy.Buffs != null && enemy.HasBuff("rocketgrab2"))
                        {
                            if (spell.IsReady())
                            {
                                spell.Cast();
                            }
                        }
                    }
                }
            }
        }
        public static void SKalista(int minrange, int maxrange, Spell spell)
        {
            if (!spell.IsReady())
            {
                return;
            }
            var skarner = ObjectManager.Get<AIBaseClient>().FirstOrDefault(o => o.IsAlly && o.CharacterName == "Skarner" &&
                o.HasBuff("kalistacoopstrikeally"));
            if (skarner != null && spell.IsReady())
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(o => o.IsEnemy 
                    && o.IsValidTarget(1849))) // Kalista R Range + Skarner R Range - 1
                {
                    if (skarner.Distance(enemy.Position) <= 350 &&
                        skarner.Distance(ObjectManager.Player.Position) >= minrange &&
                        skarner.Distance(ObjectManager.Player.Position) <= maxrange)
                    {
                        if (enemy.Buffs != null && enemy.HasBuff("SkarnerImpale"))
                        {
                            if (spell.IsReady())
                            {
                                spell.Cast();
                            }
                        }
                    }
                }
            }
        }
        public static void SupportProtector(Spell spell)
        {
            foreach (var ally in ObjectManager.Get<AIHeroClient>().Where(x => x.IsAlly && !x.IsMe && x.HasBuff("kalistacoopstrikeally") && x.Distance(ObjectManager.Player.Position) < Kalista.R.Range &&
                x.HealthPercent <= Initializer.Config["savePercent"].GetValue<MenuSlider>().Value))
            {
                spell.Cast();
            }
        }

        public static float CustomCalculator(AIBaseClient target, int customStacks = -1)
        {
            int buff = target.GetBuffCount("KalistaExpungeMarker");

            if (buff > 0 || customStacks > -1)
            {
                var tDamage = (RRD[Kalista.E.Level - 1] + RRDM[Kalista.E.Level - 1] * (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod)) +
                       ((customStacks < 0 ? buff : customStacks) - 1) *
                       (RRPS[Kalista.E.Level - 1] + RRPSM[Kalista.E.Level - 1] * (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod));

                return (float)ObjectManager.Player.CalculateDamage(target, DamageType.Physical, tDamage);
            }
            return 0;
        }
        public static float JungleCalculator(AIMinionClient minion, int customStacks = -1)
        {
            int buff = minion.GetBuffCount("KalistaExpungeMarker");

            if (buff > 0 || customStacks > -1)
            {
                var tDamage = (RRD[Kalista.E.Level - 1] + RRDM[Kalista.E.Level - 1] * (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod)) +
                       ((customStacks < 0 ? buff : customStacks) - 1) *
                       (RRPS[Kalista.E.Level - 1] + RRPSM[Kalista.E.Level - 1] * (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod));

                return (float)ObjectManager.Player.CalculateDamage(minion, DamageType.Physical, tDamage);
            }

            return 0;
        }
        public static float MinionCalculator(AIBaseClient minion, int customStacks = -1)
        {
            int buff = minion.GetBuffCount("KalistaExpungeMarker");

            if (buff > 0 || customStacks > -1)
            {
                var tDamage = (RRD[Kalista.E.Level - 1] + RRDM[Kalista.E.Level - 1] * (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod)) +
                       ((customStacks < 0 ? buff : customStacks) - 1) *
                       (RRPS[Kalista.E.Level - 1] + RRPSM[Kalista.E.Level - 1] * (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod));

                return (float)ObjectManager.Player.CalculateDamage(minion, DamageType.Physical, tDamage);
            }

            return 0;
        }
        public static float ChampionTotalDamage(AIHeroClient target)
        {
            var damage = 0f;

            if (Kalista.E.IsReady())
            {
                switch (Initializer.Config["calculator"].GetValue<MenuList>().Index)
                {
                    case 0:
                        damage += CustomCalculator(target);
                        break;
                    case 1:
                        damage += (float)ObjectManager.Player.CalculateDamage(target, DamageType.Physical, Kalista.E.GetDamage(target));
                        break;
                }

            }
            return (float)damage;
        }
        public static float JungleTotalDamage(AIMinionClient target)
        {
            var damage = 0f;

            if (Kalista.E.IsReady())
            {
                switch (Initializer.Config["calculator"].GetValue<MenuList>().Index)
                {
                    case 0:
                        damage += JungleCalculator(target);
                        break;
                    case 1:
                        damage += (float)ObjectManager.Player.CalculateDamage(target, DamageType.Physical, Kalista.E.GetDamage(target));
                        break;
                }
            }
            return (float)damage;
        }
        public static float MinionTotalDamage(AIMinionClient target)
        {
            var damage = 0f;

            if (Kalista.E.IsReady())
            {
                switch (Initializer.Config["calculator"].GetValue<MenuList>().Index)
                {
                    case 0:
                        damage += JungleCalculator(target);
                        break;
                    case 1:
                        damage += (float)ObjectManager.Player.CalculateDamage(target, DamageType.Physical, Kalista.E.GetDamage(target));
                        break;
                }

            }
            return (float)damage;
        }
        public static int KillableSpearCount(AIHeroClient enemy)
        {
            float spearDamage = ChampionTotalDamage(enemy);
            float killableSpearCount = enemy.Health / spearDamage;
            int totalSpear = (int)Math.Ceiling(killableSpearCount) - 1;

            return totalSpear;
        }


    }
}