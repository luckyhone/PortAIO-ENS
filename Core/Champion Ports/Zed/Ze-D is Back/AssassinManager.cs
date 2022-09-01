using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;

namespace zedisback
{
    internal class AssassinManager
    {
        public AssassinManager()
        {
            Load();
        }

        private static void Load()
        {
            var assassinSettings = Program._config.Add(new Menu("MenuAssassin", "Assassin Manager"));
            assassinSettings.Add(new MenuBool("AssassinActive", "Active").SetValue(true));
            assassinSettings.Add(new MenuSeparator("asdfasdf", " "));
            assassinSettings.Add(new MenuList("AssassinSelectOption", "Set: ",new[] {"Single Select", "Multi Select"}));
            assassinSettings.Add(new MenuSeparator("fasda", " "));
            assassinSettings.Add(new MenuBool("AssassinSetClick", "Add/Remove with Right-Click").SetValue(true));
            assassinSettings.Add(new MenuKeyBind("AssassinReset", "Reset List",Keys.L,KeyBindType.Press));

            var draws = assassinSettings.Add(new Menu("Draw", "Draw:"));
            draws.Add(new MenuBool("DrawSearch", "Search Range").SetValue(true)); // Gray
            draws.Add(new MenuBool("DrawActive", "Active Enemy").SetValue(true)); //GreenYellow
            draws.Add(new MenuBool("DrawNearest", "Nearest Enemy").SetValue(true)); // DarlSeaGreen

            var assassinMode = assassinSettings.Add(new Menu("AssassinMode", "Assassin List:"));
            foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
            {
                assassinMode.Add(new MenuBool("Assassin" + enemy.CharacterName, enemy.CharacterName).SetValue(
                            TargetSelector.GetPriority(enemy) > 3));
            }
            assassinSettings.Add(new MenuSlider("AssassinSearchRange", "Search Range",1000,1000, 2000));
            
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
        }
        
        static void ClearAssassinList()
        {
            foreach (
                var enemy in ObjectManager.Get<AIHeroClient>().Where(enemy => enemy.IsEnemy)) 
            {
                
                Program._config["AssassinMode"].GetValue<MenuBool>("Assassin" + enemy.CharacterName).SetValue(false);
                
            }
        }
        private static void OnUpdate(EventArgs args)
        {
        }
        private static void Game_OnWndProc(GameWndEventArgs args)
        {
            if (Program._config.GetValue<MenuKeyBind>("AssassinReset").Active && args.Msg == 257)
            {
                ClearAssassinList();
                Game.Print(
                    "<font color='#FFFFFF'>Reset Assassin List is Complete! Click on the enemy for Add/Remove.</font>");
            }

            if (args.Msg != 0x201)
            {
                return;
            }

            if (Program._config["MenuAssassin"].GetValue<MenuBool>("AssassinSetClick").Enabled)
            {
                foreach (var objAiHero in from hero in ObjectManager.Get<AIHeroClient>()
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
                        var xSelect =
                            Program._config["MenuAssassin"].GetValue<MenuList>("AssassinSelectOption").Index;

                        switch (xSelect)
                        {
                            case 0:
                                ClearAssassinList();
                                Program._config["AssassinMode"].GetValue<MenuBool>("Assassin" + objAiHero.CharacterName).SetValue(true);
                                Game.Print(
                                    string.Format(
                                        "<font color='FFFFFF'>Added to Assassin List</font> <font color='#09F000'>{0} ({1})</font>",
                                        objAiHero.Name, objAiHero.CharacterName));
                                break;
                            case 1:
                                var menuStatus = Program._config.GetValue<MenuBool>("Assassin" + objAiHero.CharacterName).Enabled;
                                Program._config["AssassinMode"].GetValue<MenuBool>("Assassin" + objAiHero.CharacterName).SetValue(!menuStatus);
                                Game.Print(
                                    string.Format("<font color='{0}'>{1}</font> <font color='#09F000'>{2} ({3})</font>",
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
            if (!Program._config.GetValue<MenuBool>("AssassinActive").Enabled)
                return;

            var drawSearch = Program._config["Draw"].GetValue<MenuBool>("DrawSearch").Enabled;
            var drawActive = Program._config["Draw"].GetValue<MenuBool>("DrawActive").Enabled;
            var drawNearest = Program._config["Draw"].GetValue<MenuBool>("DrawNearest").Enabled;

            var drawSearchRange = Program._config.GetValue<MenuSlider>("AssassinSearchRange").Value;
            if (drawSearch)
            {
                CircleRender.Draw(ObjectManager.Player.Position, drawSearchRange, SharpDX.Color.Gray);
            }

            foreach (
                var enemy in
                ObjectManager.Get<AIHeroClient>()
                    .Where(enemy => enemy.Team != ObjectManager.Player.Team)
                    .Where(
                        enemy =>
                            enemy.IsVisible &&
                            Program._config["Assassin" + enemy.CharacterName] != null &&
                            !enemy.IsDead)
                    .Where(
                        enemy => Program._config["AssassinMode"].GetValue<MenuBool>("Assassin" + enemy.CharacterName).Enabled))
            {
                if (ObjectManager.Player.Distance(enemy.Position) < drawSearchRange)
                {
                    if (drawActive)
                        CircleRender.Draw(enemy.Position, 85f, SharpDX.Color.GreenYellow);
                }
                else if (ObjectManager.Player.Distance(enemy.Position) > drawSearchRange &&
                         ObjectManager.Player.Distance(enemy.Position) < drawSearchRange + 400) 
                {
                    if (drawNearest)
                        CircleRender.Draw(enemy.Position, 85f, SharpDX.Color.DarkSeaGreen);
                }
            }
        }
    }
}