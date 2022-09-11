using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using HikiCarry.Core.Activator;
using DynamicInitializer = HikiCarry.Core.Plugins.DynamicInitializer;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace HikiCarry
{
    internal static class Initializer
    {

        internal static Menu Config;
        internal static Menu Activator;
        internal static AIHeroClient Player = ObjectManager.Player;

        public static string[] HitchanceNameArray = { "Low", "Medium", "High", "Very High", "Only Immobile" };
        public static HitChance[] HitchanceArray = { HitChance.Low, HitChance.Medium, HitChance.High, HitChance.VeryHigh, HitChance.Immobile };

        public static string[] SupportedChampions =
        {
            "Ashe", "Caitlyn", "Draven" , "Ezreal" , "Jhin" ,"Jinx", "Kalista" , "Lucian",
                "Quinn", "Sivir", "Tristana", "Varus", "Vayne"
        };

        public static Cleanser Cleanser = new Cleanser();
        public static Youmuu Youmuu = new Youmuu();


        public static void Load()
        {
            try
            {
                if (SupportedChampions.Contains(ObjectManager.Player.CharacterName))
                {
                    Game.Print("<font color = \"#FFFF33\">HikiCarry: " + 
                        ObjectManager.Player.CharacterName + " Loaded</font>");
                }

                else
                {
                    Game.Print("<font color = \"#FFFF33\">HikiCarry: " + 
                        ObjectManager.Player.CharacterName + " Not Supported </font>");
                }

                Config = new Menu("HikiCarry","HikiCarry: " + Player.CharacterName, true); 
                {


                    var type = Type.GetType("HikiCarry.Champions." + Player.CharacterName);
                    if (type != null)
                    {
                        DynamicInitializer.NewInstance(type);
                    }

                    if (ObjectManager.Player.CharacterName != "Vayne" || ObjectManager.Player.CharacterName != "Tristana")
                    {
                        Config.Add(
                            new MenuList("hitchance", "Hit Chance", HitchanceNameArray, 0));
                        Config["hitchance"].SetFontColor(SharpDX.Color.Gold);
                        Config.Add(new MenuList("prediction", "Hit Chance", new[] { "Common", "Sebby", "sPrediction", "SDK" }, 0));
                        Config["prediction"].SetFontColor(SharpDX.Color.Gold);

                        if (Config["prediction"].GetValue<MenuList>().Index == 2)
                        {
                            SPrediction.Prediction.Initialize(Config, ":: SPREDICTION");
                        }
                    }

                    Config.Add(
                        new MenuSeparator("credits.x1", "                          Developed by Hikigaya"));
                    Config["credits.x1"].SetFontColor(SharpDX.Color.DodgerBlue);

                    Config.Attach();
                }
                Config.SetFontColor(SharpDX.Color.GreenYellow);

                Activator = new Menu("HikiCarry: Activator", "HikiCarry: Activator", true);
                {
                    

                    var youmuu = new Menu("Youmuu Settings", "Youmuu Settings");
                    {
                        youmuu.Add(new MenuBool("youmuu", "Use (Youmuu)").SetValue(true));
                        Activator.Add(youmuu);
                    }

                    var cleanse = new Menu("Cleanse Settings", "Cleanse Settings");
                    {
                        var cleansedebuffs = new Menu("Cleanse Debuffs", "Cleanse Debuffs");
                        {
                            cleansedebuffs.Add(new MenuBool("qss.charm", "Charm").SetValue(true));
                            cleansedebuffs.Add(new MenuBool("qss.flee", "Flee").SetValue(true));
                            cleansedebuffs.Add(new MenuBool("qss.snare", "Snare").SetValue(true));
                            cleansedebuffs.Add(new MenuBool("qss.polymorph", "Polymorph").SetValue(true));
                            cleansedebuffs.Add(new MenuBool("qss.stun", "Stun").SetValue(true));
                            cleansedebuffs.Add(new MenuBool("qss.suppression", "Suppression").SetValue(true));
                            cleansedebuffs.Add(new MenuBool("qss.taunt", "Taunt").SetValue(true));
                            cleanse.Add(cleansedebuffs);
                        }
                        cleanse.Add(new MenuBool("use.cleanse", "Use Cleanser Item").SetValue(true));
                        cleanse.Add(new MenuSlider("cleanse.delay", "Max. Cleanse Delay",1000, 1, 2500));
                        Activator.Add(cleanse);
                    }

                    Activator.Attach();
                }
                Activator.SetFontColor(SharpDX.Color.GreenYellow);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}