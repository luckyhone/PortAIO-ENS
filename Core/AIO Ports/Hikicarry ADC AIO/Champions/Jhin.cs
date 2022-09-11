using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using HikiCarry.Core.Plugins;
using HikiCarry.Core.Predictions;
using HikiCarry.Core.Utilities;
using LeagueSharpCommon;
using SharpDX;
using SPrediction;
using Color = SharpDX.Color;
using KeyBindType = EnsoulSharp.SDK.MenuUI.KeyBindType;
using Keys = EnsoulSharp.SDK.MenuUI.Keys;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using Utilities = HikiCarry.Core.Utilities.Utilities;

 namespace HikiCarry.Champions
{
    internal class Jhin
    {
        internal static Spell Q;
        internal static Spell W;
        internal static Spell E;
        internal static Spell R;

        public Jhin()
        {
            Q = new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W, 2500);
            E = new Spell(SpellSlot.E, 2000);
            R = new Spell(SpellSlot.R, 3500);

            W.SetSkillshot(0.75f, 40, float.MaxValue, false, SpellType.Line);
            E.SetSkillshot(0.23f, 120, 1600, false, SpellType.Circle);
            R.SetSkillshot(0.21f, 80, 5000, false, SpellType.Line);

            var comboMenu = new Menu("Combo Settings", ":: Combo Settings");
            {

                comboMenu.Add(new MenuBool("q.combo", "Use (Q)", true).SetValue(true));

                comboMenu.Add(new MenuBool("w.combo", "Use (W)", true).SetValue(true));
                comboMenu.Add(
                    new MenuSlider("w.combo.min.distance", "Min. Distance", 400, 1, 2500));
                comboMenu.Add(
                    new MenuSlider("w.combo.max.distance", "Max. Distance", 1000, 1, 2500));
                comboMenu.Add(new MenuBool("w.passive.combo", "Use (W) If Enemy Is Marked", true).SetValue(true));
                comboMenu.Add(new MenuBool("e.combo", "Use (E)", true).SetValue(true));
                comboMenu.Add(new MenuBool("e.to.dash.end", "Use (E) to Dash End", true).SetValue(true));
                Initializer.Config.Add(comboMenu);
            }

            var harassMenu = new Menu("Harass Settings", ":: Harass Settings");
            {
                harassMenu.Add(new MenuBool("q.harass", "Use (Q)", true).SetValue(true));
                harassMenu.Add(new MenuBool("w.harass", "Use (W)",true).SetValue(true));
                harassMenu.Add(
                    new MenuSlider("harass.mana", "Min. Mana Percentage",50, 1, 99));
                Initializer.Config.Add(harassMenu);
            }

            var clearMenu = new Menu("Clear Settings", ":: Clear Settings");
            {
                var laneclearMenu = new Menu("Wave Clear", ":: Wave Clear");
                {
                    laneclearMenu.Add(
                        new MenuSeparator("keysinfo1", "                  (Q) Settings").SetTooltip("Q Settings"));
                    laneclearMenu.Add(new MenuBool("q.clear", "Use (Q)", true).SetValue(true));
                    laneclearMenu.Add(
                        new MenuSeparator("keysinfo2", "                  (W) Settings").SetTooltip("W Settings"));
                    laneclearMenu.Add(new MenuBool("w.clear", "Use (W)", true).SetValue(true));
                    laneclearMenu.Add(new MenuSlider("w.hit.x.minion", "Min. Minion", 4, 1, 5));
                    clearMenu.Add(laneclearMenu);
                }


                var jungleClear = new Menu("Jungle Clear", ":: Jungle Clear");
                {
                    jungleClear.Add(
                            new MenuSeparator("keysinfo1X", "                  (Q) Settings").SetTooltip("Q Settings"));
                    jungleClear.Add(new MenuBool("q.jungle", "Use (Q)", true).SetValue(true));
                    jungleClear.Add(
                        new MenuSeparator("keysinfo2X", "                  (W) Settings").SetTooltip("W Settings"));
                    jungleClear.Add(new MenuBool("w.jungle", "Use (W)", true).SetValue(true));
                    clearMenu.Add(jungleClear);
                }

                clearMenu.Add(
                        new MenuSlider("clear.mana", "LaneClear Min. Mana Percentage", 50, 1, 99));
                clearMenu.Add(
                    new MenuSlider("jungle.mana", "Jungle Min. Mana Percentage", 50, 1, 99));
                Initializer.Config.Add(clearMenu);

            }

            var ksMenu = new Menu("Kill Steal", ":: Kill Steal");
            {
                ksMenu.Add(new MenuBool("q.ks", "Use (Q)", true).SetValue(true));
                ksMenu.Add(new MenuBool("w.ks", "Use (W)", true).SetValue(true));
                Initializer.Config.Add(ksMenu);
            }

