﻿using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using LeagueSharpCommon;
using PortAIO.Properties;
using SebbyLib;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Geometry = LeagueSharpCommon.Geometry.Geometry;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using MenuItem = EnsoulSharp.SDK.MenuUI.MenuItem;
using Render = LeagueSharpCommon.Render;
//using Menu = EnsoulSharp.SDK.MenuUI;

namespace OneKeyToWin_AIO_Sebby.Core
{
    class OktwNotification
    {
        public Render.Sprite Hero;
        public Render.Sprite SpellIco;
        public bool Lost;
        public int TimeRec = 0;
        public OktwNotification(Render.Sprite hero, Render.Sprite spellIco, bool lost)
        {
            Hero = hero;
            SpellIco = spellIco;
            Lost = lost;
        }
    }

    class OKTWdraws : Program
    {
        public static List<OktwNotification> NotificationsList = new List<OktwNotification>();

        public static Font Tahoma13, Tahoma13B, TextBold, HudLevel, HudCd, RecFont, HudLevel2;
        public static Vector2 centerScreen = new Vector2(Drawing.Width / 2 - 20, Drawing.Height / 2 - 90);
        private float IntroTimer = Game.Time;
        private Render.Sprite Intro;
        
        private MenuItem en_enabled;
        private MenuItem en_animated;
        private MenuItem en_thickness;

        private static Render.Sprite Flash,
            Heal,
            Exhaust,
            Teleport,
            Smite,
            Ignite,
            Barrier,
            Clairvoyance,
            Cleanse,
            Ghost,
            Ultimate,
            Pink,
            Ward,
            PinkMM,
            WardMM,
            Not;

        private static Render.Sprite FlashS,
            HealS,
            ExhaustS,
            TeleportS,
            SmiteS,
            IgniteS,
            BarrierS,
            ClairvoyanceS,
            CleanseS,
            GhostS,
            UltimateS,
            Isready,
            Lost;

        private static Render.Sprite FlashSquare,
            HealSquare,
            ExhaustSquare,
            TeleportSquare,
            SmiteSquare,
            IgniteSquare,
            BarrierSquare,
            ClairvoyanceSquare,
            CleanseSquare,
            GhostSquare;

        private bool MenuOpen = false;
        private static int NotTimer = Environment.TickCount + 1500;

        public static string[] IgnoreR =
        {
            "Corki", "Jayce", "Kassadin", "KogMaw", "Leblanc", "Teemo", "Swain", "Shyvana", "Nidalee", "Anivia",
            "Quinn", "Gnar"
        };

        public void LoadOKTW()
        {
            Flash = ImageLoader.CreateSummonerSprite("Flash");
            Heal = ImageLoader.CreateSummonerSprite("Heal");
            Exhaust = ImageLoader.CreateSummonerSprite("Exhaust");
            Teleport = ImageLoader.CreateSummonerSprite("Teleport");
            Ignite = ImageLoader.CreateSummonerSprite("Ignite");
            Barrier = ImageLoader.CreateSummonerSprite("Barrier");
            Clairvoyance = ImageLoader.CreateSummonerSprite("Clairvoyance");
            Cleanse = ImageLoader.CreateSummonerSprite("Cleanse");
            Ghost = ImageLoader.CreateSummonerSprite("Ghost");
            Smite = ImageLoader.CreateSummonerSprite("Smite");
            Ultimate = ImageLoader.CreateSummonerSprite("r");
            Pink = ImageLoader.CreateSummonerSprite("pink");
            Pink.Scale = new Vector2(0.15f, 0.15f);
            Ward = ImageLoader.CreateSummonerSprite("ward");
            Ward.Scale = new Vector2(0.15f, 0.15f);
            PinkMM = ImageLoader.CreateSummonerSprite("WT_Pink");
            WardMM = ImageLoader.CreateSummonerSprite("WT_Green");

            Not = ImageLoader.GetSprite("not");

            FlashS = ImageLoader.GetSprite("Flash");
            HealS = ImageLoader.GetSprite("Heal");
            ExhaustS = ImageLoader.GetSprite("Exhaust");
            TeleportS = ImageLoader.GetSprite("Teleport");
            IgniteS = ImageLoader.GetSprite("Ignite");
            BarrierS = ImageLoader.GetSprite("Barrier");
            ClairvoyanceS = ImageLoader.GetSprite("Clairvoyance");
            CleanseS = ImageLoader.GetSprite("Cleanse");
            GhostS = ImageLoader.GetSprite("Ghost");
            SmiteS = ImageLoader.GetSprite("Smite");
            UltimateS = ImageLoader.GetSprite("r");
            Isready = ImageLoader.GetSprite("isready");
            Lost = ImageLoader.GetSprite("lost");

            FlashSquare = ImageLoader.GetSprite("Flash");
            HealSquare = ImageLoader.GetSprite("Heal");
            ExhaustSquare = ImageLoader.GetSprite("Exhaust");
            TeleportSquare = ImageLoader.GetSprite("Teleport");
            IgniteSquare = ImageLoader.GetSprite("Ignite");
            BarrierSquare = ImageLoader.GetSprite("Barrier");
            ClairvoyanceSquare = ImageLoader.GetSprite("Clairvoyance");
            CleanseSquare = ImageLoader.GetSprite("Cleanse");
            GhostSquare = ImageLoader.GetSprite("Ghost");
            SmiteSquare = ImageLoader.GetSprite("Smite");

            FlashSquare.Scale = new Vector2(0.4f, 0.4f);
            HealSquare.Scale = new Vector2(0.4f, 0.4f);
            ExhaustSquare.Scale = new Vector2(0.4f, 0.4f);
            TeleportSquare.Scale = new Vector2(0.4f, 0.4f);
            IgniteSquare.Scale = new Vector2(0.4f, 0.4f);
            BarrierSquare.Scale = new Vector2(0.4f, 0.4f);
            ClairvoyanceSquare.Scale = new Vector2(0.4f, 0.4f);
            CleanseSquare.Scale = new Vector2(0.4f, 0.4f);
            GhostSquare.Scale = new Vector2(0.4f, 0.4f);
            SmiteSquare.Scale = new Vector2(0.4f, 0.4f);

            if (Config["aboutoktw"]["logo"].GetValue<MenuBool>().Enabled)
            {
                Intro = new Render.Sprite(LoadImg("intro"),
                    new Vector2((Drawing.Width / 2) - 500, (Drawing.Height / 2) - 350));
                Intro.Add(0);
                Intro.OnDraw();
            }

            DelayAction.Add(7000, () => Intro.Remove());

            var ud = Config.Add(new Menu("ud", "Utility, Draws OKTW©"));

            var hud = ud.Add(new Menu("hud", "Hud"));
            hud.Add(new MenuBool("championInfo", "Show enemy avatars").SetValue(true));
            hud.Add(new MenuBool("championInfoHD", "Full HD screen size").SetValue(true));
            hud.Add(new MenuSlider("posX", "Avatars posX", 839, 0, 1000));
            hud.Add(new MenuSlider("posY", "Avatars posY", 591, 0, 1000));

            hud.Add(new MenuBool("gankalert", "Gank alert").SetValue(true));
            hud.Add(new MenuSlider("posXj", "Alert posX", 639, 0, 1000));
            hud.Add(new MenuSlider("posYj", "Alert posY", 591, 0, 1000));

            var screen = ud.Add(new Menu("screen", "Screen"));
            var notifications = screen.Add(new Menu("noti", "Notification"));
            notifications.Add(new MenuBool("Notification", "Enable").SetValue(true));
            notifications.Add(new MenuBool("NotificationR", "Ultimate").SetValue(true));
            notifications.Add(new MenuBool("NotificationS", "Summoners").SetValue(true));
            notifications.Add(new MenuSlider("posXn", "Notifications posX ", 400, 0, 1000));
            notifications.Add(new MenuSlider("posYn", "Notifications posY", 50, 0, 1000));

            var awr = screen.Add(new Menu("awr", "Awareness radar"));
            awr.Add(new MenuBool("ScreenRadar", "Enable").SetValue(true));
            awr.Add(new MenuBool("ScreenRadarEnemy", "Only enemy").SetValue(true));
            awr.Add(new MenuBool("ScreenRadarJungler", "Only jungler").SetValue(true));

            //screen.Add(new MenuBool("HpBar", "Damage indicators").SetValue(true));

            var spelltracker = screen.Add(new Menu("spelltracker", "Spell tracker"));
            spelltracker.Add(new MenuBool("SpellTrackerEnemy", "Enemy").SetValue(true));
            spelltracker.Add(new MenuBool("SpellTrackerAlly", "Ally").SetValue(true));
            spelltracker.Add(new MenuBool("SpellTrackerMe", "Me").SetValue(true));
            spelltracker.Add(new MenuBool("SpellTrackerLvl", "Show spell lvl (can drop fps)").SetValue(true));

            ud.Add(new MenuBool("ShowClicks", "Show enemy clicks").SetValue(false));
            ud.Add(new MenuBool("showWards", "Show hidden objects, wards").SetValue(true));
            ud.Add(new MenuBool("buffTracker", "My buff tracker").SetValue(false));
            ud.Add(new MenuBool("HpBar", "Damage indicators").SetValue(true));
            ud.Add(new MenuBool("minimap", "Mini-map hack").SetValue(true));

            ud.Add(new MenuBool("disableDraws", "DISABLE UTILITY DRAWS").SetValue(false));

            Tahoma13B = new Font(Drawing.Direct3DDevice9, new FontDescription
            {
                FaceName = "Tahoma", Height = 14, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default,
                Quality = FontQuality.Antialiased
            });

            Tahoma13 = new Font(Drawing.Direct3DDevice9, new FontDescription
            {
                FaceName = "Tahoma", Height = 14, OutputPrecision = FontPrecision.Default,
                Quality = FontQuality.Antialiased
            });

            TextBold = new Font(Drawing.Direct3DDevice9, new FontDescription
            {
                FaceName = "Impact", Height = 30, Weight = FontWeight.Normal, OutputPrecision = FontPrecision.Default,
                Quality = FontQuality.Antialiased
            });

            HudLevel = new Font(Drawing.Direct3DDevice9, new FontDescription
            {
                FaceName = "Tahoma", Height = 17, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default,
                Quality = FontQuality.Antialiased
            });

            HudLevel2 = new Font(Drawing.Direct3DDevice9, new FontDescription
            {
                FaceName = "Tahoma", Height = 12, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default,
                Quality = FontQuality.Antialiased
            });

            RecFont = new Font(Drawing.Direct3DDevice9, new FontDescription
            {
                FaceName = "Tahoma", Height = 12, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default,
                Quality = FontQuality.Antialiased
            });

            HudCd = new Font(Drawing.Direct3DDevice9, new FontDescription
            {
                FaceName = "Tahoma", Height = 11, Weight = FontWeight.Normal, OutputPrecision = FontPrecision.Default,
                Quality = FontQuality.Antialiased
            });

            Drawing.OnEndScene += Drawing_OnEndScene;
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
        }
        

