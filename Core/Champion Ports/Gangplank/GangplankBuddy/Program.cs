using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using SharpDX;

namespace GangplankBuddy
{
    class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, FarmingMenu, HealingMenu, DrawingMenu;

        public static void Loads()
        {
            GameEvent.OnGameLoad += Game_OnStart;
        }

        private static void Game_OnStart()
        {
            Menu = new Menu("gangbang","Gangplank",true);
            
            ComboMenu = Menu.Add(new Menu("comboSettings","Combo Settings"));
            //ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add(new MenuSeparator("adfaf","Q Settings"));
            ComboMenu.Add(new MenuBool("useQCombo", ("Use Q on Enemies")));
            ComboMenu.Add(new MenuBool("useQBarrels", ("Use Q on Barrels")));
            ComboMenu.Add(new MenuSeparator("adsfadfs","E Settings"));
            ComboMenu.Add(new MenuBool("useE", ("Use Barrels")));
            ComboMenu.Add(new MenuSlider("useEMaxChain", "Max Barrel Chain", 3, 1, 3));

            HarassMenu = Menu.Add(new Menu("HarassSettings","Harass Settings"));
            //HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add(new MenuSeparator("1212","Q Settings"));
            HarassMenu.Add(new MenuBool("useQHarass", ("Use Q on Enemies")));
            HarassMenu.Add(new MenuBool("useQBarrelsHarass", ("Use Q on Barrels")));
            HarassMenu.Add(new MenuSeparator("234","E Settings"));
            HarassMenu.Add(new MenuBool("useEHarass", ("Use Barrels")));
            HarassMenu.Add(new MenuSlider("useEMaxChainHarass","Max Barrel Chain", 3, 1, 3));

            FarmingMenu = Menu.Add(new Menu("farmsettings","Farming Settings"));
            FarmingMenu.Add(new MenuSeparator("asdf12","Last Hit Settings"));
            FarmingMenu.Add(new MenuBool("useQLastHit", "Use Q Execute"));

            FarmingMenu.Add(new MenuSeparator("adsf234324","WaveClear Settings"));
            FarmingMenu.Add(new MenuBool("useQWaveClear", "Use Q Execute"));
            FarmingMenu.Add(new MenuSeparator("asdfasdf1","Barrel Settings"));
            FarmingMenu.Add(new MenuBool("useEWaveClear", "Use E"));
            FarmingMenu.Add(new MenuSlider("useEWaveClearMin","E Min Units", 3, 1, 8));
            FarmingMenu.Add(new MenuBool("useEQKill", "Use Q on Barrel with Min Killable units"));
            FarmingMenu.Add(new MenuSlider("useEQKillMin", "Min Units", 2, 1, 8));

            HealingMenu = Menu.Add(new Menu("healSettings","Healing Settings"));
            //HealingMenu.AddGroupLabel("Healing Settings");
            HealingMenu.Add(new MenuBool("enableHeal", "Heal with W"));
            HealingMenu.Add(new MenuSlider("healMin", "Min % HP for Heal", 20, 1));
            HealingMenu.Add(new MenuSeparator("chupameloscoco","CC To Heal on"));
            HealingMenu.Add(new MenuBool("healStun", "Stun", false));
            HealingMenu.Add(new MenuBool("healRoot", "Root", false));

            DrawingMenu = Menu.Add(new Menu("drawSettings","Drawing Settings"));
            //DrawingMenu.AddGroupLabel("Drawing Settings");
            DrawingMenu.Add(new MenuBool("drawQ", "Draw Q Range", false));
            DrawingMenu.Add(new MenuBool("drawE", "Draw E Range", false));
            DrawingMenu.Add(new MenuBool("drawKillable", "Draw Killable Barrels", false));
            DrawingMenu.Add(new MenuBool("drawUnKillable", "Draw Un-Killable Barrels", false));

            Menu.Attach();
            BarrelManager.Init();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawingMenu["drawQ"].GetValue<MenuBool>().Enabled)
            {
                CircleRender.Draw(ObjectManager.Player.Position, SpellManager.Q.Range,Color.Wheat);
            }
            if (DrawingMenu["drawE"].GetValue<MenuBool>().Enabled)
            {
                CircleRender.Draw(ObjectManager.Player.Position, SpellManager.E.Range,Color.Wheat);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            StateHandler.Healing();
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                StateHandler.Combo();
            }
            else if (Orbwalker.ActiveMode  == OrbwalkerMode.Harass)
            {
                StateHandler.Harass();
            }
            else if(Orbwalker.ActiveMode  == OrbwalkerMode.LaneClear)
            {
                StateHandler.Waveclear();
            }
            else if (Orbwalker.ActiveMode  == OrbwalkerMode.LastHit)
            {
                StateHandler.LastHit();
            }
        }
    }
}