            var miscMenu = new Menu("Miscellaneous", ":: Miscellaneous");
            {
                miscMenu.Add(new MenuBool("auto.e.immobile", "Auto Cast (E) Immobile Target", true).SetValue(true));
                Initializer.Config.Add(miscMenu);
            }



            var rComboMenu = new Menu("Ultimate Settings", ":: Ultimate Settings");
            {
                var rComboWhiteMenu = new Menu(":: R - Whitelist", ":: R - Whitelist");
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValid))
                    {
                        rComboWhiteMenu.Add(
                            new MenuBool("r.combo." + enemy.CharacterName, "(R): " + enemy.CharacterName, true).SetValue(
                                true));
                    }
                    rComboMenu.Add(rComboWhiteMenu);
                }
                rComboMenu.Add(new MenuBool("r.combo", "Use (R)", true).SetValue(true));
                rComboMenu.Add(
                    new MenuBool("auto.shoot.bullets", "If Jhin Casting (R) Auto Cast Bullets", true).SetValue(true));
                Initializer.Config.Add(rComboMenu);
            }
            rComboMenu.SetFontColor(SharpDX.Color.Yellow);

            Initializer.Config.Add(
                    new MenuKeyBind("semi.manual.ult", "Semi-Manual (R)!", Keys.A,
                        KeyBindType.Press));
            Initializer.Config.Add(new MenuKeyBind("use.combo", "Combo (Active)", Keys.Space, KeyBindType.Press));

            var drawMenu = new Menu("Draw Settings", "Draw Settings");
            {
                var drawDamageMenu = new MenuBool("RushDrawEDamage", "Combo Damage").SetValue(true);
                var drawFill = new MenuColor("RushDrawEDamageFill", "Combo Damage Fill",Color.Gold);

                var damageDraws = drawMenu.Add(new Menu("Damage Draws", "Damage Draws"));
                damageDraws.Add(drawDamageMenu);
                damageDraws.Add(drawFill);
                
                Initializer.Config.Add(drawMenu);
            }

            Game.OnUpdate += JhinOnUpdate;
            AIBaseClient.OnNewPath += ObjAiHeroOnOnNewPath;

        }

        private void ObjAiHeroOnOnNewPath(AIBaseClient sender, AIBaseClientNewPathEventArgs args)
        {
            if (sender.IsEnemy && sender is AIHeroClient && args.IsDash && Utilities.Enabled("e.to.dash.end")
                && sender.IsValidTarget(E.Range) && E.IsReady())
            {
                var starttick = Utils.TickCount;
                var speed = args.Speed;
                var startpos = sender.ServerPosition.ToVector2();
                var forch = args.Path.OrderBy(x => starttick + (int)(1000 * (new Vector3(x.X, x.Y, x.Z).
                    Distance(startpos.ToVector3()) / speed))).FirstOrDefault();
                {
                    var endpos = new Vector3(forch.X, forch.Y, forch.Z);
                    var endtick = starttick + (int)(1000 * (endpos.Distance(startpos.ToVector3())
                        / speed));
                    var duration = endtick - starttick;

                    if (duration < starttick)
                    {
                        E.Cast(endpos);
                    }
                }
            }
        }
        private static float TotalDamage(AIHeroClient hero)
        {
            var damage = 0d;
            if (Q.IsReady())
            {
                damage += Q.GetDamage(hero);
            }
            if (W.IsReady())
            {
                damage += W.GetDamage(hero);
            }
            if (E.IsReady())
            {
                damage += W.GetDamage(hero);
            }
            if (R.IsReady())
            {
                damage += R.GetDamage(hero);
            }
            return (float)damage;
        }
        private void JhinOnUpdate(EventArgs args)
        {
            #region Orbwalker & Modes 

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    OnCombo();
                    break;
                case OrbwalkerMode.LaneClear:
                    OnClear();
                    OnJungle();
                    break;
                case OrbwalkerMode.Harass:
                    OnHarass();
                    break;
                case OrbwalkerMode.None:
                    OnKillSteal();
                    break;
            }

            #endregion

            if (ObjectManager.Player.IsActive(R))
            {
                Orbwalker.AttackEnabled = (false);
                Orbwalker.MoveEnabled = (false);
            }
            else
            {
                Orbwalker.AttackEnabled = (true);
                Orbwalker.MoveEnabled = (true);
            }

            if (Initializer.Config["semi.manual.ult"].GetValue<MenuKeyBind>().Active &&
                R.IsReady() && Utilities.Enabled("r.combo") && !Utilities.Enabled("auto.shoot.bullets"))
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(R.Range) &&
                                                                          Initializer.Config["Ultimate Settings"][":: R - Whitelist"]["r.combo." + x.CharacterName].GetValue<MenuBool>().Enabled))
                {
                    R.Do(target, Utilities.HikiChance("hitchance"));
                }
            }

            if (ObjectManager.Player.IsActive(R) && Utilities.Enabled("auto.shoot.bullets") && R.IsReady())
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(R.Range) &&
                                                                      Initializer.Config["Ultimate Settings"][":: R - Whitelist"]["r.combo." + x.CharacterName].GetValue<MenuBool>().Enabled))
                {
                    R.Do(target, Utilities.HikiChance("hitchance"));
                }
            }
        }

        private void OnKillSteal()
        {
            if (Q.IsReady() && Utilities.Enabled("q.ks"))
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                if (target != null && target.IsValidTarget(Q.Range) && target.Health < Q.GetDamage(target))
                {
                    Q.CastOnUnit(target);
                }
            }
            if (W.IsReady() && Utilities.Enabled("w.ks"))
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                if (target != null && target.IsValidTarget(W.Range) && target.Health < W.GetDamage(target))
                {
                    W.Do(target,Utilities.HikiChance("hitchance"));
                }
            }
        }
        private void OnCombo()
        {
            if (Q.IsReady() && Utilities.Enabled("q.combo"))
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(x=> x.IsValidTarget(Q.Range)))
                {
                    Q.CastOnUnit(enemy);
                }
            }
            if (W.IsReady() && Utilities.Enabled("w.combo"))
            {
                if (Utilities.Enabled("w.passive.combo"))
                {
                    var target = TargetSelector.GetTarget(Utilities.Slider("w.combo.max.distance"), DamageType.Physical);
                    if (target != null && target.IsValidTarget(Utilities.Slider("w.combo.max.distance")) && 
                        target.HasBuff("jhinespotteddebuff") && target.Distance(ObjectManager.Player.Position) > Utilities.Slider("w.combo.min.distance"))
                    {
                        W.Do(target,Utilities.HikiChance("hitchance"));
                    }
                }
                else
                {
                    var target = TargetSelector.GetTarget(Utilities.Slider("w.combo.max.distance"), DamageType.Physical);
                    if (target != null && target.IsValidTarget(Utilities.Slider("w.combo.max.distance")) 
                        && target.Distance(ObjectManager.Player.Position) > Utilities.Slider("w.combo.min.distance"))
                    {
                        W.Do(target, Utilities.HikiChance("hitchance"));
                    }
                }
            }

            if (E.IsReady() && Utilities.Enabled("e.combo"))
            {
                var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
                if (target != null && target.IsValidTarget(E.Range) && Utilities.IsImmobile(target))
                {
                    E.Do(target,Utilities.HikiChance("hitchance"));
                }
            }

        }
        private void OnHarass()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("harass.mana"))
            {
                return;
            }
            if (Q.IsReady() && Utilities.Enabled("q.harass"))
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                if (target != null && target.IsValidTarget(Q.Range))
                {
                    Q.CastOnUnit(target);
                }
            }
            if (W.IsReady() && Utilities.Enabled("w.harass"))
            {
                var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
                if (target != null && target.IsValidTarget(W.Range))
                {
                    W.Do(target,Utilities.HikiChance("hitchance"));
                }
            }
        }
        private void OnClear()
        {
            if (ObjectManager.Player.ManaPercent < Initializer.Config["Clear Settings"]["clear.mana"].GetValue<MenuSlider>().Value)
            {
                return;
            }
            if (Q.IsReady() && Initializer.Config["Clear Settings"]["Wave Clear"]["q.clear"].GetValue<MenuBool>().Enabled)
            {
                var min = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range).MinOrDefault(x => x.Health);
                if (min != null)
                {
                    Q.CastOnUnit(min);
                }
            }

            if (W.IsReady() && Initializer.Config["Clear Settings"]["Wave Clear"]["w.clear"].GetValue<MenuBool>().Enabled)
            {
                var min = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range);
                if (min != null && W.GetLineFarmLocation(min).MinionsHit >= Initializer.Config["Clear Settings"]["Wave Clear"]["w.hit.x.minion"].GetValue<MenuSlider>().Value)
                {
                    W.Cast(W.GetLineFarmLocation(min).Position);
                }
            }
        }
        private void OnJungle()
        {
            if (ObjectManager.Player.ManaPercent < Initializer.Config["Clear Settings"]["jungle.mana"].GetValue<MenuSlider>().Value)
            {
                return;
            }
            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Jhin.Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth);

            if (mobs == null || (mobs.Count == 0))
            {
                return;
            }

            if (Q.IsReady() && Initializer.Config["Clear Settings"]["Jungle Clear"]["q.jungle"].GetValue<MenuBool>().Enabled)
            {
                Q.Cast(mobs[0]);
            }

            if (W.IsReady() && Initializer.Config["Clear Settings"]["Jungle Clear"]["w.jungle"].GetValue<MenuBool>().Enabled)
            {
                W.Cast(mobs[0]);
            }
        }

    }
}