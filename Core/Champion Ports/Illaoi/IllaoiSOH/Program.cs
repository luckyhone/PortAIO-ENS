using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using SharpDX;
using SPrediction;
using Color = System.Drawing.Color;

namespace IllaoiSOH
{
    class Program
    {
        const string champName = "Illaoi";

        static AIHeroClient player
        {
            get { return ObjectManager.Player; }
        }

        static Spell q, w, e, r;
        static Menu menu;

        public static void Game_OnGameLoad()
        {
            if (player.CharacterName != champName)
                return;

            q = new Spell(SpellSlot.Q, 800);
            w = new Spell(SpellSlot.W, 390);
            e = new Spell(SpellSlot.E, 900);
            r = new Spell(SpellSlot.R, 350);
            q.SetSkillshot(.484f, 0, 500, false, SpellType.Circle);
            e.SetSkillshot(.066f, 50, 1900, true, SpellType.Line);

            maimMenu();
            menu.Attach();
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Draw_OnDraw;
            Game.Print("<font color ='#2F9D27'>Illaoi - SOH</font> Korean Developer");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (player.IsDead)
                return;
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    DoCombo(menu.GetValue<MenuBool>("UseQ").Enabled, menu.GetValue<MenuBool>("UseQG").Enabled,
                        menu.GetValue<MenuBool>("UseW").Enabled, menu.GetValue<MenuBool>("UseE").Enabled,
                        menu.GetValue<MenuBool>("UseR").Enabled);
                    break;
                case OrbwalkerMode.Harass:
                    DoHarass(menu.GetValue<MenuBool>("harassUseQ").Enabled,
                        menu.GetValue<MenuBool>("harassUseW").Enabled);
                    break;
                case OrbwalkerMode.LaneClear:
                    DoLaneClear(menu.GetValue<MenuBool>("lcUseQ").Enabled);
                    break;
            }
        }

        static void DoCombo(bool useQ, bool useQG, bool useW, bool useE, bool useR)
        {
            AIHeroClient target = TargetSelector.GetTarget(e.Range, DamageType.Magical);
            var Ghost = target != null
                ? ObjectManager.Get<AIMinionClient>().FirstOrDefault(x => x.Name == target.Name)
                : null;
            if (target != null && target.IsValidTarget())
            {
                if (player.Distance(target.Position) < w.Range) //W>>EQ(R)>>W
                {
                    if (w.IsReady() && useW)
                        w.Cast(target, false);
                    if (e.IsReady() && useE)
                    {
                        PredictionOutput prediction;
                        prediction = e.GetPrediction(target, true);
                        if (prediction.Hitchance >= HitChance.High)
                            e.Cast(target, true);
                    }

                    if (q.IsReady() && useQ)
                        q.Cast(target, false);
                    if (r.IsReady() && useR && player.CountEnemyHeroesInRange(r.Range) >=
                        menu.GetValue<MenuSlider>("r.enemy").Value)
                        r.Cast(target, true);
                    if (w.IsReady() && useW && target != null)
                        w.Cast(target, false);
                }
                else if (e.IsReady() == false && player.Distance(target.Position) < e.Range) //E>>Q
                {
                    if (e.IsReady() && useE)
                    {
                        PredictionOutput prediction;
                        prediction = e.GetPrediction(target, true);
                        if (prediction.Hitchance >= HitChance.High)
                            e.Cast(target, true);
                    }

                    if (q.IsReady() && useQ)
                        q.Cast(target, true);
                }
            }
            else if (target == null && Ghost != null && Ghost.IsValidTarget())
            {
                if (player.Distance(Ghost.Position) < q.Range) //Ghost attack
                {
                    if (q.IsReady() && useQG)
                        q.Cast(Ghost.Position);
                }
            }
        }

