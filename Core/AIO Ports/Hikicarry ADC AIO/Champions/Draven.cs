using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using HikiCarry.Core.Predictions;
using SharpDX;
using SPrediction;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using Render = EnsoulSharp.SDK.Render;
using Utilities = HikiCarry.Core.Utilities.Utilities;

namespace HikiCarry.Champions
{
    class Draven
    {
        internal static Spell Q;
        internal static Spell W;
        internal static Spell E;
        internal static Spell R;

        public static List<Axe> AxeSpots = new List<Axe>();
        public static List<string> AxesList = new List<string>
        {
            "Draven_reticle.troy" ,
            "Draven_reticle.troy" ,
            "Draven_reticle.troy"
        };

        public static List<string> BuffList = new List<string>
        {
            "Draven_Base_Q_buf.troy",
            "Draven_Skin01_Q_buf.troy",
            "Draven_Skin02_Q_buf.troy",
            "Draven_Skin03_Q_buf.troy"
        };

        private static int CurrentAxes { get; set; }
        private static int LastQTime { get; set; }

        public Draven()
        {
            Q = new Spell(SpellSlot.Q, ObjectManager.Player.AttackRange);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 950);
            R = new Spell(SpellSlot.R, 3000f);

            E.SetSkillshot(0.25f, 100, 1400, false, SpellType.Line);
            R.SetSkillshot(0.4f, 160, 2000, false, SpellType.Line);

            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.Add(new MenuBool("draven.q.combo", "Use Q",true).SetValue(true));
                comboMenu.Add(
                    new MenuSlider("draven.q.combo.axe.count", "Min. Axe Count",2, 1, 2));
                comboMenu.Add(new MenuBool("draven.e.combo", "Use E",true).SetValue(true));
                comboMenu.Add(new MenuBool("draven.r.combo", "Use R",true).SetValue(true));
                Initializer.Config.Add(comboMenu);
            }

            var harassmenu = new Menu("Harass Settings","Harass Settings");
            {
                harassmenu.Add(new MenuBool("draven.q.harass", "Use Q", true).SetValue(true));
                harassmenu.Add(
                    new MenuSlider("draven.q.harass.axe.count", "Min. Axe Count", 2, 1, 2));
                harassmenu.Add(new MenuBool("draven.e.harass", "Use E", true).SetValue(true));
                harassmenu.Add(
                    new MenuSlider("draven.harass.mana", "Min. Harass Mana", 60, 1, 99));
                Initializer.Config.Add(harassmenu);
            }

            var clearMenu = new Menu("Clear Settings", "Clear Settings");
            {
                clearMenu.Add(new MenuBool("draven.q.clear", "Use Q",true).SetValue(true));
                clearMenu.Add(
                    new MenuSlider("draven.q.lane.clear.axe.count", "Min. Axe Count",1, 1, 2));
                clearMenu.Add(
                    new MenuSlider("draven.q.minion.count", "(Q) Min. Minion Count",4, 1, 10));
                clearMenu.Add(new MenuSlider("draven.clear.mana", "Min. Mana",50, 1, 99));
                Initializer.Config.Add(clearMenu);
            }

            var jungleMenu = new Menu("Jungle Settings", "Jungle Settings");
            {
                jungleMenu.Add(new MenuBool("draven.q.jungle", "Use Q",true).SetValue(true));
                jungleMenu.Add(
                    new MenuSlider("draven.q.jungle.clear.axe.count", "Min. Axe Count",1, 1, 2));
                jungleMenu.Add(new MenuSlider("draven.jungle.mana", "Min. Mana",50, 1, 99));
                Initializer.Config.Add(jungleMenu);
            }
            var drawMenu = new Menu("Draw Settings", "Draw Settings");
            {
                var drawDamageMenu = new MenuBool("RushDrawEDamage", "Combo Damage").SetValue(true);
                var drawFill = new MenuColor("RushDrawEDamageFill", "Combo Damage Fill",Color.Gold);

                var damageDraws = drawMenu.Add(new Menu("Damage Draws", "Damage Draws"));
                damageDraws.Add(drawDamageMenu);
                damageDraws.Add(drawFill);
                
                Initializer.Config.Add(drawMenu);
            }

            Initializer.Config.Add(
                    new MenuBool("draven.axe.underturret", "Dont Catch Axe Under Turret", true).SetValue(true));
            Initializer.Config.Add(
                new MenuBool("draven.axe.in.enemies", "Dont Catch Axe In Enemies", true).SetValue(true));
            Initializer.Config.Add(new MenuBool("catch.axes", "Auto Catch Axes ?", true).SetValue(false));
            Initializer.Config.Add(new MenuBool("draw.catch.modes", "Draw Catch Sector/Circle", true).SetValue(false));
            Initializer.Config.Add(new MenuBool("draw.axe.positions", "Draw Axe Positions & Duration", true).SetValue(false));

