using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using LeagueSharpCommon;
using SharpDX.Direct3D9;
using Font = SharpDX.Direct3D9.Font;
using KeyBindType = EnsoulSharp.SDK.MenuUI.KeyBindType;
using Keys = EnsoulSharp.SDK.MenuUI.Keys;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using MenuItem = EnsoulSharp.SDK.MenuUI.MenuItem;
using Render = LeagueSharpCommon.Render;

namespace iDZed.Utils
{
    internal class AssassinManager
    {
        private static Font _text;
        private static Font _textBold;

        public AssassinManager()
        {
            Load();
        }

        private static void Load()
        {
            _textBold = new Font(
                Drawing.Direct3DDevice9,
                new FontDescription
                {
                    FaceName = "Tahoma",
                    Height = 13,
                    Weight = FontWeight.Bold,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.ClearType
                });
            _text = new Font(
                Drawing.Direct3DDevice9,
                new FontDescription
                {
                    FaceName = "Tahoma",
                    Height = 13,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.ClearType
                });
            var menuAssasin =Zed.Menu.Add(new Menu( "MenuAssassin",":: Deathmark Priority Targets"));
            menuAssasin.Add(new MenuBool("AssassinActive", "Active").SetValue(true));
            menuAssasin.Add(new MenuSlider("AssassinSearchRange", " Search Range",1400,0, 2000));
            menuAssasin.Add(new MenuList("AssassinSelectOption", " Set:",new[] { "Single Select", "Multi Select" }));
            menuAssasin.Add(new MenuSeparator("xM1", "Enemies:"));
            foreach (AIHeroClient enemy in
                ObjectManager.Get<AIHeroClient>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
            {
                menuAssasin.Add(new MenuBool("Assassin" + enemy.CharacterName, " " + enemy.CharacterName).SetValue(TargetSelector.GetPriority(enemy) > 3));
            }
            
            menuAssasin.Add(new MenuSeparator("xM2", "Other Settings:"));
            menuAssasin.Add(new MenuBool("AssassinSetClick", " Add/Remove with click").SetValue(true));
            menuAssasin.Add(new MenuKeyBind("AssassinReset", " Reset List",Keys.T, KeyBindType.Press));
            var drawMenu = menuAssasin.Add(new Menu("Drawings", "Draw"));
            
            drawMenu.Add(new MenuBool("DrawSearch", "Search Range"));
            drawMenu.Add(new MenuBool("DrawActive", "Active Enemy"));
            drawMenu.Add(new MenuBool("DrawNearest", "Nearest Enemy"));
            drawMenu.Add(new MenuBool("DrawStatus", "Show status on the screen").SetValue(true));
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
        }

        private static void ClearAssassinList()
        {
            foreach (
                var enemy in ObjectManager.Get<AIHeroClient>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
            {
                Zed.Menu["MenuAssassin"]["Assassin" + enemy.CharacterName].GetValue<MenuBool>().SetValue(false);
            }
        }

        private static void OnUpdate(EventArgs args) {}

        public static void DrawText(Font vFont, string vText, float vPosX, float vPosY, SharpDX.ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int) vPosX, (int) vPosY, vColor);
        }

