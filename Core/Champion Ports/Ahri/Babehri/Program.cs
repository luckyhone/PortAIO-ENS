using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using SharpDX;
using SharpDX.Direct3D9;
using SPrediction;
using HealthPrediction = SebbyLib.HealthPrediction;

namespace Babehri
{
    internal class Program
    {
        public static MissileClient AhriQMissile;
        public static EffectEmitter AhriQParticle;
        public static AttackableUnit LastAutoTarget;
        public static Menu Menu,eMisc,rMisc;
        public static Font PassiveText;// = new Render.Text("", 0, 0, 24, Color.White, "Tahoma");
        
        public static GameObject QObject
        {
            get
            {
                if (AhriQMissile != null && AhriQMissile.IsValid && AhriQMissile.IsVisible)
                {
                    return AhriQMissile;
                }

                if (AhriQParticle != null && AhriQParticle.IsValid && AhriQParticle.IsVisibleOnScreen)
                {
                    return AhriQParticle;
                }
                return null;
            }
        }
        
        public static int PassiveStack
        {
            get
            {
                var buff = Player.Buffs.FirstOrDefault(b => b.Name.Equals("ahrisoulcrushercounter"));
                return buff == null ? 0 : buff.Count;
            }
        }

        public static int UltStack
        {
            get
            {
                var buff = Player.Buffs.FirstOrDefault(b => b.Name.Equals("AhriTumble"));
                return buff == null ? 3 : buff.Count;
            }
        }
        
        public static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }


        public static void Game_OnGameLoad()
        {
            PassiveText = new Font(Drawing.Direct3DDevice9, new FontDescription
            {
                FaceName = "Tahoma",
                Height = 24,
                Weight = FontWeight.ExtraBold,
                OutputPrecision = FontPrecision.Default,
                Quality = FontQuality.ClearType,
            });

            if (Player.CharacterName != "Ahri")
            {
                return;
            }
            
            Menu = new Menu("Babehri", "Babehri", true);

            var combo = Menu.Add(new Menu("Combo", "Combo"));
            combo.AddBool("ComboQ", "Use Q");
            combo.AddBool("ComboW", "Use W");
            combo.AddSlider("ComboWMinHit", "Min Fox-Fire Hits", 2, 0, 3);
            combo.AddBool("ComboE", "Use E");

            var harass = Menu.Add(new Menu("Harass", "Harass"));
            harass.AddBool("HarassQ", "Use Q");
            harass.AddBool("HarassW", "Use W");
            harass.AddSlider("HarassWMinHit", "Min Fox-Fire Hits", 2, 0, 3);
            harass.AddBool("HarassE", "Use E");
            harass.AddSlider("HarassMinMana", "Min Mana Percent", 30);
            
            var farm = Menu.Add(new Menu("Farm", "Farm"));
            farm.AddBool("FarmQ", "Smart Farm with Q");
            farm.AddSlider("FarmQHC", "Q Min HitCount", 3, 1, 5);
            farm.AddBool("FarmQLH", "Save Q for LH", false);
            farm.AddBool("FarmW", "Farm W (LC)", false);
            farm.AddSlider("FarmMana", "Minimum Mana %", 50);

            var misc = Menu.Add(new Menu("Misc", "Misc"));
            eMisc = misc.Add(new Menu("E", "E"));
            eMisc.AddBool("GapcloseE", "Use E on Gapclose");
            eMisc.AddBool("InterruptE", "Use E to Interrupt");
            rMisc = misc.Add(new Menu("R", "R"));
            rMisc.AddSlider("DamageR", "R Dmg Prediction Bolt Count", 2, 0, 3);
            rMisc.Add(new MenuKeyBind("FleeR", "Use R Flee", Keys.T, KeyBindType.Press));
            
            var drawing = Menu.Add(new Menu("Drawing", "Drawing"));
            
            var damage = drawing.Add(new Menu("Damage Indicator","Damage Indicator"));
            damage.AddBool("Enabled", "Enabled");
            damage.Add(new MenuColor("HPColor", "Health Color", Color.White));
            damage.Add(new MenuColor("FillColor", "Damage Color", Color.DeepPink));
            damage.AddBool("DmgEnabled", "Killable");
            
            drawing.AddBool("DrawQ","Draw Q");
            drawing.AddBool("DrawW","Draw W",false);
            drawing.AddBool("DrawE","Draw E");
            drawing.AddBool("DrawR","Draw R");

            Menu.Attach();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnAfterAttack += Orbwalking_AfterAttack;
            AIBaseClient.OnDoCast += Obj_AI_Base_OnProcessSpellCast;
            Interrupter.OnInterrupterSpell += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
        }

