using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using EnsoulSharp.SDK.Utility;
using PortAIO;
using SharpDX;

namespace Bangplank
{
    class Program
    {
        public static String Version = "1.0.4.4";
        private static String championName = "Gangplank";
        public static AIHeroClient Player;
        private static Menu _menu;
        //private static Orbwalking.Orbwalker _orbwalker;
        private static Spell Q, W, E, R;
        private const float ExplosionRange = 400;
        private const float LinkRange = 650;
        private static List<Keg> LiveBarrels = new List<Keg>(); // Keg means powder keg, = barrel
        private static bool _qautoallowed = true;

        public static void Loads()
        {
            Game_OnGameLoad();
        }

        private static void MenuIni()
        {
            _menu = new Menu("bangplank.menu", "BangPlank", true);
            
            //comboMenu
            var comboMenu = new Menu("bangplank.menu.combo", "Combo");
            comboMenu.Add(new MenuBool("bangplank.menu.combo.q", "Use Q = ON"));
            comboMenu.Add(new MenuBool("bangplank.menu.combo.e", "Use E = ON"));
            comboMenu.Add(new MenuBool("bangplank.menu.combo.r", "Use R").SetValue(true));
            comboMenu.Add(new MenuSlider("bangplank.menu.combo.rmin", "Minimum enemies to cast R",2,1,5));
            
            // Harass Menu
            var harassMenu = new Menu("bangplank.menu.harass", "Harass");
            harassMenu.Add(new MenuSeparator("bangplank.menu.harass.info", "Use your mixed key for harass"));
            harassMenu.Add(new MenuBool("bangplank.menu.harass.q", "Use Q"));
            harassMenu.Add(new MenuSeparator("bangplank.menu.harass.separator1", "Extended EQ:"));
            harassMenu.Add(new MenuBool("bangplank.menu.harass.extendedeq", "Enabled"));
            harassMenu.Add(new MenuSeparator("bangplank.menu.harass.instructioneq", "Place E near your pos, then wait it will automatically"));
            harassMenu.Add(new MenuSeparator("bangplank.menu.harass.instructionqe2", "place E in range of 1st barrel + Q to harass enemy"));
            harassMenu.Add(new MenuSlider("bangplank.menu.harass.qmana", "Minimum mana to use Q harass",20,1,100));
            
            // Farm Menu
            var farmMenu = new Menu("bangplank.menu.farm", "Farm");
            farmMenu.Add(new MenuBool("bangplank.menu.farm.qlh", "Use Q to lasthit"));
            farmMenu.Add(new MenuSlider("bangplank.menu.farm.qlhmana", "Minimum mana for Q lasthit",10,1,100));
            farmMenu.Add(new MenuBool("bangplank.menu.farm.ewc", "Use E to Laneclear & Jungle").SetValue(true));
            farmMenu.Add(new MenuSlider("bangplank.menu.farm.eminwc", "Minimum minions to use E",5,1,15));
            farmMenu.Add(new MenuBool("bangplank.menu.farm.qewc", "Use Q on E to clear").SetValue(true));
            farmMenu.Add(new MenuSlider("bangplank.menu.farm.qewcmana", "Minimum mana to use Q on E",10));
            
            // Misc Menu
            var miscMenu = new Menu("bangplank.menu.misc", "Misc");
            miscMenu.Add(new MenuBool("bangplank.menu.misc.wheal", "Use W to heal").SetValue(true));
            miscMenu.Add(new MenuSlider("bangplank.menu.misc.healmin", "Health %",30,1,100));
            miscMenu.Add(new MenuSlider("bangplank.menu.misc.healminmana", "Minimum Mana %",35,1,100));
            miscMenu.Add(new MenuBool("bangplank.menu.misc.ks", "KillSteal").SetValue(true));
            miscMenu.Add(new MenuBool("bangplank.menu.misc.qks", "Use Q to KillSteal").SetValue(true));
            miscMenu.Add(new MenuBool("bangplank.menu.misc.rks", "Use R to KillSteal").SetValue(false));
            miscMenu.Add(new MenuBool("bangplank.menu.misc.rksoffinfo", "Ks Notification").SetValue(true));
            miscMenu.Add(new MenuKeyBind("bangplank.menu.misc.fleekey", "[WIP] Flee",Keys.A,KeyBindType.Press));
            // Barrel Manager Options
            var barrelManagerMenu = new Menu("bangplank.menu.misc.barrelmanager", "Barrel Manager");
            barrelManagerMenu.Add(new MenuBool("bangplank.menu.misc.barrelmanager.edisabled", "Block E usage").SetValue(false));
            barrelManagerMenu.Add(new MenuSlider("bangplank.menu.misc.barrelmanager.stacks", "Number of stacks to keep",1,0,4));
            barrelManagerMenu.Add(new MenuBool("bangplank.menu.misc.barrelmanager.autoboom", "Auto explode when enemy in explosion range").SetValue(true));
            
            // Cleanser W Manager Menu
            var cleanserManagerMenu = new Menu("bangplank.menu.misc.cleansermanager", "W cleanser");
            cleanserManagerMenu.Add(new MenuBool("bangplank.menu.misc.cleansermanager.enabled", "Enabled").SetValue(true));
            cleanserManagerMenu.Add(new MenuSeparator("bangplank.menu.misc.cleansermanager.separation1", "-"));
            cleanserManagerMenu.Add(new MenuSeparator("bangplank.menu.misc.cleansermanager.separation2", "Buff Types: "));
            cleanserManagerMenu.Add(new MenuBool("bangplank.menu.misc.cleansermanager.charm", "Charm").SetValue(true));
            cleanserManagerMenu.Add(new MenuBool("bangplank.menu.misc.cleansermanager.flee", "Flee").SetValue(true));
            cleanserManagerMenu.Add(new MenuBool("bangplank.menu.misc.cleansermanager.polymorph", "Polymorph").SetValue(true));
            cleanserManagerMenu.Add(new MenuBool("bangplank.menu.misc.cleansermanager.snare", "Snare").SetValue(true));
            cleanserManagerMenu.Add(new MenuBool("bangplank.menu.misc.cleansermanager.stun", "Stun").SetValue(true));
            cleanserManagerMenu.Add(new MenuBool("bangplank.menu.misc.cleansermanager.taunt", "Taunt").SetValue(true));
            cleanserManagerMenu.Add(new MenuBool("bangplank.menu.misc.cleansermanager.exhaust", "Exhaust").SetValue(false));
            cleanserManagerMenu.Add(new MenuBool("bangplank.menu.misc.cleansermanager.suppression", "Supression").SetValue(true));
            
            // Drawing Menu
            Menu drawingMenu = new Menu("bangplank.menu.drawing", "Drawings");
            drawingMenu.Add(new MenuBool("bangplank.menu.drawing.enabled", "Enabled").SetValue(true));
            drawingMenu.Add(new MenuBool("bangplank.menu.drawing.q", "Draw Q range").SetValue(true));
            drawingMenu.Add(new MenuBool("bangplank.menu.drawing.e", "Draw E range").SetValue(true));
            drawingMenu.Add(new MenuBool("bangplank.menu.drawing.ehelper", "Draw manual E indicator").SetValue(false));
            
            _menu.Add(comboMenu);
            _menu.Add(harassMenu);
            _menu.Add(farmMenu);
            _menu.Add(miscMenu);
            miscMenu.Add(barrelManagerMenu);
            miscMenu.Add(cleanserManagerMenu);
            //miscMenu.AddSubMenu(swagplankMenu);
            _menu.Add(drawingMenu);
            _menu.Attach();
        }