        static void DoHarass(bool useQ, bool useW)
        {
            if (player.ManaPercent < menu.GetValue<MenuSlider>("harassMana").Value)
            {
                return;
            }

            var enemy = GameObjects.EnemyHeroes.FirstOrDefault(x => x.IsValidTarget(850));
            AIHeroClient target = TargetSelector.GetTarget(q.Range, DamageType.Magical);
            if (target != null && target.IsValidTarget())
            {
                if (player.Distance(target.Position) < q.Range)
                {
                    if (q.IsReady() && useQ)
                        q.CastIfHitchanceEquals(target, HitChance.High);
                }

                if (player.Distance(target.Position) < w.Range && enemy != null)
                {
                    if (w.IsReady() && useW)
                        w.Cast(target, true);
                }
            }
        }

        static void DoLaneClear(bool useq)
        {
            if (player.ManaPercent < menu.GetValue<MenuSlider>("laneclearMana").Value)
            {
                return;
            }

            var minionCount = MinionManager.GetMinions(player.Position, q.Range, MinionManager.MinionTypes.All,
                MinionManager.MinionTeam.NotAlly);
            if (q.IsReady())
            {
                var mfarm = q.GetLineFarmLocation(minionCount);
                if (minionCount.Count >= menu.GetValue<MenuSlider>("q.minion").Value)
                {
                    q.Cast(mfarm.Position);
                }
            }
        }

        static void Draw_OnDraw(EventArgs args)
        {
            if (menu.GetValue<MenuBool>("drawQ").Enabled)
            {
                if (q.IsReady())
                {

                    CircleRender.Draw(player.Position, q.Range + 50, SharpDX.Color.LightGreen);
                }
                else
                {
                    CircleRender.Draw(player.Position, q.Range + 50, SharpDX.Color.Red);
                }
            }

            if (menu.GetValue<MenuBool>("drawW").Enabled)
            {
                if (w.IsReady())
                {
                    CircleRender.Draw(player.Position, w.Range, SharpDX.Color.LightGreen);
                }
                else
                {
                    CircleRender.Draw(player.Position, w.Range, SharpDX.Color.Red);
                }
            }

            if (menu.GetValue<MenuBool>("drawE").Enabled)
            {
                if (e.IsReady())
                {
                    CircleRender.Draw(player.Position, e.Range, SharpDX.Color.LightGreen);
                }
                else
                {
                    CircleRender.Draw(player.Position, e.Range, SharpDX.Color.Red);
                }
            }

            if (menu.GetValue<MenuBool>("drawR").Enabled)
            {
                if (r.IsReady())
                {
                    CircleRender.Draw(player.Position, player.BoundingRadius + r.Range + 100, SharpDX.Color.LightGreen);
                }
                else
                {
                    CircleRender.Draw(player.Position, player.BoundingRadius + r.Range + 100, SharpDX.Color.Red);
                }
            }

            if (menu.GetValue<MenuBool>("Passive").Enabled)
            {
                var enemy = GameObjects.EnemyHeroes.FirstOrDefault(x => x.IsValidTarget(2000));
                foreach (var passive in ObjectManager.Get<AIMinionClient>().Where(x => x.Name == "God"))
                {
                    CircleRender.Draw(new Vector3(passive.Position.X, passive.Position.Y, passive.Position.Z),
                        50, SharpDX.Color.YellowGreen, 50);
                    if (enemy != null)
                    {
                        var xx = Drawing.WorldToScreen(passive.Position.Extend(enemy.Position, 850));
                        var xy = Drawing.WorldToScreen(passive.Position);
                        Drawing.DrawLine(xy.X, xy.Y, xx.X, xx.Y, 5, Color.YellowGreen);
                    }

                }
            }

            if (menu.GetValue<MenuBool>("drawDmg").Enabled)
                DrawHPBarDamage();
        }


