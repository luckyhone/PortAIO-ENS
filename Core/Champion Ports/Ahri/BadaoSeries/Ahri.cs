using System;
using System.Collections.Generic;
using System.Linq;
using BadaoSeries.CustomOrbwalker;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using EnsoulSharp.SDK.Utility;
using PortAIO;
using SharpDX;
using SPrediction;
using Geometry = PortAIO.Geometry;
using Prediction = EnsoulSharp.SDK.Prediction;

namespace BadaoSeries.Plugin
{
    internal class Ahri : AddUI
    {
        public static string ahri1 = "Ahri_Base_Orb_mis.troy";
        public static string ahri2 = "Ahri_Base_Orb_mis_02.troy";

        public static GameObject AhriOrbReturn
        {
            get { return ObjectManager.Get<GameObject>().FirstOrDefault(x => x.Name == "Ahri_Base_Orb_mis_02.troy"); }
        }

        public static GameObject AhriOrb
        {
            get { return ObjectManager.Get<GameObject>().FirstOrDefault(x => x.Name == "Ahri_Base_Orb_mis.troy"); }
        }

        public static List<Vector2> pos = new List<Vector2>();
        public static int Rcount;

        private static bool IsCombo
        {
            get { return Orbwalker.ActiveMode == OrbwalkerMode.Combo; }
        }

        private static bool IsHarass
        {
            get { return Orbwalker.ActiveMode == OrbwalkerMode.Harass; }
        }

        private static bool IsClear
        {
            get { return Orbwalker.ActiveMode == OrbwalkerMode.LaneClear; }
        }

        private static float Qdamage(AIBaseClient target)
        {
            return (float) Player.CalculateDamage(target, DamageType.Magical,
                       new double[] {40, 65, 90, 115, 140}[Q.Level - 1] + 0.4 * Player.FlatMagicDamageMod) +
                   (float) Player.CalculateDamage(target, DamageType.True,
                       new double[] {40, 65, 90, 115, 140}[Q.Level - 1] + 0.4 * Player.FlatMagicDamageMod);
        }

        private static float Wdamage(AIBaseClient target)
        {
            return (float) Player.CalculateDamage(target, DamageType.Magical,
                new double[] {60, 85, 110, 135, 160}[W.Level - 1]
                + 0.3 * Player.FlatMagicDamageMod) * 1.6f;
        }

        private static float Edamage(AIBaseClient target)
        {
            return (float) Player.CalculateDamage(target, DamageType.Magical,
                new double[] {80, 110, 140, 170, 200}[E.Level - 1]
                + 0.60 * Player.FlatMagicDamageMod);
        }

        private static float Rdamage(AIBaseClient target)
        {
            return (float) Player.CalculateDamage(target, DamageType.Magical,
                (new double[] {60, 90, 120}[R.Level - 1]
                 + 0.35 * Player.FlatMagicDamageMod) * 3);
        }

        private static float AhriDamage(AIHeroClient target)
        {
            float x = 0;
            if (Player.Mana > Q.Instance.ManaCost)
            {
                if (Q.IsReady()) x += Qdamage(target);
                if (Player.Mana > Q.Instance.ManaCost + R.Instance.ManaCost)
                {
                    if (R.IsReady()) x += Rdamage(target);
                    if (Player.Mana > Q.Instance.ManaCost + R.Instance.ManaCost + E.Instance.ManaCost)
                    {
                        if (E.IsReady()) x += Edamage(target);
                        if (Player.Mana > Q.Instance.ManaCost + R.Instance.ManaCost + E.Instance.ManaCost +
                            W.Instance.ManaCost)
                            if (W.IsReady())
                                x += Wdamage(target);
                    }
                }

            }

            if (LudensEcho.IsReady)
            {
                x = x + (float) Player.CalculateDamage(target, DamageType.Magical,
                    100 + 0.1 * Player.FlatMagicDamageMod);
            }

            if (Ignite.IsReady())
            {
                x = x + (float) Player.GetSpellDamage(target, Ignite);
            }

            x = x + (float) Player.GetAutoAttackDamage(target, true);
            return x;
        }

