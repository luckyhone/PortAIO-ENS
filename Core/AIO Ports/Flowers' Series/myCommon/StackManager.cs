using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using LeagueSharpCommon;
using SPrediction;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace ADCCOMMON
{
    using System;
    using System.Linq;

    public static class StackManager
    {
        private static float lastSpellCast;
        private static Menu Menu;

        public static void AddToMenu(Menu mainMenu, bool useQ = true, bool useW = true, bool useE = true)
        {
            Menu = mainMenu;

            mainMenu.Add(new MenuBool("AutoStack", "Auto Stack?", true).SetValue(true));
            mainMenu.Add(new MenuBool("AutoStackQ", "Use Q", true).SetValue(useQ));
            mainMenu.Add(new MenuBool("AutoStackW", "Use W", true).SetValue(useW));
            mainMenu.Add(new MenuBool("AutoStackE", "Use E", true).SetValue(useE));
            mainMenu.Add(
                new MenuSlider("AutoStackMana", "When Player ManaPercent >= x%").SetValue(80));

            Spellbook.OnCastSpell += OnCastSpell;
            Game.OnUpdate += OnUpdate;
        }

        private static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs Args)
        {
            if (sender.Owner.IsMe)
            {
                if (Args.Slot == SpellSlot.Q || Args.Slot == SpellSlot.W || Args.Slot == SpellSlot.E)
                {
                    lastSpellCast = Utils.TickCount;
                }
            }
        }

        private static void OnUpdate(EventArgs Args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling())
            {
                return;
            }

            if (Menu["AutoStack"].GetValue<MenuBool>().Enabled && Orbwalker.ActiveMode == OrbwalkerMode.None && 
                ObjectManager.Player.ManaPercent >= Menu["AutoStackMana"].GetValue<MenuSlider>().Value)
            {
                if (Utils.TickCount - lastSpellCast < 4100)
                {
                    return;
                }

                if (!Items.HasItem(ObjectManager.Player,ItemId.Tear_of_the_Goddess) && !Items.HasItem(ObjectManager.Player,ItemId.Manamune) && !Items.HasItem(ObjectManager.Player,ItemId.Winters_Approach))
                {
                    return;
                }

                if (!HeroManager.Enemies.Any(x => x.DistanceToPlayer() <= 1800) &&
                    !MinionManager.GetMinions(ObjectManager.Player.Position, 1800, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly).Any())
                {
                    if (Menu["AutoStackQ"].GetValue<MenuBool>().Enabled &&
                        ObjectManager.Player.GetSpell(SpellSlot.Q).IsReady() &&
                        Utils.TickCount - lastSpellCast > 4100)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Q, Game.CursorPos);
                        lastSpellCast = Utils.TickCount;
                    }
                    else if (Menu["AutoStackW"].GetValue<MenuBool>().Enabled &&
                             ObjectManager.Player.GetSpell(SpellSlot.W).IsReady() &&
                             Utils.TickCount - lastSpellCast > 4100)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W, Game.CursorPos);
                        lastSpellCast = Utils.TickCount;
                    }
                    else if (Menu["AutoStackE"].GetValue<MenuBool>().Enabled &&
                             ObjectManager.Player.GetSpell(SpellSlot.E).IsReady() &&
                             Utils.TickCount - lastSpellCast > 4100)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(SpellSlot.E, Game.CursorPos);
                        lastSpellCast = Utils.TickCount;
                    }
                }
            }
        }
    }
}
