using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using PortAIO;
using SharpDX;
using SPrediction;
using Prediction = EnsoulSharp.SDK.Prediction;

namespace CamilleSharp
{
    class Program
    {
        #region Static Fields

        internal static Menu RootMenu;
        internal static Spell Q, W, E, R;
        internal static AIHeroClient Player => ObjectManager.Player;

        internal static bool IsBrawl;
        internal static int LastECastT;
        internal static bool HasQ2 => Player.HasBuff(Q2BuffName);
        internal static bool HasQ => Player.HasBuff(QBuffName);
        internal static bool OnWall => Player.HasBuff(WallBuffName) || E.Instance.Name != "CamilleE";
        internal static bool IsDashing => Player.HasBuff(EDashBuffName + "1") || Player.HasBuff(EDashBuffName + "2") || Player.IsDashing();
        internal static bool ChargingW => Player.HasBuff(WBuffName);
        internal static bool KnockedBack(AIBaseClient target) => target != null && target.HasBuff(KnockBackBuffName);

        internal static string WBuffName => "camillewconeslashcharge";
        internal static string EDashBuffName => "camilleedash";
        internal static string WallBuffName => "camilleedashtoggle";
        internal static string QBuffName => "camilleqprimingstart";
        internal static string Q2BuffName => "camilleqprimingcomplete";
        internal static string RBuffName => "camillertether";
        internal static string KnockBackBuffName => "camilleeknockback2";

        #endregion
        
        #region Collections

        internal static Dictionary<float, DangerPos> DangerPoints = new Dictionary<float, DangerPos>();

        #endregion

        #region Properties

        // general
        internal static bool AllowSkinChanger => RootMenu["skmenu"]["useskin"].GetValue<MenuBool>().Enabled;
        internal static bool ForceUltTarget => RootMenu["tcmenu"]["r33"].GetValue<MenuBool>().Enabled;

        // keybinds
        internal static bool FleeModeActive => RootMenu["kemenu"]["useflee"].GetValue<MenuKeyBind>().Active;

        // sliders
        internal static int HarassMana => RootMenu["hamenu"]["harassmana"].GetValue<MenuSlider>().Value;
        internal static int WaveClearMana => RootMenu["wcmenu"]["wcclearmana"].GetValue<MenuSlider>().Value;
        internal static int JungleClearMana => RootMenu["jgmenu"]["jgclearmana"].GetValue<MenuSlider>().Value;

        #endregion
        
        public static void Loads()
        {
            // Camille by Kurisu :^)
            OnGameLoad(new EventArgs());
        }
        
        private static bool UltEnemies(string chapName)
        {
            return RootMenu["whR" + chapName].GetValue<MenuBool>().Enabled;

        }
        
