using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using hCamille.Extensions;
using PortAIO;
using ShadowTracker;
using SharpDX;
using SPrediction;
using Color = System.Drawing.Color;
using Geometry = LeagueSharpCommon.Geometry;
using Utilities = hCamille.Extensions.Utilities;
//using static LeagueSharpCommon.Geometry;

namespace hCamille.Champions
{
    class Camille
    {
        public static string WallBuff => "camilleedashtoggle";
        public static string DashName => "camilleedash";
        public static bool OnWall => ObjectManager.Player.HasBuff(WallBuff) || Spells.E.Instance.Name == "CamilleEDash2";
        public static bool HasQBuff => ObjectManager.Player.HasBuff("CamilleQ");
        public static string UltimateEmitterName => "Indicator_Edge.troy";

        public Camille()
        {
            Spells.Initializer();
            Menus.Initializer();

            Game.OnUpdate += CamilleOnUpdate;
            AIBaseClient.OnDoCast += CamilleOnSpellCast;
            AIBaseClient.OnIssueOrder += OnIssueOrder;
            Drawing.OnDraw += OnDraw;
            AIBaseClient.OnDoCast += OnProcess;
            AntiGapcloser.OnGapcloser += OnEnemyGapcloser;
            Interrupter.OnInterrupterSpell += OnInterrupt;
        }

