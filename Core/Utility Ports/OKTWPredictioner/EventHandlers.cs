using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SebbyLib;
using HitChance = EnsoulSharp.SDK.HitChance;

namespace OKTWPredictioner
{
    public class EventHandlers
    {
        private static bool[] handleEvent = { true, true, true, true };

        public static void Game_OnGameLoad()
        {
            OKTWPredictioner.Initialize();
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
                    if (OKTWPredictioner.Spells[(int)slot] != null)
                        handleEvent[(int)slot] = true;
                }
            }
        }

        public static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe)
            {
                if (OKTWPredictioner.Config.GetValue<MenuBool>("ENABLED").Enabled && (OKTWPredictioner.Config.GetValue<MenuKeyBind>("COMBOKEY").Active || OKTWPredictioner.Config.GetValue<MenuKeyBind>("HARASSKEY").Active))
                {
                    if (!Utility.IsValidSlot(args.Slot))
                        return;

                    if (OKTWPredictioner.Spells[(int)args.Slot] == null)
                        return;

                    if (!OKTWPredictioner.Config.GetValue<MenuBool>(String.Format("{0}{1}", ObjectManager.Player.CharacterName, args.Slot)).Enabled)
                        return;

                    if (handleEvent[(int)args.Slot])
                    {
                        args.Process = false;

                        var enemy = TargetSelector.GetTarget(OKTWPredictioner.Spells[(int)args.Slot].Range, DamageType.Physical);

                        var QWER = OKTWPredictioner.Spells[(int)args.Slot];

                        if (enemy != null)
                        {
                            SkillshotType CoreType2 = SebbyLib.SkillshotType .SkillshotLine;
                            bool aoe2 = false;

                            if (QWER.Type == (SpellType) SkillshotType.SkillshotCircle)
                            {
                                CoreType2 = SkillshotType.SkillshotCircle;
                                aoe2 = true;
                            }

                            if (QWER.Width > 80 && !QWER.Collision)
                                aoe2 = true;

                            var predInput2 = new SebbyLib.PredictionInput()
                            {
                                Aoe = aoe2,
                                Collision = QWER.Collision,
                                Speed = QWER.Speed,
                                Delay = QWER.Delay,
                                Range = QWER.Range,
                                From = ObjectManager.Player.Position,
                                Radius = QWER.Width,
                                Unit = enemy,
                                Type = CoreType2
                            };
                            
                            var poutput2 = SebbyLib.Prediction.GetPrediction(predInput2);

                            if (QWER.Speed != float.MaxValue && OktwCommon.CollisionYasuo(ObjectManager.Player.Position, poutput2.CastPosition))
                                return;

                            if (Utility.HitchanceArray[OKTWPredictioner.Config.GetValue<MenuList>("SPREDHITC").Index] == HitChance.VeryHigh)
                            {
                                if (poutput2.Hitchance >= SebbyLib.HitChance.VeryHigh)
                                {
                                    if (QWER.Cast(poutput2.CastPosition))
                                        handleEvent[(int)args.Slot] = false;
                                }
                                else if (predInput2.Aoe && poutput2.AoeTargetsHitCount > 1 && poutput2.Hitchance >= SebbyLib.HitChance.High)
                                {
                                    if (QWER.Cast(poutput2.CastPosition))
                                        handleEvent[(int)args.Slot] = false;
                                }
                            }
                            else if (Utility.HitchanceArray[OKTWPredictioner.Config.GetValue<MenuList>("SPREDHITC").Index] == HitChance.High)
                            {
                                if (poutput2.Hitchance >= SebbyLib.HitChance.High)
                                {
                                    if (QWER.Cast(poutput2.CastPosition))
                                        handleEvent[(int)args.Slot] = false;
                                }
                            }
                            else if (Utility.HitchanceArray[OKTWPredictioner.Config.GetValue<MenuList>("SPREDHITC").Index] == HitChance.Medium)
                            {
                                if (poutput2.Hitchance >= SebbyLib.HitChance.Medium)
                                {
                                    if (QWER.Cast(poutput2.CastPosition))
                                        handleEvent[(int)args.Slot] = false;
                                }
                            }
                            else if (Utility.HitchanceArray[OKTWPredictioner.Config.GetValue<MenuList>("SPREDHITC").Index] == HitChance.Low)
                            {
                                if (poutput2.Hitchance >= SebbyLib.        HitChance.Low)
                                {
                                    if (QWER.Cast(poutput2.CastPosition))
                                        handleEvent[(int)args.Slot] = false;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}