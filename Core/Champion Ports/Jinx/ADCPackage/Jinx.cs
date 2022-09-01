using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using EnsoulSharp.SDK.Utility;
using SebbyLib;
using SharpDX;
using SPrediction;
using Collision = SPrediction.Collision;
using Color = SharpDX.Color;
using HitChance = EnsoulSharp.SDK.HitChance;
using PredictionInput = EnsoulSharp.SDK.PredictionInput;

namespace ADCPackage.Plugins
{
    internal static class Jinx
    {
        public static Spell Q, W, E, R;

        private static bool EnemyInRange
            => TargetSelector.GetTarget(525 + Player.BoundingRadius, DamageType.Physical) != null;

        public static bool FishBones => Player.HasBuff("JinxQ");
        private static AIHeroClient Player => ObjectManager.Player;

        public static void Load()
        {
            Game.Print(
                "[<font color='#F8F46D'>ADC Package</font>] by <font color='#79BAEC'>God</font> - <font color='#FFFFFF'>Jinx</font> loaded");
            //Orbwalker.OnBeforeAttack += CustomOrbwalker_BeforeAttack;
            Game.OnUpdate += PermaActive;
            AIBaseClient.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;

            InitSpells();
            InitMenu();
        }
        
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Menu.Config.GetValue<MenuBool>("draw.w").Enabled) return;
            switch (W.IsReady())
            {
                case true:
                    CircleRender.Draw(Player.Position, W.Range, Color.Sienna);
                    break;
                case false:
                    CircleRender.Draw(Player.Position, W.Range, Color.Maroon);
                    break;
            }
        }
        
        private static void PermaActive(EventArgs args)
        {
            if (E.IsReady() || W.IsReady())
            {
                foreach (
                    var enemy in
                        GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(W.Range))
                            .OrderBy(TargetSelector.GetPriority))
                {
                    if (E.IsReady() && E.IsInRange(enemy)) /* e Logic */
                    {
                        if (Menu.Config.GetValue<MenuBool>("cc.e").Enabled)
                        {
                            if (enemy.HasBuff("RocketGrab") || enemy.HasBuff("rocketgrab2"))
                            {
                                var blitzcrank = GameObjects.AllyHeroes.FirstOrDefault(a => a.CharacterName == "Blitzcrank");

                                if (blitzcrank != null)
                                {
                                    E.Cast(blitzcrank.Position.Extend(enemy.Position, 30));
                                }
                                return;
                            }

                            if (CantMove(enemy) && !enemy.HasBuff("RocketGrab") && !enemy.HasBuff("rocketgrab2"))
                            {
                                E.Cast(enemy.Position);
                                return;
                            }

                            var pred = E.GetPrediction(enemy);
                            if (pred.Hitchance >= HitChance.Dash)
                            {
                                E.Cast(pred.CastPosition);
                                return;
                            }
                        }

                        if (Menu.Config.GetValue<MenuBool>("tp.e").Enabled)
                        {
                            CastWithExtraTrapLogic(E);
                            return;
                        }
                    }

                    if (W.IsReady())
                    {
                        if (Menu.Config.GetValue<MenuBool>("ks.w").Enabled)
                        {
                            if (enemy.Health <= W.GetDamage(enemy))
                            {
                                W.Cast(enemy);
                                return;
                            }
                        }

                        if (enemy.HasBuff("RocketGrab") || enemy.HasBuff("rocketgrab2"))
                        {
                            var blitzcrank = GameObjects.AllyHeroes.FirstOrDefault(a => a.CharacterName == "Blitzcrank");

                            if (blitzcrank != null)
                            {
                                DelayAction.Add(250, () => W.Cast(enemy));
                            }
                            return;
                        }


                        if (Menu.Config.GetValue<MenuBool>("cc.w").Enabled)
                        {
                            if (CantMove(enemy))
                            {
                                W.Cast(enemy);
                                return;
                            }
                        }
                    }
                }
            }
            //if (W.IsReady())
            //{
            //    AutoW();
            //}

            if (Menu.Config.GetValue<MenuBool>("ks.r").Enabled)
            {
                foreach (
                    var enemy in
                        GameObjects.EnemyHeroes.Where(e => e.Distance(Player) <= 2500)
                            .Where(enemy => R.IsReady() && enemy.IsValidTarget(R.Range)))
                {
                    var pred = R.GetPrediction(enemy);

                    if (W.IsReady() && W.GetDamage(enemy) >= enemy.Health && W.IsInRange(enemy) &&
                        W.GetPrediction(enemy).Hitchance >= HitChance.VeryHigh)
                    {
                        return;
                    }

                    if (GetRDamage(enemy) > enemy.Health && !HitsEnemyInTravel(enemy))
                    {
                        R.Cast(pred.CastPosition);
                        return;
                    }
                }
            }

            // fix knockup/knockback/stun auto e/w stuff
        }
        
        public static void Combo()
        {
            QHandler();

            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (target == null)
            {
                return;
            }

            
            if (Menu.Config.GetValue<MenuSlider>("w.range").Value != -1 &&
                W.IsReady() &&
                !Player.Spellbook.IsAutoAttack && target.Distance(Player) >
                Menu.Config.GetValue<MenuSlider>("w.range").Value &&
                W.GetPrediction(target).Hitchance >= HitChance.VeryHigh)
            {
                W.Cast(target);
            }

            if (E.IsReady())
            {
                if (Menu.Config.GetValue<MenuBool>("e.slowed").Enabled)
                {
                    if (E.IsInRange(target) && target.HasBuffOfType(BuffType.Slow))
                    {
                        E.Cast(target);
                        return;
                    }
                }

                if (Menu.Config.GetValue<MenuBool>("e.imlow").Enabled)
                {
                    if (E.IsInRange(target) && Player.HealthPercent <= 40)
                    {
                        E.Cast(target);
                        return;
                    }
                }

                if (Menu.Config.GetValue<MenuBool>("e.moreally").Enabled)
                {
                    if (E.IsInRange(target) && Player.CountAllyHeroesInRange(1000) > Player.CountEnemyHeroesInRange(1500))
                    {
                        E.Cast(target);
                        return;
                    }
                }

                if (Menu.Config.GetValue<MenuSlider>("e.aoe").Value != 6)
                {
                    if (target.CountEnemyHeroesInRange(300) >=
                        Menu.Config
                            .GetValue<MenuSlider>("e.aoe")
                            .Value)
                    {
                        E.Cast(target, true);
                    }
                }
            }

            if (R.IsReady())
            {
                if (Menu.Config
                    .GetValue<MenuBool>("r.aoe").Enabled)
                {
                    if (target.HealthPercent < 75)
                    {
                        R.CastIfWillHit(target, 3);
                    }
                }
            }
        }
        
        private static void QHandler()
        {
            if (Player.Spellbook.IsAutoAttack)
            {
                return;
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.Harass && Orbwalker.GetTarget() != null &&
                Orbwalker.GetTarget().Type == GameObjectType.AIMinionClient)
            {
                if (Q.IsReady() && FishBones)
                {
                    Q.Cast();
                }

                return;
            }
            

            if (Menu.Config.GetValue<MenuBool>("q.range").Enabled &&
                Orbwalker.ActiveMode == OrbwalkerMode.Combo ||
                Menu.Config.GetValue<MenuBool>("q.range").Enabled &&
                Orbwalker.ActiveMode == OrbwalkerMode.Harass)
            {
                if (!EnemyInRange && !FishBones && Q.IsReady()) // go rocket
                {
                    if (Orbwalker.ActiveMode == OrbwalkerMode.Harass &&
                        Orbwalker.GetTarget() == null)
                    {
                        return;
                    }

                    Q.Cast();
                }
            }
            

            if (Menu.Config.GetValue<MenuSlider>("q.aoe").Value != 6 &&
                Orbwalker.ActiveMode == OrbwalkerMode.Combo ||
                Menu.Config.GetValue<MenuSlider>("q.aoe").Value != 6 &&
                Orbwalker.ActiveMode == OrbwalkerMode.Harass)
            {
                foreach (
                    var enemy in
                        GameObjects.EnemyHeroes.Where(
                            a => a.IsValidTarget(525 + (50 + ((Q.Level)*25) + Player.BoundingRadius + 50)))
                            .OrderBy(TargetSelector.GetPriority)
                            .Where(
                                enemy =>
                                    enemy.CountEnemyHeroesInRange(150) >= (Menu.Config
                                        .GetValue<MenuSlider>("q.aoe")
                                        .Value)))
                {
                    if (!FishBones)
                    {
                        Q.Cast();
                    }
                    Orbwalker.ForceTarget = enemy;
                    return;
                }
            }

            var qtarget = TargetSelector.GetTarget(525 + (50 + ((Q.Level)*25) + Player.BoundingRadius + 50),
                DamageType.Physical);
            var target = TargetSelector.GetTarget(525 + Player.BoundingRadius, DamageType.Physical);

            if (Menu.Config.GetValue<MenuBool>("q.range").Enabled &&
                Orbwalker.ActiveMode == OrbwalkerMode.Combo ||
                Menu.Config.GetValue<MenuBool>("q.range").Enabled &&
                Orbwalker.ActiveMode == OrbwalkerMode.Harass)
            {
                if (qtarget != null && target != null && qtarget != target && !FishBones && Q.IsReady())
                    // maybe just switch to if qtarget isnt in range..? idk
                {
                    Q.Cast();
                }
                else
                {
                    if (target != null && qtarget == target && FishBones && Q.IsReady())
                    {
                        Q.Cast();
                    }
                }
            }
        }
        
        internal static CastStates CastWithExtraTrapLogic(this Spell spell)
        {
            if (spell.IsReady())
            {
                var teleport = MinionManager.GetMinions(spell.Range).FirstOrDefault(x => x.HasBuff("teleport_target"));
                var zhonya =
                    GameObjects.EnemyHeroes.FirstOrDefault(
                        x => ObjectManager.Player.Distance(x) <= spell.Range && x.HasBuff("zhonyasringshield"));

                if (teleport != null)
                    return spell.Cast(teleport);

                if (zhonya != null)
                    return spell.Cast(zhonya);
            }
            return CastStates.NotCasted;
        }
        
        private static bool HitsEnemyInTravel(AIBaseClient source)
        {
            var pred = R.GetPrediction(source);
            var collision =
                Collisions.GetCollision(new List<Vector3> {pred.UnitPosition},
                        new PredictionInput
                        {
                            Unit = ObjectManager.Player,
                            Delay = R.Delay,
                            Speed = R.Speed,
                            Radius = R.Width,
                            CollisionObjects = new[] {CollisionObjects.Heroes}
                        })
                    .Any(x => x.NetworkId != source.NetworkId);
            return (collision);
        }
        
        private static double GetRDamage(AIBaseClient target)
        {
            return ObjectManager.Player.CalculateDamage(target, DamageType.Physical,
                new double[] {0, 25, 40, 55}[R.Level]/100*(target.MaxHealth - target.Health) +
                ((new double[] {0, 20, 32, 44}[R.Level] + 0.1*ObjectManager.Player.FlatPhysicalDamageMod)*
                 Math.Min((1 + ObjectManager.Player.Distance(target.Position)/15*0.09d), 10)));
        }
        
        private static bool CantMove(AIHeroClient target)
        {
            return (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) ||
                    target.HasBuffOfType(BuffType.Knockup) ||
                    target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) ||
                    target.HasBuffOfType(BuffType.Knockback) ||
                    target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Suppression) ||
                    target.IsStunned || target.IsCastingImporantSpell() || target.MoveSpeed <= 50f);
        }
        
        public static bool ShouldUseE(string spellName)
        {
            switch (spellName)
            {
                case "ThreshQ":
                    return true;
                case "KatarinaR":
                    return true;
                case "AlZaharNetherGrasp":
                    return true;
                case "GalioIdolOfDurand":
                    return true;
                case "LuxMaliceCannon":
                    return true;
                case "MissFortuneBulletTime":
                    return true;
                case "RocketGrabMissile":
                    return true;
                case "CaitlynPiltoverPeacemaker":
                    return true;
                case "EzrealTrueshotBarrage":
                    return true;
                case "InfiniteDuress":
                    return true;
                case "VelkozR":
                    return true;
            }
            return false;
        }
        
        public static void Harass()
        {
            QHandler();

            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (target == null)
            {
                return;
            }
            
            if (Menu.Config.GetValue<MenuSlider>("w.range").Value != -1 &&
                W.IsReady() &&
                !Player.Spellbook.IsAutoAttack && target.Distance(Player) >
                Menu.Config.GetValue<MenuSlider>("w.range").Value &&
                W.GetPrediction(target).Hitchance >= HitChance.VeryHigh)
            {
                W.Cast(target);
            }
        }
        
        public static void LaneClear()
        {
            if (!Player.Spellbook.IsAutoAttack && FishBones && Q.IsReady())
            {
                Q.Cast();
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (E.IsReady() && ShouldUseE(args.SData.Name) && sender.IsValidTarget(E.Range))
            {
                E.Cast(sender.Position);
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (sender.IsValidTarget(E.Range) && E.IsReady())
            {
                if (Menu.Config.GetValue<MenuBool>("agc.e").Enabled)
                {
                    E.Cast(args.EndPosition);
                }
            }

            if (sender.IsValidTarget(W.Range) && W.IsReady())
            {
                if (Menu.Config.GetValue<MenuBool>("agc.w").Enabled)
                {
                    W.Cast(sender);
                }
            }
        }

        private static void InitSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1490);
            E = new Spell(SpellSlot.E, 900);
            R = new Spell(SpellSlot.R, 2500);

            W.SetSkillshot(0.6f, 75f, 3300f, true, SpellType.Line);
            E.SetSkillshot(1.2f, 1f, 1750f, false, SpellType.Line);
            R.SetSkillshot(0.7f, 140f, 1500f, false, SpellType.Line);
        }
        
        private static void InitMenu()
        {
            Menu.Config.Add(new MenuSeparator("adcpackage.jinx","Jinx"));


            var comboMenu =
                Menu.Config.Add(new EnsoulSharp.SDK.MenuUI.Menu("combo","Combo Menu"));
            {
                comboMenu.SetFontColor(SharpDX.Color.MediumVioletRed);
            }

            comboMenu.Add(new MenuSeparator("q.settings", "Q Settings"))
                .SetFontColor(SharpDX.Color.Yellow);
            comboMenu.Add(new MenuSeparator("q.when", "Use Q when:"));
            comboMenu.Add(new MenuBool("q.range", "   out of range").SetValue(true));
            comboMenu.Add(new MenuSlider("q.aoe", "   AOE if X targets hit",3, 2, 6))
                .SetTooltip("Set to 6 to disable");


            comboMenu.Add(new MenuSeparator("w.settings", "W Settings"))
                .SetFontColor(SharpDX.Color.Yellow);
            comboMenu.Add(new MenuSeparator("w.when", "Use W when:"));
            comboMenu.Add(new MenuSlider("w.range", "   target > X range",525, -1, 1000))
                .SetTooltip("Set to -1 to disable.");


            comboMenu.Add(new MenuSeparator("e.settings", "E Settings"))
                .SetFontColor(SharpDX.Color.Yellow);
            comboMenu.Add(new MenuSeparator("e.when", "Use E when:"));
            comboMenu.Add(new MenuBool("e.slowed", "   target slowed")).SetValue(true);
            comboMenu.Add(new MenuBool("e.imlow", "   im low")).SetValue(true);
            comboMenu.Add(new MenuBool("e.moreally", "   ally count > enemy")).SetValue(true);
            comboMenu.Add(new MenuSlider("e.aoe", "   enemy count >=",3, 2, 6))
                .SetTooltip("Set to 6 to disable.");
            
            
            comboMenu.Add(new MenuSeparator("r.settings", "R Settings"))
                .SetFontColor(SharpDX.Color.Yellow);
            comboMenu.Add(new MenuSeparator("r.when", "Use R when:"));
            comboMenu.Add(new MenuBool("r.killable", "   target killable")).SetValue(true);
            comboMenu.Add(new MenuBool("r.aoe", "   AOE")).SetValue(true);


            //
            //  harass
            //

            var harassMenu =
                Menu.Config.Add(new EnsoulSharp.SDK.MenuUI.Menu("harass","Harass Menu"));
            {
                harassMenu.SetFontColor(SharpDX.Color.MediumVioletRed);
            }

            harassMenu.Add(new MenuSeparator("q.settings", "Q Settings"))
                .SetFontColor(SharpDX.Color.Yellow);
            harassMenu.Add(new MenuSeparator("q.when", "Use Q when:"));
            harassMenu.Add(new MenuBool("q.range", "   out of range").SetValue(true));
            harassMenu.Add(new MenuSlider("q.aoe", "   AOE if X targets hit",3, 2, 6))
                .SetTooltip("Set to 6 to disable");

            //  w settings
            harassMenu.Add(new MenuSeparator("w.settings", "W Settings"))
                .SetFontColor(SharpDX.Color.Yellow);
            harassMenu.Add(new MenuSeparator("w.when", "Use W when:"));
            harassMenu.Add(new MenuSlider("w.range", "   target > X range",525, -1, 1000))
                .SetTooltip("Set to -1 to disable.");

            //
            //  extras
            //
            var extrasMenu =
                Menu.Config.Add(new EnsoulSharp.SDK.MenuUI.Menu("extras","Extras Menu"));
            {
                extrasMenu.SetFontColor(SharpDX.Color.Aquamarine);
            }
            extrasMenu.Add(new MenuSeparator("extras.ks", "Killsteal settings")).SetFontColor(
                SharpDX.Color.Yellow);
            extrasMenu.Add(new MenuBool("ks.w", "Use W to KS")).SetValue(true);
            extrasMenu.Add(new MenuBool("ks.r", "Use R to KS")).SetValue(true);
            extrasMenu.Add(new MenuSeparator("extras.cc", "Auto spell on:")).SetFontColor(
                SharpDX.Color.Yellow);
            extrasMenu.Add(new MenuBool("cc.w", "Use W on CC'd")).SetValue(true);
            extrasMenu.Add(new MenuBool("cc.e", "Use E on CC'd")).SetValue(true);
            extrasMenu.Add(new MenuBool("tp.e", "Use E on TP")).SetValue(true);

            extrasMenu.Add(new MenuSeparator("extras.antigc", "Anti-Gapcloser settings")).SetFontColor(
                SharpDX.Color.Yellow);
            extrasMenu.Add(new MenuBool("agc.w", "Use W on gapclose")).SetValue(false);
            extrasMenu.Add(new MenuBool("agc.e", "Use E on gapclose")).SetValue(true);

            //
            //  DRAWINGS
            //

            var drawingsMenu =
                Menu.Config.Add(new EnsoulSharp.SDK.MenuUI.Menu("drawings","Drawings Menu"));
            {
                drawingsMenu.SetFontColor(Color.Aquamarine);
            }
            drawingsMenu.Add(new MenuBool("draw.w", "Draw W range"))
                .SetValue(true);
        }
    }
}