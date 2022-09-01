using EnsoulSharp;
using EnsoulSharp.SDK;

namespace xSalice_Reworked.Managers
{
    using LeagueSharpCommon;
    using SharpDX;

    public class SummonerManager
    {
        private static readonly SpellSlot IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
        private static readonly SpellSlot FlashSlot = ObjectManager.Player.GetSpellSlot("SummonerFlash");
        private static readonly AIHeroClient Player = ObjectManager.Player;

        public static bool Ignite_Ready()
        {
            return IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready;
        }

        public static bool Flash_Ready()
        {
            return FlashSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready;
        }

        public static void UseFlash(Vector3 pos)
        {
            ObjectManager.Player.Spellbook.CastSpell(FlashSlot, pos);
        }
    }
}