        private void Game_OnWndProc(GameWndEventArgs args)
        {
            if (args.Msg == 256)
                MenuOpen = true;
            if (args.Msg == 257)
                MenuOpen = false;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (NotTimer + 1000 < Environment.TickCount)
            {
                foreach (var hero in OKTWtracker.ChampionInfoList.Where(x => x.Hero.IsEnemy))
                {
                    var rSlot = hero.Hero.Spellbook.Spells[3];
                    var sum1 = hero.Hero.Spellbook.Spells[4];
                    var sum2 = hero.Hero.Spellbook.Spells[5];
                    if (Config["screen"]["noti"]["NotificationR"].GetValue<MenuBool>().Enabled && rSlot != null && !IgnoreR.Any(x => x == hero.Hero.CharacterName))
                    {
                        var time = rSlot.CooldownExpires - Game.Time;
                        if (time < 1 && time >= 0)
                            NotificationsList.Add(new OktwNotification(hero.SquareSprite, GetSummonerIconS("r"), false));
                        else if (rSlot.Cooldown - time < 1 && rSlot.Cooldown - time >= 0)
                            NotificationsList.Add(new OktwNotification(hero.SquareSprite, GetSummonerIconS("r"), true));
                    }
                    if (Config["screen"]["noti"]["NotificationS"].GetValue<MenuBool>().Enabled)
                    {
                        if (sum1 != null)
                        {
                            var time = sum1.CooldownExpires - Game.Time;
                            if (time < 1 && time >= 0)
                                NotificationsList.Add(new OktwNotification(hero.SquareSprite, GetSummonerIconS(sum1.Name), false));
                            else if (sum1.Cooldown - time < 1 && sum1.Cooldown - time >= 0)
                                NotificationsList.Add(new OktwNotification(hero.SquareSprite, GetSummonerIconS(sum1.Name), true));
                        }
                        if (sum2 != null)
                        {
                            var time = sum2.CooldownExpires - Game.Time;
                            if (time < 1 && time >= 0)
                                NotificationsList.Add(new OktwNotification(hero.SquareSprite, GetSummonerIconS(sum2.Name), false));
                            else if (sum2.Cooldown - time < 1 && sum2.Cooldown - time >= 0)
                                NotificationsList.Add(new OktwNotification(hero.SquareSprite, GetSummonerIconS(sum2.Name), true));
                        }
                    }
                }
                NotTimer = Environment.TickCount;
            }
        }

