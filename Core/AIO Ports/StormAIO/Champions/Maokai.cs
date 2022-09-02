using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using Entropy.AIO.Bases;
using SharpDX;
using StormAIO.utilities;
using Color = System.Drawing.Color;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace StormAIO.Champions
{
    internal class Maokai
    {
        #region Basics

        private static Spell Q, W, E, R;
        private static Menu ChampMenu;
        private static AIHeroClient Player => ObjectManager.Player;
        private static Vector3 InsecPos = Vector3.Zero;

        // ReSharper disable once NotAccessedField.Local
        private static Vector3 startPos = Vector3.Zero;

        // ReSharper disable once NotAccessedField.Local
        private static Vector3 endPos = Vector3.Zero;

        #endregion

        #region Menu

        private static void CreateMenu()
        {
            ChampMenu = new Menu(Player.CharacterName, Player.CharacterName);

            var comboMenu = new Menu("combo", "Combo")
            {
                ComboMenu.QBool,
                ComboMenu.WBool,
                ComboMenu.EBool,
                ComboMenu.EUsage,
                ComboMenu.RBool
            };

            var harassMenu = new Menu("harass", "Harass")
            {
                HarassMenu.QSliderBool,
                HarassMenu.WSliderBool,
                HarassMenu.ESliderBool
            };

            var killStealMenu = new Menu("killSteal", "KillSteal")
            {
                KillStealMenu.QBool,
                KillStealMenu.WBool,
                KillStealMenu.EBool
            };

            var laneClearMenu = new Menu("LaneClear", "Lane Clear")
            {
                LaneClearMenu.QSliderBool,
                LaneClearMenu.ESliderBool,
                new Menu("customization", "Customization")
                {
                    LaneClearMenu.QCountSliderBool,
                    LaneClearMenu.ECountSliderBool
                }
            };

            var jungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                JungleClearMenu.QSliderBool,
                JungleClearMenu.WSliderBool,
                JungleClearMenu.ESliderBool
            };

            var MiscHMenu = new Menu("MiscHMenu", "Misc")
            {
                MiscMenu.WGapClose,
                MiscMenu.Wdash
            };

            var drawingMenu = new Menu("Drawing", "Drawing")
            {
                DrawingMenu.DrawQ,
                DrawingMenu.DrawW,
                DrawingMenu.DrawE,
                DrawingMenu.DrawR
            };
            var menuList = new[]
            {
                comboMenu,
                harassMenu,
                killStealMenu,
                laneClearMenu,
                jungleClearMenu,
                MiscHMenu,
                drawingMenu
            };

            foreach (var menu in menuList) ChampMenu.Add(menu);
            MainMenu.Main_Menu.Add(ChampMenu);
        }

        #endregion

        #region MenuHelper

        private static class ComboMenu
        {
            public static readonly MenuBool QBool = new MenuBool("comboQ", "Use Q");
            public static readonly MenuBool WBool = new MenuBool("comboW", "Use W");
            public static readonly MenuBool EBool = new MenuBool("comboE", "Use E");
            public static readonly MenuBool EUsage = new MenuBool("comboEUsage", "Use E ^ use only cc target", false);
            public static readonly MenuBool RBool = new MenuBool("comboR", "Use R");
        }

        private static class HarassMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("harassQ", "Use Q | If Mana >= x%", 50);

            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("harassW", "Use W | If Mana >= x%", 50);

            public static readonly MenuSliderButton ESliderBool =
                new MenuSliderButton("harassE", "Use E | If Mana >= x%", 50);
        }

        private static class KillStealMenu
        {
            public static readonly MenuBool QBool = new MenuBool("killStealQ", "Use Q");
            public static readonly MenuBool WBool = new MenuBool("killStealW", "Use W");
            public static readonly MenuBool EBool = new MenuBool("killStealE", "Use E");
        }

        private static class JungleClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("jungleClearQ", "Use Q | If Mana >= x%", 50);

            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("jungleClearW", "Use W | If Mana >= x%", 50);

            public static readonly MenuSliderButton ESliderBool =
                new MenuSliderButton("jungleClearE", "Use E | If Mana >= x%", 50);
        }

        private static class LaneClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("laneClearQ", "Use Q | If Mana >= x%", 50);

            public static readonly MenuSlider QCountSliderBool =
                new MenuSlider("laneClearQCount", "Use Q if hittable minions >= x", 3, 1, 5);


            public static readonly MenuSliderButton ESliderBool =
                new MenuSliderButton("laneClearE", "Use E | If Mana >= x%", 50);

            public static readonly MenuSlider ECountSliderBool =
                new MenuSlider("laneClearECount", "Use E if hittable minions >= x", 3, 1, 5);
        }

        private static class MiscMenu
        {
            public static readonly MenuBool WGapClose =
                new MenuBool("WGapClose", "Use W on Gapcloser");

            public static readonly MenuBool Wdash =
                new MenuBool("Wdash", "Use W on Dasher");

            public static readonly MenuKeyBind inseckey =
                new MenuKeyBind("inseckey", "inseckey", Keys.T, KeyBindType.Press);
        }


        private static class DrawingMenu
        {
            public static readonly MenuBool DrawQ = new MenuBool("DrawQ", "Draw Q");
            public static readonly MenuBool DrawW = new MenuBool("DrawW", "Draw W");
            public static readonly MenuBool DrawE = new MenuBool("DrawE", "Draw E");
            public static readonly MenuBool DrawR = new MenuBool("DrawR", "Draw R");
        }

        #endregion

        #region Spells

        private static void InitSpell()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 525);
            E = new Spell(SpellSlot.E, 1100f);
            R = new Spell(SpellSlot.R, 3000f);

            Q.SetSkillshot(0.35f, 300f, 1800f, false, SpellType.Line);
            E.SetSkillshot(1.5f, 100f, 1400f, false, SpellType.Circle);

            R = new Spell(SpellSlot.R);
            R.SetSkillshot(0.5f, 1500f, 500f, false, SpellType.Line);
        }

        #endregion

        #region Gamestart

        public Maokai()
        {
            InitSpell();
            CreateMenu();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += delegate
            {
                var t = TargetSelector.GetTarget(2000f,DamageType.Physical);
                if (!Helper.drawIndicator || t == null) return;
                Helper.Indicator(AllDamage(t));
            };
            AIBaseClient.OnDoCast += AIBaseClientOnOnProcessSpellCast;
            AntiGapcloser.OnGapcloser += GapcloserOnOnGapcloser;
            Dash.OnDash += DashOnOnDash;
        }

        private void DashOnOnDash(AIBaseClient sender, Dash.DashArgs args)
        {
            if (!W.IsReady() ||
                Player.IsDead || !MiscMenu.Wdash.Enabled)
                return;


            if (sender == null || !sender.IsValid || !sender.IsEnemy || !sender.IsMelee) return;

            W.CastOnUnit(sender);
        }

        private void GapcloserOnOnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (!W.IsReady() ||
                Player.IsDead || !MiscMenu.WGapClose.Enabled)
                return;


            if (sender == null || !sender.IsValid || !sender.IsEnemy || !sender.IsMelee) return;

            W.CastOnUnit(sender);
        }

        private void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;

            if (args.Slot == SpellSlot.W)
                InsecPos =
                    GameObjects.AllyHeroes.FirstOrDefault(x => x.IsValidTarget(1500) &&
                                                               !x.IsMe) !=
                    null
                        // ReSharper disable once PossibleNullReferenceException
                        ? GameObjects.AllyHeroes.FirstOrDefault(x => x.IsValidTarget(1500) &&
                                                                     !x.IsMe && x != null).Position
                        : GameObjects.AllyTurrets.Where(x =>
                              x.IsValid &&
                              x.DistanceToPlayer() <= 1500
                          ).OrderBy(x => x.DistanceToPlayer()).FirstOrDefault() !=
                          null
                            // ReSharper disable once PossibleNullReferenceException
                            ? GameObjects.AllyTurrets.Where(x =>
                                    x.IsValid &&
                                    x.DistanceToPlayer() <= 1500).OrderBy(x => x.DistanceToPlayer()).FirstOrDefault()
                                .Position
                            : Vector3.Zero;
        }

        #endregion

        #region args
        

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawingMenu.DrawQ.Enabled && Q.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, Q.Range, Color.Violet);
            if (DrawingMenu.DrawW.Enabled && W.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, W.Range, Color.DarkCyan);
            if (DrawingMenu.DrawE.Enabled && E.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, E.Range, Color.DarkCyan);
            if (DrawingMenu.DrawR.Enabled && R.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, R.Range, Color.Violet);
        }

        #endregion

        #region gameupdate

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Helper.Checker()) return;


            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    if (MainMenu.SpellFarm.Active) LaneClear();
                    JungleClear();
                    break;
                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
            }

            KillSteal();
            if (MiscMenu.inseckey.Active) ExecuteInsec();
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            if (ComboMenu.QBool.Enabled) CastQ();
            if (ComboMenu.WBool.Enabled) CastW();
            if (ComboMenu.EBool.Enabled) CastE(ComboMenu.EUsage.Enabled ? 1 : 0);
            if (ComboMenu.RBool.Enabled) CastR();
        }

        private static void Harass()
        {
            if (HarassMenu.QSliderBool.Enabled && Player.ManaPercent > HarassMenu.QSliderBool.ActiveValue) CastQ();
            if (HarassMenu.WSliderBool.Enabled && Player.ManaPercent > HarassMenu.WSliderBool.ActiveValue) CastW();
            if (HarassMenu.ESliderBool.Enabled && Player.ManaPercent > HarassMenu.ESliderBool.ActiveValue) CastE();
        }

        private static void LaneClear()
        {
            if (LaneClearMenu.QSliderBool.Enabled && Player.ManaPercent > LaneClearMenu.QSliderBool.ActiveValue)
            {
                var minions = GameObjects.GetMinions(Player.Position, Q.Range);
                if (minions == null) return;
                var farmline = Q.GetLineFarmLocation(minions, Q.Width);
                if (farmline.MinionsHit >= LaneClearMenu.QCountSliderBool.Value) Q.Cast(farmline.Position);
            }

            if (LaneClearMenu.ESliderBool.Enabled && Player.ManaPercent > LaneClearMenu.ESliderBool.ActiveValue)
            {
                var minions = GameObjects.GetMinions(Player.Position, E.Range);
                if (minions == null) return;
                var farmline = Q.GetCircularFarmLocation(minions, E.Width);
                if (farmline.MinionsHit >= LaneClearMenu.QCountSliderBool.Value) E.Cast(farmline.Position);
            }
        }

        private static void JungleClear()
        {
            if (JungleClearMenu.QSliderBool.Enabled && Player.ManaPercent > JungleClearMenu.QSliderBool.ActiveValue)
            {
                var Jungle = GameObjects.GetJungles(Player.Position, Q.Range).OrderBy(x => x.DistanceToPlayer())
                    .FirstOrDefault();
                if (Jungle == null) return;
                Q.Cast(Jungle);
            }

            if (JungleClearMenu.WSliderBool.Enabled && Player.ManaPercent > JungleClearMenu.WSliderBool.ActiveValue)
            {
                var Jungle = GameObjects.GetJungles(Player.Position, W.Range).OrderBy(x => x.DistanceToPlayer())
                    .FirstOrDefault();
                if (Jungle == null) return;
                W.CastOnUnit(Jungle);
            }

            if (JungleClearMenu.ESliderBool.Enabled && Player.ManaPercent > JungleClearMenu.ESliderBool.ActiveValue)
            {
                var Jungle = GameObjects.GetJungles(Player.Position, E.Range).OrderBy(x => x.DistanceToPlayer())
                    .FirstOrDefault();
                if (Jungle == null) return;
                E.Cast(Jungle);
            }
        }

        private static void LastHit()
        {
        }

        private static void KillSteal()
        {
            if (KillStealMenu.QBool.Enabled)
            {
                var target = Q.GetTarget();
                if (target == null) return;
                if (Q.GetDamage(target) > target.TrueHealth())
                    Q.GetPrediction(target).CastPosition.Extend(Player.Position, -300);
            }

            if (KillStealMenu.WBool.Enabled)
            {
                var target = W.GetTarget();
                if (target == null) return;
                if (W.GetDamage(target) > target.TrueHealth()) W.CastOnUnit(target);
            }

            if (KillStealMenu.EBool.Enabled)
            {
                var target = E.GetTarget();
                if (target == null) return;
                if (E.GetDamage(target) > target.TrueHealth()) E.Cast(E.GetPrediction(target).CastPosition);
            }
        }

        #endregion

        #region Spell Stage

        #endregion

        #region Spell Functions

        private static void CastQ()
        {
            if (!Q.IsReady()) return;

            var target = Q.GetTarget();
            if (target == null) return;

            if (target.DistanceToPlayer() <= 10) return;

            if (Player.IsDashing()) return;

            if (target.DistanceToPlayer() <= 200 && ComboMenu.EBool.Enabled)
            {
                var pos = Q.GetPrediction(target).CastPosition.Extend(Player.Position, -300);
                if (target.HaveImmovableBuff())
                {
                    if (E.Cast(pos)) Q.Cast(pos);
                }
                else
                {
                    if (Q.Cast(pos)) E.Cast(pos);
                }
            }
            else
            {
                Q.Cast(target);
            }
        }

        private static void CastW()
        {
            var target = W.GetTarget();
            if (target == null || !W.IsReady()) return;


            W.CastOnUnit(target);
        }

        private static void CastE(int index = 0)
        {
            var target = E.GetTarget();
            if (target == null || !E.IsReady()) return;

            if (ComboMenu.EUsage.Enabled && index == 1)
            {
                if (!target.HaveImmovableBuff()) return;

                E.Cast(target);
                return;
            }

            E.Cast(E.GetPrediction(target).CastPosition);
        }

        private static void CastR()
        {
            var target = TargetSelector.GetTarget(1000,DamageType.Physical);
            if (target == null || !R.IsReady()) return;
            if (!target.HaveImmovableBuff() || target.HaveSpellShield()) return;
            R.Cast(target);
        }

        #endregion

        #region damage

        // Use it if some some damages aren't available by the sdk 

        #endregion

        #region Extra functions

        private static void ExecuteInsec()
        {
            // if (!ComboMenu.Insec.Enabled)
            // {
            //     return;
            // }
            Orbwalker.Move(Game.CursorPos);

            if (!Q.IsReady() || Player.Mana < 130) return;
            var target = W.GetTarget();
            if (target == null) return;

            W.CastOnUnit(target);
            if (InsecPos == Vector3.Zero) return;

            var pos = target.Position.Extend(InsecPos, 300);

            startPos = target.Position;
            endPos = pos;

            if (target.DistanceToPlayer() <= 200)
            {
                Orbwalker.Move(target.Position.Extend(InsecPos, -300));
                if (!target.IsDashing())
                {
                    E.Cast(pos);
                    DelayAction.Add(500, () => RunInsec(InsecPos));
                }
            }
        }

        private static void RunInsec(Vector3 insecpos)
        {
            Q.Cast(insecpos);
            InsecPos = Vector3.Zero;
            startPos = Vector3.Zero;
            endPos = Vector3.Zero;
        }

        private static float AllDamage(AIHeroClient target)
        {
            float Damage = 0;
            if (target == null) return 0;
            if (target.HasBuffOfType(BuffType.Invulnerability)) return 0;
            Damage += (float) Player.GetAutoAttackDamage(target);
            if (Q.IsReady()) Damage += Q.GetDamage(target);
            if (W.IsReady()) Damage += W.GetDamage(target);
            if (E.IsReady()) Damage += E.GetDamage(target);
            if (R.IsReady()) Damage += R.GetDamage(target);
            if (Player.GetBuffCount("itemmagicshankcharge") == 100)
                Damage += (float) Player.CalculateMagicDamage(target, 100 + 0.1 * Player.TotalMagicalDamage);
            if (Helper.Ignite) Damage += (float) Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return Damage;
        }

        #endregion
    }
}