        private void OnInterrupt(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (sender.IsEnemy && args.DangerLevel > Interrupter.DangerLevel.Medium && Utilities.Enabled("e.interrupt"))
            {
                var result = ObjectManager.Player;
                var rng = Utilities.Slider("wall.search.range");
                var listPoint = new List<Tuple<Vector2, float>>();
                for (var i = 0; i <= 360; i += 1)
                {
                    var cosX = Math.Cos(i * Math.PI / 180);
                    var sinX = Math.Sin(i * Math.PI / 180);
                    var pos1 = new Vector3(
                        (float)(result.Position.X + rng * cosX), (float)(result.Position.Y + rng * sinX),
                        ObjectManager.Player.Position.Z);
                    var time = Environment.TickCount;
                    for (int j = 0; j < rng; j += 100)
                    {
                        var pos = new Vector3(
                            (float)(result.Position.X + j * cosX), (float)(result.Position.Y + j * sinX),
                            ObjectManager.Player.Position.Z);
                        if (NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Building))
                        {
                            if (j != 0)
                            {
                                int left = j - 99, right = j;
                                do
                                {
                                    var middle = (left + right) / 2;
                                    pos = new Vector3(
                                        (float)(result.Position.X + middle * cosX), (float)(result.Position.Y + middle * sinX),
                                        ObjectManager.Player.Position.Z);
                                    if (NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Building))
                                    {
                                        right = middle;
                                    }
                                    else
                                    {
                                        left = middle + 1;
                                    }
                                } while (left < right);
                            }
                            pos1 = pos;
                            time = Environment.TickCount;
                            break;
                        }
                    }

                    listPoint.Add(new Tuple<Vector2, float>(pos1.To2D(), time));
                }
                var target = sender;
                if (!OnWall)
                {
                    for (int i = 0; i < listPoint.Count - 1; i++)
                    {
                        if (listPoint[i].Item1.IsWall() && listPoint[i].Item1.Distance(ObjectManager.Player.Position) < Utilities.Slider("wall.distance.to.enemy")
                             && listPoint[i].Item1.Distance(target.Position) < 500 && target.IsCastingImporantSpell() && !listPoint[i].Item1.To3D().IsUnderEnemyTurret())
                        {
                            Spells.E.Cast(listPoint[i].Item1);
                        }
                    }
                }
            }
        }

        private void OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (sender != null && sender.IsEnemy && args.EndPosition.Distance(ObjectManager.Player.Position) < 300
                && Utilities.Enabled("e.anti"))
            {
                var result = ObjectManager.Player;
                var rng = Utilities.Slider("wall.search.range");
                var listPoint = new List<Tuple<Vector2, float>>();
                for (var i = 0; i <= 360; i += 1)
                {
                    var cosX = Math.Cos(i * Math.PI / 180);
                    var sinX = Math.Sin(i * Math.PI / 180);
                    var pos1 = new Vector3(
                        (float)(result.Position.X + rng * cosX), (float)(result.Position.Y + rng * sinX),
                        ObjectManager.Player.Position.Z);
                    var time = Environment.TickCount;
                    for (int j = 0; j < rng; j += 100)
                    {
                        var pos = new Vector3(
                            (float)(result.Position.X + j * cosX), (float)(result.Position.Y + j * sinX),
                            ObjectManager.Player.Position.Z);
                        if (NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Building))
                        {
                            if (j != 0)
                            {
                                int left = j - 99, right = j;
                                do
                                {
                                    var middle = (left + right) / 2;
                                    pos = new Vector3(
                                        (float)(result.Position.X + middle * cosX), (float)(result.Position.Y + middle * sinX),
                                        ObjectManager.Player.Position.Z);
                                    if (NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Building))
                                    {
                                        right = middle;
                                    }
                                    else
                                    {
                                        left = middle + 1;
                                    }
                                } while (left < right);
                            }
                            pos1 = pos;
                            time = Environment.TickCount;
                            break;
                        }
                    }

                    listPoint.Add(new Tuple<Vector2, float>(pos1.To2D(), time));
                }
                var target = sender;
                if (!OnWall)
                {
                    for (int i = 0; i < listPoint.Count - 1; i++)
                    {
                        if (listPoint[i].Item1.IsWall() && listPoint[i].Item1.Distance(ObjectManager.Player.Position) < Utilities.Slider("wall.distance.to.enemy")
                             && listPoint[i].Item1.Distance(sender.Position) > 400 && !listPoint[i].Item1.To3D().IsUnderEnemyTurret())
                        {
                            var i1 = i;
                            var starttick = listPoint[i1].Item2;
                            var startpos = target.ServerPosition.To2D();
                            var speed = target.GetSpell(args.Slot).SData.MissileSpeed;
                            var pathshit = target.Path.OrderBy(x => starttick + (int)(1000 * (new Vector3(x.X, x.Y, x.Z).
                            Distance(startpos.To3D()) / speed))).FirstOrDefault();

                            var endpos = new Vector3(pathshit.X, pathshit.Y, pathshit.Z);
                            var endtick = starttick + (int)(1000 * (endpos.Distance(startpos.To3D())
                                / speed));
                            var camilleendtic = starttick + (int)(1000 * (listPoint[i].Item1.Distance(ObjectManager.Player.Position)
                                / Spells.E.Speed));

                            if (listPoint[i].Item1.Distance(endpos) < 500 && camilleendtic > endtick)
                            {
                                Spells.E.Cast(listPoint[i].Item1);
                            }
                        }
                    }
                }
            }
        }


        private void OnProcess(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
               
                if (args.SData.Name == "CamilleW")
                {
                    Spells.W.LastCastAttemptTime = Environment.TickCount;
                }

                if (args.SData.Name == "CamilleE" || args.SData.Name == "CamilleEDash2")
                {
                    Spells.E.LastCastAttemptTime = Environment.TickCount;
                }

                if (args.SData.Name == "CamilleR")
                {
                    Spells.R.LastCastAttemptTime = Environment.TickCount;
                }


            }
        }
        
        private void OnDraw(EventArgs args)
        {
            if (Menus.Config["Skill Draws"]["q.draw"].GetValue<MenuBool>().Enabled && Spells.Q.IsReady())
            {
                LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, Spells.Q.Range, Color.White);
            }
            if (Menus.Config["Skill Draws"]["w.draw"].GetValue<MenuBool>().Enabled && Spells.W.IsReady())
            {
                LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, Spells.W.Range, Color.White);
            }
            if (Menus.Config["Skill Draws"]["e.draw"].GetValue<MenuBool>().Enabled && Spells.E.IsReady())
            {
                LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, Spells.E.Range, Color.White);
            }
            if (Menus.Config["Skill Draws"]["r.draw"].GetValue<MenuBool>().Enabled && Spells.R.IsReady())
            {
                LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, Spells.R.Range, Color.White);
            }
        }

        private void OnIssueOrder(AIBaseClient sender, AIBaseClientIssueOrderEventArgs args)
        {
            if (sender.IsMe && OnWall && Orbwalker.ActiveMode == OrbwalkerMode.Combo && 
                Spells.E.IsReady() && args.Order == GameObjectOrder.MoveTo)
            {
                var target = TargetSelector.GetTarget(Utilities.Slider("enemy.search.range"), DamageType.Physical);
                if (target != null)
                {
                    args.Process = false;
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, target.ServerPosition, false);
                }
                else
                {
                    args.Process = true;
                }
            }
        }

        private void CamilleOnSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && Orbwalker.IsAutoAttack(ObjectManager.Player.CharacterName) && Orbwalker.ActiveMode == OrbwalkerMode.Combo
                && Menus.Config["q.mode"].GetValue<MenuList>().Index == 0 && Spells.Q.IsReady() && Utilities.Enabled("q.combo"))
            {
                Spells.Q.Cast();
            }
        }

        private void CamilleOnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    OnCombo();
                    break;
                case OrbwalkerMode.Harass:
                    OnMixed();
                    break;
                case OrbwalkerMode.LaneClear:
                    OnJungle();
                    OnClear();
                    break;
            }

            if (Menus.Config["flee"].GetValue<MenuKeyBind>().Active)
            {
                Orbwalker.Move(Game.CursorPos);
                if (Spells.E.IsReady())
                {
                    FleeE();
                }
            }
        }

        private void OnCombo()
        {
            var target = TargetSelector.GetTarget(1000, DamageType.Physical);
            if (target != null)
            {
                if (Spells.Q.IsReady() && Utilities.Enabled("q.combo") && target.IsValidTarget(ObjectManager.Player.AttackRange)
                    && Menus.Config["q.mode"].GetValue<MenuList>().Index == 1)
                {
                    Spells.Q.Cast();
                }

                if (Spells.W.IsReady() && Menus.Config["Combo Settings"]["w.combo"].GetValue<MenuBool>().Enabled && target.IsValidTarget(Spells.W.Range) && 
                    Environment.TickCount - Spells.E.LastCastAttemptTime > 1200)
                {
                    switch (Menus.Config["w.mode"].GetValue<MenuList>().Index)
                    {
                        case 0:
                            if (OnWall)
                            {
                                var pred = Spells.W.GetPrediction(target);
                                if (pred.Hitchance >= HitChance.Medium)
                                {
                                    Spells.W.Cast(pred.CastPosition);
                                }
                            }
                            break;
                        case 1:
                            var predx = Spells.W.GetPrediction(target);
                            if (predx.Hitchance >= HitChance.Medium)
                            {
                                Spells.W.Cast(predx.CastPosition);
                            }
                            break;
                    }
                    
                }

                if (Spells.E.IsReady() && Utilities.Enabled("e.combo") && target.IsValidTarget(Utilities.Slider("enemy.search.range")))
                {
                    if (ObjectManager.Player.CountEnemyHeroesInRange(Utilities.Slider("enemy.search.range")) <= Utilities.Slider("max.enemy.count")
                        && Environment.TickCount - Spells.R.LastCastAttemptTime > 4000)
                    {
                        UseE();
                    }
                }

                if (Spells.R.IsReady() && Utilities.Enabled("r.combo"))
                {
                    switch (Menus.Config["r.mode"].GetValue<MenuList>().Index)
                    {
                        case 0:
                            if (target.IsValidTarget(Spells.R.Range) && target.HealthPercent < Utilities.Slider("enemy.health.percent") && Utilities.Enabled("r." + target.CharacterName))
                            {
                                Spells.R.CastOnUnit(target);
                            }
                            break;
                        case 1:
                            var selectedtarget = TargetSelector.SelectedTarget;
                            if (selectedtarget != null && selectedtarget.IsValidTarget(Spells.R.Range) && selectedtarget.HealthPercent < Utilities.Slider("enemy.health.percent") && Utilities.Enabled("r." + selectedtarget.CharacterName))
                            {
                                Spells.R.CastOnUnit(selectedtarget);
                            }
                            break;
                    }
                    
                }
            }
        }
        private static void UseE()
        {
            var result = ObjectManager.Player;
            var rng = Utilities.Slider("wall.search.range");
            var listPoint = new List<Tuple<Vector2, float>>();
            for (var i = 0; i <= 360; i += 1)
            {
                var cosX = Math.Cos(i * Math.PI / 180);
                var sinX = Math.Sin(i * Math.PI / 180);
                var pos1 = new Vector3(
                    (float)(result.Position.X + rng * cosX), (float)(result.Position.Y + rng * sinX),
                    ObjectManager.Player.Position.Z);
                var time = Environment.TickCount;
                for (int j = 0; j < rng; j += 100)
                {
                    var pos = new Vector3(
                        (float)(result.Position.X + j * cosX), (float)(result.Position.Y + j * sinX),
                        ObjectManager.Player.Position.Z);
                    if (NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Wall))
                    {
                        if (j != 0)
                        {
                            int left = j - 99, right = j;
                            do
                            {
                                var middle = (left + right) / 2;
                                pos = new Vector3(
                                    (float)(result.Position.X + middle * cosX), (float)(result.Position.Y + middle * sinX),
                                    ObjectManager.Player.Position.Z);
                                if (NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Wall))
                                {
                                    right = middle;
                                }
                                else
                                {
                                    left = middle + 1;
                                }
                            } while (left < right);
                        }
                        pos1 = pos;
                        time = Environment.TickCount;
                        break;
                    }
                }

                listPoint.Add(new Tuple<Vector2, float>(pos1.To2D(), time));
            }
            var target = TargetSelector.GetTarget(Utilities.Slider("enemy.search.range"), DamageType.Physical);
            if (target != null && !OnWall)
            {
                for (int i = 0; i < listPoint.Count - 1; i++)
                {
                    if (listPoint[i].Item1.IsWall() && listPoint[i].Item1.Distance(ObjectManager.Player.Position) < Utilities.Slider("wall.distance.to.enemy"))
                    {
                        var i1 = i;
                        var starttick = listPoint[i1].Item2;
                        var startpos = target.ServerPosition.To2D();
                        var speed = target.MoveSpeed;
                        var pathshit = target.Path.OrderBy(x => starttick + (int)(1000 * (new Vector3(x.X, x.Y, x.Z).
                        Distance(startpos.To3D()) / speed))).FirstOrDefault();

                        var endpos = new Vector3(pathshit.X, pathshit.Y, pathshit.Z);
                        var endtick = starttick + (int)(1000 * (endpos.Distance(startpos.To3D())
                            / speed));
                        var camilleendtic = starttick + (int)(1000 * (listPoint[i].Item1.Distance(ObjectManager.Player.Position)
                            / Spells.E.Speed));

                        if (listPoint[i].Item1.Distance(endpos) < 500 && camilleendtic > endtick)
                        {
                            Spells.E.Cast(listPoint[i].Item1);
                        }
                    }
                }
            }
        }
        private static void FleeE()
        {
            var result = ObjectManager.Player;
            var rng = Utilities.Slider("wall.search.range");
            var listPoint = new List<Tuple<Vector2, float>>();
            for (var i = 0; i <= 360; i += 1)
            {
                var cosX = Math.Cos(i * Math.PI / 180);
                var sinX = Math.Sin(i * Math.PI / 180);
                var pos1 = new Vector3(
                    (float)(result.Position.X + rng * cosX), (float)(result.Position.Y + rng * sinX),
                    ObjectManager.Player.Position.Z);
                var time = Environment.TickCount;
                for (int j = 0; j < rng; j += 100)
                {
                    var pos = new Vector3(
                        (float)(result.Position.X + j * cosX), (float)(result.Position.Y + j * sinX),
                        ObjectManager.Player.Position.Z);
                    if (NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Wall))
                    {
                        if (j != 0)
                        {
                            int left = j - 99, right = j;
                            do
                            {
                                var middle = (left + right) / 2;
                                pos = new Vector3(
                                    (float)(result.Position.X + middle * cosX), (float)(result.Position.Y + middle * sinX),
                                    ObjectManager.Player.Position.Z);
                                if (NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Wall))
                                {
                                    right = middle;
                                }
                                else
                                {
                                    left = middle + 1;
                                }
                            } while (left < right);
                        }
                        pos1 = pos;
                        time = Environment.TickCount;
                        break;
                    }
                }

                listPoint.Add(new Tuple<Vector2, float>(pos1.To2D(), time));
            }

            if (!OnWall)
            {
                for (int i = 0; i < listPoint.Count - 1; i++)
                {
                    var rectangle = new Geometry.Geometry.Polygon.Rectangle(ObjectManager.Player.Position, ObjectManager.Player.Position.Extend(Game.CursorPos, Spells.E.Range), Spells.E.Width);
                    if (listPoint[i].Item1.IsWall() && listPoint[i].Item1.Distance(ObjectManager.Player.Position) < Utilities.Slider("wall.distance.to.enemy") && rectangle.IsInside(listPoint[i].Item1))
                    {
                        Spells.E.Cast(listPoint[i].Item1);
                    }
                }
            }
        }
        private static void OnMixed()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("harass.mana"))
            {
                return;
            }

            var target = TargetSelector.GetTarget(1000, DamageType.Physical);
            if (target != null)
            {
                if (Spells.Q.IsReady() && Utilities.Enabled("q.combo") && target.IsValidTarget(ObjectManager.Player.AttackRange))
                {
                    Spells.Q.Cast();
                }

                if (Spells.W.IsReady() && Utilities.Enabled("w.combo") && target.IsValidTarget(Spells.W.Range))
                {
                    var pred = Spells.W.GetPrediction(target);
                    if (pred.Hitchance >= HitChance.High)
                    {
                        Spells.W.Cast(pred.CastPosition);
                    }
                }
            }
        }
        private static void OnJungle()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("jungle.mana"))
            {
                return;
            }

            var mob = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth);
            
            if (mob == null || mob.Count == 0)
            {
                return;
            }

            if (Spells.Q.IsReady() && Utilities.Enabled("q.jungle") && Spells.Q.IsInRange(mob[0]))
            {
                Spells.Q.Cast();
            }

            if (Spells.W.IsReady() && Utilities.Enabled("w.jungle"))
            {
                Spells.W.Cast(mob[0].Position);
            }
            
        }
        
        private static void OnClear()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("clear.mana"))
            {
                return;
            }

            if (Spells.Q.IsReady() && Utilities.Enabled("q.clear"))
            {
                var minions = MinionManager.GetMinions(ObjectManager.Player.Position, 500, MinionManager.MinionTypes.All,
                MinionManager.MinionTeam.Enemy).Count;
                if (minions > 4)
                {
                    Spells.Q.Cast();
                }
            }

            if (Spells.Q.IsReady() && Calculation.HasProtocolOneBuff && Utilities.Enabled("q.clear"))
            {
                var minion = MinionManager.GetMinions(ObjectManager.Player.Position, 500, MinionManager.MinionTypes.All,
                    MinionManager.MinionTeam.Enemy).FirstOrDefault();
                if (minion != null && minion.Health < minion.ProtocolDamage())
                {
                    Spells.Q.Cast();
                }
            }

            if (Spells.Q.IsReady() && Calculation.HasProtocolTwoBuff && Utilities.Enabled("q.clear"))
            {
                var minion = MinionManager.GetMinions(ObjectManager.Player.Position, 500, MinionManager.MinionTypes.All,
                    MinionManager.MinionTeam.Enemy).FirstOrDefault();
                if (minion != null && minion.Health < minion.ProtocolTwoDamage())
                {
                    Spells.Q.Cast();
                }
            }

            if (Spells.W.IsReady() && Utilities.Enabled("w.clear"))
            {
                var minions = MinionManager.GetMinions(ObjectManager.Player.Position, Spells.W.Range, MinionManager.MinionTypes.All,
                MinionManager.MinionTeam.NotAlly);

                var minioncount = Spells.W.GetLineFarmLocation(minions);
                if (minions == null || minions.Count == 0)
                {
                    return;
                }

                if (minioncount.MinionsHit >= Utilities.Slider("min.count"))
                {
                    Spells.W.Cast(minioncount.Position);
                }
            }
        }
    }
}