        public Ahri()
        {
            Q = new Spell(SpellSlot.Q, 880);
            W = new Spell(SpellSlot.W, 550);
            E = new Spell(SpellSlot.E, 975);
            E2 = new Spell(SpellSlot.E, 975);
            R = new Spell(SpellSlot.R, 450); //600
            Q.SetSkillshot(0.25f, 100, 1600, false, SpellType.Line);
            E.SetSkillshot(0.25f, 60, 1550, true, SpellType.Line);
            E2.SetSkillshot(0.25f, 60, 1550, true, SpellType.Line);
            Q.DamageType = W.DamageType = E.DamageType = DamageType.Magical;
            Q.MinHitChance = HitChance.High;
            E.MinHitChance = HitChance.High;

            Menu Asassin = new Menu("Assassin", "AssasinMode");
            {
                KeyBind(Asassin, "activeAssasin", "Assassin Key", Keys.T, KeyBindType.Press);
                Separator(Asassin, "1", "Make sure you select a target");
                Separator(Asassin, "2", "before press this key");
                MainMenu.Add(Asassin);
            }
            Menu Combo = new Menu("Combo", "Combo");
            {
                Bool(Combo, "Qc", "Q", true);
                Bool(Combo, "Wc", "W", true);
                Bool(Combo, "Ec", "E", true);
                MainMenu.Add(Combo);
            }
            Menu Harass = new Menu("Harass", "Harass");
            {
                Bool(Harass, "Qh", "Q", true);
                Bool(Harass, "Wh", "W", true);
                Bool(Harass, "Eh", "E", true);
                Slider(Harass, "manah", "Min mana", 40, 0, 100);
                MainMenu.Add(Harass);
            }
            Menu Clear = new Menu("Clear", "Clear");
            {
                Bool(Clear, "Qj", "Q", true);
                Slider(Clear, "Qhitj", "Q if will hit", 2, 1, 3);
                Slider(Clear, "manaj", "Min mana", 40, 0, 100);
                MainMenu.Add(Clear);
            }
            Menu Auto = new Menu("Auto", "Auto");
            {
                KeyBind(Auto, "harassa", "Harass Q", Keys.H, KeyBindType.Toggle);
                Bool(Auto, "interrupta", "E interrupt + gapcloser", true);
                Bool(Auto, "killsteala", "KillSteal", true);
                MainMenu.Add(Auto);
            }
            Menu drawMenu = new Menu("Draw", "Draw");
            {
                Bool(drawMenu, "Qd", "Q");
                Bool(drawMenu, "Wd", "W");
                Bool(drawMenu, "Ed", "E");
                Bool(drawMenu, "Rd", "R");
                //Bool(drawMenu, "Hpd", "Damage Indicator")
                MainMenu.Add(drawMenu);
            }

            Game.OnUpdate += Game_OnUpdate;
            AIBaseClient.OnNewPath += Obj_AI_Base_OnNewPath;
            Drawing.OnDraw += Drawing_OnDraw;
            AIBaseClient.OnDoCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterrupterSpell += InterruptableSpell_OnInterruptableTarget;
            Orbwalker.OnAfterAttack += Orbwalking_AfterAttack;
        }


        private static bool comboq
        {
            get { return MainMenu.GetValue<MenuBool>("Qc").Enabled; }
        }

        private static bool combow
        {
            get { return MainMenu.GetValue<MenuBool>("Wc").Enabled; }
        }

        private static bool comboe
        {
            get { return MainMenu.GetValue<MenuBool>("Ec").Enabled; }
        }

        private static bool harassq
        {
            get { return MainMenu.GetValue<MenuBool>("Qh").Enabled; }
        }

        private static bool harassw
        {
            get { return MainMenu.GetValue<MenuBool>("Wh").Enabled; }
        }

        private static bool harasse
        {
            get { return MainMenu.GetValue<MenuBool>("Eh").Enabled; }
        }

        private static int manaharass
        {
            get { return MainMenu.GetValue<MenuSlider>("manah").Value; }
        }

        private static bool clearq
        {
            get { return MainMenu.GetValue<MenuBool>("Qj").Enabled; }
        }

        private static int clearqhit
        {
            get { return MainMenu.GetValue<MenuSlider>("Qhitj").Value; }
        }

        private static int manaclear
        {
            get { return MainMenu.GetValue<MenuSlider>("manaj").Value; }
        }

        private static bool autoharassq
        {
            get { return MainMenu.GetValue<MenuKeyBind>("harassa").Active; }
        }

        private static bool autointerrupt
        {
            get { return MainMenu.GetValue<MenuBool>("interrupta").Enabled; }
        }

        private static bool autokillsteal
        {
            get { return MainMenu.GetValue<MenuBool>("killsteala").Enabled; }
        }

        private static bool drawq
        {
            get { return MainMenu.GetValue<MenuBool>("Qd").Enabled; }
        }

        private static bool draww
        {
            get { return MainMenu.GetValue<MenuBool>("Wd").Enabled; }
        }

