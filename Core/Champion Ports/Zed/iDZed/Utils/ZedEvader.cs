using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using Evade;
using LeagueSharpCommon;
using SharpDX;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
namespace iDZed.Utils
{
    internal static class ZedEvader
    {
        public static readonly List<Skillshot> DetectedSkillShots = new List<Skillshot>();
        private static readonly List<Skillshot> EvadeDetectedSkillshots = new List<Skillshot>();
        //todo Orianna ult, Malphite ult, Annie ult, Scarner Ult (if possible), Riven Ult, Lissandra Ult, Chogath Ult, Veigar Ult, Nautilus Ult, Cassiopeia Ult, Gnar Ult, Katarina Ult...
        private static readonly List<string> DangerousList = new List<string>
            // Credits to Jack is back :), thanks jack! saved me some time.
        {
            "CurseoftheSadMummy",
            "InfernalGuardian",
            "EnchantedCrystalArrow",
            "AzirR",
            "BrandWildfire",
            "DariusExecute",
            "DravenRCast",
            "EvelynnR",
            "Terrify",
            "GalioIdolOfDurand",
            "GarenR",
            "GravesChargeShot",
            "HecarimUlt",
            "LissandraR",
            "LuxMaliceCannon",
            "UFSlash",
            "AlZaharNetherGrasp",
            "OrianaDetonateCommand",
            "LeonaSolarFlare",
            "SejuaniGlacialPrisonStart",
            "SonaCrescendo",
            "VarusR",
            "GragasR",
            "GnarR",
            "FizzMarinerDoom",
            "SyndraR",
            "CaitlynAceintheHole",
            "rivenizunablade",
            "TristanaR"
        };

        private static Menu localMenu;

