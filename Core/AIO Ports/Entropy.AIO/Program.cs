using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;

namespace Entropy.AIO
{
    public class Program
    {
        public static void Loads()
        {
            var delay = Game.Time > 5 ? 0f : 1750f;
            GameEvent.OnGameLoad += () =>
            {
                DelayAction.Add((int)delay,() => Bootstrap.Initialize());
            };
        }
    }
}