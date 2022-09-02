using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
using Color = System.Drawing.Color;

namespace StormAIO.utilities
{
    public static class Helper
    {
        private static AIHeroClient Player => ObjectManager.Player;

        /// <summary>
        ///     Returns the current AttackSpeed state of Player.
        /// </summary>
        /// <returns></returns>
        public static float CurrentAttackSpeed(double BaseAttackSpeed)
        {
            return (float) (BaseAttackSpeed * Player.AttackSpeedMod);
        }

        /// <summary>
        ///     Checks if the user is recalling or typing or isdead.
        /// </summary>
        /// <returns></returns>
        public static bool Checker()
        {
            if (Player.IsRecalling() || Player.IsDead || MenuGUI.IsChatOpen) return true;
            return false;
        }

        /// <summary>
        ///     Draws Damage Indicator
        /// </summary>
        /// <returns></returns>
        public static void Indicator(float damage)
        {
            foreach (var target in GameObjects.EnemyHeroes.Where(x =>
                x.IsValidTarget(2000) && !x.IsDead && x.IsHPBarRendered))
            {
                var pos = Drawing.WorldToScreen(target.Position);

                if (!pos.IsOnScreen())
                    return;

                var hpBar = target.HPBarPosition;
                var damagePercentage =
                    (target.Health - damage > 0 ? target.Health - damage : 0) / target.MaxHealth;
                var currentHealthPercentage = target.Health / target.MaxHealth;

                var startPoint = new Vector2(hpBar.X - 45 + damagePercentage * 104, hpBar.Y - 18);
                var endPoint = new Vector2(hpBar.X - 45 + currentHealthPercentage * 104, hpBar.Y - 18);

                Drawing.DrawLine(startPoint, endPoint, 12,
                    Color.FromArgb(drawColor.Color.A, drawColor.Color.R, drawColor.Color.G, drawColor.Color.B));
            }
        }

        /// <summary>
        ///     Checks for Indicator from menu
        /// </summary>
        /// <returns></returns>
        public static bool drawIndicator => MainMenu.UtilitiesMenu["Indicator"].GetValue<MenuBool>("Indicator").Enabled;

        /// <summary>
        ///     gets Selected color from menu
        /// </summary>
        /// <returns></returns>
        public static MenuColor drawColor => MainMenu.UtilitiesMenu["Indicator"].GetValue<MenuColor>("SetColor");

        /// <summary>
        ///     Checks if the Target is moving towards you or you are moving Towards the target
        /// </summary>
        /// <returns></returns>
        public static bool IsMovingTowards(this AIHeroClient unit, Vector3 position)
        {
            return unit.IsMoving && unit.Path.Last().Distance(position) <= 100;
        }

        /// <summary>
        ///     Returns the spellstate of Ingite
        /// </summary>
        /// <returns></returns>
        public static bool Ignite =>
            Player.Spellbook.CanUseSpell(Player.GetSpellSlot("SummonerDot")) == SpellState.Ready;

        /// <summary>
        ///     Checks for Truehealth
        /// </summary>
        /// <returns></returns>
        public static float TrueHealth(this AIBaseClient Target)
        {
            var health = Target.Health;
            health += Target.AllShield;
            if (Target.HasBuffOfType(BuffType.Invulnerability)) health += 99999;
            return health;
        }

        /// <summary>
        ///     Checks for if it's a team fight
        /// </summary>
        /// <returns></returns>
        public static bool TeamFight()
        {
            var Count = Player.CountEnemyHeroesInRange(3000);
            var Ally = Player.CountAllyHeroesInRange(3000);
            if (Ally > 1 && Count > 2) return true;

            return false;
        }

        /// <summary>
        ///     Checks if aibaseclient has a Hard CC
        /// </summary>
        /// <returns></returns>
        public static bool HardCC(this BuffType buffType)
        {
            switch (buffType)
            {
                case BuffType.Flee:
                case BuffType.Charm:
                case BuffType.Snare:
                case BuffType.Stun:
                case BuffType.Taunt:
                case BuffType.Suppression:
                case BuffType.Knockup:
                case BuffType.Asleep:
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     Checks if aibaseclient is slowed or has a cc
        /// </summary>
        /// <returns></returns>
        public static bool IsMovementImpairing(this BuffType buffType)
        {
            return buffType.HardCC() || buffType == BuffType.Slow;
        }

        /// <summary>
        ///     Checks for buff time left
        /// </summary>
        /// <returns></returns>
        public static float TimeLeft(this BuffInstance buff)
        {
            return buff.EndTime - Variables.GameTimeTickCount;
        }

        /// <summary>
        ///     Checks attack if u can auto attack a hero
        /// </summary>
        /// <returns></returns>
        public static bool CanAttackAnyHero
            => Orbwalker.CanAttack() &&
               GameObjects.EnemyHeroes.Any(x => x.IsValidTarget(Player.GetRealAutoAttackRange()));

        /// <summary>
        ///     Checks attack if u can auto attack a minion
        /// </summary>
        /// <returns></returns>
        public static bool CanAttackAnyMinion
            => Orbwalker.CanAttack() &&
               GameObjects.EnemyMinions.Any(x => x.IsValidTarget(Player.GetRealAutoAttackRange()));

        /// <summary>
        ///     Checks attack if u can auto attack a Monster
        /// </summary>
        /// <returns></returns>
        public static bool CanAttackAnyMonster
            => Orbwalker.CanAttack() &&
               GameObjects.Jungle.Any(x => x.IsValidTarget(Player.GetRealAutoAttackRange()));
        /// <summary>
        ///     Checks attack if u can auto attack a Turrent
        /// </summary>
        /// <returns></returns>
        public static bool CanAttackTurret
            => Orbwalker.CanAttack() &&
               GameObjects.EnemyTurrets.Any(x => x.IsValidTarget(Player.GetRealAutoAttackRange()));
        /// <summary>
        ///     Gets wall Width
        /// </summary>
        /// <returns></returns>
        public static float GetWallWidth(Vector3 startPos, Vector3 endPos, float maxRange) {
            var width = 0f;
            if (!startPos.IsValid() || !endPos.IsValid()) {
                return width;
            }
    
            for (var i = 0; i < maxRange; i += 1) {
                if (startPos.Extend(endPos, i).IsWall()) {
                    width += 1;
                }
            }

            return width;
        }
    }
}