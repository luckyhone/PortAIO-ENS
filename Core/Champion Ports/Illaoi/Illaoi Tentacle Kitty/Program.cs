using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using SharpDX;
using SPrediction;
using Color = System.Drawing.Color;

namespace Illaoi_Tentacle_Kitty
{
    class Program
    {
        public static Spell Q,W,E,R;
        private static readonly AIHeroClient Illaoi = ObjectManager.Player;
        public static Menu Config;
        
        public static string[] HighChamps =
        {
            "Ahri", "Anivia", "Annie", "Ashe", "Azir", "Brand", "Caitlyn", "Cassiopeia", "Corki", "Draven",
            "Ezreal", "Graves", "Jinx", "Kalista", "Karma", "Karthus", "Katarina", "Kennen", "KogMaw", "Leblanc",
            "Lucian", "Lux", "Malzahar", "MasterYi", "MissFortune", "Orianna", "Quinn", "Sivir", "Syndra", "Talon",
            "Teemo", "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar", "VelKoz", "Viktor", "Xerath",
            "Zed", "Ziggs","Kindred"
        };

        public static void Game_OnGameLoad()
        {
            if (Illaoi.CharacterName != "Illaoi")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 850);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 900);
            R = new Spell(SpellSlot.R, 450);

            Q.SetSkillshot(.484f, 0, 500, false, SpellType.Circle);
            E.SetSkillshot(.066f, 50, 1900, true, SpellType.Line);
            
            Config = new Menu("Illaoi - Tentacle Kitty", "Illaoi - Tentacle Kitty", true);
            {
                var comboMenu = new Menu("Combo Settings", "Combo Settings");
                {
                    comboMenu.Add(new MenuBool("q.combo", "Use Q").SetValue(true));
                    comboMenu.Add(new MenuBool("q.ghost.combo", "Use Q (Ghost)").SetValue(true));
                    comboMenu.Add(new MenuBool("w.combo", "Use W").SetValue(true));
                    comboMenu.Add(new MenuBool("e.combo", "Use E").SetValue(true));
                    comboMenu.Add(new MenuBool("r.combo", "Use R").SetValue(true));
                    comboMenu.Add(new MenuSlider("r.min.hit", "(R) Min. Hit",3,1,5));
                    Config.Add(comboMenu);
                }
                var harassMenu = new Menu("Harass Settings", "Harass Settings");
                {
                    harassMenu.Add(new MenuBool("q.harass", "Use Q").SetValue(true));
                    harassMenu.Add(new MenuBool("q.ghost.harass", "Use Q (Ghost)").SetValue(true));
                    harassMenu.Add(new MenuBool("w.harass", "Use W").SetValue(true));
                    harassMenu.Add(new MenuBool("e.harass", "Use E").SetValue(true));
                    harassMenu.Add(new MenuSlider("harass.mana", "Mana Manager",20,1,99));
                    Config.Add(harassMenu);
                }
                var clearMenu = new Menu("Clear Settings", "Clear Settings");
                {
                    clearMenu.Add(new MenuBool("q.clear", "Use Q").SetValue(true)); //
                    clearMenu.Add(new MenuSlider("q.minion.hit", "(Q) Min. Hit",3,1,6));
                    clearMenu.Add(new MenuSlider("clear.mana", "Mana Manager",20,1,99));
                    Config.Add(clearMenu);
                }
                
                var eMenu = new Menu("E Settings", "E Settings");
                {
                    eMenu.Add(new MenuSeparator("e.whte", "E Whitelist"));
                    foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(o => o.IsEnemy))
                    {
                        eMenu.Add(new MenuBool("enemy." + enemy.CharacterName, string.Format("E: {0}", enemy.CharacterName)).SetValue(HighChamps.Contains(enemy.CharacterName)));

                    }
                    Config.Add(eMenu);
                }
                