        private void Drawing_OnEndScene(EventArgs args)
        {
            if (Config["ud"]["disableDraws"].GetValue<MenuBool>().Enabled)
                return;

            bool blink = (int) (Game.Time * 10) % 2 != 0;

            var minimap = Config["ud"]["minimap"].GetValue<MenuBool>().Enabled;
            var HpBar = Config["ud"]["HpBar"].GetValue<MenuBool>().Enabled;
            var championInfo = Config["ud"]["hud"]["championInfo"].GetValue<MenuBool>().Enabled;
            var ScreenRadar = Config["ud"]["screen"]["awr"]["ScreenRadar"].GetValue<MenuBool>().Enabled;
            var ScreenRadarEnemy = Config["ud"]["screen"]["awr"]["ScreenRadarEnemy"].GetValue<MenuBool>().Enabled;
            var ScreenRadarJungler = Config["ud"]["screen"]["awr"]["ScreenRadarJungler"].GetValue<MenuBool>().Enabled;
            var SpellTrackerEnemy = Config["ud"]["screen"]["spelltracker"]["SpellTrackerEnemy"].GetValue<MenuBool>().Enabled;
            var SpellTrackerAlly = Config["ud"]["screen"]["spelltracker"]["SpellTrackerAlly"].GetValue<MenuBool>().Enabled;
            var SpellTrackerMe = Config["ud"]["screen"]["spelltracker"]["SpellTrackerMe"].GetValue<MenuBool>().Enabled;
            var SpellTrackerLvl = Config["ud"]["screen"]["spelltracker"]["SpellTrackerLvl"].GetValue<MenuBool>().Enabled;
            var ShowClicks = Config["ud"]["ShowClicks"].GetValue<MenuBool>().Enabled;
            var championInfoHD = Config["ud"]["hud"]["championInfoHD"].GetValue<MenuBool>().Enabled;
            float posY = (Config["ud"]["hud"]["posY"].GetValue<MenuSlider>().Value * 0.001f) * Drawing.Height;
            float posX = (Config["ud"]["hud"]["posX"].GetValue<MenuSlider>().Value * 0.001f) * Drawing.Width;

            float posYn = (Config["ud"]["screen"]["noti"]["posYn"].GetValue<MenuSlider>().Value * 0.001f) * Drawing.Height;
            float posXn = (Config["ud"]["screen"]["noti"]["posXn"].GetValue<MenuSlider>().Value * 0.001f) * Drawing.Width;

            var Width = 104;
            var Height = 11;
            var XOffset = -45;
            var YOffset = -24;
            var FillColor = System.Drawing.Color.GreenYellow;
            var Color = System.Drawing.Color.Azure;

            var hudSpace = 0;
            var area = 5000;
            var notPos = new Vector2(posXn, posYn);

            var centerScreenWorld = Drawing.ScreenToWorld(centerScreen);
            if (Config["ud"]["hud"]["gankalert"].GetValue<MenuBool>().Enabled)
            {
                var jungler = OKTWtracker.ChampionInfoList.FirstOrDefault(x => x.Hero.IsEnemy && x.IsJungler);
                var stringg = "Jungler not detected";
                float posXj = (Config["ud"]["hud"]["posXj"].GetValue<MenuSlider>().Value * 0.001f) * Drawing.Width;
                float posYj = (Config["ud"]["hud"]["posYj"].GetValue<MenuSlider>().Value * 0.001f) * Drawing.Height;


                var jungleAlertPos = new Vector2(posXj, posYj);
                if (jungler != null)
                {
                    Drawing.DrawLine(jungleAlertPos, jungleAlertPos + new Vector2(120, 0), 20,
                        System.Drawing.Color.Black);
                    Drawing.DrawLine(jungleAlertPos + new Vector2(1, 1), jungleAlertPos + new Vector2(119, 1), 18,
                        System.Drawing.Color.DarkGreen);
                    float percent = 0;
                    var distance = jungler.LastVisablePos.Distance(Player.Position);
                    if (Game.Time - jungler.FinishRecallTime < 4)
                    {
                        stringg = "Jungler in base";
                        percent = 0;
                    }
                    else if (jungler.Hero.IsDead)
                    {
                        stringg = "Jungler dead";
                        percent = 0;
                    }
                    else if (distance < 3500)
                    {
                        stringg = "Jungler NEAR you";
                        percent = 1;
                    }
                    else if (jungler.Hero.IsHPBarRendered)
                    {
                        stringg = "Jungler visable";
                        percent = 0;
                    }
                    else
                    {
                        var timer = jungler.LastVisablePos.Distance(Player.Position) / 330;
                        var time2 = timer - (Game.Time - jungler.LastVisableTime);
                        stringg = "Jungler in jungle " + (int) time2;
                        time2 = time2 - 10;
                        if (time2 > 0)
                            percent = 0;
                        else
                            percent = (-time2) * 0.05f;
                        //Console.WriteLine(timer + " " + time2);
                        percent = Math.Min(percent, 1);
                    }

                    if (percent != 0)
                        Drawing.DrawLine(jungleAlertPos + new Vector2(1, 1),
                            jungleAlertPos + new Vector2(1 + 118 * percent, 1), 18, System.Drawing.Color.OrangeRed);
                }

                DrawFontTextScreen(RecFont, stringg, jungleAlertPos.X + 3, jungleAlertPos.Y + 3, SharpDX.Color.White);
            }

            if (Config["ud"]["screen"]["noti"]["Notification"].GetValue<MenuBool>().Enabled)
            {
                //if (Config.DrawMenu)
                //{
                    //Not.Position = notPos;
                    //Not.Color = new ColorBGRA(0, 0.5f, 1f, 0.6f * 1);
                    //Not.OnEndScene();
                //}

                var noti = NotificationsList.FirstOrDefault();
                if (noti != null)
                {
                    if (noti.TimeRec == 0)
                    {
                        noti.TimeRec = Environment.TickCount;
                    }
                    else if (Environment.TickCount - noti.TimeRec > 3000)
                    {
                        NotificationsList.Remove(noti);
                    }
                    else
                    {
                        float time = Environment.TickCount - noti.TimeRec;
                        float calcOpacity = 1;

                        if (time < 500)
                            calcOpacity = time / 500;
                        else if (time > 2500)
                            calcOpacity = (3000 - time) / 500;

                        var opacity = new ColorBGRA(256, 256, 256, 0.9f * calcOpacity);
                        Not.Position = notPos;

                        if (noti.Lost)
                            Not.Color = new ColorBGRA(0, 0.5f, 1f, 0.6f * calcOpacity);

                        else
                            Not.Color = new ColorBGRA(0.5f, 0, 0, 0.6f * calcOpacity);

                        Not.OnEndScene();

                        if (noti.Lost)
                        {
                            Lost.Position = notPos + new Vector2(81, 8);
                            Lost.Color = opacity;
                            Lost.OnEndScene();
                        }
                        else
                        {
                            Isready.Position = notPos + new Vector2(81, 8);
                            Isready.Color = opacity;
                            Isready.OnEndScene();
                        }

                        noti.SpellIco.Position = notPos + new Vector2(8, 8);
                        noti.SpellIco.Color = opacity;
                        noti.SpellIco.OnEndScene();

                        noti.Hero.Position = notPos + new Vector2(152, 8);
                        noti.Hero.Color = opacity;
                        noti.Hero.Scale = new Vector2(0.547f, 0.547f);
                        noti.Hero.OnEndScene();

                    }
                }
            }
            
            if (Config["ud"]["showWards"].GetValue<MenuBool>().Enabled)
            {
                #region showWards
                var circleSize = 30;
                foreach (var obj in OKTWward.HiddenObjList)
                {
                    if (obj.pos.IsOnScreen())
                    {
                        var pos = Drawing.WorldToScreen(obj.pos) + new Vector2(-50, -50); ;
                        if (obj.type == 1)
                        {
                            Drawing.DrawText(pos.X - 15, pos.Y - 13, System.Drawing.Color.YellowGreen, MakeNiceNumber(obj.endTime - Game.Time));
                            Ward.Position = pos + new Vector2(10, 10);

                            Ward.OnEndScene();
                            DrawFontTextMap(Tahoma13, ((int)(obj.endTime - Game.Time)).ToString(), obj.pos + new Vector3(-18, 18, 0), SharpDX.Color.White);
                        }
                        else if (obj.type == 2)
                        {
                            Pink.Position = pos + new Vector2(10, 10);
                            Pink.OnEndScene();

                        }
                        else if (obj.type == 3)
                        {
                            OktwCommon.DrawTriangleOKTW(circleSize, obj.pos, System.Drawing.Color.Orange);
                            DrawFontTextMap(Tahoma13, "! " + (int)(obj.endTime - Game.Time), obj.pos, SharpDX.Color.Orange);
                        }
                    }

                    {
                        var pos = Drawing.WorldToMinimap(obj.pos);
                        if (obj.type == 1)
                        {
                            WardMM.Position = pos;
                            WardMM.OnEndScene();
                        }
                        else if (obj.type == 2)
                        {
                            PinkMM.Position = pos;
                            PinkMM.OnEndScene();
                        }
                        else if (obj.type == 3)
                        {

                        }
                    }
                }
                #endregion
            }
            if (Config["ud"]["buffTracker"].GetValue<MenuBool>().Enabled)
            {
                var j = 50;
                foreach (var buff in Player.Buffs.Where(buff => buff.Type == BuffType.CombatEnchancer))
                {
                    var timeToEnd = buff.EndTime - Game.Time;

                    if (timeToEnd < 0 || timeToEnd > 1000 || buff.Name.ToLower().Contains("mastery"))
                        continue;

                    var percent = (timeToEnd / (buff.EndTime - buff.StartTime)) * 50;

                    var color = System.Drawing.Color.YellowGreen;

                    var buffName = "";

                    if (buff.Name.Contains("AncientGolem"))
                    {
                        color = System.Drawing.Color.Aqua;
                        buffName = " Blue";
                    }
                    else if (buff.Name.Contains("LizardElder"))
                    {
                        color = System.Drawing.Color.Red;
                        buffName = " Red";
                    }
                    else if (buff.Name.Contains("OfBaron"))
                    {
                        color = System.Drawing.Color.Purple;
                        buffName = " Baron";
                    }
                    else if (buff.Name.Contains("Sheen"))
                    {
                        color = System.Drawing.Color.Blue;
                        buffName = " Sheen";
                    }
                    else
                    {
                        if (percent < 10)
                            color = System.Drawing.Color.OrangeRed;
                        else if (percent < 25)
                            color = System.Drawing.Color.Orange;

                        foreach (var letter in buff.Name)
                        {
                            if (char.IsUpper(letter))
                            {
                                buffName += " ";
                                buffName += letter;
                            }
                            else
                            {
                                buffName += letter;
                            }
                        }
                    }

                    var position = Player.HPBarPosition + new Vector2(+250, j);
                    DrawFontTextScreen(HudCd, (timeToEnd).ToString("0.0") + " " + buffName, position.X, position.Y, SharpDX.Color.White);
                    Drawing.DrawLine(position + new Vector2(-56, 0), position + new Vector2(-4, 0), 10, System.Drawing.Color.Black);
                   
                    Drawing.DrawLine(position + new Vector2(-55, 1), position + new Vector2(-55 + percent, 1), 8, color);
                    j += 15;
                }
            }

            foreach (var hero in OKTWtracker.ChampionInfoList.Where(x => !x.Hero.IsDead))
            {
                var barPos = hero.Hero.HPBarPosition;
                var q = hero.Hero.Spellbook.GetSpell(SpellSlot.Q);
                var w = hero.Hero.Spellbook.GetSpell(SpellSlot.W);
                var e = hero.Hero.Spellbook.GetSpell(SpellSlot.E);
                var r = hero.Hero.Spellbook.GetSpell(SpellSlot.R);

                if (hero.Hero.IsHPBarRendered && ((SpellTrackerAlly && (hero.Hero.IsAlly && !hero.Hero.IsMe)) ||
                                                  (SpellTrackerEnemy && hero.Hero.IsEnemy) ||
                                                  (SpellTrackerMe && hero.Hero.IsMe)))
                {
                    if (hero.Hero.IsAlly && !hero.Hero.IsMe)
                        barPos = barPos + new Vector2(-50, -30);
                    if (hero.Hero.IsMe)
                        barPos = barPos + new Vector2(-50, -30);
                    if (hero.Hero.IsEnemy)
                        barPos = barPos + new Vector2(-50, -30);

                    Drawing.DrawLine(barPos + new Vector2(7, 35), barPos + new Vector2(115, 35), 12,
                        System.Drawing.Color.DimGray);
                    Drawing.DrawLine(barPos + new Vector2(8, 35), barPos + new Vector2(113, 35), 8,
                        System.Drawing.Color.Black);

                    var qCal = Math.Max(Math.Min((q.CooldownExpires - Game.Time) / q.Cooldown, 1), 0);
                    var wCal = Math.Max(Math.Min((w.CooldownExpires - Game.Time) / w.Cooldown, 1), 0);
                    var eCal = Math.Max(Math.Min((e.CooldownExpires - Game.Time) / e.Cooldown, 1), 0);
                    var rCal = Math.Max(Math.Min((r.CooldownExpires - Game.Time) / r.Cooldown, 1), 0);

                    if (q.Level > 0)
                    {
                        var position = barPos + new Vector2(9, 36);
                        Drawing.DrawLine(position, barPos + new Vector2(33 - (24 * qCal), 36), 5,
                            qCal > 0 ? System.Drawing.Color.Orange : System.Drawing.Color.YellowGreen);
                        if (SpellTrackerLvl)
                            for (int i = 0; i < Math.Min(q.Level, 5); i++)
                                Drawing.DrawLine(barPos + new Vector2(10 + i * 5, 37),
                                    barPos + new Vector2(11 + i * 5, 37), 3, System.Drawing.Color.Black);
                        if (qCal > 0)
                            DrawFontTextScreen(HudCd, MakeNiceNumber(q.CooldownExpires - Game.Time), position.X + 6,
                                position.Y + 7, SharpDX.Color.White);
                    }

                    if (w.Level > 0)
                    {
                        var position = barPos + new Vector2(35, 36);
                        Drawing.DrawLine(position, barPos + new Vector2(59 - (24 * wCal), 36), 5,
                            wCal > 0 ? System.Drawing.Color.Orange : System.Drawing.Color.YellowGreen);
                        if (SpellTrackerLvl)
                            for (int i = 0; i < Math.Min(w.Level, 5); i++)
                                Drawing.DrawLine(barPos + new Vector2(36 + i * 5, 37),
                                    barPos + new Vector2(37 + i * 5, 37), 3, System.Drawing.Color.Black);
                        if (wCal > 0)
                            DrawFontTextScreen(HudCd, MakeNiceNumber(w.CooldownExpires - Game.Time), position.X + 6,
                                position.Y + 7, SharpDX.Color.White);
                    }

                    if (e.Level > 0)
                    {
                        var position = barPos + new Vector2(61, 36);
                        Drawing.DrawLine(position, barPos + new Vector2(85 - (24 * eCal), 36), 5,
                            eCal > 0 ? System.Drawing.Color.Orange : System.Drawing.Color.YellowGreen);
                        if (SpellTrackerLvl)
                            for (int i = 0; i < Math.Min(e.Level, 5); i++)
                                Drawing.DrawLine(barPos + new Vector2(62 + i * 5, 37),
                                    barPos + new Vector2(63 + i * 5, 37), 3, System.Drawing.Color.Black);

                        if (eCal > 0)
                            DrawFontTextScreen(HudCd, MakeNiceNumber(e.CooldownExpires - Game.Time), position.X + 6,
                                position.Y + 7, SharpDX.Color.White);
                    }

                    if (r.Level > 0)
                    {
                        var position = barPos + new Vector2(87, 36);

                        Drawing.DrawLine(position, barPos + new Vector2(112 - (24 * rCal), 36), 5,
                            rCal > 0 ? System.Drawing.Color.Orange : System.Drawing.Color.YellowGreen);
                        if (SpellTrackerLvl)
                            for (int i = 0; i < Math.Min(r.Level, 5); i++)
                                Drawing.DrawLine(barPos + new Vector2(88 + i * 5, 37),
                                    barPos + new Vector2(89 + i * 5, 37), 3, System.Drawing.Color.Black);

                        if (rCal > 0)
                            DrawFontTextScreen(HudCd, MakeNiceNumber(r.CooldownExpires - Game.Time), position.X + 6,
                                position.Y + 7, SharpDX.Color.White);
                    }

                    var sum1 = hero.Hero.Spellbook.Spells[4];
                    var sum2 = hero.Hero.Spellbook.Spells[5];

                    if (sum1 != null)
                    {

                        var sumSprite1 = GetSummonerIconSquare(sum1.Name);

                        var offset = new Vector2(117, 17);
                        if (hero.Hero.IsMe)
                            offset = new Vector2(117, 17);

                        sumSprite1.Position = barPos + offset;


                        var sumTime = sum1.CooldownExpires - Game.Time;
                        if (sumTime < 0)
                        {
                            sumSprite1.Color = new ColorBGRA(System.Drawing.Color.White.ToArgb());
                            sumSprite1.OnEndScene();
                        }
                        else
                        {
                            sumSprite1.Color = new ColorBGRA(System.Drawing.Color.DimGray.ToArgb());
                            sumSprite1.OnEndScene();

                            DrawFontTextScreen(HudCd, MakeNiceNumber(sumTime), sumSprite1.Position.X + 6,
                                sumSprite1.Position.Y + 7, SharpDX.Color.White);
                        }

                    }

                    if (sum2 != null)
                    {
                        var offset = new Vector2(145, 17);
                        if (hero.Hero.IsMe)
                            offset = new Vector2(145, 17);
                        var sumSprite1 = GetSummonerIconSquare(sum2.Name);
                        sumSprite1.Position = barPos + offset;

                        sumSprite1.OnEndScene();
                        var sumTime = sum2.CooldownExpires - Game.Time;

                        if (sumTime < 0)
                        {
                            sumSprite1.Color = new ColorBGRA(System.Drawing.Color.White.ToArgb());
                            sumSprite1.OnEndScene();
                        }
                        else
                        {
                            sumSprite1.Color = new ColorBGRA(System.Drawing.Color.DimGray.ToArgb());
                            sumSprite1.OnEndScene();

                            DrawFontTextScreen(HudCd, MakeNiceNumber(sumTime), sumSprite1.Position.X + 6,
                                sumSprite1.Position.Y + 7, SharpDX.Color.White);
                        }
                    }
                }

                if (hero.Hero.IsMe)
                    continue;

                if (hero.Hero.IsEnemy)
                {
                    if (ShowClicks && hero.Hero.IsValidTarget() && hero.LastWayPoint.IsValid() &&
                        hero.Hero.Position.Distance(hero.LastWayPoint) > 100)
                    {
                        drawLine(hero.Hero.Position, hero.LastWayPoint, 1, System.Drawing.Color.Red);
                        DrawFontTextMap(Tahoma13, hero.Hero.CharacterName, hero.LastWayPoint, SharpDX.Color.WhiteSmoke);
                    }

                    #region Damage indicators

                    if (HpBar && hero.Hero.IsHPBarRendered && hero.Hero.Position.IsOnScreen())
                    {

                        if (HpBar)
                        {
                            var barPoss = hero.Hero.HPBarPosition;
                            if (Math.Abs(barPos.X) < 0.0000001) continue;
                            if (Math.Abs(barPos.Y) < 0.0000001) continue;

                            double qDamage = 0;
                            double wDamage = 0;
                            double eDamage = 0;
                            double rDamage = 0;

                            if (Q.IsReady())
                            {
                                qDamage = Player.GetSpellDamage(hero.Hero, SpellSlot.Q);
                            }

                            if (W.IsReady())
                            {
                                wDamage = Player.GetSpellDamage(hero.Hero, SpellSlot.W);
                            }

                            if (E.IsReady())
                            {
                                eDamage = Player.GetSpellDamage(hero.Hero, SpellSlot.E);
                            }

                            if (R.IsReady())
                            {
                                rDamage = Player.GetSpellDamage(hero.Hero, SpellSlot.R);
                            }

                            var totalDamage = qDamage + wDamage + eDamage + rDamage;
                            var xPos = barPos.X - 45;
                            var yPos = barPos.Y - 19;
                            var remainHelath = hero.Hero.Health - totalDamage;
                            var x1 = xPos + (hero.Hero.Health / hero.Hero.MaxHealth * 104);
                            var x2 = (float) (xPos + ((remainHelath > 0 ? remainHelath : 0) / hero.Hero.MaxHealth * 103.4));
                            Drawing.DrawLine(x1, yPos + 1, x2, yPos + 1, 12, Color.Cyan);
                        }
                    }

                    #endregion

                    if (minimap)
                    {
                        if (!hero.Hero.IsVisible)
                        {
                            var minimapSprite = hero.MinimapSprite;

                            var lastWP = hero.LastWayPoint;
                            var heroPos = hero.PredictedPos;

                            if (!hero.LastWayPoint.IsZero)
                            {
                                if (heroPos.Distance(lastWP) < 1200)
                                    lastWP = heroPos.Extend(lastWP, 1200);
                                Drawing.DrawLine(Drawing.WorldToMinimap(heroPos), Drawing.WorldToMinimap(lastWP), 2,
                                    System.Drawing.Color.OrangeRed);
                            }

                            minimapSprite.Position = Drawing.WorldToMinimap(heroPos) - new Vector2(10, 10);
                            minimapSprite.OnEndScene();
                        }
                    }

                    if (championInfo && championInfoHD)
                    {

                        var hudSprite = hero.HudSprite;

                        if (!hero.Hero.IsVisible)
                            hudSprite.Color = new ColorBGRA(System.Drawing.Color.DimGray.ToArgb());
                        else
                            hudSprite.Color = new ColorBGRA(System.Drawing.Color.White.ToArgb());

                        Vector2 hudPos = new Vector2(posX + hudSpace, posY);
                        float scale = 0.5f;
                        hudSprite.Scale = new Vector2(scale, scale);
                        hudSprite.Position = hudPos - new Vector2(12, 10);
                        hudSprite.OnEndScene();

                        var vec1manaB = new Vector2(hudPos.X - 9, hudPos.Y + 49);
                        var vec2manaB = new Vector2(hudPos.X - 8 + 50 + 3, hudPos.Y + 49);
                        Drawing.DrawLine(vec1manaB, vec2manaB, 19, System.Drawing.Color.DarkGoldenrod);

                        var vec1hpB = new Vector2(hudPos.X - 8, hudPos.Y + 49);
                        var vec2hpB = new Vector2(hudPos.X - 8 + 50 + 2, hudPos.Y + 49);
                        Drawing.DrawLine(vec1hpB, vec2hpB, 16, System.Drawing.Color.Black);

                        System.Drawing.Color color = System.Drawing.Color.LimeGreen;
                        if (hero.Hero.HealthPercent < 30)
                            color = System.Drawing.Color.OrangeRed;
                        else if (hero.Hero.HealthPercent < 50)
                            color = System.Drawing.Color.DarkOrange;
                        var vec1hp = new Vector2(hudPos.X - 7, hudPos.Y + 46);
                        var vec2hp = new Vector2(hudPos.X - 7 + hero.Hero.HealthPercent / 2, hudPos.Y + 46);
                        Drawing.DrawLine(vec1hp, vec2hp, 7, color);

                        var vec1mana = new Vector2(hudPos.X - 7, hudPos.Y + 53);
                        var vec2mana = new Vector2(hudPos.X - 7 + hero.Hero.ManaPercent / 2, hudPos.Y + 53);
                        Drawing.DrawLine(vec1mana, vec2mana, 5, System.Drawing.Color.DodgerBlue);
                        var vecHudLevel = new Vector2(hudPos.X + 30, hudPos.Y + 25);

                        DrawFontTextScreen(HudLevel, hero.Hero.Level.ToString(), vecHudLevel.X, vecHudLevel.Y,
                            SharpDX.Color.White);

                        {
                            if (Game.Time - hero.FinishRecallTime < 4)
                            {
                                DrawFontTextScreen(RecFont, "    FINISH", hudPos.X - 10, hudPos.Y + 18,
                                    SharpDX.Color.YellowGreen);
                            }
                            else if (hero.StartRecallTime <= hero.AbortRecallTime &&
                                     Game.Time - hero.AbortRecallTime < 4)
                            {
                                DrawFontTextScreen(RecFont, "    ABORT", hudPos.X - 10, hudPos.Y + 18,
                                    SharpDX.Color.Yellow);
                            }
                            else if (Game.Time - hero.StartRecallTime < 8)
                            {
                                var recallPercent = (Game.Time - hero.StartRecallTime) / 8;
                                var vec1rec = new Vector2(hudPos.X - 9, hudPos.Y + 35);
                                var vec2rec = new Vector2(hudPos.X - 8 + 50 + 3, hudPos.Y + 35);
                                Drawing.DrawLine(vec1rec, vec2rec, 14, System.Drawing.Color.DarkGoldenrod);

                                vec1rec = new Vector2(hudPos.X - 8, hudPos.Y + 36);
                                vec2rec = new Vector2(hudPos.X - 8 + 50 + 2, hudPos.Y + 36);
                                Drawing.DrawLine(vec1rec, vec2rec, 12, System.Drawing.Color.Black);

                                vec1rec = new Vector2(hudPos.X - 7, hudPos.Y + 36);
                                vec2rec = new Vector2(hudPos.X - 7 + 100 * recallPercent / 2, hudPos.Y + 36);
                                Drawing.DrawLine(vec1rec, vec2rec, 10, System.Drawing.Color.Yellow);

                                if (blink)
                                    DrawFontTextScreen(RecFont, "RECALLING", hudPos.X - 10, hudPos.Y + 18,
                                        SharpDX.Color.White);
                            }
                            else if (!hero.Hero.IsVisible)
                            {
                                DrawFontTextScreen(RecFont, "SS  " + (int) (Game.Time - hero.LastVisableTime), hudPos.X,
                                    hudPos.Y + 18, SharpDX.Color.White);
                            }
                        }

                        var ult = hero.Hero.Spellbook.Spells[3];
                        var sum1 = hero.Hero.Spellbook.Spells[4];
                        var sum2 = hero.Hero.Spellbook.Spells[5];

                        if (ult != null)
                        {
                            var sumTime = ult.CooldownExpires - Game.Time;

                            var spritePos = new Vector2(hudPos.X + 3, hudPos.Y - 53);
                            var vecHudCd = new Vector2(hudPos.X + 10, hudPos.Y - 46);
                            var sumSprite = GetSummonerIcon("r");
                            sumSprite.Position = spritePos;

                            sumSprite.Scale = new Vector2(0.4f, 0.4f);
                            if (hero.Hero.Level < 6)
                            {
                                sumSprite.Color = new ColorBGRA(System.Drawing.Color.DimGray.ToArgb());
                                sumSprite.OnEndScene();
                            }
                            else if (sumTime < 0)
                            {
                                sumSprite.Color = new ColorBGRA(System.Drawing.Color.White.ToArgb());
                                sumSprite.OnEndScene();
                            }
                            else
                            {
                                sumSprite.Color = new ColorBGRA(System.Drawing.Color.DimGray.ToArgb());
                                sumSprite.OnEndScene();
                                DrawFontTextScreen(HudCd, MakeNiceNumber(sumTime), vecHudCd.X, vecHudCd.Y,
                                    SharpDX.Color.White);
                            }
                        }

                        if (sum1 != null)
                        {
                            var sumTime = sum1.CooldownExpires - Game.Time;

                            var vecFlashPos = new Vector2(hudPos.X - 10, hudPos.Y - 30);
                            var vecHudCd = new Vector2(hudPos.X - 4, hudPos.Y - 24);
                            var sumSprite = GetSummonerIcon(sum1.Name);
                            sumSprite.Position = vecFlashPos;
                            sumSprite.Scale = new Vector2(0.4f, 0.4f);

                            if (sumTime < 0)
                            {
                                sumSprite.Color = new ColorBGRA(System.Drawing.Color.White.ToArgb());
                                sumSprite.OnEndScene();
                            }
                            else
                            {
                                sumSprite.Color = new ColorBGRA(System.Drawing.Color.DimGray.ToArgb());
                                sumSprite.OnEndScene();
                                DrawFontTextScreen(HudCd, MakeNiceNumber(sumTime), vecHudCd.X, vecHudCd.Y,
                                    SharpDX.Color.White);
                            }
                        }

                        if (sum2 != null)
                        {
                            var sumTime = sum2.CooldownExpires - Game.Time;

                            var vecHealPos = new Vector2(hudPos.X + 15, hudPos.Y - 30);
                            var vecHudCd = new Vector2(hudPos.X + 22, hudPos.Y - 24);
                            var sumSprite = GetSummonerIcon(sum2.Name);
                            sumSprite.Position = vecHealPos;
                            sumSprite.Scale = new Vector2(0.4f, 0.4f);

                            if (sumTime < 0)
                            {
                                sumSprite.Color = new ColorBGRA(System.Drawing.Color.White.ToArgb());
                                sumSprite.OnEndScene();
                            }
                            else
                            {
                                sumSprite.Color = new ColorBGRA(System.Drawing.Color.DimGray.ToArgb());
                                sumSprite.OnEndScene();
                                DrawFontTextScreen(HudCd, MakeNiceNumber(sumTime), vecHudCd.X, vecHudCd.Y,
                                    SharpDX.Color.White);
                            }
                        }

                        hudSpace += 65;
                    }
                    else if (championInfo)
                    {
                        var hudSprite = hero.HudSprite;

                        if (!hero.Hero.IsHPBarRendered)
                            hudSprite.Color = new ColorBGRA(System.Drawing.Color.DimGray.ToArgb());
                        else
                            hudSprite.Color = new ColorBGRA(System.Drawing.Color.White.ToArgb());

                        Vector2 hudPos = new Vector2(posX + hudSpace, posY);
                        float scale = 0.33f;
                        hudSprite.Scale = new Vector2(scale, scale);
                        hudSprite.Position = hudPos - new Vector2(11, -8);
                        hudSprite.OnEndScene();

                        var vec1manaB = new Vector2(hudPos.X - 9, hudPos.Y + 48);
                        var vec2manaB = new Vector2(hudPos.X - 8 + 33 + 3, hudPos.Y + 48);
                        Drawing.DrawLine(vec1manaB, vec2manaB, 18, System.Drawing.Color.DarkGoldenrod);

                        var vec1hpB = new Vector2(hudPos.X - 8, hudPos.Y + 49);
                        var vec2hpB = new Vector2(hudPos.X - 8 + 33 + 2, hudPos.Y + 49);
                        Drawing.DrawLine(vec1hpB, vec2hpB, 16, System.Drawing.Color.Black);

                        System.Drawing.Color color = System.Drawing.Color.LimeGreen;
                        if (hero.Hero.HealthPercent < 30)
                            color = System.Drawing.Color.OrangeRed;
                        else if (hero.Hero.HealthPercent < 50)
                            color = System.Drawing.Color.DarkOrange;
                        var vec1hp = new Vector2(hudPos.X - 7, hudPos.Y + 50);
                        var vec2hp = new Vector2(hudPos.X - 7 + 33 * hero.Hero.HealthPercent * 0.01f, hudPos.Y + 50);
                        Drawing.DrawLine(vec1hp, vec2hp, 7, color);

                        var vec1mana = new Vector2(hudPos.X - 7, hudPos.Y + 59);
                        var vec2mana = new Vector2(hudPos.X - 7 + 33 * hero.Hero.ManaPercent * 0.01f, hudPos.Y + 59);
                        Drawing.DrawLine(vec1mana, vec2mana, 5, System.Drawing.Color.DodgerBlue);
                        var vecHudLevel = new Vector2(hudPos.X + 15, hudPos.Y + 36);
                        DrawFontTextScreen(HudLevel2, hero.Hero.Level.ToString(), vecHudLevel.X, vecHudLevel.Y,
                            SharpDX.Color.White);
                        {
                            if (Game.Time - hero.FinishRecallTime < 4)
                            {
                                DrawFontTextScreen(HudLevel2, "FINISH", hudPos.X - 9, hudPos.Y + 18,
                                    SharpDX.Color.YellowGreen);
                            }
                            else if (hero.StartRecallTime <= hero.AbortRecallTime &&
                                     Game.Time - hero.AbortRecallTime < 4)
                            {
                                DrawFontTextScreen(HudLevel2, "ABORT", hudPos.X - 9, hudPos.Y + 18,
                                    SharpDX.Color.Yellow);
                            }
                            else if (Game.Time - hero.StartRecallTime < 8)
                            {
                                var recallPercent = (Game.Time - hero.StartRecallTime) / 8;
                                var vec1rec = new Vector2(hudPos.X - 9, hudPos.Y + 35);
                                var vec2rec = new Vector2(hudPos.X - 8 + 33 + 3, hudPos.Y + 35);
                                Drawing.DrawLine(vec1rec, vec2rec, 14, System.Drawing.Color.DarkGoldenrod);

                                vec1rec = new Vector2(hudPos.X - 8, hudPos.Y + 36);
                                vec2rec = new Vector2(hudPos.X - 8 + 33 + 2, hudPos.Y + 36);
                                Drawing.DrawLine(vec1rec, vec2rec, 12, System.Drawing.Color.Black);

                                vec1rec = new Vector2(hudPos.X - 7, hudPos.Y + 37);
                                vec2rec = new Vector2(hudPos.X - 7 + 33 * recallPercent, hudPos.Y + 37);
                                Drawing.DrawLine(vec1rec, vec2rec, 10, System.Drawing.Color.Yellow);

                                if (blink)
                                    DrawFontTextScreen(HudLevel2, "RECALL", hudPos.X - 9, hudPos.Y + 18,
                                        SharpDX.Color.White);

                            }
                            else if (!hero.Hero.IsHPBarRendered)
                            {
                                DrawFontTextScreen(HudLevel2, "SS  " + (int) (Game.Time - hero.LastVisableTime),
                                    hudPos.X - 9, hudPos.Y + 21, SharpDX.Color.White);
                            }
                        }

                        var ult = hero.Hero.Spellbook.Spells[3];
                        var sum1 = hero.Hero.Spellbook.Spells[4];
                        var sum2 = hero.Hero.Spellbook.Spells[5];

                        if (ult != null)
                        {
                            var sumTime = ult.CooldownExpires - Game.Time;

                            var spritePos = new Vector2(hudPos.X - 2, hudPos.Y - 30);
                            var vecHudCd = new Vector2(hudPos.X + 2, hudPos.Y - 24);
                            var sumSprite = GetSummonerIcon("r");
                            sumSprite.Position = spritePos;

                            sumSprite.Scale = new Vector2(0.35f, 0.35f);
                            if (hero.Hero.Level < 6)
                            {
                                sumSprite.Color = new ColorBGRA(System.Drawing.Color.DimGray.ToArgb());
                                sumSprite.OnEndScene();
                            }
                            else if (sumTime < 0)
                            {
                                sumSprite.Color = new ColorBGRA(System.Drawing.Color.White.ToArgb());
                                sumSprite.OnEndScene();
                            }
                            else
                            {
                                sumSprite.Color = new ColorBGRA(System.Drawing.Color.DimGray.ToArgb());
                                sumSprite.OnEndScene();
                                DrawFontTextScreen(HudCd, MakeNiceNumber(sumTime), vecHudCd.X, vecHudCd.Y,
                                    SharpDX.Color.White);
                            }
                        }

                        if (sum1 != null)
                        {
                            var sumTime = sum1.CooldownExpires - Game.Time;

                            var vecFlashPos = new Vector2(hudPos.X - 13, hudPos.Y - 10);
                            var vecHudCd = new Vector2(hudPos.X - 8, hudPos.Y - 5);
                            var sumSprite = GetSummonerIcon(sum1.Name);
                            sumSprite.Position = vecFlashPos;
                            sumSprite.Scale = new Vector2(0.35f, 0.35f);

                            if (sumTime < 0)
                            {
                                sumSprite.Color = new ColorBGRA(System.Drawing.Color.White.ToArgb());
                                sumSprite.OnEndScene();
                            }
                            else
                            {
                                sumSprite.Color = new ColorBGRA(System.Drawing.Color.DimGray.ToArgb());
                                sumSprite.OnEndScene();
                                DrawFontTextScreen(HudCd, MakeNiceNumber(sumTime), vecHudCd.X, vecHudCd.Y,
                                    SharpDX.Color.White);
                            }
                        }

                        if (sum2 != null)
                        {
                            var sumTime = sum2.CooldownExpires - Game.Time;

                            var vecHealPos = new Vector2(hudPos.X + 9, hudPos.Y - 10);
                            var vecHudCd = new Vector2(hudPos.X + 15, hudPos.Y - 5);
                            var sumSprite = GetSummonerIcon(sum2.Name);
                            sumSprite.Position = vecHealPos;
                            sumSprite.Scale = new Vector2(0.35f, 0.35f);

                            if (sumTime < 0)
                            {
                                sumSprite.Color = new ColorBGRA(System.Drawing.Color.White.ToArgb());
                                sumSprite.OnEndScene();
                            }
                            else
                            {
                                sumSprite.Color = new ColorBGRA(System.Drawing.Color.DimGray.ToArgb());
                                sumSprite.OnEndScene();
                                DrawFontTextScreen(HudCd, MakeNiceNumber(sumTime), vecHudCd.X, vecHudCd.Y,
                                    SharpDX.Color.White);
                            }
                        }

                        hudSpace += 45;
                    }
                }

                if (ScreenRadar && !hero.Hero.Position.IsOnScreen() && (!ScreenRadarEnemy || hero.Hero.IsEnemy) &&
                    (!ScreenRadarJungler || hero.IsJungler))
                {
                    var dis = centerScreenWorld.Distance(hero.Hero.Position);
                    if (dis < area)
                    {
                        float scale = 1.1f;

                        if (dis < area)
                            scale -= dis / area;

                        var normalSprite = hero.NormalSprite;

                        Vector2 dupa2 = centerScreen.Extend(Drawing.WorldToScreen(hero.Hero.Position),
                            Drawing.Height / 2 - 120);

                        normalSprite.Scale = new Vector2(scale, scale);

                        if (hero.Hero.IsEnemy)
                        {
                            if (!hero.Hero.IsHPBarRendered)
                            {
                                dupa2 = centerScreen.Extend(Drawing.WorldToScreen(hero.LastWayPoint),
                                    Drawing.Height / 2 - 120);
                                normalSprite.Color = new ColorBGRA(System.Drawing.Color.DimGray.ToArgb());
                            }
                            else
                                normalSprite.Color = new ColorBGRA(System.Drawing.Color.White.ToArgb());
                        }

                        normalSprite.Scale = new Vector2(scale, scale);
                        normalSprite.Position = dupa2;
                        normalSprite.OnEndScene();
                    }
                }
                
            }
        }
        public bool is_on_screen(SharpDX.Vector2 pos)
        {
            return pos.X > 0 && pos.X <= Drawing.Width && pos.Y > 0 && pos.Y <= Drawing.Height;
        }
        