            Initializer.Config.Add(
                new MenuList("catch.logic", "Axe Catch Mode", new[] { "Sector", "Circle" }));
            Initializer.Config.Add(
                new MenuSlider("catch.radius", "Axe Catch Radius", 600, 1, 1500));
            Initializer.Config["catch.radius"].SetFontColor(Color.Yellow);

            Initializer.Config.Add(
                new MenuSeparator("info.draven", "                                 Prediction Settings"));

            AIBaseClient.OnDoCast += DravenOnProcess;
            GameObject.OnCreate += DravenOnCreate;
            GameObject.OnDelete += DravenOnDelete;
            Drawing.OnDraw += DravenOnDraw;
            Game.OnUpdate += DravenOnUpdate;
        }

        private float TotalDamage(AIHeroClient hero)
        {
            var damage = 0d;
            if (Q.IsReady())
            {
                damage += Q.GetDamage(hero);
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

        private void DravenOnProcess(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }
            if (args.SData.Name == "dravenspinning")
            {
                LastQTime = Environment.TickCount;
            }
        }

        private void DravenOnDraw(EventArgs args)
        {
            if (Utilities.Enabled("draw.catch.modes"))
            {
                switch (Initializer.Config["catch.logic"].GetValue<MenuList>().Index)
                {
                    case 0: //Sector
                        var sectorpoly = new LeagueSharpCommon.Geometry.Geometry.Polygon.Sector(
                                ObjectManager.Player.Position.ToVector2(),
                                Game.CursorPos.ToVector2(),
                                100 * (float)Math.PI / 180,
                                Utilities.Slider("catch.radius"));
                        sectorpoly.Draw(Color.Gold.ToSystemColor());
                        break;
                    case 1: // Circle
                        var circlepoly = new LeagueSharpCommon.Geometry.Geometry.Polygon.Circle(ObjectManager.Player.Position.Extend(Game.CursorPos, Utilities.Slider("catch.radius")), 
                            Utilities.Slider("catch.radius"),100);
                        circlepoly.Draw(Color.Gold.ToSystemColor());
                        break;
                }
            }

            if (Utilities.Enabled("draw.axe.positions"))
            {
                foreach (var axe in AxeSpots)
                {
                    if (CatchableAxes(axe))
                    {
                        Render.Circle.DrawCircle(axe.Object.Position, 100, Color.GreenYellow.ToSystemColor());
                        Drawing.DrawText(Drawing.WorldToScreen(axe.Object.Position).X - 40, Drawing.WorldToScreen(axe.Object.Position).Y,
                            Color.Gold.ToSystemColor(), (((float)(axe.EndTick - Environment.TickCount))) + " ms");
                    }
                }
            }
        }

        private void DravenOnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    OnCombo();
                    break;
                case OrbwalkerMode.Harass:
                    OnHarass();
                    break;
                case OrbwalkerMode.LaneClear:
                    OnClear();
                    OnJungle();
                    break;
            }

            CatchAxe();
        }

        private void OnHarass()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("harass.mana"))
            {
                return;
            }
            var target = TargetSelector.GetTarget(3500f, DamageType.Physical);
            if (target != null)
            {
                if (Q.IsReady() && LastQTime + 100 < Environment.TickCount && target.IsValidTarget(Q.Range)
                    && Utilities.Enabled("draven.q.harass") &&
                    CurrentAxes < Utilities.Slider("draven.q.harass.axe.count"))
                {
                    Q.Cast();
                }

                if (E.IsReady() && Utilities.Enabled("draven.e.harass") && target.IsValidTarget(E.Range))
                {
                    E.Do(target, Utilities.HikiChance("hitchance"));
                }

            }
        }

        public static void CatchAxe()
        {
            if (Utilities.Enabled("catch.axes"))
            {
                if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                {
                    var axe = AxeSpots.OrderBy(ax3 => ax3.EndTick).FirstOrDefault(CatchableAxes);
                    if (axe != null)
                    {
                        Orbwalker.SetOrbwalkerPosition(axe.Object.Position);
                        DelayAction.Add(500, () => Orbwalker.SetOrbwalkerPosition(Game.CursorPos));
                    }
                    else
                    {
                        Orbwalker.SetOrbwalkerPosition(Game.CursorPos);
                    }
                }
                if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
                {
                    var axe = AxeSpots.OrderBy(ax3 => ax3.EndTick).FirstOrDefault(CatchableAxes);
                    if (axe != null)
                    {
                        Orbwalker.SetOrbwalkerPosition(axe.Object.Position);
                        DelayAction.Add(500, () => Orbwalker.SetOrbwalkerPosition(Game.CursorPos));
                    }
                    else
                    {
                        Orbwalker.SetOrbwalkerPosition(Game.CursorPos);
                    }
                }
            }
            

        }

        private void OnCombo()
        {
            var target = TargetSelector.GetTarget(3500f, DamageType.Physical);
            if (target != null)
            {
                if (Q.IsReady() && LastQTime + 100 < Environment.TickCount && target.IsValidTarget(Q.Range)
                && Utilities.Enabled("draven.q.combo") && CurrentAxes < Utilities.Slider("draven.q.combo.axe.count"))
                {
                    Q.Cast();
                }

                if (E.IsReady() && Utilities.Enabled("draven.e.combo") && target.IsValidTarget(E.Range))
                {
                    E.Do(target, Utilities.HikiChance("hitchance"));
                }

                if (R.IsReady() && Utilities.Enabled("draven.r.combo") && target.IsValidTarget(R.Range)
                    && R.GetDamage(target) > target.Health)
                {
                    R.Do(target, Utilities.HikiChance("hitchance"));
                }
            }
        }
        private void OnClear()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("draven.clear.mana"))
            {
                return;
            }

            if (Q.IsReady() && LastQTime + 100 < Environment.TickCount
               && Utilities.Enabled("draven.q.clear") && CurrentAxes < Utilities.Slider("draven.q.lane.clear.axe.count"))
            {
                var minionlist = MinionManager.GetMinions(ObjectManager.Player.AttackRange);
                if (minionlist.Count() >= Utilities.Slider("draven.q.minion.count") && minionlist != null)
                {
                    Q.Cast();
                }
            }
        }
        
        private void OnJungle()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("draven.jungle.mana"))
            {
                return;
            }

            if (Q.IsReady() && LastQTime + 100 < Environment.TickCount
               && Utilities.Enabled("draven.q.jungle") && CurrentAxes < Utilities.Slider("draven.q.jungle.clear.axe.count"))
            {
                var target = MinionManager.GetMinions(ObjectManager.Player.AttackRange, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth)
                   .FirstOrDefault(x => x.IsValidTarget(Q.Range));

                if (target != null)
                {
                    Q.Cast();
                }
            }
        }

        private void DravenOnDelete(GameObject sender, EventArgs args)
        {
            for (var i = 0; i < AxeSpots.Count; i++)
            {
                if (AxeSpots[i].Object.NetworkId == sender.NetworkId)
                {
                    AxeSpots.RemoveAt(i);
                    return;
                }
            }

            if ((BuffList.Contains(sender.Name)) && 
                sender.Position.Distance(ObjectManager.Player.Position) < 300)
            {
                if (CurrentAxes == 0)
                {
                    CurrentAxes = 0;
                }

                if (CurrentAxes <= 2)
                {
                    CurrentAxes = CurrentAxes - 1;
                }
                else
                {
                    CurrentAxes = CurrentAxes - 1;
                }
            }
        }

        private void DravenOnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Draven_") && sender.Name.Contains("_Q_reticle_self") && sender.Position.Distance(ObjectManager.Player.Position) /
                ObjectManager.Player.MoveSpeed <= 2)
            {
                AxeSpots.Add(new Axe(sender));
            }
            if (BuffList.Contains(sender.Name) && sender.Position.Distance(ObjectManager.Player.Position) < 100)
            {
                CurrentAxes += 1;
            }
        }

        public static bool CatchableAxes(Axe axe)
        {
            switch (Initializer.Config["catch.logic"].GetValue<MenuList>().Index)
            {
                case 0:
                    var sectorpoly = new LeagueSharpCommon.Geometry.Geometry.Polygon.Sector(
                        ObjectManager.Player.Position.ToVector2(),
                        Game.CursorPos.ToVector2(),
                        100 * (float)Math.PI / 180,
                        600).IsInside(axe.Object.Position);
                    return sectorpoly;
                default:
                    var circlepoly = new LeagueSharpCommon.Geometry.Geometry.Polygon.Circle(Game.CursorPos, Utilities.Slider("catch.radius"))
                        .IsInside(axe.Object.Position);
                    return circlepoly;
            }
        }
    }

    class Axe
    {
        public Axe(GameObject obj)
        {
            Object = obj;
            EndTick = Environment.TickCount + 1500;
        }

        public int EndTick;
        public GameObject Object;
    }
}