        internal static void OnGameLoad(EventArgs args)
        {
            try
            {
                if (ObjectManager.Player.CharacterName == "Camille")
                {
                    SetupSpells();
                    SetupConfig();

                    #region Subscribed Events

                    Game.OnUpdate += Game_OnUpdate;
                    Drawing.OnDraw += Drawing_OnDraw;
                    Drawing.OnEndScene += Drawing_OnEndScene;
                    AIBaseClient.OnProcessSpellCast += Obj_AI_Base_OnSpellCast;
                    AIBaseClient.OnIssueOrder += CamilleOnIssueOrder;
                    AIBaseClient.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
                    GameObject.OnCreate += Obj_GeneralParticleEmitter_OnCreate;
                    Interrupter.OnInterrupterSpell += Interrupter2_OnInterruptableTarget;

                    #endregion

                    var color = System.Drawing.Color.FromArgb(200, 0, 220, 144);
                    var hexargb = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

                    Game.Print("<b><font color=\"" + hexargb + "\">Camille#</font></b> - Loaded!");
                    //LeagueSharp.Common.Utility.DelayAction.Add(1000, CheckActivator);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            var attacker = sender as AIHeroClient;
            if (attacker != null && attacker.IsEnemy && attacker.Distance(Player) <= R.Range + 25)
            {
                var aiTarget = args.Target as AIHeroClient;

                var tsTarget = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                if (tsTarget == null)
                {
                    return;
                }

                if (R.IsReady() && RootMenu["revade"]["revadee"].GetValue<MenuBool>().Enabled)
                {
                    foreach (var spell in Evadeable.DangerList.Select(entry => entry.Value)
                        .Where(spell => spell.SDataName.ToLower() == args.SData.Name.ToLower())
                        .Where(spell => RootMenu["revade" + spell.SDataName.ToLower()].GetValue<MenuBool>().Enabled))
                    {
                        switch (spell.EvadeType)
                        {
                            case EvadeType.Target:
                                if (aiTarget != null && aiTarget.IsMe)
                                {
                                    UseR(tsTarget, true);
                                }
                                break;

                            case EvadeType.SelfCast:
                                if (attacker.Distance(Player) <= R.Range)
                                {
                                    UseR(tsTarget, true);
                                }
                                break;
                            case EvadeType.SkillshotLine:
                                var lineStart = args.Start.To2D();
                                var lineEnd = args.End.To2D();

                                if (lineStart.Distance(lineEnd) < R.Range)
                                    lineEnd = lineStart + (lineEnd - lineStart).Normalized() * R.Range + 25;

                                if (lineStart.Distance(lineEnd) > R.Range)
                                    lineEnd = lineStart + (lineEnd - lineStart).Normalized() * R.Range * 2;

                                var spellProj = Player.ServerPosition.To2D().ProjectOn(lineStart, lineEnd);
                                if (spellProj.IsOnSegment)
                                {
                                    UseR(tsTarget, true);
                                }
                                break;

                            case EvadeType.SkillshotCirce:
                                var curStart = args.Start.To2D();
                                var curEnd = args.End.To2D();

                                if (curStart.Distance(curEnd) > R.Range)
                                    curEnd = curStart + (curEnd - curStart).Normalized() * R.Range;

                                if (curEnd.Distance(Player) <= R.Range)
                                {
                                    UseR(tsTarget, true);
                                }
                                break;
                        }
                    }
                }
            }
        }

        private static void Interrupter2_OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (RootMenu["interrupt2"].GetValue<MenuBool>().Enabled)
            {
                if (sender.IsValidTarget(E.Range) && E.IsReady())
                {
                    UseE(sender.ServerPosition);
                }
            }
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var eCircle = RootMenu["drawmyehehe"].GetValue<MenuBool>().Enabled;
            if (eCircle)
            {
                LeagueSharpCommon.Render.Circle.DrawCircle(Player.Position, E.Range, System.Drawing.Color.FromArgb(165, 0, 220, 144));
            }

            var wCircle = RootMenu["drawmywhehe"].GetValue<MenuBool>().Enabled;
            if (wCircle)
            {
                LeagueSharpCommon.Render.Circle.DrawCircle(Player.Position, W.Range, System.Drawing.Color.FromArgb(165, 0, 220, 144));
            }

            var rCircle = RootMenu["drawmyrhehe"].GetValue<MenuBool>().Enabled;
            if (rCircle)
            {
                LeagueSharpCommon.Render.Circle.DrawCircle(Player.Position, R.Range, System.Drawing.Color.FromArgb(165, 0, 220, 144));
            }
        }

        private static void Obj_GeneralParticleEmitter_OnCreate(GameObject sender, EventArgs args)
        {
            var emitter = sender as EffectEmitter;
            if (emitter != null && emitter.Name.ToLower() == "camille_base_r_indicator_edge.troy")
            {
                DangerPoints[Game.Time] = new DangerPos(emitter, AvoidType.Outside, 450f); // 450f ?
            }

            if (emitter != null && emitter.Name.ToLower() == "veigar_base_e_cage_red.troy")
            {
                DangerPoints[Game.Time] = new DangerPos(emitter, AvoidType.Inside, 400f); // 400f ?
            }
        }

        private static void CamilleOnIssueOrder(AIBaseClient sender, AIBaseClientIssueOrderEventArgs args)
        {
            if (OnWall && E.IsReady() && RootMenu["usecombo"].GetValue<MenuKeyBind>().Active)
            {
                var issueOrderPos = args.TargetPosition;
                if (sender.IsMe && args.Order == GameObjectOrder.MoveTo)
                {
                    var issueOrderDirection = (issueOrderPos - Player.Position).To2D().Normalized();

                    var aiHero = TargetSelector.GetTarget(E.Range + 100, DamageType.Physical);
                    if (aiHero != null)
                    {
                        var heroDirection = (aiHero.Position - Player.Position).To2D().Normalized();
                        if (heroDirection.AngleBetween(issueOrderDirection) > 10)
                        {
                            var anyDangerousPos = false;
                            var dashEndPos = Player.Position.To2D() + heroDirection * Player.Distance(aiHero.Position);

                            if (Player.Position.To2D().Distance(dashEndPos) > E.Range)
                                dashEndPos = Player.Position.To2D() + heroDirection * E.Range;

                            foreach (var x in DangerPoints)
                            {
                                var obj = x.Value;
                                if (obj.Type == AvoidType.Outside && dashEndPos.Distance(obj.Emitter.Position) > obj.Radius)
                                {
                                    anyDangerousPos = true;
                                    break;
                                }

                                if (obj.Type == AvoidType.Inside)
                                {
                                    var proj = obj.Emitter.Position.To2D().ProjectOn(Player.Position.To2D(), dashEndPos);
                                    if (proj.IsOnSegment && proj.SegmentPoint.Distance(obj.Emitter.Position) <= obj.Radius)
                                    {
                                        anyDangerousPos = true;
                                        break;
                                    }
                                }
                            }

                            if (dashEndPos.To3D().IsUnderEnemyTurret() && RootMenu["eturret"].GetValue<MenuKeyBind>().Active)
                                anyDangerousPos = true;

                            if (anyDangerousPos)
                            {
                                args.Process = false;
                            }
                            else
                            {
                                args.Process = false;

                                var poutput = E.GetPrediction(aiHero);
                                if (poutput.Hitchance >= HitChance.High)
                                {
                                    Player.IssueOrder(GameObjectOrder.MoveTo, poutput.CastPosition, false);
                                }
                            }
                        }
                    }
                }
            }

            if (OnWall && E.IsReady() && RootMenu["usejgclear"].GetValue<MenuKeyBind>().Active)
            {
                var issueOrderPos = args.TargetPosition;
                if (sender.IsMe && args.Order == GameObjectOrder.MoveTo)
                {
                    var issueOrderDirection = (issueOrderPos - Player.Position).To2D().Normalized();

                    var aiMob = MinionManager.GetMinions(Player.Position, W.Range + 100,
                        MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth).FirstOrDefault();

                    if (aiMob != null)
                    {
                        //var heroDirection = (aiMob.Position - Player.Position).To2D().Normalized();
                        //if (heroDirection.AngleBetween(issueOrderDirection) > 10)
                        //{
                        args.Process = false;
                        Player.IssueOrder(GameObjectOrder.MoveTo, aiMob.ServerPosition, false);
                        //}
                    }
                }
            }
        }

        private static void Obj_AI_Base_OnSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && Orbwalker.IsAutoAttack(sender.CharacterName))
            {
                var aiHero = args.Target as AIHeroClient;
                if (aiHero.IsValidTarget())
                {
                    if (!Player.IsUnderEnemyTurret() || RootMenu["usecombo"].GetValue<MenuKeyBind>().Active)
                    {
                        if (!Q.IsReady() || HasQ && !HasQ2)
                        {
                            /*if (Items.CanUseItem(3077))
                                Items.UseItem(3077);
                            if (Items.CanUseItem(3074))
                                Items.UseItem(3074);
                            if (Items.CanUseItem(3748))
                                Items.UseItem(3748);*/
                        }
                    }
                }

                if (RootMenu["usecombo"].GetValue<MenuKeyBind>().Active)
                {
                    if (aiHero.IsValidTarget() && RootMenu["useqcombo"].GetValue<MenuBool>().Enabled)
                    {
                        UseQ(aiHero);
                    }
                }

                if (RootMenu["useharass"].GetValue<MenuKeyBind>().Active)
                {
                    if (aiHero.IsValidTarget())
                    {
                        if (Player.Mana / Player.MaxMana * 100 < RootMenu["harassmana"].GetValue<MenuSlider>().Value)
                        {
                            return;
                        }

                        UseQ(aiHero);
                    }
                }

                if (RootMenu["usejgclear"].GetValue<MenuKeyBind>().Active)
                {
                    var aiMob = args.Target as AIMinionClient;
                    if (aiMob != null && aiMob.IsValidTarget())
                    {
                        if (!Player.IsUnderEnemyTurret() || Player.CountEnemyHeroesInRange(1000) <= 0)
                        {
                            if (!Q.IsReady() || HasQ && !HasQ2)
                            {
                            }
                        }
                    }

                    #region AA-> Q any attackable
                    var unit = args.Target as AttackableUnit;
                    if (unit != null)
                    {
                        if (Player.CountEnemyHeroesInRange(1000) < 1 || Player.IsUnderAllyTurret()
                            || !RootMenu["clearnearenemy"].GetValue<MenuBool>().Enabled)
                        {
                            // if jungle minion
                            var m = unit as AIMinionClient;
                            if (m != null)
                            {
                                if (!m.SkinName.StartsWith("sru_plant") && !m.Name.StartsWith("Minion"))
                                {
                                    #region AA -> Q

                                    if (Q.IsReady() && RootMenu["useqjgclear"].GetValue<MenuBool>().Enabled)
                                    {
                                        if (m.Position.Distance(Player.ServerPosition) <= Q.Range + 90)
                                        {
                                            UseQ(m);
                                        }
                                    }

                                    #endregion
                                }
                            }

                            if (Q.IsReady() && !unit.Name.StartsWith("Minion"))
                            {
                                if (RootMenu["useqjgclear"].GetValue<MenuBool>().Enabled)
                                {
                                    UseQ(unit);
                                }
                            }
                        }
                    }

                    #endregion
                }

                if (RootMenu["usewcclear"].GetValue<MenuKeyBind>().Active)
                {
                    var aiMob = args.Target as AIMinionClient;
                    if (aiMob != null && aiMob.IsValidTarget())
                    {
                        if (!Player.IsUnderEnemyTurret() || Player.CountEnemyHeroesInRange(1000) <= 0)
                        {
                            if (!Q.IsReady() || HasQ && !HasQ2)
                            {
                            }
                        }
                    }

                    var aiBase = args.Target as AIBaseClient;
                    if (aiBase != null && aiBase.IsValidTarget() && aiBase.Name.StartsWith("Minion"))
                    {
                        #region LaneClear Q

                        if (Player.CountEnemyHeroesInRange(1000) < 1 || Player.IsUnderAllyTurret()
                            || !RootMenu["clearnearenemy"].GetValue<MenuBool>().Enabled)
                        {
                            if (aiBase.IsUnderEnemyTurret() && Player.CountEnemyHeroesInRange(1000) > 0 && !Player.IsUnderAllyTurret())
                            {
                                return;
                            }

                            if (Player.Mana / Player.MaxMana * 100 < RootMenu["wcclearmana"].GetValue<MenuSlider>().Value)
                            {
                                if (Player.CountEnemyHeroesInRange(1000) > 0 && !Player.IsUnderAllyTurret())
                                {
                                    return;
                                }
                            }

                            #region AA -> Q 

                            if (Q.IsReady() && RootMenu["useqwcclear"].GetValue<MenuBool>().Enabled)
                            {
                                if (aiBase.Distance(Player.ServerPosition) <= Q.Range + 90)
                                {
                                    UseQ(aiBase);
                                }
                            }

                            #endregion
                        }

                        #endregion
                    }

                    #region AA-> Q any attackable
                    var unit = args.Target as AttackableUnit;
                    if (unit != null)
                    {
                        if (Player.CountEnemyHeroesInRange(1000) < 1 || Player.IsUnderAllyTurret()
                            || !RootMenu["clearnearenemy"].GetValue<MenuBool>().Enabled)
                        {
                            // if jungle minion
                            var m = unit as AIMinionClient;
                            if (m != null && !m.SkinName.StartsWith("sru_plant"))
                            {
                                #region AA -> Q

                                if (Q.IsReady() && RootMenu["useqwcclear"].GetValue<MenuBool>().Enabled)
                                {
                                    if (m.Position.Distance(Player.ServerPosition) <= Q.Range + 90)
                                    {
                                        UseQ(m);
                                    }
                                }

                                #endregion
                            }

                            if (Q.IsReady())
                            {
                                if (RootMenu["useqwcclear"].GetValue<MenuBool>().Enabled)
                                {
                                    UseQ(unit);
                                }
                            }
                        }
                    }

                    #endregion
                }
            }
        }