        public System.Drawing.Color GetColorFade(System.Drawing.Color ColorA, System.Drawing.Color ColorB, float Percentage)
        {
            var R1 = ColorA.R;
            var R2 = ColorB.R;
            var G1 = ColorA.G;
            var G2 = ColorB.G;
            var B1 = ColorA.B;
            var B2 = ColorB.B;
            var A1 = ColorA.A;
            var A2 = ColorB.A;

            return System.Drawing.Color.FromArgb((int)((1f - Percentage) * A1 + Percentage * A2), (int)((1f - Percentage) * R1 + Percentage * R2), (int)((1f - Percentage) * G1 + Percentage * G2), (int)((1f - Percentage) * B1 + Percentage * B2));
        }

        private string MakeNiceNumber(float number)
        {
            int num = (int)number;
            if (num < 10)
                return " " + num.ToString();
            else
                return num.ToString();

        }
        
        private Render.Sprite GetSummonerIconSquare(string name)
        {
            var nameToLower = name.ToLower();
            if (nameToLower.Contains("flash"))
                return FlashSquare;
            else if (nameToLower.Contains("heal"))
                return HealSquare;
            else if (nameToLower.Contains("exhaust"))
                return ExhaustSquare;
            else if (nameToLower.Contains("teleport"))
                return TeleportSquare;
            else if (nameToLower.Contains("dot"))
                return IgniteSquare;
            else if (nameToLower.Contains("boost"))
                return CleanseSquare;
            else if (nameToLower.Contains("barrier"))
                return BarrierSquare;
            else if (nameToLower.Contains("haste"))
                return GhostSquare;
            else if (nameToLower.Contains("smite"))
                return SmiteSquare;
            else
                return ClairvoyanceSquare;

        }
        private Render.Sprite GetSummonerIconS(string name)
        {
            var nameToLower = name.ToLower();
            if (nameToLower.Contains("flash"))
                return FlashS;
            else if (nameToLower.Contains("heal"))
                return HealS;
            else if (nameToLower.Contains("exhaust"))
                return ExhaustS;
            else if (nameToLower.Contains("teleport"))
                return TeleportS;
            else if (nameToLower.Contains("dot"))
                return IgniteS;
            else if (nameToLower.Contains("boost"))
                return CleanseS;
            else if (nameToLower.Contains("barrier"))
                return BarrierS;
            else if (nameToLower.Contains("haste"))
                return GhostS;
            else if (nameToLower.Contains("smite"))
                return SmiteS;
            else if (nameToLower.Contains("r"))
                return UltimateS;
            else
                return ClairvoyanceS;

        }

