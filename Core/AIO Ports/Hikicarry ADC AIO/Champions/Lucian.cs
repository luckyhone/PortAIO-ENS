using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using HikiCarry.Core.Plugins;
using SharpDX;
using SPrediction;
using Color = SharpDX.Color;
using Prediction = EnsoulSharp.SDK.Prediction;
using Utilities = HikiCarry.Core.Utilities.Utilities;

 namespace HikiCarry.Champions
{
    class Lucian
    {
        internal static Spell Q;
        internal static Spell Q2;
        internal static Spell W;
        internal static Spell E;
        internal static Spell R;
        public static int ERange => Utilities.Slider("lucian.e.range");

        public Lucian()
        {
            Q = new Spell(SpellSlot.Q, 675);
            Q2 = new Spell(SpellSlot.Q, 900);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 475);
            R = new Spell(SpellSlot.R, 1400);

            Q.SetTargetted(0.25f, float.MaxValue);
            Q2.SetSkillshot(0.55f, 50f, float.MaxValue, false, SpellType.Line);
            W.SetSkillshot(0.4f, 150f, 1600, true, SpellType.Circle);
            E.SetSkillshot(0.25f, 1f, float.MaxValue, false, SpellType.Line);
            R.SetSkillshot(0.2f, 110f, 2500, true, SpellType.Line);

            var comboMenu = new Menu(":: Combo Settings", ":: Combo Settings");
            {
                comboMenu.Add(new MenuBool("lucian.q.combo", "Use Q", true).SetValue(true)).SetTooltip("Uses Q in Combo");
                comboMenu.Add(new MenuBool("lucian.e.combo", "Use E", true).SetValue(true)).SetTooltip("Uses E in Combo");
                comboMenu.Add(new MenuList("lucian.e.mode", "E Type", new[] { "Safe", "Cursor Position" }));
                comboMenu.Add(new MenuBool("lucian.w.combo", "Use W", true).SetValue(true)).SetTooltip("Uses W in Combo");
                comboMenu.Add(new MenuBool("lucian.disable.w.prediction", "Disable W Prediction").SetTooltip("10/10 for speed combo!"));
                comboMenu.Add(new MenuBool("lucian.r.combo", "Use R", true).SetValue(true)).SetTooltip("Uses R in Combo (Only Casting If Enemy Killable)");
                comboMenu.Add(new MenuBool("lucian.combo.start.e", "Start Combo With E", true).SetValue(true)).SetTooltip("Starting Combo With E");
                comboMenu.Add(new MenuSlider("lucian.e.range", "(E) Range",475,1,475)).SetTooltip("If you wanna do short dash just set that slider to 1");

                Initializer.Config.Add(comboMenu);
            }