        public static void DrawTextBold(Font vFont, string vText, float vPosX, float vPosY, SharpDX.ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int) vPosX, (int) vPosY, vColor);
        }

        private static void Game_OnWndProc(GameWndEventArgs args)
        {
            if (Zed.Menu["MenuAssassin"]["AssassinReset"].GetValue<MenuKeyBind>().Active && args.Msg == 257)
            {
                ClearAssassinList();
                Game.Print(
                    "<font color='#FFFFFF'>Reset Assassin List is Complete! Click on the enemy for Add/Remove.</font>");
            }
            if (args.Msg != (uint) WindowsMessages.WM_LBUTTONDOWN)
            {
                return;
            }
            if (Zed.Menu["MenuAssassin"]["AssassinSetClick"].GetValue<MenuBool>().Enabled)
            {
                foreach (AIHeroClient objAiHero in from hero in ObjectManager.Get<AIHeroClient>()
                    where hero.IsValidTarget()
                    select hero
                    into h
                    orderby h.Distance(Game.CursorPos) descending
                    select h
                    into enemy
                    where enemy.Distance(Game.CursorPos) < 150f
                    select enemy)
                {
                    if (objAiHero != null && objAiHero.IsVisible && !objAiHero.IsDead)
                    {
                        var xSelect = Zed.Menu["MenuAssassin"]["AssassinSelectOption"].GetValue<MenuList>().Index;
                        switch (xSelect)
                        {
                            case 0:
                                ClearAssassinList();
                                Zed.Menu["MenuAssassin"]["Assassin" + objAiHero.CharacterName].GetValue<MenuBool>().SetValue(true);
                                Game.Print(
                                    string.Format(
                                        "<font color='FFFFFF'>Added to Assassin List</font> <font color='#09F000'>{0} ({1})</font>",
                                        objAiHero.Name, objAiHero.CharacterName));
                                break;
                            case 1:
                                var menuStatus = Zed.Menu["MenuAssassin"]["Assassin" + objAiHero.CharacterName].GetValue<MenuBool>().Enabled;
                                Zed.Menu["MenuAssassin"]["Assassin" + objAiHero.CharacterName].GetValue<MenuBool>().SetValue(!menuStatus);
                                Game.Print(
                                    string.Format(
                                        "<font color='{0}'>{1}</font> <font color='#09F000'>{2} ({3})</font>",
                                        !menuStatus ? "#FFFFFF" : "#FF8877",
                                        !menuStatus ? "Added to Assassin List:" : "Removed from Assassin List:",
                                        objAiHero.Name, objAiHero.CharacterName));
                                break;
                        }
                    }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Zed.Menu["MenuAssassin"]["AssassinActive"].GetValue<MenuBool>().Enabled)
            {
                return;
            }
            if (Zed.Menu["MenuAssassin"]["Drawings"]["DrawStatus"].GetValue<MenuBool>().Enabled)
            {
                var enemies = ObjectManager.Get<AIHeroClient>().Where(xEnemy => xEnemy.IsEnemy);
                var objAiHeroes = enemies as AIHeroClient[] ?? enemies.ToArray();
                DrawText(_textBold, "Target Mode:", Drawing.Width * 0.89f, Drawing.Height * 0.55f, SharpDX.Color.White);
                var xSelect = Zed.Menu["MenuAssassin"]["AssassinSelectOption"].GetValue<MenuList>().Index;
                DrawText(
                    _text, xSelect == 0 ? "Single Target" : "Multi Targets", Drawing.Width * 0.94f,
                    Drawing.Height * 0.55f, SharpDX.Color.White);
                DrawText(
                    _textBold, "Priority Targets", Drawing.Width * 0.89f, Drawing.Height * 0.58f, SharpDX.Color.White);
                DrawText(_textBold, "_____________", Drawing.Width * 0.89f, Drawing.Height * 0.58f, SharpDX.Color.White);
                for (int i = 0; i < objAiHeroes.Count(); i++)
                {
                    var xValue = Zed.Menu["MenuAssassin"]["Assassin" + objAiHeroes[i].CharacterName].GetValue<MenuBool>().Enabled;
                    DrawTextBold(
                        xValue ? _textBold : _text, objAiHeroes[i].CharacterName, Drawing.Width * 0.895f,
                        Drawing.Height * 0.58f + (float) (i + 1) * 15,
                        xValue ? SharpDX.Color.GreenYellow : SharpDX.Color.DarkGray);
                }
            }
            var drawSearch = Zed.Menu["MenuAssassin"]["Drawings"]["DrawSearch"].GetValue<MenuBool>().Enabled;
            var drawActive = Zed.Menu["MenuAssassin"]["Drawings"]["DrawActive"].GetValue<MenuBool>().Enabled;
            var drawNearest = Zed.Menu["MenuAssassin"]["Drawings"]["DrawNearest"].GetValue<MenuBool>().Enabled;
            var drawSearchRange = Zed.Menu["MenuAssassin"]["AssassinSearchRange"].GetValue<MenuSlider>().Value;
            if (drawSearch)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, drawSearchRange, Color.GreenYellow, 1);
            }
            foreach (var enemy in
                ObjectManager.Get<AIHeroClient>()
                    .Where(enemy => enemy.Team != ObjectManager.Player.Team)
                    .Where(
                        enemy =>
                            enemy.IsVisible && Zed.Menu["MenuAssassin"]["Assassin" + enemy.CharacterName]!= null && !enemy.IsDead)
                    .Where(enemy => Zed.Menu["MenuAssassin"]["Assassin" + enemy.CharacterName].GetValue<MenuBool>().Enabled))
            {
                if (ObjectManager.Player.Distance(enemy) < drawSearchRange)
                {
                    if (drawActive)
                    {
                        Render.Circle.DrawCircle(enemy.Position, 115f, Color.GreenYellow, 1);
                    }
                }
                else if (ObjectManager.Player.Distance(enemy) > drawSearchRange &&
                         ObjectManager.Player.Distance(enemy) < drawSearchRange + 400)
                {
                    if (drawNearest)
                    {
                        Render.Circle.DrawCircle(enemy.Position, 115f, Color.DarkSeaGreen, 1);
                    }
                }
            }
        }
    }
}