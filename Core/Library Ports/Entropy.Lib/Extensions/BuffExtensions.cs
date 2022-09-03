using EnsoulSharp;

namespace PortAIO.Library_Ports.Entropy.Lib.Extensions
{
    public static class BuffExtensions
    {
        #region Public Methods and Operators

        public static bool IsMovementImpairing(this BuffInstance buffInstance)
        {
            return buffInstance.Type.IsMovementImpairing();
        }

        public static bool IsMovementImpairing(this BuffType buffType)
        {
            return buffType.IsHardCC() || buffType == BuffType.Slow;
        }

        public static bool IsHardCC(this BuffInstance buff)
        {
            return buff.Type.IsHardCC();
        }

        public static bool IsHardCC(this BuffType buffType)
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

        public static bool IsActive(this BuffInstance buff)
        {
            return buff != null && buff.IsValid && buff.TimeLeft() > 0f;
        }

        public static bool PreventsCasting(this BuffInstance buff)
        {
            return buff.Type.PreventsCasting();
        }

        public static bool PreventsCasting(this BuffType buffType)
        {
            switch (buffType)
            {
                case BuffType.Silence:
                case BuffType.Charm:
                case BuffType.Taunt:
                case BuffType.Knockup:
                case BuffType.Flee:
                case BuffType.Suppression:
                case BuffType.Stun:
                case BuffType.Polymorph:
                case BuffType.Knockback:
                case BuffType.Asleep:
                    return true;
            }

            return false;
        }

        public static bool PreventsAutoAttacking(this BuffInstance buff)
        {
            return buff.Type.PreventsAutoAttacking();
        }

        public static bool PreventsAutoAttacking(this BuffType buffType)
        {
            switch (buffType)
            {
                case BuffType.Polymorph:
                case BuffType.Disarm:
                case BuffType.Blind:
                    return true;
            }

            return false;
        }

        public static float TimeLeft(this BuffInstance buff)
        {
            return buff.EndTime - Game.Time;
        }

        #endregion
    }
}