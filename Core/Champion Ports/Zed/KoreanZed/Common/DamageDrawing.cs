﻿using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Rendering;
using PortAIO;
using SharpDX;
using Render = LeagueSharpCommon.Render;

namespace KoreanZed.Common
{
    using Color = System.Drawing.Color;
    class CommonDamageDrawing
    {
        public delegate float DrawDamageDelegate(AIHeroClient hero);

        private const int XOffset = 10;
        private static int YOffset = 20;
        private const int Width = 103;
        private const int Height = 11;

        private readonly Render.Text killableText = new Render.Text(0, 0, "KILLABLE", 20, new ColorBGRA(255, 0, 0, 255));

        private DrawDamageDelegate amountOfDamage;

        public bool Active = true;

        private readonly ZedMenu zedMenu;

        public CommonDamageDrawing(ZedMenu zedMenu)
        {
            this.zedMenu = zedMenu;
        }

        public DrawDamageDelegate AmountOfDamage
        {
            get
            {
                return amountOfDamage;
            }

            set
            {
                if (amountOfDamage == null)
                {
                    Drawing.OnEndScene += DrawDamage;
                }
                amountOfDamage = value;
            }
        }

        private bool Enabled()
        {
            return ((Active) && (amountOfDamage != null)
                    && ((zedMenu.GetParamBool("koreanzed.drawing.damageindicator"))
                        || zedMenu.GetParamBool("koreanzed.drawing.killableindicator")));
        }

        private void DrawDamage(EventArgs args)
        {
            Color color = Color.Gray;
            Color barColor = Color.White;

            if (zedMenu.GetParamStringList("koreanzed.drawing.damageindicatorcolor") == 1)
            {
                color = Color.Gold;
                barColor = Color.Olive;
            }
            else if (zedMenu.GetParamStringList("koreanzed.drawing.damageindicatorcolor") == 2)
            {
                color = Color.FromArgb(100, Color.Black);
                barColor = Color.Lime;
            }
            


            if (Enabled())
            {
                foreach (
                    AIHeroClient champ in
                        ObjectManager.Get<AIHeroClient>()
                            .Where(h => h.IsVisible && h.IsEnemy && h.IsValid && h.IsHPBarRendered))
                {
                    float damage = amountOfDamage(champ);

                    if (damage > 0 && !champ.IsDead)
                    {
                        Vector2 pos = champ.HPBarPosition - new Vector2(55,45);

                        if (zedMenu.GetParamBool("koreanzed.drawing.killableindicator")
                            && (damage > champ.Health + 50f))
                        {
                            Render.Circle.DrawCircle(champ.Position, 100, Color.Red);
                            Render.Circle.DrawCircle(champ.Position, 75, Color.Red);
                            Render.Circle.DrawCircle(champ.Position, 50, Color.Red);
                            killableText.X = (int)pos.X + 40;
                            killableText.Y = (int)pos.Y - 20;
                            killableText.OnEndScene();
                        }

                        if (zedMenu.GetParamBool("koreanzed.drawing.damageindicator"))
                        {
                            float healthAfterDamage = Math.Max(0, champ.Health - damage) / champ.MaxHealth;
                            float posY = pos.Y + YOffset;
                            float posDamageX = pos.X + XOffset + Width * healthAfterDamage;
                            float posCurrHealthX = pos.X + XOffset + Width * champ.Health / champ.MaxHealth;

                            float diff = (posCurrHealthX - posDamageX) + 3;

                            float pos1 = pos.X + 8 + (107 * healthAfterDamage);

                            for (int i = 0; i < diff-3; i++)
                            {
                                Drawing.DrawLine(pos1 + i, posY, pos1 + i, posY + Height, 1, color);
                            }

                            Drawing.DrawLine(posDamageX, posY, posDamageX, posY + Height, 2, barColor);
                        }
                    }
                }
            }
        }
    }
}