using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using LeagueSharpCommon;
using SPrediction;
using OktwCommon = SebbyLib.OktwCommon;

namespace HikiCarry.Core.Predictions
{
    internal static class PredCommon
    {
        public static void CastSebby(this Spell spell, AIBaseClient unit, HitChance hit, bool aoe2 = false)
        {
            SebbyLib.SkillshotType CoreType2 = SebbyLib.SkillshotType.SkillshotLine;

            if (spell.Type == SpellType.Circle)
            {
                CoreType2 = SebbyLib.SkillshotType.SkillshotCircle;
                aoe2 = true;
            }

            if (spell.Width > 80 && !spell.Collision)
                aoe2 = true;

            var predInput2 = new SebbyLib.PredictionInput
            {
                Aoe = aoe2,
                Collision = spell.Collision,
                Speed = spell.Speed,
                Delay = spell.Delay,
                Range = spell.Range,
                From = ObjectManager.Player.ServerPosition,
                Radius = spell.Width,
                Unit = unit,
                Type = CoreType2
            };
            var poutput2 = SebbyLib.Prediction.GetPrediction(predInput2);

            //var poutput2 = QWER.GetPrediction(target);

            if (spell.Speed != Single.MaxValue && OktwCommon.CollisionYasuo(ObjectManager.Player.ServerPosition,
                poutput2.CastPosition))
                return;

            switch (hit)
            {
                case HitChance.Low:
                    if (poutput2.Hitchance >= SebbyLib.HitChance.Low)
                        spell.Cast(poutput2.CastPosition);
                    break;
                case HitChance.Medium:
                    if (poutput2.Hitchance >= SebbyLib.HitChance.Medium)
                        spell.Cast(poutput2.CastPosition);
                    break;
                case HitChance.High:
                    if (poutput2.Hitchance >= SebbyLib.HitChance.High)
                        spell.Cast(poutput2.CastPosition);
                    break;
                case HitChance.VeryHigh:
                    if (poutput2.Hitchance >= SebbyLib.HitChance.VeryHigh)
                        spell.Cast(poutput2.CastPosition);
                    break;
                case HitChance.Immobile:
                    if (poutput2.Hitchance >= SebbyLib.HitChance.Immobile)
                        spell.Cast(poutput2.CastPosition);
                    break;
            }
        }

        public static void CastSdk(this Spell spell, AIBaseClient unit, HitChance hit, bool aoe = false)
        {
            SebbyLib.SkillshotType CoreType2 = SebbyLib.SkillshotType.SkillshotLine;

            if (spell.Type == SpellType.Circle)
            {
                CoreType2 = SebbyLib.SkillshotType.SkillshotCircle;
                aoe = true;
            }

            if (spell.Width > 80 && !spell.Collision)
                aoe = true;

            var predInput2 = new SebbyLib.PredictionInput
            {
                Aoe = aoe,
                Collision = spell.Collision,
                Speed = spell.Speed,
                Delay = spell.Delay,
                Range = spell.Range,
                From = ObjectManager.Player.ServerPosition,
                Radius = spell.Width,
                Unit = unit,
                Type = CoreType2
            };

            var poutput2 = SebbyLib.Prediction.GetPrediction(predInput2);

            if (spell.Speed != Single.MaxValue &&
                OktwCommon.CollisionYasuo(ObjectManager.Player.ServerPosition, poutput2.CastPosition))
            {
                return;
            }

            switch (hit)
            {
                case HitChance.Low:
                    if (poutput2.Hitchance >= SebbyLib.HitChance.Low)
                        spell.Cast(poutput2.CastPosition);
                    break;
                case HitChance.Medium:
                    if (poutput2.Hitchance >= SebbyLib.HitChance.Medium)
                        spell.Cast(poutput2.CastPosition);
                    break;
                case HitChance.High:
                    if (poutput2.Hitchance >= SebbyLib.HitChance.High)
                        spell.Cast(poutput2.CastPosition);
                    break;
                case HitChance.VeryHigh:
                    if (poutput2.Hitchance >= SebbyLib.HitChance.VeryHigh)
                        spell.Cast(poutput2.CastPosition);
                    break;
                case HitChance.Immobile:
                    if (poutput2.Hitchance >= SebbyLib.HitChance.Immobile)
                        spell.Cast(poutput2.CastPosition);
                    break;
            }

            if (predInput2.Aoe && poutput2.AoeTargetsHitCount > 1 &&
                poutput2.Hitchance >= SebbyLib.HitChance.High)
            {
                spell.Cast(poutput2.CastPosition);
            }
        }

        public static void Do(this Spell spell, AIBaseClient unit, HitChance hit, bool aoe2 =false)
        {
            switch (Initializer.Config["prediction"].GetValue<MenuList>().Index)
            {
                case 0: // Common
                    var pred = spell.GetPrediction(unit);
                    if (pred.Hitchance >= hit && pred.AoeTargetsHitCount >= 1)
                    {
                        spell.Cast(pred.CastPosition);
                    }
                    break;
                case 1: // Sebby
                    spell.CastSebby(unit, hit, aoe2);
                    break;
                case 2:
                    spell.CastSebby(unit, hit, aoe2);
                    break;
                case 3: // SDK
                    spell.CastSdk(unit, hit, aoe2);
                    break;
            }
        }

        public static void Do(this Spell spell, AIHeroClient unit, HitChance hit, bool aoe2 = false)
        {
            switch (Initializer.Config["prediction"].GetValue<MenuList>().Index)
            {
                case 0: // Common
                    var pred = spell.GetPrediction(unit);
                    if (pred.Hitchance >= hit)
                    {
                        spell.Cast(pred.CastPosition);
                    }
                    break;
                case 1: // Sebby
                    spell.CastSebby(unit, hit, aoe2);
                    break;
                case 2:
                    spell.SPredictionCast(unit, hit);
                    break;
                case 3: // SDK
                    spell.CastSdk(unit, hit, aoe2);
                    break;
            }
        }
    }
}