                var ksMenu = new Menu("KillSteal Settings", "KillSteal Settings");
                {
                    ksMenu.Add(new MenuBool("q.ks", "Use Q").SetValue(true));
                    Config.Add(ksMenu);
                }
                var drawMenu = new Menu("Draw Settings", "Draw Settings");
                {
                    var damageDraw = new Menu("Damage Draw", "Damage Draw");
                    {
                        //Damagedraw.AddItem(new MenuItem("aa.indicator", "AA Indicator").SetValue(new Circle(true, Color.Gold)));
                        drawMenu.Add(damageDraw);
                    }
                    drawMenu.Add(new MenuBool("q.draw", "Q Range"));
                    drawMenu.Add(new MenuBool("w.draw", "W Range"));
                    drawMenu.Add(new MenuBool("e.draw", "E Range"));
                    drawMenu.Add(new MenuBool("r.draw", "R Range"));
                    drawMenu.Add(new MenuBool("passive.draw", "Passive Draw"));
                    Config.Add(drawMenu);
                }
                Config.Attach();
            }
            Game.Print("<font color='#ff3232'>Illaoi - Tentacle Kitty: </font> <font color='#d4d4d4'>If you like this assembly feel free to upvote on Assembly DB</font>");
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem1 = Config.GetValue<MenuBool>("q.draw").Enabled;
            var menuItem2 = Config.GetValue<MenuBool>("w.draw").Enabled;
            var menuItem3 = Config.GetValue<MenuBool>("e.draw").Enabled;
            var menuItem4 = Config.GetValue<MenuBool>("r.draw").Enabled;
            var menuItem5 = Config.GetValue<MenuBool>("passive.draw").Enabled;
            //var menuItem6 = Config.GetValue<MenuBool>().Enabled;
            
            if (menuItem1 && Q.IsReady())
            {
                CircleRender.Draw(new Vector3(Illaoi.Position.X, Illaoi.Position.Y, Illaoi.Position.Z), Q.Range,SharpDX.Color.White);
            }
            if (menuItem2 && W.IsReady())
            {
                CircleRender.Draw(new Vector3(Illaoi.Position.X, Illaoi.Position.Y, Illaoi.Position.Z), W.Range, SharpDX.Color.Gold);
            }
            if (menuItem3 && E.IsReady())
            {
                CircleRender.Draw(new Vector3(Illaoi.Position.X, Illaoi.Position.Y, Illaoi.Position.Z), E.Range, SharpDX.Color.DodgerBlue);
            }
            if (menuItem4 && R.IsReady())
            {
                CircleRender.Draw(new Vector3(Illaoi.Position.X, Illaoi.Position.Y, Illaoi.Position.Z), R.Range, SharpDX.Color.GreenYellow);
            }
            if (menuItem4)
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(1500) && x.IsValid && x.IsVisible && !x.IsDead && !x.IsZombie()))
                {
                    Drawing.DrawText(enemy.HPBarPosition.X, enemy.HPBarPosition.Y, Color.Gold,
                        string.Format("{0} Basic Attack = Kill", AaIndicator(enemy)));
                }
            }
            if (menuItem5)
            {
                var enemy = GameObjects.EnemyHeroes.FirstOrDefault(x => x.IsValidTarget(2000));
                foreach (var passive in ObjectManager.Get<AIMinionClient>().Where(x=> x.Name == "God"))
                {
                    CircleRender.Draw(new Vector3(passive.Position.X, passive.Position.Y, passive.Position.Z), 850, SharpDX.Color.Gold);
                    if (enemy != null)
                    {
                        var xx = Drawing.WorldToScreen(passive.Position.Extend(enemy.Position, 850));
                        var xy = Drawing.WorldToScreen(passive.Position);
                        Drawing.DrawLine(xy.X, xy.Y, xx.X, xx.Y, 5, Color.Gold);
                    }
                    
                }
            }
        }
        private static int AaIndicator(AIHeroClient enemy)
        {
            double aCalculator = ObjectManager.Player.CalculateDamage(enemy, DamageType.Physical, Illaoi.TotalAttackDamage);
            double killableAaCount = enemy.Health / aCalculator;
            int totalAa = (int)Math.Ceiling(killableAaCount);
            return totalAa;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;

                case OrbwalkerMode.Harass:
                    Harass();
                    break;

                case OrbwalkerMode.LaneClear:
                    Clear();
                    break;
            }
        }
        private static void Combo()
        {
            if (Q.IsReady() && Config.GetValue<MenuBool>("q.combo").Enabled)
            {
                var enemy = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                var enemyGhost = enemy != null ? ObjectManager.Get<AIMinionClient>().FirstOrDefault(x => x.Name == enemy.Name) : null;
                if (enemy != null && enemyGhost == null )
                {
                    if (Q.CanCast(enemy) && Q.GetPrediction(enemy).Hitchance >= HitChance.High)
                    {
                        Q.Cast(Q.GetPrediction(enemy).CastPosition);
                    }
                }
                if (enemy == null && enemyGhost != null && Config.GetValue<MenuBool>("q.ghost.combo").Enabled)
                {
                    if (Q.CanCast(enemyGhost) && Q.GetPrediction(enemyGhost).Hitchance >= HitChance.High)
                    {
                        Q.Cast(enemyGhost.Position);
                    }
                }
            }

            if (W.IsReady() && Config.GetValue<MenuBool>("w.combo").Enabled)
            {
                var tentacle = ObjectManager.Get<AIMinionClient>().First(x=> x.Name == "God");
                if (tentacle != null)
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(x=> x.IsValidTarget(850)))
                    {
                        W.Cast();
                    }
                }

            }
            if (E.IsReady() && Config.GetValue<MenuBool>("e.combo").Enabled)
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(o => o.IsValidTarget(E.Range) && !o.IsDead && !o.IsZombie()))
                {
                    if (Config.GetValue<MenuBool>("enemy." + enemy.CharacterName).Enabled && E.GetPrediction(enemy).Hitchance >= HitChance.High
                        && E.GetPrediction(enemy).CollisionObjects.Count == 0)
                    {
                        E.Cast(enemy);
                    }
                } 
            }
            if (R.IsReady() && Config.GetValue<MenuBool>("r.combo").Enabled)
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(o => o.IsValidTarget(R.Range) && !o.IsDead && !o.IsZombie()))
                {
                    if (Illaoi.CountEnemyHeroesInRange(R.Range) >= Config.GetValue<MenuSlider>("r.min.hit").Value)
                    {
                        R.Cast();
                    }
                } 
            }
        }

        private static void Harass()
        {
            if (Illaoi.ManaPercent < Config.GetValue<MenuSlider>("harass.mana").Value)
            {
                return;
            }

            if (Q.IsReady() && Config.GetValue<MenuBool>("q.harass").Enabled)
            {
                var enemy = GameObjects.EnemyHeroes.FirstOrDefault(x => x.IsValidTarget(Q.Range));
                var enemyGhost = ObjectManager.Get<AIMinionClient>().FirstOrDefault(x => x.Name == enemy.Name) ?? null;
                if (enemy != null && enemyGhost == null)
                {
                    if (Q.CanCast(enemy) && Q.GetPrediction(enemy).Hitchance >= HitChance.High
                                         && Q.GetPrediction(enemy).CollisionObjects.Count == 0)
                    {
                        Q.Cast(enemy);
                    }
                }

                if (enemy == null && enemyGhost != null && Config.GetValue<MenuBool>("q.ghost.harass").Enabled)
                {
                    if (Q.CanCast(enemyGhost) && Q.GetPrediction(enemyGhost).Hitchance >= HitChance.High
                                              && Q.GetPrediction(enemyGhost).CollisionObjects.Count == 0)
                    {
                        Q.Cast(enemyGhost);
                    }
                }
            }

            if (W.IsReady() && Config.GetValue<MenuBool>("w.harass").Enabled)
            {
                var tentacle = ObjectManager.Get<AIMinionClient>().First(x => x.Name == "God");
                if (tentacle != null)
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(850)))
                    {
                        W.Cast();
                    }
                }

            }

            if (E.IsReady() && Config.GetValue<MenuBool>("e.harass").Enabled)
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(o =>
                             o.IsValidTarget(E.Range) && !o.IsDead && !o.IsZombie()))
                {
                    if (Config.GetValue<MenuBool>("enemy." + enemy.CharacterName).Enabled &&
                        E.GetPrediction(enemy).Hitchance >= HitChance.High
                        && E.GetPrediction(enemy).CollisionObjects.Count == 0)
                    {
                        E.Cast(enemy);
                    }
                }
            }
        }

        private static void Clear()
        {
            if (Illaoi.ManaPercent < Config.GetValue<MenuSlider>("clear.mana").Value)
            {
                return;
            }

            var minionCount = MinionManager.GetMinions(Illaoi.Position, Q.Range);
            if (Q.IsReady() && Config.GetValue<MenuBool>("q.clear").Enabled)
            {
                var mfarm = Q.GetLineFarmLocation(minionCount);
                if (minionCount.Count >= Config.GetValue<MenuSlider>("q.minion.hit").Value)
                {
                    Q.Cast(mfarm.Position);
                }
            }
        }
    }
}