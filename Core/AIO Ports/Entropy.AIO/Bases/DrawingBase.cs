using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using Entropy.Lib.Render;
using SharpDX;

namespace Entropy.AIO.Bases
{
    abstract class DrawingBase
    {
        public static Dictionary<AIBaseClient, float> DamageDictionary = new Dictionary<AIBaseClient, float>();

        protected DrawingBase()
        {
            Drawing.OnDraw += OnRender;
            GameEvent.OnGameTick += OnCustomTick;
            Drawing.OnEndScene += OnEndScene;
        }

        protected Spell[] Spells { get; set; }
        
        private static MenuBool GetMenuBoolOf(SpellSlot slot)
        {
            MenuBool menuBool = null;

            switch (slot)
            {
                case SpellSlot.Q:
                    menuBool = Components.DrawingMenu.QBool;
                    break;
                case SpellSlot.W:
                    menuBool = Components.DrawingMenu.WBool;
                    break;
                case SpellSlot.E:
                    menuBool = Components.DrawingMenu.EBool;
                    break;
                case SpellSlot.R:
                    menuBool = Components.DrawingMenu.RBool;
                    break;
            }

            return menuBool;
        }
        
        private static MenuBool GetDamageMenuBoolOf(SpellSlot slot)
        {
            MenuBool menuBool = null;

            switch (slot)
            {
                case SpellSlot.Q:
                    menuBool = Components.DrawingMenu.QDamageBool;
                    break;
                case SpellSlot.W:
                    menuBool = Components.DrawingMenu.WDamageBool;
                    break;
                case SpellSlot.E:
                    menuBool = Components.DrawingMenu.EDamageBool;
                    break;
                case SpellSlot.R:
                    menuBool = Components.DrawingMenu.RDamageBool;
                    break;
            }

            return menuBool;
        }

        protected void OnRender(EventArgs args)
        {
            foreach (var spell in this.Spells)
            {
                var menuBoolOfSpell = GetMenuBoolOf(spell.Slot);

                if (menuBoolOfSpell == null || !menuBoolOfSpell.Enabled)
                {
                    continue;
                }

                var color = Color.White;

                switch (spell.Slot)
                {
                    case SpellSlot.Q:
                        color = Components.DrawingMenu.ColorQ.Color;
                        break;
                    case SpellSlot.W:
                        color = Components.DrawingMenu.ColorW.Color;
                        break;
                    case SpellSlot.E:
                        color = Components.DrawingMenu.ColorE.Color;
                        break;
                    case SpellSlot.R:
                        color = Components.DrawingMenu.ColorR.Color;
                        break;
                }

                DrawCircle(ObjectManager.Player, spell.Range, color);
            }
        }
        
        private void OnCustomTick(EventArgs args)
        {
            if (Components.DrawingMenu.SharpDXMode.Enabled)
            {
                Components.DrawingMenu.CircleThickness.Visible = true;
            }
            else
            {
                Components.DrawingMenu.CircleThickness.Visible = false;
            }

            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                if (enemy.IsValidTarget())
                {
                    var damage = this.Spells.Where(spell => GetDamageMenuBoolOf(spell.Slot).Enabled).
                        Sum(spell => spell.GetDamage(enemy));

                    if (Components.DrawingMenu.AutoDamageSliderBool.Enabled)
                    {
                        damage += (float)(Components.DrawingMenu.AutoDamageSliderBool.Value *
                                  ObjectManager.Player.GetAutoAttackDamage(enemy));
                    }

                    DamageDictionary[enemy] = damage;
                }
                else
                {
                    DamageDictionary[enemy] = 0;
                }
            }
        }
        private static void OnEndScene(EventArgs args)
        {
            foreach (var element in DamageDictionary.Where(e => e.Value > 0))
            {
                DrawDamage(element.Key, element.Value);
            }
        }
        protected static void DrawCircle(AIBaseClient obj, float radius, Color color)
        {
            if (Components.DrawingMenu.SharpDXMode.Enabled)
            {
                CircleRendering.Render(color, radius, obj, Components.DrawingMenu.CircleThickness.Value);
            }
            else
            {
                Drawing.DrawCircleIndicator(obj.Position, radius, color.ToSystemColor());
            }
        }

        protected static void DrawCircle(Vector3 worldPosition, float radius, Color color)
        {
            if (Components.DrawingMenu.SharpDXMode.Enabled)
            {
                CircleRendering.Render(color, radius, 1f, false, worldPosition);
            }
            else
            {
                Drawing.DrawCircleIndicator(worldPosition, radius, color.ToSystemColor());
            }
        }

        private static void DrawDamage(AIBaseClient target, float damage, DamageType damageType = DamageType.Mixed)
        {
            DamageIndicatorRendering.Render(target, damage, damageType,Color.White);
        }
    }
}