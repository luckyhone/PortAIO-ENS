using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using Entropy.AIO.Bases;
using StormAIO.utilities;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace StormAIO.Champions
{
    internal class Kowmaw
    {
        #region Basics

        public static readonly float[] RRange = {0f, 1300f, 1550f, 1800f};
        public static readonly float[] WRange = {0f, 630f, 650f, 670f, 690f, 710f};
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
                ComboMenu.EWBool,
                ComboMenu.RBool,
                ComboMenu.RRangeBool,
                ComboMenu.RHpBool,
                ComboMenu.RStacksBool,
                ComboMenu.LethalTempoBool,
                ComboMenu.RSemiAutoKeyBind
            };
            if (GameObjects.AllyHeroes.Any(x => x.CharacterName.Equals("Lulu")))
            {
                Lulu = GameObjects.AllyHeroes.FirstOrDefault(x => x.CharacterName.Equals("Lulu"));
                comboMenu.Add(ComboMenu.Lulu);
            }

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
            var AutomaticMenu = new Menu("AutomaticMenu", "Automatic")
            {
                Kowmaw.AutomaticMenu.EOnCCBool,
                Kowmaw.AutomaticMenu.EOnTPBool,
                Kowmaw.AutomaticMenu.ROnCCBool
            };
            var GapCloserMenu = new Menu("GapCloserMenu", "GapCloser")
            {
                Kowmaw.GapCloserMenu.EBool
            };
            var laneClearMenu = new Menu("LaneClear", "Lane Clear")
            {
                LaneClearMenu.QSliderBool,
                LaneClearMenu.ESliderBool
            };

            var jungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                JungleClearMenu.QSliderBool,
                JungleClearMenu.ESliderBool
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
                GapCloserMenu,
                AutomaticMenu,
                laneClearMenu,
                jungleClearMenu,
                drawingMenu
            };

            foreach (var menu in menuList) ChampMenu.Add(menu);
            MainMenu.Main_Menu.Add(ChampMenu);
        }

        #endregion

        #region MenuHelper

        private static class ComboMenu
        {
            public static readonly MenuBool QBool = new MenuBool("q", "Use Q");
            public static readonly MenuBool WBool = new MenuBool("w", "Use W");

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static readonly MenuBool Lulu = new MenuBool("lulu", " ^ Only W if Lulu's W/E is Ready", false);
            public static readonly MenuBool EBool = new MenuBool("e", "Use E");
            public static readonly MenuBool EWBool = new MenuBool("ew", "^ Don't Use When W Active");
            public static readonly MenuBool RBool = new MenuBool("r", "Use R");
            public static readonly MenuBool RRangeBool = new MenuBool("rrange", "^ Only Outside AA Range");

            public static readonly MenuSliderButton RHpBool =
                new MenuSliderButton("rhp", "^ Only if target HP <= X%", 40);

            public static readonly MenuSliderButton RStacksBool =
                new MenuSliderButton("rstacks", "^ Max Stacks", 3, 1, 9);

            public static readonly MenuBool LethalTempoBool =
                new MenuBool("lethaltempo", "Block Q/E/R Spells if Lethal Tempo is Active");

            public static readonly MenuKeyBind RSemiAutoKeyBind =
                new MenuKeyBind("rsemiautomatic", "^ Semi-R Cast", Keys.R, KeyBindType.Press);
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
            public static readonly MenuBool EBool = new MenuBool("killStealE", "Use E");
            public static readonly MenuBool RBool = new MenuBool("killStealR", "Use R");
        }

        private static class AutomaticMenu
        {
            public static readonly MenuBool EOnCCBool = new MenuBool("eoncc", "Use E on CC'd targets");
            public static readonly MenuBool ROnCCBool = new MenuBool("roncc", "Use R on CC'd targets");
            public static readonly MenuBool EOnTPBool = new MenuBool("eontp", "Use E to canncel Recalls");
        }

        private static class GapCloserMenu
        {
            public static readonly MenuBool EBool = new MenuBool("enabled", "Enabled || Use on gapcloser");
        }

        private static class JungleClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("jungleClearQ", "Use Q | If Mana >= x%", 50);

            public static readonly MenuSliderButton ESliderBool =
                new MenuSliderButton("jungleClearE", "Use E | If Mana >= x%", 50);
        }

        private static class LaneClearMenu
        {
            public static readonly MenuSliderButton QSliderBool =
                new MenuSliderButton("laneClearQ", "Use Q | If Mana >= x%", 50);

            public static readonly MenuSliderButton ESliderBool =
                new MenuSliderButton("laneClearE", "Use E | If Mana >= x%", 50);
        }

        private static class DrawingMenu
        {
            public static readonly MenuBool DrawQ = new MenuBool("DrawQ", "Draw Q", false);
            public static readonly MenuBool DrawW = new MenuBool("DrawW", "Draw W");
            public static readonly MenuBool DrawE = new MenuBool("DrawE", "Draw E", false);
            public static readonly MenuBool DrawR = new MenuBool("DrawR", "Draw R");
        }

        #endregion

        #region Spells

        private static void InitSpell()
        {
            Q = new Spell(SpellSlot.Q, 1175f);
            W = new Spell(SpellSlot.W, 630f);
            E = new Spell(SpellSlot.E, 1280);
            R = new Spell(SpellSlot.R, 1300f);

            Q.SetSkillshot(0.35f, 95f, 1650f, true, SpellType.Line, HitChance.VeryHigh);
            E.SetSkillshot(0.25f, 125f, 1350f, false, SpellType.Line);
            R.SetSkillshot(1.20f, 120f, float.MaxValue, false, SpellType.Circle);
        }

        #endregion

        #region Gamestart

        public Kowmaw()
        {
            InitSpell();
            CreateMenu();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnBeforeAttack += OrbwalkerOnOnAction;
            Drawing.OnEndScene += delegate
            {
                var t = TargetSelector.GetTarget(2000f,DamageType.Physical);
                if (!Helper.drawIndicator || t == null) return;
                Helper.Indicator(AllDamage(t));
            };
            Teleport.OnTeleport += TeleportOnOnTeleport;
            AntiGapcloser.OnGapcloser += GapcloserOnOnGapcloser;
            Game.OnWndProc += GameOnOnWndProc;
            AIHeroClient.OnLevelUp += delegate(AIHeroClient sender, AIHeroClientLevelUpEventArgs args)
            {
                if (sender.IsMe)
                {
                    if (R.Level != 0) R.Range = 1300f + 250 * (R.Level - 1);
                    if (W.Level != 0) W.Range = 630f + 20 * (W.Level - 1) + Player.GetRealAutoAttackRange();
                }
            };
            if (R.Level != 0) R.Range = 1300f + 250 * (R.Level - 1);
            if (W.Level != 0) W.Range = 630f + 20 * (W.Level - 1) + Player.GetRealAutoAttackRange();
            // ReSharper disable once ObjectCreationAsStatement
            new DrawText("Simi R Key", ComboMenu.RSemiAutoKeyBind.Key.ToString(), ComboMenu.RSemiAutoKeyBind,
                Color.GreenYellow, Color.Red, 123, 132);
        }

        private void GameOnOnWndProc(GameWndEventArgs args)
        {
            if (args.WParam == (int) ComboMenu.RSemiAutoKeyBind.Key && R.IsReady())
            {
                args.Process = false;
                ExecuteSemiAutomaticR();
            }
        }


        private void GapcloserOnOnGapcloser(AIHeroClient Sender, AntiGapcloser.GapcloserArgs args)
        {
            if (Player.IsDead || Player.IsMoving) return;
            var sender = Sender;
            if (sender == null || !sender.IsValid || !sender.IsEnemy) return;

            if (E.IsReady() && !sender.HasBuffOfType(BuffType.SpellShield))
            {
                if (!GapCloserMenu.EBool.Enabled) return;

                switch (args.Type)
                {
                    case AntiGapcloser.GapcloserType.Targeted:
                        if (args.StartPosition == Player.Position) E.Cast(args.StartPosition);

                        break;

                    case AntiGapcloser.GapcloserType.SkillShot:
                        if (args.EndPosition.DistanceToPlayer() <= E.Range) E.Cast(args.EndPosition);

                        break;
                }

                E.Cast(sender);
            }
        }

        private void TeleportOnOnTeleport(AIBaseClient Sender, Teleport.TeleportEventArgs args)
        {
            if (!E.IsReady()) return;

            if (!AutomaticMenu.EOnTPBool.Enabled) return;

            if (args.Type != Teleport.TeleportType.Teleport || args.Status != Teleport.TeleportStatus.Start) return;

            var sender = args.Source;

            if (!sender.IsEnemy) return;

            if (sender.DistanceToPlayer() <= E.Range) E.Cast(sender.Position);
        }

        #endregion

        #region args

        private static void OrbwalkerOnOnAction(object sender, BeforeAttackEventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    if (ComboMenu.QBool.Enabled) CastQ();
                    if (ComboMenu.EBool.Enabled) CastE();
                    break;
                case OrbwalkerMode.Harass:
                    if (HarassMenu.QSliderBool.Enabled &&
                        Player.ManaPercent > HarassMenu.QSliderBool.ActiveValue) CastQ();
                    break;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawingMenu.DrawQ.Enabled  && Q.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, Q.Range, Color.Violet);
            if (DrawingMenu.DrawW.Enabled  && W.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, W.Range, Color.DarkCyan);
            if (DrawingMenu.DrawE.Enabled  && E.IsReady())
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
            }

            Auto();
            KillSteal();
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            if (ComboMenu.QBool.Enabled) CastQ();
            if (ComboMenu.WBool.Enabled) CastW();
            if (ComboMenu.EBool.Enabled) CastE();
            if (ComboMenu.RBool.Enabled) CastR();
        }

        private static void Harass()
        {
            if (HarassMenu.QSliderBool.Enabled && Player.ManaPercent > HarassMenu.QSliderBool.ActiveValue) CastQ();
            if (HarassMenu.WSliderBool.Enabled && Player.ManaPercent > HarassMenu.WSliderBool.ActiveValue) CastW();
        }

        private static void LaneClear()
        {
            var Minions = GameObjects.GetMinions(Player.Position, E.Range).Where(x => x.IsEnemy)
                .OrderBy(x => x.DistanceToPlayer())
                .FirstOrDefault();
            if (Minions != null && Q.IsInRange(Minions) && Player.ManaPercent > LaneClearMenu.QSliderBool.ActiveValue &&
                LaneClearMenu.QSliderBool.Enabled)
            {
                if (!Q.IsReady() || Helper.CanAttackAnyMinion) return;
                if (Minions.Health < Q.GetDamage(Minions)) Q.Cast(Minions);
            }

            if (Minions != null && E.IsReady() && Player.ManaPercent > LaneClearMenu.ESliderBool.ActiveValue &&
                LaneClearMenu.ESliderBool.Enabled)
            {
                var allminions = GameObjects.GetMinions(Player.Position, E.Range);
                var eline = E.GetLineFarmLocation(allminions, E.Width);
                if (allminions != null && eline.MinionsHit >= 2) E.Cast(eline.Position);
            }
        }

        private static void JungleClear()
        {
            var jungle = GameObjects.GetJungles(Player.Position, E.Range).OrderBy(x => x.DistanceToPlayer())
                .FirstOrDefault();
            if (jungle != null && Q.IsInRange(jungle) && Player.ManaPercent > JungleClearMenu.QSliderBool.ActiveValue &&
                JungleClearMenu.QSliderBool.Enabled)
            {
                if (!Q.IsReady()) return;
                Q.Cast(jungle);
            }

            if (jungle != null && E.IsReady() && Player.ManaPercent > JungleClearMenu.ESliderBool.ActiveValue &&
                JungleClearMenu.ESliderBool.Enabled) E.Cast(jungle);
        }

        private static void KillSteal()
        {
            var rtarget = R.GetTarget();
            if (rtarget != null && R.IsReady() && KillStealMenu.RBool.Enabled)
                if (rtarget.TrueHealth() < R.GetDamage(rtarget))
                {
                    var rpre = R.GetPrediction(rtarget);
                    if (rpre.Hitchance >= HitChance.High) R.Cast(rpre.CastPosition);
                }

            var qtarget = Q.GetTarget();
            if (qtarget != null && Q.IsReady() && KillStealMenu.QBool.Enabled)
                if (qtarget.TrueHealth() < Q.GetDamage(qtarget))
                    Q.Cast(qtarget);
            var etarget = E.GetTarget();
            if (etarget != null && E.IsReady() && KillStealMenu.EBool.Enabled)
                if (etarget.TrueHealth() < E.GetDamage(qtarget))
                    E.Cast(qtarget);
        }

        private static void Auto()
        {
            if (E.IsReady()) EOnImmobile();
            if (R.IsReady()) ROnImmobile();
        }

        #endregion

        #region Spell Stage

        #endregion

        #region Spell Functions

        private static void CastQ()
        {
            if (!Q.IsReady()) return;

            if (HasLTActive && ComboMenu.LethalTempoBool.Enabled) return;

            var target = TargetSelector.GetTarget(Q.Range,DamageType.Physical);

            if (target == null) return;

            Q.Cast(target);
        }

        private static void CastW()
        {
            if (!W.IsReady()) return;

            if (WaitForLulu()) return;

            var target = TargetSelector.GetTarget(W.Range,DamageType.Physical);

            if (target == null) return;

            if (target.IsValidTarget(W.Range)) W.Cast();
        }

        private static void CastE()
        {
            if (!E.IsReady()) return;

            if (!ComboMenu.EBool.Enabled) return;

            if (ComboMenu.EWBool.Enabled && HasWActive) return;

            if (HasLTActive && ComboMenu.LethalTempoBool.Enabled) return;

            var target = TargetSelector.GetTarget(E.Range,DamageType.Physical);

            if (target == null) return;

            E.Cast(target);
        }

        private static void CastR()
        {
            if (!R.IsReady()) return;

            if (!ComboMenu.RBool.Enabled) return;

            if (GetRStacks > ComboMenu.RStacksBool.Value && ComboMenu.RStacksBool.Enabled) return;

            if (HasLTActive && ComboMenu.LethalTempoBool.Enabled && Helper.CanAttackAnyHero) return;

            var target = TargetSelector.GetTarget(R.Range,DamageType.Physical);

            if (target == null) return;
            if (target.HealthPercent > ComboMenu.RHpBool.ActiveValue && ComboMenu.RHpBool.Enabled) return;
            if (ComboMenu.RRangeBool.Enabled && target.DistanceToPlayer() <= Player.GetRealAutoAttackRange()) return;
        
            var pred = R.GetPrediction(target);
            if (pred.Hitchance < HitChance.Medium) return;

            R.Cast(pred.CastPosition);
        }

        #endregion

        #region Extra functions

        private static void EOnImmobile()
        {
            if (!AutomaticMenu.EOnCCBool.Enabled) return;

            var target = GameObjects.EnemyHeroes.FirstOrDefault(t => t.HaveImmovableBuff() && t.IsValidTarget(E.Range));
            if (target == null) return;

            E.Cast(target.Position);
        }

        private static void ROnImmobile()
        {
            if (!AutomaticMenu.ROnCCBool.Enabled) return;

            var target = GameObjects.EnemyHeroes.FirstOrDefault(t => t.HaveImmovableBuff() && t.IsValidTarget(R.Range));
            if (target == null) return;

            R.Cast(target.Position);
        }

        private static void ExecuteSemiAutomaticR()
        {
            var target = R.GetTarget();
            if (target == null) return;
            var rpre = R.GetPrediction(target);
            if (rpre.Hitchance >= HitChance.High) R.Cast(rpre.CastPosition);
        }

        private static float AllDamage(AIHeroClient target)
        {
            float Damage = 0;
            if (target == null) return 0;
            if (target.HasBuffOfType(BuffType.Invulnerability)) return 0;
            Damage += (float) Player.GetAutoAttackDamage(target) * Helper.CurrentAttackSpeed(0.665);
            if (Q.IsReady()) Damage += Q.GetDamage(target);
            if (W.IsReady()) Damage += W.GetDamage(target);
            if (E.IsReady()) Damage += E.GetDamage(target);
            if (R.IsReady()) Damage += R.GetDamage(target);
            if (Player.GetBuffCount("itemmagicshankcharge") == 100)
                Damage += (float) Player.CalculateMagicDamage(target, 100 + 0.1 * Player.TotalMagicalDamage);
            if (Helper.Ignite) Damage += (float) Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return Damage;
        }

        private static bool HasWActive => Player.HasBuff("KogMawBioArcaneBarrage");

        private static bool HasLTActive =>
            Player.HasBuff("ASSETS/Perks/Styles/Precision/LethalTempo/LethalTempoEmpowered.lua");

        private static int GetRStacks => Player.GetBuffCount("kogmawlivingartillerycost");
        private static AIHeroClient Lulu { get; set; }

        private static bool WaitForLulu()
        {
            if (!ComboMenu.Lulu.Enabled) return false;

            if (Lulu == null) return false;

            if (!Lulu.IsValid) return false;

            if (!Lulu.IsValidTarget(650f)) return false;

            return Lulu.Spellbook.GetSpell(SpellSlot.W).IsReady() || Lulu.Spellbook.GetSpell(SpellSlot.E).IsReady();
        }

        #endregion
    }
}