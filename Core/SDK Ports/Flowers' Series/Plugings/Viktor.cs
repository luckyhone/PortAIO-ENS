using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using Flowers_Series.Common;
using Movement;
using SharpDX;
using static Flowers_Series.Common.Manager;
using HitChance = EnsoulSharp.SDK.HitChance;
using PredictionInput = Movement.PredictionInput;
using Render = LeagueSharpCommon.Render;

namespace Flowers_Series.Plugings
{
    public static class Viktor
    {
        private static AIHeroClient Target = null;
        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell E2;
        private static Spell R;
        private static int tick = 0;
        private static HpBarDraw DrawHpBar = new HpBarDraw();

        private static AIHeroClient Me => Program.Me;

        private static Menu Menu => Program.Menu;

        public static void Init()
        {
            Q = new Spell(SpellSlot.Q, 600f);
            W = new Spell(SpellSlot.W, 700f);
            E = new Spell(SpellSlot.E, 525f);
            E2 = new Spell(SpellSlot.E, 525f + 700f);
            R = new Spell(SpellSlot.R, 700f);

            W.SetSkillshot(0.5f, 300f, float.MaxValue, false, SpellType.Circle);
            E.SetSkillshot(0.25f, 100f, float.MaxValue, false, SpellType.Line);
            E2.SetSkillshot(0.25f, 100f, float.MaxValue, false, SpellType.Line);
            R.SetSkillshot(0.25f, 450f, float.MaxValue, false, SpellType.Circle);

            Q.DamageType = W.DamageType = E.DamageType = E2.DamageType = R.DamageType = DamageType.Magical;
            W.MinHitChance = E.MinHitChance = E2.MinHitChance = R.MinHitChance = HitChance.High;

            var ComboMenu = Menu.Add(new Menu("Viktor_Combo", "Combo"));
            {
                ComboMenu.Add(new MenuBool("Q", "Use Q", true));
                ComboMenu.Add(new MenuBool("QAuto", "Use Q|Auto Speed Up"));
                ComboMenu.Add(new MenuBool("W", "Use W", true));
                ComboMenu.Add(new MenuBool("WSmart", "Solo W Cast", false));
                ComboMenu.Add(new MenuSlider("WMin", "Min Enemies to Cast W", 2, 1, 5));
                ComboMenu.Add(new MenuBool("E", "Use E", true));
                ComboMenu.Add(new MenuBool("R", "Use R", true));
                ComboMenu.Add(new MenuBool("ROnlyKill", "Only Use R In Can Kill Enemy(1v1)", true));
                ComboMenu.Add(new MenuSlider("RCounts", "Or Counts Enemies >= (6 is off)", 3, 2, 6));
                ComboMenu.Add(new MenuBool("DisableAA", "Disable Auto Attack"));
            }

            var HarassMenu = Menu.Add(new Menu("Viktor_Harass", "Harass"));
            {
                HarassMenu.Add(new MenuBool("Q", "Use Q", true));
                HarassMenu.Add(new MenuBool("E", "Use E", true));
                HarassMenu.Add(new MenuSlider("Mana", "Min Harass ManaPercent", 40));
            }

            var LaneClearMenu = Menu.Add(new Menu("Viktor_LaneClear", "Lane Clear"));
            {
                LaneClearMenu.Add(new MenuBool("Q", "Use Q", true));
                LaneClearMenu.Add(new MenuBool("E", "Use E", true));
                LaneClearMenu.Add(new MenuSlider("EMin", "Use E Clear Min", 3, 1, 7));
                LaneClearMenu.Add(new MenuSlider("Mana", "Min LaneClear ManaPercent", 40));
            }

            var JungleClearMenu = Menu.Add(new Menu("Viktor_JungleClear", "Jungle Clear"));
            {
                JungleClearMenu.Add(new MenuBool("Q", "Use Q", true));
                JungleClearMenu.Add(new MenuBool("E", "Use E", true));
                JungleClearMenu.Add(new MenuSlider("Mana", "Min JungleClear ManaPercent", 40));
            }

            var KillStealMenu = Menu.Add(new Menu("Viktor_KillSteal", "Kill Steal"));
            {
                KillStealMenu.Add(new MenuBool("Q", "Use Q", true));
                KillStealMenu.Add(new MenuBool("E", "Use E", true));
            }

            var FleeMenu = Menu.Add(new Menu("Viktor_Flee", "Flee"));
            {
                FleeMenu.Add(new MenuBool("Q", "Use Q", true));
                FleeMenu.Add(new MenuKeyBind("Key", "Key", Keys.Z, KeyBindType.Press));
            }

            var MiscMenu = Menu.Add(new Menu("Viktor_Misc", "Misc"));
            {
                MiscMenu.Add(new MenuBool("AutoWInMePos", "Use W | Anti Gapcloser", true));
                MiscMenu.Add(new MenuBool("SmartW", "Use W | Interrupt", true));
                MiscMenu.Add(new MenuBool("AutoFollowR", "Auto R Follow", true));
            }

            var DrawMenu = Menu.Add(new Menu("Viktor_Draw", "Drawings"));
            {
                DrawMenu.Add(new MenuBool("Q", "Draw Q"));
                DrawMenu.Add(new MenuBool("W", "Draw W"));
                DrawMenu.Add(new MenuBool("E", "Draw E"));
                DrawMenu.Add(new MenuBool("EMax", "Draw E Max Range"));
                DrawMenu.Add(new MenuBool("R", "Draw R"));
                DrawMenu.Add(new MenuBool("DrawDamage", "Draw Combo Damage", true));
                DrawMenu.Add(new MenuBool("DrawTarget", "Draw Target", true));
            }

            Manager.WriteConsole(GameObjects.Player.CharacterName + " Inject!");

            Orbwalker.OnBeforeAttack += OnAction;
            Game.OnUpdate += OnUpdate;
            AntiGapcloser.OnGapcloser += OnGapCloser;
            AIBaseClient.OnDoCast += OnProcessSpellCast;
            Drawing.OnDraw += OnDraw;
        }

