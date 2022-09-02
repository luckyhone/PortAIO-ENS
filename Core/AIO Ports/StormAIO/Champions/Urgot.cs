using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using StormAIO.utilities;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace StormAIO.Champions
{
    internal class Urgot
    {
        #region Basics

        private static Spell Q, W, E, R;
        private static Menu ChampMenu;
        private static AIHeroClient Player => ObjectManager.Player;

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
                ComboMenu.RBool,
                ComboMenu.RSimi
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
                KillStealMenu.EBool,
                KillStealMenu.RBool
            };

            var laneClearMenu = new Menu("LaneClear", "Lane Clear")
            {
                LaneClearMenu.QSliderBool,
                LaneClearMenu.WSliderBool,
                LaneClearMenu.ESliderBool,
                new Menu("customization", "Customization")
                {
                    LaneClearMenu.QCountSliderBool
                }
            };

            var jungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                JungleClearMenu.QSliderBool,
                JungleClearMenu.WSliderBool,
                JungleClearMenu.ESliderBool
            };

            var lastHitMenu = new Menu("lastHit", "Last Hit")
            {
                LastHitMenu.QSliderBool,
                LastHitMenu.WSliderBool,
                LastHitMenu.ESliderBool
            };

            var drawingMenu = new Menu("Drawing", "Drawing")
            {
                DrawingMenu.DrawQ,
                DrawingMenu.DrawE,
                DrawingMenu.DrawR
            };
            var StructureMenu = new Menu("StructureClear", "Structure Clear")
            {
                StructureClearMenu.WSliderBool
            };
            var menuList = new[]
            {
                comboMenu,
                harassMenu,
                killStealMenu,
                laneClearMenu,
                jungleClearMenu,
                StructureMenu,
                lastHitMenu,
                drawingMenu
            };

            foreach (var menu in menuList) ChampMenu.Add(menu);
            MainMenu.Main_Menu.Add(ChampMenu);
        }

        #endregion

        #region MenuHelper

        public static class ComboMenu
        {
            public static readonly MenuBool QBool = new MenuBool("comboQ", "Use Q");
            public static readonly MenuBool WBool = new MenuBool("comboW", "Use W");
            public static readonly MenuBool EBool = new MenuBool("comboE", "Use E");
            public static readonly MenuBool RBool = new MenuBool("comboR", "Use R");

            public static readonly MenuKeyBind RSimi = new MenuKeyBind("comboRSimi",
                "Simi R Key || Seleceted Target Only", Keys.T, KeyBindType.Press);
        }

        public static class HarassMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("harassQ", "Use Q | If Mana >= x%", 50);

            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("harassW", "Use W | If Mana >= x%", 50);

            public static readonly MenuSliderButton ESliderBool =
                new MenuSliderButton("harassE", "Use E | If Mana >= x%", 50);
        }

        public static class KillStealMenu
        {
            public static readonly MenuBool QBool = new MenuBool("killStealQ", "Use Q");
            public static readonly MenuBool EBool = new MenuBool("killStealE", "Use E");
            public static readonly MenuBool RBool = new MenuBool("killStealR", "Use R");
        }

        public static class JungleClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("jungleClearQ", "Use Q | If Mana >= x%", 50);

            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("jungleClearW", "Use W | If Mana >= x%", 50);

            public static readonly MenuSliderButton ESliderBool =
                new MenuSliderButton("jungleClearE", "Use E | If Mana >= x%", 50);
        }

        public static class LaneClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("laneClearQ", "Use Q | If Mana >= x%", 50);

            public static readonly MenuSlider QCountSliderBool =
                new MenuSlider("laneClearQCount", "Use Q if hittable minions >= x", 3, 1, 5);

            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("laneClearW", "Use W | If Mana >= x%", 50);

            public static readonly MenuSlider WCountSliderBool =
                new MenuSlider("laneClearWCount", "Use W if hittable minions >= x", 3, 1, 5);

            public static readonly MenuSliderButton ESliderBool =
                new MenuSliderButton("laneClearE", "Use E | If Mana >= x%", 50);

            public static readonly MenuSlider ECountSliderBool =
                new MenuSlider("laneClearECount", "Use E if hittable minions >= x", 3, 1, 5);
        }

        public static class StructureClearMenu
        {
            public static readonly MenuSliderButton WSliderBool =
                new MenuSliderButton("structClearW", "Use W | If Mana >= x%", 50);
        }


        public static class LastHitMenu
        {
            public static readonly MenuBool QSliderBool = new MenuBool("lastHitQ", "Use Q");
            public static readonly MenuBool WSliderBool = new MenuBool("lastHitW", "Use W");
            public static readonly MenuBool ESliderBool = new MenuBool("lastHitE", "Use E");
        }

        public static class DrawingMenu
        {
            public static readonly MenuBool DrawQ = new MenuBool("DrawQ", "Draw Q");
            public static readonly MenuBool DrawE = new MenuBool("DrawE", "Draw E");
            public static readonly MenuBool DrawR = new MenuBool("DrawR", "Draw R");
        }

        #endregion

        #region Spells

        private static void InitSpell()
        {
            Q = new Spell(SpellSlot.Q, 800f);
            Q.SetSkillshot(0.25f, 210f, float.MaxValue, false, SpellType.Circle);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 450f);
            E.SetSkillshot(0.45f, 100f, 1200f, false, SpellType.Line);
            R = new Spell(SpellSlot.R, 2500f);
            R.SetSkillshot(0.5f, 160f, 3200, false, SpellType.Line);
        }

        #endregion

        #region Gamestart

        public Urgot()
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
            // ReSharper disable once ObjectCreationAsStatement
            new DrawText("Simi R Key", ComboMenu.RSimi.Key.ToString(), ComboMenu.RSimi, Color.GreenYellow, Color.Red,
                123, 132);
            Orbwalker.OnBeforeAttack += OnBeforeAttack;
            Orbwalker.OnAfterAttack += OnAfterAttack;
        }

        private void OnAfterAttack(object sender, AfterAttackEventArgs args)
        {
            if(args.Target is AITurretClient && !WActive &&
               StructureClearMenu.WSliderBool.Enabled &&
               Player.ManaPercent > StructureClearMenu.WSliderBool.ActiveValue) W.Cast();
        }

        private void OnBeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if(WActive) args.Process = false;
        }
        

        #endregion

        #region args

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawingMenu.DrawQ.Enabled && Q.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, Q.Range, Color.Violet);
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

            if (ComboMenu.RSimi.Active) CastR2();
            RHelper();
            KillSteal();
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            var Rmana = R.IsReady() ? 100 : 0;
            if (ComboMenu.QBool.Enabled && Player.Mana > Rmana) CastQ();
            if (ComboMenu.WBool.Enabled && Player.Mana > Rmana) CastW();
            if (ComboMenu.EBool.Enabled && Player.Mana > Rmana) CastE();
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
            if (Q.IsReady() && LaneClearMenu.QSliderBool.Enabled &&
                Player.ManaPercent > LaneClearMenu.QSliderBool.ActiveValue)
            {
                var minions = GameObjects.GetMinions(Player.Position, Q.Range);
                if (!minions.Any()) return;
                var Qfarm = Q.GetCircularFarmLocation(minions, Q.Width / 2);
                if (Qfarm.MinionsHit >= LaneClearMenu.QCountSliderBool.Value) Q.Cast(Qfarm.Position);
            }

            if (W.IsReady() && LaneClearMenu.WSliderBool.Enabled &&
                Player.ManaPercent > LaneClearMenu.WSliderBool.ActiveValue)
            {
                if (WActive) return;
                var minions = GameObjects.GetMinions(Player.Position, Player.GetRealAutoAttackRange()).FirstOrDefault();
                if (minions == null) return;
                W.Cast();
            }
        }

        private static void JungleClear()
        {
            if (Q.IsReady() && JungleClearMenu.QSliderBool.Enabled &&
                Player.ManaPercent > JungleClearMenu.QSliderBool.ActiveValue)
            {
                var jungels = GameObjects.GetJungles(Player.Position, Q.Range).OrderBy(x => x.DistanceToPlayer())
                    .FirstOrDefault();
                if (jungels == null) return;
                Q.Cast(jungels);
            }

            if (W.IsReady() && JungleClearMenu.WSliderBool.Enabled &&
                Player.ManaPercent > JungleClearMenu.WSliderBool.ActiveValue)
            {
                if (WActive) return;
                var jungels = GameObjects.GetJungles(Player.Position, Player.GetRealAutoAttackRange()).FirstOrDefault();
                if (jungels == null) return;
                W.Cast();
            }

            if (E.IsReady() && JungleClearMenu.ESliderBool.Enabled &&
                Player.ManaPercent > JungleClearMenu.ESliderBool.ActiveValue)
            {
                var jungels = GameObjects.GetJungles(Player.Position, E.Range).OrderBy(x => x.DistanceToPlayer())
                    .FirstOrDefault();
                if (jungels == null) return;
                E.Cast(jungels);
            }
        }

        private static void LastHit()
        {
        }

        private static void KillSteal()
        {
            var qtarget = TargetSelector.GetTarget(Q.Range,DamageType.Physical);
            if (KillStealMenu.QBool.Enabled && qtarget != null)
                if (qtarget.TrueHealth() < Q.GetDamage(qtarget))
                {
                    if (!Q.IsReady()) return;
                    var input = new PredictionInput
                    {
                        Unit = qtarget,
                        Radius = Q.Width / 2,
                        Speed = Q.Speed,
                        Range = 800,
                        Delay = 0.5f,
                        Aoe = false,
                        AddHitBox = false,
                        From = Player.PreviousPosition,
                        RangeCheckFrom = Player.PreviousPosition,
                        Type = SpellType.Circle,
                        CollisionObjects = new[] { CollisionObjects.YasuoWall }
                    };
                    var qpre = Prediction.GetPrediction(input);
                    if (qpre.Hitchance >= HitChance.High) Q.Cast(qpre.CastPosition);
                }

            var Rtarget = TargetSelector.GetTarget(R.Range,DamageType.Physical);
            if (KillStealMenu.RBool.Enabled && Rtarget != null)
                if (Rtarget.TrueHealth() < R.GetDamage(Rtarget))
                {
                    if (!R.IsReady()) return;
                    var input = new PredictionInput
                    {
                        Unit = Rtarget,
                        Radius = R.Width,
                        Speed = R.Speed,
                        Range = R.Range,
                        Delay = R.Delay,
                        Aoe = false,
                        AddHitBox = false,
                        From = Player.PreviousPosition,
                        RangeCheckFrom = Player.PreviousPosition,
                        Type = SpellType.Circle,
                        CollisionObjects = new[] { CollisionObjects.YasuoWall | CollisionObjects.Heroes }
                    };
                    var Rpre = Prediction.GetPrediction(input);
                    if (Rpre.Hitchance >= HitChance.High) Q.Cast(Rpre.CastPosition);
                }

            var Etarget = TargetSelector.GetTarget(E.Range,DamageType.Physical);
            if (KillStealMenu.EBool.Enabled && Etarget != null)
                if (Etarget.TrueHealth() < E.GetDamage(Etarget))
                {
                    if (!E.IsReady()) return;
                    var epre = E.GetPrediction(Etarget);
                    if (epre.Hitchance >= HitChance.High && epre.Hitchance != HitChance.OutOfRange)
                        E.Cast(epre.CastPosition);
                }
        }

        #endregion

        #region Spell Stage

        #endregion


        private static void CastQ()
        {
            if (!Q.IsReady()) return;
            var target = TargetSelector.GetTarget(Q.Range,DamageType.Physical);
            if (target == null) return;
            var input = new PredictionInput
            {
                Unit = target,
                Radius = Q.Width / 2,
                Speed = Q.Speed,
                Range = 800,
                Delay = 0.5f,
                Aoe = false,
                AddHitBox = false,
                From = Player.PreviousPosition,
                RangeCheckFrom = Player.PreviousPosition,
                Type = SpellType.Circle,
                CollisionObjects = new[] { CollisionObjects.YasuoWall }
            };
            var qpre = Prediction.GetPrediction(input);
            if (qpre.Hitchance >= HitChance.High) Q.Cast(qpre.CastPosition);
        }

        private static void CastW()
        {
            if (!W.IsReady() || WActive) return;
            var target = TargetSelector.GetTarget(300,DamageType.Physical);
            if (target == null || E.IsReady()) return;
            W.Cast();
        }

        private static void CastE()
        {
            if (!E.IsReady()) return;
            var target = TargetSelector.GetTarget(E.Range,DamageType.Physical);
            if (target == null) return;
            var epre = E.GetPrediction(target);
            if (epre.Hitchance >= HitChance.High && epre.Hitchance != HitChance.OutOfRange) E.Cast(epre.CastPosition);
        }

        private static void CastR()
        {
            if (!R.IsReady()) return;
            var target = TargetSelector.GetTarget(R.Range,DamageType.Physical);
            if (target == null) return;
            var input = new PredictionInput
            {
                Unit = target,
                Radius = R.Width,
                Speed = R.Speed,
                Range = R.Range,
                Delay = R.Delay,
                Aoe = false,
                AddHitBox = false,
                From = Player.PreviousPosition,
                RangeCheckFrom = Player.PreviousPosition,
                Type = SpellType.Circle,
                CollisionObjects = new[] { CollisionObjects.YasuoWall | CollisionObjects.Heroes }
            };
            var rpre = Prediction.GetPrediction(input);
            if (rpre.Hitchance >= HitChance.High && !target.HasBuffOfType(BuffType.SpellShield) &&
                target.HealthPercent < 25) R.Cast(rpre.CastPosition);
        }

        private static void CastR2()
        {
            if (!R.IsReady()) return;
            var target = TargetSelector.SelectedTarget;
            if (target == null) return;
            if (target.DistanceToPlayer() > R.Range) return;
            var input = new PredictionInput
            {
                Unit = target,
                Radius = R.Width,
                Speed = R.Speed,
                Range = R.Range,
                Delay = R.Delay,
                Aoe = false,
                AddHitBox = false,
                From = Player.PreviousPosition,
                RangeCheckFrom = Player.PreviousPosition,
                Type = SpellType.Circle,
                CollisionObjects = new[] { CollisionObjects.YasuoWall | CollisionObjects.Heroes }
            };
            var rpre = Prediction.GetPrediction(input);
            if (rpre.Hitchance >= HitChance.High && !target.HasBuffOfType(BuffType.SpellShield))
                R.Cast(rpre.CastPosition);
        }

        #region damage

        // Use it if some some damages aren't available by the sdk 
        private static float Wdmg(AIBaseClient t)
        {
            var wDMG = 12 + (0.20 + 4 * (W.Level - 1)) * Player.FlatPhysicalDamageMod * 3 * 1;

            return (float) Player.CalculateDamage(t, DamageType.Physical, wDMG);
        }

        private enum Rstage
        {
            Cast,
            Recast
        }

        private static Rstage _Rstage => R.Name.ToLower() == "urgotrrecast" ? Rstage.Recast : Rstage.Cast;

        #endregion

        #region Extra functions

        private static float AllDamage(AIHeroClient target)
        {
            float Damage = 0;
            if (target == null) return 0;
            if (target.HasBuffOfType(BuffType.Invulnerability)) return 0;
            Damage += (float) Player.GetAutoAttackDamage(target);
            if (Q.IsReady()) Damage += Q.GetDamage(target);
            if (W.IsReady()) Damage += Wdmg(target);
            if (E.IsReady()) Damage += E.GetDamage(target);
            if (R.IsReady()) Damage += R.GetDamage(target);
            if (Player.GetBuffCount("itemmagicshankcharge") == 100)
                Damage += (float) Player.CalculateMagicDamage(target, 100 + 0.1 * Player.TotalMagicalDamage);
            if (Helper.Ignite) Damage += (float) Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return Damage;
        }

        private static void RHelper()
        {
            var target = GameObjects.EnemyHeroes.FirstOrDefault(x => x.HasBuff("Urgot") && x.HealthPercent < 25);
            if (target == null) return;
            if (_Rstage == Rstage.Recast) R.Cast();
        }

        private static bool WActive => W.Name.ToLower() == "urgotwcancel";

        #endregion
    }
}