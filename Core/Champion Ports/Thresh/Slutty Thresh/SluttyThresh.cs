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
using SPrediction;
using Prediction = EnsoulSharp.SDK.Prediction;

namespace Slutty_Thresh
{
    internal class SluttyThresh : MenuConfig
    {
        public const string ChampName = "Thresh";
        private static readonly AIHeroClient Player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        private static SpellSlot Ignite;
        private static SpellSlot FlashSlot;
        public static float FlashRange = 450f;

        public static Dictionary<string, string> channeledSpells = new Dictionary<string, string>();
        private static int elastattempt;
        private static int elastattemptin;

        public static void OnLoad()
        {
            if (Player.CharacterName != ChampName)
                return;

            Q = new Spell(SpellSlot.Q, 1080);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 500);
            R = new Spell(SpellSlot.R, 400);

            Q.SetSkillshot(0.4f, 60f, 1400f, true, SpellType.Line);
            W.SetSkillshot(0.5f, 50f, 2200f, false, SpellType.Circle);

            FlashSlot = Player.GetSpellSlot("SummonerFlash");

            CreateMenuu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Interrupter.OnInterrupterSpell += ThreshInterruptableSpell;
            Dash.OnDash += Unit_OnDash;
            AntiGapcloser.OnGapcloser += OnGapCloser;
            AIHeroClient.OnDoCast += Game_ProcessSpell;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            Orbwalker.AttackEnabled = true;
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;

                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    break;

                case OrbwalkerMode.LastHit:
                    break;

                case OrbwalkerMode.Harass:
                    //   Mixed();
                    break;

                case OrbwalkerMode.None:
                    break;
            }
            AIHeroClient target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (target != null)
            {
                if (target.CharacterName == "Katarina")
                {
                    if (target.HasBuff("katarinaereduction"))
                    {
                        if (target.IsValidTarget(E.Range))
                        {
                            E.Cast(target.Position);
                            eattempt = Environment.TickCount;
                        }
                        if (Environment.TickCount - eattempt >= 90f + Game.Ping
                            && Q.IsReady())
                            Q.Cast(target.Position);
                    }
                }
            }
            if (Config.GetValue<MenuKeyBind>("qflash").Active)
                flashq();

