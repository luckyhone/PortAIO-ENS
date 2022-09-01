using System.Collections.Generic;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace UniversalMinimapHack
{
    public class MinimapHack
    {
        private static readonly MinimapHack MinimapHackInstance = new MinimapHack();

        private readonly IList<HeroTracker> _heroTrackers = new List<HeroTracker>();

        public Menu Menu { get; private set; }

        public static MinimapHack Instance()
        {
            return MinimapHackInstance;
        }

        public void Load()
        {
            Menu = new Menu();
            foreach (AIHeroClient hero in GameObjects.EnemyHeroes)
            {
                _heroTrackers.Add(new HeroTracker(hero, ImageLoader.Load(hero.CharacterName)));
            }
        }
    }
}