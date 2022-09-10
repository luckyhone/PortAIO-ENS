using System;
using System.Collections.Generic;
using System.Linq;
using Challenger_Series.Utils;
using EnsoulSharp;
using EnsoulSharp.SDK;
using LeagueSharpCommon;
using PortAIO;
using SharpDX;
using Geometry = LeagueSharpCommon.Geometry.Geometry;

namespace hi_im_gosu_Reborn
{
    public static class Extensions
    {
        private static AIHeroClient Player = ObjectManager.Player;

       
           // if (!hero.IsValidTarget(550f) || hero.HasBuffOfType(BuffType.SpellShield) ||
               // hero.HasBuffOfType(BuffType.SpellImmunity) || hero.IsDashing()) return false;

            

           

         
        public static Vector3 GetTumblePos(this AIBaseClient target)
        {
            var cursorPos = Game.CursorPos;

            if (!cursorPos.IsDangerousPosition()) return cursorPos;
            //if the target is not a melee and he's alone he's not really a danger to us, proceed to 1v1 him :^ )
            if (!target.IsMelee && Heroes.Player.CountEnemyHeroesInRange(800) == 1) return cursorPos;

            var aRC = new Geometry.Polygon.Circle(Heroes.Player.ServerPosition.To2D(), 300).ToClipperPath();
            var targetPosition = target.ServerPosition;


            foreach (var p in aRC)
            {
                var v3 = new Vector2(p.X, p.Y).To3D();
                var dist = v3.Distance(targetPosition);
                if (dist > 325 && dist < 450)
                {
                    return v3;
                }
            }
            return Vector3.Zero;
        }
        

            public static class Heroes
        {
            private static List<AIHeroClient> _heroes;

            public static AIHeroClient Player = ObjectManager.Player;

            public static List<AIHeroClient> AllyHeroes
            {
                get { return _heroes.FindAll(h => h.IsAlly); }
            }

            public static List<AIHeroClient> EnemyHeroes
            {
                get { return _heroes.FindAll(h => h.IsEnemy); }
            }

            public static void Load()
            {
                Player = ObjectManager.Player;
                _heroes = ObjectManager.Get<AIHeroClient>().ToList();
            }
        }



        public static int VayneWStacks(this AIBaseClient o)
        {
            if (o == null) return 0;
            if (o.Buffs.FirstOrDefault(b => b.Name.Contains("vaynesilver")) == null || !o.Buffs.Any(b => b.Name.Contains("vaynesilver"))) return 0;
            return o.Buffs.FirstOrDefault(b => b.Name.Contains("vaynesilver")).Count;
        }

        public static Vector3 Randomize(this Vector3 pos)
        {
            var r = new Random(Environment.TickCount);
            return new Vector2(pos.X + r.Next(-150, 150), pos.Y + r.Next(-150, 150)).To3D();
        }

        public static bool IsDangerousPosition(this Vector3 pos)
        {
            return
                HeroManager.Enemies.Any(
                    e => AttackUnitExtensions.IsValidTarget(e) && e.IsVisible &&
                        e.Distance(pos) < 375) ||
                Traps.EnemyTraps.Any(t => pos.Distance(t.Position) < 125) ||
                (pos.UnderTurret(true) && !Player.UnderTurret(true)) || Utility.IsWall(pos);
        }

        public static bool IsKillable(this AIHeroClient hero)
        {
            return Player.GetAutoAttackDamage(hero) * 2 < hero.Health;
        }

        public static bool IsCollisionable(this Vector3 pos)
        {
            return NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Wall) ||
                (NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Building));
        }
        public static bool IsValidState(this AIHeroClient target)
        {
            return !target.HasBuffOfType(BuffType.SpellShield) && !target.HasBuffOfType(BuffType.SpellImmunity) &&
                   !target.HasBuffOfType(BuffType.Invulnerability);
        }

        public static int CountHerosInRange(this AIHeroClient target, bool checkteam, float range = 1200f)
        {
            var objListTeam =
                ObjectManager.Get<AIHeroClient>()
                    .Where(
                        x => x.IsValidTarget(range, false));

            return objListTeam.Count(hero => checkteam ? hero.Team != target.Team : hero.Team == target.Team);
        }
    }
}