        private static void OnAction(object sender, BeforeAttackEventArgs Args)
        {
            if (InCombo && Menu["Viktor_Combo"]["DisableAA"].GetValue<MenuBool>().Enabled && !Me.HasBuff("ViktorPowerTransferReturn"))
            {
                Args.Process = false;
            }
            else
            {
                Args.Process = true;
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Menu["Viktor_Draw"]["Q"].GetValue<MenuBool>().Enabled && Q.IsReady())
                Render.Circle.DrawCircle(Me.Position, Q.Range, System.Drawing.Color.SkyBlue);

            if (Menu["Viktor_Draw"]["W"].GetValue<MenuBool>().Enabled && W.IsReady())
                Render.Circle.DrawCircle(Me.Position, W.Range, System.Drawing.Color.YellowGreen);

            if (Menu["Viktor_Draw"]["E"].GetValue<MenuBool>().Enabled)
                Render.Circle.DrawCircle(Me.Position, E.Range, System.Drawing.Color.PaleGoldenrod);

            if (Menu["Viktor_Draw"]["EMax"].GetValue<MenuBool>().Enabled)
                Render.Circle.DrawCircle(Me.Position, E2.Range, System.Drawing.Color.Red);

            if (Menu["Viktor_Draw"]["R"].GetValue<MenuBool>().Enabled && R.IsReady())
                Render.Circle.DrawCircle(Me.Position, R.Range, System.Drawing.Color.LightSkyBlue);

            if (Menu["Viktor_Draw"]["DrawTarget"].GetValue<MenuBool>().Enabled && (InCombo || InHarass))
            {
                if (Target != null)
                {
                    Render.Circle.DrawCircle(Target.Position, Target.BoundingRadius, System.Drawing.Color.Red, 2);
                }
            }

            if (Menu["Viktor_Draw"]["DrawDamage"].GetValue<MenuBool>().Enabled)
            {
                foreach (var e in ObjectManager.Get<AIHeroClient>().Where(e => e.IsValidTarget() && !e.IsZombie()))
                {
                    HpBarDraw.Unit = e;
                    HpBarDraw.DrawDmg((float)GetDamage(e), new ColorBGRA(255, 204, 0, 170));
                }
            }
        }

        private static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs Args)
        {
            if (Menu["Viktor_Misc"]["SmartW"].GetValue<MenuBool>().Enabled && sender.IsEnemy && W.IsReady() && sender.IsValidTarget(W.Range))
            {
                if (CastWTargetPos(Args.SData.Name))
                {
                    W.Cast(sender.ServerPosition);
                }
                else if (CastWMePos(Args.SData.Name))
                {
                    if (Args.To.Distance(Me.Position) <= 100)
                        W.Cast(Me);
                }
            }
        }