        private static void Game_OnGameLoad()
        {
            if (ObjectManager.Player.CharacterName != championName)
            {
                return;
            }
            Game.Print("<b><font color='#FF6600'>Bang</font><font color='#FF0000'>Plank</font></b> " + Version + " loaded - By <font color='#6666FF'>Baballev</font>");
            Game.Print("Don't forget to <font color='#00CC00'><b>Upvote</b></font> <b><font color='#FF6600'>Bang</font><font color='#FF0000'>Plank</font></b> in the AssemblyDB if you like it ^_^");
            MenuIni();
            
            Player = ObjectManager.Player;
            // Spells ranges
            Q = new Spell(SpellSlot.Q, 625);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R);
            Q.SetTargetted(0.25f, 2150f);
            E.SetSkillshot(0.5f, 40, float.MaxValue, false, SpellType.Circle);
            R.SetSkillshot(0.9f, 100, float.MaxValue, false, SpellType.Circle);
            Game.OnUpdate += Logic;
            Drawing.OnDraw += Draw;
            GameObject.OnCreate += GameObjCreate;
            GameObject.OnDelete += GameObjDelete;
        }

        private static void Logic(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }
            var activeOrbwalker = Orbwalker.ActiveMode;
            switch (activeOrbwalker)
            {
                case OrbwalkerMode.Combo:
                    try{
                        Combo();
                        _qautoallowed = false;
                    }catch(Exception){}
                    break;
                case OrbwalkerMode.LaneClear:
                    try
                    {
                        WaveClear();
                        _qautoallowed = true;
                    }catch(Exception){}

                    break;
                case OrbwalkerMode.Harass:
                    Mixed();
                    _qautoallowed = false;
                    break;
                case OrbwalkerMode.LastHit:
                    LastHit();
                    _qautoallowed = true;
                    break;
                case OrbwalkerMode.None:
                    _qautoallowed = true;
                    break;
            }

