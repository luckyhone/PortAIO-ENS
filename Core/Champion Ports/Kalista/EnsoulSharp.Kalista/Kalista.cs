using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;

namespace EnsoulSharp.Kalista
{
    using Color = System.Drawing.Color;

    internal class Kalista
    {
        private static Menu MyMenu;
        private static Spell Q, E, R;
        private static AIHeroClient SweetHeart = null;

        private static Dictionary<float, float> _incomingDamage = new Dictionary<float, float>();
        private static Dictionary<float, float> _instantDamage = new Dictionary<float, float>();
        public static float IncomingDamage
        {
            get { return _incomingDamage.Sum(e => e.Value) + _instantDamage.Sum(e => e.Value); }
        }

        public static void OnLoad()
        {
            Q = new Spell(SpellSlot.Q, 1150f);
            Q.SetSkillshot(.25f,40f,2400f,true,SpellType.Line);
            E = new Spell(SpellSlot.E, 1000f);
            R = new Spell(SpellSlot.R, 1500f);

            MyMenu = new Menu("ensoulsharp.kalista", "EnsoulSharp.Kalista", true);

            var combat = new Menu("combat", "Combo Settings");
            combat.Add(MenuWrapper.Combat.Q);
            combat.Add(MenuWrapper.Combat.DisableQ);
            combat.Add(MenuWrapper.Combat.E);
            combat.Add(MenuWrapper.Combat.DisableE);
            combat.Add(MenuWrapper.Combat.DisableE2);
            combat.Add(MenuWrapper.Combat.OrbwalkerMinion);
            MyMenu.Add(combat);

            var harass = new Menu("harass", "Harass Settings");
            harass.Add(MenuWrapper.Harass.Q);
            harass.Add(MenuWrapper.Harass.QMinion);
            harass.Add(MenuWrapper.Harass.E);
            harass.Add(MenuWrapper.Harass.DisableE);
            harass.Add(MenuWrapper.Harass.DisableE2);
            harass.Add(MenuWrapper.Harass.Mana);
            MyMenu.Add(harass);

            var lane = new Menu("lane", "LaneClear Settings");
            lane.Add(MenuWrapper.LaneClear.E);
            lane.Add(MenuWrapper.LaneClear.Mana);
            MyMenu.Add(lane);

            var jungle = new Menu("jungle", "JungleClear Settings");
            jungle.Add(MenuWrapper.JungleClear.Q);
            jungle.Add(MenuWrapper.JungleClear.E);
            jungle.Add(MenuWrapper.JungleClear.Mana);

            var killable = new Menu("killable", "KillSteal Settings");
            killable.Add(MenuWrapper.KillAble.Q);
            killable.Add(MenuWrapper.KillAble.E);
            MyMenu.Add(killable);

            var misc = new Menu("misc", "Misc Settings");
            misc.Add(MenuWrapper.Misc.R);
            misc.Add(MenuWrapper.Misc.HP);
            MyMenu.Add(misc);

            var draw = new Menu("draw", "Draw Settings");
            draw.Add(MenuWrapper.Draw.Q);
            draw.Add(MenuWrapper.Draw.E);
            draw.Add(MenuWrapper.Draw.OnlyReady);
            draw.Add(MenuWrapper.Draw.DMG);
            MyMenu.Add(draw);
            MyMenu.Add(new MenuSeparator("author", "Author: EnsoulSharp Team"));

            MyMenu.Attach();

            Game.OnUpdate += OnTick;
            AIBaseClient.OnProcessSpellCast += OnDoCast;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += OnEndScene;
        }
        private static readonly float[] EBaseDamage = {0, 20, 30, 40, 50, 60, 60};
        private static readonly float[] EStackBaseDamage = {0, 10, 14, 19, 25, 32, 32};
        private static readonly float[] EStackMultiplierDamage = {0, .198f, .23748f, .27498f, .31248f, .34988f};

        private static float GetEDamage(AIBaseClient target)
        {
            var eLevel = E.Level;
            var eBaseDamage = EBaseDamage[eLevel] + .6 * GameObjects.Player.TotalAttackDamage;
            var eStackDamage = EStackBaseDamage[eLevel] +
                               EStackMultiplierDamage[eLevel] * GameObjects.Player.TotalAttackDamage;
            var eStacksOnTarget = target.GetBuffCount("kalistaexpungemarker");
            if (eStacksOnTarget == 0)
            {
                return 0;
            }

            var total = eBaseDamage + eStackDamage * (eStacksOnTarget - 1);
            if (target is AIMinionClient minion && (minion.GetJungleType() & JungleType.Legendary) != 0)
            {
                total /= 2;
            }

            return (float) GameObjects.Player.CalculateDamage(target, DamageType.Physical, total);
        }

