namespace Entropy.Awareness.Bases
{
    public abstract class TrackerBase : ITracker
    {
        public string Name { get; }

        public TrackerBase(string name)
        {
            Name = name;
        }
        public abstract void Initialize();
        public abstract void Render();
        public abstract void WorldRender();
    }
}