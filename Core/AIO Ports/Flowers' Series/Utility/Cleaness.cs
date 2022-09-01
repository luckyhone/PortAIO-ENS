/* using EnsoulSharp;
 using EnsoulSharp.SDK;
 using EnsoulSharp.SDK.MenuUI;
 using LeagueSharpCommon;
 using Menu = EnsoulSharp.SDK.MenuUI.Menu;

 namespace Flowers_ADC_Series.Utility
{
    using System;
    using System.Collections.Generic;

    internal class Cleaness : Logic
    {
        private static int UseCleanTime, CleanID;

        private static readonly int Dervish = 3137;
        private static readonly int Mercurial = 3139;
        private static readonly int Quicksilver = 3140;
        private static readonly int Mikaels = 3222;

        private static readonly List<BuffType> DebuffTypes = new List<BuffType>();

        private static readonly Menu Menu = Utilitymenu;

        public static void Init()
        {
            var CleanseMenu = Menu.Add(new Menu("Cleanse", "Cleanse"));
            {
                var Debuff = CleanseMenu.Add(new Menu("Debuffs", "Debuffs"));
                {
                    Debuff.Add(new MenuBool("Cleanblind", "Blind", true).SetValue(true));
                    Debuff.Add(new MenuBool("Cleancharm", "Charm", true).SetValue(true));
                    Debuff.Add(new MenuBool("Cleanfear", "Fear", true).SetValue(true));
                    Debuff.Add(new MenuBool("Cleanflee", "Flee", true).SetValue(true));
                    Debuff.Add(new MenuBool("Cleanstun", "Stun", true).SetValue(true));
                    Debuff.Add(new MenuBool("Cleansnare", "Snare", true).SetValue(true));
                    Debuff.Add(new MenuBool("Cleantaunt", "Taunt", true).SetValue(true));
                    Debuff.Add(new MenuBool("Cleansuppression", "Suppression", true).SetValue(true));
                    Debuff.Add(new MenuBool("Cleanpolymorph", "Polymorph", true).SetValue(false));
                    Debuff.Add(new MenuBool("Cleansilence", "Silence", true).SetValue(false));
                    Debuff.Add(new MenuBool("Cleanexhaust", "Exhaust", true).SetValue(true));
                }
                CleanseMenu.Add(new MenuBool("CleanEnable", "Enabled", true).SetValue(true));
                CleanseMenu.Add(new MenuSlider("CleanDelay", "Clean Delay(ms)", 0, 0, 2000));
                CleanseMenu.Add(
                    new MenuSlider("CleanBuffTime", "Debuff Less End Times(ms)", 800, 0, 1000));
                CleanseMenu.Add(new MenuBool("CleanOnlyKey", "Only Combo Mode Active?", true).SetValue(true));
            }

            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs Args)
        {
            if (Me.IsDead)
            {
                return;
            }

            if (Menu["Cleanse"]["CleanEnable"].GetValue<MenuBool>().Enabled)
            {
                if (Menu["CleanOnlyKey"].GetValue<MenuBool>().Enabled &&
                    Orbwalker.ActiveMode != OrbwalkerMode.Combo)
                {
                    return;
                }

                if (CanClean(Me))
                {
                    if (CanUseDervish() || CanUseMercurial() || CanUseMikaels() || CanUseQuicksilver())
                    {
                        Items.UseItem(Me,CleanID);
                        UseCleanTime = Utils.TickCount + 2500;
                    }
                }
            }
        }

        private static bool CanClean(AIHeroClient hero)
        {
            var CanUse = false;

            if (UseCleanTime > Utils.TickCount)
            {
                return false;
            }

            if (Menu["Cleanse"]["Debuffs"]["Cleanblind"].GetValue<MenuBool>().Enabled)
            {
                DebuffTypes.Add(BuffType.Blind);
            }

            if (Menu["Cleanse"]["Debuffs"]["Cleancharm"].GetValue<MenuBool>().Enabled)
            {
                DebuffTypes.Add(BuffType.Charm);
            }

            if (Menu["Cleanse"]["Debuffs"]["Cleanfear"].GetValue<MenuBool>().Enabled)
            {
                DebuffTypes.Add(BuffType.Fear);
            }

            if (Menu["Cleanse"]["Debuffs"]["Cleanflee"].GetValue<MenuBool>().Enabled)
            {
                DebuffTypes.Add(BuffType.Flee);
            }

            if (Menu["Cleanse"]["Debuffs"]["Cleanstun"].GetValue<MenuBool>().Enabled)
            {
                DebuffTypes.Add(BuffType.Stun);
            }

            if (Menu["Cleanse"]["Debuffs"]["Cleansnare"].GetValue<MenuBool>().Enabled)
            {
                DebuffTypes.Add(BuffType.Snare);
            }

            if (Menu["Cleanse"]["Debuffs"]["Cleantaunt"].GetValue<MenuBool>().Enabled)
            {
                DebuffTypes.Add(BuffType.Taunt);
            }

            if (Menu["Cleanse"]["Debuffs"]["Cleansuppression"].GetValue<MenuBool>().Enabled)
            {
                DebuffTypes.Add(BuffType.Suppression);
            }

            if (Menu["Cleanse"]["Debuffs"]["Cleanpolymorph"].GetValue<MenuBool>().Enabled)
            {
                DebuffTypes.Add(BuffType.Polymorph);
            }

            if (Menu["Cleanse"]["Debuffs"]["Cleansilence"].GetValue<MenuBool>().Enabled)
            {
                DebuffTypes.Add(BuffType.Blind);
            }

            if (Menu["Cleanse"]["Debuffs"]["Cleansilence"].GetValue<MenuBool>().Enabled)
            {
                DebuffTypes.Add(BuffType.Silence);
            }

            foreach (var buff in hero.Buffs)
            {
                if (DebuffTypes.Contains(buff.Type) &&
                    (buff.EndTime - Game.Time)*1000 >= Menu["Cleanse"]["CleanBuffTime"].GetValue<MenuSlider>().Value &&
                    buff.IsActive)
                {
                    CanUse = true;
                }
            }

            if (Menu["Cleanse"]["Debuffs"]["Cleanexhaust"].GetValue<MenuBool>().Enabled && hero.HasBuff("CleanSummonerExhaust"))
            {
                CanUse = true;
            }

            UseCleanTime = Utils.TickCount + Menu["Cleanse"]["CleanDelay"].GetValue<MenuSlider>().Value;

            return CanUse;
        }

        private static bool CanUseQuicksilver()
        {
            if (Items.HasItem(Quicksilver) && Items.CanUseItem(Me,Quicksilver))
            {
                CleanID = Quicksilver;
                return true;
            }

            CleanID = 0;
            return false;
        }

        private static bool CanUseMikaels()
        {
            if (Items.HasItem(Mikaels) && Items.CanUseItem(Me,Mikaels))
            {
                CleanID = Mikaels;
                return true;
            }

            CleanID = 0;
            return false;
        }

        private static bool CanUseMercurial()
        {
            if (Items.HasItem(Me,Mercurial) && Items.CanUseItem(Me,Mercurial))
            {
                CleanID = Mercurial;
                return true;
            }

            CleanID = 0;
            return false;
        }

        private static bool CanUseDervish()
        {
            if (Items.HasItem(Me,Dervish) && Items.CanUseItem(Me,Dervish))
            {
                CleanID = Dervish;
                return true;
            }

            CleanID = 0;
            return false;
        }
    }
}
*/