        // Credit Hellsing
        private static void SaveSweetHeart()
        {
            if (SweetHeart == null)
            {
                SweetHeart = GameObjects.AllyHeroes.Find(h => h.Buffs.Any(b => b.Caster.IsMe && b.Name.Contains("kalistacoopstrikeally")));
            }
            else if (MenuWrapper.Misc.R.Enabled && R.IsReady())
            {
                if (SweetHeart.HealthPercent < MenuWrapper.Misc.HP.Value && SweetHeart.CountEnemyHeroesInRange(500) > 0 ||
                    IncomingDamage > SweetHeart.Health)
                {
                    R.Cast();
                }
            }

            foreach (var entry in _incomingDamage)
            {
                if (entry.Key < Game.Time)
                {
                    _incomingDamage.Remove(entry.Key);
                }
            }

            foreach (var entry in _instantDamage)
            {
                if (entry.Key < Game.Time)
                {
                    _instantDamage.Remove(entry.Key);
                }
            }
        }

        private static void KillAble()
        {
            if (MenuWrapper.KillAble.Q.Enabled && Q.IsReady())
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range) && !x.IsInvulnerable))
                {
                    if (target.IsValidTarget(Q.Range) && target.Health < Q.GetDamage(target))
                    {
                        var qPred = Q.GetPrediction(target, false, 0);
                        if (qPred.Hitchance >= HitChance.High)
                        {
                            Q.Cast(qPred.UnitPosition);
                        }
                    }
                }
            }

            if (MenuWrapper.KillAble.E.Enabled && E.IsReady())
            {
                if (GameObjects.EnemyHeroes.Any(x =>
                    x.IsValidTarget(E.Range) && x.HasBuff("kalistaexpungemarker") && x.Health < GetEDamage(x) && !x.IsInvulnerable))
                {
                    E.Cast();
                }
            }
        }

        private static void Combat()
        {
            var target = TargetSelector.GetTarget(Q.Range,DamageType.Physical);
            if (target == null || !target.IsValidTarget(Q.Range))
            {
                return;
            }

            if (MenuWrapper.Combat.Q.Enabled && Q.IsReady())
            {
                if (MenuWrapper.Combat.DisableQ.Enabled)
                {
                    if (ObjectManager.Player.AttackSpeed() < 1.98)
                    {
                        var qPred = Q.GetPrediction(target, false, 0);
                        if (qPred.Hitchance >= HitChance.High)
                        {
                            Q.Cast(qPred.UnitPosition);
                        }
                    }
                }
                else
                {
                    var qPred = Q.GetPrediction(target, false, 0);
                    if (qPred.Hitchance >= HitChance.High)
                    {
                        Q.Cast(qPred.UnitPosition);
                    }
                }
            }

            if (MenuWrapper.Combat.E.Enabled && E.IsReady())
            {
                foreach (var t in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(E.Range) && x.HasBuff("kalistaexpungemarker")))
                {
                    if (MenuWrapper.Combat.DisableE.Enabled)
                    {
                        if (t.HasBuffOfType(BuffType.Asleep) || t.HasBuffOfType(BuffType.Charm) ||
                            t.HasBuffOfType(BuffType.Fear) || t.HasBuffOfType(BuffType.Knockup) ||
                            t.HasBuffOfType(BuffType.Asleep) || t.HasBuffOfType(BuffType.Slow) ||
                            t.HasBuffOfType(BuffType.Stun))
                        {
                            continue;
                        }

                        if (GameObjects.EnemyMinions.Any(m => m.IsValidTarget(E.Range) && m.HasBuff("kalistaexpungemarker") && m.Health <= GetEDamage(m)))
                        {
                            if (MenuWrapper.Combat.DisableE2.Enabled)
                            {
                                if (Variables.TickCount - E.LastCastAttemptTime > 2500)
                                {
                                    E.Cast();
                                }
                            }
                            else
                            {
                                E.Cast();
                            }
                        }
                    }
                }
            }

            if (MenuWrapper.Combat.OrbwalkerMinion.Enabled)
            {
                if (GameObjects.EnemyHeroes.All(x => !x.IsValidTarget(ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + x.BoundingRadius)) &&
                    GameObjects.EnemyHeroes.Any(x => x.IsValidTarget(1000)))
                {
                    var AttackUnit =
                        GameObjects.EnemyMinions.Where(x => x.InAutoAttackRange())
                            .OrderBy(x => x.Distance(Game.CursorPos))
                            .FirstOrDefault();

                    if (AttackUnit != null && !AttackUnit.IsDead && AttackUnit.InAutoAttackRange())
                    {
                        Orbwalker.ForceTarget = AttackUnit;
                    }
                }
                else
                {
                    Orbwalker.ForceTarget = null;
                }
            }
        }

        private static void Harass()
        {
            if (ObjectManager.Player.ManaPercent < MenuWrapper.Harass.Mana.Value)
            {
                return;
            }

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target == null || !target.IsValidTarget(Q.Range))
            {
                return;
            }

            if (MenuWrapper.Harass.Q.Enabled && Q.IsReady())
            {
                var qPred = Q.GetPrediction(target, false, 0);
                if (qPred.Hitchance >= HitChance.High)
                {
                    Q.Cast(qPred.UnitPosition);
                }
                else if (MenuWrapper.Harass.QMinion.Enabled)
                {
                    var c = qPred.CollisionObjects;
                    if (c.Count > 0 && !c.All(x =>
                            GameObjects.EnemyMinions.Any(m => m.NetworkId == x.NetworkId) ||
                            GameObjects.Heroes.Any(h => h.NetworkId == x.NetworkId))) // no hit on windwall
                    {
                        if (c.Any(x => x.IsValidTarget(Q.Range) && x.Health < Q.GetDamage(x)))
                        {
                            Q.Cast(qPred.UnitPosition);
                        }
                    }
                }
            }

            if (MenuWrapper.Harass.E.Enabled && E.IsReady())
            {
                foreach (var t in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(E.Range) && x.HasBuff("kalistaexpungemarker")))
                {
                    if (MenuWrapper.Harass.DisableE.Enabled)
                    {
                        if (t.HasBuffOfType(BuffType.Asleep) || t.HasBuffOfType(BuffType.Charm) ||
                            t.HasBuffOfType(BuffType.Fear) || t.HasBuffOfType(BuffType.Knockup) ||
                            t.HasBuffOfType(BuffType.Asleep) || t.HasBuffOfType(BuffType.Slow) ||
                            t.HasBuffOfType(BuffType.Stun))
                        {
                            continue;
                        }

                        if (GameObjects.EnemyMinions.Any(m => m.IsValidTarget(E.Range) && m.HasBuff("kalistaexpungemarker") && m.Health <= GetEDamage(m)))
                        {
                            if (MenuWrapper.Harass.DisableE2.Enabled)
                            {
                                if (Variables.TickCount - E.LastCastAttemptTime > 2500)
                                {
                                    E.Cast();
                                }
                            }
                            else
                            {
                                E.Cast();
                            }
                        }
                    }
                }
            }
        }

        private static void LaneClear()
        {
            if (ObjectManager.Player.ManaPercent < MenuWrapper.LaneClear.Mana.Value)
            {
                return;
            }

            if (MenuWrapper.LaneClear.E.Enabled && E.IsReady())
            {
                if (GameObjects.EnemyMinions.Count(x =>
                        x.IsValidTarget(E.Range) && x.HasBuff("kalistaexpungemarker") && x.Health < GetEDamage(x)) >=
                    MenuWrapper.LaneClear.E.Value)
                {
                    E.Cast();
                }
            }
        }

        private static void JungleClear()
        {
            if (ObjectManager.Player.ManaPercent < MenuWrapper.JungleClear.Mana.Value)
            {
                return;
            }

            if (MenuWrapper.JungleClear.Q.Enabled && Q.IsReady())
            {
                foreach (var mob in GameObjects.Jungle.Where(x => x.IsValidTarget(Q.Range) && (x.GetJungleType() == JungleType.Large || x.GetJungleType() == JungleType.Legendary)))
                {
                    if (mob.IsValidTarget(Q.Range))
                    {
                        var qPred = Q.GetPrediction(mob);
                        if (qPred.Hitchance >= HitChance.Medium)
                        {
                            Q.Cast(qPred.UnitPosition);
                        }
                    }
                }
            }

            if (MenuWrapper.JungleClear.E.Enabled && E.IsReady())
            {
                if (GameObjects.JungleLarge.Any(x => x.IsValidTarget(E.Range) && x.HasBuff("kalistaexpungemarker") && x.Health < GetEDamage(x) * 0.5))
                {
                    E.Cast();
                }

                if (GameObjects.JungleLegendary.Any(x => x.IsValidTarget(E.Range) && x.HasBuff("kalistaexpungemarker") && x.Health < GetEDamage(x) * 0.5))
                {
                    E.Cast();
                }

                if (GameObjects.JungleSmall.Any(x => x.IsValidTarget(E.Range) && x.HasBuff("kalistaexpungemarker") && x.Health < GetEDamage(x)))
                {
                    E.Cast();
                }
            }
        }

        private static void OnTick(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling() || MenuGUI.IsChatOpen)
            {
                return;
            }

            SaveSweetHeart();
            KillAble();

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combat();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;
            }
        }

        private static void OnDoCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy)
            {
                if (SweetHeart != null && MenuWrapper.Misc.R.Enabled)
                {
                    if ((sender.Type != GameObjectType.AIHeroClient || Orbwalker.IsAutoAttack(args.SData.Name)) && args.Target != null && args.Target.NetworkId == SweetHeart.NetworkId)
                    {
                        _incomingDamage.Add(SweetHeart.Position.Distance(sender.Position) / args.SData.MissileSpeed + Game.Time, (float)sender.GetAutoAttackDamage(SweetHeart));
                    }
                    else if (sender.Type == GameObjectType.AIHeroClient)
                    {
                        var attacker = sender as AIHeroClient;
                        var slot = attacker.GetSpellSlot(args.SData.Name);

                        if (slot != SpellSlot.Unknown)
                        {
                            if (slot.HasFlag(SpellSlot.Q | SpellSlot.W | SpellSlot.E | SpellSlot.R) &&
                                (args.Target != null && args.Target.NetworkId == SweetHeart.NetworkId ||
                                args.To.Distance(SweetHeart.Position) < Math.Pow(args.SData.LineWidth, 2)))
                            {
                                _instantDamage.Add(Game.Time + 2, (float)attacker.GetSpellDamage(SweetHeart, slot));
                            }
                        }
                    }
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || MenuGUI.IsChatOpen)
            {
                return;
            }

            if (MenuWrapper.Draw.Q.Enabled)
            {
                if (MenuWrapper.Draw.OnlyReady.Enabled && Q.IsReady())
                {
                    CircleRender.Draw(GameObjects.Player.Position, Q.Range, SharpDX.Color.Blue, 1);
                }
                else if (!MenuWrapper.Draw.OnlyReady.Enabled)
                {
                    CircleRender.Draw(GameObjects.Player.Position, Q.Range, SharpDX.Color.Blue, 1);
                }
            }

            if (MenuWrapper.Draw.E.Enabled)
            {
                if (MenuWrapper.Draw.OnlyReady.Enabled && E.IsReady())
                {
                    CircleRender.Draw(GameObjects.Player.Position, E.Range, SharpDX.Color.Red, 1);
                }
                else if (!MenuWrapper.Draw.OnlyReady.Enabled)
                {
                    CircleRender.Draw(GameObjects.Player.Position, E.Range, SharpDX.Color.Red, 1);
                }
            }
        }

        private static void OnEndScene(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || MenuGUI.IsChatOpen)
            {
                return;
            }

            if (MenuWrapper.Draw.DMG.Enabled && E.IsReady())
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(x =>
                    x.IsValidTarget() && x.IsVisibleOnScreen && x.HasBuff("kalistaexpungemarker")))
                {
                    var dmg = GetEDamage(target);
                    if (dmg > 0)
                    {
                        var barPos = target.HPBarPosition;
                        var xPos = barPos.X - 45;
                        var yPos = barPos.Y - 19;
                        if (target.CharacterName == "Annie")
                        {
                            yPos += 2;
                        }

                        var remainHealth = target.Health - dmg;
                        var x1 = xPos + (target.Health / target.MaxHealth * 104);
                        var x2 = (float) (xPos + ((remainHealth > 0 ? remainHealth : 0) / target.MaxHealth * 103.4));
                        Drawing.DrawLine(x1, yPos, x2, yPos, 11, Color.FromArgb(255, 147, 0));
                    }
                }
            }
        }
    }
}