        private static void OnGapCloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (Menu["Viktor_Misc"]["AutoWInMePos"].GetValue<MenuBool>().Enabled && W.IsReady())
            {
                if (Me.Distance(args.EndPosition) <= 170)
                    W.Cast(Me);
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Me.IsDead || Me.IsRecalling())
                return;

            if (Menu["Viktor_Misc"]["AutoFollowR"].GetValue<MenuBool>().Enabled && Me.HasBuff("ViktorChaosStormTimer"))
                FollowLogic();

            if (InCombo)
                Combo();

            if (InHarass)
                Harass();

            if (InClear)
            {
                LaneClear();
                JungleClear();
            }

            KillSteal();

            if (Menu["Viktor_Flee"]["Key"].GetValue<MenuKeyBind>().Active)
            {
                Orbwalker.Move(Game.CursorPos);

                if (Q.IsReady() && Menu["Viktor_Flee"]["Q"].GetValue<MenuBool>().Enabled)
                {
                    var minion = GetMinions(Me.Position, Q.Range).FirstOrDefault();
                    var mob = GetMobs(Me.Position, Q.Range).FirstOrDefault();

                    if (minion != null)
                    {
                        Q.CastOnUnit(minion);
                        return;
                    }
                    else if (mob != null)
                    {
                        Q.CastOnUnit(mob);
                        return;
                    }
                }
            }

