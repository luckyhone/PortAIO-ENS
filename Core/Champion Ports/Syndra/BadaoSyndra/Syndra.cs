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

namespace BadaoSeries.Plugin
{
    class SyndraOrbs
    {
        public int Key;
        public GameObject Value;
        public SyndraOrbs(int key, GameObject value)
        {
            Key = key;
            Value = value;
        }
    }
    class LineEQs
    {
        public GameObject Key;
        public Vector2 Value;
        public LineEQs(GameObject key, Vector2 value)
        {
            Key = key;
            Value = value;
        }
    }
    class StunableOrbs
    {
        public AIHeroClient Key;
        public GameObject Value;
        public StunableOrbs(AIHeroClient key, GameObject value)
        {
            Key = key;
            Value = value;
        }
    }

    internal class Syndra : AddUI
    {
        private static int qcount, wcount, ecount, spellcount, waitE, w1cast;
        private static List<SyndraOrbs> SyndraOrb = new List<SyndraOrbs>();

        private static List<AIMinionClient> seed
        {
            get { return ObjectManager.Get<AIMinionClient>().Where(i => i.IsAlly && i.Name == "Seed").ToList(); }
        }

        private static GameObject Wobject()
        {
            return
                ObjectManager.Get<GameObject>().FirstOrDefault(
                    obj => obj.Name.Contains("Syndra_Base_W") && obj.Name.Contains("held") && obj.Name.Contains("02"));
        }
        private static GameObject PickableOrb
        {
            get
            {
                var firstOrDefault = SyndraOrb
                    .FirstOrDefault(x => x.Value.Position.To2D().Distance(Player.Position.ToVector2()) <= 950);
                return firstOrDefault?.Value;
            }
        } 
        private static AIMinionClient PickableMinion
        {
            get
            {
                var firstOrDefault = ObjectManager.Get<AIMinionClient>()
                    .FirstOrDefault(
                        x => x.IsEnemy && x.IsValid && x.Position.To2D().Distance(Player.Position.To2D()) <= 950);
                return firstOrDefault;
            }
        }
        private static List<LineEQs> LineEQ
        {
            get
            {
                if (Wobject() == null)
                {
                    return
                        SyndraOrb.Where(x => x.Value.Position.To2D().Distance(Player.Position.To2D()) <= 700)
                            .Select(
                                x =>
                                    new LineEQs(x.Value,
                                        Player.Position.To2D().Extend(x.Value.Position.To2D(), 1100)))
                            .ToList();
                }
                {
                    return
                        SyndraOrb.Where(
                                x =>
                                    x.Value.Position.To2D().Distance(Wobject().Position.To2D()) >= 20 &&
                                    x.Value.Position.To2D().Distance(Player.Position.To2D()) <= 700)
                            .Select(
                                x =>
                                    new LineEQs(x.Value,
                                        Player.Position.To2D().Extend(x.Value.Position.To2D(), 1100)))
                            .ToList();
                }
            }
        }
        private static List<StunableOrbs> StunAbleOrb
        {
            get
            {
                return (from orb in LineEQ
                    from target in GameObjects.EnemyHeroes.Where(a => a.IsValidTarget())
                    where
                        Prediction.GetPrediction(target, Player.Distance(target) / 1600)
                            .UnitPosition.To2D()
                            .Distance(orb.Key.Position.To2D().Extend(orb.Value, -200), orb.Value, true) <=
                        target.BoundingRadius + 70
                    select new StunableOrbs(target, orb.Key)).ToList();
            }
        }
        private static bool CanEQtarget(AIHeroClient target)
        {
            var pred = E.GetPrediction(target);
            if (pred.Hitchance < HitChance.OutOfRange) return false;
            return Player.Position.To2D().Distance(pred.CastPosition) <= 1200;
        }

