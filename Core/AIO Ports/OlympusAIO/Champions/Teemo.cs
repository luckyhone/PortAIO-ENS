using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using EnsoulSharp.SDK.Utility;
using Entropy.AIO.Bases;
using SharpDX;

using OlympusAIO.General;
using OlympusAIO.Helpers;
using Color = SharpDX.Color;
using MenuManager = OlympusAIO.Helpers.MenuManager;

namespace OlympusAIO.Champions
{
    class Teemo
    {
        public static AIHeroClient objPlayer = ObjectManager.Player;

        public static List<Misc.CShrooms> Shrooms;

        public static int LastR;

        public static void OnLoad()
        {
            MenuManager.Execute.Teemo();

            SpellManager.Q = new Spell(SpellSlot.Q, 680f);
            SpellManager.W = new Spell(SpellSlot.W, objPlayer.AttackRange);
            SpellManager.E = new Spell(SpellSlot.E, 0f);
            SpellManager.R = new Spell(SpellSlot.R, 400f);

            SpellManager.Q.SetTargetted(05f, 1500f);
            SpellManager.R.SetSkillshot(0.5f, 120f, 1000f, false, SpellType.Circle);

            Misc.CShrooms.Initialize();

            /* Main */
            Game.OnUpdate += Events.OnUpdate;

            /* Drawings */
            Drawing.OnDraw += Events.OnDraw;
            Drawing.OnEndScene += DamageIndicator.OnEndScene;

            /* AIBaseClient */
            AIBaseClient.OnProcessSpellCast += Events.OnProcessSpellCast;
            AIBaseClient.OnBuffAdd += Events.OnBuffGain;

            /* Orbwalker */
            Orbwalker.OnAfterAttack += Events.OnAfter;
            Orbwalker.OnBeforeAttack += Events.OnBefore;

            /* Gapcloser */
            AntiGapcloser.OnGapcloser += Events.OnGapcloser;

            /* Interrupter */
            Interrupter.OnInterrupterSpell += Events.OnInterrupterSpell;
        }