        private Render.Sprite GetSummonerIcon(string name)
        {
            var nameToLower = name.ToLower();
            if (nameToLower.Contains("flash"))
                return Flash;
            else if (nameToLower.Contains("heal"))
                return Heal;
            else if (nameToLower.Contains("exhaust"))
                return Exhaust;
            else if (nameToLower.Contains("teleport"))
                return Teleport;
            else if (nameToLower.Contains("dot"))
                return Ignite;
            else if (nameToLower.Contains("boost"))
                return Cleanse;
            else if (nameToLower.Contains("barrier"))
                return Barrier;
            else if (nameToLower.Contains("haste"))
                return Ghost;
            else if (nameToLower.Contains("smite"))
                return Smite;
            else if (nameToLower.Contains("r"))
                return Ultimate;
            else
                return Clairvoyance;

        }

        private static System.Drawing.Bitmap LoadImg(string imgName)
        {
            var bitmap = Resources.ResourceManager.GetObject(imgName) as System.Drawing.Bitmap;
            if (bitmap == null)
            {
                Console.WriteLine(imgName + ".png not found.");
            }

            return bitmap;
        }

        public static void drawText(string msg, Vector3 Hero, System.Drawing.Color color, int weight = 0)
        {
            var wts = Drawing.WorldToScreen(Hero);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1] + weight, color, msg);
        }

        public static void DrawFontTextScreen(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int) vPosX, (int) vPosY, vColor);
        }

        public static void DrawFontTextMap(Font vFont, string vText, Vector3 Pos, ColorBGRA vColor)
        {
            var wts = Drawing.WorldToScreen(Pos);
            vFont.DrawText(null, vText, (int) wts[0], (int) wts[1], vColor);
        }

        public static void drawLine(Vector3 pos1, Vector3 pos2, int bold, System.Drawing.Color color)
        {
            var wts1 = Drawing.WorldToScreen(pos1);
            var wts2 = Drawing.WorldToScreen(pos2);

            Drawing.DrawLine(wts1[0], wts1[1], wts2[0], wts2[1], bold, color);
        }


        /*class OKTWdraws
    {
        private static Menu Config = Program.Config;
        private static AIHeroClient Player { get { return ObjectManager.Player; } }
        public static Font Tahoma13, Tahoma13B, TextBold;
        public Spell Q, W, E, R;
        private float IntroTimer = Game.Time;
        private Render.Sprite Intro;
        public static Menu ud, eig;

        public void LoadOKTW()
        {
            if (Config.GetValue<MenuBool>("logo").Enabled)
            {
                Intro = new Render.Sprite(LoadImg("intro"),new Vector2((Drawing.Width / 2) - 500, (Drawing.Height / 2) - 350));
                Intro.Add(0);
                Intro.OnPostReset();
            }

            DelayAction.Add(7000, () => Intro.Remove());
            
            ud = Config["utilitydraws"] as Menu;
            
            ud.Add(new MenuBool("disableDraws", "DISABLE UTILITY DRAWS"));
            
            eig = new Menu("enemyinfogrid", "Enemy info grid");
            eig.Add(new MenuBool("championInfo", "Game Info", true));
            eig.Add(new MenuBool("ShowKDA", "Show flash and R CD", true));
            eig.Add(new MenuBool("ShowRecall", "Show recall", true));
            eig.Add(new MenuSlider("posX", "posX", 70, 0, 100));
            eig.Add(new MenuSlider("posY", "posY", 10, 0, 100));
            ud.Add(eig);
            
            ud.Add(new MenuBool("GankAlert", "Gank Alert", true));
            ud.Add(new MenuBool("HpBar", "Dmg indicators BAR OKTW© style", true));
            ud.Add(new MenuBool("ShowClicks", "Show enemy clicks", true));
            ud.Add(new MenuBool("SS", "SS notification", true));
            ud.Add(new MenuBool("RF", "R and Flash notification", true));
            ud.Add(new MenuBool("showWards", "Show hidden objects, wards", true));
            ud.Add(new MenuBool("minimap", "Mini-map hack", true));
            
            Tahoma13B = new Font(Drawing.Direct3DDevice, new FontDescription
                { FaceName = "Tahoma", Height = 14, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });

            Tahoma13 = new Font(Drawing.Direct3DDevice, new FontDescription
                { FaceName = "Tahoma", Height = 14, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });

            TextBold = new Font(Drawing.Direct3DDevice, new FontDescription
                { FaceName = "Impact", Height = 30, Weight = FontWeight.Normal, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });

            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            Drawing.OnEndScene += Drawing_OnEndScene;
            Drawing.OnDraw += Drawing_OnDraw;
        }
        
        private static System.Drawing.Bitmap LoadImg(string imgName)
        {
            var bitmap = Resources.ResourceManager.GetObject(imgName) as System.Drawing.Bitmap;
            if (bitmap == null)
            {
                Console.WriteLine(imgName + ".png not found.");
            }
            return bitmap;
        }

        private void Drawing_OnEndScene(EventArgs args)
        {
            if (!Config["utilitydraws"].GetValue<MenuBool>("disableDraws").Enabled)
                return;

            if (Config["utilitydraws"].GetValue<MenuBool>("minimap").Enabled)
            {
                foreach (var enemy in GameObjects.EnemyHeroes)
                {
                    if (!enemy.IsVisible)
                    {
                        var ChampionInfoOne = OKTWtracker.ChampionInfoList.Find(x => x.NetworkId == enemy.NetworkId);
                        if (ChampionInfoOne != null)
                        {
                            var wts = Drawing.WorldToMinimap(ChampionInfoOne.LastVisiblePos);
                            Program.DrawFontTextScreen(Tahoma13, enemy.CharacterName[0].ToString() + enemy.CharacterName[1].ToString(), wts[0], wts[1], Color.Yellow);
                        }
                    }
                }
            }

            if (Config["utilitydraws"].GetValue<MenuBool>("showWards").Enabled)
            {
                foreach (var obj in OKTWward.HiddenObjList)
                {
                    if (obj.type == 1)
                    {
                        Program.DrawCircle(obj.pos, 100, Color.Yellow, 3, 20, true);
                    }

                    if (obj.type == 2)
                    {
                        Program.DrawCircle(obj.pos, 100, Color.HotPink, 3, 20, true);
                    }

                    if (obj.type == 3)
                    {
                        Program.DrawCircle(obj.pos, 100, Color.Orange, 3, 20, true);
                    }
                }
            }

            var HpBar = Config["utilitydraws"].GetValue<MenuBool>("HpBar").Enabled;
            var Width = 104;
            var Height = 11;
            var XOffset = -45;
            var YOffset = -24;

            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                if (HpBar && enemy.IsHPBarRendered && Program.OnScreen(Drawing.WorldToScreen(enemy.Position)))
                {
                    var barPos = enemy.HPBarPosition;

                    float QdmgDraw = 0, WdmgDraw = 0, EdmgDraw = 0, RdmgDraw = 0, damage = 0;

                    if (Q.IsReady())
                        damage = damage + Q.GetDamage(enemy);

                    if (W.IsReady() && Player.CharacterName != "Kalista")
                        damage = damage + W.GetDamage(enemy);

                    if (E.IsReady())
                        damage = damage + E.GetDamage(enemy);

                    if (R.IsReady())
                        damage = damage + R.GetDamage(enemy);

                    if (Q.IsReady())
                        QdmgDraw = (Q.GetDamage(enemy) / damage);

                    if (W.IsReady() && Player.CharacterName != "Kalista")
                        WdmgDraw = (W.GetDamage(enemy) / damage);

                    if (E.IsReady())
                        EdmgDraw = (E.GetDamage(enemy) / damage);

                    if (R.IsReady())
                        RdmgDraw = (R.GetDamage(enemy) / damage);

                    var percentHealthAfterDamage = Math.Max(0, enemy.Health - damage) / enemy.MaxHealth;

                    var yPos = barPos.Y + YOffset;
                    var xPosDamage = barPos.X + XOffset + Width * percentHealthAfterDamage;
                    var xPosCurrentHp = barPos.X + XOffset + Width * enemy.Health / enemy.MaxHealth;

                    var differenceInHP = xPosCurrentHp - xPosDamage;
                    var pos1 = barPos.X + XOffset + (Width * percentHealthAfterDamage);

                    for (var i = 0; i < differenceInHP; i++)
                    {
                        if (Q.IsReady() && i < QdmgDraw * differenceInHP)
                            Drawing.DrawLine(pos1 + i, yPos, pos1 + i, yPos + Height, 1, System.Drawing.Color.Cyan);
                        else if (W.IsReady() && i < (QdmgDraw + WdmgDraw) * differenceInHP)
                            Drawing.DrawLine(pos1 + i, yPos, pos1 + i, yPos + Height, 1, System.Drawing.Color.Orange);
                        else if (E.IsReady() && i < (QdmgDraw + WdmgDraw + EdmgDraw) * differenceInHP)
                            Drawing.DrawLine(pos1 + i, yPos, pos1 + i, yPos + Height, 1, System.Drawing.Color.Yellow);
                        else if (R.IsReady() && i < (QdmgDraw + WdmgDraw + EdmgDraw + RdmgDraw) * differenceInHP)
                            Drawing.DrawLine(pos1 + i, yPos, pos1 + i, yPos + Height, 1, System.Drawing.Color.YellowGreen);
                    }
                }
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!Config["utilitydraws"].GetValue<MenuBool>("disableDraws").Enabled)
                return;
            if (ud.GetValue<MenuBool>("showWards").Enabled)
            {
                var circleSize = 30;
                foreach (var obj in OKTWward.HiddenObjList.Where(obj => Program.OnScreen(Drawing.WorldToScreen(obj.pos))))
                {
                    if (obj.type == 1)
                    {
                        OktwCommon.DrawTriangleOKTW(circleSize, obj.pos, System.Drawing.Color.Yellow);
                        Program.DrawFontTextMap(Tahoma13, "" + (int)(obj.endTime - Game.Time), obj.pos, SharpDX.Color.Yellow);
                    }

                    if (obj.type == 2)
                    {
                        OktwCommon.DrawTriangleOKTW(circleSize, obj.pos, System.Drawing.Color.HotPink);
                        Program.DrawFontTextMap(Tahoma13, "VW", obj.pos, SharpDX.Color.HotPink);
                    }

                    if (obj.type == 3)
                    {
                        OktwCommon.DrawTriangleOKTW(circleSize, obj.pos, System.Drawing.Color.Orange);
                        Program.DrawFontTextMap(Tahoma13, "! " + (int)(obj.endTime - Game.Time), obj.pos, SharpDX.Color.Orange);
                    }
                }
            }
            var championInfo = Config["utilitydraws"]["enemyinfogrid"].GetValue<MenuBool>("championInfo").Enabled;
            var GankAlert = Config["utilitydraws"].GetValue<MenuBool>("GankAlert").Enabled;
            var ShowKDA = Config["utilitydraws"]["enemyinfogrid"].GetValue<MenuBool>("ShowKDA").Enabled;
            var ShowRecall = Config["utilitydraws"]["enemyinfogrid"].GetValue<MenuBool>("ShowRecall").Enabled;
            var ShowClicks = Config["utilitydraws"].GetValue<MenuBool>("ShowClicks").Enabled;
            var RF = Config["utilitydraws"].GetValue<MenuBool>("RF").Enabled;
            var posX = Config["utilitydraws"]["enemyinfogrid"].GetValue<MenuSlider>("posX").Value * 0.01f * Drawing.Width;
            var posY = Config["utilitydraws"]["enemyinfogrid"].GetValue<MenuSlider>("posY").Value * 0.01f * Drawing.Height;
            var positionDraw = 0f;
            var positionGang = 500f;
            var FillColor = System.Drawing.Color.GreenYellow;
            var Color = System.Drawing.Color.Azure;
            var offset = 0f;
            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                offset += 0.15f;

                if (Config["utilitydraws"].GetValue<MenuBool>("SS").Enabled)
                {
                    if (!enemy.IsVisible && !enemy.IsDead)
                    {
                        var ChampionInfoOne = OKTWtracker.ChampionInfoList.Find(x => x.NetworkId == enemy.NetworkId);
                        if (ChampionInfoOne != null && enemy != Program.jungler)
                        {
                            if ((int)(Game.Time * 10) % 2 == 0 && Game.Time - ChampionInfoOne.LastVisibleTime > 3 && Game.Time - ChampionInfoOne.LastVisibleTime < 7)
                            {
                                Program.DrawFontTextScreen(TextBold, "SS " + enemy.CharacterName + " " + (int)(Game.Time - ChampionInfoOne.LastVisibleTime), Drawing.Width * offset, Drawing.Height * 0.02f, SharpDX.Color.Orange);
                            }
                            if (Game.Time - ChampionInfoOne.LastVisibleTime >= 7)
                            {
                                Program.DrawFontTextScreen(TextBold, "SS " + enemy.CharacterName + " " + (int)(Game.Time - ChampionInfoOne.LastVisibleTime), Drawing.Width * offset, Drawing.Height * 0.02f, SharpDX.Color.OrangeRed);
                            }
                        }
                    }
                }

                if (enemy.IsValidTarget() && ShowClicks)
                {
                    var lastWaypoint = enemy.GetWaypoints().Last().ToVector3();
                    if (lastWaypoint.IsValid())
                    {
                        Program.drawLine(enemy.Position, lastWaypoint, 1, System.Drawing.Color.Red);

                        if (enemy.GetWaypoints().Count() > 1)
                            Program.DrawFontTextMap(Tahoma13, enemy.CharacterName, lastWaypoint, SharpDX.Color.WhiteSmoke);
                    }
                }

                var kolor = System.Drawing.Color.GreenYellow;

                if (enemy.IsDead)
                    kolor = System.Drawing.Color.Gray;
                else if (!enemy.IsVisible)
                    kolor = System.Drawing.Color.OrangeRed;

                var kolorHP = System.Drawing.Color.GreenYellow;

                if (enemy.IsDead)
                    kolorHP = System.Drawing.Color.Gray;
                else if (enemy.HealthPercent < 30)
                    kolorHP = System.Drawing.Color.Red;
                else if (enemy.HealthPercent < 60)
                    kolorHP = System.Drawing.Color.Orange;

                if (championInfo)
                {
                    positionDraw += 15;
                    Program.DrawFontTextScreen(Tahoma13, "" + enemy.Level, posX - 25, posY + positionDraw, SharpDX.Color.White);
                    Program.DrawFontTextScreen(Tahoma13, enemy.CharacterName, posX, posY + positionDraw, SharpDX.Color.White);

                    if (ShowRecall)
                    {
                        var ChampionInfoOne = OKTWtracker.ChampionInfoList.Find(x => x.NetworkId == enemy.NetworkId);
                        if (Game.Time - ChampionInfoOne.FinishRecallTime < 4)
                        {
                            Program.DrawFontTextScreen(Tahoma13, "FINISH", posX - 90, posY + positionDraw, SharpDX.Color.GreenYellow);
                        }
                        else if (ChampionInfoOne.StartRecallTime <= ChampionInfoOne.AbortRecallTime && Game.Time - ChampionInfoOne.AbortRecallTime < 4)
                        {
                            Program.DrawFontTextScreen(Tahoma13, "ABORT", posX - 90, posY + positionDraw, SharpDX.Color.Yellow);
                        }
                        else if (Game.Time - ChampionInfoOne.StartRecallTime < 8)
                        {
                            var recallPercent = (int)((Game.Time - ChampionInfoOne.StartRecallTime) / 8 * 100);
                            var recallX1 = posX - 90;
                            var recallY1 = posY + positionDraw + 6;
                            var recallX2 = (recallX1 + recallPercent / 2) + 1;
                            var recallY2 = posY + positionDraw + 6;
                            Drawing.DrawLine(recallX1, recallY1, recallX1 + 50, recallY2, 8, System.Drawing.Color.Red);
                            Drawing.DrawLine(recallX1, recallY1, recallX2, recallY2, 8, System.Drawing.Color.White);
                        }
                    }

                    var fSlot = enemy.Spellbook.Spells[4];

                    if (fSlot.Name != "SummonerFlash")
                        fSlot = enemy.Spellbook.Spells[5];

                    if (fSlot.Name == "SummonerFlash")
                    {
                        var fT = fSlot.CooldownExpires - Game.Time;
                        if (ShowKDA)
                        {
                            if (fT < 0)
                                Program.DrawFontTextScreen(Tahoma13, "F rdy", posX + 130, posY + positionDraw, SharpDX.Color.GreenYellow);
                            else
                                Program.DrawFontTextScreen(Tahoma13, "F " + (int)fT, posX + 130, posY + positionDraw, SharpDX.Color.Yellow);
                        }
                        if (RF)
                        {
                            if (fT < 2 && fT > -3)
                                Program.DrawFontTextScreen(TextBold, enemy.CharacterName + " FLASH READY!", Drawing.Width * offset, Drawing.Height * 0.1f, SharpDX.Color.Yellow);
                            else if (fSlot.Cooldown - fT < 5)
                                Program.DrawFontTextScreen(TextBold, enemy.CharacterName + " FLASH LOST!", Drawing.Width, Drawing.Height * 0.1f, SharpDX.Color.Red);
                        }
                    }

                    if (enemy.Level > 5)
                    {
                        var rSlot = enemy.Spellbook.Spells[3];
                        var t = rSlot.CooldownExpires - Game.Time;
                        if (ShowKDA)
                        {
                            if (t < 0)
                                Program.DrawFontTextScreen(Tahoma13, "R rdy", posX + 165, posY + positionDraw, SharpDX.Color.GreenYellow);
                            else
                                Program.DrawFontTextScreen(Tahoma13, "R " + (int)t, posX + 165, posY + positionDraw, SharpDX.Color.Yellow);
                        }
                        if (RF)
                        {
                            if (t < 2 && t > -3)
                                Program.DrawFontTextScreen(TextBold, enemy.CharacterName + " R READY!", Drawing.Width * offset, Drawing.Height * 0.2f, SharpDX.Color.YellowGreen);
                            else if (rSlot.Cooldown - t < 5)
                                Program.DrawFontTextScreen(TextBold, enemy.CharacterName + " R LOST!", Drawing.Width * offset, Drawing.Height * 0.1f, SharpDX.Color.Red);
                        }
                    }
                    else if (ShowKDA)
                        Program.DrawFontTextScreen(Tahoma13, "R ", posX + 165, posY + positionDraw, SharpDX.Color.Yellow);
                }

                var Distance = Player.Distance(enemy.Position);

                if (GankAlert && !enemy.IsDead && Distance > 1200)
                {
                    var wts = Drawing.WorldToScreen(Player.Position.Extend(enemy.Position, positionGang));

                    wts[0] = wts[0];
                    wts[1] = wts[1] + 15;

                    if (enemy.HealthPercent > 0)
                        Drawing.DrawLine(wts[0], wts[1], wts[0] + enemy.HealthPercent / 2 + 1, wts[1], 8, kolorHP);

                    if (enemy.HealthPercent < 100)
                        Drawing.DrawLine(wts[0] + enemy.HealthPercent / 2, wts[1], wts[0] + 50, wts[1], 8, System.Drawing.Color.White);

                    if (enemy.IsVisible)
                    {
                        if (Program.jungler.NetworkId == enemy.NetworkId)
                            Program.DrawFontTextMap(Tahoma13B, enemy.CharacterName, Player.Position.Extend(enemy.Position, positionGang), SharpDX.Color.OrangeRed);
                        else
                            Program.DrawFontTextMap(Tahoma13, enemy.CharacterName, Player.Position.Extend(enemy.Position, positionGang), SharpDX.Color.White);
                    }
                    else
                    {
                        var ChampionInfoOne = OKTWtracker.ChampionInfoList.Find(x => x.NetworkId == enemy.NetworkId);
                        if (ChampionInfoOne != null)
                        {
                            if (Game.Time - ChampionInfoOne.LastVisibleTime > 3 && Game.Time - ChampionInfoOne.LastVisibleTime < 7)
                                Program.DrawFontTextMap(Tahoma13, "SS " + enemy.CharacterName + " " + (int)(Game.Time - ChampionInfoOne.LastVisibleTime), Player.Position.Extend(enemy.Position, positionGang), SharpDX.Color.Yellow);
                            else
                                Program.DrawFontTextMap(Tahoma13, "SS " + enemy.CharacterName + " " + (int)(Game.Time - ChampionInfoOne.LastVisibleTime), Player.Position.Extend(enemy.Position, positionGang), SharpDX.Color.YellowGreen);
                        }
                        else
                            Program.DrawFontTextMap(Tahoma13, "SS " + enemy.CharacterName, Player.Position.Extend(enemy.Position, positionGang), SharpDX.Color.LightYellow);
                    }

                    if (Distance < 3500 && enemy.IsVisible && !Program.OnScreen(Drawing.WorldToScreen(enemy.Position)) && Program.jungler != null)
                    {
                        if (Program.jungler.NetworkId == enemy.NetworkId)
                            Program.drawLine(Player.Position.Extend(enemy.Position, 100), Player.Position.Extend(enemy.Position, positionGang - 100), (int)((3500 - Distance) / 280), System.Drawing.Color.Crimson);
                        else
                        {
                            if (enemy.IsFacing(Player))
                                Program.drawLine(Player.Position.Extend(enemy.Position, 100), Player.Position.Extend(enemy.Position, positionGang - 100), (int)((3500 - Distance) / 280), System.Drawing.Color.Orange);
                            else
                                Program.drawLine(Player.Position.Extend(enemy.Position, 100), Player.Position.Extend(enemy.Position, positionGang - 100), (int)((3500 - Distance) / 280), System.Drawing.Color.Gold);
                        }
                    }
                    else if (Distance < 3500 && !enemy.IsVisible && !Program.OnScreen(Drawing.WorldToScreen(Player.Position.Extend(enemy.Position, Distance + 500))))
                    {
                        var need = OKTWtracker.ChampionInfoList.Find(x => x.NetworkId == enemy.NetworkId);
                        if (need != null && Game.Time - need.LastVisibleTime < 5)
                        {
                            Program.drawLine(Player.Position.Extend(enemy.Position, 100), Player.Position.Extend(enemy.Position, positionGang - 100), (int)((3500 - Distance) / 300), System.Drawing.Color.Gray);
                        }
                    }
                }

                positionGang = positionGang + 100;
            }

            if (Program.AIOmode == 2)
            {
                Program.DrawFontTextScreen(TextBold, "OKTW AIO only utility mode ON", Drawing.Width * 0.5f, Drawing.Height * 0.7f, SharpDX.Color.Cyan);
            }
        }
        
    }*/
    }
}