        private static Vector2 PositionEQtarget(AIHeroClient target)
        {
            var pred1 = E.GetPrediction(target);
            var pred2 = Q.GetPrediction(target);
            if (pred2.Hitchance >= HitChance.Medium &&
                pred2.UnitPosition.To2D().Distance(Player.Position.To2D()) <= E.Range)
                return pred2.UnitPosition.To2D();
            return pred1.Hitchance >= HitChance.OutOfRange
                ? Player.Position.To2D().Extend(pred1.UnitPosition.To2D(), E.Range)
                : new Vector2();
        }
        private static float Qdamage(AIBaseClient target)
        {
            return (float)Player.CalculateDamage(target, DamageType.Magical,
                (new double[] { 70, 105, 140, 175, 210 }[Q.Level - 1]
                 + 0.65 * Player.FlatMagicDamageMod)
                * ((Q.Level == 5 && target is AIHeroClient) ? 1.15 : 1));
        }
        private static float Wdamage(AIBaseClient target)
        {
            return (float)Player.CalculateDamage(target, DamageType.Magical,
                (new double[] { 70, 110, 150, 190, 230 }[W.Level - 1]
                 + 0.7 * Player.FlatMagicDamageMod));
        }
        private static float Edamage(AIBaseClient target)
        {
            return (float)Player.CalculateDamage(target, DamageType.Magical,
                (new double[] { 85, 130, 175, 220, 265 }[E.Level - 1]
                 + 0.6 * Player.FlatMagicDamageMod));
        }
        private static float Rdamage(AIBaseClient target)
        {
            return (float)Player.CalculateDamage(target, DamageType.Magical,
                new double[] { 90, 140, 190 }[R.Level - 1]
                + 0.2 * Player.FlatMagicDamageMod);
        }
        
        private static float SyndraHalfDamage(AIHeroClient target)
        {
            float x = 0;
            if (Player.Mana > Q.Instance.ManaCost)
            {
                if (Q.IsReady()) x += Qdamage(target);
                if (Player.Mana > Q.Instance.ManaCost)
                {
                    if (Player.Mana > Q.Instance.ManaCost + E.Instance.ManaCost)
                    {
                        if (E.IsReady()) x += Edamage(target);
                        if (Player.Mana > Q.Instance.ManaCost + E.Instance.ManaCost + W.Instance.ManaCost)
                            if (W.IsReady()) x += Wdamage(target);
                    }
                }

            }
            if (LudensEcho.IsReady)
            {
                x = x + (float)Player.CalculateDamage(target, DamageType.Magical, 100 + 0.1 * Player.FlatMagicDamageMod);
            }
            return x;
        }
        private static float SyndraDamage(AIHeroClient target)
        {
            float x = 0;
            if (Player.Mana > Q.Instance.ManaCost)
            {
                if (Q.IsReady()) x += Qdamage(target);
                if (Player.Mana > Q.Instance.ManaCost + R.Instance.ManaCost)
                {
                    if (R.IsReady()) x += Rdamage(target) * (SyndraOrb.Count + 1);
                    if (Player.Mana > Q.Instance.ManaCost + R.Instance.ManaCost + E.Instance.ManaCost)
                    {
                        if (E.IsReady()) x += Edamage(target);
                        if (Player.Mana > Q.Instance.ManaCost + R.Instance.ManaCost + E.Instance.ManaCost + W.Instance.ManaCost)
                            if (W.IsReady()) x += Wdamage(target);
                    }
                }

            }
            if (LudensEcho.IsReady)
            {
                x = x + (float)Player.CalculateDamage(target, DamageType.Magical, 100 + 0.1 * Player.FlatMagicDamageMod);
            }
            x = x + (float)Player.GetAutoAttackDamage(target, true);
            return x;
        }

