using EnsoulSharp;
using EnsoulSharp.SDK;

namespace KoreanZed
{
    class ZedUnderTurretFarm
    {
        private readonly Orbwalker zedOrbwalker;

        private readonly ZedSpell q;

        private readonly ZedSpell e;

        private AIMinionClient targetUnderTurret;

        private AIBaseClient turrent;

        public ZedUnderTurretFarm(ZedSpells zedSpells, Orbwalker zedOrbwalker)
        {
            q = zedSpells.Q;
            e = zedSpells.E;

            this.zedOrbwalker = zedOrbwalker;

            //Obj_AI_Base.OnTarget += Obj_AI_Base_OnTarget;
            Game.OnUpdate += Game_OnUpdate;
        }

        private void Obj_AI_Base_OnTarget(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender == null || args.Target == null || !sender.IsAlly || !args.Target.IsEnemy
                || !sender.CharacterName.ToLowerInvariant().Contains("turret")
                || !args.Target.Name.ToLowerInvariant().Contains("minion"))
            {
                return;
            }

            if (false)
            {
                turrent = sender;
                //targetUnderTurret = new Obj_AI_Minion((ushort)args.Target.Index, (uint)args.Target.NetworkId); ;
            }
            else
            {
                turrent = null;
                targetUnderTurret = null;
            }
        }

        private void Game_OnUpdate(System.EventArgs args)
        {
            if (targetUnderTurret != null && targetUnderTurret.IsDead)
            {
                targetUnderTurret = null;
                turrent = null;
            }

            if (Orbwalker.ActiveMode != OrbwalkerMode.Combo
                && Orbwalker.ActiveMode != OrbwalkerMode.None && turrent != null
                && targetUnderTurret != null && !targetUnderTurret.IsDead && targetUnderTurret.IsValid)
            {
                if (targetUnderTurret.IsValid)
                {
                    if (ObjectManager.Player.Distance(targetUnderTurret)
                        < ObjectManager.Player.GetRealAutoAttackRange(targetUnderTurret) + 20F
                        && (targetUnderTurret.Health
                            < (ObjectManager.Player.GetAutoAttackDamage(targetUnderTurret) * 2)
                            + turrent.GetAutoAttackDamage(targetUnderTurret)
                            && targetUnderTurret.Health
                            > turrent.GetAutoAttackDamage(targetUnderTurret)
                            + ObjectManager.Player.GetAutoAttackDamage(targetUnderTurret)))
                    {
                        ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, targetUnderTurret);
                    }

                    if (q.IsReady() && q.CanCast(targetUnderTurret)
                        && ObjectManager.Player.Distance(targetUnderTurret)
                        < ObjectManager.Player.GetRealAutoAttackRange(targetUnderTurret) + 20F
                        && targetUnderTurret.Health
                        < q.GetDamage(targetUnderTurret)
                        + ObjectManager.Player.GetAutoAttackDamage(targetUnderTurret, true))
                    {
                        q.Cast(targetUnderTurret);
                        return;
                    }

                    if (e.IsReady() && e.CanCast(targetUnderTurret) && !q.IsReady()
                        && ObjectManager.Player.Distance(targetUnderTurret)
                        < ObjectManager.Player.GetRealAutoAttackRange(targetUnderTurret) + 20F
                        && targetUnderTurret.Health
                        < e.GetDamage(targetUnderTurret)
                        + ObjectManager.Player.GetAutoAttackDamage(targetUnderTurret, true))
                    {
                        e.Cast(targetUnderTurret);
                    }
                }
            }
        }
    }
}