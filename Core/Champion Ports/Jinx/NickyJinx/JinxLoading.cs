using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;

namespace NickyJinx
{
    using static IsMenu;
    using Color = System.Drawing.Color;
    internal class JinxLoading
    {
        private static Menu MyMenu;
        private static Spell Q, W, E, R;

        internal static void OnLoad()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1500f);
            E = new Spell(SpellSlot.E, 900f);
            R = new Spell(SpellSlot.R, 25000f);

            W.SetSkillshot(0.6f, 60f, 3300f, true, SpellType.Line);
            E.SetSkillshot(0.7f, 120f, 1750f, false, SpellType.Circle);
            R.SetSkillshot(0.6f, 140f, 1700f, false, SpellType.Line);

            MyMenu = new Menu("ensoulsharp.jinx", "EnsoulSharp.Jinx", true);
            var comboJinx = MyMenu.Add(new Menu("combo", "Combo")
            {
                Combat.Q,
                Combat.W,
                Combat.E,
            });
            var harassJinx = MyMenu.Add(new Menu("harass", "Harass")
            {
                Harass.Q,
                Harass.W,
                Harass.Mana,
            });
            var KillAbleJinx = MyMenu.Add(new Menu("killsteal", "Killsteal")
            {
                KillAble.W,
                KillAble.R,
            });
            var MiscJinx = MyMenu.Add(new Menu("Misc", "Misc")
            {
                Misc.RAntiGapcloser,
                Misc.RInterrupt,
            });
            var drawJinx = MyMenu.Add(new Menu("draw", "Draw")
            {
                Draw.W
            });
            MyMenu.Add(SemiR.Key);
            MyMenu.Attach();

