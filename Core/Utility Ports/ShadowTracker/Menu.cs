using System;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;

namespace ShadowTracker
{
    class MainMenu
    {
        public static Menu _MainMenu;        
        public static void Menu()
        {
            try // try start
            {
                _MainMenu = new Menu("ShadowTracker", "ShadowTracker", true);
                _MainMenu.Attach();

                var Draw = new Menu("Draw", "Draw");
                {
                    Draw.Add(new MenuBool("Skill", "Skill").SetValue(true));
                    Draw.Add(new MenuBool("Spell", "Spell").SetValue(true));
                    Draw.Add(new MenuBool("Item", "Item").SetValue(true));
                }
                _MainMenu.Add(Draw);                
            } // try end     
            catch (Exception e)
            {
                Console.Write(e);
                Game.Print("ShadowTracker is not working. plz send message by KorFresh (Code 1)");
            }           
            
        }
    }
}