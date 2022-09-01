﻿using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
 using EnsoulSharp.SDK.MenuUI;
 using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace MightyAio.Champions
{
    internal class Lillia
    {
        #region Starter

        private static AIHeroClient Player => ObjectManager.Player;

        private static Menu Menu, alliles, Emotes;

        #endregion

        #region Menu

        private static void CreateMenu()
        {
            Menu = new Menu("Senna", "Senna", true);

            // Q
            var QMenu = new Menu("Q", "Q")
            {
                new MenuSlider("HealSet", "Auto Heal When Ally is below", 50),
                new MenuBool("QHeal", "Auto Heal"),
                new MenuSlider("QHealM", "Mana for Q heal", 50),
                new MenuKeyBind("QSimi", "Heal simikey", Keys.A, KeyBindType.Press),
                new MenuBool("QC", "Use Q in Combo"),
                new MenuBool("QH", "Use Q in Harass"),
                new MenuSlider("QMana", "Mana for Q in harass", 40),
                new MenuBool("QE", "Use Extended Q ")
            };
            var ally = from hero in ObjectManager.Get<AIHeroClient>()
                where hero.IsAlly
                select hero;
            alliles = new Menu("alliles", "Use Q on");
            foreach (var hero in ally.Where(x => x.CharacterName != "Senna"))
                alliles.Add(new MenuBool(hero.CharacterName, "Use Q on " + hero.CharacterName));
            QMenu.Add(alliles);
            Menu.Add(QMenu);

            // W
            var WMenu = new Menu("W", "W")
            {
                new MenuBool("WC", "Use W in Combo"),
                new MenuBool("WH", "Use W in Harass"),
                new MenuSlider("WMana", "Mana for W in harass", 40),
                new MenuBool("WGap", "Use W in gapclose "),
                new MenuBool("WI", "Use W in Interrupter", false)
            };
            Menu.Add(WMenu);
            // E
            var EMenu = new Menu("E", "E")
            {
                new MenuBool("EC", "Use E in Combo"),
                new MenuBool("EF", "Use E in Feel"),
                new MenuKeyBind("FeelKey", "Feel Key", Keys.Z, KeyBindType.Press)
            };
            Menu.Add(EMenu);
            // R
            var RMenu = new Menu("R", "R")
            {
                new MenuBool("RC", "Use R in Combo Only When Target is out of range and killable"),
                new MenuKeyBind("RS", "Rsimikey", Keys.H, KeyBindType.Press)
            };
            Menu.Add(RMenu);
            // lane clear
            var laneclear = new Menu("laneclear", "Lane Clear")
            {
                new MenuBool("Q", "Use Q for Lane Clear", false),
                new MenuSlider("QMana", "Mana for Q in LaneClear", 40),
                new MenuSlider("Qcount", "Only use Q if it can hit ", 2, 0, 5),
                new MenuSeparator("Soul", "X Simi collecting Souls key")
            };
            Menu.Add(laneclear);

            // kill steal
            var killsteal = new Menu("KillSteal", "Kill Steal")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("WQ", "UseWardQ"),
                new MenuBool("D", "Use Ignite"),
                new MenuBool("W", "Use W"),
                new MenuBool("R", "Use R", false)
            };
            Menu.Add(killsteal);

            // Misc
            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuBool("UseSkin", "Use Skin Changer"),
                new MenuSlider("setskin", "set skin", 10, 0, 55),
                new MenuBool("autolevel", "Auto Level")
            };

            // use emotes
            Emotes = new Menu("Emotes", "Emotes")
            {
                new MenuList("selectitem", "Select Item",
                    new[] {"Center", "East", "West", "South", "North", "Mastery"}),
                new MenuBool("Kill", "Use on kill")
            };
            miscMenu.Add(Emotes);
            Menu.Add(miscMenu);

            // Drawing
            var drawMenu = new Menu("Drawing", "Draw")
            {
                new MenuBool("DrawQ", "Draw Q"),
                new MenuBool("DrawW", "Draw W"),
                new MenuBool("DrawE", "Draw E"),
                new MenuBool("DrawQ2", "Draw Q max range"),
                new MenuBool("PermaShow", "Perma Show"),
                new MenuBool("Drawkillabeabilities", "Draw kill abe abilities")
            };
            Menu.Add(drawMenu);

            Menu.Attach();
        }

        #endregion Menu

        #region Spells

        private static Font Berlinfont;

        private static int mykills = 0 + Player.ChampionsKilled;
        private static Spell Q, W, E, E2 , R;
        private static int[] SpellLevels;
        
        #endregion Spells

        #region GameLoad

        public Lillia()
        {
            SpellLevels = new[] {1, 2, 3, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3};
            CreateMenu();
            Q = new Spell(SpellSlot.Q, 485);
            W = new Spell(SpellSlot.W, 500);
            W.SetSkillshot(0.1f, 250, 1200f, true, SpellType.Circle);
            E = new Spell(SpellSlot.E, 750);
            E.SetSkillshot(0.4f,150,1000f,false,SpellType.Line);
            E2 = new Spell(SpellSlot.E, 20000f);
            E2.SetSkillshot(0.4f,150,1000f,true,SpellType.Line);
            R = new Spell(SpellSlot.R, 20000f);
            R.SetSkillshot(1f, 320f, 20000f, false, SpellType.Line);

            Berlinfont = new Font(
                Drawing.Direct3DDevice9,
                new FontDescription
                {
                    FaceName = "Berlin San FB Demi",
                    Height = 23,
                    Weight = FontWeight.DemiBold,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.ClearTypeNatural
                });

            Game.OnUpdate += GameOnOnUpdate;
            AntiGapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Drawing.OnDraw += DrawingOnOnDraw;
            Interrupter.OnInterrupterSpell += OnInterruptible;
        }

        private static void DrawText(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor)
        {
            aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = Menu["Drawing"].GetValue<MenuBool>("DrawQ").Enabled;
            var drawQM = Menu["Drawing"].GetValue<MenuBool>("DrawQ2").Enabled;
            var drawW = Menu["Drawing"].GetValue<MenuBool>("DrawW").Enabled;
            var drawE = Menu["Drawing"].GetValue<MenuBool>("DrawE").Enabled;
            var drawKill = Menu["Drawing"].GetValue<MenuBool>("Drawkillabeabilities").Enabled;
            var PermaShow = Menu["Drawing"].GetValue<MenuBool>("PermaShow").Enabled;
            var p = Player.Position;

            if (drawQ && (Q.IsReady() || PermaShow))
                Drawing.DrawCircleIndicator(p, Player.GetRealAutoAttackRange(), Color.Purple);
            if (drawE && (E.IsReady() || PermaShow)) Drawing.DrawCircleIndicator(p, E.Range, Color.Red);
            if (drawW && (W.IsReady() || PermaShow)) Drawing.DrawCircleIndicator(p, W.Range, Color.DarkCyan);

            foreach (
                var enemyVisible in
                ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget(2000)))
                if (enemyVisible.IsValidTarget())
                {
                    var autodmg = Player.CalculateDamage(enemyVisible, DamageType.Physical, Player.TotalAttackDamage) +
                                  Player.CalculateDamage(enemyVisible, DamageType.Physical, Player.TotalAttackDamage) *
                                  Player.Crit;
                    var aa = string.Format("AA Left:" + (int) (enemyVisible.Health / autodmg));
                    if (drawKill)
                    {
                        if (100> enemyVisible.Health)
                            DrawText(Berlinfont, "Killable Skills (Q):",
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                        else
                            DrawText(Berlinfont, aa, (int) Drawing.WorldToScreen(enemyVisible.Position)[0] - 38,
                                (int) Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                    }
                }
        }

        #endregion GameLoad

        #region Update

        private static void GameOnOnUpdate(EventArgs args)
        {
            if (Player.ChampionsKilled > mykills && Emotes.GetValue<MenuBool>("Kill").Enabled)
            {
                mykills = Player.ChampionsKilled;
                Emote();
            }

            var getskin = Menu["Misc"].GetValue<MenuSlider>("setskin").Value;
            var skin = Menu["Misc"].GetValue<MenuBool>("UseSkin").Enabled;
            if (skin && Player.SkinId != getskin) Player.SetSkin(getskin);

            if (!Player.CanCast) return;
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Harass:
                    Harass();
                    break;

                case OrbwalkerMode.Combo:
                    Combo();
                    break;

                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    break;

                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
            }

            if (Menu["E"].GetValue<MenuKeyBind>("FeelKey").Active) Feel();
            Killsteal();
            if (Menu["Misc"].GetValue<MenuBool>("autolevel").Enabled) Levelup();
        }

        #endregion Update


        #region Orbwalker mod

        private static void LastHit()
        {
            
        }

        private static void Feel()
        {
            var UseEtofeel = Menu["E"].GetValue<MenuBool>("EF").Enabled;
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (E.IsReady() && UseEtofeel) E.Cast();
        }

        private static void LaneClear()
        {
          
        }
        

        private static void Combo()
        {
          
        }


        private static void Harass()
        {
           
        }

        #endregion

        #region Args

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs Args)
        {
            if (!Menu["W"].GetValue<MenuBool>("WGap").Enabled) return;

            if (W.IsReady() && sender != null && sender.IsValidTarget(W.Range))
            {
                var pred = W.GetPrediction(sender);

                if (pred != null && pred.Hitchance >= HitChance.High) W.Cast(pred.CastPosition);
            }
        }

        private void OnInterruptible(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (!Menu["W"].GetValue<MenuBool>("WI").Enabled) return;

            if (W.IsReady() && sender != null && sender.IsValidTarget(W.Range))
            {
                var pred = W.GetPrediction(sender);

                if (pred != null && pred.Hitchance >= HitChance.High) W.Cast(pred.CastPosition);
            }
        }

       

        #endregion Args

        #region Extra functions

       

       

        private static void Levelup()
        {
            var qLevel = Q.Level;
            var wLevel = W.Level;
            var eLevel = E.Level;
            var rLevel = R.Level;

            if (qLevel + wLevel + eLevel + rLevel >= ObjectManager.Player.Level || Player.Level > 18) return;

            var level = new[] {0, 0, 0, 0};
            for (var i = 0; i < ObjectManager.Player.Level; i++)
                level[SpellLevels[i] - 1] = level[SpellLevels[i] - 1] + 1;

            if (qLevel < level[0]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);

            if (wLevel < level[1]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);

            if (eLevel < level[2]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
            if (rLevel < level[3]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
        }

        private static bool IsInInnerRange(AIHeroClient Target)
        {
            return Target.DistanceToPlayer() > 225;
        }
        private static bool DreamBuff(AIHeroClient Target)
        {
            const string BuffName = "buffname";
            return Target.HasBuff(BuffName);
        }

        private static void Emote()
        {
            var b = Emotes.GetValue<MenuList>("selectitem").SelectedValue;
            switch (b)
            {
                case "Mastery":
                    Game.SendSummonerEmote(SummonerEmoteSlot.Mastery);
                    break;

                case "Center":
                    Game.SendSummonerEmote(SummonerEmoteSlot.Center);
                    break;

                case "South":
                    Game.SendSummonerEmote(SummonerEmoteSlot.South);
                    break;

                case "West":
                    Game.SendSummonerEmote(SummonerEmoteSlot.West);
                    break;

                case "East":
                    Game.SendSummonerEmote(SummonerEmoteSlot.East);
                    break;

                case "North":
                    Game.SendSummonerEmote(SummonerEmoteSlot.North);
                    break;
            }
        }

        private static void Killsteal()
        {
       
        }

     
        #endregion
    }
}