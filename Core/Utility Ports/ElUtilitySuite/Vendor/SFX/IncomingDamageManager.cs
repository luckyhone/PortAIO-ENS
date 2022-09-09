using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;

namespace ElUtilitySuite.Vendor.SFX
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    public static class IncomingDamageManager
    {
        private static readonly Dictionary<int, float> IncomingDamages = new Dictionary<int, float>();
        private static int _removeDelay = 300;

        static IncomingDamageManager()
        {
            AIBaseClient.OnDoCast += OnObjAiBaseProcessSpellCast;
            AIBaseClient.OnProcessSpellCast += OnObjAiBaseProcessSpellCast;
            AIBaseClient.OnProcessSpellCast += OnObjAiBaseProcessSpellCast;
        }
        

        public static bool Skillshots { get; set; }
        public static bool Enabled { get; set; }

        public static int RemoveDelay
        {
            get { return _removeDelay; }
            set { _removeDelay = value; }
        }

        private static void OnObjAiBaseProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            try
            {
                if (!Enabled)
                {
                    return;
                }
                var enemy = sender as AIHeroClient;
                var turret = sender as AITurretClient;
                foreach (
                    var hero in GameObjects.Heroes.Where(h => h.IsValid && IncomingDamages.ContainsKey(h.NetworkId)))
                {
                    if (ShouldReset(hero))
                    {
                        IncomingDamages[hero.NetworkId] = 0;
                        continue;
                    }

                    if (enemy != null && enemy.IsValid && enemy.IsEnemy && enemy.Distance(hero) <= 2000f)
                    {
                        if (args.Target != null && args.Target.NetworkId.Equals(hero.NetworkId))
                        {
                            if (sender.Spellbook.IsAutoAttack)
                            {
                                AddDamage(
                                    hero, (int)(GetTime(sender, hero, args.SData) * 0.3f),
                                    (float)sender.GetAutoAttackDamage(hero, true));
                            }
                            else if (args.SData.TargetingType == SpellDataTargetType.Target || args.SData.TargetingType == SpellDataTargetType.Self)
                            {
                                var a = sender.GetSpellSlot(args.SData.Name);
                                var b = new Spell(a);
                                AddDamage(
                                    hero, (int)(GetTime(sender, hero, args.SData) * 0.3f),
                                    (float)b.GetDamage(hero));
                            }
                        }

                        if (args.Target == null && Skillshots)
                        {
                            var slot = enemy.GetSpellSlot(args.SData.Name);
                            var spells = new Spell(slot);
                            if (slot != SpellSlot.Unknown &&
                                (slot == SpellSlot.Q || slot == SpellSlot.E || slot == SpellSlot.W ||
                                 slot == SpellSlot.R))
                            {
                                
                                var width = Math.Min(
                                    750f,
                                    (args.SData.TargetingType == SpellDataTargetType.Cone || args.SData.LineWidth > 0) &&
                                    args.SData.TargetingType != SpellDataTargetType.Cone
                                        ? args.SData.LineWidth
                                        : (args.SData.CastRadius <= 0
                                            ? args.SData.CastRadiusSecondary
                                            : args.SData.CastRadius));
                                if (args.End.Distance(hero.ServerPosition) <= Math.Pow(width, 2))
                                {
                                    AddDamage(
                                        hero, (int)(GetTime(sender, hero, args.SData) * 0.6f),
                                        (float)spells.GetDamage(hero));
                                }
                            }
                        }
                    }

                    if (turret != null && turret.IsValid && turret.IsEnemy && turret.Distance(hero) <= 1500f)
                    {
                        if (args.Target != null && args.Target.NetworkId.Equals(hero.NetworkId))
                        {
                            AddDamage(
                                hero, (int)(GetTime(sender, hero, args.SData) * 0.3f),
                                (float)
                                    sender.CalculateDamage(
                                        hero, DamageType.Physical,
                                        sender.BaseAttackDamage + sender.FlatPhysicalDamageMod));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static bool ShouldReset(AIHeroClient hero)
        {
            try
            {
                return !hero.IsValidTarget(float.MaxValue, false) || hero.IsZombie() ||
                       hero.HasBuffOfType(BuffType.Invulnerability) || hero.IsInvulnerable;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }

        private static float GetTime(AIBaseClient sender, AIHeroClient hero, SpellData sData)
        {
            try
            {
                return (Math.Max(2, sData.CastFrame / 30f) - 100 + Game.Ping / 2000f +
                        sender.Distance(hero.ServerPosition) / Math.Max(500, Math.Min(5000, sData.MissileSpeed))) *
                       1000f;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return 0;
        }

        private static void AddDamage(AIHeroClient hero, int delay, float damage)
        {
            try
            {
                if (delay >= 5000 || damage <= 0)
                {
                    return;
                }
                if (delay < 0)
                {
                    delay = 0;
                }
                DelayAction.Add(
                    delay, () =>
                    {
                        IncomingDamages[hero.NetworkId] += damage;
                        DelayAction.Add(
                            _removeDelay,
                            () =>
                            {
                                IncomingDamages[hero.NetworkId] = IncomingDamages[hero.NetworkId] - damage < 0
                                    ? 0
                                    : IncomingDamages[hero.NetworkId] - damage;
                            });
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void AddChampion(AIHeroClient hero)
        {
            try
            {
                if (IncomingDamages.ContainsKey(hero.NetworkId))
                {
                    throw new ArgumentException(
                        string.Format("IncomingDamageManager: NetworkId \"{0}\" already exist.", hero.NetworkId));
                }

                IncomingDamages[hero.NetworkId] = 0;

                Enabled = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static float GetDamage(AIHeroClient hero)
        {
            try
            {
                float damage;
                if (IncomingDamages.TryGetValue(hero.NetworkId, out damage))
                {
                    return damage;
                }
                throw new KeyNotFoundException(
                    string.Format("IncomingDamageManager: NetworkId \"{0}\" not found.", hero.NetworkId));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return 0;
        }
    }
}