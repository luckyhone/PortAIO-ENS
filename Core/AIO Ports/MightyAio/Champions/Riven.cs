using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using SharpDX;
using HPB;

namespace MightyAio.Champions
{
    internal class Riven
    {
        private static Menu Menu;

        private static readonly AIHeroClient Player = ObjectManager.Player;

        private const string IsFirstR = "RivenFengShuiEngine";
        private const string IsSecondR = "RivenIzunaBlade";
        private static readonly SpellSlot Flash = Player.GetSpellSlot("summonerFlash");
        private static Spell Q, Q1, W, E, R;
        private static Render.Text Timer, Timer2;
        private static bool forceQ;
        private static bool forceW;
        private static bool forceR;
        private static bool forceR2;
        private static bool forceItem;
        private static float LastQ;
        private static float LastR;
        private static AIBaseClient QTarget;
        private static readonly HpBarIndicator Indicator = new HpBarIndicator();
        private static int[] SpellLevels;

        private static bool Dind => Menu["Draw"].GetValue<MenuBool>("Dind").Enabled;
        private static bool DrawCB => Menu["Draw"].GetValue<MenuBool>("DrawCB").Enabled;
        private static bool KillstealW => Menu["KillSteal"].GetValue<MenuBool>("W").Enabled;
        private static bool KillstealR => Menu["KillSteal"].GetValue<MenuBool>("R").Enabled;
        private static bool DrawTimer1 => Menu["Draw"].GetValue<MenuBool>("DrawTimer1").Enabled;
        private static bool DrawTimer2 => Menu["Draw"].GetValue<MenuBool>("DrawTimer2").Enabled;
        private static bool DrawHS => Menu["Draw"].GetValue<MenuBool>("DrawHS").Enabled;
        private static bool DrawBT => Menu["Draw"].GetValue<MenuBool>("DrawBT").Enabled;
        private static bool AutoShield => Menu["E"].GetValue<MenuBool>("AutoShield").Enabled;
        private static bool Shield => Menu["E"].GetValue<MenuBool>("Shield").Enabled;
        private static bool KeepQ => Menu["Q"].GetValue<MenuBool>("KeepQ").Enabled;
        private static int QD => Menu["Q"].GetValue<MenuSlider>("QD").Value;
        private static int QLD => Menu["Q"].GetValue<MenuSlider>("QLD").Value;
        private static int AutoW => Menu["W"].GetValue<MenuSlider>("AutoW").Value;
        private static bool ComboW => Menu["W"].GetValue<MenuBool>("WC").Enabled;
        private static bool RMaxDam => Menu["Misc"].GetValue<MenuBool>("RMaxDam").Enabled;
        private static bool RKillable => Menu["R"].GetValue<MenuBool>("R").Enabled;
        private static int LaneW => Menu["Lane"].GetValue<MenuSlider>("LaneW").Value;
        private static bool LaneE => Menu["Lane"].GetValue<MenuBool>("LaneE").Enabled;
        private static bool WInterrupt => Menu["W"].GetValue<MenuBool>("WInterrupt").Enabled;
        private static bool Qstrange => Menu["Misc"].GetValue<MenuBool>("Qstrange").Enabled;
        private static bool FirstHydra => Menu["Misc"].GetValue<MenuBool>("FirstHydra").Enabled;
        private static bool LaneQ => Menu["Lane"].GetValue<MenuBool>("LaneQ").Enabled;
        private static bool Youmu => Menu["Misc"].GetValue<MenuBool>("youmu").Enabled;
        private static bool UseWC => Menu["W"].GetValue<MenuBool>("WC").Enabled;
        private static bool UseEC => Menu["E"].GetValue<MenuBool>("EC").Enabled;
        private static bool UseR => Menu["R"].GetValue<MenuBool>("R").Enabled;
        private static int UseRWhen => Menu["R"].GetValue<MenuSlider>("RHP").Value;


        public Riven()
        {
            SpellLevels = new[] {1, 2, 3, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2};
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 300);
            R = new Spell(SpellSlot.R, 900);
            R.SetSkillshot(0.25f, 45, 1600, false,  SpellType.Cone);
            OnMenuLoad();
            Timer = new Render.Text(
                "Q Expiry =>  " + ((double) (LastQ - Variables.GameTimeTickCount + 3800) / 1000).ToString("0.0"),
                (int) Drawing.WorldToScreen(Player.Position).X - 140,
                (int) Drawing.WorldToScreen(Player.Position).Y + 10, 30, Color.MidnightBlue, "calibri");
            Timer2 = new Render.Text(
                "R Expiry =>  " + (((double) LastR - Variables.GameTimeTickCount + 15000) / 1000).ToString("0.0"),
                (int) Drawing.WorldToScreen(Player.Position).X - 60,
                (int) Drawing.WorldToScreen(Player.Position).Y + 10, 30, Color.IndianRed, "calibri");
            Game.OnUpdate += OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AIBaseClient.OnDoCast += OnCast;
            AIBaseClient.OnProcessSpellCast += OnDoCast;
            AIBaseClient.OnProcessSpellCast += OnDoCastLC;
            AIBaseClient.OnPlayAnimation += OnPlay;
            AIBaseClient.OnDoCast += OnCasting;
            Interrupter.OnInterrupterSpell += Interrupt;
        }


