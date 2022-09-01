using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using EnsoulSharp.SDK.Utility;
using PortAIO;
using SharpDX;
using SPrediction;

namespace BePlank
{
    class Program
    {
        #region Declaration
        static Spell Q, W, E, R;
        public static List<Barrel> savedBarrels = new List<Barrel>();
        static double mouseToClosestBarrel;
        public static double maxSearchRange = 900;
        public static int correctionRange = 1000;
        public static int potentielRange = 1000;
        public const int BarrelConnectionRange = 340;
        public const int UltRadius = 500;
        public const int DeathDaughterRadius = 200;
        static Vector3 blockPos;
        static Menu Menu;
        private static Vector2 PingLocation;
        static bool isEQ = false;
        private static int LastPingT = 0;
        public static bool ECasted;
        static AIHeroClient Player { get { return ObjectManager.Player; } }
        static string CHAMPION_NAME = "Gangplank";
        #endregion

        static void Game_OnGameLoad()
        {
            if (Player.CharacterName != CHAMPION_NAME)
                return;
            Game.Print("BePlank by Brikovich loaded - Credits to baballev & Soresu");

            #region Spells
            Q = new Spell(SpellSlot.Q, 610);
            Q.SetTargetted(0.25f, 2150f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 980);
            R = new Spell(SpellSlot.R);
            R.SetSkillshot(0.9f, 100, float.MaxValue, false, SpellType.Circle);
            #endregion

            #region Menu
            Menu = new Menu(Player.CharacterName, "BePlank", true);

            Menu DrawingMenu = Menu.Add(new Menu("Drawings", "Drawing"));
            DrawingMenu.Add(new MenuBool("DrawAA", "Draw AA Range").SetValue(true));
            DrawingMenu.Add(new MenuBool("DrawQ", "Draw Q Range").SetValue(true));
            DrawingMenu.Add(new MenuBool("DrawE", "Draw E Range").SetValue(true));
            DrawingMenu.Add(new MenuBool("DrawERadius", "Draw E Raduis").SetValue(true));
            DrawingMenu.Add(new MenuBool("DrawEConnection", "Draw E Connection Line").SetValue(true));
            DrawingMenu.Add(new MenuBool("DrawEConnectionRadius", "Draw E Connection Circles").SetValue(true));
            DrawingMenu.Add(new MenuBool("DrawEPrediction", "Draw E Predicted Range").SetValue(true));
            DrawingMenu.Add(new MenuBool("DrawR", "Draw R Radius").SetValue(false));
            DrawingMenu.Add(new MenuBool("OnReady", "Draw only if Ready").SetValue(true));


            Menu.Add(new MenuBool("Corrector", "Connection correction [BETA]").SetTooltip("If E connection miss will try to cast E on the lastest succesfull position"));
            Menu.Add(new MenuKeyBind("CastQ", "Quick Q detonate nearest (health decay support)", Keys.A, KeyBindType.Press, false));
            Menu.Add(new MenuKeyBind("CastEQ", "Quick cast EQ at mouse (first barrel manual)", Keys.T, KeyBindType.Press, false));

            Menu.Add(new MenuBool("Ping", "Ping on low hp (local)").SetValue(true));
            Menu.Add(new MenuBool("KS", "Q KillSecure").SetValue(true));
            Menu.Add(new MenuKeyBind("Qlasthit", "Q last hit toggle", Keys.K, KeyBindType.Toggle, false));

            var cleanserManagerMenu = new Menu("cleanserManager", "W cleanser - By baballev");
            cleanserManagerMenu.Add(new MenuBool("enabled", "Enabled").SetValue(true));
            cleanserManagerMenu.Add(new MenuSeparator("separation1", " "));
            cleanserManagerMenu.Add(new MenuSeparator("separation2", "Buff Types: "));
            cleanserManagerMenu.Add(new MenuBool("charm", "Charm").SetValue(true));
            cleanserManagerMenu.Add(new MenuBool("flee", "Flee").SetTooltip("Fear"));
            cleanserManagerMenu.Add(new MenuBool("polymorph", "Polymorph").SetValue(true));
            cleanserManagerMenu.Add(new MenuBool("snare", "Snare").SetValue(true));
            cleanserManagerMenu.Add(new MenuBool("stun", "Stun").SetValue(true));
            cleanserManagerMenu.Add(new MenuBool("taunt", "Taunt").SetValue(true));
            cleanserManagerMenu.Add(new MenuBool("exhaust", "Exhaust").SetTooltip("Will only remove Slow"));
            cleanserManagerMenu.Add(new MenuBool("suppression", "Supression").SetValue(true));

            Menu.Add(cleanserManagerMenu);
            Menu.Attach();
            #endregion

            #region Subscriptions
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            GameObject.OnCreate += Game_OnCreate;
            Spellbook.OnCastSpell += Game_OnCastSpell;
            #endregion
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            //Remove barrelss, onDelete have huge delay
            for (int i = 0; i < savedBarrels.Count; i++)
            {
                if ((int)savedBarrels[i].barrel.Health != 1 && (int)savedBarrels[i].barrel.Health != 2 && (int)savedBarrels[i].barrel.Health != 3)
                {
                    savedBarrels.RemoveAt(i);
                    return;
                }
            }
            if (Menu["cleanserManager"]["enabled"].GetValue<MenuBool>().Enabled)
            {
                CleanserManager();
            }

            if (savedBarrels.Count >= 1)
            {
                if (Menu["CastEQ"].GetValue<MenuKeyBind>().Active || Menu["CastQ"].GetValue<MenuKeyBind>().Active)
                {

                    if (Menu["CastEQ"].GetValue<MenuKeyBind>().Active) isEQ = true;
                    else isEQ = false;
                    Barrel myQTarget = NearestExpBarrelToMouse();

                    //Kreygasm
                    if (!ECasted)
                    {
                        if (Player.Level >= 7 && Player.Level < 13)
                        {
                            var time = 2f * 1000;
                            var kappaHD = Environment.TickCount - myQTarget.time +
                                          (Player.Distance(myQTarget.barrel) / 2800f + Q.Delay) * 1000;

                            if (time < kappaHD)
                            {
                                if (isEQ)
                                {
                                    ECasted = true;
                                    if (myQTarget.barrel.Distance(Game.CursorPos) > BarrelConnectionRange * 2)
                                        E.Cast(correctThisPosition(Game.CursorPos.To2D(), myQTarget));
                                    else E.Cast(Game.CursorPos);
                                    Q.CastOnUnit(myQTarget.barrel);

                                }
                                else Q.CastOnUnit(myQTarget.barrel);



                            }

                        }
                        else if (Player.Level >= 13)
                        {

                            var time = 1f * 1000;
                            var kappaHD = Environment.TickCount - myQTarget.time +
                                          (Player.Distance(myQTarget.barrel) / 2800f + Q.Delay) * 1000;

                            if (time < kappaHD)
                            {
                                if (isEQ)
                                {
                                    ECasted = true;
                                    if (myQTarget.barrel.Distance(Game.CursorPos) > BarrelConnectionRange * 2)
                                        E.Cast(correctThisPosition(Game.CursorPos.To2D(), myQTarget));
                                    else E.Cast(Game.CursorPos);
                                    Q.CastOnUnit(myQTarget.barrel);

                                }
                                else Q.CastOnUnit(myQTarget.barrel);


                            }
                        }
                        else
                        {
                            var time = 4f * 1000;
                            var kappaHD = Environment.TickCount - myQTarget.time +
                                          (Player.Distance(myQTarget.barrel) / 2800f + Q.Delay) * 1000;

                            if (time < kappaHD)
                            {
                                if (isEQ)
                                {
                                    ECasted = true;
                                    if (myQTarget.barrel.Distance(Game.CursorPos) > BarrelConnectionRange * 2)
                                        E.Cast(correctThisPosition(Game.CursorPos.To2D(), myQTarget));
                                    else E.Cast(Game.CursorPos);
                                    Q.CastOnUnit(myQTarget.barrel);

                                }
                                else Q.CastOnUnit(myQTarget.barrel);


                            }
                        }
                    }


                }
                else
                {
                    ECasted = false;
                    isEQ = false;

                }
            }

            //Last hit
            if (Menu["Qlasthit"].GetValue<MenuKeyBind>().Active && !Menu["CastQ"].GetValue<MenuKeyBind>().Active && !Menu["CastEQ"].GetValue<MenuKeyBind>().Active && Q.IsReady())
            {

                var mini =
                    MinionManager.GetMinions(Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly)
                        .Where(m => m.Health < Q.GetDamage(m) && m.Name != "GangplankBarrel")
                        .OrderByDescending(m => m.MaxHealth)
                        .ThenByDescending(m => m.Distance(Player))
                        .FirstOrDefault();

                Q.CastOnUnit(mini);
            }

            //KS
            if (Menu["KS"].GetValue<MenuBool>().Enabled)
            {
                var kstarget = GameObjects.EnemyHeroes;
                if (kstarget != null)
                {
                    foreach (var ks in kstarget)
                    {
                        if (ks != null)
                        {
                            if (ks.Health <= Player.GetSpellDamage(ks, SpellSlot.Q) && ks.Health > 0 && Q.IsInRange(ks))
                            {

                                Q.CastOnUnit(ks);
                            }
                        }
                    }
                }
            }

            if (Menu["Ping"].GetValue<MenuBool>().Enabled)
                foreach (
                    var enemy in
                    ObjectManager.Get<AIHeroClient>()
                        .Where(
                            h =>
                                ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready &&
                                h.IsValidTarget() && h.HealthPercent <= 20))
                {
                    Ping(enemy.Position.To2D());
                }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Menu["Drawings"]["DrawAA"].GetValue<MenuBool>().Enabled)
                CircleRender.Draw(Player.Position, Player.AttackRange, Color.MediumSeaGreen);

