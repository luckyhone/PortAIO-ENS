using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;

namespace Entropy.AIO.LeBlanc.Misc
{
    using static Components;
    using static ChampionBase;
    static class Definitions
    {
        public static bool HasW1  => W.Name.Equals("LeblancW") || W.ToggleState == (SpellToggleState)1;
        public static bool HasRW1 => R.Name.Equals("LeblancRW");

        public static RMode GetRMode => (RMode) ComboMenu.RModelist.Index;

        public static AIHeroClient GetMarkedTarget => GameObjects.EnemyHeroes.FirstOrDefault(HasQBuff);

        public static bool HasQBuff(this AIBaseClient target)
        {
            return target.HasBuff("LeblancQMark") || target.HasBuff("LeblancRQMark");
        }

        public static RState GetRState()
        {
            switch (R.Name)
            {
                case "LeblancRQ": return RState.RQ;
                case "LeblancRW": return RState.RW;
                case "LeblancRE": return RState.RE;
                default:          return RState.NULL;
            }
        }
    }

    enum RState
    {
        RQ,
        RE,
        RW,
        NULL
    }

    enum RMode
    {
        RQ,
        RE,
        RW,
        NULL
    }
}