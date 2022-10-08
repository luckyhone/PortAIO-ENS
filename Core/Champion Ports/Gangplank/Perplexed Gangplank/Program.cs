using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using Entropy.AIO.Bases;
using PortAIO;
using PortAIO.Library_Ports.Entropy.Lib.Geometry;
using SharpDX;

namespace Perplexed_Gangplank
{
    public class Program
    {
        private static AIHeroClient Player;
        public static void Loads()
        {
            GameEvent.OnGameLoad += GameEvents_GameStart;
        }

        private static void GameEvents_GameStart()
        {
            Player = ObjectManager.Player;
            if (Player.CharacterName != "Gangplank")
                return;

            MenuManager.Initialize();
            SpellManager.Initialize();

            BarrelManager.Barrels = new List<Barrel>();
            GameObject.OnCreate += GameObject_OnCreate;
            AIBaseClient.OnDoCast += ObjAiBaseOnOnProcessSpellCast;
            Game.OnUpdate += Game_OnUpdate;
            Dash.OnDash += DashOnHeroDashed;
            Drawing.OnDraw += Render_OnPresent;
        }
        

        private static void DashOnHeroDashed(object sender, Dash.DashArgs dashArgs)
        {
            if(sender is AIHeroClient)
            {
                var nearestBarrel = BarrelManager.GetNearestBarrel(dashArgs.EndPos.To3D());
                if(nearestBarrel != null)
                {
                    var chainedBarrels = BarrelManager.GetChainedBarrels(nearestBarrel);
                    if (chainedBarrels.Any(x => BarrelManager.BarrelWillHit(x, dashArgs.EndPos.To3D())))
                    {
                        //If any of the chained barrels will hit, cast Q on best barrel.
                        var barrelToQ = BarrelManager.GetBestBarrelToQ(chainedBarrels);
                        if (barrelToQ != null)
                        {
                            if (SpellManager.Q.IsReady())
                                SpellManager.Q.Cast(barrelToQ.Object);
                            else if (GameObjectExtensions.Distance(barrelToQ.Object, Player) <= Player.AttackRange && Orbwalker.CanAttack() && MenuManager.Combo["explodeQCooldown"].GetValue<MenuBool>().Enabled)
                                Orbwalker.Attack(barrelToQ.Object);
                            return;
                        }
                    }
                    else
                    {
                        //No chained barrels will hit, so let's chain them.
                        var bestChainPosition = BarrelManager.GetBestChainPosition(dashArgs.EndPos.To3D(), nearestBarrel);
                        if (bestChainPosition != Vector3.Zero && dashArgs.EndPos.Distance(Player) <= SpellManager.E.Range && GameObjectExtensions.Distance(Player, bestChainPosition) <= SpellManager.E.Range && SpellManager.E.IsReady() && nearestBarrel.CanChain)
                        {
                            Drawing.DrawLine(WorldAndScreenConversions.WorldToScreen(nearestBarrel.ServerPosition), bestChainPosition.WorldToScreen(), 5, Color.Red.ToSystemColor());
                            SpellManager.E.Cast(bestChainPosition);
                        }
                    }
                }
            }
        }

