using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

namespace ADCCOMMON
{
    using System;

    public static class LevelsManager
    {
        private static Menu levelMenu;

        public static void AddToMenu(Menu mainMenu)
        {
            levelMenu = mainMenu;

            mainMenu.Add(new MenuBool("LevelsEnable", "Enabled", true).SetValue(false));
            mainMenu.Add(new MenuBool("LevelsAutoR", "Auto Level R", true).SetValue(true));
            mainMenu.Add(
                new MenuSlider("LevelsDelay", "Auto Level Delays", 700, 0, 2000));
            mainMenu.Add(
                new MenuSlider("LevelsLevels", "When Player Level >= Enable!", 3, 1, 6));
            mainMenu.Add(
                new MenuList("LevelsMode", "Mode: ", new[]
                        {"Q -> W -> E", "Q -> E -> W", "W -> Q -> E", "W -> E -> Q", "E -> Q -> W", "E -> W -> Q"}));

            AIHeroClient.OnLevelUp += OnLevelUp;
        }

        private static void OnLevelUp(AIHeroClient sender, EventArgs Args)
        {
            if (!sender.IsMe || !levelMenu["LevelsEnable"].GetValue<MenuBool>().Enabled)
            {
                return;
            }

            if (levelMenu["LevelsAutoR"].GetValue<MenuBool>().Enabled && (ObjectManager.Player.Level == 6 || ObjectManager.Player.Level == 11 || ObjectManager.Player.Level == 16))
            {
                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
            }

            if (ObjectManager.Player.Level >= levelMenu["LevelsLevels"].GetValue<MenuSlider>().Value)
            {
                int Delay = levelMenu["LevelsDelay"].GetValue<MenuSlider>().Value;

                if (ObjectManager.Player.Level < 3)
                {
                    switch (levelMenu["LevelsMode"].GetValue<MenuList>().Index)
                    {
                        case 0:
                            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                            break;
                        case 1:
                            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                            break;
                        case 2:
                            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                            break;
                        case 3:
                            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                            break;
                        case 4:
                            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                            break;
                        case 5:
                            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                            break;
                    }
                }
                else if (ObjectManager.Player.Level > 3)
                {
                    switch (levelMenu["LevelsMode"].GetValue<MenuList>().Index)
                    {
                        case 0:
                            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);

                            //Q -> W -> E
                            DelayLevels(Delay, SpellSlot.Q);
                            DelayLevels(Delay + 50, SpellSlot.W);
                            DelayLevels(Delay + 100, SpellSlot.E);
                            break;
                        case 1:
                            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);

                            //Q -> E -> W
                            DelayLevels(Delay, SpellSlot.Q);
                            DelayLevels(Delay + 50, SpellSlot.E);
                            DelayLevels(Delay + 100, SpellSlot.W);
                            break;
                        case 2:
                            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);

                            //W -> Q -> E
                            DelayLevels(Delay, SpellSlot.W);
                            DelayLevels(Delay + 50, SpellSlot.Q);
                            DelayLevels(Delay + 100, SpellSlot.E);
                            break;
                        case 3:
                            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);

                            //W -> E -> Q
                            DelayLevels(Delay, SpellSlot.W);
                            DelayLevels(Delay + 50, SpellSlot.E);
                            DelayLevels(Delay + 100, SpellSlot.Q);
                            break;
                        case 4:
                            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);

                            //E -> Q -> W
                            DelayLevels(Delay, SpellSlot.E);
                            DelayLevels(Delay + 50, SpellSlot.Q);
                            DelayLevels(Delay + 100, SpellSlot.W);
                            break;
                        case 5:
                            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);

                            //E -> W -> Q
                            DelayLevels(Delay, SpellSlot.E);
                            DelayLevels(Delay + 50, SpellSlot.W);
                            DelayLevels(Delay + 100, SpellSlot.Q);
                            break;
                    }
                }
            }
        }

        private static void DelayLevels(int time, SpellSlot slot)
        {
            DelayAction.Add(time, () => ObjectManager.Player.Spellbook.LevelSpell(slot));
        }
    }
}
