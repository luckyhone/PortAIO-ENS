using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using hikiMarksmanRework.Core.Drawings;
using hikiMarksmanRework.Core.Menus;
using hikiMarksmanRework.Core.Spells;
using hikiMarksmanRework.Core.Utilitys;
using LeagueSharpCommon;
using SharpDX;

using PortAIO.Properties;
using SPrediction;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using Render = LeagueSharpCommon.Render;

namespace hikiMarksmanRework.Champions
{
    public class Corki
    {
        public Corki()
        {
            CorkiOnLoad();
        }
        private static readonly Render.Sprite HikiSprite = new Render.Sprite(Resources.logo, new Vector2((Drawing.Width / 2) - 500, (Drawing.Height / 2) - 350));
        private static void CorkiOnLoad()
        {
            CorkiMenu.Config =
                new Menu("hikiMarksman:AIO - Corki", "hikiMarksman:AIO - Corki", true);
            {
                CorkiSpells.Init();
                CorkiMenu.OrbwalkerInit();
                CorkiMenu.MenuInit();
            }
            CorkiMenu.Config.SetFontColor(SharpDX.Color.Gold);

            Game.Print("<font color='#ff3232'>hikiMarksman:AIO - Corki: </font><font color='#00FF00'>loaded! You can rekt everyone with this assembly</font>", ObjectManager.Player.CharacterName);
            Game.Print(string.Format("<font color='#ff3232'>hikiMarksman:AIO - </font><font color='#00FF00'>Assembly Version: </font><font color='#ff3232'><b>{0}</b></font> ", typeof(Program).Assembly.GetName().Version));
            Game.Print("<font color='#ff3232'>If you like this assembly feel free to upvote on Assembly Database</font>");


            HikiSprite.Add(0);
            HikiSprite.OnDraw();
            DelayAction.Add(8000, () => HikiSprite.Remove());

            /*Notifications.AddNotification("hikiMarksman:AIO", 4000);
            Notifications.AddNotification(String.Format("{0} Loaded", ObjectManager.Player.ChampionName), 5000);
            Notifications.AddNotification("Gift From Hikigaya", 6000);*/

            Game.OnUpdate += CorkiOnUpdate;
            AIBaseClient.OnProcessSpellCast += CorkiOnProcessSpellCast;
            Orbwalker.OnAfterAttack += CorkiAfterAttack;
            Drawing.OnDraw += CorkiOnDraw;
        }

        private static void CorkiOnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    Clear();
                    Jungle();
                    break;
            }
        }

        private static void Combo()
        {
            if (CorkiSpells.Q.IsReady() && Helper.CEnabled("corki.q.combo"))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(CorkiSpells.Q.Range) &&
                    CorkiSpells.Q.GetPrediction(x).Hitchance >= HitChance.High))
                {
                    CorkiSpells.Q.Cast(enemy);
                }
            }
            if (CorkiSpells.R.IsReady() && Helper.CEnabled("corki.r.combo"))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(CorkiSpells.R.Range) &&
                    CorkiSpells.R.GetPrediction(x).Hitchance >= HitChance.High))
                {
                    CorkiSpells.R.Cast(enemy);
                }
            }
            if (CorkiSpells.R.IsReady() && Helper.CEnabled("corki.r.combo"))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(CorkiSpells.BIG.Range) &&
                    CorkiSpells.BIG.GetPrediction(x).Hitchance >= HitChance.High))
                {
                    CorkiSpells.BIG.Cast(enemy);
                }
            }
        }

        private static void Harass()
        {
            if (ObjectManager.Player.ManaPercent < Helper.CSlider("corki.harass.mana"))
            {
                return;
            }
            if (CorkiSpells.Q.IsReady() && Helper.CEnabled("corki.q.harass"))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(CorkiSpells.Q.Range) &&
                    CorkiSpells.Q.GetPrediction(x).Hitchance >= HitChance.High))
                {
                    CorkiSpells.Q.Cast(enemy);
                }
            }
            if (CorkiSpells.R.IsReady() && Helper.CEnabled("corki.r.harass"))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(CorkiSpells.R.Range) &&
                    CorkiSpells.R.GetPrediction(x).Hitchance >= HitChance.High))
                {
                    CorkiSpells.R.Cast(enemy);
                }
            }
            if (CorkiSpells.R.IsReady() && Helper.CEnabled("corki.r.harass"))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(CorkiSpells.BIG.Range) &&
                    CorkiSpells.BIG.GetPrediction(x).Hitchance >= HitChance.High))
                {
                    CorkiSpells.BIG.Cast(enemy);
                }
            }
        }

        private static void Clear()
        {
            if (ObjectManager.Player.ManaPercent < Helper.CSlider("corki.clear.mana"))
            {
                return;
            }

            if (CorkiSpells.Q.GetLineFarmLocation(MinionManager.GetMinions(ObjectManager.Player.Position, CorkiSpells.Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly)).MinionsHit >= Helper.CSlider("corki.q.minion.hit.count"))
            {
                CorkiSpells.Q.Cast(CorkiSpells.Q.GetLineFarmLocation(MinionManager.GetMinions(ObjectManager.Player.Position, CorkiSpells.Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly)).Position);
            }
        }

        private static void Jungle()
        {
            if (ObjectManager.Player.ManaPercent < Helper.CSlider("corki.jungle.mana") && MinionManager.GetMinions(ObjectManager.Player.ServerPosition, ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth) == null ||
                MinionManager.GetMinions(ObjectManager.Player.ServerPosition, ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth).Count == 0)
            {
                return;
            }

            if (CorkiSpells.Q.IsReady() && Helper.CEnabled("corki.q.jungle"))
            {
                CorkiSpells.Q.Cast(
                    MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                        ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All,
                        MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth)[0].Position);
            }

            if (CorkiSpells.E.IsReady() && Helper.CEnabled("corki.e.jungle"))
            {
                CorkiSpells.E.Cast(
                    MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                        ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All,
                        MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth)[0].Position);
            }

            if (CorkiSpells.R.IsReady() && Helper.CEnabled("corki.r.jungle"))
            {
                CorkiSpells.R.Cast(
                    MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                        ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All,
                        MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth)[0].Position);
            }
        }

        private static void CorkiOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (CorkiMenu.Config["Miscellaneous"]["Anti-Gapclose Settings"]["corki.w.gapclosex"].GetValue<MenuList>().Index == 0)
            {
                Helper.CorkiAntiGapcloser(sender, args);
            }
        }

        private static void CorkiAfterAttack(object unit, AfterAttackEventArgs args)
        {
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && Helper.CEnabled("corki.e.combo") && CorkiSpells.E.IsReady()
                && args.Target.IsValidTarget(CorkiSpells.E.Range-100))
            {
                CorkiSpells.E.Cast(args.Target.Position);
            }
        }

        private static void CorkiOnDraw(EventArgs args)
        {
            CorkiDrawing.Init();
        }
    }
}
