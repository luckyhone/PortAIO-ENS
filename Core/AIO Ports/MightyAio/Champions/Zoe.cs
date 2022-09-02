using EnsoulSharp;
using EnsoulSharp.SDK;
using SharpDX;
using System;
using System.Linq;

namespace MightyAio.Champions
{
    internal class Zoe
    {
        private static AIHeroClient Player => ObjectManager.Player;
        private static Spell Q, Q2, W, E,E2, R;

        public Zoe()
        {
            Q = new Spell(SpellSlot.Q, 600f);
            Q.SetSkillshot(0.25f, 100f, 1200, true, SpellType.Circle);
            Q2 = new Spell(SpellSlot.Q, 1000f);
            Q2.SetSkillshot(0.25f, 100f, 1200f, false, SpellType.Circle);
            E = new Spell(SpellSlot.E, 800f);
            E.SetSkillshot(0.5f, 100f, 2000, true, SpellType.Line); 
            E2 = new Spell(SpellSlot.E, int.MaxValue);
            E2.SetSkillshot(0.5f, 100f, 2000, true, SpellType.Line);
            R = new Spell(SpellSlot.R, 575);
            R.SetSkillshot(0.7f, 140f, 1500f, true, SpellType.Circle);

            Game.OnUpdate += Game_OnUpdate;
            AIBaseClient.OnDoCast += AIBaseClient_OnProcessSpellCast;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private void Drawing_OnDraw(EventArgs args)
        {

        }

        private void AIBaseClient_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                Game.Print(args.SData.Name);
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Harass();
                    ECastWall();
                    break;
            }
            if (Player.SkinId != 9) { Player.SetSkin(9); }
        }

        private void Harass()
        {
            var target = TargetSelector.GetTarget(1500,DamageType.Magical);
            var rQ2ange = Math.Floor(900 * Player.MoveSpeed);
            float stuntime = 0;
            float nobufftime = 0;

            if (target != null)
            {
                if (Variables.GameTimeTickCount - stuntime > 1 && !QRecast() && target.HasBuff("zoeesleepcountdownslow") && target.DistanceToPlayer() < 1000)
                {
                    var point = target.Position.Extend((Player.Position), 1000);
                    var Collision = minionCollision2(Player.Position, point, Q);
                    if (Q.IsReady() && Collision == 0)
                    {
                        nobufftime = Variables.GameTimeTickCount;
                        Q.Cast(point);
                    }
                }
                if (!QRecast() && target.HasBuff("zoeesleepcountdownslow") && target.DistanceToPlayer() < rQ2ange)
                {
                    var point2 = target.Position.Perpendicular();
                    var startpos = Player.Position;
                    var endpos = target.Position;
                    var dir = (endpos - startpos).Normalized();
                    var pDir = dir.Perpendicular();
                    var Collision = minionCollision2(Player.Position, pDir, Q);
                    if (Q.IsReady() && Collision == 0)
                    {
                        nobufftime = Variables.GameTimeTickCount;
                        Q.Cast(pDir);
                    }
                }
                if (!QRecast() && target.HasBuff("zoeesleepcountdownslow") && target.DistanceToPlayer() < rQ2ange)
                {
                    var point3 = target.Position.ToVector2().Perpendicular();
                    var Collision = minionCollision2(Player.Position, point3.ToVector3(), Q);
                    if (Q.IsReady() && Collision == 0)
                    {
                        nobufftime = Variables.GameTimeTickCount;
                        Q.Cast(point3);
                    }
                }
                if (!QRecast() && target.HasBuff("zoeesleepcountdownslow") && target.DistanceToPlayer() < rQ2ange)
                {
                    var point4 = target.Position.ToVector2().Perpendicular().Perpendicular();
                    var Collision = minionCollision2(Player.Position, point4.ToVector3(), Q);
                    if (Q.IsReady() && Collision == 0)
                    {
                        nobufftime = Variables.GameTimeTickCount;
                        Q.Cast(point4);
                    }
                }
                if (!QRecast() && target.DistanceToPlayer() < 1000 && !E.IsReady())
                {
                    var point5 = target.Position.Extend(Player.Position, 1000);
                    var Collision = minionCollision2(Player.Position, point5, Q);
                    if (Q.IsReady() && Collision == 0)
                    {
                        nobufftime = Variables.GameTimeTickCount;
                        Q.Cast(point5);
                    }
                }
                if (Q.IsReady() && R.IsReady() && QRecast() && target.IsValid)
                {
                    var pointr = Player.Position.Extend(target.Position, 1000);

                    if (target.DistanceToPlayer() > 600)
                    {
                        R.Cast(pointr);
                    }
                }
            }

            if (target != null)
            {
                var qpre = Q2.GetPrediction(target, true);

                if (target.HasBuff("zoeesleepstun") && QRecast() && target.Distance(Player) < rQ2ange)
                {
                    if (qpre.Hitchance >= HitChance.High)
                    { Q2.Cast(target.Position); }
                }
                if (Variables.GameTimeTickCount - nobufftime > 0.6 && target.HasBuff("zoeesleepcountdownslow") && QRecast() && target.Distance(Player) < rQ2ange)
                {
                    if (qpre.Hitchance >= HitChance.High)
                    { Q2.Cast(target.Position); }
                }
                if (!target.CanMove && QRecast() && target.Distance(Player) < 800)
                {
                    if (qpre.Hitchance >= HitChance.High)
                    { Q2.Cast(target.Position); }
                }
                if (Variables.GameTimeTickCount - nobufftime > 0.5 && target.HasBuff("zoeesleepstun") && QRecast() && target.Distance(Player) < rQ2ange && !target.HasBuff("zoeesleepcountdownslow"))
                {
                    if (qpre.Hitchance >= HitChance.High)
                    { Q2.Cast(target.Position); }
                }

                if (target.DistanceToPlayer() < E.Range)
                {
                    var Epre = E.GetPrediction(target);

                    if (E.IsReady() && Epre.Hitchance >= HitChance.High)
                    {
                        E.Cast(Epre.CastPosition);
                    }
                    stuntime = Variables.GameTimeTickCount;
                }
            }
        }

