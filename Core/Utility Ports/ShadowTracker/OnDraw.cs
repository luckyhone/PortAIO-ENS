﻿using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Rendering;

namespace ShadowTracker
{
    class OnDraw
    {
        private static AIHeroClient Player => ObjectManager.Player;

        public static void Drawing_OnDraw(EventArgs args)
        {
            try
            {
                foreach (var item in Program.TrackInfomationList.Where(x => x.InfoType == InfoType.MovingSkill && x.ExpireTime > Environment.TickCount))
                {
                    //Render.Circle.DrawCircle(item.StartPosition, 100, Color.YellowGreen);
                    CircleRender.Draw(item.EndPosition, 30, SharpDX.Color.YellowGreen);                    

                    var StartScreenPos = Drawing.WorldToScreen(item.StartPosition);
                    var EndScreenPos = Drawing.WorldToScreen(item.EndPosition);
                    Drawing.DrawText(EndScreenPos.X, EndScreenPos.Y, Color.LightYellow, item.Sender.SkinName);
                    Drawing.DrawLine(StartScreenPos, EndScreenPos, 2, Color.YellowGreen);
                }

                foreach (var item in Program.StopTrackInfomationList.Where(x => x.InfoType == InfoType.StopSkill && x.ExpireTime > Environment.TickCount))
                {
                    CircleRender.Draw(item.Sender.Position, 100, SharpDX.Color.YellowGreen);
                    var TextPosition = Drawing.WorldToScreen(item.CastPosition);
                    Drawing.DrawText(TextPosition.X, TextPosition.Y, Color.LightYellow, item.Sender.SkinName);
                    Drawing.DrawText(TextPosition.X - 20, TextPosition.Y + 15, Color.LawnGreen, (item.ExpireTime - Environment.TickCount).ToString());
                }

                foreach (var item in Program.UsingItemInfomationList.Where(x => x.InfoType == InfoType.UsingItem && x.ExpireTime > Environment.TickCount))
                {
                    var TextPosition = Drawing.WorldToScreen(item.Sender.Position);
                    Drawing.DrawText(TextPosition.X - 20, TextPosition.Y + 15, Color.LawnGreen, (item.ExpireTime - Environment.TickCount).ToString());
                }

                foreach (var item in Program.PetSkillInfoList)
                {
                    var enemy = GameObjects.EnemyHeroes.Find(x => x.CharacterName == item.ChampionName);
                    if(enemy != null && enemy.Pet != null && !enemy.Pet.IsDead)
                    {
                        CircleRender.Draw(enemy.Position, 50, SharpDX.Color.Red);
                        var TextPosition = Drawing.WorldToScreen(enemy.Position);
                        Drawing.DrawText(TextPosition.X-20, TextPosition.Y+30, Color.LawnGreen, "Here");
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e);
                Game.Print("ShadowTracker is not working. plz send message by KorFresh (Code 5)");
            }
        }
    }
}