        public static void OnLoad(Menu menu)
        {
            localMenu = menu;
            Menu dodgeMenu = new Menu( "com.idz.zed.spelldodging",":: Spell Dodging");

            //create dangerous spells
            Menu dangerSpellMenu =
                dodgeMenu.Add(new Menu( "com.idz.zed.spelldodging.dangerous","Dangerous Spells"));
            {
                foreach (
                    AIHeroClient hero in
                        HeroManager.Enemies.Where(
                            hero => DangerousList.Contains(hero.Spellbook.GetSpell(SpellSlot.R).SData.Name)))
                {
                    dangerSpellMenu.Add(
                        new MenuBool(
                            "com.idz.zed.spelldodging.dangerous.dodge" + hero.Spellbook.GetSpell(SpellSlot.R).SData.Name,
                            "Dodge: " + hero.Spellbook.GetSpell(SpellSlot.R).Name).SetValue(true));
                }
            }

            dodgeMenu.Add(
                new MenuBool("com.idz.zed.spelldodging.dodgeSwap", "Swap with W shadow for dangerous skillshots")
                    .SetValue(false));
            dodgeMenu.Add(
                new MenuSlider("com.idz.zed.spelldodging.customDangerValue", "-> W Shadow Danger Value",3, 1, 5));
            dodgeMenu.Add(
                new MenuBool("com.idz.zed.spelldodging.useUltDodge", "Use Dangerous spells with R").SetValue(true));

            menu.Add(dodgeMenu);

            //Events.
            Game.OnUpdate += OnGameUpdate;
            GameObject.OnCreate += OnCreateObject;
            AIBaseClient.OnDoCast += OnSpellCast;
            SkillshotDetector.OnDetectSkillshot += OnDetectSkillshot;
            SkillshotDetector.OnDeleteMissile += OnDeleteMissile;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            EvadeDetectedSkillshots.RemoveAll(skillshot => !skillshot.IsActive());
            if (localMenu["com.idz.zed.spelldodging"]["com.idz.zed.spelldodging.dodgeSwap"].GetValue<MenuBool>().Enabled)
            {
                foreach (Skillshot skillshot in EvadeDetectedSkillshots)
                {
                    //Chat.Print(""+skillshot.SpellData.SpellName);
                    if (ShadowManager.CanGoToShadow(ShadowManager.WShadow) && Zed.WShadowSpell.ToggleState == (SpellToggleState)2 &&
                        skillshot.IsAboutToHit(200, ObjectManager.Player))
                    {
                        
                        if (skillshot.SpellData.IsDangerous &&
                            skillshot.SpellData.DangerValue >=
                            localMenu["com.idz.zed.spelldodging"]["com.idz.zed.spelldodging.customDangerValue"].GetValue<MenuSlider>().Value ||
                             15 > ObjectManager.Player.Health)
                        {
                            if (skillshot.IsSafe(ShadowManager.WShadow.ShadowObject.ServerPosition.ToVector2()) &&
                                Zed._spells[SpellSlot.W].IsReady())
                            {
                                Zed._spells[SpellSlot.W].Cast();
                            }
                        }

                        if (!ShadowManager.RShadow.IsUsable && !ShadowManager.RShadow.Exists &&
                            localMenu["com.idz.zed.spelldodging"]["com.idz.zed.spelldodging.dangerous.dodge" + skillshot.SpellData.SpellName].GetValue<MenuBool>().Enabled)
                        {
                            if (DangerousList.Any(spell => spell.Contains(skillshot.SpellData.SpellName)))
                            {
                                if (skillshot.IsSafe(ShadowManager.WShadow.ShadowObject.ServerPosition.ToVector2()) &&
                                    Zed._spells[SpellSlot.W].IsReady())
                                {
                                    Zed._spells[SpellSlot.W].Cast();
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void OnSpellCast(AIBaseClient sender1, AIBaseClientProcessSpellCastEventArgs args)
        {
            try
            {
                AIHeroClient sender = sender1 as AIHeroClient;
                if (sender == null || !sender.IsEnemy || sender.Team == ObjectManager.Player.Team)
                {
                    return;
                }

                if (localMenu["com.idz.zed.spelldodging"]["com.idz.zed.spelldodging.useUltDodge"].GetValue<MenuBool>()
                        .Enabled &&
                    localMenu["com.idz.zed.spelldodging"]["com.idz.zed.spelldodging.dangerous.dodge" + args.SData.Name]
                        .GetValue<MenuBool>().Enabled)
                {
                    if (Zed._spells[SpellSlot.R].IsReady() &&
                        DangerousList.Any(spell => spell.Contains(args.SData.Name)) &&
                        ShadowManager.RShadow.IsUsable)
                    {
                        if (Zed._spells[SpellSlot.R].IsInRange(sender) || ObjectManager.Player.Distance(args.End) < 250)
                        {
                            if (args.SData.Name == "SyndraR" || args.SData.Name == "TristanaR" ||
                                args.SData.Name == "BrandWildfire" && args.Target.IsMe)
                            {
                                DelayAction.Add((int)0.25, () => Zed._spells[SpellSlot.R].Cast());
                            }
                            else
                            {
                                Zed._spells[SpellSlot.R].Cast(sender);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignorte  
            }
        }

        private static void OnCreateObject(GameObject sender, EventArgs args1)
        {
            if (!(sender is MissileClient) || !sender.IsValid)
            {
                return;
            }

            MissileClient args = (MissileClient) sender;

            AIHeroClient selectedTarget =
                HeroManager.Enemies.FirstOrDefault(
                    x => Zed._spells[SpellSlot.R].IsInRange(x) && x.IsValidTarget(Zed._spells[SpellSlot.R].Range));

            if (args.SData.Name == "CaitlynAceintheHoleMissile" && args.Type == GameObjectType.MissileClient &&
                args.Target.IsMe && localMenu["com.idz.zed.spelldodging"]["com.idz.zed.spelldodging.useUltDodge"].GetValue<MenuBool>().Enabled)
            {
                if (selectedTarget != null && ShadowManager.RShadow.IsUsable)
                {
                    DelayAction.Add(
                        ((int)
                            (args.StartPosition.Distance(ObjectManager.Player.ServerPosition) / 2000f + Game.Ping / 2f)),
                        () => Zed._spells[SpellSlot.R].Cast(selectedTarget));
                }
            }
        }

        #region skillshot detection

        private static void OnDetectSkillshot(Skillshot skillshot)
        {
            //Check if the skillshot is already added.
            var alreadyAdded = false;
            foreach (
                Skillshot item in
                    EvadeDetectedSkillshots.Where(
                        item =>
                            item.SpellData.SpellName == skillshot.SpellData.SpellName &&
                            (item.Unit.NetworkId == skillshot.Unit.NetworkId &&
                             (skillshot.Direction).AngleBetween(item.Direction) < 5 &&
                             (skillshot.Start.Distance(item.Start) < 100 || skillshot.SpellData.FromObjects.Length == 0)))
                )
            {
                alreadyAdded = true;
            }
            //Check if the skillshot is from an ally.
            if (skillshot.Unit.Team == ObjectManager.Player.Team)
            {
                return;
            }
            //Check if the skillshot is too far away.
            if (skillshot.Start.Distance(ObjectManager.Player.ServerPosition.ToVector2()) >
                (skillshot.SpellData.Range + skillshot.SpellData.Radius + 1000) * 1.5)
            {
                return;
            }
            //Add the skillshot to the detected skillshot list.
            if (!alreadyAdded)
            {
                //Multiple skillshots like twisted fate _spells[Spells.Q].
                if (skillshot.DetectionType == DetectionType.ProcessSpell)
                {
                    if (skillshot.SpellData.MultipleNumber != -1)
                    {
                        var originalDirection = skillshot.Direction;
                        for (var i = -(skillshot.SpellData.MultipleNumber - 1) / 2;
                            i <= (skillshot.SpellData.MultipleNumber - 1) / 2;
                            i++)
                        {
                            var end = skillshot.Start +
                                      skillshot.SpellData.Range *
                                      originalDirection.Rotated(skillshot.SpellData.MultipleAngle * i);
                            var skillshotToAdd = new Skillshot(
                                skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end,
                                skillshot.Unit);
                            EvadeDetectedSkillshots.Add(skillshotToAdd);
                        }
                        return;
                    }
                    if (skillshot.SpellData.SpellName == "UFSlash")
                    {
                        skillshot.SpellData.MissileSpeed = 1600 + (int) skillshot.Unit.MoveSpeed;
                    }
                    if (skillshot.SpellData.Invert)
                    {
                        var newDirection = -(skillshot.End - skillshot.Start).Normalized();
                        var end = skillshot.Start + newDirection * skillshot.Start.Distance(skillshot.End);
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end,
                            skillshot.Unit);
                        EvadeDetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }
                    if (skillshot.SpellData.Centered)
                    {
                        var start = skillshot.Start - skillshot.Direction * skillshot.SpellData.Range;
                        var end = skillshot.Start + skillshot.Direction * skillshot.SpellData.Range;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit);
                        EvadeDetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }
                    if (skillshot.SpellData.SpellName == "SyndraE" || skillshot.SpellData.SpellName == "syndrae5")
                    {
                        const int angle = 60;
                        const int fraction = -angle / 2;
                        var edge1 =
                            (skillshot.End - skillshot.Unit.ServerPosition.ToVector2()).Rotated(
                                fraction * (float) Math.PI / 180);
                        var edge2 = edge1.Rotated(angle * (float) Math.PI / 180);
                        foreach (var minion in ObjectManager.Get<AIMinionClient>())
                        {
                            var v = minion.ServerPosition.ToVector2() - skillshot.Unit.ServerPosition.ToVector2();
                            if (minion.Name == "Seed" && edge1.CrossProduct(v) > 0 && v.CrossProduct(edge2) > 0 &&
                                minion.Distance(skillshot.Unit) < 800 && (minion.Team != ObjectManager.Player.Team))
                            {
                                var start = minion.ServerPosition.ToVector2();
                                var end = skillshot.Unit.ServerPosition.ToVector2()
                                    .Extend(
                                        minion.ServerPosition.ToVector2(),
                                        skillshot.Unit.Distance(minion) > 200 ? 1300 : 1000);
                                var skillshotToAdd = new Skillshot(
                                    skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                                    skillshot.Unit);
                                EvadeDetectedSkillshots.Add(skillshotToAdd);
                            }
                        }
                        return;
                    }
                    if (skillshot.SpellData.SpellName == "AlZaharCalloftheVoid")
                    {
                        var start = skillshot.End - skillshot.Direction.Perpendicular() * 400;
                        var end = skillshot.End + skillshot.Direction.Perpendicular() * 400;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit);
                        EvadeDetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }
                    if (skillshot.SpellData.SpellName == "ZiggsQ")
                    {
                        var d1 = skillshot.Start.Distance(skillshot.End);
                        var d2 = d1 * 0.4f;
                        var d3 = d2 * 0.69f;
                        var bounce1SpellData = SpellDatabase.GetByName("ZiggsQBounce1");
                        var bounce2SpellData = SpellDatabase.GetByName("ZiggsQBounce2");
                        var bounce1Pos = skillshot.End + skillshot.Direction * d2;
                        var bounce2Pos = bounce1Pos + skillshot.Direction * d3;
                        bounce1SpellData.Delay =
                            (int) (skillshot.SpellData.Delay + d1 * 1000f / skillshot.SpellData.MissileSpeed + 500);
                        bounce2SpellData.Delay =
                            (int) (bounce1SpellData.Delay + d2 * 1000f / bounce1SpellData.MissileSpeed + 500);
                        var bounce1 = new Skillshot(
                            skillshot.DetectionType, bounce1SpellData, skillshot.StartTick, skillshot.End, bounce1Pos,
                            skillshot.Unit);
                        var bounce2 = new Skillshot(
                            skillshot.DetectionType, bounce2SpellData, skillshot.StartTick, bounce1Pos, bounce2Pos,
                            skillshot.Unit);
                        EvadeDetectedSkillshots.Add(bounce1);
                        EvadeDetectedSkillshots.Add(bounce2);
                    }
                    if (skillshot.SpellData.SpellName == "ZiggsR")
                    {
                        skillshot.SpellData.Delay =
                            (int) (1500 + 1500 * skillshot.End.Distance(skillshot.Start) / skillshot.SpellData.Range);
                    }
                    if (skillshot.SpellData.SpellName == "JarvanIVDragonStrike")
                    {
                        var endPos = new Vector2();
                        foreach (var s in EvadeDetectedSkillshots)
                        {
                            if (s.Unit.NetworkId == skillshot.Unit.NetworkId && s.SpellData.Slot == SpellSlot.E)
                            {
                                endPos = s.End;
                            }
                        }
                        foreach (var m in ObjectManager.Get<AIMinionClient>())
                        {
                            if (m.Name == "jarvanivstandard" && m.Team == skillshot.Unit.Team &&
                                skillshot.IsDanger(m.Position.ToVector2()))
                            {
                                endPos = m.Position.ToVector2();
                            }
                        }
                        if (!endPos.IsValid())
                        {
                            return;
                        }
                        skillshot.End = endPos + 200 * (endPos - skillshot.Start).Normalized();
                        skillshot.Direction = (skillshot.End - skillshot.Start).Normalized();
                    }
                }
                if (skillshot.SpellData.SpellName == "OriannasQ")
                {
                    var endCSpellData = SpellDatabase.GetByName("OriannaQend");
                    var skillshotToAdd = new Skillshot(
                        skillshot.DetectionType, endCSpellData, skillshot.StartTick, skillshot.Start, skillshot.End,
                        skillshot.Unit);
                    EvadeDetectedSkillshots.Add(skillshotToAdd);
                }
                //Dont allow fow detection.
                if (skillshot.SpellData.DisableFowDetection && skillshot.DetectionType == DetectionType.RecvPacket)
                {
                    return;
                }
                EvadeDetectedSkillshots.Add(skillshot);
            }
        }

        private static void OnDeleteMissile(Skillshot skillshot, MissileClient missile)
        {
            if (skillshot.SpellData.SpellName == "VelkozQ")
            {
                var spellData = SpellDatabase.GetByName("VelkozQSplit");
                var direction = skillshot.Direction.Perpendicular();
                if (EvadeDetectedSkillshots.Count(s => s.SpellData.SpellName == "VelkozQSplit") == 0)
                {
                    for (var i = -1; i <= 1; i = i + 2)
                    {
                        Skillshot skillshotToAdd = new Skillshot(
                            DetectionType.ProcessSpell, spellData, Environment.TickCount, missile.Position.ToVector2(),
                            missile.Position.ToVector2() + i * direction * spellData.Range, skillshot.Unit);
                        EvadeDetectedSkillshots.Add(skillshotToAdd);
                    }
                }
            }
        }

        #endregion
    }
}