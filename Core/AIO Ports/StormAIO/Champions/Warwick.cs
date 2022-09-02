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
    public class Warwick
    {
        #region Basics

        private static Spell _q, _e, _r;
        private static Menu champMenu;
        private static AIHeroClient Player => ObjectManager.Player;

        #endregion

        #region Menu

        private static void CreateMenu()
        {
            champMenu = new Menu("Warwick", "Warwick");
            var qMenu = new Menu("Q", "Q")
            {
                new MenuBool("Q", "Use Q in combo"),
                new MenuBool("QA", "Auto Q on dasher in combo mode only")
            };
            champMenu.Add(qMenu);
            var eMenu = new Menu("E", "E")
            {
                new MenuBool("EC", "Use E in Combo")
            };
            champMenu.Add(eMenu);
            var rMenu = new Menu("R", "R")
            {
                new MenuBool("R", "Use in Combo R"),
                new MenuBool("RTT", "Use R if target is under tower", false),
                new MenuKeyBind("RT", "simi R Key", Keys.T, KeyBindType.Press),
                new MenuSlider("RH", " R || When ur target health is below %", 50)
            };
            champMenu.Add(rMenu);

            var laneClearMenu = new Menu("LaneClear", "LaneClear")
            {
                new MenuBool("Q", "Use Q")
            };
            champMenu.Add(laneClearMenu);
            var JungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("E", "Use auto E")
            };

            champMenu.Add(JungleClearMenu);
            var killSteal = new Menu("KS", "KillSteal")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("R", "Use R", false)
            };
            champMenu.Add(killSteal);
            var drawMenu = new Menu("Drawing", "Draw")
            {
                new MenuBool("DrawQ", "Draw Q"),
                new MenuBool("DrawR", "Draw R")
            };
            champMenu.Add(drawMenu);


            MainMenu.Main_Menu.Add(champMenu);
        }

        #endregion menu


        #region Gamestart

        private static void InitSpell()
        {
            _q = new Spell(SpellSlot.Q, 350);
            _q.SetTargetted(0f, 2000f);
            _e = new Spell(SpellSlot.E, 375) {Delay = 0f};
            _e.SetSkillshot(0f, 225f, 1000f, false, SpellType.Line);
            _r = new Spell(SpellSlot.R);
            _r.SetSkillshot(0.1f, 100f, float.MaxValue, false, SpellType.Line);
        }

        public Warwick()
        {
            InitSpell();
            CreateMenu();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AIBaseClient.OnProcessSpellCast += AIBaseClientOnOnProcessSpellCast;
            Orbwalker.OnAfterAttack += OnAfterAttack;
            Orbwalker.OnBeforeAttack += OnBeforeAttack;
            Dash.OnDash += (sender, args) =>
            {
                if (!champMenu["Q"].GetValue<MenuBool>("QA").Enabled) return;
                if (sender.IsEnemy)
                    CastQ();
            };
            Drawing.OnEndScene += delegate
            {
                var t = TargetSelector.GetTarget(2000f,DamageType.Physical);
                if (!Helper.drawIndicator || t == null) return;
                Helper.Indicator(AllDamage(t));
            };
        }

        private void OnBeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (Player.HasBuff("warwickrsound")) args.Process = false;
        }

        private void OnAfterAttack(object sender, AfterAttackEventArgs args)
        {
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo) CastQ();
            if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
                JungleClear();
        }

        #endregion

        #region args

        private static void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender,
            AIBaseClientProcessSpellCastEventArgs args)
        {
            if (args.Target == null) return;
            if (sender.IsMe && args.Target.IsEnemy && args.Target is AIHeroClient) Game.SendEmote(EmoteId.Laugh);
            if (!args.Target.IsMe) return;
            // Cast E to reduce damage from jgl camps
            if (_EStage == EStage.Recast) return;
            if (sender.IsJungle() && Player.CountEnemyHeroesInRange(1000) == 0 &&
                champMenu["JungleClear"].GetValue<MenuBool>("E").Enabled) _e.Cast();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (champMenu["Drawing"].GetValue<MenuBool>("DrawQ").Enabled && _q.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, _q.Range, Color.DarkCyan);
            if (champMenu["Drawing"].GetValue<MenuBool>("DrawR").Enabled && _r.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, Rrange, Color.Violet);
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
                case OrbwalkerMode.LaneClear:
                if(MainMenu.SpellFarm.Active)    LaneClear();
                    break;
                case OrbwalkerMode.LastHit:
                    break;
            }

            KillSteal();
            if (champMenu["R"].GetValue<MenuKeyBind>("RT").Active) CastR2();
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            if (!Orbwalker.CanAttack()) CastQ();
            CastE();
            if (champMenu["R"].GetValue<MenuBool>("R").Enabled) CastR();
        }

        private static void LaneClear()
        {
            var minons = GameObjects.GetMinions(Player.Position, _q.Range).Where(x => x.Health <= Qdmg(x))
                .OrderByDescending(x => x.MaxHealth).FirstOrDefault();
            if (minons == null || !champMenu["LaneClear"].GetValue<MenuBool>("Q").Enabled) return;
            _q.Cast(minons);
        }

        private static void JungleClear()
        {
            var Jungle = GameObjects.GetJungles(Player.Position, _q.Range).OrderByDescending(x => x.MaxHealth)
                .ThenBy(x => x.DistanceToPlayer()).FirstOrDefault();
            if (Jungle == null || !champMenu["JungleClear"].GetValue<MenuBool>("Q").Enabled) return;
            _q.Cast(Jungle);
        }

        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(3000,DamageType.Physical);
            if (target == null) return;
            if (Qdmg(target) >= target.Health + target.AllShield && champMenu["KS"].GetValue<MenuBool>("Q").Enabled &&
                _q.IsInRange(target)) _q.Cast(target);
            if (Rdmg(target) >= target.Health + target.AllShield && champMenu["KS"].GetValue<MenuBool>("R").Enabled)
            {
                target = TargetSelector.GetTarget(Rrange,DamageType.Physical);
                if (target == null || !_r.IsReady()) return;
                _r.Range = Rrange;
                var rpre = _r.GetPrediction(target);
                if (rpre.Hitchance >= HitChance.Medium) _r.Cast(rpre.CastPosition);
            }
        }

        #endregion

        #region SpellStage

        private enum EStage
        {
            Cast,
            Recast,
            Cooldown
        }

        private static EStage _EStage
        {
            get
            {
                if (!_e.IsReady()) return EStage.Cooldown;
                return !Player.HasBuff("WarwickE") ? EStage.Cast : EStage.Recast;
            }
        }
        #endregion

        #region Spell Functions

        private static void CastQ()
        {
            var target = TargetSelector.GetTarget(_q.Range,DamageType.Physical);
            if (target == null) return;
            if (!_q.IsReady() || !champMenu["Q"].GetValue<MenuBool>("Q").Enabled) return;
            _q.Cast(target);
        }


        private static void CastE()
        {
            var target = TargetSelector.GetTarget(_e.Range,DamageType.Physical);
            if (target == null || !_e.IsInRange(target) || !champMenu["E"].GetValue<MenuBool>("EC").Enabled ||
                target.HaveImmovableBuff()) return;
            if (!_e.IsReady()) return;
            if (target.IsFacing(Player) && _EStage == EStage.Cast)
            {
                _e.Cast();
                return;
            }

            _e.Cast();
        }

        private static void CastR()
        {
            var target = TargetSelector.GetTarget(Rrange,DamageType.Physical);
            if (target == null || !_r.IsReady()) return;
            if (target.IsUnderEnemyTurret() && !champMenu["R"].GetValue<MenuBool>("RTT").Enabled) return;
            var rpre = _r.GetPrediction(target);
            if (rpre.Hitchance >= HitChance.Medium &&
                target.HealthPercent <= champMenu["R"].GetValue<MenuSlider>("RH").Value) _r.Cast(rpre.CastPosition);
        }

        private static void CastR2()
        {
            var target = TargetSelector.GetTarget(Rrange,DamageType.Physical);
            if (target == null || !_r.IsReady()) return;
            var rpre = _r.GetPrediction(target);
            if (rpre.Hitchance >= HitChance.Medium) _r.Cast(rpre.CastPosition);
        }

        #endregion

        #region damage

        private static float passivedmg(AIBaseClient t)
        {
            return (float) Player.CalculateMagicDamage(t, 8 + 2 * Player.Level);
        }

        private static float Qdmg(AIBaseClient t)
        {
            return _q.IsReady() ? _q.GetDamage(t) + passivedmg(t) : 0;
        }

        private static float Rdmg(AIHeroClient t)
        {
            return (float) (_r.IsReady() ? _r.GetDamage(t) + passivedmg(t) * 3 + Player.GetAutoAttackDamage(t) : 0);
        }

        #endregion

        #region Extra functions

        private static float Rrange => (float) (675 >= Player.MoveSpeed * 1.9 ? 675 : Player.MoveSpeed * 1.9);

        private static float AllDamage(AIHeroClient target)
        {
            float Damage = 0;
            if (target == null) return 0;
            if (target.HasBuffOfType(BuffType.Invulnerability)) return 0;
            Damage += (float) Player.GetAutoAttackDamage(target) + passivedmg(target);
            if (_q.IsReady()) Damage += Qdmg(target);
            if (_r.IsReady()) Damage += Rdmg(target);
            if (Player.GetBuffCount("itemmagicshankcharge") == 100)
                Damage += (float) Player.CalculateMagicDamage(target, 100 + 0.1 * Player.TotalMagicalDamage);
            if (Helper.Ignite) Damage += (float) Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return Damage;
        }

        #endregion
    }
}