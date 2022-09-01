using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using LeagueSharpCommon;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace xSalice_Reworked.Managers
{
    using Base;

    public static class ManaManager
    {
        public static void AddManaManagertoMenu(Menu myMenu, string source, int standart)
        {
            myMenu.Add(new MenuSlider(source + "_Manamanager", "Mana Manager", standart));
        }

        public static bool FullManaCast()
        {
            return ObjectManager.Player.Mana >= SpellManager.QSpell.ManaCost + SpellManager.WSpell.ManaCost + SpellManager.ESpell.ManaCost + SpellManager.RSpell.ManaCost;
        }
        public static bool HasMana(string source)
        {
            return ObjectManager.Player.ManaPercent > Champion.Menu[source + "_Manamanager"].GetValue<MenuSlider>().Value;
        }
    }
}