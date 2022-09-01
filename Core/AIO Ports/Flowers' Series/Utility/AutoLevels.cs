 /*using EnsoulSharp;
 using EnsoulSharp.SDK.MenuUI;
 using EnsoulSharp.SDK.Utility;

 namespace Flowers_ADC_Series.Utility
{
    using System;

    internal class AutoLevels : Logic
    {
        private static readonly Menu Menu = Utilitymenu;

        public static void Init()
        {
            var AutoLevelMenu = Menu.Add(new Menu("Auto Levels", "Auto Levels"));
            {
                AutoLevelMenu.Add(new MenuBool("LevelsEnable", "Enabled", true).SetValue(false));
                AutoLevelMenu.Add(new MenuBool("LevelsAutoR", "Auto Level R", true).SetValue(true));
                AutoLevelMenu.Add(
                    new MenuSlider("LevelsDelay", "Auto Level Delays",700, 0, 2000));
                AutoLevelMenu.Add(
                    new MenuSlider("LevelsLevels", "When Player Level >= Enable!",3, 1, 6));
                AutoLevelMenu.Add(
                    new MenuList("LevelsMode", "Mode: ",new[]
                            {"Q -> W -> E", "Q -> E -> W", "W -> Q -> E", "W -> E -> Q", "E -> Q -> W", "E -> W -> Q"}));
            }

            AIHeroClient.OnLevelUp += OnLevelUp;
        }
        
        private static void OnLevelUp(AIHeroClient sender, AIHeroClientLevelUpEventArgs Args)
        {
            if (!sender.IsMe || !Menu["LevelsEnable"].GetValue<MenuBool>().Enabled)
            {
                return;
            }

            if (Menu["LevelsAutoR"].GetValue<MenuBool>().Enabled && (Me.Level == 6 || Me.Level == 11 || Me.Level == 16))
            {
                Me.Spellbook.LevelSpell(SpellSlot.R);
            }

            if (Me.Level >= Menu["LevelsLevels"].GetValue<MenuSlider>().Value)
            {
                int Delay = Menu["LevelsDelay"].GetValue<MenuSlider>().Value;

                if (Me.Level < 3)
                {
                    switch (Menu["LevelsMode"].GetValue<MenuList>().Index)
                    {
                        case 0:
                            if (Me.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.Q);
                            else if (Me.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.W);
                            else if (Me.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.E);
                            break;
                        case 1:
                            if (Me.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.Q);
                            else if (Me.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.E);
                            else if (Me.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.W);
                            break;
                        case 2:
                            if (Me.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.W);
                            else if (Me.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.Q);
                            else if (Me.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.E);
                            break;
                        case 3:
                            if (Me.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.W);
                            else if (Me.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.E);
                            else if (Me.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.Q);
                            break;
                        case 4:
                            if (Me.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.E);
                            else if (Me.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.Q);
                            else if (Me.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.W);
                            break;
                        case 5:
                            if (Me.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.E);
                            else if (Me.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.W);
                            else if (Me.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.Q);
                            break;
                    }
                }
                else if (Me.Level > 3)
                {
                    switch (Menu["LevelsMode"].GetValue<MenuList>().Index)
                    {
                        case 0:
                            if (Me.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.Q);
                            else if (Me.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.W);
                            else if (Me.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.E);

                            //Q -> W -> E
                            DelayLevels(Delay, SpellSlot.Q);
                            DelayLevels(Delay + 50, SpellSlot.W);
                            DelayLevels(Delay + 100, SpellSlot.E);
                            break;
                        case 1:
                            if (Me.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.Q);
                            else if (Me.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.E);
                            else if (Me.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.W);

                            //Q -> E -> W
                            DelayLevels(Delay, SpellSlot.Q);
                            DelayLevels(Delay + 50, SpellSlot.E);
                            DelayLevels(Delay + 100, SpellSlot.W);
                            break;
                        case 2:
                            if (Me.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.W);
                            else if (Me.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.Q);
                            else if (Me.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.E);

                            //W -> Q -> E
                            DelayLevels(Delay, SpellSlot.W);
                            DelayLevels(Delay + 50, SpellSlot.Q);
                            DelayLevels(Delay + 100, SpellSlot.E);
                            break;
                        case 3:
                            if (Me.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.W);
                            else if (Me.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.E);
                            else if (Me.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.Q);

                            //W -> E -> Q
                            DelayLevels(Delay, SpellSlot.W);
                            DelayLevels(Delay + 50, SpellSlot.E);
                            DelayLevels(Delay + 100, SpellSlot.Q);
                            break;
                        case 4:
                            if (Me.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.E);
                            else if (Me.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.Q);
                            else if (Me.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.W);

                            //E -> Q -> W
                            DelayLevels(Delay, SpellSlot.E);
                            DelayLevels(Delay + 50, SpellSlot.Q);
                            DelayLevels(Delay + 100, SpellSlot.W);
                            break;
                        case 5:
                            if (Me.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.E);
                            else if (Me.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.W);
                            else if (Me.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                                Me.Spellbook.LevelSpell(SpellSlot.Q);

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
*/