            if (!Menu["Drawings"]["OnReady"].GetValue<MenuBool>().Enabled)
            {
                if (Menu["Drawings"]["DrawQ"].GetValue<MenuBool>().Enabled)
                    CircleRender.Draw(Player.Position, Q.Range, Color.BlueViolet);
                if (Menu["Drawings"]["DrawE"].GetValue<MenuBool>().Enabled)
                {
                    CircleRender.Draw(Player.Position, E.Range, Color.IndianRed);
                    if (Menu["Drawings"]["DrawERadius"].GetValue<MenuBool>().Enabled)
                        CircleRender.Draw(Game.CursorPos, BarrelConnectionRange, Color.Gray);
                }

                if (Menu["Drawings"]["DrawR"].GetValue<MenuBool>().Enabled)
                {
                    CircleRender.Draw(Game.CursorPos, UltRadius, Color.Black);
                    CircleRender.Draw(Game.CursorPos, DeathDaughterRadius, Color.DarkKhaki);
                }
            }
            else
            {
                if (Menu["Drawings"]["DrawQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
                    CircleRender.Draw(Player.Position, Q.Range, Color.BlueViolet);
                if (Menu["Drawings"]["DrawE"].GetValue<MenuBool>().Enabled && E.IsReady())
                {
                    CircleRender.Draw(Player.Position, E.Range, Color.IndianRed);
                    if (Menu["Drawings"]["DrawERadius"].GetValue<MenuBool>().Enabled)
                        CircleRender.Draw(Game.CursorPos, BarrelConnectionRange, Color.Gray);
                }
                if (Menu["Drawings"]["DrawR"].GetValue<MenuBool>().Enabled && R.IsReady())
                {
                    CircleRender.Draw(Game.CursorPos, UltRadius, Color.Black);
                    CircleRender.Draw(Game.CursorPos, DeathDaughterRadius, Color.DarkKhaki);
                }
            }

            //Draw connection line

            if (savedBarrels.Count >= 1)
            {
                if (NearestBarrelToMouse().barrel == null)
                {
                    return;
                }

                mouseToClosestBarrel = NearestBarrelToMouse().barrel.Distance(Game.CursorPos);
                //In connection range


                if (mouseToClosestBarrel <= BarrelConnectionRange * 2)
                {
                    Game.Print("2");
                    if (E.IsReady())
                    {
                        if (Menu["Drawings"]["DrawEConnection"].GetValue<MenuBool>().Enabled)
                            Drawing.DrawLine(Drawing.WorldToScreen(Game.CursorPos),
                                Drawing.WorldToScreen(NearestBarrelToMouse().barrel.Position), 4,
                                System.Drawing.Color.DarkGreen);
                        if (Menu["Drawings"]["DrawEConnectionRadius"].GetValue<MenuBool>().Enabled)
                            CircleRender.Draw(NearestBarrelToMouse().barrel.Position, BarrelConnectionRange,
                                Color.DarkGreen);
                        if (Menu["Drawings"]["DrawEPrediction"].GetValue<MenuBool>().Enabled)
                            CircleRender.Draw(NearestBarrelToMouse().barrel.Position, potentielRange,
                                Color.BlanchedAlmond);
                        CircleRender.Draw(Game.CursorPos, BarrelConnectionRange, Color.Green);
                    }
                }
                //Out of connection range & in search range
                else if (mouseToClosestBarrel > BarrelConnectionRange && mouseToClosestBarrel < maxSearchRange &&
                         E.IsReady())
                {
                    Game.Print("3");
                    if (Menu["Drawings"]["DrawEConnection"].GetValue<MenuBool>().Enabled)
                        Drawing.DrawLine(Drawing.WorldToScreen(Game.CursorPos),
                            Drawing.WorldToScreen(NearestBarrelToMouse().barrel.Position), 4, System.Drawing.Color.Red);
                    if (Menu["Drawings"]["DrawEConnectionRadius"].GetValue<MenuBool>().Enabled)
                        CircleRender.Draw(NearestBarrelToMouse().barrel.Position, BarrelConnectionRange, Color.Red);
                    if (Menu["Drawings"]["DrawEPrediction"].GetValue<MenuBool>().Enabled)
                        CircleRender.Draw(NearestBarrelToMouse().barrel.Position, potentielRange, Color.BlanchedAlmond);
                }
            }
        }

        private static void Game_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Barrel")
            {
                savedBarrels.Add(new Barrel(sender as AIMinionClient, System.Environment.TickCount));
            }
        }

