using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
using SPrediction;
using Color = System.Drawing.Color;

namespace LordsCamille
{
    class Program
    {

        public static void Loads()
        {
            Game_OnGameUpdate(new EventArgs());
        }

        public static AIHeroClient p;

        private static string News = "Welcome to Lord's Camille";

        public static AIHeroClient Player = ObjectManager.Player;

        public static Spell Q, W, E, R;

        public static Menu CamilleMenu;

        internal static bool OnWall => Player.HasBuff("camilleedashtoggle") || E.Instance.Name != "CamilleE";

        internal static bool IsDashing => Player.HasBuff("camilleedash" + "1") || Player.HasBuff("camilleedash" + "2");
        
        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.CharacterName != "Camille")
            {
                return;
            }
            //Spells
            Q = new Spell(SpellSlot.Q, 135f);
            W = new Spell(SpellSlot.W, 625f);
            E = new Spell(SpellSlot.E, 1000f);
            R = new Spell(SpellSlot.R, 375f);

            E.SetSkillshot(0.3f, 30, 500, false, SpellType.Line);
            W.SetSkillshot(0.195f, 100, 1750, false, SpellType.Cone);

            Game.Print("<font size='30'>Lord's Camille</font> <font color='#b756c5'>by LordZEDith</font>");
            Game.Print("<font color='#b756c5'>NEWS: </font>" + Program.News);