        private static void ObjAiBaseOnOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs e)
        {
            if(sender.IsMe && e.Slot == SpellSlot.Q && e.Target.Name == "Barrel")
            {
                var barrel = (Barrel)e.Target;
                if (GameObjectExtensions.Distance(barrel.Object, Player) >= 530 && GameObjectExtensions.Distance(barrel.Object, Player) <= 660 && BarrelManager.GetChainedBarrels(barrel).Count == 1) //1 part combo only works at max range.
                {
                    var enemies = BarrelManager.GetEnemiesInChainRadius(barrel);
                    var bestEnemy = enemies.Where(x => x.IsValidTarget()).OrderBy(x => x.Health).FirstOrDefault();
                    if (bestEnemy != null)
                    {
                        var bestChainPosition = BarrelManager.GetBestChainPosition(bestEnemy, barrel);
                        if (bestChainPosition != Vector3.Zero && bestEnemy.IsInRange(Player,SpellManager.E.Range) && GameObjectExtensions.Distance(Player, bestChainPosition) <= SpellManager.E.Range && SpellManager.E.IsReady())
                            SpellManager.E.Cast(bestChainPosition);
                    }
                }
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Barrel")
                BarrelManager.Barrels.Add(new Barrel(sender as AIBaseClient, Environment.TickCount));
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            BarrelManager.Barrels.RemoveAll(x => x.Object.IsDead);
            RemoveCC();
            Killsteal();

            var closestHittingBarrel = BarrelManager.GetBarrelsThatWillHit().FirstOrDefault();
            if (closestHittingBarrel != null)
            {
                var chainedBarrels = BarrelManager.GetChainedBarrels(closestHittingBarrel);
                var barrelToQ = BarrelManager.GetBestBarrelToQ(chainedBarrels);
                if (barrelToQ != null && SpellManager.Q.IsReady())
                {
                    SpellManager.Q.Cast(barrelToQ.Object);
                    return;
                }
            }

            if ((Orbwalker.ActiveMode == OrbwalkerMode.Combo || Orbwalker.ActiveMode == OrbwalkerMode.Harass || Orbwalker.ActiveMode == OrbwalkerMode.LastHit ||
               Orbwalker.ActiveMode == OrbwalkerMode.LaneClear || MenuManager.Keys["comboToMouse"].GetValue<MenuKeyBind>().Active || MenuManager.Keys["qBarrel"].GetValue<MenuKeyBind>().Active) && MenuManager.Misc["aaBarrel"].GetValue<MenuBool>().Enabled)
            {
                AttackNearestBarrel();
            }

            if (MenuManager.Keys["comboToMouse"].GetValue<MenuKeyBind>().Active)
            {
                AttackNearestBarrel();
                ComboToMouse();
            }

            if (MenuManager.Keys["qBarrel"].GetValue<MenuKeyBind>().Active)
            {
                AttackNearestBarrel();
                QNearestBarrel();
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(SpellManager.E2.Range,DamageType.Physical);
            if (target.IsValidTarget())
            {
                var nearestBarrel = BarrelManager.GetNearestBarrel();
                if (nearestBarrel != null)
                {
                    var chainedBarrels = BarrelManager.GetChainedBarrels(nearestBarrel);
                    if (chainedBarrels.Any(x => BarrelManager.BarrelWillHit(x, target)))
                    {
                        //If any of the chained barrels will hit, cast Q on best barrel.
                        var barrelToQ = BarrelManager.GetBestBarrelToQ(chainedBarrels);
                        if (barrelToQ != null)
                        {
                            if (SpellManager.Q.IsReady())
                                SpellManager.Q.Cast(barrelToQ.Object);
                            else if (GameObjectExtensions.Distance(barrelToQ.Object, Player) <= Player.AttackRange && Orbwalker.CanAttack() && MenuManager.Combo["explodeQCooldown"].GetValue<MenuBool>().Enabled)
                                Orbwalker.Attack(barrelToQ.Object);
                            return;
                        }
                    }
                    else
                    {
                        //No chained barrels will hit, so let's chain them.
                        var bestChainPosition = BarrelManager.GetBestChainPosition(target, nearestBarrel);
                        if(bestChainPosition != Vector3.Zero && target.IsInRange(Player,SpellManager.E.Range) && GameObjectExtensions.Distance(Player, bestChainPosition) <= SpellManager.E.Range && SpellManager.E.IsReady() && nearestBarrel.CanChain)
                        {
                            Drawing.DrawLine(nearestBarrel.ServerPosition.WorldToScreen(), bestChainPosition.WorldToScreen(), 5, Color.Red.ToSystemColor());
                            SpellManager.E.Cast(bestChainPosition);

                        }
                    }
                }
            }
        }

        private static void ComboToMouse()
        {
            Orbwalker.Move(Game.CursorPos);
            var nearestBarrelToCursor = BarrelManager.GetNearestBarrel(Game.CursorPos);
            if (nearestBarrelToCursor != null)
            {
                var chainedBarrels = BarrelManager.GetChainedBarrels(nearestBarrelToCursor);
                if (chainedBarrels.Count > 1)
                {
                    var barrelToQ = BarrelManager.GetBestBarrelToQ(chainedBarrels);
                    if (barrelToQ != null && SpellManager.Q.IsReady())
                        SpellManager.Q.Cast(barrelToQ.Object);
                }
                else
                {
                    if (GameObjectExtensions.Distance(nearestBarrelToCursor.Object, Game.CursorPos) <= SpellManager.ChainRadius)
                    {
                        if (SpellManager.E.IsReady() && nearestBarrelToCursor.CanChain)
                            SpellManager.E.Cast(Game.CursorPos);
                    }
                    else
                    {
                        var bestPos = Vector3Extensions.Extend(nearestBarrelToCursor.ServerPosition, Game.CursorPos, (SpellManager.ChainRadius - 5));
                        if (SpellManager.E.IsReady() && nearestBarrelToCursor.CanChain)
                            SpellManager.E.Cast(bestPos);
                    }
                }
            }
            else
            {
                if (GameObjectExtensions.Distance(Player, Game.CursorPos) <= SpellManager.E.Range && SpellManager.E.IsReady())
                    SpellManager.E.Cast(Game.CursorPos);
            }
        }

        private static void QNearestBarrel()
        {
            Orbwalker.Move(Game.CursorPos);
            var barrel = BarrelManager.GetNearestBarrel();
            if(barrel != null)
            {
                if (barrel.CanQ && SpellManager.Q.IsReady())
                    SpellManager.Q.Cast(barrel.Object);
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(SpellManager.Q.Range,DamageType.Physical);
            var minManaPct = MenuManager.Harass["harassManaPct"].GetValue<MenuSlider>().Value;
            if (target.IsValidTarget() && SpellManager.Q.IsReady() && Player.ManaPercent >= minManaPct)
                SpellManager.Q.Cast(target);
        }

        private static void LastHit()
        {
            if (MenuManager.LastHit["lasthitQ"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
            {
                var minManaPct = MenuManager.LastHit["lasthitManaPct"].GetValue<MenuSlider>().Value;
                if (Player.ManaPercent >= minManaPct)
                {
                    var minion = GameObjects.EnemyMinions.Where(x => GameObjectExtensions.Distance(x, Player) <= SpellManager.Q.Range && GameObjectExtensions.Distance(x, Player) > Player.AttackRange && Utility.CanKillWithQ(x) && x.SkinName.Contains("Minion")).OrderBy(x => x.Health).FirstOrDefault();
                    if (minion != null)
                        SpellManager.Q.Cast(minion);
                }
            }
        }
        
        private static void RemoveCC()
        {
            if(MenuManager.W["wEnable"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady())
            {
                if (Player.HasBuffOfType(BuffType.Blind) && MenuManager.CCTypes["blind"].GetValue<MenuBool>().Enabled)
                    SpellManager.W.Cast();
                else if (Player.HasBuffOfType(BuffType.Charm) && MenuManager.CCTypes["charm"].GetValue<MenuBool>().Enabled)
                    SpellManager.W.Cast();
                else if (Player.HasBuffOfType(BuffType.Fear) && MenuManager.CCTypes["fear"].GetValue<MenuBool>().Enabled)
                    SpellManager.W.Cast();
                else if (Player.HasBuffOfType(BuffType.Flee) && MenuManager.CCTypes["flee"].GetValue<MenuBool>().Enabled)
                    SpellManager.W.Cast();
                else if (Player.HasBuffOfType(BuffType.Polymorph) && MenuManager.CCTypes["polymorph"].GetValue<MenuBool>().Enabled)
                    SpellManager.W.Cast();
                else if (Player.HasBuffOfType(BuffType.Silence) && MenuManager.CCTypes["silence"].GetValue<MenuBool>().Enabled)
                    SpellManager.W.Cast();
                else if (Player.HasBuffOfType(BuffType.Slow) && MenuManager.CCTypes["slow"].GetValue<MenuBool>().Enabled)
                    SpellManager.W.Cast();
                else if (Player.HasBuffOfType(BuffType.Snare) && MenuManager.CCTypes["snare"].GetValue<MenuBool>().Enabled)
                    SpellManager.W.Cast();
                else if (Player.HasBuffOfType(BuffType.Stun) && MenuManager.CCTypes["stun"].GetValue<MenuBool>().Enabled)
                    SpellManager.W.Cast();
                else if (Player.HasBuffOfType(BuffType.Suppression) && MenuManager.CCTypes["suppression"].GetValue<MenuBool>().Enabled)
                    SpellManager.W.Cast();
                else if (Player.HasBuffOfType(BuffType.Taunt) && MenuManager.CCTypes["taunt"].GetValue<MenuBool>().Enabled)
                    SpellManager.W.Cast();
            }
        }

        private static void Killsteal()
        {
            if (SpellManager.Q.IsReady() && MenuManager.Killsteal["killstealQ"].GetValue<MenuBool>().Enabled)
            {
                var target = Utility.GetAllEnemiesInRange(SpellManager.Q.Range).Where(x => Utility.CanKillWithQ(x)).OrderBy(x => x.Health).FirstOrDefault();
                if (target != null)
                {
                    SpellManager.Q.Cast(target);
                    return;
                }
            }
            if (SpellManager.R.IsReady() && MenuManager.Killsteal["killstealR"].GetValue<MenuBool>().Enabled)
            {
                var waves = MenuManager.Killsteal["killstealRWaves"].GetValue<MenuSlider>().Value;
                var target = Utility.GetAllEnemiesInRange(SpellManager.R.Range).Where(x => Utility.CanKillWithR(x, waves) && GameObjectExtensions.Distance(x, Player) > SpellManager.Q.Range).OrderBy(x => x.Health).FirstOrDefault();
                if (target != null)
                {
                    SpellManager.R.Cast(target.ServerPosition);
                    return;
                }
            }
        }

        private static void AttackNearestBarrel()
        {
            var nearestBarrel = BarrelManager.GetNearestBarrel();
            if (nearestBarrel != null)
            {
                if (GameObjectExtensions.Distance(nearestBarrel.Object, Player) <= Player.AttackRange && nearestBarrel.Health >= 2 && Orbwalker.CanAttack())
                    Orbwalker.Attack(nearestBarrel.Object);
            }
        }

        private static void Render_OnPresent(EventArgs args)
        {
            if (MenuManager.Drawing["drawQ"].GetValue<MenuBool>().Enabled)
                CircleRender.Draw(Player.Position, SpellManager.Q.Range, Color.White);
            if (MenuManager.Drawing["drawE"].GetValue<MenuBool>().Enabled)
                CircleRender.Draw(Player.Position, SpellManager.E.Range, Color.Orange);
            //Render.Text(Player.ServerPosition.ToScreenPosition(), Color.Red, $"Barrels will hit: {BarrelManager.GetBarrelsThatWillHit().Count}");
            foreach (var barrel in BarrelManager.Barrels)
            {
                //Render.Text(barrel.ServerPosition.ToScreenPosition(), Color.Red, $"Chain: {BarrelManager.GetChainedBarrels(barrel).Count}");
                if (MenuManager.Drawing["drawBarrelExplode"].GetValue<MenuBool>().Enabled)
                    CircleRender.Draw(barrel.Position, SpellManager.ExplosionRadius, Color.Gold);
                if (MenuManager.Drawing["drawBarrelChain"].GetValue<MenuBool>().Enabled)
                    CircleRender.Draw(barrel.Position, SpellManager.ChainRadius, Color.Orange);
            }
        }
    }
}