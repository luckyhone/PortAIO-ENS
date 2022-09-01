using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace OneKeyToWin_AIO_Sebby
{
    internal class Program
    {
        private static string OktNews = "First port release";

        public static Menu Config;

        public static AIHeroClient Player { get { return ObjectManager.Player; } }

        public static Spell Q, W, E, R, Q1, W1, E1, R1;
        public static float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;

        public static float JungleTime, DrawSpellTime = 0;
        public static AIHeroClient jungler = ObjectManager.Player;
        public static int timer, HitChanceNum = 4, tickNum = 4, tickIndex = 0;
        public static Obj_SpawnPoint enemySpawn;
        public static PredictionOutput DrawSpellPos;
        public static bool Combo = false, Farm = false, Harass = false, LaneClear = false, None = false;

        
        public static bool SPredictionLoad = false;
        public static int AIOmode = 0;
        private static float dodgeRange = 420;
        private static float dodgeTime = Game.Time;
        private static float spellFarmTimer = 0;
        private static Font TextBold;
        
        public static void debug(string msg)
        {
            if (Config["aboutoktw"].GetValue<MenuBool>("debug").Enabled)
            {
                Console.WriteLine(msg);
            }
            if (Config["aboutoktw"].GetValue<MenuBool>("debugChat").Enabled)
            {
                Game.Print(msg);
            }
        }
        public static void GameOnOnGameLoad()
        {
            TextBold = new Font(Drawing.Direct3DDevice9, new FontDescription
                { FaceName = "Impact", Height = 30, Weight = FontWeight.Normal, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });

            enemySpawn = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy);
            Q = new Spell(SpellSlot.Q);
            E = new Spell(SpellSlot.E);
            W = new Spell(SpellSlot.W);
            R = new Spell(SpellSlot.R);
            
            Config = new Menu("OneKeyToWin_AIO", "OneKeyToWin AIO " + Player.CharacterName, true);
            Config.SetFontColor(Color.DeepSkyBlue.ToSharpDxColor());
            
            #region MENU ABOUT OKTW

            var about = new Menu("aboutoktw", "About OKTW©");
            about.Add(new MenuBool("debug", "Debug"));
            about.Add(new MenuBool("debugChat", "Debug Chat"));
            about.Add(new Menu("0", "OneKeyToWin© by Sebby"));
            about.Add(new Menu("1", "visit joduska.me"));
            about.Add(new Menu("2", "DONATE: kaczor.sebastian@gmail.com"));
            about.Add(new MenuBool("print", "OKTW NEWS in chat", true));
            about.Add(new MenuBool("logo", "Intro logo OKTW").SetValue(true));
            Config.Add(about);

            #endregion

            Config.Add(new MenuList("AIOmode", "AIO mode",
                new[] {"Utility and Champion", "Only Champion", "Only Utility"}, 0));

            AIOmode = Config.GetValue<MenuList>("AIOmode").Index;

            if (AIOmode != 1)
            {
            }
            
            var predictionmode = new Menu("predictionmode", "Prediction Mode");
            predictionmode.Add(new MenuList("QHitChance", "Q Hit Chance", new[] {"Very High", "High", "Medium"}, 0));
            predictionmode.Add(new MenuList("WHitChance", "W Hit Chance", new[] {"Very High", "High", "Medium"}, 0));
            predictionmode.Add(new MenuList("EHitChance", "E Hit Chance", new[] {"Very High", "High", "Medium"}, 0));
            predictionmode.Add(new MenuList("RHitChance", "R Hit Chance", new[] {"Very High", "High", "Medium"}, 0));
            Config.Add(predictionmode);

            if (AIOmode != 2)
            {
                #region LOAD CHAMPIONS

                switch (Player.CharacterName)
                {
                    case "Ahri":
                        new Champions.Ahri();
                        break;
                    case "Annie":
                        new Champions.Annie();
                        break;
                    case "Ashe":
                        new Champions.Ashe();
                        break;
                    case "Caitlyn":
                        new Champions.Caitlyn();
                        break;
                    case "Corki":
                        new Champions.Corki();
                        break;
                    case "Darius":
                        new Champions.Darius();
                        break;
                    case "Ekko":
                        new Champions.Ekko();
                        break;
                    case "Ezreal":
                        new Champions.Ezreal();
                        break;
                    case "Graves":
                        new Champions.Graves();
                        break;
                    case "Jayce":
                        new Champions.Jayce();
                        break;
                    case "Jhin":
                        new Champions.Jhin();
                        break;
                    case "Jinx":
                        new Champions.Jinx();
                        break;
                    case "Kalista":
                        new Champions.Kalista();
                        break;
                    case "Karthus":
                        new Champions.Karthus();
                        break;
                    case "Kindred":
                        new Champions.Kindred();
                        break;
                    case "Lucian":
                        new Champions.Lucian();
                        break;
                    case "Lux":
                        new Champions.Lux();
                        break;
                    case "Varus":
                        new Champions.Varus();
                        break;
                    case "Vayne":
                        new Champions.Vayne();
                        break;
                    case "Thresh":
                        new Champions.Thresh();
                        break;
                    case "Sivir":
                        new Champions.Sivir();
                        break;
                    case "Syndra":
                        new Champions.Syndra();
                        break;
                }
                #endregion
                
                var player = Config[Player.CharacterName] as Menu;
                if (player == null)
                {
                    player = (new Menu(Player.CharacterName, Player.CharacterName));
                    player.SetFontColor(SharpDX.Color.Orange);
                    Config.Add(player);
                }
                var farm = player["farm"] as Menu;
                if (farm == null)
                {
                    farm = new Menu("farm", "Farm");
                    player.Add(farm);
                }
            }
            

            foreach (var hero in GameObjects.EnemyHeroes)
            {
                if (IsJungler(hero))
                {
                    jungler = hero;
                }
            }

            if (AIOmode != 1)
            {
                new Core.OKTWdraws().LoadOKTW();
                new Core.OKTWtracker().LoadOKTW();
                new Core.OKTWward().LoadOKTW();
                new Core.AutoLvlUp().LoadOKTW();
            }
            Config.Add(new Menu("aiomodes", "!!! PRESS F5 TO RELOAD MODE !!!"));
            Config.Attach();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            if (Config["aboutoktw"].GetValue<MenuBool>("print").Enabled)
            {
                Game.Print("<font size='30'>OneKeyToWin</font> <font color='#b756c5'>by Sebby</font>");
                Game.Print("<font color='#b756c5'>OKTW NEWS: </font>" + OktNews);
            }
        }
        
        public static void drawText(string msg, Vector3 Hero, System.Drawing.Color color, int weight = 0)
        {
            var wts = Drawing.WorldToScreen(Hero);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1] + weight, color, msg);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            /*if (Config["utilitydraws"]["ganktimer"].GetValue<MenuBool>("enabled").Enabled && jungler != null)
            {
                if (jungler == Player)
                    drawText("Jungler not detected", Player.Position, System.Drawing.Color.Yellow, 100);
                else if (jungler.IsDead)
                    drawText("Jungler dead " + timer, Player.Position, System.Drawing.Color.Cyan, 100);
                else if (jungler.IsVisible)
                    drawText("Jungler visible " + timer, Player.Position, System.Drawing.Color.GreenYellow, 100);
                else
                {
                    if (timer > 0)
                        drawText("Junger in jungle " + timer, Player.Position, System.Drawing.Color.Orange, 100);
                    else if ((int)(Game.Time * 10) % 2 == 0)
                        drawText("BE CAREFUL " + timer, Player.Position, System.Drawing.Color.OrangeRed, 100);
                    if (Game.Time - JungleTime >= 1)
                    {
                        timer = timer - 1;
                        JungleTime = Game.Time;
                    }
                }
            }*/
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (AIOmode == 2)
            {
                if (Player.IsMoving)
                    Combo = true;
                else
                    Combo = false;
            }
            else
            {
                Combo = Orbwalker.ActiveMode == OrbwalkerMode.Combo;

                if ((Config["extra"] as Menu)?.GetValue<MenuBool>("harassMixed").Enabled ?? false)
                    Harass = Orbwalker.ActiveMode == OrbwalkerMode.Harass;
                else
                    Harass = Orbwalker.ActiveMode == OrbwalkerMode.LaneClear ||
                             Orbwalker.ActiveMode == OrbwalkerMode.Harass;

                Farm = Orbwalker.ActiveMode == OrbwalkerMode.LaneClear || Orbwalker.ActiveMode == OrbwalkerMode.Harass;
                None = Orbwalker.ActiveMode == OrbwalkerMode.None;
                LaneClear = Orbwalker.ActiveMode == OrbwalkerMode.LaneClear;
            }

            tickIndex++;

            if (tickIndex > 4)
                tickIndex = 0;

            if (!LagFree(0))
                return;
        }
        private static bool IsJungler(AIHeroClient hero)
        {
            return hero.Spellbook.Spells.Any(spell => spell.Name.ToLower().Contains("smite"));
        }
        public static void DrawFontTextMap(Font vFont, string vText, Vector3 Pos, ColorBGRA vColor)
        {
            var wts = Drawing.WorldToScreen(Pos);
            vFont.DrawText(null, vText, (int)wts[0] , (int)wts[1], vColor);
        }

        public static bool OnScreen(Vector2 point)
        {
            return point.X > 0 && point.Y > 0 && point.X < Drawing.Width && point.Y < Drawing.Height;
        }
        public static bool LagFree(int offset)
        {
            return (tickIndex == offset);
        }
        public  static void DrawFontTextScreen(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
        }
        public static void drawLine(Vector3 pos1, Vector3 pos2, int bold, System.Drawing.Color color)
        {
            var wts1 = Drawing.WorldToScreen(pos1);
            var wts2 = Drawing.WorldToScreen(pos2);

            Drawing.DrawLine(wts1[0], wts1[1], wts2[0], wts2[1], bold, color);
        }
        public static void CastSpell(Spell qwer, AIBaseClient target)
        {
            var hitChance = HitChance.High;

            if (qwer.Slot == SpellSlot.Q)
            {
                //hitChance = (HitChance)(4 - QHitChance.Index);
                hitChance = (HitChance)(4 - Config["predictionmode"].GetValue<MenuList>("QHitChance").Index);
            }
            else if (qwer.Slot == SpellSlot.W)
            {
                //hitChance = (HitChance)(4 - WHitChance.Index);
                hitChance = (HitChance)(4 - Config["predictionmode"].GetValue<MenuList>("WHitChance").Index);
            }
            else if (qwer.Slot == SpellSlot.E)
            {
                //hitChance = (HitChance)(4 - EHitChance.Index);
                hitChance = (HitChance)(4 - Config["predictionmode"].GetValue<MenuList>("EHitChance").Index);
            }
            else if (qwer.Slot == SpellSlot.R)
            {
                //hitChance = (HitChance)(4 - RHitChance.Index);
                hitChance = (HitChance)(4 - Config["predictionmode"].GetValue<MenuList>("RHitChance").Index);
            }

            qwer.CastIfHitchanceMinimum(target, hitChance);
        }
        
        public static void DrawCircle(Vector3 center, float radius, SharpDX.Color color, int thickness =5,int quality = 30,bool onMinimap = false)
        {
            if (!onMinimap)
            {
                CircleRender.Draw(center, radius, color, thickness);
                return;
            }

            var pointList = new List<Vector3>();
            for (var i = 0; i < quality; i++)
            {
                var angle = i * Math.PI * 2 / quality;
                pointList.Add(
                    new Vector3(
                        center.X + radius * (float)Math.Cos(angle),
                        center.Y + radius * (float)Math.Sin(angle),
                        center.Z));
            }

            for (var i = 0; i < pointList.Count; i++)
            {
                var a = pointList[i];
                var b = pointList[i == pointList.Count - 1 ? 0 : i + 1];

                var aonScreen = Drawing.WorldToMinimap(a);
                var bonScreen = Drawing.WorldToMinimap(b);

                Drawing.DrawLine(aonScreen.X, aonScreen.Y, bonScreen.X, bonScreen.Y, thickness, color.ToSystemColor());
            }
        }
    }
    
    
}