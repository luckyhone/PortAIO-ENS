namespace Entropy.Awareness.Bases
{
    public interface ITracker
    {
        string Name { get;  }
       
        void Initialize();

        void Render();

        void WorldRender();
    }
}