        #region functions

        private bool QRecast()
        {
            bool a = false;
            if (Player.Spellbook.GetSpell(SpellSlot.Q).Name == "ZoeQRecast")
            {
                a = true;
            }
            if (Player.Spellbook.GetSpell(SpellSlot.Q).Name == "ZoeQ")
            {
                a = false;
            }
            return a;
        }

        private void ECastWall()
        {
            var target = TargetSelector.GetTarget(2000,DamageType.Magical);

            if (target != null)
            {
                if (target.IsValid && target.DistanceToPlayer() < (E.Range + 800) && E.IsReady())
                {
                    var from = Player.PreviousPosition.ToVector2();
                    var to = target.PreviousPosition.ToVector2();
                    var direction = (from - to).Normalized();
                    var distance = from.Distance(to);

                    for (var d = 0; d < distance; d = d + 20)
                    {
                        var point = from + d * direction;
                        var flags = NavMesh.GetCollisionFlags(point.ToVector3());

                        if (flags.HasFlag(CollisionFlags.Wall) || flags.HasFlag(CollisionFlags.Building))
                        {
                            var Epre = E2.GetPrediction(target);
                            if (Epre.Hitchance >= HitChance.High)
                            {
                                E2.Cast(Epre.CastPosition);
                            }
                        }
                       
                    }
                }
            }
        }

        private int minionCollision2(Vector3 player, Vector3 position, Spell spell)
        {
            var targemyCounter = 0;
            var ms = GameObjects.EnemyMinions.Where(x => x.Distance(player) <= 550);
            foreach (var minion in ms)
            {
                if (minion.IsValid && !minion.IsDead)
                {
                    Vector2 linesegment = player.ProjectOn(position, minion.Position).SegmentPoint;
                    Vector2 line = player.ProjectOn(position, minion.Position).LinePoint;
                    bool isOnSegment = player.ProjectOn(position, minion.Position).IsOnSegment;
                    if (isOnSegment && player.Distance(minion.Position) <= (minion.BoundingRadius + spell.Width) * (minion.BoundingRadius + spell.Width))
                    {
                        targemyCounter = targemyCounter + 1;
                    }
                }
            }

            return targemyCounter;
        }

        #endregion functions
    }
}