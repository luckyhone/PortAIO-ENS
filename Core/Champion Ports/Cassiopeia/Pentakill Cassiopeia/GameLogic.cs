using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using ExorAIO.Utilities;
using LeagueSharpCommon;
using Pentakill_Cassiopeia.Util;
using SPrediction;

namespace Pentakill_Cassiopeia
{
    class GameLogic
    {

        static int lastECast = 0;

        public static void Checks()
        {
            //Updates the auto-leveling sequence each time (redundant)
            List<SpellSlot> SKILL_SEQUENCE = new List<SpellSlot>() { SpellSlot.Q, SpellSlot.E, SpellSlot.E, SpellSlot.W, SpellSlot.E, SpellSlot.R, SpellSlot.E, SpellSlot.Q, SpellSlot.E, SpellSlot.Q, SpellSlot.R, SpellSlot.Q, SpellSlot.Q, SpellSlot.W, SpellSlot.W, SpellSlot.R, SpellSlot.W, SpellSlot.W };
            /*AutoLevel.UpdateSequence(SKILL_SEQUENCE);
            //Enables if user selects yes else disables
            if (Program.menuController.getMenu().Item("autoLevel").GetValue<bool>())
                AutoLevel.Enable();
            else
                AutoLevel.Disable();*/
        }

        public static void performCombo()
        {
            //Gets best target in Q range
            AIHeroClient target = TargetSelector.GetTarget(Program.q.Range, DamageType.Magical);
            if(target == null) return;

            //Ignite handler
            if (Program.menuController.getMenu()["combo"]["useIgnite"].GetValue<MenuBool>().Enabled)
            {
                if (SpellDamage.getComboDamage(target) > target.Health && target.Distance(Program.player.Position) < 500)
                {
                    Program.player.Spellbook.CastSpell(Program.ignite);
                }
            }
            //Ulti handler in combo
            if (Program.menuController.getMenu()["combo"]["comboUseR"].GetValue<MenuBool>().Enabled)
            {
                List<AIHeroClient> targets;
                /* if (Program.menuController.getMenu().Item("faceOnlyR").GetValue<bool>())
                 {
                     //Gets all facing enemies who can get hit by R
                     targets = HeroManager.Enemies.Where(o => Program.r.WillHit(o, target.Position) && o.IsFacing(Program.player)).ToList<Obj_AI_Hero>();
                 }
                 else
                 {*/
                //Gets all enemies who can get hit by R
                targets = HeroManager.Enemies.Where(o => Program.r.WillHit(o, target.Position) && o.Distance(Program.player.Position) < 500).ToList<AIHeroClient>();

                // }
                if (targets.Count >= Program.menuController.getMenu()["combo"]["minEnemies"].GetValue<MenuSlider>().Value)
                {
                    Program.r.Cast(target.Position);
                }
            }
            //Casts Q if selected in menu
            if (Program.menuController.getMenu()["combo"]["comboUseQ"].GetValue<MenuBool>().Enabled)
            {
                if (target != null)
                {
                    Program.q.CastIfHitchanceEquals(target, HitChance.High);
                }
            }
            //Casts E if selected in menu and in E range and target is poisoned
            if (Program.menuController.getMenu()["combo"]["comboUseE"].GetValue<MenuBool>().Enabled)
            {
                if (target != null && target.Distance(Program.player.Position) < Program.e.Range && (Environment.TickCount - lastECast) > Program.menuController.getMenu()["eDelay"].GetValue<MenuSlider>().Value)
                {
                    if (target.HasBuffOfType(BuffType.Poison))
                    {
                        Program.e.CastOnUnit(target);
                        lastECast = Environment.TickCount;

                    }
                }
            }
            //Casts W if selected in menu
            if (Program.menuController.getMenu()["combo"]["comboUseW"].GetValue<MenuBool>().Enabled)
            {
                if (target != null)
                {
                    Program.w.CastIfHitchanceEquals(target, HitChance.High);
                }
            }
        }