        static void maimMenu()
        {
            menu = new Menu("Illaoi - SOH", "Illaoi - SOH", true);

            Menu comboMenu = menu.Add(new Menu("Combo", "combo"));
            comboMenu.Add(new MenuBool("UseQ", "Use Q").SetValue(true));
            comboMenu.Add(new MenuBool("UseQG", "Use Q Ghost").SetValue(true));
            comboMenu.Add(new MenuBool("UseW", "Use W").SetValue(true));
            comboMenu.Add(new MenuBool("UseE", "Use E").SetValue(true));
            comboMenu.Add(new MenuBool("UseR", "Use R").SetValue(true));
            comboMenu.Add(new MenuSlider("r.enemy", "R Enemy", 1, 1, 5));

            Menu harassMenu = menu.Add(new Menu("Harass", "harass"));
            harassMenu.Add(new MenuBool("harassUseQ", "Use Q").SetValue(true));
            harassMenu.Add(new MenuBool("harassUseW", "Use W").SetValue(true));
            harassMenu.Add(new MenuSlider("harassMana", "Mana Manager (%)", 70, 1, 100));

            Menu lcMenu = menu.Add(new Menu("Lane Clear", "laneClear"));
            lcMenu.Add(new MenuBool("lcUseQ", "Use Q").SetValue(true));
            lcMenu.Add(new MenuSlider("laneclearMana", "Mana Manager (%)", 30, 1, 100));
            lcMenu.Add(new MenuSlider("q.minion", "Minion", 3, 1, 10));

            Menu drawMenu = menu.Add(new Menu("Drawing", "drawing"));
            drawMenu.Add(new MenuBool("drawQ", "Draw Q Range").SetValue(true));
            drawMenu.Add(new MenuBool("drawW", "Draw W Range").SetValue(true));
            drawMenu.Add(new MenuBool("drawE", "Draw E Range").SetValue(true));
            drawMenu.Add(new MenuBool("drawR", "Draw R Range").SetValue(true));
            drawMenu.Add(new MenuBool("Passive", "Draw Passive").SetValue(true));
            drawMenu.Add(new MenuBool("drawDmg", "Draw Combo Damage").SetValue(true));

        }

        static double getComboDamage(AIBaseClient target)
        {
            double damage = player.GetAutoAttackDamage(target);
            if (q.IsReady() && menu.GetValue<MenuBool>("UseQ").Enabled)
                damage += player.GetSpellDamage(target, SpellSlot.Q);
            if (w.IsReady() && menu.GetValue<MenuBool>("UseW").Enabled)
                damage += player.GetSpellDamage(target, SpellSlot.W);
            if (e.IsReady() && menu.GetValue<MenuBool>("UseE").Enabled)
                damage += player.GetSpellDamage(target, SpellSlot.E);
            if (r.IsReady() && menu.GetValue<MenuBool>("UseR").Enabled)
                damage += player.GetSpellDamage(target, SpellSlot.R);
            return damage;
        }

        static void DrawHPBarDamage()
        {
            const int XOffset = 10;
            const int YOffset = 20;
            const int Width = 103;
            const int Height = 8;
            foreach (var unit in ObjectManager.Get<AIHeroClient>()
                         .Where(h => h.IsValid && h.IsHPBarRendered && h.IsEnemy))
            {
                var barPos = unit.HPBarPosition;
                var damage = getComboDamage(unit);
                var percentHealthAfterDamage = Math.Max(0, unit.Health - damage) / unit.MaxHealth;
                var yPos = barPos.Y + YOffset;
                var xPosDamage = barPos.X + XOffset + Width * percentHealthAfterDamage;
                var xPosCurrentHp = barPos.X + XOffset + Width * unit.Health / unit.MaxHealth;

                if (damage > unit.Health)
                {
                    /*Text.X = (int)barPos.X + XOffset;
                    Text.Y = (int)barPos.Y + YOffset - 13;
                    Text.text = ((int)(unit.Health - damage)).ToString();
                    Text.OnEndScene();*/
                }

                Drawing.DrawLine((float) xPosDamage, yPos, (float) xPosDamage, yPos + Height, 2, Color.Red);
            }
        }
    }
}