using EnsoulSharp.SDK.MenuUI;

namespace Pentakill_Cassiopeia.Controller
{
    public class MenuController
    {
        private Menu menu;

        public MenuController()
        {
            menu = new Menu("menu","Pentakill Cassiopeia", true);
            Combo();
            Harass();
            LastHit();
            LaneClear();
            Drawings();
            Misc();
        }

        private void Combo()
        {
            Menu comboMenu = menu.Add(new Menu("combo","Combo"));
            comboMenu.Add(new MenuBool("comboUseQ", "Use Q")).SetValue(true);
            comboMenu.Add(new MenuBool("comboUseW", "Use W")).SetValue(true);
            comboMenu.Add(new MenuBool("comboUseE", "Use E")).SetValue(true);
            comboMenu.Add(new MenuBool("comboUseR", "Use R")).SetValue(true);
            //   comboMenu.AddItem(new MenuItem("faceOnlyR", "Only R If Can Stun")).SetValue(true);
            comboMenu.Add(new MenuSlider("minEnemies", "Minimum Enemies for R",2, 1, 5));
            comboMenu.Add(new MenuBool("useIgnite", "Smart Ignite").SetValue(true));
        }

        private void Harass()
        {
            Menu harassMenu = menu.Add(new Menu("harass","Harass"));
            harassMenu.Add(new MenuBool("harassUseQ", "Use Q")).SetValue(true);
            harassMenu.Add(new MenuBool("harassUseW", "Use W")).SetValue(false);
            harassMenu.Add(new MenuBool("harassUseE", "Use E")).SetValue(true);
            harassMenu.Add(new MenuSlider("harassManager", "Minimum Mana %",60, 1, 100));
        }

        private void LastHit()
        {
            Menu lastHitMenu = menu.Add(new Menu("lastHit","Last Hit"));
            lastHitMenu.Add(new MenuBool("lastHitUseQ", "Use Q")).SetValue(true);
            lastHitMenu.Add(new MenuBool("lastHitUseE", "Use E")).SetValue(true);
            lastHitMenu.Add(new MenuSlider("lastHitManager", "Minimum Mana %",50, 1, 100));
        }

        private void LaneClear()
        {
            Menu laneClearMenu = menu.Add(new Menu("laneClear","Lane Clear"));
            laneClearMenu.Add(new MenuBool("laneClearUseQ", "Use Q")).SetValue(true);
            laneClearMenu.Add(new MenuBool("laneClearUseW", "Use W")).SetValue(true);
            laneClearMenu.Add(new MenuBool("laneClearUseE", "Use E")).SetValue(true);
            laneClearMenu.Add(new MenuSlider("laneClearManager", "Minimum Mana %",25, 1, 100));
        }

        private void Drawings()
        {
            Menu drawingsMenu = menu.Add(new Menu("drawings","Drawings"));
            drawingsMenu.Add(new MenuBool("drawQW", "Draw Q/W")).SetValue(true);
            drawingsMenu.Add(new MenuBool("drawE", "Draw E")).SetValue(true);
            drawingsMenu.Add(new MenuBool("drawR", "Draw R")).SetValue(true);
            drawingsMenu.Add(new MenuBool("drawDmg", "Draw Damage")).SetValue(true);
        }

        private void Misc()
        {
            menu.Add(new MenuSlider("eDelay", "E Cast Delay (ms)",75, 1, 1000));
            //menu.Add(new MenuBool("autoLevel", "Auto Level Spells")).SetValue(true);
        }
        

        public Menu getMenu()
        {
            return menu;
        }

        public void addToMainMenu()
        {
            menu.Attach();
        }

    }
}