        private static bool drawe
        {
            get { return MainMenu.GetValue<MenuBool>("Ed").Enabled; }
        }

        private static bool drawr
        {
            get { return MainMenu.GetValue<MenuBool>("Rd").Enabled; }
        }

        private static bool activeAssasin
        {
            get { return MainMenu.GetValue<MenuKeyBind>("activeAssasin").Active; }
        }
        
        private void InterruptableSpell_OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (!Enable)
                return;
            if (sender.IsEnemy && sender.IsValidTarget(E.Range) && E.IsReady() && autointerrupt)
            {
                E.BadaoCast(sender);
            }
        }
        
        private void Gapcloser_OnGapCloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs gapcloser)
        {
            if (!Enable)
                return;
            if (sender.IsEnemy && sender.IsValidTarget(E.Range) && E.IsReady() && autointerrupt)
            {
                E.BadaoCast(sender);
            }
        }
        
        private void Obj_AI_Base_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!Enable)
                return;
            if (sender.IsMe)
            {
                if (args.SData.Name == R.Instance.Name) Rcount = Environment.TickCount;
            }
            if (!activeAssasin && autoharassq && !sender.IsMe && sender.IsEnemy && (sender as AIHeroClient).IsValidTarget(Q.Range)&& Player.ManaPercent >= manaharass)
            {
                Q.Cast(sender);
            }
        }
        
        private void Orbwalking_AfterAttack(object sender, AfterAttackEventArgs e)
        {
            if (!Enable)
                return;
            // Q after attack
            if (!E.IsReady() && Q.IsReady() && (IsCombo || IsHarass))
            {
                foreach (var x in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range)))
                    Q.CastIfWillHit(x, 2);
                if ((e.Target as AIHeroClient).IsValidTarget())
                    Q.Cast(e.Target as AIHeroClient);
            }
            // E after attack
            if (E.IsReady() && (IsCombo || IsHarass))
            {
                if ((e.Target as AIHeroClient).IsValidTarget())
                {
                    if (E.BadaoCast(e.Target as AIHeroClient))
                        DelayAction.Add(50, () => Q.Cast(e.Target as AIHeroClient));
                }
            }
        }
        
        private void Drawing_OnDraw(EventArgs args)
        {
            if (!Enable)
                return;
            if (Player.IsDead)
                return;
            if (drawq)
                CircleRender.Draw(Player.Position, Q.Range, Color.Aqua);
            if (draww)
                CircleRender.Draw(Player.Position, W.Range, Color.Aqua);
            if (drawe)
                CircleRender.Draw(Player.Position, E.Range, Color.Aqua);
            if (drawr)
                CircleRender.Draw(Player.Position, R.Range, Color.Aqua);
        }
        
        private void Obj_AI_Base_OnNewPath(AIBaseClient sender, AIBaseClientNewPathEventArgs args)
        {
            if (!Enable)
                return;
            if (Player.IsDashing()) return;
            var enemies = GameObjects.EnemyHeroes.Select(x => x.NetworkId).ToList();
            if (enemies.Contains(sender.NetworkId) && sender.IsValidTarget())
            {
                if (IsCombo)
                    DelayAction.Add(50, () => comboonnewpath());
                if (IsHarass && Player.ManaPercent >= manaharass)
                    DelayAction.Add(50, () => harassonnewpath());
            }
            if (activeAssasin)
            {
                var target = TargetSelector.SelectedTarget;
                if (target.IsValidTarget() && target.NetworkId == sender.NetworkId)
                {
                    AssasinOnNewPath();
                }
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!Enable)
            {
                //LeagueSharp.Common.Utility.HpBar//DamageIndicator.Enabled = false;
                //Custom//DamageIndicator.Enabled = false;
                return;
            }
            if ((IsCombo || IsHarass) && Orbwalker.CanMove()
                                      && (Q.IsReady() || E.IsReady()) )
            {
                Orbwalker.AttackEnabled = false;
            }
            else Orbwalker.AttackEnabled = true;
            if (autokillsteal && !activeAssasin)
                killstealUpdate();
            if (IsCombo)
                comboupdate();
            if (IsHarass && Player.ManaPercent >= manaharass)
                harassupdate();
            if (IsClear && Player.ManaPercent >= manaclear)
                ClearOnUpdate();
            if (activeAssasin)
                AssasinMode();
        }
        
        private static void killstealUpdate()
        {
            var enemies = GameObjects.EnemyHeroes;
            foreach (var x in enemies.Where(x => x.IsValidTarget(Q.Range) && Qdamage(x) > x.Health))
            {
                Q.Cast(x);
            }
            foreach (var x in enemies.Where(x => x.IsValidTarget(W.Range) && Wdamage(x) > x.Health))
            {
                W.Cast(x);
            }
            foreach (var x in enemies.Where(x => x.IsValidTarget(E.Range) && Edamage(x) > x.Health))
            {
                E.Cast(x);
            }
        }
        
        private static void comboupdate()
        {
            // use W
            if (combow)
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Magical);
                if (W.IsReady() && target.IsValidTarget() && !target.IsZombie())
                {
                    W.Cast();
                }
            }
            //use Q
            if (comboq)
            {
                if (Q.IsReady())
                {
                    foreach (var x in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range) && !x.IsZombie()))
                    {
                        if (x.HasBuffOfType(BuffType.Charm) || x.HasBuffOfType(BuffType.Stun) || x.HasBuffOfType(BuffType.Suppression))
                            if (Q.Cast(x) == CastStates.SuccessfullyCasted)
                                return;
                    }
                }
            }
        }
        
        private static void comboonnewpath()
        {
            // use Q
            if (comboq)
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                if (Q.IsReady() && target.IsValidTarget() && !target.IsZombie() &&
                    (!E.IsReady() || E.GetBadaoPrediction(target).CollisionObjects.Any()))
                {
                    if (Q.Cast(target) == CastStates.SuccessfullyCasted)
                        return;
                }
                if (Q.IsReady() &&
                    (!E.IsReady() || E.GetBadaoPrediction(target).CollisionObjects.Any()))
                {
                    foreach (var x in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range) && !x.IsZombie()))
                    {
                        if (x.HasBuffOfType(BuffType.Charm) || x.HasBuffOfType(BuffType.Stun) || x.HasBuffOfType(BuffType.Suppression))
                            if (Q.Cast(x) == CastStates.SuccessfullyCasted)
                                return;
                    }
                }
                if (!comboe && Q.IsReady() && target.IsValidTarget() && !target.IsZombie())
                {
                    if (Q.Cast(target) == CastStates.SuccessfullyCasted)
                        return;
                }
            }
            //use E
            if(comboe)
            {
                var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                if (E.IsReady() && target.IsValidTarget() && !target.IsZombie())
                {
                    if (E.BadaoCast(target) && Q.IsReady() && comboq)
                    {
                        DelayAction.Add(50, () => Q.Cast(target));
                        return;
                    }
                }
                foreach (var x in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(E.Range) && !x.IsZombie()))
                {
                    if (E.BadaoCast(x) && Q.IsReady() && comboq)
                    {
                        DelayAction.Add(50, () => Q.Cast(target));
                        return;
                    }
                }
            }
        }
        private static void harassupdate()
        {
            // use W
            if (harassw)
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Magical);
                if (W.IsReady() && target.IsValidTarget() && !target.IsZombie())
                {
                    W.Cast();
                }
            }
            //use Q
            if (harassq)
            {
                if (Q.IsReady())
                {
                    foreach (var x in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range) && !x.IsZombie()))
                    {
                        if (x.HasBuffOfType(BuffType.Charm) || x.HasBuffOfType(BuffType.Stun) || x.HasBuffOfType(BuffType.Suppression))
                            if (Q.Cast(x) == CastStates.SuccessfullyCasted)
                                return;
                    }
                }
            }
        }
        private static void harassonnewpath()
        {
            // use Q
            if (harassq)
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                if (Q.IsReady() && target.IsValidTarget() && !target.IsZombie() &&
                    (!E.IsReady() || E.GetBadaoPrediction(target).CollisionObjects.Any()))
                {
                    if (Q.Cast(target) == CastStates.SuccessfullyCasted)
                        return;
                }
                if (Q.IsReady() &&
                    (!E.IsReady() || E.GetBadaoPrediction(target).CollisionObjects.Any()))
                {
                    foreach (var x in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range) && !x.IsZombie()))
                    {
                        if (x.HasBuffOfType(BuffType.Charm) || x.HasBuffOfType(BuffType.Stun) || x.HasBuffOfType(BuffType.Suppression))
                            if (Q.Cast(x) == CastStates.SuccessfullyCasted)
                                return;
                    }
                }
                if (!harasse && Q.IsReady() && target.IsValidTarget() && !target.IsZombie())
                {
                    if (Q.Cast(target) == CastStates.SuccessfullyCasted)
                        return;
                }
            }
            //use E
            if (harasse)
            {
                var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                if (E.IsReady() && target.IsValidTarget() && !target.IsZombie())
                {
                    if (E.BadaoCast(target) && Q.IsReady() && harassq)
                    {
                        DelayAction.Add(50, () => Q.Cast(target));
                        return;
                    }
                }
                foreach (var x in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(E.Range) && !x.IsZombie()))
                {
                    if (E.BadaoCast(x) && Q.IsReady() && harassq)
                    {
                        DelayAction.Add(50, () => Q.Cast(target));
                        return;
                    }
                }
            }

        }
        private static void ClearOnUpdate()
        {
            var farmlocation = Q.GetLineFarmLocation(MinionManager.GetMinions(Q.Range));
            if (clearq && Q.IsReady() && farmlocation.MinionsHit >= clearqhit)
                Q.Cast(farmlocation.Position);
        }
        
        private static void AssasinMode()
        {
            var target = TargetSelector.SelectedTarget;
            if (Orbwalker.CanMove())
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (target.IsValidTarget() && !target.IsZombie())
            {
                var targetpos = Prediction.GetPrediction(target, 0.25f).UnitPosition.To2D();
                var distance = targetpos.Distance(Player.Position.To2D());
                if (Ignite.IsReady() && target.IsValidTarget(450))
                {
                    Player.Spellbook.CastSpell(Ignite, target);
                }
                if (!R.IsReady(3000) || Player.IsDashing())
                {
                    if (W.IsReady() && Player.Distance(target.Position) <= W.Range)
                    {
                        W.Cast();
                    }
                }
                if (R.IsReady() && AhriOrbReturn == null && AhriOrb == null && Environment.TickCount - Rcount >= 500)
                {
                    Vector2 intersec = new Vector2();
                    for (int i = 450; i >= 0; i = i - 50)
                    {
                        for (int j = 50;  j <= 600;  j = j + 50)
                        {
                            var vectors = Geometry.CircleCircleIntersection(Player.Position.To2D(),targetpos, i, j);
                            foreach (var x in vectors)
                            {
                                if (!Collide(x,target) && !x.IsWall())
                                {
                                    intersec = x;
                                    goto ABC;
                                }
                            }
                        }
                    }
                    ABC:
                    if (intersec.IsValid())
                        R.Cast(intersec.To3D());
                }
                else if (R.IsReady() && AhriOrbReturn != null &&
                         Player.Distance(targetpos) < Player.Distance(AhriOrbReturn.Position.To2D()) &&
                         Environment.TickCount - Rcount >= 0)
                {
                    var Orb = AhriOrbReturn.Position.To2D();
                    var dis = Orb.Distance(targetpos);
                    Vector2 castpos = new Vector2();
                    for (int i = 450; i >= 200; i = i - 50)
                    {
                        if (Orb.Extend(targetpos, dis + i).Distance(Player.Position.To2D()) <= R.Range &&
                            !Orb.Extend(targetpos, dis + i).IsWall())
                        {
                            castpos = Orb.Extend(targetpos, dis + i);
                            break;
                        }
                    }
                    if (castpos.IsValid())
                        R.Cast(castpos.To3D());
                }
                if (Orbwalker.CanAttack() && Player.InAutoAttackRange(target,0f))
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target.Position);
                    Orbwalker.LastAutoAttackTick = Environment.TickCount - 4;
                }
            }
        }
        private static void AssasinOnNewPath()
        {
            // use Q
            {
                var target = TargetSelector.SelectedTarget;
                if (Q.IsReady() && target.IsValidTarget() && !target.IsZombie() &&
                    (!E.IsReady() || E.GetBadaoPrediction(target).CollisionObjects.Any()))
                {
                    if (Q.Cast(target) == CastStates.SuccessfullyCasted)
                    {
                        Rcount = Environment.TickCount;
                        return;
                    }
                }
            }
            //use E
            {
                var target = TargetSelector.SelectedTarget;
                if (E.IsReady() && target.IsValidTarget() && !target.IsZombie() &&  Environment.TickCount >= Rcount + 400)
                {
                    if (E.BadaoCast(target) && Q.IsReady())
                    {
                        DelayAction.Add(50, () => castQ(target));
                        return;
                    }
                }
            }

        }
        private static bool Collide(Vector2 pos, AIHeroClient target)
        {
            E2.UpdateSourcePosition(pos.To3D(),pos.To3D());
            return
                E2.GetBadaoPrediction(target).CollisionObjects.Any();
        }
        private static void castQ(AIHeroClient target)
        {
            if(Q.Cast(target) == CastStates.SuccessfullyCasted)
                Rcount = Environment.TickCount;
        }
    }

}