            if (!InCombo && !InHarass)
            {
                Target = null;
            }
        }
        
        private static void FollowLogic()
        {
            if (Variables.TickCount >= tick + 500)
            {
                var e = TargetSelector.GetTarget(2000, DamageType.Magical);

                if (e != null && e.IsValid && e.IsVisible && e.IsHPBarRendered)
                {
                    R.Cast(e.ServerPosition);
                    tick = Variables.TickCount;
                }
                else if (e == null)
                {
                    R.Cast(Me.ServerPosition);
                    tick = Variables.TickCount;
                }
            }
        }
        
        private static void Combo()
        {
            var target = GetTarget(1200, DamageType.Magical);

            if (CheckTarget(target))
            {
                Target = target;

                if (E.IsReady() && Menu["Viktor_Combo"]["E"].GetValue<MenuBool>().Enabled)
                {
                    CastE(target);
                }

                if (Q.IsReady() && Menu["Viktor_Combo"]["Q"].GetValue<MenuBool>().Enabled && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }

                if (W.IsReady() && Menu["Viktor_Combo"]["W"].GetValue<MenuBool>().Enabled && target.IsValidTarget(W.Range))
                {
                    if (Menu["Viktor_Combo"]["WSmart"].GetValue<MenuBool>().Enabled)
                    {
                        if (Me.IsFacing(target))
                        {
                            var WPos = W.GetPrediction(target).CastPosition - 150 * (W.GetPrediction(target).UnitPosition - target.ServerPosition).Normalized();
                            W.Cast(WPos);
                        }
                        else if (!Me.IsFacing(target))
                        {
                            var WPos = W.GetPrediction(target).CastPosition + 150 * (W.GetPrediction(target).UnitPosition - target.ServerPosition).Normalized();
                            W.Cast(WPos);
                        }
                    }

                    if (target.CountEnemyHeroesInRange(W.Range) >= Menu["Viktor_Combo"]["WMin"].GetValue<MenuSlider>().Value)
                    {
                        W.Cast(target);
                    }
                }

                if (R.IsReady() && Menu["Viktor_Combo"]["R"].GetValue<MenuBool>().Enabled && !Me.HasBuff("ViktorChaosStormTimer"))
                {
                    var GetDamage = Manager.GetDamage(target);

                    if (target.HealthPercent > 5 && Menu["Viktor_Combo"]["ROnlyKill"].GetValue<MenuBool>().Enabled && target.Health < GetDamage)
                    {
                        R.Cast(target.ServerPosition);
                    }

                    if (Me.CountEnemyHeroesInRange(1200) >= Menu["Viktor_Combo"]["RCounts"].GetValue<MenuSlider>().Value)
                    {
                        var couts = GameObjects.EnemyHeroes.OrderBy(x => x.Health - Manager.GetDamage(x)).Where(x => x.IsValidTarget(1200) && !x.IsDead && !x.IsZombie());

                        foreach (var cast in couts)
                        {
                            R.Cast(cast);
                        }
                    }
                }
            }
            else
            {
                if (Menu["Viktor_Combo"]["QAuto"].GetValue<MenuBool>().Enabled && Q.IsReady())
                {
                    var minion = GetMinions(Me.Position, Q.Range).FirstOrDefault();
                    var mob = GetMobs(Me.Position, Q.Range).FirstOrDefault();

                    if (minion != null && minion.IsValidTarget(Q.Range))
                    {
                        Q.CastOnUnit(minion);
                    }
                    else if (mob != null && mob.IsValidTarget(Q.Range))
                    {
                        Q.CastOnUnit(mob);
                    }
                }
            }
        }
        
        private static void Harass()
        {
            if (Me.ManaPercent > Menu["Viktor_Harass"]["Mana"].GetValue<MenuSlider>().Value)
            {
                var target = GetTarget(E2.Range, DamageType.Magical);

                if (CheckTarget(target))
                {
                    Target = target;

                    if (E.IsReady() && Menu["Viktor_Harass"]["E"].GetValue<MenuBool>().Enabled)
                    {
                        CastE(target);
                    }

                    if (Q.IsReady() && Menu["Viktor_Harass"]["Q"].GetValue<MenuBool>().Enabled && target.IsValidTarget(Q.Range))
                    {
                        Q.Cast(target);
                    }
                }
            }
        }
        
        private static void LaneClear()
        {
            if (Me.ManaPercent > Menu["Viktor_LaneClear"]["Mana"].GetValue<MenuSlider>().Value)
            {
                var mins = GameObjects.EnemyMinions.Where(m => m.IsValidTarget(E2.Range)).ToList();

                if (mins != null)
                {
                    foreach (var min in mins)
                    {
                        if (E.IsReady() && Menu["Viktor_LaneClear"]["E"].GetValue<MenuBool>().Enabled)
                        {
                            var EFarm = E2.GetLineFarmLocation(mins, E.Width);
                            var Pos = Vector2.Zero;
                            if (EFarm.MinionsHit >= Menu["Viktor_LaneClear"]["EMin"].GetValue<MenuSlider>().Value)
                            {
                                if (EFarm.Position.DistanceToPlayer() > E.Range + 100f)
                                {
                                    Pos = EFarm.Position.Extend(EFarm.Position, E.Range);
                                }

                                E.Cast(min.ServerPosition.ToVector2(), Pos);
                                return;
                            }
                        }

                        if (Menu["Viktor_LaneClear"]["Q"].GetValue<MenuBool>().Enabled && Q.IsReady())
                        {
                            Q.Cast(min);
                            return;
                        }
                    }
                }
            }
        }

        private static void JungleClear()
        {
            if (Me.ManaPercent > Menu["Viktor_JungleClear"]["Mana"].GetValue<MenuSlider>().Value)
            {
                var mobs = GetMobs(Me.Position, E.Range);

                if (mobs.Any())
                {
                    foreach (var mob in mobs)
                    {
                        if (E.IsReady() && Menu["Viktor_JungleClear"]["E"].GetValue<MenuBool>().Enabled)
                        {
                            var EFarm = E2.GetLineFarmLocation(mobs, E.Width);
                            var Pos = Vector2.Zero;
                            if (EFarm.MinionsHit >= 1)
                            {
                                if (EFarm.Position.DistanceToPlayer() > E.Range + 100f)
                                {
                                    Pos = EFarm.Position.Extend(EFarm.Position, E.Range);
                                }

                                E.Cast(mob.ServerPosition.ToVector2(), Pos);
                                return;
                            }
                        }

                        if (Q.IsReady() && Menu["Viktor_JungleClear"]["Q"].GetValue<MenuBool>().Enabled)
                        {
                            Q.Cast(mob);
                            return;
                        }
                    }

                }
            }
        }

        private static void KillSteal()
        {
            foreach (var e in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(1200) && !e.IsDead && !e.IsZombie()))
            {
                if (CheckTarget(e))
                {
                    if (E.IsReady() && Menu["Viktor_KillSteal"]["E"].GetValue<MenuBool>().Enabled && e.Health < E.GetDamage(e))
                    {
                        CastE(e);
                        return;
                    }

                    if (Q.IsReady() && Menu["Viktor_KillSteal"]["Q"].GetValue<MenuBool>().Enabled && e.Health < Q.GetDamage(e) && e.IsValidTarget(Q.Range))
                    {
                        Q.Cast(e);
                        return;
                    }
                }
            }
        }
        private static void CastE(AIHeroClient target)
        {
            if (CheckTarget(target))
            {
                var E2Input = new PredictionInput
                {
                    Unit = target,
                    Delay = E.Delay,
                    Radius = E.Width,
                    Range = E2.Range,
                    Speed = E.Speed,
                    Type = (SkillshotType) E.Type,
                    UseBoundingRadius = true
                };

                var FromPos = Vector3.Zero;
                var ToPos = Vector3.Zero;
                var enemies = GetEnemies(E2.Range);

                if (FromPos == Vector3.Zero)
                {
                    foreach (var newtarget in enemies.Where(x => x.NetworkId != target.NetworkId))
                    {
                        if (CheckTarget(newtarget))
                        {
                            E2Input.Unit = newtarget;
                            E2Input.From = target.ServerPosition;

                            var MovePred = Movement.AoePrediction.GetPrediction(E2Input);

                            if (MovePred.Hitchance >= (Movement.HitChance) HitChance.VeryHigh)
                            {
                                ToPos = MovePred.CastPosition;
                            }
                        }
                    }

                    FromPos = target.ServerPosition;
                }

                if (ToPos == Vector3.Zero)
                {
                    ToPos = FromPos.Extend(FromPos, 700f);
                }

                if (FromPos != Vector3.Zero && ToPos != Vector3.Zero)
                {
                    if (FromPos.DistanceToPlayer() > E.Range)
                    {
                        FromPos = Me.ServerPosition.Extend(FromPos, E.Range);
                    }

                    if (ToPos.DistanceToPlayer() > E.Range + 100f)
                    {
                        ToPos = FromPos.Extend(ToPos, E.Range);
                    }

                    E.Cast(FromPos, ToPos);
                }
            }
        }
        private static bool CastWMePos(string SpellName)
        {
            if (SpellName == "AatroxQ")
                return true;

            if (SpellName == "AkaliShadowDance")
                return true;

            if (SpellName == "Headbutt")
                return true;

            if (SpellName == "DianaTeleport")
                return true;

            if (SpellName == "AlZaharNetherGrasp")
                return true;

            if (SpellName == "JaxLeapStrike")
                return true;

            if (SpellName == "KatarinaE")
                return true;

            if (SpellName == "KhazixE")
                return true;

            if (SpellName == "LeonaZenithBlade")
                return true;

            if (SpellName == "MaokaiTrunkLine")
                return true;

            if (SpellName == "MonkeyKingNimbus")
                return true;

            if (SpellName == "PantheonW")
                return true;

            if (SpellName == "PoppyHeroicCharge")
                return true;

            if (SpellName == "ShenShadowDash")
                return true;

            if (SpellName == "SejuaniArcticAssault")
                return true;

            if (SpellName == "RenektonSliceAndDice")
                return true;

            if (SpellName == "Slash")
                return true;

            if (SpellName == "XenZhaoSweep")
                return true;

            if (SpellName == "RocketJump")
                return true;

            return false;
        }

        private static bool CastWTargetPos(string SpellName)
        {
            if (SpellName == "KatarinaR")
                return true;

            if (SpellName == "GalioIdolOfDurand")
                return true;

            if (SpellName == "GragasE")
                return true;

            if (SpellName == "Crowstorm")
                return true;

            if (SpellName == "BandageToss")
                return true;

            if (SpellName == "LissandraE")
                return true;

            if (SpellName == "AbsoluteZero")
                return true;

            if (SpellName == "AlZaharNetherGrasp")
                return true;

            if (SpellName == "FallenOne")
                return true;

            if (SpellName == "PantheonRJump")
                return true;

            if (SpellName == "CaitlynAceintheHole")
                return true;

            if (SpellName == "MissFortuneBulletTime")
                return true;

            if (SpellName == "InfiniteDuress")
                return true;

            if (SpellName == "ThreshQ")
                return true;

            if (SpellName == "RocketGrab")
                return true;

            return false;
        }
    }
}