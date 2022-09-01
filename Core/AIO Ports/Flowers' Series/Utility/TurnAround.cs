 /*using EnsoulSharp;
 using EnsoulSharp.SDK;
 using EnsoulSharp.SDK.MenuUI;
 using EnsoulSharp.SDK.Utility;

 namespace Flowers_ADC_Series.Utility
{
    using System.Linq;
    using SharpDX;
    using System.Collections.Generic;

    internal class TurnAround : Logic //This Part From SFX Utility 
    {
        private static float blockMovementTime;
        private static Vector3 lastMove;

        private static readonly Menu Menu = Utilitymenu;

        private static readonly List<SpellInfo> spellInfos = new List<SpellInfo>
        {
            new SpellInfo("cassiopeia", "cassiopeiapetrifyinggaze", 1000f, false, true, 0.85f),
            new SpellInfo("tryndamere", "mockingshout", 900f, false, false, 0.65f)
        };

        internal static void Init()
        {
            var TurnAround = Menu.Add(new Menu("Trun Around", "Turn Around"));
            {
                TurnAround.Add(
                    GameObjects.EnemyHeroes.Any(h => spellInfos.Any(i => i.Owner == h.CharacterName.ToLower()))
                        ? new MenuBool("TrunAroundEnabled", "Enabled", true).SetValue(true)
                        : new MenuBool("NotSupport", "Not Have Support Champion", true));
            }

            AIBaseClient.OnIssueOrder += OnIssueOrder;
            AIBaseClient.OnProcessSpellCast += OnProcessSpellCast;
        }

        private static void OnIssueOrder(AIBaseClient sender, AIBaseClientIssueOrderEventArgs Args)
        {
            if (Menu["Trun Around"]["TrunAroundEnabled"] == null)
            {
                return;
            }

            if (!Menu["Trun Around"]["TrunAroundEnabled"].GetValue<MenuBool>().Enabled)
            {
                return;
            }

            if (sender.IsMe)
            {
                if (Args.Order == GameObjectOrder.MoveTo)
                {
                    lastMove = Args.TargetPosition;
                }

                if (blockMovementTime > Game.Time)
                {
                    Args.Process = false;
                }
            }
        }

        private static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs Args)
        {
            if (Menu["Trun Around"]["TrunAroundEnabled"] == null)
            {
                return;
            }

            if (!Menu["Trun Around"]["TrunAroundEnabled"].GetValue<MenuBool>().Enabled)
            {
                return;
            }

            if (sender == null || !sender.IsValid || sender.Team == Me.Team || Me.IsDead || !Me.IsTargetable)
            {
                return;
            }

            var spellInfo = spellInfos.FirstOrDefault(i => Args.SData.Name.ToLower().Contains(i.Name));

            if (spellInfo != null)
            {
                if ((spellInfo.Target && Args.Target == Me) ||
                    Me.Distance(sender.ServerPosition) + Me.BoundingRadius <= spellInfo.Range)
                {
                    var moveTo = lastMove;

                    Me.IssueOrder(GameObjectOrder.MoveTo,
                        sender.ServerPosition.Extend(Me.ServerPosition,
                            Me.ServerPosition.Distance(sender.ServerPosition) + (spellInfo.TurnOpposite ? 100 : -100)));

                    blockMovementTime = Game.Time + spellInfo.CastTime;

                    DelayAction.Add((int) ((spellInfo.CastTime + 0.1)*1000),
                        () => Me.IssueOrder(GameObjectOrder.MoveTo, moveTo));
                }
            }
        }

        private class SpellInfo
        {
            public SpellInfo(string owner, string name, float range, bool target, bool turnOpposite, float castTime)
            {
                Owner = owner;
                Name = name;
                Range = range;
                Target = target;
                TurnOpposite = turnOpposite;
                CastTime = castTime;
            }

            public string Name { get; private set; }
            public string Owner { get; private set; }
            public float Range { get; private set; }
            public bool Target { get; private set; }
            public bool TurnOpposite { get; private set; }
            public float CastTime { get; private set; }
        }
    }
}*/