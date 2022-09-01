using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SPrediction;

namespace SPredictioner
{
    public class EventHandlers
    {
        private static bool[] handleEvent = { true, true, true, true };

        public static void Game_OnGameLoad()
        {
            SPredictioner.Initialize();
        }

        public static void AIHeroClient_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs  args)
        {
            if (sender.IsMe)
            {
                SpellSlot slot = ObjectManager.Player.GetSpellSlot(args.SData.Name);
                if (!Utility.IsValidSlot(slot))
                    return;

                if (!handleEvent[(int)slot])
                {
                    if (SPredictioner.Spells[(int)slot] != null)
                        handleEvent[(int)slot] = true;
                }
            }
        }

        public static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe)
            {
                if (SPredictioner.Config.GetValue<MenuBool>("ENABLED").Enabled && (SPredictioner.Config.GetValue<MenuKeyBind>("COMBOKEY").Active || SPredictioner.Config.GetValue<MenuKeyBind>("HARASSKEY").Active))
                {
                    if (!Utility.IsValidSlot(args.Slot))
                        return;

                    if (SPredictioner.Spells[(int)args.Slot] == null)
                        return;

                    if (!SPredictioner.Config.GetValue<MenuBool>(String.Format("{0}{1}", ObjectManager.Player.CharacterName, args.Slot)).Enabled)
                        return;

                    if (handleEvent[(int)args.Slot])
                    {
                        args.Process = false;
                        var enemy = TargetSelector.GetTarget(SPredictioner.Spells[(int)args.Slot].Range, DamageType.Physical);


                        if (enemy != null)
                        {
                            if (ObjectManager.Player.CharacterName == "Viktor" && args.Slot == SpellSlot.E)
                            {
                                handleEvent[(int)args.Slot] = false;
                                SPredictioner.Spells[(int)args.Slot].SPredictionCastVector(enemy, 500, Utility.HitchanceArray[SPredictioner.Config.GetValue<MenuList>("SPREDHITC").Index]);
                            }
                            else if (ObjectManager.Player.CharacterName == "Diana" && args.Slot == SpellSlot.Q)
                            {
                                handleEvent[(int)args.Slot] = false;
                                SPredictioner.Spells[(int)args.Slot].SPredictionCastArc(enemy, Utility.HitchanceArray[SPredictioner.Config.GetValue<MenuList>("SPREDHITC").Index]);
                            }
                            else if (ObjectManager.Player.CharacterName == "Veigar" && args.Slot == SpellSlot.E)
                            {
                                handleEvent[(int)args.Slot] = false;
                                SPredictioner.Spells[(int)args.Slot].SPredictionCastRing(enemy, 80, Utility.HitchanceArray[SPredictioner.Config.GetValue<MenuList>("SPREDHITC").Index]);
                            }
                            else
                            {
                                SPredictioner.Spells[(int)args.Slot].SPredictionCast(enemy, Utility.HitchanceArray[SPredictioner.Config.GetValue<MenuList>("SPREDHITC").Index]);
                                handleEvent[(int)args.Slot] = false;
                            }
                        }
                    }
                }
            }
        }
    }
}