            var harassMenu = new Menu(":: Harass Settings", ":: Harass Settings");
            {
                harassMenu.Add(new MenuBool("lucian.q.harass", "Use Q", true).SetValue(true)).SetTooltip("Uses Q in Harass");
                harassMenu.Add(new MenuList("lucian.q.type", "Harass Type", new[] { "Extended", "Normal" }));
                harassMenu.Add(new MenuBool("lucian.w.harass", "Use W", true).SetValue(true)).SetTooltip("Uses W in Harass");
                harassMenu.Add(new MenuSlider("lucian.harass.mana", "Min. Mana", 50, 1, 99)).SetTooltip("Manage your Mana!");
                var qToggleMenu = new Menu(":: Q Whitelist (Extended)", ":: Q Whitelist (Extended)");
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValid))
                    {
                        qToggleMenu.Add(new MenuBool("lucian.white" + enemy.CharacterName, "(Q) " + enemy.CharacterName, true).SetValue(true));
                    }
                    harassMenu.Add(qToggleMenu);
                }

                Initializer.Config.Add(harassMenu);
            }

            var clearMenu = new Menu(":: Clear Settings", ":: Clear Settings");
            {
                clearMenu.Add(new MenuBool("lucian.q.clear", "Use Q", true).SetValue(true)).SetTooltip("Uses Q in Clear");
                clearMenu.Add(new MenuBool("lucian.q.harass.in.laneclear", "Use Extended (Q) Harass Enemy", true).SetValue(true)).SetTooltip("Uses Q in Clear");
                clearMenu.Add(new MenuBool("lucian.w.clear", "Use W", true).SetValue(true)).SetTooltip("Uses W in Clear");
                clearMenu.Add(new MenuSlider("lucian.q.minion.hit.count", "(Q) Min. Minion Hit", 3, 1, 5)).SetTooltip("Minimum minion count for Q");
                clearMenu.Add(new MenuSlider("lucian.w.minion.hit.count", "(W) Min. Minion Hit", 3, 1, 5)).SetTooltip("Minimum minion count for W");
                clearMenu.Add(new MenuSlider("lucian.clear.mana", "Min. Mana", 50, 1, 99)).SetTooltip("Manage your Mana!");
                Initializer.Config.Add(clearMenu);
            }

            var jungleMenu = new Menu(":: Jungle Settings", ":: Jungle Settings");
            {
                jungleMenu.Add(new MenuBool("lucian.q.jungle", "Use Q", true).SetValue(true)).SetTooltip("Uses Q in Jungle");
                jungleMenu.Add(new MenuBool("lucian.w.jungle", "Use W", true).SetValue(true)).SetTooltip("Uses W in Jungle");
                jungleMenu.Add(new MenuBool("lucian.e.jungle", "Use E", true).SetValue(true)).SetTooltip("Uses E in Jungle (Using Mouse Position)");
                jungleMenu.Add(new MenuSlider("lucian.jungle.mana", "Min. Mana", 50, 1, 99)).SetTooltip("Manage your Mana!");
                Initializer.Config.Add(jungleMenu);
            }

            var killStealMenu = new Menu(":: KillSteal Settings", ":: KillSteal Settings");
            {
                killStealMenu.Add(new MenuBool("lucian.q.ks", "Use Q", true).SetValue(true)).SetTooltip("Uses Q if Enemy Killable");
                killStealMenu.Add(new MenuBool("lucian.w.ks", "Use W", true).SetValue(true)).SetTooltip("Uses W if Enemy Killable");
                Initializer.Config.Add(killStealMenu);
            }

            var eqMenu = new Menu(":: E+Q KS Settings", ":: E+Q KS Settings");
            {
                eqMenu.Add(new MenuBool("use.eq", "Use E+Q", true).SetValue(true));
                eqMenu.Add(new MenuBool("eq.safety.check", "Safety Check?", true).SetValue(true));
                eqMenu.Add(new MenuSlider("eq.safety.range", "Safety Range", 1150, 1, 1150));
                eqMenu.Add(new MenuSlider("eq.min.enemy.count.range", "Min Enemy Count", 1, 1, 5));
                Initializer.Config.Add(eqMenu);
            }
            eqMenu.SetFontColor(SharpDX.Color.Crimson);

            var miscMenu = new Menu(":: Miscellaneous", ":: Miscellaneous");
            {
                miscMenu.Add(new MenuBool("dodge.jarvan.ult", "Dodge JarvanIV Ult ?", true)).SetValue(true);
                Initializer.Config.Add(miscMenu);
            }
            var drawMenu = new Menu("Draw Settings", "Draw Settings");
            {
                var drawDamageMenu = new MenuBool("RushDrawEDamage", "Combo Damage").SetValue(true);
                var drawFill = new MenuColor("RushDrawEDamageFill", "Combo Damage Fill",Color.Gold);

                var damageDraws = drawMenu.Add(new Menu("Damage Draws", "Damage Draws"));
                damageDraws.Add(drawDamageMenu);
                damageDraws.Add(drawFill);
                
                Initializer.Config.Add(drawMenu);
            }
            Game.OnUpdate += LucianOnUpdate;
            AIBaseClient.OnProcessSpellCast += LucianOnSpellCast;
            Drawing.OnDraw += LucianOnDraw;
            AIBaseClient.OnDoCast += OnProcess;
        }

        private float TotalDamage(AIHeroClient hero)
        {
            var damage = 0d;
            if (Q.IsReady())
            {
                damage += Q.GetDamage(hero);
            }
            if (W.IsReady())
            {
                damage += W.GetDamage(hero);
            }
            if (E.IsReady())
            {
                damage += W.GetDamage(hero);
            }
            if (R.IsReady())
            {
                damage += R.GetDamage(hero);
            }
            return (float)damage;
        }

        private void LucianOnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    Clear();
                    break;
            }

            

            if (!UltActive && Utilities.Enabled("use.eq"))
            {
                if (E.IsReady() &&
                    ObjectManager.Player.CountEnemyHeroesInRange(Utilities.Slider("eq.safety.range")) <= Utilities.Slider("eq.min.enemy.count.range"))
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range + E.Range - 100)))
                    {
                        var aadamage = ObjectManager.Player.GetAutoAttackDamage(enemy);
                        var dmg = ObjectManager.Player.CalculateDamage(enemy, DamageType.Physical,
                            Q.GetDamage(enemy));
                        var combodamage = aadamage + dmg;

                        if (enemy.Health < combodamage)
                        {
                            E.Cast(ObjectManager.Player.Position.Extend(enemy.Position, ERange));
                        }
                    }

                    if (Q.IsReady() && ObjectManager.Player.CountEnemyHeroesInRange(Utilities.Slider("eq.safety.range")) <= Utilities.Slider("eq.min.enemy.count.range"))
                    {
                        foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range)))
                        {
                            var dmg = ObjectManager.Player.CalculateDamage(enemy, DamageType.Physical,
                                Q.GetDamage(enemy));
                            var aadamage = ObjectManager.Player.GetAutoAttackDamage(enemy);

                            var combodamage = aadamage + dmg;

                            if (enemy.Health < combodamage)
                            {
                                Q.CastOnUnit(enemy);
                            }
                        }
                    }
                }
            }
            if (!UltActive && Utilities.Enabled("lucian.q.ks") && Q.IsReady())
            {
                ExtendedQKillSteal();
            }

            if (!UltActive && Utilities.Enabled("lucian.w.ks") && W.IsReady())
            {
                KillstealW();
            }

        }
        private static void SemiManual()
        {
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(R.Range) &&
                R.GetPrediction(x).CollisionObjects.Count < 2))
            {
                R.Cast(enemy);
            }

        }
        private static void Harass()
        {
            
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("lucian.harass.mana"))
            {
                return;
            }
            if (Q.IsReady() || Q2.IsReady() && Utilities.Enabled("lucian.q.harass") && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
            {
                HarassQCast();
            }
            if (W.IsReady() && Utilities.Enabled("lucian.w.harass") && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(W.Range) && W.GetPrediction(x).Hitchance >= HitChance.Medium))
                {
                    W.Cast(enemy);
                }
            }
        }
        private static void HarassQCast()
        {
            switch (Initializer.Config["lucian.q.type"].GetValue<MenuList>().Index)
            {
                case 0:
                    var minions = ObjectManager.Get<AIMinionClient>().Where(o => o.IsValidTarget(Q.Range));
                    var target = ObjectManager.Get<AIHeroClient>().Where(x => x.IsValidTarget(Q2.Range)).FirstOrDefault(x => Initializer.Config[":: Q Whitelist (Extended)"]["lucian.white" + x.CharacterName].GetValue<MenuBool>().Enabled);
                    if (target != null)
                    {
                        if (target.Distance(ObjectManager.Player.Position) > Q.Range &&
                            target.CountEnemyHeroesInRange(Q2.Range) > 0)
                        {
                            foreach (var minion in minions)
                            {
                                if (Q2.WillHit(target,
                                        ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, Q2.Range), 0,
                                        HitChance.VeryHigh))
                                {
                                    Q2.CastOnUnit(minion);
                                }
                            }
                        }
                    }

                    break;
                case 1:
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range)))
                    {
                        Q.CastOnUnit(enemy);
                    }
                    break;
            }
        }
        private static void ExtendedQKillSteal()
        {
            var minions = ObjectManager.Get<AIMinionClient>().Where(o => o.IsValidTarget(Q.Range));
            var target = GameObjects.EnemyHeroes.FirstOrDefault(x => x.IsValidTarget(Q2.Range));
            
            if (target != null && (target.Distance(ObjectManager.Player.Position) > Q.Range &&
                                   target.Distance(ObjectManager.Player.Position) < Q2.Range && 
                                   target.CountEnemyHeroesInRange(Q2.Range) >= 1 && target.Health < Q.GetDamage(target) && !target.IsDead))
            {
                foreach (var minion in minions)
                {
                    if (Q2.WillHit(target, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, Q2.Range),0,HitChance.VeryHigh))
                    {
                        Q2.CastOnUnit(minion);
                    }
                }
            }
        }
        private static void KillstealW()
        {
            var target = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(W.Range)).
                FirstOrDefault(x=> x.Health < W.GetDamage(x));

            var pred = W.GetPrediction(target);

            if (target != null && pred.Hitchance >= HitChance.High)
            {
                W.Cast(pred.CastPosition);
            }
        }
        private static void Clear()
        {
            try
            {
                if (ObjectManager.Player.ManaPercent < Initializer.Config[":: Clear Settings"]["lucian.clear.mana"]
                        .GetValue<MenuSlider>().Value)
                {
                    return;
                }

                if (Q.IsReady() &&
                    Initializer.Config[":: Clear Settings"]["lucian.q.clear"].GetValue<MenuBool>().Enabled &&
                    ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                {
                    foreach (var minion in MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                                 MinionManager.MinionTypes.All,
                                 MinionManager.MinionTeam.NotAlly))
                    {
                        var prediction = Prediction.GetPrediction(minion, Q.Delay,
                            ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRadius);

                        var collision = Q.GetCollision(ObjectManager.Player.Position.ToVector2(),
                            new List<Vector2> { prediction.UnitPosition.ToVector2() });

                        foreach (var cs in collision)
                        {
                            if (collision.Count >= Initializer.Config[":: Clear Settings"]["lucian.q.minion.hit.count"]
                                    .GetValue<MenuSlider>().Value)
                            {
                                if (collision.Last().Distance(ObjectManager.Player) -
                                    collision[0].Distance(ObjectManager.Player) <= 600
                                    && collision[0].Distance(ObjectManager.Player) <= 500)
                                {
                                    Q.Cast(cs);
                                }
                            }
                        }

                    }
                }

                if (W.IsReady() &&
                    Initializer.Config[":: Clear Settings"]["lucian.w.clear"].GetValue<MenuBool>().Enabled &&
                    ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                {
                    if (W.GetCircularFarmLocation(MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range,
                            MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly)).MinionsHit >=
                        Utilities.Slider("lucian.w.minion.hit.count"))
                    {
                        W.Cast(W.GetCircularFarmLocation(MinionManager.GetMinions(ObjectManager.Player.Position,
                            Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly)).Position);
                    }
                }

                if (Q.IsReady() && Initializer.Config[":: Clear Settings"]["lucian.q.harass.in.laneclear"]
                        .GetValue<MenuBool>().Enabled &&
                    ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                {
                    var minions = ObjectManager.Get<AIMinionClient>().Where(o => o.IsValidTarget(Q.Range));
                    var target = ObjectManager.Get<AIHeroClient>().Where(x => x.IsValidTarget(Q2.Range))
                        .FirstOrDefault(x =>
                            Initializer.Config[":: Q Whitelist (Extended)"]["lucian.white" + x.CharacterName]
                                .GetValue<MenuBool>().Enabled);
                    if (target.Distance(ObjectManager.Player.Position) > Q.Range &&
                        target.CountEnemyHeroesInRange(Q2.Range) > 0)
                    {
                        foreach (var minion in minions)
                        {
                            if (Q2.WillHit(target,
                                    ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, Q2.Range), 0,
                                    HitChance.VeryHigh))
                            {
                                Q2.CastOnUnit(minion);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // 
            }
        }
        private void LucianOnSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && ObjectManager.Player.Spellbook.IsAutoAttack && args.Target is AIHeroClient && args.Target.IsValid)
            {
                if (Initializer.Config[":: Combo Settings"]["lucian.combo.start.e"].GetValue<MenuBool>().Enabled)
                {
                    if (!E.IsReady() && Q.IsReady() && Initializer.Config[":: Combo Settings"]["lucian.q.combo"].GetValue<MenuBool>().Enabled &&
                    ObjectManager.Player.Distance(args.Target.Position) < Q.Range &&
                    Orbwalker.ActiveMode == OrbwalkerMode.Combo && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                    {
                        Q.CastOnUnit(((AIHeroClient)args.Target));
                    }
                    
                    if (!E.IsReady() && W.IsReady() && Initializer.Config[":: Combo Settings"]["lucian.w.combo"].GetValue<MenuBool>().Enabled &&
                        ObjectManager.Player.Distance(args.Target.Position) < W.Range &&
                        Orbwalker.ActiveMode == OrbwalkerMode.Combo && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                    {
                        if (Initializer.Config[":: Combo Settings"]["lucian.disable.w.prediction"].GetValue<MenuBool>().Enabled)
                        {
                            W.Cast(((AIHeroClient)args.Target).Position);
                        }
                        else
                        {
                            if (W.GetPrediction(((AIHeroClient)args.Target)).Hitchance >= HitChance.Medium)
                            {
                                W.Cast(((AIHeroClient)args.Target).Position);
                            }
                        }
                       
                    }
                    if (E.IsReady() && Initializer.Config[":: Combo Settings"]["lucian.e.combo"].GetValue<MenuBool>().Enabled &&
                        ObjectManager.Player.Distance(args.Target.Position) < Q2.Range &&
                        Orbwalker.ActiveMode == OrbwalkerMode.Combo && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                    {
                        switch (Initializer.Config["lucian.e.mode"].GetValue<MenuList>().Index)
                        {
                            case 0:
                                Utilities.ECast(((AIHeroClient)args.Target), E);
                                break;
                            case 1:
                                E.Cast(Game.CursorPos);
                                break;
                        }
                        
                    }
                }
                else
                {
                    if (Q.IsReady() && Initializer.Config[":: Combo Settings"]["lucian.q.combo"].GetValue<MenuBool>().Enabled &&
                    ObjectManager.Player.Distance(args.Target.Position) < Q.Range &&
                    Orbwalker.ActiveMode == OrbwalkerMode.Combo && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                    {
                        Q.CastOnUnit(((AIHeroClient)args.Target));
                    }
                    if (W.IsReady() && Initializer.Config[":: Combo Settings"]["lucian.w.combo"].GetValue<MenuBool>().Enabled &&
                        ObjectManager.Player.Distance(args.Target.Position) < W.Range &&
                        Orbwalker.ActiveMode == OrbwalkerMode.Combo && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff")
                        && W.GetPrediction(((AIHeroClient)args.Target)).Hitchance >= HitChance.Medium)
                    {
                        W.Cast(((AIHeroClient)args.Target).Position);
                    }
                    if (E.IsReady() && Initializer.Config[":: Combo Settings"]["lucian.e.combo"].GetValue<MenuBool>().Enabled &&
                        ObjectManager.Player.Distance(args.Target.Position) < Q2.Range &&
                        Orbwalker.ActiveMode == OrbwalkerMode.Combo && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                    {
                        switch (Initializer.Config["lucian.e.mode"].GetValue<MenuList>().Index)
                        {
                            case 0:
                                Utilities.ECast(((AIHeroClient)args.Target),E);
                                break;
                            case 1:
                                E.Cast(Game.CursorPos);
                                break;
                        }
                    }
                }
                
            }
            else if (sender.IsMe && ObjectManager.Player.Spellbook.IsAutoAttack && args.Target is AIMinionClient && args.Target.IsValid && ((AIMinionClient)args.Target).Team == GameObjectTeam.Neutral
                && ObjectManager.Player.ManaPercent > Utilities.Slider("lucian.clear.mana"))
            {
                if (Q.IsReady() && Initializer.Config[":: Combo Settings"]["lucian.q.jungle"].GetValue<MenuBool>().Enabled &&
                    ObjectManager.Player.Distance(args.Target.Position) < Q.Range &&
                    Orbwalker.ActiveMode == OrbwalkerMode.LaneClear && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                {
                    Q.CastOnUnit(((AIMinionClient)args.Target));
                }
                if (W.IsReady() && Initializer.Config[":: Combo Settings"]["lucian.w.jungle"].GetValue<MenuBool>().Enabled &&
                    ObjectManager.Player.Distance(args.Target.Position) < W.Range &&
                    Orbwalker.ActiveMode == OrbwalkerMode.LaneClear && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                {
                    W.Cast(((AIMinionClient)args.Target).Position);
                }
                if (E.IsReady() && Initializer.Config[":: Combo Settings"]["lucian.e.jungle"].GetValue<MenuBool>().Enabled &&
                   ((AIMinionClient)args.Target).IsValidTarget(1000) &&
                    Orbwalker.ActiveMode == OrbwalkerMode.LaneClear && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                {
                    E.Cast(Game.CursorPos);
                }

            }
        }

        private void LucianOnDraw(EventArgs args)
        {
            //throw new NotImplementedException();
        }
        public static bool UltActive => ObjectManager.Player.HasBuff("LucianR");
        private void OnProcess(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy && sender is AIHeroClient &&
                args.End.Distance(ObjectManager.Player.Position) < 100 &&
                args.SData.Name == "JarvanIVCataclysm" && args.Slot == SpellSlot.R
                && Utilities.Enabled("dodge.jarvan.ult") &&
                (!ObjectManager.Player.Position.Extend(args.End, -E.Range).IsWall() ||
                !ObjectManager.Player.Position.Extend(args.End, -E.Range).IsUnderEnemyTurret()))
            {
                var extpos = ObjectManager.Player.Position.Extend(args.End, -E.Range);
                E.Cast(extpos);
            }
        }
    }
}
