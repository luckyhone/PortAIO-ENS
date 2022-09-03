using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;

namespace Entropy.AIO.Cassiopeia.Logics
{
    using Bases;
    using static Components;

    public class Other
    {
        public static void ExecuteQ(AIBaseClient sender, Dash.DashArgs args)
        {
            if (!MiscMenu.AutoQ.Enabled)
            {
                return;
            }

            var heroSender = sender as AIHeroClient;
            if (heroSender == null || heroSender.CharacterName.Equals("Kalista"))
            {
                return;
            }

            if (args.EndPos.DistanceToPlayer() >= ChampionBase.Q.Range ||
                Invulnerable.Check(heroSender))
            {
                return;
            }

            ChampionBase.Q.Cast(args.EndPos);
        }

        public static void SemiR()
        {
            if (!ComboMenu.semiR.Active)
            {
                return;
            }

            if (!ChampionBase.R.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(ChampionBase.R.Range,DamageType.Magical);
            if (target == null)
            {
                return;
            }

            var predictions = ChampionBase.R.GetPrediction(target);
            if (predictions.Hitchance >= HitChance.High && predictions.Hitchance != HitChance.OutOfRange)
            {
                ChampionBase.R.Cast(predictions.CastPosition);
            }
        }

        public static void FlashR()
        {
            if (!ComboMenu.rFlash.Active)
            {
                return;
            }

            Orbwalker.Move(Game.CursorPos);
            if (!ChampionBase.R.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(ChampionBase.R.Range + 410,DamageType.Magical);
            if (target == null                                                         ||
                ComboMenu.rFlashFace.Enabled && !target.IsFacing(ObjectManager.Player) ||
                !Spells.Flash.IsReady())
            {
                return;
            }

            var prediction = Spells.FlashR.GetPrediction(target);
            if (prediction.Hitchance >= HitChance.High && prediction.Hitchance != HitChance.OutOfRange)
            {
                if (ChampionBase.R.Cast(prediction.CastPosition))
                {
                    DelayAction.Add((int)(0.25f + Game.Ping / 1000f),() => Spells.Flash.Cast(prediction.CastPosition));
                }
            }
        }
    }
}