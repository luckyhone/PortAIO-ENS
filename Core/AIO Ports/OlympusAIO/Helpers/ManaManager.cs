using EnsoulSharp.SDK.MenuUI;

namespace OlympusAIO.Helpers
{
    class ManaManager
    {
        public static bool HaveNoEnoughMana(MenuSlider menu)
        {
            if (OlympusAIO.MainMenu["DisableManaManagerIfBlueBuff"].GetValue<MenuBool>().Enabled && OlympusAIO.objPlayer.HasBuff("crestoftheancientgolem"))
            {
                return false;
            }

            return OlympusAIO.objPlayer.ManaPercent < menu.GetValue<MenuSlider>().Value;
        }
    }
}
