using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using LeagueSharpCommon;

namespace xSalice_Reworked.Base
{
    using System;
    using System.Linq;
    using LeagueSharpCommon;
    using SharpDX;
    using Base;
    using Pluging;
    using Geometry = LeagueSharpCommon.Geometry.Geometry;
    using Menu = EnsoulSharp.SDK.MenuUI.Menu;
    internal class AOESpellManager : SpellBase
    {
        private static Menu _menu;
        private static bool _qEnabled, _wEnabled, _eEnabled, _rEnabled, _qeEnabled;

        public static Menu AddHitChanceMenuCombo(bool q, bool w, bool e, bool r, bool qe = false)
        {
            _menu = new Menu("AOE Spells", "AOE Spells");
            _menu.Add(new MenuBool("enabledAOE", "Enabled",true).SetValue(true));
            if (q)
            {
                _menu.Add(new MenuSlider("qAutoLaunch", "Auto Q if hit >= Enemies", 3, 1, 5));
                _qEnabled = true;
            }
            if (w)
            {
                _menu.Add(new MenuSlider("wAutoLaunch", "Auto W if hit >= Enemies", 3, 1, 5));
                _wEnabled = true;
            }
            if (e)
            {
                _menu.Add(new MenuSlider("eAutoLaunch", "Auto E if hit >= Enemies", 3, 1, 5));
                _eEnabled = true;
            }
            if (r)
            {
                _menu.Add(new MenuSlider("rAutoLaunch", "Auto R if hit >= Enemies", 3, 1, 5));
                _rEnabled = true;
            }
            if (qe)
            {
                _menu.Add(new MenuSlider("qeAutoLaunch", "Auto QE if hit >= Enemies", 3, 1, 5));
                _qeEnabled = true;
            }

            Game.OnUpdate += Mec;
            return _menu;
        }

        private static void Mec(EventArgs args)
        {
            try
            {
                if (!_menu["AOE Spells"]["enabledAOE"].GetValue<MenuBool>().Enabled)
                {
                    Game.Print("aaaa");
                    return;
                }

                if (_qeEnabled)
                {
                    CastComboMec(QExtend, _menu["AOE Spells"]["qeAutoLaunch"].GetValue<MenuSlider>().Value);
                }

                if (_qEnabled)
                {
                    CastMec(Q, _menu["AOE Spells"]["qAutoLaunch"].GetValue<MenuSlider>().Value);
                }

                if (_wEnabled)
                {
                    CastMec(W, _menu["AOE Spells"]["wAutoLaunch"].GetValue<MenuSlider>().Value);
                }

                if (_eEnabled)
                {
                    CastMec(E, _menu["AOE Spells"]["eAutoLaunch"].GetValue<MenuSlider>().Value);
                }

                if (_rEnabled)
                {
                    CastMec(R, _menu["AOE Spells"]["rAutoLaunch"].GetValue<MenuSlider>().Value);
                }
            }catch(Exception e){}

        }
        private static void CastMec(Spell spell, int minHit)
        {
            if (!spell.IsReady() || ObjectManager.Player.HealthPercent <= 10)
            {
                return;
            }

            foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(spell.Range)))
            {
                var pred = spell.GetPrediction(target, true);
                var nearByEnemies = 1;

                if (spell.Type == SpellType.Line && spell.Collision)
                {
                    var poly = new Geometry.Polygon.Circle((Vector2)pred.UnitPosition, spell.Width);

                    nearByEnemies +=
                        HeroManager.Enemies.Where(x => x.NetworkId != target.NetworkId)
                            .Count(enemy => poly.IsInside(enemy.ServerPosition));

                }
                else
                {
                    nearByEnemies = pred.AoeTargetsHitCount;
                }

                if (nearByEnemies >= minHit)
                {
                    spell.Cast(target);
                    return;
                }
            }
        }
        private static void CastComboMec(Spell spell, int minHit)
        {
            if (!spell.IsReady() || !E.IsReady() || ObjectManager.Player.HealthPercent <= 10)
            {
                return;
            }

            const int gateDis = 200;

            foreach (var target in ObjectManager.Get<AIHeroClient>().Where(x => x.IsValidTarget(spell.Range)))
            {
                var tarPred = spell.GetPrediction(target, true); 
                var gateVector = ObjectManager.Player.Position + Vector3.Normalize(target.ServerPosition - ObjectManager.Player.Position)*gateDis;
                var nearByEnemies = 1;
                var poly = new Geometry.Polygon.Circle(tarPred.UnitPosition, spell.Width);

                nearByEnemies += HeroManager.Enemies.Where(x => x.NetworkId != target.NetworkId).Count(enemy => poly.IsInside(enemy.ServerPosition));

                if (ObjectManager.Player.Distance(tarPred.CastPosition) < spell.Range + 100 && nearByEnemies >= minHit)
                {
                    /*if (Jayce.IsMelee && R.IsReady() && Jayce.Qcd == 0 && Jayce.Ecd == 0)
                    {
                        R.Cast();
                    }
                    else if (Jayce.IsMelee)
                    {
                        return;
                    }

                    E.Cast(gateVector);
                    spell.Cast(tarPred.CastPosition);*/
                    return;
                }
            }
        }
    }
}