        private static bool HasTitan()
        {
            return Player.HasItem(3748) && Player.CanUseItem(3748);
        }

        private static void CastTitan()
        {
            if (Player.HasItem(3748) && Player.CanUseItem(3748))
            {
                Player.UseItem(3748);
                Orbwalker.LastAutoAttackTick = 0;
            }
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            foreach (
                var enemy in
                ObjectManager.Get<AIHeroClient>()
                    .Where(x => x.IsValidTarget() && !x.IsDead))
                if (Dind)
                {
                    Indicator.unit = enemy;
                    Indicator.drawDmg(getComboDamage(enemy), new ColorBGRA(255, 204, 0, 170));
                }
        }

        private static void OnDoCastLC(AIBaseClient Sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!Sender.IsMe || !Orbwalker.IsAutoAttack(args.SData.Name)) return;
            if (LaneQ)
                QTarget = (AIBaseClient) args.Target;
            if (args.Target is AIMinionClient)
                if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
                {
                    var minions = GameObjects.EnemyMinions.Where(m => m.IsValidTarget(120 + 70 + Player.BoundingRadius))
                        .Cast<AIBaseClient>();
                    var jungle = GameObjects.Jungle.Where(m => m.IsValidTarget(120 + 70 + Player.BoundingRadius))
                        .Cast<AIBaseClient>();
                    var Minions = minions.Concat(jungle).OrderBy(m => m.MaxHealth).ToList();
                    if (HasTitan())
                    {
                        CastTitan();
                        return;
                    }

                    if (Q.IsReady() && LaneQ)
                    {
                        ForceItem();
                        DelayAction.Add(1, () => ForceCastQ(Minions[0]));
                    }

                    if ((!Q.IsReady() || Q.IsReady() && !LaneQ) && W.IsReady() && LaneW != 0 &&
                        Minions.Count >= LaneW)
                    {
                        ForceItem();
                        DelayAction.Add(1, ForceW);
                    }

                    if ((!Q.IsReady() || Q.IsReady() && !LaneQ) &&
                        (!W.IsReady() || W.IsReady() && LaneW == 0 || Minions.Count < LaneW) &&
                        E.IsReady() && LaneE)
                    {
                        E.Cast(Minions[0].Position);
                        DelayAction.Add(1, ForceItem);
                    }
                }
        }

        private static int Item => Player.CanUseItem(3077) && Player.HasItem(3077) ? 3077 :
            Player.CanUseItem(3074) && Player.HasItem(3074) ? 3074 : 0;

        private static void OnDoCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            var spellName = args.SData.Name;
            if (!sender.IsMe || !Orbwalker.IsAutoAttack(spellName)) return;
            QTarget = (AIBaseClient) args.Target;

            if (args.Target is AIMinionClient)
                if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
                {
                    var minions = GameObjects.EnemyMinions.Where(m => m.IsValidTarget(120 + 70 + Player.BoundingRadius))
                        .Cast<AIBaseClient>();
                    var jungle = GameObjects.Jungle.Where(m => m.IsValidTarget(120 + 70 + Player.BoundingRadius))
                        .Cast<AIBaseClient>();
                    var Mobs = minions.Concat(jungle).OrderBy(m => m.MaxHealth).ToList();
                    if (Mobs.Count != 0)
                    {
                        if (HasTitan())
                        {
                            CastTitan();
                            return;
                        }

                        if (Q.IsReady())
                        {
                            ForceItem();
                            DelayAction.Add(1, () => ForceCastQ(Mobs[0]));
                        }
                        else if (W.IsReady())
                        {
                            ForceItem();
                            DelayAction.Add(1, ForceW);
                        }
                        else if (E.IsReady())
                        {
                            E.Cast(Mobs[0].Position);
                        }
                    }
                }

            if (args.Target is AITurretClient || args.Target is Barracks || args.Target is BarracksDampenerClient ||
                args.Target is BuildingClient)
                if (args.Target.IsValid && args.Target != null && Q.IsReady() && LaneQ &&
                    Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
                    ForceCastQ((AIBaseClient) args.Target);
            if (args.Target is AIHeroClient)
            {
                var target = (AIHeroClient) args.Target;
                if (KillstealR && R.IsReady() && R.Instance.Name == IsSecondR)
                    if (target.Health < Rdame(target, target.Health) + Player.GetAutoAttackDamage(target) &&
                        target.Health > Player.GetAutoAttackDamage(target))
                        R.Cast(target.Position);
                if (KillstealW && W.IsReady())
                    if (target.Health < W.GetDamage(target) + Player.GetAutoAttackDamage(target) &&
                        target.Health > Player.GetAutoAttackDamage(target))
                        W.Cast();
                if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                {
                    if (HasTitan())
                    {
                        CastTitan();
                        return;
                    }

                    if (Q.IsReady())
                    {
                        ForceItem();
                        DelayAction.Add(1, () => ForceCastQ(target));
                    }
                    else if (W.IsReady() && InWRange(target))
                    {
                        ForceItem();
                        DelayAction.Add(1, ForceW);
                    }
                    else if (E.IsReady() && !target.InAutoAttackRange())
                    {
                        E.Cast(target.Position);
                    }
                }


                if (Orbwalker.ActiveMode == OrbwalkerMode.Harass)
                {
                    if (HasTitan())
                    {
                        CastTitan();
                        return;
                    }

                    if (Player.GetBuffCount("rivenpassiveaaboost") == 2 && Q.IsReady())
                    {
                        ForceItem();
                        DelayAction.Add(1, () => ForceCastQ(target));
                    }
                }

                if (Menu["Keys"].GetValue<MenuKeyBind>("Burst").Active)
                {
                    Orbwalker.ActiveMode = OrbwalkerMode.Combo;
                    if (HasTitan())
                    {
                        CastTitan();
                        return;
                    }

                    if (R.IsReady() && R.Instance.Name == IsSecondR)
                    {
                        ForceItem();
                        DelayAction.Add(1, ForceR2);
                    }
                    else if (Q.IsReady())
                    {
                        ForceItem();
                        DelayAction.Add(1, () => ForceCastQ(target));
                    }
                }
            }
        }

        private static void OnMenuLoad()
        {
            Menu = new Menu("Riven", "Riven", true);
            var Keys = new Menu("Keys", "Keys Binding")
            {
                new MenuKeyBind("Burst", "Burst", EnsoulSharp.SDK.MenuUI.Keys.T, KeyBindType.Press),
                new MenuKeyBind("Flee", "Flee", EnsoulSharp.SDK.MenuUI.Keys.Z, KeyBindType.Press)
            };
            Menu.Add(Keys);
            var Q = new Menu("Q", "Q")
            {
                new MenuBool("QC", "Use Q in Combo"),
                new MenuBool("QF", "Use Q To Feel"),
                new MenuBool("KeepQ", "Keep Q Alive", false),
                new MenuSlider("QD", "First,Second Q Delay", 29, 20, 43),
                new MenuSlider("QLD", "Third Q Delay", 39, 30, 53),

            };
            Menu.Add(Q);
            var W = new Menu("W", "W")
            {
                new MenuBool("WC", "Use W in Combo"),
                new MenuSlider("AutoW", "Auto W When x Enemy", 3, 1, 5),
                new MenuBool("WInterrupt", "W interrupt")
            };
            Menu.Add(W);
            var E = new Menu("E", "E")
            {
                new MenuBool("EC", "Use E in Combo"),
                new MenuBool("EF", "Use E To Feel"),
                new MenuBool("AutoShield", "Auto Cast E"),
                new MenuBool("Shield", "Auto Cast E While LastHit")

            };
            Menu.Add(E);
            var R = new Menu("R", "R")
            {
                new MenuBool("R", "Use R in Combo When Target is Killable"),
                new MenuSlider("RHP", "Use First R When target health is Below ", 75)
            };
            Menu.Add(R);

            var Lane = new Menu("Lane", "Lane")
            {
                new MenuBool("LaneQ", "Use Q While Laneclear"),
                new MenuSlider("LaneW", "Use W X Minion (0 = Don't)", 5, 1, 5),
                new MenuBool("LaneE", "Use E While Laneclear")
            };

            Menu.Add(Lane);
            var KS = new Menu("KillSteal", "KillSteal")
            {
                new MenuBool("W", "Use W"),
                new MenuBool("R", "Use Second R", false)
            };

            Menu.Add(KS);
            var Misc = new Menu("Misc", "Misc")
            {

                new MenuBool("youmu", "Use Youmus When E"),
                new MenuBool("FirstHydra", "Flash Burst Hydra Cast before W"),
                new MenuBool("Qstrange", "Strange Q For Speed", false),
                new MenuBool("RMaxDam", "Use Second R Max Damage", false),
                new MenuBool("UseSkin", "Use Skin Changer"),
                new MenuSlider("setskin", "set skin", 22, 0, 55),
                new MenuBool("autolevel", "Auto Level")
            };


            Menu.Add(Misc);

            var Draw = new Menu("Draw", "Draw")
            {
                new MenuBool("Dind", "Draw Damage Indicator"),
                new MenuBool("DrawCB", "Draw Combo Engage Range", false),
                new MenuBool("DrawBT", "Draw Burst Engage Range", false),
                new MenuBool("DrawHS", "Draw Harass Engage Range", false),
            };



            Menu.Add(Draw);

            Menu.Attach();
        }

        private static void Interrupt(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (sender.IsEnemy && W.IsReady() && sender.IsValidTarget() && !sender.IsDead && WInterrupt)
                if (sender.IsValidTarget(125 + Player.BoundingRadius + sender.BoundingRadius))
                    W.Cast();
        }

        private static int GetWRange => Player.HasBuff("RivenFengShuiEngine") ? 330 : 265;

        private static void AutoUseW()
        {
            if (AutoW > 0)
                if (Player.CountEnemyHeroesInRange(GetWRange) >= AutoW)
                    ForceW();
        }

        private static void OnTick(EventArgs args)
        {
            Orbwalker.ActiveMode = OrbwalkerMode.None;
            Timer.X = (int) Drawing.WorldToScreen(Player.Position).X - 60;
            Timer.Y = (int) Drawing.WorldToScreen(Player.Position).Y + 43;
            Timer2.X = (int) Drawing.WorldToScreen(Player.Position).X - 60;
            Timer2.Y = (int) Drawing.WorldToScreen(Player.Position).Y + 65;
            ForceSkill();
            UseRMaxDam();
            AutoUseW();
            Killsteal();
            var getskin = Menu["Misc"].GetValue<MenuSlider>("setskin").Value;
            var skin = Menu["Misc"].GetValue<MenuBool>("UseSkin").Enabled;
            if (skin && Player.SkinId != getskin) Player.SetSkin(getskin);
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo) Combo();
            if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear) Jungleclear();
            if (Orbwalker.ActiveMode == OrbwalkerMode.Harass) Harass();
            if (Menu["Keys"].GetValue<MenuKeyBind>("Burst").Active) Burst();
            if (Menu["Keys"].GetValue<MenuKeyBind>("Flee").Active) Flee();
            if (Variables.GameTimeTickCount - LastQ >= 3650 && Player.GetBuffCount("rivenpassiveaaboost") != 1 &&
                !Player.IsRecalling() && KeepQ && Q.IsReady()) Q.Cast(Game.CursorPos);
            if (Menu["Misc"].GetValue<MenuBool>("autolevel").Enabled) Levelup();
        }

        private static void Killsteal()
        {
            if (KillstealW && W.IsReady())
            {
                var targets = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(R.Range) && !x.IsDead);
                foreach (var target in targets)
                    if (target.Health < W.GetDamage(target) && InWRange(target))
                        W.Cast();
            }

            if (KillstealR && R.IsReady() && R.Instance.Name == IsSecondR)
            {
                var targets = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(R.Range) && !x.IsDead);
                foreach (var target in targets)
                    if (target.Health + target.AllShield < Rdame(target, target.Health) &&
                        !target.HasBuff("kindrednodeathbuff") &&
                        !target.HasBuff("Undying Rage") && !target.HasBuff("JudicatorIntervention"))
                        R.Cast(target.Position);
            }
        }

        private static void UseRMaxDam()
        {
            if (RMaxDam && R.IsReady() && R.Instance.Name == IsSecondR)
            {
                var targets = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(R.Range) && !x.IsDead);
                foreach (var target in targets)
                    if (target.Health / target.MaxHealth <= 0.25 &&
                        (!target.HasBuff("kindrednodeathbuff") || !target.HasBuff("Undying Rage") ||
                         !target.HasBuff("JudicatorIntervention")))
                        R.Cast(target.Position);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;
            var heropos = Drawing.WorldToScreen(ObjectManager.Player.Position);


            if (DrawCB)
                Drawing.DrawCircleIndicator(Player.Position, 250 + Player.AttackRange + 70,
                    E.IsReady() ? System.Drawing.Color.FromArgb(120, 0, 170, 255) : System.Drawing.Color.IndianRed);
            if (DrawBT && Flash != SpellSlot.Unknown)
                Drawing.DrawCircleIndicator(Player.Position, 800,
                    R.IsReady() && Flash.IsReady()
                        ? System.Drawing.Color.FromArgb(120, 0, 170, 255)
                        : System.Drawing.Color.IndianRed);
            if (DrawHS)
                Drawing.DrawCircleIndicator(Player.Position, 400,
                    Q.IsReady() && W.IsReady()
                        ? System.Drawing.Color.FromArgb(120, 0, 170, 255)
                        : System.Drawing.Color.IndianRed);

        }

        private static void Jungleclear()
        {
            var minions = GameObjects.EnemyMinions.Where(m => m.IsValidTarget(250 + Player.AttackRange + 70))
                .Cast<AIBaseClient>();
            var jungle = GameObjects.Jungle.Where(m => m.IsValidTarget(250 + Player.AttackRange + 70))
                .Cast<AIBaseClient>();
            var Mobs = minions.Concat(jungle).OrderBy(m => m.MaxHealth).ToList();

            if (Mobs.Count <= 0)
                return;

            if (W.IsReady() && E.IsReady() && !Mobs[0].InAutoAttackRange())
            {
                E.Cast(Mobs[0].Position);
                DelayAction.Add(1, ForceItem);
                DelayAction.Add(200, ForceW);
            }
        }

        private static void Combo()
        {
            var targetR = TargetSelector.GetTarget(250 + Player.AttackRange + 70,DamageType.Physical);
            if (targetR == null)
                return;
            if (R.IsReady() && R.Instance.Name == IsFirstR && targetR.InAutoAttackRange() && UseR &&
                targetR.HealthPercent < UseRWhen) ForceR();
            if (R.IsReady() && R.Instance.Name == IsFirstR && W.IsReady() && InWRange(targetR) && ComboW && UseR &&
                targetR.HealthPercent < UseRWhen)
            {
                ForceR();
                DelayAction.Add(1, ForceW);
            }

            if (W.IsReady() && InWRange(targetR) && ComboW) W.Cast();
            if (R.IsReady() && R.Instance.Name == IsFirstR && W.IsReady() &&
                E.IsReady() && targetR.IsValidTarget() && !targetR.IsDead && (IsKillableR(targetR) || UseR))
            {
                if (!InWRange(targetR))
                {
                    E.Cast(targetR.Position);
                    ForceR();
                    DelayAction.Add(200, ForceW);
                    DelayAction.Add(305, () => ForceCastQ(targetR));
                }
            }
            else if (R.IsReady() && R.Instance.Name == IsFirstR && W.IsReady() &&
                     E.IsReady() && targetR.IsValidTarget() && !targetR.IsDead && (IsKillableR(targetR) || UseR))
            {
                if (!InWRange(targetR))
                {
                    E.Cast(targetR.Position);
                    ForceR();
                    DelayAction.Add(200, ForceW);
                }
            }
            else if (W.IsReady() && E.IsReady() && UseEC)
            {
                if (targetR.IsValidTarget() && !targetR.IsDead && !InWRange(targetR))
                {
                    E.Cast(targetR.Position);
                    DelayAction.Add(10, ForceItem);
                    DelayAction.Add(200, ForceW);
                    DelayAction.Add(305, () => ForceCastQ(targetR));
                }
            }
            else if (W.IsReady() && E.IsReady() && UseWC)
            {
                if (targetR.IsValidTarget() && !targetR.IsDead && !InWRange(targetR))
                {
                    E.Cast(targetR.Position);
                    DelayAction.Add(10, ForceItem);
                    DelayAction.Add(240, ForceW);
                }
            }
            else if (E.IsReady() && UseEC)
            {
                if (targetR.IsValidTarget() && !targetR.IsDead && !InWRange(targetR)) E.Cast(targetR.Position);
            }
        }

        private static void Burst()
        {
            Orbwalker.ActiveMode = OrbwalkerMode.Combo;
            var target = TargetSelector.SelectedTarget;
            if (target != null && target.IsValidTarget() && !target.IsDead)
            {
                if (R.IsReady() && R.Instance.Name == IsFirstR && W.IsReady() && E.IsReady() &&
                    Player.Distance(target.Position) <= 250 + 70 + Player.AttackRange)
                {
                    E.Cast(target.Position);
                    CastYoumoo();
                    ForceR();
                    DelayAction.Add(100, ForceW);
                }
                else if (R.IsReady() && R.Instance.Name == IsFirstR && E.IsReady() && W.IsReady() && Q.IsReady() &&
                         Player.Distance(target.Position) <= 400 + 70 + Player.AttackRange)
                {
                    E.Cast(target.Position);
                    CastYoumoo();
                    ForceR();
                    DelayAction.Add(150, () => ForceCastQ(target));
                    DelayAction.Add(160, ForceW);
                }
                else if (Flash.IsReady()
                         && R.IsReady() && R.Instance.Name == IsFirstR && Player.Distance(target.Position) <= 800 &&
                         (!FirstHydra || FirstHydra && !HasItem()))
                {
                    E.Cast(target.Position);
                    CastYoumoo();
                    ForceR();
                    DelayAction.Add(180, FlashW);
                }
                else if (Flash.IsReady()
                         && R.IsReady() && E.IsReady() && W.IsReady() && R.Instance.Name == IsFirstR &&
                         Player.Distance(target.Position) <= 800 && FirstHydra && HasItem())
                {
                    E.Cast(target.Position);
                    ForceR();
                    DelayAction.Add(100, ForceItem);
                    DelayAction.Add(210, FlashW);
                }
            }
        }


        private static bool HasItem()
        {
            return Player.HasItem(ItemId.Tiamat) && Player.CanUseItem("Tiamat") ||
                   Player.HasItem(ItemId.Ravenous_Hydra) && Player.CanUseItem("Ravenous Hydra");
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(400,DamageType.Physical);
            if (Q.IsReady() && W.IsReady() && E.IsReady() && Player.GetBuffCount("rivenpassiveaaboost") == 1)
                if (target.IsValidTarget() && !target.IsDead)
                {
                    ForceCastQ(target);
                    DelayAction.Add(1, ForceW);
                }

            if (Q.IsReady() && E.IsReady() && Player.GetBuffCount("rivenpassiveaaboost") == 3 &&
                !Orbwalker.CanAttack() && Orbwalker.CanMove())
            {
                var epos = Player.Position +
                           (Player.Position - target.Position).Normalized() * 300;
                E.Cast(epos);
                DelayAction.Add(190, () => Q.Cast(epos));
            }
        }

        private static void Flee()
        {
            Orbwalker.Move(Game.CursorPos);
            var enemy =
                GameObjects.EnemyHeroes.Where(
                    hero =>
                        hero.IsValidTarget(Player.HasBuff("RivenFengShuiEngine")
                            ? 70 + 195 + Player.BoundingRadius
                            : 70 + 120 + Player.BoundingRadius) && W.IsReady());
            var x = Player.Position.Extend(Game.CursorPos, 300);
            if (W.IsReady() && enemy.Any())
                foreach (var target in enemy)
                    if (InWRange(target))
                        W.Cast();
            if (Q.IsReady() && !Player.IsDashing()) Q.Cast(Game.CursorPos);
            if (E.IsReady() && !Player.IsDashing()) E.Cast(x);
        }

        private static void OnPlay(AIBaseClient sender, AIBaseClientPlayAnimationEventArgs args)
        {
            if (!sender.IsMe) return;
            switch (args.Animation)
            {
                case "Spell1a":
                    LastQ = Variables.GameTimeTickCount;
                    if (Qstrange && Orbwalker.ActiveMode != OrbwalkerMode.None)
                        if (Orbwalker.ActiveMode != OrbwalkerMode.None &&
                            Orbwalker.ActiveMode != OrbwalkerMode.LastHit &&
                            !Menu["Keys"].GetValue<MenuKeyBind>("Flee").Active)
                            DelayAction.Add(QD * 1 + 1, Reset);
                    Orbwalker.ResetAutoAttackTimer();
                    break;
                case "Spell1b":
                    LastQ = Variables.GameTimeTickCount;
                    if (Qstrange && Orbwalker.ActiveMode != OrbwalkerMode.None)
                        if (Orbwalker.ActiveMode != OrbwalkerMode.None &&
                            Orbwalker.ActiveMode != OrbwalkerMode.LastHit &&
                            !Menu["Keys"].GetValue<MenuKeyBind>("Flee").Active)
                            DelayAction.Add(QD * 1 + 1, Reset);
                    Orbwalker.ResetAutoAttackTimer();
                    break;
                case "Spell1c":
                    LastQ = Variables.GameTimeTickCount;
                    if (Qstrange && Orbwalker.ActiveMode != OrbwalkerMode.None)
                        if (Orbwalker.ActiveMode != OrbwalkerMode.None &&
                            Orbwalker.ActiveMode != OrbwalkerMode.LastHit &&
                            !Menu["Keys"].GetValue<MenuKeyBind>("Flee").Active)
                            DelayAction.Add(QLD * 1 + 3, Reset);
                    Orbwalker.ResetAutoAttackTimer();
                    break;
                case "Spell3":
                    if ((Menu["Keys"].GetValue<MenuKeyBind>("Burst").Active ||
                         Orbwalker.ActiveMode == OrbwalkerMode.Combo ||
                         Menu["Keys"].GetValue<MenuKeyBind>("Flee").Active) && Youmu) CastYoumoo();
                    Orbwalker.ResetAutoAttackTimer();
                    break;
                case "Spell4a":
                    LastR = Variables.GameTimeTickCount;
                    Orbwalker.ResetAutoAttackTimer();
                    break;
                case "Spell4b":
                    var target = TargetSelector.SelectedTarget;
                    if (Q.IsReady() && target.IsValidTarget()) ForceCastQ(target);
                    Orbwalker.ResetAutoAttackTimer();
                    break;
            }
        }

        private static void OnCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;

            if (args.SData.Name.Contains("ItemTiamatCleave")) forceItem = false;
            if (args.SData.Name.Contains("RivenTriCleave")) forceQ = false;
            if (args.SData.Name.Contains("RivenMartyr")) forceW = false;
            if (args.SData.Name == IsFirstR) forceR = false;
            if (args.SData.Name == IsSecondR) forceR2 = false;
        }

        private static void Reset()
        {
            Orbwalker.LastAutoAttackTick = 0;
            Player.IssueOrder(GameObjectOrder.MoveTo,
                Player.Position.Extend(Game.CursorPos, Player.Distance(Game.CursorPos) + 10));
        }

        private static bool InWRange(GameObject target)
        {
            return Player.HasBuff("RivenFengShuiEngine") && target != null
                ? 330 >= Player.Distance(target.Position)
                : 265 >= Player.Distance(target.Position);
        }


        private static void ForceSkill()
        {
            if (Player.GetBuffCount("rivenpassiveaaboost") < 3 && forceQ && QTarget != null &&
                QTarget.IsValidTarget(E.Range + Player.BoundingRadius + 70) && Q.IsReady()) Q.Cast(QTarget);
            if (forceW) W.Cast();
            if (forceR && R.Instance.Name == IsFirstR) R.Cast();
            if (forceItem && Player.CanUseItem(Item) && Player.HasItem(Item) && Item != 0) Player.UseItem(Item);
            if (forceR2 && R.Instance.Name == IsSecondR)
            {
                var target = TargetSelector.SelectedTarget;
                if (target != null) R.Cast(target.Position);
            }
        }

        private static void ForceItem()
        {
            if (Player.CanUseItem(Item) && Player.HasItem(Item) && Item != 0) forceItem = true;
            DelayAction.Add(500, () => forceItem = false);
        }

        private static void ForceR()
        {
            forceR = R.IsReady() && R.Instance.Name == IsFirstR;
            DelayAction.Add(500, () => forceR = false);
        }

        private static void ForceR2()
        {
            forceR2 = R.IsReady() && R.Instance.Name == IsSecondR;
            DelayAction.Add(500, () => forceR2 = false);
        }

        private static void ForceW()
        {
            forceW = W.IsReady();
            DelayAction.Add(500, () => forceW = false);
        }

        private static void ForceCastQ(AttackableUnit target)
        {
            forceQ = true;
            QTarget = (AIBaseClient) target;
        }


        private static void FlashW()
        {
            var target = TargetSelector.SelectedTarget;
            if (target != null && target.IsValidTarget() && !target.IsDead)
            {
                W.Cast();
                DelayAction.Add(10, () => Player.Spellbook.CastSpell(Flash, target.Position));
            }
        }


        private static void CastYoumoo()
        {
            if (Player.CanUseItem(3142)) Player.UseItem(3142);
        }

        private static void OnCasting(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy && sender.Type == Player.Type &&
                (AutoShield || Shield && Orbwalker.ActiveMode == OrbwalkerMode.LastHit))
            {
                var epos = Player.Position +
                           (Player.Position - sender.Position).Normalized() * 300;

                if (Player.Distance(sender.Position) <= args.SData.CastRange)
                {
                    switch (args.SData.TargetingType)
                    {
                        case SpellDataTargetType.Target:

                            if (args.Target.NetworkId == Player.NetworkId)
                                if (Orbwalker.ActiveMode == OrbwalkerMode.LastHit &&
                                    !args.SData.Name.Contains("NasusW"))
                                    if (E.IsReady())
                                        E.Cast(epos);

                            break;
                        case SpellDataTargetType.SelfAoe:

                            if (Orbwalker.ActiveMode == OrbwalkerMode.LastHit)
                                if (E.IsReady())
                                    E.Cast(epos);

                            break;
                    }

                    if (args.SData.Name.Contains("IreliaEquilibriumStrike"))
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (W.IsReady() && InWRange(sender)) W.Cast();
                            else if (E.IsReady()) E.Cast(epos);
                        }

                    if (args.SData.Name.Contains("TalonCutthroat"))
                        if (args.Target.NetworkId == Player.NetworkId)
                            if (W.IsReady())
                                W.Cast();
                    if (args.SData.Name.Contains("RenektonPreExecute"))
                        if (args.Target.NetworkId == Player.NetworkId)
                            if (W.IsReady())
                                W.Cast();
                    if (args.SData.Name.Contains("GarenRPreCast"))
                        if (args.Target.NetworkId == Player.NetworkId)
                            if (E.IsReady())
                                E.Cast(epos);
                    if (args.SData.Name.Contains("GarenQAttack"))
                        if (args.Target.NetworkId == Player.NetworkId)
                            if (E.IsReady())
                                E.Cast();
                    if (args.SData.Name.Contains("XenZhaoThrust3"))
                        if (args.Target.NetworkId == Player.NetworkId)
                            if (W.IsReady())
                                W.Cast();
                    if (args.SData.Name.Contains("RengarQ"))
                        if (args.Target.NetworkId == Player.NetworkId)
                            if (E.IsReady())
                                E.Cast();
                    if (args.SData.Name.Contains("RengarPassiveBuffDash"))
                        if (args.Target.NetworkId == Player.NetworkId)
                            if (E.IsReady())
                                E.Cast();
                    if (args.SData.Name.Contains("RengarPassiveBuffDashAADummy"))
                        if (args.Target.NetworkId == Player.NetworkId)
                            if (E.IsReady())
                                E.Cast();
                    if (args.SData.Name.Contains("TwitchEParticle"))
                        if (args.Target.NetworkId == Player.NetworkId)
                            if (E.IsReady())
                                E.Cast();
                    if (args.SData.Name.Contains("FizzPiercingStrike"))
                        if (args.Target.NetworkId == Player.NetworkId)
                            if (E.IsReady())
                                E.Cast();
                    if (args.SData.Name.Contains("HungeringStrike"))
                        if (args.Target.NetworkId == Player.NetworkId)
                            if (E.IsReady())
                                E.Cast();
                    if (args.SData.Name.Contains("YasuoDash"))
                        if (args.Target.NetworkId == Player.NetworkId)
                            if (E.IsReady())
                                E.Cast();
                    if (args.SData.Name.Contains("KatarinaRTrigger"))
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (W.IsReady() && InWRange(sender)) W.Cast();
                            else if (E.IsReady()) E.Cast();
                        }

                    if (args.SData.Name.Contains("YasuoDash"))
                        if (args.Target.NetworkId == Player.NetworkId)
                            if (E.IsReady())
                                E.Cast();
                    if (args.SData.Name.Contains("KatarinaE"))
                        if (args.Target.NetworkId == Player.NetworkId)
                            if (W.IsReady())
                                W.Cast();
                    if (args.SData.Name.Contains("MonkeyKingQAttack"))
                        if (args.Target.NetworkId == Player.NetworkId)
                            if (E.IsReady())
                                E.Cast();
                    if (args.SData.Name.Contains("MonkeyKingSpinToWin"))
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (E.IsReady()) E.Cast();
                            else if (W.IsReady()) W.Cast();
                        }

                    if (args.SData.Name.Contains("MonkeyKingQAttack"))
                        if (args.Target.NetworkId == Player.NetworkId)
                            if (E.IsReady())
                                E.Cast();
                    if (args.SData.Name.Contains("MonkeyKingQAttack"))
                        if (args.Target.NetworkId == Player.NetworkId)
                            if (E.IsReady())
                                E.Cast();
                    if (args.SData.Name.Contains("MonkeyKingQAttack"))
                        if (args.Target.NetworkId == Player.NetworkId)
                            if (E.IsReady())
                                E.Cast();
                }
            }
        }

        private static double basicdmg(AIBaseClient target)
        {
            if (target != null)
            {
                double dmg = 0;
                double passivenhan = 0;
                if (Player.Level >= 18)
                    passivenhan = 0.5;
                else if (Player.Level >= 15)
                    passivenhan = 0.45;
                else if (Player.Level >= 12)
                    passivenhan = 0.4;
                else if (Player.Level >= 9)
                    passivenhan = 0.35;
                else if (Player.Level >= 6)
                    passivenhan = 0.3;
                else if (Player.Level >= 1) passivenhan = 0.25;
                if (HasItem()) dmg = dmg + Player.GetAutoAttackDamage(target) * 0.7;
                if (W.IsReady()) dmg = dmg + W.GetDamage(target);
                if (Q.IsReady())
                {
                    var qnhan = 4 - Player.GetBuffCount("rivenpassiveaaboost");
                    dmg = dmg + Q.GetDamage(target) * qnhan +
                          Player.GetAutoAttackDamage(target) * qnhan * (1 + passivenhan);
                }

                dmg = dmg + Player.GetAutoAttackDamage(target) * (1 + passivenhan);
                return dmg;
            }

            return 0;
        }


        private static float getComboDamage(AIBaseClient enemy)
        {
            if (enemy != null)
            {
                float damage = 0;
                float passivenhan = 0;
                if (Player.Level >= 18)
                    passivenhan = 0.5f;
                else if (Player.Level >= 15)
                    passivenhan = 0.45f;
                else if (Player.Level >= 12)
                    passivenhan = 0.4f;
                else if (Player.Level >= 9)
                    passivenhan = 0.35f;
                else if (Player.Level >= 6)
                    passivenhan = 0.3f;
                else if (Player.Level >= 1) passivenhan = 0.25f;
                if (HasItem()) damage = damage + (float) Player.GetAutoAttackDamage(enemy) * 0.7f;
                if (W.IsReady()) damage = damage + W.GetDamage(enemy);
                if (Q.IsReady())
                {
                    var qnhan = 4 - Player.GetBuffCount("rivenpassiveaaboost");
                    damage = damage + Q.GetDamage(enemy) * qnhan +
                             (float) Player.GetAutoAttackDamage(enemy) * qnhan * (1 + passivenhan);
                }

                damage = damage + (float) Player.GetAutoAttackDamage(enemy) * (1 + passivenhan);
                if (R.IsReady()) return damage * 1.2f + R.GetDamage(enemy);

                return damage;
            }

            return 0;
        }

        private static bool IsKillableR(AIHeroClient target)
        {
            if (RKillable && target.IsValidTarget() && totaldame(target) >= target.Health &&
                basicdmg(target) <= target.Health || Player.CountEnemyHeroesInRange(900) >= 2 &&
                !target.HasBuff("kindrednodeathbuff") && !target.HasBuff("Undying Rage") &&
                !target.HasBuff("JudicatorIntervention")) return true;
            return false;
        }

        private static double totaldame(AIBaseClient target)
        {
            if (target != null)
            {
                double dmg = 0;
                double passivenhan = 0;
                if (Player.Level >= 18)
                    passivenhan = 0.5;
                else if (Player.Level >= 15)
                    passivenhan = 0.45;
                else if (Player.Level >= 12)
                    passivenhan = 0.4;
                else if (Player.Level >= 9)
                    passivenhan = 0.35;
                else if (Player.Level >= 6)
                    passivenhan = 0.3;
                else if (Player.Level >= 1) passivenhan = 0.25;
                if (HasItem()) dmg = dmg + Player.GetAutoAttackDamage(target) * 0.7;
                if (W.IsReady()) dmg = dmg + W.GetDamage(target);
                if (Q.IsReady())
                {
                    var qnhan = 4 - Player.GetBuffCount("rivenpassiveaaboost");
                    dmg = dmg + Q.GetDamage(target) * qnhan +
                          Player.GetAutoAttackDamage(target) * qnhan * (1 + passivenhan);
                }

                dmg = dmg + Player.GetAutoAttackDamage(target) * (1 + passivenhan);
                if (R.IsReady())
                {
                    var rdmg = Rdame(target, target.Health - dmg * 1.2);
                    return dmg * 1.2 + rdmg;
                }

                return dmg;
            }

            return 0;
        }

        private static double Rdame(AIBaseClient target, double health)
        {
            if (target != null)
            {
                var missinghealth = (target.MaxHealth - health) / target.MaxHealth > 0.75
                    ? 0.75
                    : (target.MaxHealth - health) / target.MaxHealth;
                var pluspercent = missinghealth * (8 / 2.667);
                var rawdmg = new double[] {100, 150, 200}[R.Level - 1] + 0.6 * Player.FlatPhysicalDamageMod;
                return Player.CalculateDamage(target, DamageType.Physical, rawdmg * (1 + pluspercent));
            }

            return 0;
        }

        private static void Levelup()
        {
            var qLevel = Q.Level;
            var wLevel = W.Level;
            var eLevel = E.Level;
            var rLevel = R.Level;

            if (qLevel + wLevel + eLevel + rLevel >= ObjectManager.Player.Level || Player.Level > 18) return;

            var level = new[] {0, 0, 0, 0};
            for (var i = 0; i < ObjectManager.Player.Level; i++)
                level[SpellLevels[i] - 1] = level[SpellLevels[i] - 1] + 1;

            if (qLevel < level[0]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);

            if (wLevel < level[1]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);

            if (eLevel < level[2]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
            if (rLevel < level[3]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
        }

    }
}