        private static void Game_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.E && Menu["Corrector"].GetValue<MenuBool>().Enabled)
            {
                if (mouseToClosestBarrel > BarrelConnectionRange * 2 && mouseToClosestBarrel < maxSearchRange && correctThisPosition(Game.CursorPos.To2D(), closestToPosition(Game.CursorPos)).Distance(Game.CursorPos) <= correctionRange && savedBarrels.Count > 0 && !isEQ)
                {
                    args.Process = false;
                    Spellbook.OnCastSpell -= Game_OnCastSpell;
                    E.Cast(correctThisPosition(Game.CursorPos.To2D(), closestToPosition(Game.CursorPos)));
                    Spellbook.OnCastSpell += Game_OnCastSpell;

                }
            }
        }

        private static void CleanserManager()
        {
            // List of disable buffs
            if
                (W.IsReady() && (
                     (Player.HasBuffOfType(BuffType.Charm) && Menu["cleanserManager"]["charm"].GetValue<MenuBool>().Enabled)
                     || (Player.HasBuffOfType(BuffType.Flee) && Menu["cleanserManager"]["flee"].GetValue<MenuBool>().Enabled)
                     || (Player.HasBuffOfType(BuffType.Polymorph) && Menu["cleanserManager"]["polymorph"].GetValue<MenuBool>().Enabled)
                     || (Player.HasBuffOfType(BuffType.Snare) && Menu["cleanserManager"]["snare"].GetValue<MenuBool>().Enabled)
                     || (Player.HasBuffOfType(BuffType.Stun))
                     || (Player.HasBuffOfType(BuffType.Taunt) && Menu["cleanserManager"]["taunt"].GetValue<MenuBool>().Enabled)
                     || (Player.HasBuff("summonerexhaust") && Menu["cleanserManager"]["exhaust"].GetValue<MenuBool>().Enabled)
                     || (Player.HasBuffOfType(BuffType.Suppression) && Menu["cleanserManager"]["suppression"].GetValue<MenuBool>().Enabled)
                 ))

            {
                W.Cast();
            }
        }

        #region BarrelsSection
        internal class Barrel
        {
            public AIMinionClient barrel;
            public float time;

            public Barrel(AIMinionClient objAiBase, int tickCount)
            {
                barrel = objAiBase;
                time = tickCount;
            }
        }
        //return nearest barrel to mouse
        private static Barrel NearestBarrelToMouse()
        {
            var pos = Game.CursorPos.ToVector2();
            if (savedBarrels.Count == 0)
            {
                return null;
            }
            return savedBarrels.OrderBy(k => k.barrel.Position.Distance(pos.To3D())).FirstOrDefault();
        }

        private static Barrel NearestExpBarrelToMouse()
        {
            var pos = Game.CursorPos.To2D();
            if (savedBarrels.Count == 0)
            {
                return null;
            }
            savedBarrels.OrderBy(k => k.barrel.Position.Distance(pos.To3D()));

            return savedBarrels.OrderBy(k => k.barrel.Health).Where(k => k.barrel.Distance(Player) <= Q.Range).FirstOrDefault();

        }

        #endregion BarrelSection

        public static void Loads()
        {
            Game_OnGameLoad();
        }

        //ping
        private static void Ping(Vector2 position)
        {
            if (Environment.TickCount - LastPingT < 30 * 1000)
            {
                return;
            }

            LastPingT = Environment.TickCount;
            PingLocation = position;
            SimplePing();

            DelayAction.Add(150, SimplePing);
            DelayAction.Add(300, SimplePing);

        }

        private static void SimplePing()
        {
            Game.ShowPing(PingCategory.Fallback, PingLocation, true);
        }

        //Correct given position so it will connect to barrel to that position at max range

        public static Vector2 correctThisPosition(Vector2 position, Barrel barrelToConnect)
        {
            double vX = position.X - barrelToConnect.barrel.Position.X;
            double vY = position.Y - barrelToConnect.barrel.Position.Y;
            double magV = Math.Sqrt(vX * vX + vY * vY);
            double aX = Math.Round(barrelToConnect.barrel.Position.X + vX / magV * 680);
            double aY = Math.Round(barrelToConnect.barrel.Position.Y + vY / magV * 680);
            Vector2 newPosition = new Vector2(Convert.ToInt32(aX), Convert.ToInt32(aY));
            return newPosition;
        }


        //Return closest barrel to a position
        public static Barrel closestToPosition(Vector3 position)
        {
            if (savedBarrels.Count() == 0)
                return null;
            Barrel closest = null;
            float bestSoFar = -1;


            for (int i = 0; i < savedBarrels.Count; i++)
            {
                if (bestSoFar == -1 || savedBarrels[i].barrel.Distance(position) < bestSoFar)
                {
                    bestSoFar = savedBarrels[i].barrel.Distance(position);
                    closest = savedBarrels[i];
                }
            }
            return closest;
        }
    }


}