        public static void performHarass()
        {
            //Gets best target in Q range
            AIHeroClient target = TargetSelector.GetTarget(Program.q.Range, DamageType.Magical);
            //Casts Q if selected in menu
            if (Program.menuController.getMenu()["harassUseQ"].GetValue<MenuBool>().Enabled)
            {
                if (target != null)
                {
                    Program.q.CastIfHitchanceEquals(target, HitChance.High);
                }
            }
            //Casts E if selected in menu and in E range and target is poisoned
            if (Program.menuController.getMenu()["harassUseE"].GetValue<MenuBool>().Enabled)
            {
                if (target != null && target.Distance(Program.player.Position) < Program.e.Range && (Environment.TickCount - lastECast) > Program.menuController.getMenu()["eDelay"].GetValue<MenuSlider>().Value)
                {
                    if (target.HasBuffOfType(BuffType.Poison))
                    {
                        Program.e.CastOnUnit(target);
                        lastECast = Environment.TickCount;
                    }
                }
            }
            //Casts W if selected in menu
            if (Program.menuController.getMenu()["harassUseW"].GetValue<MenuBool>().Enabled)
            {
                if (target != null)
                {
                    Program.w.CastIfHitchanceEquals(target, HitChance.High);
                }
            }
        }

        public static void performLastHit()
        {
            //Gets minion that can be killed with Q + E
            AIBaseClient minion = MinionManager.GetMinions(Program.player.Position, Program.e.Range).FirstOrDefault(o => (o.Health < Program.q.GetDamage(o) + Program.e.GetDamage(o)));
            if (Program.menuController.getMenu()["lastHitUseQ"].GetValue<MenuBool>().Enabled)
            {
                //Checks if minion will NOT die from Q
                if (minion != null && minion.Health > Program.e.GetDamage(minion))
                {
                    Program.q.CastIfHitchanceEquals(minion, HitChance.High);
                }
            }
            if (Program.menuController.getMenu()["lastHitUseE"].GetValue<MenuBool>().Enabled)
            {
                //Checks if minion will die from E
                if (minion != null && minion.Health < Program.e.GetDamage(minion) && (Environment.TickCount - lastECast) > Program.menuController.getMenu()["eDelay"].GetValue<MenuSlider>().Value)
                {
                    //Is the minion poisoned so E doesn't go on CD?
                    if (minion.HasBuffOfType(BuffType.Poison))
                    {
                        Program.e.CastOnUnit(minion);
                        lastECast = Environment.TickCount;
                    }
                }
            }
        }

        public static void performLaneClear()
        {
            if (Program.menuController.getMenu()["laneClearUseW"].GetValue<MenuBool>().Enabled)
            {
                //Finds the best location to cast W in hitting maximum minions
                List<AIBaseClient> minionList = MinionManager.GetMinions(Program.e.Range);
                var wCast = FarmPrediction.GetBestCircularFarmLocation(minionList.Select(minion => minion.Position.To2D()).ToList(), Program.w.Width, Program.w.Range);
                if (wCast.MinionsHit >= 2)
                {
                    Program.w.Cast(wCast.Position);
                }
            }
            if (Program.menuController.getMenu()["laneClearUseQ"].GetValue<MenuBool>().Enabled)
            {
                //Gets minion and casts Q on it
                AIBaseClient minion = MinionManager.GetMinions(Program.player.Position, Program.e.Range).FirstOrDefault();
                if (minion != null)
                {
                    Program.q.CastIfHitchanceEquals(minion, HitChance.High);
                }
            }
            if (Program.menuController.getMenu()["laneClearUseE"].GetValue<MenuBool>().Enabled)
            {
                //Gets minion that is poisoned and casts E on it
                AIBaseClient minion = MinionManager.GetMinions(Program.player.Position, Program.e.Range).FirstOrDefault(o => o.HasBuffOfType(BuffType.Poison));
                if (minion != null && (Environment.TickCount - lastECast) > Program.menuController.getMenu()["eDelay"].GetValue<MenuSlider>().Value)
                {
                    Program.e.CastOnUnit(minion);
                    lastECast = Environment.TickCount;
                }
            }
        }
    }
}