        public Syndra()
        {
            Q = new Spell(SpellSlot.Q, 950);
            W = new Spell(SpellSlot.W, 1150);
            E = new Spell(SpellSlot.E, 700); //1100
            R = new Spell(SpellSlot.R, 675);
            Q.SetSkillshot(0.5f, 10, float.MaxValue, false,SpellType.Line);
            W.SetSkillshot(0.25f, 10, 1450, false,SpellType.Line,HitChance.Medium, Player.Position, Player.Position);
            E.SetSkillshot(0.5f, 10, 1600, false,SpellType.Line);
            Q.DamageType = W.DamageType = E.DamageType = DamageType.Magical;
            Q.MinHitChance = HitChance.Medium;
            W.MinHitChance = HitChance.Medium;
            
            Menu Combo = new Menu("Combo", "Combo");
            {
                Bool(Combo, "Qc", "Q", true);
                Bool(Combo, "Wc", "W", true);
                Bool(Combo, "Ec", "E", true);
                Bool(Combo, "QEc", "QE", true);
                Bool(Combo, "Rc", "R", true);
                Separator(Combo, "Rbc", "cast R target:");
                foreach (var hero in GameObjects.EnemyHeroes)
                {
                    Bool(Combo, hero.CharacterName + "c", hero.CharacterName, true);
                }
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
            Menu Auto = new Menu("Auto", "Auto");
            {
                Bool(Auto, "Qa", "Q on target AA + spellcast ", true);
                Bool(Auto, "GapIntera", "Anti-Gap & Interrupt", true);
                Bool(Auto, "killsteala", "KillSteal ", true);
                MainMenu.Add(Auto);
            }
            Menu Helper = new Menu("Helper", "Helper");
            {
                Bool(Helper, "enableh", "Enabale", true);
                KeyBind(Helper, "QEh", "QE to mouse", Keys.G, KeyBindType.Press);
                MainMenu.Add(Helper);
            }
            Menu drawMenu = new Menu("Draw", "Draw");
            {
                Bool(drawMenu, "Qd", "Q");
                Bool(drawMenu, "Wd", "W");
                Bool(drawMenu, "Ed", "E");
                Bool(drawMenu, "QEd", "QE");
                Bool(drawMenu, "Rd", "R");
                Bool(drawMenu, "Hpd", "Damage Indicator").ValueChanged += Syndra_ValueChanged;
                MainMenu.Add(drawMenu);
            }
            
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            AIBaseClient.OnProcessSpellCast += OnProcessSpellCast;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
            AntiGapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterrupterSpell += InterruptableSpell_OnInterruptableTarget;
            //Orb.OnAction += Orbwalker_OnAction;
            //LeagueSharp.Common.Utility.HpBar//DamageIndicator.DamageToUnit = SyndraDamage;
            //Custom//DamageIndicator.Initialize(SyndraDamage);
            //LeagueSharp.Common.Utility.HpBar//DamageIndicator.Enabled = drawhp;
            //Custom//DamageIndicator.Enabled = drawhp;
            AIHeroClient.OnLevelUp += Obj_AI_Base_OnLevelUp;
        }

        private void OnUpdate(EventArgs args)
        {
            if (!Enable)
            {
                //LeagueSharp.Common.Utility.HpBar//DamageIndicator.Enabled = false;
                //Custom//DamageIndicator.Enabled = false;
                return;
            }
            helper();
            killsteal();
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                Combo();
            if (Orbwalker.ActiveMode == OrbwalkerMode.Harass && Player.ManaPercent >= harassmana)
                Harass();
        }
        
        public static Vector3 GetGrabableObjectPos(bool onlyOrbs)
        {
            if (onlyOrbs)
                return OrbManager.GetOrbToGrab((int)W.Range);
            foreach (var minion in ObjectManager.Get<AIMinionClient>().Where(minion => minion.IsValidTarget(W.Range)))
                return minion.ServerPosition;
            return OrbManager.GetOrbToGrab((int)W.Range);
        }
        
        public static void UseW(AIBaseClient grabObject, AIBaseClient enemy)
        {
            if (grabObject != null && W.IsReady() && Player.Spellbook.GetSpell(SpellSlot.W).Name == "SyndraW")
            {
                var gObjectPos = GetGrabableObjectPos(false);

                if (gObjectPos.To2D().IsValid() && Environment.TickCount - Q.LastCastAttemptTime > Game.Ping + 150
                                                && Environment.TickCount - E.LastCastAttemptTime > 750 + Game.Ping && Environment.TickCount - W.LastCastAttemptTime > 750 + Game.Ping)
                {
                    var grabsomething = false;
                    if (enemy != null)
                    {
                        var pos2 = W.GetPrediction(enemy, true);
                        if (pos2.Hitchance >= HitChance.High) grabsomething = true;
                    }
                    if (grabsomething || grabObject.IsStunned)
                    {
                        W.Cast(gObjectPos);
                    }

                }
            }
            if (enemy != null && W.IsReady() && Player.Spellbook.GetSpell(SpellSlot.W).Name == "SyndraWCast")
            {
                var pos = W.GetPrediction(enemy, true);
                if (pos.Hitchance >= HitChance.High)
                {
                    W.Cast(pos.CastPosition);
                }
            }
        }
        
        private static void Combo()
        {
            // Use R
            if (R.IsReady() && combor)
            {
                foreach (
                    var target in
                        GameObjects.EnemyHeroes.Where(
                            x =>
                                castRtarget(x) && x.IsValidTarget(W.Range) && !x.IsZombie() && SyndraHalfDamage(x) < x.Health &&
                                SyndraDamage(x) > x.Health))
                {
                    R.Cast(target);
                }

            }

            // final cases;
            //else 
            if (Environment.TickCount > ecount)
            {
                {
                    if (R.IsReady() && E.IsReady() && combor && comboe)
                    {
                        var target =
                            GameObjects.EnemyHeroes.Where(x => castRtarget(x) && x.IsValidTarget() && !x.IsZombie())
                                .OrderByDescending(x => x.Distance(Player.Position))
                                .LastOrDefault();
                        if (target.IsValidTarget(R.Range) && !target.IsZombie())
                        {
                            var count = target.CountEnemyHeroesInRange(400);
                            if (count >= 3)
                            {
                                R.Cast(target);
                                Q.Cast(target);
                                DelayAction.Add(500, () => E.Cast(target.Position));
                                ecount = Environment.TickCount + 510;
                                return;
                            }
                        }
                    }
                }
                {
                    if (Q.IsReady() && comboq)
                    {
                        var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                        if (target.IsValidTarget() && !target.IsZombie())
                        {
                            var x = Q.GetPrediction(target).CastPosition;
                            if (Q.Cast(target) == CastStates.SuccessfullyCasted && E.IsReady()
                                && x.Distance(Player.Position) <= E.Range - 100 && comboe)
                            {
                                DelayAction.Add(250, () => E.Cast(x));
                                ecount = Environment.TickCount + 350;
                            }
                        }
                    }
                    if (E.IsReady() && StunAbleOrb.Any() && Environment.TickCount >= wcount + 500 && comboe)
                    {
                        var targetE = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                        var Orb = StunAbleOrb.Any(x => x.Key == targetE)
                            ? StunAbleOrb.First(x => x.Key == targetE).Value
                            : StunAbleOrb.First().Value;
                        if (Orb != null)
                        {
                            if (E.Cast(Orb.Position.To2D()))
                                ecount = Environment.TickCount + 100;
                        }
                    }

                    if (W.IsReady() && combow)
                    {
                        foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(W.Range + W.Width) && W.GetPrediction(x).Hitchance >= HitChance.High))
                        {
                            UseW(enemy, enemy);
                        }
                    }

                    if (Environment.TickCount > ecount && E.IsReady() && Q.IsReady() &&
                        Environment.TickCount >= wcount + 500 && comboqe &&
                        Player.Mana >= E.Instance.ManaCost + Q.Instance.ManaCost)
                    {
                        var target =
                            GameObjects.EnemyHeroes.FirstOrDefault(
                                x => x.IsValidTarget() && !x.IsZombie() && CanEQtarget(x));
                        if (target.IsValidTarget() && !target.IsZombie())
                        {
                            var pos = PositionEQtarget(target);
                            if (pos.IsValid())
                            {
                                if (Q.Cast(pos))
                                {
                                    if (pos.Distance(Player.Position.To2D()) < E.Range - 200)
                                    {
                                        DelayAction.Add(250, () => E.Cast(pos));
                                        ecount = Environment.TickCount + 350;
                                    }
                                    else
                                    {
                                        DelayAction.Add(150, () => E.Cast(pos));
                                        ecount = Environment.TickCount + 250;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private void helper()
        {
            if (!helperenable) return;
            if (!helperqe) return;
            if (Player.Mana <= Q.Instance.ManaCost + E.Instance.ManaCost || !(Q.IsReady() && E.IsReady())) return;
            Q.Cast(Player.Position.Extend(Game.CursorPos, E.Range - 200));
            DelayAction.Add(250, () => E.Cast(Player.Position.Extend(Game.CursorPos, E.Range - 200)));
        }
        
        private static void killsteal()
        {
            // killstealQ
            if (Q.IsReady() && Environment.TickCount >= spellcount + 1000)
            {
                foreach (
                    var target in
                        GameObjects.EnemyHeroes.Where(
                            x => x.IsValidTarget(Q.Range) && !x.IsZombie() && Qdamage(x) > x.Health))
                {
                    if (Q.Cast(target) == CastStates.SuccessfullyCasted)
                        spellcount = Environment.TickCount;
                }
            }
            // killstealW
            if (W.IsReady() && Environment.TickCount >= spellcount + 1000)
            {
                foreach (
                    var target in
                        GameObjects.EnemyHeroes.Where(
                            x => x.IsValidTarget(W.Range) && !x.IsZombie() && Wdamage(x) > x.Health))
                {
                    if (W.Instance.Name == "SyndraW")
                    {
                        if (PickableOrb != null || PickableMinion != null)
                        {
                            W.Cast(PickableOrb != null
                                ? PickableOrb.Position.To2D()
                                : PickableMinion.Position.To2D());
                        }
                        DelayAction.Add(500, () =>
                        {
                            W.UpdateSourcePosition(Wobject().Position);
                            W.Cast(target);
                        });
                        spellcount = Environment.TickCount + 500;
                    }
                    else
                    {
                        if (Wobject() != null && Environment.TickCount >= w1cast + 500)
                        {
                            W.UpdateSourcePosition(Wobject().Position);
                            if (W.Cast(target) == CastStates.SuccessfullyCasted)
                                spellcount = Environment.TickCount;
                        }
                    }
                }
            }
            //killstealE
            if (E.IsReady() && Environment.TickCount >= spellcount + 1000)
            {
                foreach (
                    var target in
                        GameObjects.EnemyHeroes.Where(
                            x => x.IsValidTarget(E.Range) && !x.IsZombie() && Edamage(x) > x.Health))
                {
                    E.Cast(target.Position);
                    spellcount = Environment.TickCount;
                }
            }
            //killstealQW
            if (Q.IsReady() && W.IsReady() && Environment.TickCount >= spellcount + 1000)
            {
                foreach (
                    var target in
                        GameObjects.EnemyHeroes.Where(
                            x => x.IsValidTarget(Q.Range) && !x.IsZombie() && Qdamage(x) + Wdamage(x) > x.Health))
                {
                    if (Q.Cast(target) == CastStates.SuccessfullyCasted)
                    {
                        if (W.Instance.Name == "SyndraW")
                        {
                            if (PickableOrb != null || PickableMinion != null)
                            {
                                DelayAction.Add(250, () => W.Cast(PickableOrb != null
                                    ? PickableOrb.Position.To2D()
                                    : PickableMinion.Position.To2D()));
                            }
                            DelayAction.Add(750, () =>
                            {
                                W.UpdateSourcePosition(Wobject().Position);
                                W.Cast(target);
                            });
                            spellcount = Environment.TickCount + 750;
                        }
                        else
                        {
                            if (Wobject() != null && Environment.TickCount >= w1cast + 500)
                            {
                                W.UpdateSourcePosition(Wobject().Position);
                                DelayAction.Add(250, () => W.Cast(target));
                                spellcount = Environment.TickCount + 250;
                            }
                        }
                    }
                }
            }
            //killstealR
            if (R.IsReady() && Environment.TickCount >= spellcount + 1000)
            {
                foreach (
                    var target in
                        GameObjects.EnemyHeroes.Where(
                            x => castRtarget(x) && x.IsValidTarget(W.Range) && !x.IsZombie() && Rdamage(x) * SyndraOrb.Count > x.Health))
                {
                    if (R.Cast(target) == CastStates.SuccessfullyCasted)
                        spellcount = Environment.TickCount;
                }
            }
        }
        
        private static void Harass()
        {
            if (Environment.TickCount > ecount)
            {

                if (Q.IsReady() && harassq)
                {
                    var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                    if (target.IsValidTarget() && !target.IsZombie())
                    {
                        if (Q.Cast(target) == CastStates.SuccessfullyCasted)
                            ecount = Environment.TickCount + 100;
                    }
                }
                if (E.IsReady() && StunAbleOrb.Any() && Environment.TickCount >= wcount + 500 && harassE)
                {
                    var targetE = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                    var Orb = StunAbleOrb.Any(x => x.Key == targetE)
                        ? StunAbleOrb.First(x => x.Key == targetE).Value
                        : StunAbleOrb.First().Value;
                    if (Orb != null)
                    {
                        if (E.Cast(Orb.Position.To2D()))
                            ecount = Environment.TickCount + 100;
                    }
                }
                if (W.Instance.Name != "SyndraW" && harassw)
                {
                    var target = TargetSelector.GetTarget(W.Range, DamageType.Magical);
                    if (target.IsValidTarget() && !target.IsZombie())
                    {
                        if (Wobject() != null && Environment.TickCount >= w1cast + 250)
                        {
                            W.UpdateSourcePosition(Wobject().Position, Player.Position);
                            W.Cast(target);
                        }
                    }
                }
                if (W.IsReady() && Environment.TickCount >= ecount + 500 && harassw)
                {
                    var target = TargetSelector.GetTarget(W.Range, DamageType.Magical);
                    if (target.IsValidTarget() && !target.IsZombie())
                    {
                        if (W.Instance.Name != "SyndraW")
                        {
                            if (Wobject() != null && Environment.TickCount >= w1cast + 250)
                            {
                                W.UpdateSourcePosition(Wobject().Position, Player.Position);
                                W.Cast(target);
                            }
                        }
                        else
                        {

                            if (PickableOrb != null || PickableMinion != null)
                            {
                                if (W.Cast(PickableOrb != null
                                    ? PickableOrb.Position.To2D()
                                    : PickableMinion.Position.To2D()))
                                {
                                    wcount = Environment.TickCount + 100;
                                    ecount = Environment.TickCount + 100;
                                }
                            }
                        }
                    }
                }

            }
        }

        private void Obj_AI_Base_OnLevelUp(AIHeroClient sender, AIHeroClientLevelUpEventArgs args)
        {
            if (!sender.IsMe) return;
            if (Player.Level == 16)
                R = new Spell(SpellSlot.R, 750);
        }

        private void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name.ToLower().Contains("syndraq")) qcount = Environment.TickCount;
                if (args.SData.Name.ToLower() == "syndraw") w1cast = Environment.TickCount;
                if (args.SData.Name.ToLower().Contains("syndrawcast")) wcount = Environment.TickCount;
                if (args.SData.Name.ToLower().Contains("syndrae")) ecount = Environment.TickCount;
                spellcount = Math.Max(qcount, Math.Max(ecount, wcount));
            }
            if (!(Orbwalker.ActiveMode == OrbwalkerMode.Combo || Orbwalker.ActiveMode == OrbwalkerMode.Harass || autoq)) return;
            if (sender is AIHeroClient && sender.IsEnemy &&
                (Orbwalker.IsAutoAttack(sender.CharacterName) || !Orbwalker.CanMove()) &&
                sender.IsValidTarget(Q.Range))
            {
                if (Q.IsReady())
                    Q.Cast(sender);
            }
        }

        private void InterruptableSpell_OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (!Enable) return;
            if (sender.IsEnemy && E.IsReady() && autogapinter)
            {
                if (sender.IsValidTarget(E.Range)) E.Cast(sender.Position);
                if (StunAbleOrb.Any())
                {
                    var i = StunAbleOrb.First(x => x.Key.NetworkId == sender.NetworkId);
                    if (i.Value != null)
                        E.Cast(i.Value.Position.To2D());
                }
            }
        }

        private void Gapcloser_OnGapCloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (!Enable) return;
            if (sender.IsEnemy && E.IsReady() && autogapinter)
            {
                if (sender.IsValidTarget(E.Range)) E.Cast(sender.Position);
                if (StunAbleOrb.Any())
                {
                    var i = StunAbleOrb.First(x => x.Key.NetworkId == sender.NetworkId);
                    if (i.Value != null)
                        E.Cast(i.Value.Position.To2D());
                }
            }
        }

        private void OnDelete(GameObject sender, EventArgs args)
        {
            if (!Enable) return;
            //if (sender.Name.Contains("idle"))
            //Chat.Print(sender.Name + " " + sender.Type);
            if (sender.Name.Contains("Syndra_Base_Q_idle.troy") || sender.Name.Contains("Syndra_Base_Q_Lv5_idle.troy"))
            {
                if (seed.Any(x => x.Position.To2D().Distance(sender.Position.To2D()) <= 20))
                {
                    //foreach (var x in SyndraOrb.Where(x => x.Key == sender.NetworkId))
                    //{
                    //    SyndraOrb.Remove(x);
                    //}
                    SyndraOrb.RemoveAll(x => x.Key == sender.NetworkId);
                }
            }
        }

        private void OnCreate(GameObject sender, EventArgs args)
        {
            if (!Enable) return;
            //Chat.Print(sender.Name + " " + sender.Type);
            if (sender.Name.Contains("Syndra_Base_Q_idle.troy") || sender.Name.Contains("Syndra_Base_Q_Lv5_idle.troy"))
            {
                if (seed.Any(x => x.Position.To2D().Distance(sender.Position.To2D()) <= 20))
                {
                    SyndraOrb.Add(new SyndraOrbs(sender.NetworkId, sender));
                }
            }
        }

        private void OnDraw(EventArgs args)
        {
            if (!Enable) return;
            if (Player.IsDead)
                return;
            if (drawq)
                CircleRender.Draw(Player.Position, Q.Range, SharpDX.Color.Aqua);
            if (draww)
                CircleRender.Draw(Player.Position, W.Range, SharpDX.Color.Aqua);
            if (drawe)
                CircleRender.Draw(Player.Position, E.Range, SharpDX.Color.Aqua);
            if (drawqe)
                CircleRender.Draw(Player.Position, 1100, SharpDX.Color.Aqua);
            if (drawr)
                CircleRender.Draw(Player.Position, R.Range, SharpDX.Color.Aqua);
        }

        private void Syndra_ValueChanged(MenuBool menuitem, EventArgs args)
        {
        }

        private static bool castRtarget(AIHeroClient target)
        {
            return MainMenu[target.CharacterName + "c"].GetValue<MenuBool>().Enabled;
        }
        private static bool comboq { get { return MainMenu["Qc"].GetValue<MenuBool>().Enabled; } }
        private static bool combow { get { return MainMenu["Wc"].GetValue<MenuBool>().Enabled; } }
        private static bool comboe { get { return MainMenu["Ec"].GetValue<MenuBool>().Enabled; } }
        private static bool comboqe { get { return MainMenu["QEc"].GetValue<MenuBool>().Enabled; } }
        private static bool combor { get { return MainMenu["Rc"].GetValue<MenuBool>().Enabled; } }
        private static bool harassE { get { return MainMenu["Eh"].GetValue<MenuBool>().Enabled; } }
        private static bool harassq { get { return MainMenu["Qh"].GetValue<MenuBool>().Enabled; } }
        private static bool harassw { get { return MainMenu["Wh"].GetValue<MenuBool>().Enabled; } }
        private static bool autoq { get { return MainMenu["Qa"].GetValue<MenuBool>().Enabled; } }
        private static bool autogapinter { get { return MainMenu["GapIntera"].GetValue<MenuBool>().Enabled; } }
        private static bool autokillsteal { get { return MainMenu["killsteala"].GetValue<MenuBool>().Enabled; } }
        private static bool helperenable { get { return MainMenu["enableh"].GetValue<MenuBool>().Enabled; } }
        private static bool helperqe { get { return MainMenu["QEh"].GetValue<MenuKeyBind>().Active; } }
        private static bool drawq { get { return MainMenu["Qd"].GetValue<MenuBool>().Enabled; } }
        private static bool draww { get { return MainMenu["Wd"].GetValue<MenuBool>().Enabled; } }
        private static bool drawe { get { return MainMenu["Ed"].GetValue<MenuBool>().Enabled; } }
        private static bool drawqe { get { return MainMenu["QEd"].GetValue<MenuBool>().Enabled; } }
        private static bool drawr { get { return MainMenu["Rd"].GetValue<MenuBool>().Enabled; } }
        private static bool drawhp { get { return MainMenu["Hpd"].GetValue<MenuBool>().Enabled; } }
        private static int harassmana { get { return MainMenu["manah"].GetValue<MenuSlider>().Value; } }
    }
}