            if (_menu["bangplank.menu.misc.cleansermanager"]
                .GetValue<MenuBool>("bangplank.menu.misc.cleansermanager.enabled").Enabled)
            {
                CleanserManager();
            }

            if (_menu["bangplank.menu.misc"].GetValue<MenuBool>("bangplank.menu.misc.wheal").Enabled)
            {
                HealManager();
            }

            if (_menu["bangplank.menu.misc"].GetValue<MenuBool>("bangplank.menu.misc.ks").Enabled)
            {
                KillSteal();
            }

            if (!_menu["bangplank.menu.misc.barrelmanager"]
                    .GetValue<MenuBool>("bangplank.menu.misc.barrelmanager.edisabled").Enabled &&
                _menu["bangplank.menu.misc.barrelmanager"]
                    .GetValue<MenuBool>("bangplank.menu.misc.barrelmanager.autoboom")
                    .Enabled && _qautoallowed)
            {
                BarrelManager();
            }
        }

        private static void KillSteal()
        {
            var kstarget = GameObjects.EnemyHeroes;
            if (_menu["bangplank.menu.misc"].GetValue<MenuBool>("bangplank.menu.misc.qks").Enabled && Q.IsReady())
            {
                if (kstarget != null)
                {
                    foreach (var ks in kstarget)
                    {
                        if (ks != null)
                        {
                            if (ks.Health <= Player.GetSpellDamage(ks, SpellSlot.Q) && ks.Health > 0 && Q.IsInRange(ks))
                            {
                                Q.CastOnUnit(ks);
                            }
                        }
                    }
                }
            }

            if (_menu["bangplank.menu.misc"].GetValue<MenuBool>("bangplank.menu.misc.rks").Enabled && R.IsReady())
            {
                if (kstarget != null)
                {
                    foreach (var ks in kstarget)
                    {
                        if (ks != null)
                        {
                            if (ks.Health <= Player.GetSpellDamage(ks, SpellSlot.Q) && Q.IsInRange(ks)) return;

                            if (ks.Health <= Player.GetSpellDamage(ks, SpellSlot.R) * 7 && ks.Health > 0)
                            {
                                var ksposition = Prediction.GetPrediction(ks, R.Delay).CastPosition;
                                if (ksposition.IsValid())
                                {
                                    R.Cast(ksposition);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void HealManager()
        {
            if (Player.InFountain()) return;
            if (Player.IsRecalling()) return;
            if (Player.InShop()) return;
            if (W.IsReady() && Player.HealthPercent <= _menu["bangplank.menu.misc"].GetValue<MenuSlider>("bangplank.menu.misc.healmin").Value &&
                Player.ManaPercent >= _menu["bangplank.menu.misc"].GetValue<MenuSlider>("bangplank.menu.misc.healminmana").Value)
            {
                DelayAction.Add(100 + Game.Ping, () =>
                    {
                        W.Cast();
                    }
                );
            }
        }

        private static void CleanserManager()
        {
            if(W.IsReady() && (
                (Player.HasBuffOfType(BuffType.Charm) && _menu["bangplank.menu.misc.cleansermanager"].GetValue<MenuBool>("bangplank.menu.misc.cleansermanager.charm").Enabled)
                || (Player.HasBuffOfType(BuffType.Flee) && _menu["bangplank.menu.misc.cleansermanager"].GetValue<MenuBool>("bangplank.menu.misc.cleansermanager.flee").Enabled)
                || (Player.HasBuffOfType(BuffType.Polymorph) && _menu["bangplank.menu.misc.cleansermanager"].GetValue<MenuBool>("bangplank.menu.misc.cleansermanager.polymorph").Enabled)
                || (Player.HasBuffOfType(BuffType.Snare) && _menu["bangplank.menu.misc.cleansermanager"].GetValue<MenuBool>("bangplank.menu.misc.cleansermanager.snare").Enabled)
                || (Player.HasBuffOfType(BuffType.Stun) && _menu["bangplank.menu.misc.cleansermanager"].GetValue<MenuBool>("bangplank.menu.misc.cleansermanager.stun").Enabled)
                || (Player.HasBuffOfType(BuffType.Taunt) && _menu["bangplank.menu.misc.cleansermanager"].GetValue<MenuBool>("bangplank.menu.misc.cleansermanager.taunt").Enabled)
                || (Player.HasBuffOfType(BuffType.Suppression) && _menu["bangplank.menu.misc.cleansermanager"].GetValue<MenuBool>("bangplank.menu.misc.cleansermanager.suppression").Enabled)
                || (Player.HasBuff("summonerexhaust") &&_menu["bangplank.menu.misc.cleansermanager"].GetValue<MenuBool>("bangplank.menu.misc.cleansermanager.exhaust").Enabled)
                ))
            {
                W.Cast();
            }
        }

        private static void LastHit()
        {
            // LH Logic
            var minions = GameObjects.GetMinions(Player.Position, Q.Range);
            
            // Q Last Hit
            if (_menu["bangplank.menu.farm"].GetValue<MenuBool>("bangplank.menu.farm.qlh").Enabled && Q.IsReady() &&
                Player.ManaPercent >= _menu["bangplank.menu.farm"].GetValue<MenuSlider>("bangplank.menu.farm.qlhmana")
                    .Value)
            {
                if (minions != null)
                {
                    foreach (var m in minions)
                    {
                        if (m != null)
                        {
                            if (m.Health <= Player.GetSpellDamage(m, SpellSlot.Q))
                            {
                                Q.CastOnUnit(m);
                            }
                        }
                    }
                }
            }
        }

        private static void Mixed()
        {
            try
            {
                // harass
                var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
                // Q lasthit minions
                var minions = GameObjects.GetMinions(Player.Position, Q.Range);
                Keg nbar = NearestKeg(Player.Position.To2D());

                if (_menu["bangplank.menu.farm"]["bangplank.menu.farm.qlh"].GetValue<MenuBool>().Enabled &&
                    Q.IsReady() &&
                    Player.ManaPercent >= _menu["bangplank.menu.farm"]["bangplank.menu.farm.qlhmana"]
                        .GetValue<MenuSlider>()
                        .Value && target == null)
                {
                    if (minions != null)
                    {
                        foreach (var m in minions)
                        {
                            if (m != null)
                            {
                                if (m.Health <= Player.GetSpellDamage(m, SpellSlot.Q))
                                {
                                    Q.CastOnUnit(m);
                                }
                            }
                        }
                    }
                }

                // Q
                if (_menu["bangplank.menu.harass"].GetValue<MenuBool>("bangplank.menu.harass.q").Enabled &&
                    Q.IsReady() &&
                    Player.ManaPercent >= _menu["bangplank.menu.harass"]
                        .GetValue<MenuSlider>("bangplank.menu.harass.qmana")
                        .Value && TargetSelector.GetTarget(Q.Range, DamageType.Physical) != null)
                {
                    if (LiveBarrels.Count == 0) Q.Cast(TargetSelector.GetTarget(Q.Range, DamageType.Physical));
                    if (LiveBarrels.Count >= 1 && nbar.KegObj.Distance(Player) > E.Range)
                        Q.Cast(TargetSelector.GetTarget(Q.Range, DamageType.Physical));
                }

                if (Q.IsReady() && E.IsReady() &&
                    _menu["bangplank.menu.harass"]["bangplank.menu.harass.extendedeq"].GetValue<MenuBool>().Enabled &&
                    !_menu["bangplank.menu.misc.barrelmanager"]["bangplank.menu.misc.barrelmanager.edisabled"]
                        .GetValue<MenuBool>().Enabled && Player.ManaPercent >=
                    _menu["bangplank.menu.harass"]["bangplank.menu.harass.qmana"].GetValue<MenuSlider>().Value)
                {
                    if (LiveBarrels.Count == 0) return;

                    if (Player.Position.Distance(nbar.KegObj.Position) < Q.Range && nbar.KegObj.Health < 3)
                    {
                        if (target != null)
                        {
                            var prediction = Prediction.GetPrediction(target, 0.8f).CastPosition;
                            if (nbar.KegObj.Distance(prediction) < LinkRange)
                            {
                                E.Cast(prediction);

                                if (Player.Level < 13 && Player.Level >= 7 && (int)nbar.KegObj.Health == 2)
                                {
                                    DelayAction.Add((int) (580 - Game.Ping), () => { Q.Cast(nbar.KegObj); }
                                    );
                                }

                                if (Player.Level >= 13 && (int)nbar.KegObj.Health == 2)
                                {
                                    DelayAction.Add((int) (80 - Game.Ping), () => { Q.Cast(nbar.KegObj); }
                                    );
                                }

                                if ((int)nbar.KegObj.Health == 1)
                                {
                                    Q.Cast(nbar.KegObj);
                                }
                            }
                        }
                    }
                }

                BarrelManager();
            }catch(Exception){}
        }

        private static void WaveClear()
        {
            var minions = GameObjects.GetMinions(Q.Range).Where(m => m.Health > 3).ToList();
            var jungleMobs = GameObjects.GetJungles(Q.Range).Where(j => j.Health > 3).ToList();
            minions.AddRange(jungleMobs);

            if (!_menu["bangplank.menu.misc.barrelmanager"]["bangplank.menu.misc.barrelmanager.edisabled"]
                    .GetValue<MenuBool>().Enabled &&
                _menu["bangplank.menu.farm"]["bangplank.menu.farm.ewc"].GetValue<MenuBool>().Enabled &&
                E.IsReady())
            {
                var posE = E.GetCircularFarmLocation(minions, ExplosionRange);
                if (posE.MinionsHit >= _menu["bangplank.menu.farm"]["bangplank.menu.farm.eminwc"].GetValue<MenuSlider>()
                        .Value &&
                    (LiveBarrels.Count == 0 || NearestKeg(Player.Position.To2D()).KegObj.Distance(Player) > Q.Range) &&
                    E.Instance.Ammo > _menu["bangplank.menu.misc.barrelmanager"]["bangplank.menu.misc.barrelmanager.stacks"]
                        .GetValue<MenuSlider>().Value)
                {
                    E.Cast(posE.Position);
                }

                if (jungleMobs.Count >= 1)
                {
                    if (!_menu["bangplank.menu.misc.barrelmanager"]["bangplank.menu.misc.barrelmanager.edisabled"]
                            .GetValue<MenuBool>().Enabled &&
                        _menu["bangplank.menu.farm"]["bangplank.menu.farm.ewc"].GetValue<MenuBool>().Enabled &&
                        E.IsReady() &&
                        (LiveBarrels.Count == 0 ||
                         NearestKeg(Player.Position.To2D()).KegObj.Distance(Player) > Q.Range) &&
                        E.Instance.Ammo > _menu["bangplank.menu.misc.barrelmanager"]["bangplank.menu.misc.barrelmanager.stacks"]
                            .GetValue<MenuSlider>().Value)
                    {
                        E.Cast(jungleMobs.FirstOrDefault().Position);
                    }
                }
            }

            if (Q.IsReady() && jungleMobs.Any() &&
                Player.ManaPercent > _menu["bangplank.menu.farm"]["bangplank.menu.farm.qlhmana"].GetValue<MenuSlider>()
                    .Value && _menu["bangplank.menu.farm"]["bangplank.menu.farm.qlh"].GetValue<MenuBool>().Enabled)
            {
                Q.CastOnUnit(jungleMobs.FirstOrDefault(j => j.Health < Player.GetSpellDamage(j, SpellSlot.Q)));
            }
            //if ((GetBool("bangplank.menu.farm.qlh") && minions.Any() && Player.ManaPercent > Getslider("bangplank.menu.farm.qlhmana") && Q.IsReady()) && (E.Instance.Ammo <= Getslider("bangplank.menu.misc.barrelmanager.stacks") || E.Level < 1))
            if ((_menu["bangplank.menu.farm"]["bangplank.menu.farm.qlh"].GetValue<MenuBool>().Enabled &&
                 minions.Any() &&
                 Player.ManaPercent > _menu["bangplank.menu.farm"]["bangplank.menu.farm.qlhmana"]
                     .GetValue<MenuSlider>().Value && Q.IsReady()) &&
                (E.Instance.Ammo <= _menu["bangplank.menu.misc.barrelmanager"]["bangplank.menu.misc.barrelmanager.stacks"]
                    .GetValue<MenuSlider>().Value || E.Level < 1))
            {
                Q.CastOnUnit(minions.FirstOrDefault(m => m.Health < Player.GetSpellDamage(m, SpellSlot.Q)));
            }

            if (LiveBarrels.Count >= 1)
            {
                if (LiveBarrels.Any() || NearestKeg(Player.Position.To2D()).KegObj.Distance(Player) < Q.Range + 150)
                {
                    var lol =
                        GameObjects.GetMinions(NearestKeg(Player.Position.To2D()).KegObj.Position, ExplosionRange,
                                MinionTypes.All, MinionTeam.All)
                            .Where(m => m.Health < Player.GetSpellDamage(m, SpellSlot.Q))
                            .ToList();

                    if (_menu["bangplank.menu.farm"]["bangplank.menu.farm.qewc"].GetValue<MenuBool>().Enabled &&
                        Player.ManaPercent > _menu["bangplank.menu.farm"]["bangplank.menu.farm.qewcmana"]
                            .GetValue<MenuSlider>().Value && Q.IsReady() &&
                        Q.IsInRange(NearestKeg(Player.Position.To2D()).KegObj) &&
                        NearestKeg(Player.Position.To2D()).KegObj.Health < 2 &&
                        ((Q.Level >= 3 && minions.Count > 3 && lol.Count > 3) ||
                         (Q.Level == 2 && minions.Count > 2 && lol.Count >= 2) ||
                         (Q.Level == 1 && minions.Count >= 2 && lol.Any()) || (minions.Count <= 2 && lol.Any())))
                    {
                        Q.Cast(NearestKeg(Player.Position.To2D()).KegObj);
                    }

                    if (!Q.IsReady() &&
                        Player.Position.Distance(NearestKeg(Player.Position.To2D()).KegObj.Position) <
                        Player.AttackRange &&
                        NearestKeg(Player.Position.To2D()).KegObj.IsTargetable &&
                        NearestKeg(Player.Position.To2D()).KegObj.Health < 2 &&
                        NearestKeg(Player.Position.To2D()).KegObj.IsValidTarget())
                    {
                        Player.IssueOrder(GameObjectOrder.AttackUnit, NearestKeg(Player.Position.To2D()).KegObj);
                    }
                }
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical,true);
            if (target == null)
            {
                return;
            }
            var ePrediction = Prediction.GetPrediction(target, 1f).CastPosition;
            var nbar = NearestKeg(Player.Position.To2D());
            
            if ((E.Instance.Ammo == 0 || E.Level < 1) && Q.IsReady() && Q.IsInRange(target) &&
                (LiveBarrels.Count == 0 || NearestKeg(Player.Position.To2D()).KegObj.Distance(Player) > Q.Range))
            {
                Q.CastOnUnit(target);
            }
            
            
            if(!_menu["bangplank.menu.misc.barrelmanager"]["bangplank.menu.misc.barrelmanager.edisabled"].GetValue<MenuBool>().Enabled  && E.IsReady() && (LiveBarrels.Count == 0 || NearestKeg(Player.Position.To2D()).KegObj.Distance(Player) > E.Range))
            {
                E.Cast(ePrediction);
            }

            if (R.Level <= 1 &&
                !_menu["bangplank.menu.misc.barrelmanager"]["bangplank.menu.misc.barrelmanager.edisabled"]
                    .GetValue<MenuBool>().Enabled && E.IsReady())
            {
                if ((LiveBarrels.Count == 0 || nbar.KegObj.Distance(Player) > Q.Range) && E.Instance.Ammo > 3)
                {
                    E.Cast(Player.Position);
                }
                if ((LiveBarrels.Count == 0 || nbar.KegObj.Distance(Player) > Q.Range) && E.Instance.Ammo < 3)
                {
                    foreach (var k in LiveBarrels)
                    {
                        if (k.KegObj.CountEnemyHeroesInRange(ExplosionRange) >= 1 && Player.Distance(k.KegObj) < E.Range)
                        {
                            BarrelManager();
                            return;

                        }

                    }
                    E.Cast(ePrediction);
                }
            }
            


            if (Player.CountEnemyHeroesInRange(E.Range) < 3 && !_menu["bangplank.menu.misc.barrelmanager"]["bangplank.menu.misc.barrelmanager.edisabled"]
                .GetValue<MenuBool>().Enabled)
            {
                if (Player.Position.Distance(nbar.KegObj.Position) < Q.Range && nbar.KegObj.Health < 3)
                {
                    if (target != null)
                    {
                        var prediction = Prediction.GetPrediction(target, 0.8f).CastPosition;
                        if (nbar.KegObj.Distance(prediction) < LinkRange)
                        {
                            R.Cast(prediction);
                            if (Player.Level < 13 && Player.Level >= 7 && nbar.KegObj.Health == 2)
                            {
                                DelayAction.Add(500-Game.Ping, () =>
                                    Q.Cast(nbar.KegObj));
                            }
                        }
                        if (Player.Level >= 13 && nbar.KegObj.Health == 2)
                        {
                            DelayAction.Add((int)(80 - Game.Ping), () =>
                                {
                                    Q.Cast(nbar.KegObj);
                                }
                            );
                        }
                        if (nbar.KegObj.Health == 1)
                        {
                            Q.Cast(nbar.KegObj);

                        }
                    }
                }
            }
            
            if (_menu["bangplank.menu.combo"].GetValue<MenuBool>("bangplank.menu.combo.r").Enabled && R.IsReady() &&
                target.CountEnemyHeroesInRange(600) + 1 > _menu["bangplank.menu.combo"]
                    .GetValue<MenuSlider>("bangplank.menu.combo.rmin").Value && target.Health < 30)
            {
                R.Cast(Prediction.GetPrediction(target, R.Delay).CastPosition);
            }
            BarrelManager();
        }

        private static void GameObjCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Barrel")
            {
                LiveBarrels.Add(new Keg(sender as AIMinionClient));
            }
        }

        private static void GameObjDelete(GameObject sender, EventArgs args)
        {
            for (int i = 0; i < LiveBarrels.Count; i++)
            {
                if (LiveBarrels[i].KegObj.NetworkId == sender.NetworkId)
                {
                    LiveBarrels.RemoveAt(i);
                    return;
                }
            }
        }

        private static void Draw(EventArgs args)
        {
            if (!_menu["bangplank.menu.drawing"].GetValue<MenuBool>("bangplank.menu.drawing.enabled").Enabled)
            {
                return;
            }

            if (!_menu["bangplank.menu.drawing"].GetValue<MenuBool>("bangplank.menu.drawing.q").Enabled && Q.Level > 0)
            {
                CircleRender.Draw(Player.Position, Q.Range,
                    Q.IsReady() ? SharpDX.Color.Cyan : SharpDX.Color.Black);
            }
            if (!_menu["bangplank.menu.drawing"].GetValue<MenuBool>("bangplank.menu.drawing.e").Enabled && E.Level > 0)
            {
                CircleRender.Draw(Player.Position, E.Range,
                    E.IsReady() ? SharpDX.Color.BlueViolet : SharpDX.Color.Black);
            }
            if (!_menu["bangplank.menu.drawing"].GetValue<MenuBool>("bangplank.menu.drawing.ehelper").Enabled)
            {
                CircleRender.Draw(Game.CursorPos, LinkRange / 2 + 10, SharpDX.Color.Red);
            }
        }
        private static Keg NearestKeg(Vector2 pos)
        {
            if (LiveBarrels.Count == 0)
            {
                return null;
            }
            return LiveBarrels.OrderBy(k => k.KegObj.Position.Distance(pos.To3D())).FirstOrDefault(k => !k.KegObj.IsDead);
        }
        private static void BarrelManager()
        {
            if (LiveBarrels.Count == 0) return;
            foreach (var k in LiveBarrels)
            {
                if (Q.IsReady() && Q.IsInRange(k.KegObj) && k.KegObj.CountEnemyHeroesInRange(ExplosionRange) > 0 && k.KegObj.Health < 2)
                    Q.Cast(k.KegObj);
                if (Player.Distance(k.KegObj) <= Player.AttackRange &&
                    k.KegObj.CountEnemyHeroesInRange(ExplosionRange) > 0 && k.KegObj.Health < 2 &&
                    k.KegObj.IsValidTarget() &&
                    k.KegObj.IsTargetable)
                    Player.IssueOrder(GameObjectOrder.AttackUnit, k.KegObj);
            }

        }
    }
    
    internal class Keg
    {
        public AIMinionClient KegObj;


        public Keg(AIMinionClient obj)
        {
            KegObj = obj;

        }


    }
}