        public class Events
        {
            public static bool alreadySet = false, alreadySetSecond = false;
            public static void OnUpdate(EventArgs args)
            {
                if (objPlayer.IsDead || objPlayer.IsRecalling())
                    return;

                if (SpellManager.R.Level == 2 && !alreadySet)
                {
                    SpellManager.R.Range = 650f;
                    alreadySet = true;
                }
                else if (SpellManager.R.Level == 3 && !alreadySetSecond)
                {
                    SpellManager.R.Range = 900f;
                    alreadySetSecond = true;
                }

                if (MenuManager.MiscMenu["Flee"].GetValue<MenuKeyBind>().Active)
                {
                    Misc.Flee();
                }
                if (MenuManager.MiscMenu["AutoR"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady() && Orbwalker.ActiveMode == OrbwalkerMode.None)
                {
                    Misc.AutoR();
                }

                Misc.KillSteal();

                switch (Orbwalker.ActiveMode)
                {
                    case OrbwalkerMode.Combo:
                        Modes.Combo();
                        break;
                    case OrbwalkerMode.Harass:
                        if (Methods.SpellHarass)
                        {
                            Modes.Harass();
                        }
                        break;
                    case OrbwalkerMode.LaneClear:
                        if (Methods.SpellFarm)
                        {
                            Modes.LaneClear();
                            Modes.JungleClear();
                        }
                        break;
                    case OrbwalkerMode.LastHit:
                        if (Methods.SpellFarm)
                        {
                            Modes.LastHit();
                        }
                        break;
                }
            }
            public static void OnDraw(EventArgs args)
            {
                if (MenuManager.DrawingsMenu["Disable"].GetValue<MenuBool>().Enabled)
                    return;

                if (MenuManager.SpellRangesMenu["QRange"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    CircleRender.Draw(objPlayer.Position, SpellManager.Q.Range, Color.White);
                }
                if (MenuManager.SpellRangesMenu["RRange"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady())
                {
                    CircleRender.Draw(objPlayer.Position, SpellManager.R.Range, Color.Cyan);
                }
                if (MenuManager.DrawingsMiscMenu["ShroomPos"].GetValue<MenuBool>().Enabled)
                {
                    foreach (var place in Shrooms.Where(x => x.Position.IsOnScreen() && x.Position.DistanceToPlayer() <= 1600))
                    {
                        CircleRender.Draw(place.Position, 80, Misc.IsShroomed(place.Position) ? Color.Red : Color.Lime);
                    }
                }
            }
            public static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
            {
                if (sender.IsMe && args.SData.Name == "TeemoRCast")
                {
                    LastR = Variables.GameTimeTickCount;
                }
            }
            public static void OnBuffGain(AIBaseClient sender, AIBaseClientBuffAddEventArgs args)
            {
                if (sender.IsMe || sender.IsAlly || objPlayer.IsDead)
                    return;

                if (MenuManager.MiscMenu["AutoR"].GetValue<MenuBool>().Enabled && MenuManager.MiscMenu["AutoRZhonya"].GetValue<MenuBool>().Enabled)
                {
                    BuffInstance buff = (from senderBuffs in sender.Buffs.Where(x => sender.DistanceToPlayer() < SpellManager.R.Range)
                        from b in new[]
                        {
                            "teleport",
                            "zhonya",
                            "katarinar",
                            "crowstorm",
                            "MissFortuneBulletTime",
                            "gate",
                            "chronorevive",
                        }
                        where args.Buff.Name.ToLower().Contains(b) select senderBuffs).FirstOrDefault();


                    if (buff != null && SpellManager.R.IsReady())
                    {
                        SpellManager.R.Cast(sender.Position);
                    }
                }
            }
            public static void OnInterrupterSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
            {
                if (sender.IsMe || sender.IsAlly || objPlayer.IsDead)
                    return;

                if (MenuManager.MiscInterrupterMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady() && sender.IsValidTarget(SpellManager.Q.Range))
                {
                    if (args.DangerLevel >= Interrupter.DangerLevel.Low)
                    {
                        SpellManager.Q.CastOnUnit(sender);
                    }
                }
            }
            public static void OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
            {
                if (sender.IsMe || sender.IsAlly || objPlayer.IsDead)
                    return;

                if (MenuManager.MiscGapcloserMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady() && sender.IsValidTarget(SpellManager.Q.Range) && sender.IsFacing(objPlayer))
                {
                    SpellManager.Q.Cast(sender);
                }
                if (MenuManager.MiscGapcloserMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady() && sender.IsValidTarget(SpellManager.W.Range) && sender.IsFacing(objPlayer))
                {
                    SpellManager.W.Cast();
                }
                if (MenuManager.MiscGapcloserMenu["R"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady() && sender.IsValidTarget(SpellManager.R.Range) && sender.IsFacing(objPlayer))
                {
                    if (args.EndPosition.DistanceToPlayer() <= 300 && Variables.GameTimeTickCount - LastR > 2000 && !Misc.IsShroomed(objPlayer.Position, 50))
                    {
                        SpellManager.R.Cast(objPlayer.Position);
                    }
                }
            }

            public static void OnAfter(object sender, AfterAttackEventArgs args)
            {
                if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && SpellManager.Q.IsReady() && MenuManager.ComboMenu["Q"].GetValue<MenuBool>().Enabled)
                {
                    var target = (AIBaseClient) args.Target;

                    if (target != null)
                    {
                        switch (MenuManager.ComboMenu["QMode"].GetValue<MenuList>().SelectedValue)
                        {
                            case "Normal":
                                if (MenuManager.ComboMenu["QPrioritizeADC"].GetValue<MenuBool>().Enabled && UtilityManager.Marksmans.Contains(target.SkinName) && target.IsValidTarget(SpellManager.Q.Range))
                                {
                                    SpellManager.Q.CastOnUnit(target);
                                }
                                else if (target.IsValidTarget(SpellManager.Q.Range))
                                {
                                    SpellManager.Q.CastOnUnit(target);
                                }
                                break;
                            case "Only AA Range":
                                if (MenuManager.ComboMenu["QPrioritizeADC"].GetValue<MenuBool>().Enabled && UtilityManager.Marksmans.Contains(target.SkinName) && target.IsValidTarget(objPlayer.GetRealAutoAttackRange()))
                                {
                                    SpellManager.Q.CastOnUnit(target);
                                }
                                else if (target.IsValidTarget(objPlayer.GetRealAutoAttackRange()))
                                {
                                    SpellManager.Q.CastOnUnit(target);
                                }
                                break;
                        }
                    }
                }
            }

            public static void OnBefore(object sender, BeforeAttackEventArgs args)
            {
                if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear && MenuManager.LaneClearMenu["Q"].GetValue<MenuBool>().Enabled && Methods.SpellFarm)
                {
                    if (ManaManager.HaveNoEnoughMana(MenuManager.LaneClearMenu["MinMana"].GetValue<MenuSlider>()))
                        return;

                    var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(SpellManager.Q.Range) && x.IsMinion()).OrderBy(x => x.Health);

                    if (minions.Count() == 0)
                        return;

                    var orbTarget = args.Target;

                    if (orbTarget.Type == GameObjectType.AIMinionClient)
                    {
                        foreach (var minion in minions.Where(x => x.Health <= SpellManager.Q.GetDamage(x) && x.NetworkId != orbTarget.NetworkId && x.IsValidTarget(SpellManager.Q.Range)))
                        {
                            SpellManager.Q.CastOnUnit(minion);
                        }
                    }
                }
                if (Orbwalker.ActiveMode == OrbwalkerMode.LastHit && MenuManager.LastHitMenu["Q"].GetValue<MenuBool>().Enabled && Methods.SpellFarm)
                {
                    if (ManaManager.HaveNoEnoughMana(MenuManager.LastHitMenu["MinMana"].GetValue<MenuSlider>()))
                        return;

                    var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(SpellManager.Q.Range) && x.IsMinion()).OrderBy(x => x.Health);

                    if (minions.Count() == 0)
                        return;

                    var orbTarget = args.Target;

                    if (orbTarget.Type == GameObjectType.AIMinionClient)
                    {
                        foreach (var minion in minions.Where(x => x.Health <= SpellManager.Q.GetDamage(x) && x.NetworkId != orbTarget.NetworkId && x.IsValidTarget(SpellManager.Q.Range)))
                        {
                            SpellManager.Q.CastOnUnit(minion);
                        }
                    }
                }
            }
        }
        public class Modes
        {
            public static void Combo()
            {
                if (MenuManager.ComboMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady() && MenuManager.ComboMenu["QMode"].GetValue<MenuList>().SelectedValue == "Normal")
                {
                    var target = TargetSelector.GetTarget(SpellManager.Q.Range,DamageType.Magical);

                    if (MenuManager.ComboMenu["QPrioritizeADC"].GetValue<MenuBool>().Enabled)
                    {
                        var targets = TargetSelector.GetTargets(SpellManager.Q.Range,DamageType.Magical).OrderBy(x => UtilityManager.Marksmans.Contains(x.SkinName));

                        target = targets.FirstOrDefault();
                    }

                    if (target != null && target.IsValidTarget(SpellManager.Q.Range) && !target.InAutoAttackRange())
                    {
                        SpellManager.Q.CastOnUnit(target);
                    }
                }
                if (MenuManager.ComboMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady())
                {
                    var target = TargetSelector.GetTarget(SpellManager.Q.Range,DamageType.Magical);

                    if (target != null && target.IsValidTarget(SpellManager.Q.Range))
                    {
                        SpellManager.W.Cast();
                    }
                }
                if (MenuManager.ComboMenu["R"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady())
                {
                    var target = TargetSelector.GetTarget(SpellManager.R.Range,DamageType.Magical);

                    if (target.IsValidTarget(SpellManager.R.Range) && MenuManager.ComboMenu["RSave"].GetValue<MenuSlider>().Value <= SpellManager.R.Ammo && !Misc.IsShroomed(target.Position, 100))
                    {
                        var getPrediction = SpellManager.R.GetPrediction(target, true);

                        if (getPrediction.Hitchance >= HitChance.High && Variables.GameTimeTickCount - LastR > 2000)
                        {
                            SpellManager.R.Cast(getPrediction.CastPosition);
                        }
                    }
                }
            }
            public static void Harass()
            {
                if (ManaManager.HaveNoEnoughMana(MenuManager.HarassMenu["MinMana"].GetValue<MenuSlider>()))
                    return;

                if (MenuManager.HarassMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    var target = TargetSelector.GetTarget(SpellManager.Q.Range,DamageType.Magical);

                    if (target != null && target.IsValidTarget(SpellManager.Q.Range) && MenuManager.HarassMenu[target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled)
                    {
                        SpellManager.Q.CastOnUnit(target);
                    }
                }
            }
            public static void LaneClear()
            {
                if (ManaManager.HaveNoEnoughMana(MenuManager.LaneClearMenu["MinMana"].GetValue<MenuSlider>()))
                    return;

                if (MenuManager.LaneClearMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(SpellManager.Q.Range) && x.IsMinion() && !x.IsDead).OrderBy(x => x.Health).Cast<AIBaseClient>().ToList();

                    if (minions.Count() == 0)
                        return;

                    var firstMinion = minions.FirstOrDefault();

                    if (firstMinion.Health >= objPlayer.GetAutoAttackDamage(firstMinion) * 2 && firstMinion.Name.Contains("Super"))
                    {
                        SpellManager.Q.CastOnUnit(firstMinion);
                    }
                }
                if (MenuManager.LaneClearMenu["R"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady())
                {
                    if (MenuManager.LaneClearMenu["RSave"].GetValue<MenuSlider>().Value >= SpellManager.R.Ammo)
                        return;

                    var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(SpellManager.R.Range) && x.IsMinion() && !x.IsDead).OrderBy(x => x.Health).ToList();

                    if (minions.Count() == 0)
                        return;

                    var bestCircularFarmLocation = SpellManager.R.GetCircularFarmLocation(minions);

                    if (MenuManager.LaneClearMenu["RMinHits"].GetValue<MenuSlider>().Value <= bestCircularFarmLocation.MinionsHit && !Misc.IsShroomed(bestCircularFarmLocation.Position.ToVector3()) && Variables.GameTimeTickCount - LastR > 4000)
                    {
                        SpellManager.R.Cast(bestCircularFarmLocation.Position);
                    }
                }
            }
            public static void JungleClear()
            {
                if (ManaManager.HaveNoEnoughMana(MenuManager.JungleClearMenu["MinMana"].GetValue<MenuSlider>()))
                    return;

                if (MenuManager.JungleClearMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(SpellManager.Q.Range) && x.IsJungle()).OrderByDescending(x => x.Health);

                    if (mobs.Count() == 0)
                        return;

                    var firstMob = mobs.FirstOrDefault();

                    if (firstMob != null && firstMob.Health > objPlayer.GetAutoAttackDamage(firstMob))
                    {
                        SpellManager.Q.CastOnUnit(firstMob);
                    }
                }
                if (MenuManager.JungleClearMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady() && objPlayer.ManaPercent >= MenuManager.JungleClearMenu["WMana"].GetValue<MenuSlider>().Value)
                {
                    var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(SpellManager.Q.Range) && x.IsJungle());

                    if (mobs.Count() > 0)
                    {
                        SpellManager.W.CastOnUnit(objPlayer);
                    }
                }
                if (MenuManager.JungleClearMenu["R"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady())
                {
                    var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(SpellManager.R.Range) && x.IsJungle()).OrderBy(x => x.MaxHealth);

                    if (mobs.Count() == 0)
                        return;
                        
                    var firstMob = mobs.FirstOrDefault();

                    if (firstMob != null)
                    {
                        var pos = firstMob.Position.Extend(objPlayer.Position, +200);

                        if (SpellManager.R.Ammo >= 1 && !Misc.IsShroomed(pos, 150) && Variables.GameTimeTickCount - LastR > 5000 && !firstMob.SkinName.Contains("Crab") && firstMob.Health > SpellManager.Q.GetDamage(firstMob))
                        {
                            SpellManager.R.Cast(pos);
                        }
                    }
                }
            }
            public static void LastHit()
            {
                if (ManaManager.HaveNoEnoughMana(MenuManager.LastHitMenu["MinMana"].GetValue<MenuSlider>()))
                    return;


            }
        }
        public class Misc
        {
            public static void KillSteal()
            {
                if (MenuManager.MiscKillSteal["Disable"].GetValue<MenuBool>().Enabled)
                    return;

                if (MenuManager.MiscKillSteal["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(SpellManager.Q.Range) && x.Health + x.AllShield <= SpellManager.Q.GetDamage(x)))
                    {
                        SpellManager.Q.CastOnUnit(target);
                    }
                }
            }
            public static void Flee()
            {
                objPlayer.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

                if (SpellManager.W.IsReady())
                {
                    SpellManager.W.Cast();
                }

                var target = TargetSelector.GetTarget(SpellManager.Q.Range,DamageType.Magical);

                if (target != null && target.IsValidTarget(SpellManager.R.Range) && Variables.GameTimeTickCount - LastR > 4000)
                {
                    SpellManager.R.Cast(objPlayer.Position);
                }
            }
            public static void AutoR()
            {
                var target = TargetSelector.GetTargets(SpellManager.R.Range,DamageType.Magical).OrderBy(x => x.DistanceToPlayer());

                if (objPlayer.HasBuff("camouflagestealth"))
                {
                    return;
                }

                if (target.Count() > 0 && target.FirstOrDefault().IsValidTarget(SpellManager.R.Range))
                {
                    var getPrediction = SpellManager.R.GetPrediction(target.FirstOrDefault());

                    if (getPrediction.Hitchance >= HitChance.High && Variables.GameTimeTickCount - LastR > MenuManager.MiscMenu["AutoRWait"].GetValue<MenuSlider>().Value)
                    {
                        SpellManager.R.Cast(getPrediction.CastPosition);
                    }
                }
                else
                {
                    if (MenuManager.MiscMenu["AutoRSave"].GetValue<MenuSlider>().Value >= SpellManager.R.Ammo)
                        return;

                    foreach (var place in Shrooms.Where(x => x.Position.DistanceToPlayer() <= SpellManager.R.Range && !IsShroomed(x.Position)).Where(x => Variables.GameTimeTickCount - LastR > MenuManager.MiscMenu["AutoRWait"].GetValue<MenuSlider>().Value))
                    {
                        SpellManager.R.Cast(place.Position);
                    }
                }
            }
            public static bool IsShroomed(Vector3 pos, float extraRange = 0f)
            {
                return ObjectManager.Get<AIBaseClient>().Where(x => x.Name == "Noxious Trap").Any(x => pos.Distance(x.Position) <= 250 + extraRange);
            }
            public class CShrooms
            {
                public CShrooms(Vector3 pos)
                {
                    this.Position = pos;
                }

                public Vector3 Position { get; set; }

                public static void Initialize()
                {
                    Shrooms = new List<Misc.CShrooms>
                    {
                        new CShrooms
                        (
                            new Vector3(4607.284f,12030.99f, 56.52991f)
                        ),
                        new CShrooms
                        (
                            new Vector3(4225.9f, 11804.71f, 48.68762f)
                        ),
                        new CShrooms
                        (
                            new Vector3(4483.18f, 11617.75f, 56.01575f)
                        ),
                        new CShrooms
                        (
                            new Vector3(2941.322f, 11089.65f,-71.24036f)
                        ),
                        new CShrooms
                        (
                            new Vector3(2288.499f, 11781.75f, 52.82361f)
                        ),
                        new CShrooms
                        (
                            new Vector3(2963.116f, 12533.87f, 52.83838f)
                        ),
                        new CShrooms
                        (
                            new Vector3(1072.891f, 12047.16f, 52.83801f)
                        ),
                        new CShrooms
                        (
                            new Vector3(1698.563f, 12888.6f, 52.83813f)
                        ),
                        new CShrooms
                        (
                            new Vector3(2223.366f, 13302.51f, 52.83826f)
                        ),
                        new CShrooms
                        (
                            new Vector3(2712.808f, 9917.248f, 54.27905f)
                        ),
                        new CShrooms
                        (
                            new Vector3(3615.167f, 9281.066f, 10.2168f)
                        ),
                        new CShrooms
                        (
                            new Vector3(3721.008f, 12821.07f, 54.01831f)
                        ),
                        new CShrooms
                        (
                            new Vector3(5238.263f, 9152.292f, -71.24023f)
                        ),
                        new CShrooms
                        (
                            new Vector3(4718.816f, 10189.33f, -71.24048f)
                        ),
                        new CShrooms
                        (
                            new Vector3(2974.714f, 7693.558f, 51.85535f)
                        ),
                        new CShrooms
                        (
                            new Vector3(2819.675f, 5757.833f, 55.70862f)
                        ),
                        new CShrooms
                        (
                            new Vector3(4764.329f, 6036.813f, 51.72498f)
                        ),
                        new CShrooms
                        (
                            new Vector3(4618.94f, 7594.63f, 51.85022f)
                        ),
                        new CShrooms
                        (
                            new Vector3(5057.273f, 8599.429f, -46.86829f)
                        ),
                        new CShrooms
                        (
                            new Vector3(6288.328f, 9333.229f, -46.28333f)
                        ),
                        new CShrooms
                        (
                            new Vector3(6296.075f, 10121.04f, 54.08765f)
                        ),
                        new CShrooms
                        (
                            new Vector3(8279.797f, 10268.61f, 50.06848f)
                        ),
                        new CShrooms
                        (
                            new Vector3(6650.508f, 11397.16f, 54.31152f)
                        ),
                        new CShrooms
                        (
                            new Vector3(6796.154f, 12998.33f, 55.02954f)
                        ),
                        new CShrooms
                        (
                            new Vector3(8698.456f, 12531.19f, 56.47681f)
                        ),
                        new CShrooms
                        (
                            new Vector3(9052.601f, 11118.39f, 50.8595f)
                        ),
                        new CShrooms
                        (
                            new Vector3(6599.592f, 4690.146f, 48.52686f)
                        ),
                        new CShrooms
                        (
                            new Vector3(8068.405f, 1896.87f, 50.44141f)
                        ),
                        new CShrooms
                        (
                            new Vector3(8189.194f, 3546.455f, 52.09045f)
                        ),
                        new CShrooms
                        (
                            new Vector3(8571.095f, 4766.659f, 51.68262f)
                        ),
                        new CShrooms
                        (
                            new Vector3(8591.087f, 5551.903f, -46.80432f)
                        ),
                        new CShrooms
                        (
                            new Vector3(9003.129f, 3771.324f, 53.93225f)
                        ),
                        new CShrooms
                        (
                            new Vector3(10513.51f, 3047.225f, 53.27759f)
                        ),
                        new CShrooms
                        (
                            new Vector3(11867.88f, 3893.589f, -67.58936f)
                        ),
                        new CShrooms
                        (
                            new Vector3(11734.22f, 2837.741f,-14.95874f)
                        ),
                        new CShrooms
                        (
                            new Vector3(12432.48f, 3222.754f, 51.18542f)
                        ),
                        new CShrooms
                        (
                            new Vector3(12130.55f, 1368.86f, 51.31519f)
                        ),
                        new CShrooms
                        (
                            new Vector3(13561.15f, 2839.267f, 51.36694f)
                        ),
                        new CShrooms
                        (
                            new Vector3(10112.79f, 4747.785f, -71.24036f)
                        ),
                        new CShrooms
                        (
                            new Vector3(11166.15f, 5506.172f, -35.45532f)
                        ),
                        new CShrooms
                        (
                            new Vector3(12228.22f, 5009.398f, 51.72937f)
                        ),
                        new CShrooms
                        (
                            new Vector3(12625.62f, 5374.395f, 51.72961f)
                        ),
                        new CShrooms
                        (
                            new Vector3(11873.61f, 7175.585f, 51.71106f)
                        ),
                        new CShrooms
                        (
                            new Vector3(12001.79f, 9134.915f, 51.21704f)
                        ),
                        new CShrooms
                        (
                            new Vector3(10018.96f, 8908.733f, 50.77014f)
                        ),
                        new CShrooms
                        (
                            new Vector3(10057.67f, 7767.101f, 51.7533f)
                        ),
                        new CShrooms
                        (
                            new Vector3(9431.246f, 5651.881f, -71.24048f)
                        ),
                        new CShrooms
                        (
                            new Vector3(10056.46f, 6599.326f, 50.33228f)
                        ),
                        new CShrooms
                        (
                            new Vector3(2040.469f, 7655.858f, 50.42175f)
                        ),
                        new CShrooms
                        (
                            new Vector3(12740.02f, 7259.541f, 51.68103f)
                        ),
                    };
                }
            }
        }
    }
}