        #region Modes

        static void Combo()
        {
            var target = TargetSelector.GetTarget(E.IsReady() ? E.Range * 2 : W.Range, DamageType.Physical);
            if (target.IsValidTarget() && !target.IsZombie())
            {
                if (RootMenu["mmenu"]["lockwcombo"].GetValue<MenuBool>().Enabled)
                {
                    LockW(target);
                }

                if (RootMenu["abmenu"]["usewcombo"].GetValue<MenuBool>().Enabled)
                {
                    if (!E.IsReady() || !RootMenu["abmenu"]["useecombo"].GetValue<MenuBool>().Enabled)
                    {
                        UseW(target);
                    }
                }

                if (RootMenu["abmenu"]["useecombo"].GetValue<MenuBool>().Enabled)
                {
                    UseE(target.ServerPosition);
                }

                if (RootMenu["abmenu"]["usercombo"].GetValue<MenuBool>().Enabled)
                {
                    UseR(target);
                }
            }
        }

        static void Harass()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (target.IsValidTarget() && !target.IsZombie())
            {
                if (RootMenu["mmenu"]["lockwharass"].GetValue<MenuBool>().Enabled)
                {
                    LockW(target);
                }

                if (RootMenu["hamenu"]["usewharass"].GetValue<MenuBool>().Enabled)
                {
                    UseW(target);
                }
            }
        }

        private static void Clear()
        {
            var minions = MinionManager.GetMinions(Player.Position, W.Range,
                MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly, MinionManager.MinionOrderTypes.MaxHealth);

            foreach (var unit in minions)
            {
                if (!unit.Name.Contains("Mini")) // mobs
                {
                    if (RootMenu["mmenu"]["lockwclear"].GetValue<MenuBool>().Enabled)
                    {
                        LockW(unit);
                    }

                    if (RootMenu["jgmenu"]["usewjgclear"].GetValue<MenuBool>().Enabled)
                    {
                        UseW(unit);
                    }

                    if (!W.IsReady() || !RootMenu["jgmenu"]["usewjgclear"].GetValue<MenuBool>().Enabled)
                    {
                        if (!ChargingW && RootMenu["jgmenu"]["useejgclear"].GetValue<MenuBool>().Enabled)
                        {
                            if (Player.CountEnemyHeroesInRange(1200) <= 0 || !RootMenu["clmenu"]["clearnearenemy"].GetValue<MenuBool>().Enabled)
                            {
                                UseE(unit.ServerPosition, false);
                            }
                        }
                    }
                }
                else // minions
                {
                    if (RootMenu["mmenu"]["lockwclear"].GetValue<MenuBool>().Enabled)
                    {
                        LockW(unit);
                    }

                    if (Player.CountEnemyHeroesInRange(1000) < 1 || !RootMenu["clmenu"]["clearnearenemy"].GetValue<MenuBool>().Enabled)
                    {
                        if (RootMenu["wcmenu"]["usewwcclear"].GetValue<MenuBool>().Enabled && W.IsReady())
                        {
                            var farmradius =
                                FarmPrediction.GetBestCircularFarmLocation(
                                    minions.Where(x => x.IsMinion()).Select(x => x.Position.To2D()).ToList(), 165f, W.Range);

                            if (farmradius.MinionsHit >= RootMenu["wcmenu"]["usewwcclearhit"].GetValue<MenuSlider>().Value)
                            {
                                W.Cast(farmradius.Position);
                            }
                        }
                    }
                }
            }
        }

        #endregion
        
        #region Skills
        static void UseQ(AttackableUnit t)
        {
            if (Q.IsReady())
            {
                if (!HasQ || HasQ2)
                {
                    if (Q.Cast())
                    {
                        return;
                    }
                }
                else
                {
                    var aiHero = t as AIHeroClient;
                    if (aiHero != null && Qdmg(aiHero, false) + Player.GetAutoAttackDamage(aiHero, true) * 1 >= aiHero.Health)
                    {
                        if (Q.Cast())
                        {
                            return;
                        }
                    }
                }
            }
        }

        static void UseW(AIBaseClient target)
        {
            if (ChargingW || IsDashing || OnWall || !CanW(target))
            {
                return;
            }

            if (KnockedBack(target))
            {
                return;
            }

            if (W.IsReady() && target.Distance(Player.ServerPosition) <= W.Range)
            {
                W.Cast(target.ServerPosition);
            }
        }
        
        static void UseE(Vector3 p, bool combo = true)
        {
            if (IsDashing || OnWall || ChargingW || !E.IsReady())
            {
                return;
            }

            if (combo)
            {
                if (Player.Distance(p) < RootMenu["tcmenu"]["minerange"].GetValue<MenuSlider>().Value)
                {
                    return;
                }

                if (p.IsUnderEnemyTurret() && RootMenu["tcmenu"]["eturret"].GetValue<MenuKeyBind>().Active)
                {
                    return;
                }
            }

            var posChecked = 0;
            var maxPosChecked = 40;
            var posRadius = 145;
            var radiusIndex = 0;

            if (RootMenu["tcmenu"]["enhancede"].GetValue<MenuBool>().Enabled)
            {
                maxPosChecked = 80;
                posRadius = 65;
            }

            var candidatePosList = new List<Vector2>();

            while (posChecked < maxPosChecked)
            {
                radiusIndex++;

                var curRadius = radiusIndex * (0x2 * posRadius);
                var curCurcleChecks = (int)Math.Ceiling((0x2 * Math.PI * curRadius) / (0x2 * (double)posRadius));

                for (var i = 1; i < curCurcleChecks; i++)
                {
                    posChecked++;

                    var cRadians = (0x2 * Math.PI / (curCurcleChecks - 0x1)) * i;
                    var xPos = (float)Math.Floor(p.X + curRadius * Math.Cos(cRadians));
                    var yPos = (float)Math.Floor(p.Y + curRadius * Math.Sin(cRadians));
                    var desiredPos = new Vector2(xPos, yPos);
                    var anyDangerousPos = false;

                    foreach (var x in DangerPoints)
                    {
                        var obj = x.Value;
                        if (obj.Type == AvoidType.Outside && desiredPos.Distance(obj.Emitter.Position) > obj.Radius)
                        {
                            anyDangerousPos = true;
                            break;
                        }

                        if (obj.Type == AvoidType.Inside)
                        {
                            var proj = obj.Emitter.Position.To2D().ProjectOn(desiredPos, p.To2D());
                            if (proj.IsOnSegment && proj.SegmentPoint.Distance(obj.Emitter.Position) <= obj.Radius)
                            {
                                anyDangerousPos = true;
                                break;
                            }
                        }
                    }

                    if (anyDangerousPos)
                    {
                        continue;
                    }

                    var wtarget = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                    if (wtarget != null && ChargingW)
                    {
                        if (desiredPos.Distance(wtarget.ServerPosition) > W.Range - 100)
                        {
                            continue;
                        }
                    }

                    if (desiredPos.IsWall())
                    {
                        candidatePosList.Add(desiredPos);
                    }
                }
            }

            var bestWallPoint =
                candidatePosList.Where(x => Player.Distance(x) <= E.Range && x.Distance(p) <= E.Range)
                    .OrderBy(x => x.Distance(p))
                    .FirstOrDefault();

            if (E.IsReady() && bestWallPoint.IsValid())
            {
                if (W.IsReady() && RootMenu["abmenu"]["usewcombo"].GetValue<MenuBool>().Enabled && combo)
                {
                    W.UpdateSourcePosition(bestWallPoint.To3D(), bestWallPoint.To3D());

                    if (RootMenu["tcmenu"]["www"].GetValue<MenuBool>().Enabled)
                    {
                        int dashSpeedEst = 1450;
                        int hookSpeedEst = 1250;

                        float e1Time = 1000 * (Player.Distance(bestWallPoint) / hookSpeedEst);
                        float meToWall = e1Time + (1000 * (Player.Distance(bestWallPoint) / dashSpeedEst));
                        float wallToHero = (1000 * (bestWallPoint.Distance(p) / dashSpeedEst));

                        var travelTime = 250 + meToWall + wallToHero;
                        if (travelTime >= 1250 && travelTime <= 1750)
                        {
                            W.Cast(p);
                        }

                        if (travelTime > 1750)
                        {
                            var delay = 100 + (travelTime - 1750);
                            DelayAction.Add((int)delay, () => W.Cast(p));
                        }
                    }
                }

                if (E.Cast(bestWallPoint))
                {
                    LastECastT = Environment.TickCount;
                }
            }
        }
        
        static void UseR(AIHeroClient target, bool force = false)
        {
            if (R.IsReady() && force)
            {
                R.CastOnUnit(target);
            }

            if (target.Distance(Player) <= R.Range)
            {
                if (RootMenu["tcmenu"]["r55"].GetValue<MenuBool>().Enabled)
                {
                    var unit = TargetSelector.SelectedTarget;
                    if (unit == null || unit.NetworkId != target.NetworkId)
                    {
                        return;
                    }
                }

                if (Qdmg(target) + Player.GetAutoAttackDamage(target) * 2 >= target.Health)
                {
                    if (ObjectManager.Player.InAutoAttackRange(target))
                    {
                        return;
                    }
                }

                if (R.IsReady() && Cdmg(target) >= target.Health)
                {
                    if (!IsBrawl || IsBrawl && !UltEnemies(target.CharacterName) || RootMenu["whR" + target.CharacterName].GetValue<MenuBool>().Enabled)
                    {
                        R.CastOnUnit(target);
                    }
                }
            }
        }
        
        static bool CanW(AIBaseClient target)
        {
            const float wCastTime = 2000f;

            if (OnWall || IsDashing || target == null)
            {
                return false;
            }

            if (Q.IsReady())
            {
                if (!HasQ || HasQ2)
                {
                    if (target.Distance(Player) <= Player.AttackRange + Player.Distance(Player.BBox.Minimum) + 65)
                    {
                        return false;
                    }
                }
                else
                {
                    if (Qdmg(target, false) + Player.GetAutoAttackDamage(target, true) * 1 >= target.Health)
                    {
                        return false;
                    }
                }
            }

            if (Environment.TickCount - LastECastT < 500)
            {
                // to prevent e away from w in the spur of the moment
                return false;
            }

            if (target.Distance(Player) <= Player.AttackRange + Player.Distance(Player.BBox.Minimum) + 65)
            {
                if (Player.GetAutoAttackDamage(target, true) * 2 + Qdmg(target, false) >= target.Health)
                {
                    return false;
                }
            }

            var b = Player.GetBuff(QBuffName);
            if (b != null && (b.EndTime - Game.Time) * 1000 <= wCastTime)
            {
                return false;
            }

            var c = Player.GetBuff(Q2BuffName);
            if (c != null && (c.EndTime - Game.Time) * 1000 <= wCastTime)
            {
                return false;
            }

            return true;
        }

        static void LockW(AIBaseClient target)
        {
            if (!RootMenu["mmenu"]["lockw"].GetValue<MenuBool>().Enabled)
            {
                return;
            }

            if (OnWall || IsDashing || target == null || !CanW(target))
            {
                return;
            }

            if (ChargingW && Orbwalker.ActiveMode != OrbwalkerMode.None)
            {
                Orbwalker.AttackEnabled = (false);
            }

            if (ChargingW && target.Distance(Player) <= W.Range + 35)
            {
                var pos = Prediction.GetPrediction(target, Game.Ping / 2000f).UnitPosition.Extend(Player.ServerPosition, W.Range - 65);
                if (pos.IsUnderEnemyTurret() && RootMenu["tcmenu"]["eturret"].GetValue<MenuKeyBind>().Active)
                {
                    return;
                }

                Player.IssueOrder(GameObjectOrder.MoveTo, pos, false);
            }
        }
        
        #endregion

        private static void Game_OnUpdate(EventArgs args)
        {
            // an ok check for teamfighting (sfx style)
            IsBrawl = Player.CountAllyHeroesInRange(1500) >= 2
                && Player.CountEnemyHeroesInRange(1350) > 2
                || Player.CountEnemyHeroesInRange(1200) > 3;
 
            // turn off orbwalk attack while charging to allow movement
            Orbwalker.AttackEnabled = (!ChargingW);

            // remove danger positions
            foreach (var entry in DangerPoints)
            {
                var ultimatum = entry.Value.Emitter;
                if (ultimatum.IsValid == false || ultimatum.IsVisibleOnScreen == false)
                {
                    DangerPoints.Remove(entry.Key);
                    break;
                }

                var timestamp = entry.Key;
                if (Game.Time - timestamp > 4f)
                {
                    DangerPoints.Remove(timestamp);
                    break;
                }
            }

            if (FleeModeActive)
            {
                Orbwalker.Move( Game.CursorPos);
                UseE(Game.CursorPos, false);
            }

            if (AllowSkinChanger)
            {
                //Player.SetSkin(Player.BaseSkinName, RootMenu.Item("skinid").GetValue<Slider>().Value);
            }

            if (ForceUltTarget)
            {
                var rtarget = GameObjects.EnemyHeroes.FirstOrDefault(x => x.HasBuff(RBuffName));
                if (rtarget != null && rtarget.IsValidTarget() && !rtarget.IsZombie())
                {
                    if (rtarget.Distance(Player) <= Player.AttackRange + Player.Distance(Player.BBox.Minimum) + 75)
                    {
                        TargetSelector.SelectedTarget = rtarget;
                        Orbwalker.ForceTarget =(rtarget);
                    }
                }
            }

            if (IsDashing || OnWall || Player.IsDead)
            {
                return;
            }

            if (RootMenu["usecombo"].GetValue<MenuKeyBind>().Active)
            {
                Combo();
            }

            if (RootMenu["usewcclear"].GetValue<MenuKeyBind>().Active)
            {
                if (Player.Mana / Player.MaxMana * 100 > WaveClearMana)
                {
                    Clear();
                }
            }

            if (RootMenu["usejgclear"].GetValue<MenuKeyBind>().Active)
            {
                if (Player.Mana / Player.MaxMana * 100 > JungleClearMana)
                {
                    Clear();
                }
            }

            if (RootMenu["useharass"].GetValue<MenuKeyBind>().Active)
            {
                if (Player.Mana / Player.MaxMana * 100 > HarassMana)
                {
                    Harass();
                }
            }
        }

        #region Setup
        private static void SetupSpells()
        {
            Q = new Spell(SpellSlot.Q, 135f);
            W = new Spell(SpellSlot.W, 625f);

            E = new Spell(SpellSlot.E, 975f);
            E.SetSkillshot(0.125f, ObjectManager.Player.BoundingRadius, 1750, false, SpellType.Line);

            R = new Spell(SpellSlot.R, 465f);
        }

        private static void SetupConfig()
        {
            RootMenu = new Menu("camille","Camille#", true);
            
            var kemenu = new Menu("kemenu","-] Keys");
            kemenu.Add(new MenuKeyBind("usecombo", "Combo [active]",Keys.Space, KeyBindType.Press));
            kemenu.Add(new MenuKeyBind("useharass", "Harass [active]",Keys.C, KeyBindType.Press));
            kemenu.Add(new MenuKeyBind("usewcclear", "Wave Clear [active]",Keys.V, KeyBindType.Press));
            kemenu.Add(new MenuKeyBind("usejgclear", "Jungle Clear [active]",Keys.V, KeyBindType.Press));
            kemenu.Add(new MenuKeyBind("useflee", "Flee [active]",Keys.Z, KeyBindType.Press));
            RootMenu.Add(kemenu);
            
            var comenu = new Menu("cmenu","-] Combo");

            var tcmenu = new Menu("tcmenu","-] Extra");

            var abmenu = new Menu("abmenu","-] Skills");

            var whemenu = new Menu("whemenu","R Focus Targets");
            whemenu.SetFontColor(SharpDX.Color.Cyan);
            foreach (var hero in GameObjects.EnemyHeroes)
                whemenu.Add(new MenuBool("whR" + hero.CharacterName, hero.CharacterName))
                    .SetValue(false).SetTooltip("R Only on " + hero.CharacterName);
            abmenu.Add(whemenu);
            
            abmenu.Add(new MenuBool("useqcombo", "Use Q")).SetValue(true);
            abmenu.Add(new MenuBool("usewcombo", "Use W")).SetValue(true);
            abmenu.Add(new MenuBool("useecombo", "Use E")).SetValue(true);
            abmenu.Add(new MenuBool("usercombo", "Use R")).SetValue(true);

            var revade = new Menu("revade","-] Evade");
            revade.Add(new MenuBool("revadee", "Use R to Evade")).SetValue(true);

            foreach (var spell in from entry in Evadeable.DangerList
                     select entry.Value
                     into spell
                     from hero in GameObjects.EnemyHeroes.Where(x => x.CharacterName.ToLower() == spell.ChampionName.ToLower())
                     select spell)
            {
                revade.Add(new MenuBool("revade" + spell.SDataName.ToLower(), "-> " + spell.ChampionName + " R"))
                    .SetValue(true);
            }
            
            var mmenu = new Menu("mmenu","-] Magnet");
            mmenu.Add(new MenuBool("lockw", "Magnet W [Beta]")).SetValue(true);
            mmenu.Add(new MenuBool("lockwcombo", "-> Combo")).SetValue(true);
            mmenu.Add(new MenuBool("lockwharass", "-> Harass")).SetValue(true);
            mmenu.Add(new MenuBool("lockwclear", "-> Clear")).SetValue(true);
            mmenu.Add(new MenuBool("lockorbwalk", "Magnet Orbwalking"))
                .SetValue(false).SetTooltip("Coming Soon");

            tcmenu.Add(new MenuBool("r55", "Only R Selected Target")).SetValue(false);
            tcmenu.Add(new MenuBool("r33", "Orbwalk Focus R Target")).SetValue(true);
            tcmenu.Add(new MenuKeyBind("eturret", "Dont E Under Turret",Keys.L, KeyBindType.Toggle, true)).AddPermashow();
            tcmenu.Add(new MenuSlider("minerange", "Minimum E Range",165, 0, (int)E.Range));
            tcmenu.Add(new MenuBool("enhancede", "Enhanced E Precision")).SetValue(false);
            tcmenu.Add(new MenuBool("www", "Expirimental Combo")).SetValue(false).SetTooltip("W -> E");
            comenu.Add(tcmenu);

            comenu.Add(revade);
            comenu.Add(mmenu);
            comenu.Add(abmenu);

            RootMenu.Add(comenu);
            
            
            var hamenu = new Menu("hamenu","-] Harass");
            hamenu.Add(new MenuBool("useqharass", "Use Q")).SetValue(true);
            hamenu.Add(new MenuBool("usewharass", "Use W")).SetValue(true);
            hamenu.Add(new MenuSlider("harassmana", "Harass Mana %",65));
            RootMenu.Add(hamenu);

            var clmenu = new Menu("clmenu","-] Clear");

            var jgmenu = new Menu("jgmenu","Jungle");
            jgmenu.Add(new MenuSlider("jgclearmana", "Minimum Mana %",35));
            jgmenu.Add(new MenuBool("useqjgclear", "Use Q")).SetValue(true);
            jgmenu.Add(new MenuBool("usewjgclear", "Use W")).SetValue(true);
            jgmenu.Add(new MenuBool("useejgclear", "Use E")).SetValue(true);
            clmenu.Add(jgmenu);

            var wcmenu = new Menu("wcmenu","WaveClear");
            wcmenu.Add(new MenuSlider("wcclearmana", "Minimum Mana %",55));
            wcmenu.Add(new MenuBool("useqwcclear", "Use Q")).SetValue(true);
            wcmenu.Add(new MenuBool("usewwcclear", "Use W")).SetValue(true);
            wcmenu.Add(new MenuSlider("usewwcclearhit", "-> Min Hit >=",3, 1, 6));
            clmenu.Add(wcmenu);

            clmenu.Add(new MenuBool("clearnearenemy", "Dont Clear Near Enemy")).SetValue(true);
            //clmenu.Add(new MenuBool("t11", "Use Hydra")).SetValue(true);

            RootMenu.Add(clmenu);
            
            var fmenu = new Menu("fmenu","-] Flee");
            fmenu.Add(new MenuBool("useeflee", "Use E")).SetValue(true);
            RootMenu.Add(fmenu);

            var exmenu = new Menu("exmenu","-] Events");
            exmenu.Add(new MenuBool("interrupt2", "Interrupt")).SetValue(false).ValueChanged +=
                (sender, eventArgs) => sender.Enabled = false;
            exmenu.Add(new MenuBool("antigapcloserx", "Anti-Gapcloser")).SetValue(false).ValueChanged +=
                (sender, eventArgs) => sender.Enabled = false;
            RootMenu.Add(exmenu);

            var skmenu = new Menu("skmenu","-] Skins");
            var skinitem = new MenuBool("useskin", "Enabled");
            skmenu.Add(skinitem).SetValue(false);
            

            skmenu.Add(new MenuSlider("skinid", "Skin Id",1, 0, 4));
            RootMenu.Add(skmenu);

            var drmenu = new Menu("drmenu","-] Draw");
            drmenu.Add(new MenuBool("drawhpbarfill", "Draw HPBarFill")).SetValue(true);
            drmenu.Add(new MenuBool("drawmyehehe", "Draw E")).SetValue(true); // System.Drawing.Color.FromArgb(165, 0, 220, 144)));
            drmenu.Add(new MenuBool("drawmywhehe", "Draw W")).SetValue(true); // System.Drawing.Color.FromArgb(165, 37, 230, 255)));
            drmenu.Add(new MenuBool("drawmyrhehe", "Draw R")).SetValue(true); // System.Drawing.Color.FromArgb(165, 0, 220, 144)));
            RootMenu.Add(drmenu);

            RootMenu.Attach();
        }
        
        #endregion
        
        #region Damage

        private static bool EasyKill(AIBaseClient unit)
        {
            return Cdmg(unit) / 1.65 >= unit.Health;
        }

        private static double Cdmg(AIBaseClient unit)
        {
            if (unit == null)
                return 0d;

            var extraqq = new[] { 1, 1, 2, 2, 3 };
            var qcount = new[] { 2, 3, 4, 4 }[(Math.Min(Player.Level, 18) / 6)];

            qcount += (int)Math.Abs(Player.PercentCooldownMod) * 100 / 10;

            return Math.Min(qcount * extraqq[(int)(Math.Abs(Player.PercentCooldownMod) * 100 / 10)],
                    Player.Mana / Q.Mana) * Qdmg(unit, false) + Wdmg(unit) +
                        (Rdmg(Player.GetAutoAttackDamage(unit, true), unit) * qcount) + Edmg(unit);
        }

        private static double Qdmg(AIBaseClient target, bool includeq2 = true)
        {
            double dmg = 0;

            if (Q.IsReady() && target != null)
            {
                dmg += Player.CalculateDamage(target, DamageType.Physical, Player.GetAutoAttackDamage(target, true) +
                    (new[] { 0.2, 0.25, 0.30, 0.35, 0.40 }[Q.Level - 1] * (Player.BaseAttackDamage + Player.FlatPhysicalDamageMod)));

                var dmgreg = Player.CalculateDamage(target, DamageType.Physical, Player.GetAutoAttackDamage(target, true) +
                    (new[] { 0.4, 0.5, 0.6, 0.7, 0.8 }[Q.Level - 1] * (Player.BaseAttackDamage + Player.FlatPhysicalDamageMod)));

                var pct = 52 + (3 * Math.Min(16, Player.Level));

                var dmgtrue = Player.CalculateDamage(target, DamageType.True, dmgreg * pct / 100);

                if (includeq2)
                {
                    dmg += dmgtrue;
                }
            }

            return dmg;
        }

        private static double Wdmg(AIBaseClient target, bool bonus = false)
        {
            double dmg = 0;

            if (W.IsReady() && target != null)
            {
                dmg += Player.CalculateDamage(target, DamageType.Physical,
                    (new[] { 65, 95, 125, 155, 185 }[W.Level - 1] + (0.6 * Player.FlatPhysicalDamageMod)));

                var wpc = new[] { 6, 6.5, 7, 7.5, 8 };
                var pct = wpc[W.Level - 1];

                if (Player.FlatPhysicalDamageMod >= 100)
                    pct += Math.Min(300, Player.FlatPhysicalDamageMod) * 3 / 100;

                if (bonus && target.Distance(Player.ServerPosition) > 400)
                    dmg += Player.CalculateDamage(target, DamageType.Physical, pct * (target.MaxHealth / 100));
            }

            return dmg;
        }

        private static double Edmg(AIBaseClient target)
        {
            double dmg = 0;

            if (E.IsReady() && target != null)
            {
                dmg += Player.CalculateDamage(target, DamageType.Physical,
                    (new[] { 70, 115, 160, 205, 250 }[E.Level - 1] + (0.75 * Player.FlatPhysicalDamageMod)));
            }

            return dmg;
        }

        private static double Rdmg(double dmg, AIBaseClient target)
        {
            if (R.IsReady() || target.HasBuff(RBuffName))
            {
                var xtra = new[] { 5, 10, 15, 15 }[R.Level - 1] + (new[] { 4, 6, 8, 8 }[R.Level - 1] * (target.Health / 100));
                return dmg + xtra;
            }

            return dmg;
        }

        #endregion
    }
}