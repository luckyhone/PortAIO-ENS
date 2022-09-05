using System;
using System.Linq;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace EnsoulSharp.SkinHack
{
    class Program
    {
        public static void Loads()
        {
            GameEvent.OnGameLoad += OnLoad;
        }

        private static Menu menu;

        private static void OnLoad()
        {
            try
            {
                Game.Print("SkinHack v1.0.1");
                Game.Print("Thanks for help 011110001");
                Game.Print("Creator: emredeger");
                
                menu = new Menu("skinhack", "SkinHack", true);

                var champs = menu.Add(new Menu("Champions", "Champions"));
                var allies = champs.Add(new Menu("Allies", "Allies"));
                var enemies = champs.Add(new Menu("Enemies", "Enemies"));

                foreach (var hero in GameObjects.Heroes.Where(h => !h.CharacterName.Equals("Ezreal")))
                {
                    var champMenu = new Menu(hero.CharacterName, hero.CharacterName);
                    champMenu.Add(new MenuSlider("SkinIndex", "Skin Index", 1, 1, 23));
                    champMenu.GetValue<MenuSlider>("SkinIndex").ValueChanged += (s, e) =>
                    {
                        Console.WriteLine($"[SKINHACK] Skin ID: {champMenu.GetValue<MenuSlider>("SkinIndex").Value}");
                        GameObjects.Heroes.ForEach(
                            p => 
                            {
                                if (p.CharacterName == hero.CharacterName)
                                {
                                    Console.WriteLine($"[SKINHACK] Changed: {hero.CharacterName}");
                                    p.SetSkin(champMenu.GetValue<MenuSlider>("SkinIndex").Value);
                                }
                            });
                    };

                    var rootMenu = hero.IsAlly ? allies : enemies;
                    rootMenu.Add(champMenu);
                }

                menu.Attach();
            }
            catch
            {

            }
        }
    }
}