        private static void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs gapcloser)
        {
            if (eMisc.GetValue<MenuBool>("GapcloseE").Enabled && Player.Distance(gapcloser.EndPosition) < 100 && Spells.E.IsReady())
            {
                Spells.E.Cast(sender);
            }
        }

        private static void Interrupter2_OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (eMisc.GetValue<MenuBool>("InterruptE").Enabled && Spells.E.CanCast(sender))
            {
                Spells.E.Cast(sender);
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            var unit = sender as AIHeroClient;
            var target = args.Target as AttackableUnit;

            if (unit == null || !unit.IsValid || !unit.IsMe || target == null || !target.IsValid)
            {
                return;
            }

            LastAutoTarget = target;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsDashing() || Player.Spellbook.IsAutoAttack || Player.Spellbook.IsCastingSpell)
            {
                return;
            }

            Flee();
            Farm();
            
            var activeMode = Orbwalker.ActiveMode;
            var mode = activeMode.GetModeString();
            var target = TargetSelector.GetTarget(Spells.E.Range, DamageType.Magical);

            if (!activeMode.IsComboMode())
            {
                return;
            }
            
            if (target == null || !target.IsValidTarget(Spells.E.Range))
            {
                return;
            }

            if (mode.Equals("Harass") && Player.ManaPercent < Menu.GetValue<MenuSlider>("HarassMinMana").Value)
            {
                return;
            }
            if (Spells.E.IsActive() && CastE(target))
            {
                return;
            }

            if (Spells.Q.IsActive() && CastQ(target))
            {
                return;
            }

            if (Spells.W.IsActive() && CastW(target)) {}
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var particle = sender as EffectEmitter;
            if (particle != null && particle.IsValid &&
                (particle.Name.Contains("Ahri_Orb") || particle.Name.Contains("Ahri_Passive")))
            {
                AhriQParticle = particle;
                return;
            }

            var missile = sender as MissileClient;
            if (missile == null || !missile.IsValid || !missile.SpellCaster.IsMe)
            {
                return;
            }

            if (missile.SData.Name.Equals("AhriOrbReturn") || missile.SData.Name.Equals("AhriOrbMissile"))
            {
                AhriQMissile = missile;
            }
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (AhriQMissile != null && sender.NetworkId.Equals(AhriQMissile.NetworkId))
            {
                AhriQMissile = null;
            }

            if (AhriQParticle != null && sender.NetworkId.Equals(AhriQParticle.NetworkId))
            {
                AhriQParticle = null;
            }
        }
        
        private static bool CastQ(AIBaseClient target)
        {
            if (!Spells.Q.IsReady() || Player.IsDashing() || !target.IsValidTarget(Spells.Q.Range))
            {
                return false;
            }
            

            return Spells.Q.CanCast(target) && Spells.Q.Cast(target) == CastStates.SuccessfullyCasted;
        }
        
        private static bool CastW(AttackableUnit target)
        {
            var count = CountWHits(target);
            var hitCount = Menu.GetValue<MenuSlider>(Orbwalker.ActiveMode.GetModeString() + "WMinHit").Value;
            return Spells.W.IsReady() && target.IsValidTarget(Spells.W.Range) && count >= hitCount && Spells.W.Cast();
        }

        private static bool CastE(AIBaseClient target)
        {
            var eInput = Spells.E.GetPrediction(target,false,-1,new CollisionObjects[]
            {
                CollisionObjects.Heroes,
                CollisionObjects.Minions,
                CollisionObjects.YasuoWall,
            });
            return Spells.E.CanCast(target) && Spells.E.Cast(eInput.CastPosition);
        }

        public static int CountWHits(AttackableUnit target)
        {
            return GetWTargets().Count(obj => obj.IsValid && obj.NetworkId.Equals(target.NetworkId));
        }
        
        public static List<AttackableUnit> GetWTargets()
        {
            var list = new List<AttackableUnit>();
            var range = Spells.W.Range;

            if (QObject != null)
            {
                var objects = ObjectManager.Get<AttackableUnit>().Where(obj => obj.IsValidTarget(range));
                var attackableUnits = objects as AttackableUnit[] ?? objects.ToArray();

                var firstPriority =
                    attackableUnits.Where(obj => obj is AIHeroClient)
                        .MinOrDefault(h => h.Position.Distance(QObject.Position));

                if (firstPriority != null && firstPriority.IsValidTarget(range))
                {
                    list.Add(firstPriority);
                }

                var thirdPriority = attackableUnits.MinOrDefault(h => h.Position.Distance(QObject.Position));

                if (thirdPriority != null && thirdPriority.IsValidTarget(range))
                {
                    list.Add(thirdPriority);
                }
            }

            if (LastAutoTarget != null && LastAutoTarget.IsValidTarget(range))
            {
                list.Add(LastAutoTarget);
            }

            return list;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {

            if (Menu.GetValue<MenuBool>("DrawQ").Enabled)
            {
                CircleRender.Draw(Player.Position,Spells.Q.Range,Color.DeepPink);
            }
            if (Menu.GetValue<MenuBool>("DrawW").Enabled)
            {
                CircleRender.Draw(Player.Position,Spells.W.Range,Color.White);
            }
            if (Menu.GetValue<MenuBool>("DrawE").Enabled)
            {
                CircleRender.Draw(Player.Position,Spells.E.Range,Color.MediumVioletRed);
            }
            if (Menu.GetValue<MenuBool>("DrawR").Enabled)
            {
                CircleRender.Draw(Player.Position,Spells.R.Range,Color.Cyan);
            }
        }

        private static void Flee()
        {
            if (!rMisc.GetValue<MenuKeyBind>("FleeR").Active)
            {
                return;
            }

            try
            {

                Orbwalker.ActiveMode = OrbwalkerMode.None;

                if (!Player.IsDashing() && Player.Path.ToList().Last().Distance(Game.CursorPos) > 100)
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                }

                if (Spells.R.IsReady())
                {
                    var pos = Player.Position.Extend(Game.CursorPos, Spells.R.Range + 10);
                    Spells.R.Cast(pos);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
        

        private static void Orbwalking_AfterAttack(object sender, AfterAttackEventArgs e)
        {
            if (!Spells.Q.IsReady() || !Menu.GetValue<MenuBool>("FarmQ").Enabled)
            {
                return;
            }
            if (!Orbwalker.ActiveMode.IsFarmMode() ||
                Player.ManaPercent < Menu.GetValue<MenuSlider>("FarmMana").Value)
            {
                return;
            }

            var killable =
                MinionManager.GetMinions(Spells.Q.Range)
                    .FirstOrDefault(
                        minion =>
                            !e.Target.NetworkId.Equals(minion.NetworkId) &&
                            HealthPrediction.GetHealthPrediction(
                                minion, (int) ((Player.AttackDelay * 1000) * 2.65f + Game.Ping / 2f), 0) <= 0 &&
                            Spells.Q.GetDamage(minion) >= minion.Health);

            if (killable != null)
            {
                Spells.Q.Cast(killable);
            }
        }
        private static bool ShouldWaitForMinionKill()
        {
            return
                ObjectManager.Get<AIMinionClient>()
                    .Any(
                        minion =>
                            minion.IsValidTarget(Spells.Q.Range) && minion.Team != GameObjectTeam.Neutral &&
                            HealthPrediction.LaneClearHealthPrediction(
                                minion, (int) ((Player.AttackDelay * 1000) * 2.2f), 0) < Spells.Q.GetDamage(minion));
        }
        
        private static void Farm()
        {
            if (!Orbwalker.ActiveMode.IsFarmMode() ||
                Player.ManaPercent < Menu.GetValue<MenuSlider>("FarmMana").Value)
            {
                return;
            }

            if (Spells.Q.IsReady() && Menu.GetValue<MenuBool>("FarmQ").Enabled)
            {
                if ((Menu.GetValue<MenuBool>("FarmQLH").Enabled && Orbwalker.ActiveMode.Equals(OrbwalkerMode.LastHit)) ||
                    ShouldWaitForMinionKill())
                {
                    return;
                }

                var target = TargetSelector.GetTarget(Spells.Q.Range, DamageType.Magical, false);

                //If we can hit the target and 2 other things, then cast Q
                if (Spells.Q.CastIfWillHit(target, 3) == CastStates.SuccessfullyCasted)
                {
                    return;
                }
                //  Otherwise, cast Q on the minion wave
                var minions = MinionManager.GetMinions(Player.Position, Spells.Q.Range);
                var qHit = Spells.Q.GetLineFarmLocation(minions);
                if (qHit.MinionsHit >= Menu.GetValue<MenuSlider>("FarmQHC").Value && Spells.Q.Cast(qHit.Position))
                {
                    return;
                }
            }

            if (Spells.W.IsReady() && Menu.GetValue<MenuBool>("FarmW").Enabled &&
                Orbwalker.ActiveMode.Equals(OrbwalkerMode.LaneClear) && Spells.W.Cast()) {}
        }
    }
}