            //Events
            MainMenu();
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_Update;
        }
        
        private static void Game_Update(EventArgs args)
        {
            //Activates Combo
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:

                    Combos();

                    break;


                //Activates Laneclear
                case OrbwalkerMode.LaneClear:

                    Laneclear();

                    break;


            }
        }
        
        public static void Combos()
        {
            //Target
            var Target = TargetSelector.GetTarget(E.Range, DamageType.Physical);

            //Q Combo
            if (CamilleMenu["Q"].GetValue<MenuBool>().Enabled && Q.IsReady() && Target.IsValidTarget() && CamilleMenu["Q2"].GetValue<MenuBool>().Enabled && !Player.HasBuff("CamilleQPrimingStart"))
            {
                Q.Cast(Target);
            }

            if (CamilleMenu["Q"].GetValue<MenuBool>().Enabled && Q.IsReady() && Target.IsValidTarget() && !CamilleMenu["Q2"].GetValue<MenuBool>().Enabled)
            {
                Q.Cast(Target);
            }

            //W Combo
            if (CamilleMenu["W"].GetValue<MenuBool>().Enabled && W.IsReady() && Target.IsValidTarget() && !CamilleMenu["W2"].GetValue<MenuBool>().Enabled && !Player.HasBuff("CamilleEOnWall") && !Player.HasBuff("CamilleEDash1") && !Player.HasBuff("camilleedashtoggle") && !Player.HasBuff("CamilleEDash2"))
            {
                W.Cast(Target);
            }
            if (CamilleMenu["W"].GetValue<MenuBool>().Enabled && W.IsReady() && Target.IsValidTarget() && CamilleMenu["W2"].GetValue<MenuBool>().Enabled && Player.Distance(Target) <= 630 && Player.Distance(Target) >= 350 && !Player.HasBuff("CamilleEOnWall") && !Player.HasBuff("camilleedashtoggle") && !Player.HasBuff("CamilleEDash2"))
            {
                W.Cast(Target);
            }

            //E Combo                
            if (E.IsReady() && CamilleMenu["E"].GetValue<MenuBool>().Enabled && Target.IsValidTarget(E.Range))
            {
                switch (CamilleMenu["EMode"].GetValue<MenuList>().Index)
                {
                    case 0:
                        {
                            var polygon = new LeagueSharpCommon.Geometry.Geometry.Polygon.Circle(Target.Position, 600).Points.FirstOrDefault(x => NavMesh.GetCollisionFlags(x.X, x.Y).HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(x.X, x.Y).HasFlag(CollisionFlags.Building));

                            if (new Vector2(polygon.X, polygon.Y).Distance(ObjectManager.Player.Position) < E.Range)
                            {
                                E.Cast(new Vector2(polygon.X, polygon.Y));
                                if (E.Cast(new Vector2(polygon.X, polygon.Y)))
                                {
                                    E.Cast(Target);
                                }
                            }
                        }
                        break;
                    case 1:
                        {
                            var polygon = new LeagueSharpCommon.Geometry.Geometry.Polygon.Circle(Target.Position, 600).Points.FirstOrDefault(x => NavMesh.GetCollisionFlags(x.X, x.Y).HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(x.X, x.Y).HasFlag(CollisionFlags.Building));

                            if (new Vector2(polygon.X, polygon.Y).Distance(ObjectManager.Player.Position) < E.Range)
                            {
                                E.Cast(new Vector2(polygon.X, polygon.Y));

                            }
                        }
                        break;
                    case 2:
                        {
                            var target = TargetSelector.GetTarget(E.IsReady() ? E.Range * 2 : W.Range, DamageType.Physical);
                            if (target.IsValidTarget() && !target.IsZombie())
                            {
                                if (CamilleMenu["E"].GetValue<MenuBool>().Enabled)
                                    UseE(target.ServerPosition);
                            }
                        }
                        break;
                }
            }

            //R Combo
            if (CamilleMenu["R"].GetValue<MenuBool>().Enabled && R.IsReady() && Target.IsValidTarget() && !Player.HasBuff("CamilleEOnWall") && !Player.HasBuff("CamilleEDash1") && !Player.HasBuff("camilleedashtoggle") && !Player.HasBuff("CamilleEDash2") && Target.HealthPercent < CamilleMenu["RHP"].GetValue<MenuSlider>().Value && CamilleMenu["r" + Target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled)
            {
                R.Cast(Target);
            }

        }

        public static void Laneclear()
        {

            if (Q.IsReady() && CamilleMenu["WLane"].GetValue<MenuBool>().Enabled)
            {
                var minions = MinionManager.GetMinions(ObjectManager.Player.Position, W.Range, MinionManager.MinionTypes.All,
                MinionManager.MinionTeam.NotAlly);

                var minioncount = W.GetLineFarmLocation(minions);
                if (minions == null || minions.Count == 0)
                {
                    return;
                }

                if (minioncount.MinionsHit >= CamilleMenu["Min"].GetValue<MenuSlider>().Value)
                {
                    W.Cast(minioncount.Position);
                }
            }
        }
        
        
        private static void Drawing_OnDraw(EventArgs args)
        {
            //Draw W
            if (CamilleMenu["Drawings"]["DrawW"].GetValue<MenuBool>().Enabled && W.IsReady())
            {
                LeagueSharpCommon.Render.Circle.DrawCircle(Player.Position, 630, Color.Black);
            }

            //Draw E
            if (CamilleMenu["Drawings"]["DrawE"].GetValue<MenuBool>().Enabled && E.IsReady() && !Player.HasBuff("CamilleEDash1") && !Player.HasBuff("CamilleEOnWall") && !Player.HasBuff("camilleedashtoggle"))
            {
                LeagueSharpCommon.Render.Circle.DrawCircle(Player.Position, 900, Color.BlueViolet);
            }

            if (CamilleMenu["Drawings"]["DrawR"].GetValue<MenuBool>().Enabled && R.IsReady())
            {
                LeagueSharpCommon.Render.Circle.DrawCircle(Player.Position, 475, Color.Red);
            }
        }

        public static void MainMenu()
        {
            CamilleMenu = new Menu("Lord's Camille", "Lord's Camille", true);
            CamilleMenu.SetFontColor(SharpDX.Color.SkyBlue);
            
            //Combo
            var Combo = new Menu("Combo", "Combo");
            {
                Combo.Add(new MenuSeparator("Combo Settings:", "Combo Settings:"));
                Combo.Add(new MenuBool("Q", "Use Q in Combo")).SetValue(true);
                Combo.Add(new MenuBool("Q2", "Use Q2 true DMG")).SetValue(true);
                Combo.Add(new MenuBool("W", "Use W in Combo")).SetValue(true);
                Combo.Add(new MenuBool("W2", "Use W Outer Range")).SetValue(true);
                Combo.Add(new MenuList("EMode", "Use E Mode:", new[] { "Hiki Edited", "Hiki" }));
                Combo.Add(new MenuBool("E", "Use E in Combo")).SetValue(true);
                Combo.Add(new MenuBool("R", "Use R in Combo")).SetValue(true);
                Combo.Add(new MenuSlider("RHP", "Use R if Enemy % HP",50));

            }

            CamilleMenu.Add(Combo);
            
            //Ultimate Menu
            var whitelist = new Menu("Ultimate Whitelist", "Ultimate Whitelist");
            {
                foreach (var hero in GameObjects.EnemyHeroes)
                {
                    whitelist.Add(new MenuBool("r" + hero.CharacterName.ToLower(), "Use [R]:  " + hero.CharacterName, true).SetValue(
                        true));
                }

            }
            CamilleMenu.Add(whitelist);
            //Clear
            var Clear = new Menu("Lane Clear", "Lane Clear");
            {
                Clear.Add(new MenuSeparator("Laneclear Settings:", "Laneclear Settings:"));
                //Clear.AddItem(new MenuItem("QLane", "Use Q in Laneclear")).SetValue(false);
                // Clear.AddItem(new MenuItem("QLane2", "Use Q2 in Laneclear")).SetValue(false);
                Clear.Add(new MenuBool("WLane", "Use W in Laneclear")).SetValue(true);
                Clear.Add(new MenuSlider("Min", "[W] Min. Minion Count",3, 1, 5));
            }
            CamilleMenu.Add(Clear);
            
            //DrawMenu
            var DrawMenu = new Menu("Drawings", "Drawings");
            {
                DrawMenu.Add(new MenuSeparator("Draw Settings:", "Draw Settings:"));
                DrawMenu.Add(new MenuBool("DrawW", "Draw W Range")).SetValue(true);
                DrawMenu.Add(new MenuBool("DrawE", "Draw E Range")).SetValue(true);
                DrawMenu.Add(new MenuBool("DrawR", "Draw R Range")).SetValue(true);

            }
            CamilleMenu.Add(DrawMenu);

            CamilleMenu.Attach();
        }
        
        static void UseE(Vector3 p, bool combo = true)
        {
            try
            {
                if (IsDashing || OnWall || !E.IsReady())
                {
                    return;
                }

                
                var posChecked = 0;
                var maxPosChecked = 80;
                var posRadius = 145;
                var radiusIndex = 0;

                var candidatePos = new List<Vector2>();

                while (posChecked < maxPosChecked)
                {
                    radiusIndex++;

                    var curRadius = radiusIndex * (0x2 * posRadius);
                    var curCurcleChecks = (int)Math.Ceiling((2 * Math.PI * curRadius) / (2 * (double)posRadius));

                    for (var i = 1; i < curCurcleChecks; i++)
                    {
                        posChecked++;

                        var cRadians = (0x2 * Math.PI / (curCurcleChecks - 1)) * i;
                        var xPos = (float)Math.Floor(p.X + curRadius * Math.Cos(cRadians));
                        var yPos = (float)Math.Floor(p.Y + curRadius * Math.Sin(cRadians));

                        var desiredPos = new Vector2(xPos, yPos);



                        if (desiredPos.IsWall())
                        {
                            candidatePos.Add(desiredPos);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}