            Game.OnUpdate += OnTick;
            Drawing.OnDraw += OnDraw;
        }

        private static void OnTick(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || MenuGUI.IsChatOpen)
            {
                return;
            }

            AutoE();
            AutoR();

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                 var t = TargetSelector.GetTarget(W.Range,DamageType.Physical);
                    ComboJinx(t);
                    break;
            }
        }

        private static void AutoR()
        {
            if (R.IsReady())
            {
                var checkRok = true;
                var minR = 1200;
                var maxR = 2500;
                var t = TargetSelector.GetTarget(maxR,DamageType.Physical);

                if (t.IsValidTarget())
                {
                    var distance = GetRealDistance(t);

                    if (!checkRok)
                    {
                        if (ObjectManager.Player.GetSpellDamage(t, SpellSlot.R) > t.Health)
                        {
                            if (R.Cast(t) != CastStates.SuccessfullyCasted) { }
                        }
                    }
                    else if (distance > minR)
                    {
                        var aDamage = ObjectManager.Player.GetAutoAttackDamage(t);
                        var wDamage = ObjectManager.Player.GetSpellDamage(t, SpellSlot.W);
                        var rDamage = ObjectManager.Player.GetSpellDamage(t, SpellSlot.R);
                        var powPowRange = GetRealPowPowRange(t);

                        if (distance < (powPowRange + QAddRange) && !(aDamage * 3.5 > t.Health))
                        {
                            if (!W.IsReady() || !(wDamage > t.Health) || W.GetPrediction(t).CollisionObjects.Count > 0)
                            {
                                if (CountAlliesNearTarget(t, 500) <= 3)
                                {
                                    if (rDamage > t.Health)
                                    {
                                        if (R.Cast(t) != CastStates.SuccessfullyCasted) { }
                                    }
                                }
                            }
                        }
                        else if (distance > (powPowRange + QAddRange))
                        {
                            if (!W.IsReady() || !(wDamage > t.Health) || distance > W.Range ||
                                W.GetPrediction(t).CollisionObjects.Count > 0)
                            {
                                if (CountAlliesNearTarget(t, 500) <= 3)
                                {
                                    if (rDamage > t.Health)
                                    {
                                        if (R.Cast(t) != CastStates.SuccessfullyCasted) { }
                                    }
                                }
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
            if (W.IsReady())
            {
                CircleRender.Draw(GameObjects.Player.Position, W.Range, Color.Crimson.ToSharpDxColor());
            }
        }

            private static void AutoE()
        {

            foreach (
                var enemy in ObjectManager.Get<AIHeroClient>().Where(enemy => enemy.IsValidTarget(E.Range - 150)))
            {
                if (E.IsReady() && enemy.HasBuffOfType(BuffType.Slow))
                {
                    var castPosition =
                        Prediction.GetPrediction(
                            new PredictionInput
                            {
                                Unit = enemy,
                                Delay = 0.7f,
                                Radius = 120f,
                                Speed = 1750f,
                                Range = 900f,
                                Type = SpellType.Circle,
                            }).CastPosition;


                    if (GetSlowEndTime(enemy) >= (Game.Time + E.Delay + 0.5f))
                    {
                        E.Cast(castPosition);
                    }
                }

                if (E.IsReady() &&
                    (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) ||
                     enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) ||
                     enemy.HasBuffOfType(BuffType.Taunt) || enemy.HasBuff("zhonyasringshield") ||
                     enemy.HasBuff("Recall")))
                {
                    E.CastIfHitchanceEquals(enemy, HitChance.High);
                }

                if (E.IsReady() && enemy.IsDashing())
                {
                    E.CastIfHitchanceEquals(enemy, HitChance.Dash);
                }
            }
        }

        private static void ComboJinx(AIHeroClient target)
        {
            if (W.IsReady())
            {
                var t = TargetSelector.GetTarget(W.Range,DamageType.Physical);

                if (t.IsValidTarget() && GetRealDistance(t) >= 800)
                {
                    if (W.Cast(t) != CastStates.SuccessfullyCasted)
                    {
                        return;
                    }
                }
            }

            var useQ = true;
            if (useQ)
            {
                foreach (var t in
                    ObjectManager.Get<AIHeroClient>()
                        .Where(t => t.IsValidTarget(GetRealPowPowRange(t) + QAddRange + 20f)))
                {
                    var swapDistance = true;
                    var swapAoe = true;
                    var distance = GetRealDistance(t);
                    var powPowRange = GetRealPowPowRange(t);

                    if (swapDistance && Q.IsReady())
                    {
                        if (distance > powPowRange && !FishBoneActive)
                        {
                            if (Q.Cast())
                            {
                                return;
                            }
                        }
                        else if (distance < powPowRange && FishBoneActive)
                        {
                            if (Q.Cast())
                            {
                                return;
                            }
                        }
                    }

                    if (swapAoe && Q.IsReady())
                    {
                        if (distance > powPowRange && PowPowStacks > 2 && !FishBoneActive && CountEnemies(t, 150) > 1)
                        {
                            if (Q.Cast())
                            {
                                return;
                            }
                        }
                    }
                }
            }


            if (R.IsReady())
            {
                var checkRok = true;
                var minR = 1200;
                var maxR = 2500;
                var t = TargetSelector.GetTarget(maxR,DamageType.Physical);

                if (t.IsValidTarget())
                {
                    var distance = GetRealDistance(t);

                    if (!checkRok)
                    {
                        if (ObjectManager.Player.GetSpellDamage(t, SpellSlot.R) > t.Health)
                        {
                            if (R.Cast(t) != CastStates.SuccessfullyCasted) { }
                        }
                    }
                    else if (distance > minR)
                    {
                        var aDamage = ObjectManager.Player.GetAutoAttackDamage(t);
                        var wDamage = ObjectManager.Player.GetSpellDamage(t, SpellSlot.W);
                        var rDamage = ObjectManager.Player.GetSpellDamage(t, SpellSlot.R);
                        var powPowRange = GetRealPowPowRange(t);

                        if (distance < (powPowRange + QAddRange) && !(aDamage * 3.5 > t.Health))
                        {
                            if (!W.IsReady() || !(wDamage > t.Health) || W.GetPrediction(t).CollisionObjects.Count > 0)
                            {
                                if (CountAlliesNearTarget(t, 500) <= 3)
                                {
                                    if (rDamage > t.Health)
                                    {
                                        if (R.Cast(t) != CastStates.SuccessfullyCasted) { }
                                    }
                                }
                            }
                        }
                        else if (distance > (powPowRange + QAddRange))
                        {
                            if (!W.IsReady() || !(wDamage > t.Health) || distance > W.Range ||
                                W.GetPrediction(t).CollisionObjects.Count > 0)
                            {
                                if (CountAlliesNearTarget(t, 500) <= 3)
                                {
                                    if (rDamage > t.Health)
                                    {
                                        if (R.Cast(t) != CastStates.SuccessfullyCasted) { }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static int CountEnemies(AIBaseClient target, float range)
        {
            return
                ObjectManager.Get<AIHeroClient>()
                    .Count(
                        hero =>
                            hero.IsValidTarget() && hero.Team != ObjectManager.Player.Team &&
                            hero.Position.Distance(target.Position) <= range);
        }

        private static int CountAlliesNearTarget(AIBaseClient target, float range)
        {
            return ObjectManager.Get<AIHeroClient>().Count(hero => hero.Team == ObjectManager.Player.Team &&
            hero.Position.Distance(target.Position) <= range);
        }

        private static float GetRealPowPowRange(GameObject target)
        {
            return 525f + ObjectManager.Player.BoundingRadius + target.BoundingRadius;
        }

        private static float GetRealDistance(GameObject target)
        {
            return ObjectManager.Player.Position.Distance(target.Position) + ObjectManager.Player.BoundingRadius +
            target.BoundingRadius;
        }

        private static float GetSlowEndTime(AIBaseClient target)
        {
            return
                target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Type == BuffType.Slow)
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault();
        }

        public static float QAddRange
        {
            get { return 50 + 25 * ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level; }
        }

        private static bool FishBoneActive
        {
            get { return ObjectManager.Player.AttackRange > 565f; }
        }

        private static int PowPowStacks
        {
            get
            {
                return ObjectManager.Player.Buffs.Where(buff => buff.Name.ToLower() == "jinxqramp").Select(buff => buff.Count)
                    .FirstOrDefault();
            }
        }
    }
}