            wcast();
            //  Itemusage();

        }

        private static void wcast()
        {
            if (Player.ManaPercent < Config.GetValue<MenuSlider>("manalant").Value)
                return;
            // AIHeroClient target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (!W.IsReady()) return;
            foreach (var hero in Player.GetAlliesInRange(950).Where(hero =>
                         !hero.IsDead &&
                         hero.HealthPercent <= Config.GetValue<MenuSlider>("hpsettings" + hero.CharacterName).Value &&
                         hero.Distance(Player) <= 900))
                if (Config.GetValue<MenuList>("healop" + hero.CharacterName).Index == 0)
                {
                    if (hero.Distance(Player) <= 900)
                    {
                        W.Cast(hero.Position);
                    }
                }
        }
        
        private static void Combo()
        {
            Ignite = Player.GetSpellSlot("summonerdot");
            var qSpell = comboMenu.GetValue<MenuBool>("useQ").Enabled;
            var q2Spell = comboMenu.GetValue<MenuBool>("useQ1").Enabled;
            var q2Slider = comboMenu.GetValue<MenuSlider>("useQ2").Value;
            var qrange1 = comboMenu.GetValue<MenuSlider>("qrange").Value;
            var rslider = comboMenu.GetValue<MenuSlider>("rslider").Value;
            var rSpell = comboMenu.GetValue<MenuBool>("useR").Enabled;
            var eSpell = comboMenu.GetValue<MenuBool>("useE").Enabled;

           // var wSpell = Config.Item("useW").GetValue<bool>();

            AIHeroClient target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (target == null)
                return;

            if (target.HasBuff("threshQ")
                || (Player.Distance(target) <= 650 && E.IsReady()))
                Orbwalker.AttackEnabled =false;
            else
                Orbwalker.AttackEnabled =true;

            if (target.HasBuff("threshQ"))
            {
                lastbuff = Environment.TickCount;
            }
            if (Q.IsReady()
                && (E.IsReady() || ObjectManager.Player.GetSpell(SpellSlot.E).Cooldown <= 3000f)
                && qSpell
                && !target.HasBuff("threshQ")
                && target.IsValidTarget(Q.Range)
                && target.Distance(Player) >= qrange1)
            {
                Q.Cast(target);
                lastq = Environment.TickCount;
            }

            if (q2Spell
                && target.HasBuff("threshQ"))
            {
                DelayAction.Add(q2Slider, () => Q.Cast());
                
            }

            switch (Config.GetValue<MenuList>("combooptions").Index)
            {
                case 0:
                    if (target.IsValidTarget(E.Range)
                        && eSpell
                        && !target.CanMove
                        && Environment.TickCount - lastq >= 40 + Game.Ping)
                    {
                        E.Cast(target.Position);
                        elastattempt = Environment.TickCount;
                    }
                    break;

                case 1:
                    if (target.IsValidTarget(E.Range)
                        && Environment.TickCount - lastq >= 40 + Game.Ping 
                        && eSpell)
                        E.Cast(target.Position.Extend(Player.Position,
                            Vector3.Distance(target.Position, Player.Position) + 400));
                    elastattemptin = Environment.TickCount;
                    break;
            }

            if (rSpell
                && Player.CountEnemyHeroesInRange(R.Range - 30) >= rslider
                && ((Environment.TickCount - elastattempt > 180f + Game.Ping)
                    || (Environment.TickCount - elastattemptin > 180f + Game.Ping)))
                R.Cast();
        }
        
        private static void flashq()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null)
                return;
            var x = target.Position.Extend(Prediction.GetPrediction(target, 1).UnitPosition, FlashRange);
            switch (Config.GetValue<MenuList>("flashmodes").Index)
            {
                case 0:
                    Player.Spellbook.CastSpell(FlashSlot, x);
                    Q.Cast(x);
                    E.Cast(Player.Position);
                    break;

                /*
            case 1:
            E.Cast(Player.Position);
            Q.Cast(x);
            Player.Spellbook.CastSpell(FlashSlot, x);
                break;
                 */

                case 1:
                    Player.Spellbook.CastSpell(FlashSlot, x);
                    Q.Cast(x);
                    break;
            }
        }
        
        private static void LaneClear()
        {
            var elchSpell = Config.GetValue<MenuBool>("useelch").Enabled;
            //  var elchSlider = Config.Item("elchslider").GetValue<Slider>().Value;
            var minionCount = MinionManager.GetMinions(Player.Position, Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly);

            if (minionCount == null)
                return;
            
            foreach (var minion in minionCount)
            {
                if (elchSpell
                    && minion.IsValidTarget(E.Range)
                    && E.IsReady())
                {
                    E.Cast(minion.Position);
                }
            }
        }


        private static void OnGapCloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (sender.IsAlly
                || sender.IsMe)
                return;

            if (E.IsInRange(args.StartPosition))
                E.Cast(Player.Position.Extend(sender.Position, 400));
        }

        private static void Game_ProcessSpell(AIBaseClient hero, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!hero.IsMe)
                return;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if ((args.SData.Name == "threshqinternal" || args.SData.Name == "ThreshQ")
                && Config.GetValue<MenuBool>("autolantern").Enabled
                && W.IsReady())
            {
                foreach (var heros in
                    GameObjects.AllyHeroes.Where(x => !x.IsMe
                                                  && x.Distance(Player) <= W.Range))
                {
                        W.Cast(heros.Position);
                }
            }
        }

        private static void Unit_OnDash(AIBaseClient sender, Dash.DashArgs args)
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null) return;

            if (!sender.IsEnemy)
                return;

            if (sender.NetworkId == target.NetworkId)
            {
                if (E.IsReady()
                    && E.IsInRange(sender.Position))
                {
                    E.Cast(Player.Position.Extend(sender.Position, 400));
                }
            }
        }

        private static void ThreshInterruptableSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (E.IsReady()
                && E.IsInRange(sender)
                && Config.GetValue<MenuBool>("useE2I").Enabled)
                E.Cast(sender.Position);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;
            if (!Config.GetValue<MenuBool>("Draw").Enabled)
                return;

            var qDraw = Config.GetValue<MenuBool>("qDraw").Enabled;
            var eDraw = Config.GetValue<MenuBool>("eDraw").Enabled;
            var wDraw = Config.GetValue<MenuBool>("wDraw").Enabled;
            var qfDraw = Config.GetValue<MenuBool>("qfDraw").Enabled;

            if (qDraw
                && Q.Level > 0)
                CircleRender.Draw(Player.Position, Q.Range, Color.Green);

            if (qfDraw
                && Q.IsReady()
                && FlashSlot.IsReady())
                CircleRender.Draw(Player.Position, 1440, Color.Red);

            if (wDraw
                && W.Level > 0)
                CircleRender.Draw(Player.Position, W.Range, Color.Black);

            if (eDraw
                && E.Level > 0)
                CircleRender.Draw(Player.Position, E.Range, Color.Gold);

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null)
                return;

            if (Q.IsReady()
                && FlashSlot.IsReady()
                && target.Distance(Player) <= Q.Range + 450
                && target.Distance(Player) >= Q.Range - 200)
            {
                var heroPosition = Drawing.WorldToScreen(Player.Position);
                var textDimension = Drawing.GetTextExtent("Stunnable!");

                Drawing.DrawText(heroPosition.X - textDimension.Width, heroPosition.Y - textDimension.Height,
                    System.Drawing.Color.DarkGreen, "Can Flash Q!");
            }
            else
            {
                var heroPosition = Drawing.WorldToScreen(Player.Position);
                var textDimension = Drawing.GetTextExtent("Stunnable!");
                Drawing.DrawText(heroPosition.X - textDimension.Width, heroPosition.Y - textDimension.Height,
                    System.Drawing.Color.Red, "Can't Flash Q!");
            }
        }
        public static int lastq { get; set; }

        public static int eattempt { get; set; }

        public static int lastbuff { get; set; }
    }
}