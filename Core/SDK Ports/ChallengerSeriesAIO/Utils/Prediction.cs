using System;
using System.Collections.Generic;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;

namespace Challenger_Series.Utils
{
    public static class Prediction
    {
        public static MenuList PredictionMode;
        public static Tuple<HitChance, Vector3, List<AIBaseClient>> GetPrediction(AIHeroClient target, Spell spell)
        {
            switch (Utils.Prediction.PredictionMode.SelectedValue)
            {
                case "SDK":
                {
                    var pred = spell.GetPrediction(target);
                    return new Tuple<HitChance, Vector3, List<AIBaseClient>>(pred.Hitchance, pred.UnitPosition, pred.CollisionObjects);
                }
                default:
                {

                    var pred = spell.GetPrediction(target);
                    return new Tuple<HitChance, Vector3, List<AIBaseClient>>((HitChance)((int)pred.Hitchance), pred.UnitPosition, pred.CollisionObjects);
                }
            }
        }

        public static SpellType GetCommonSkillshotType(SpellType sdkType)
        {
            switch (sdkType)
            {
                case SpellType.Circle:
                    return SpellType.Circle;
                case SpellType.Cone:
                    return SpellType.Cone;
                case SpellType.Line:
                    return SpellType.Line;
                default:
                    return SpellType.Line;
            }
        }
    }
}