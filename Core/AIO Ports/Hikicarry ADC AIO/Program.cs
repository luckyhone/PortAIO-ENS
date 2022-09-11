namespace HikiCarry
{
    internal class Program
    {
        public static void Loads()
        {
            OnGameLoad();
        }
        private static void OnGameLoad()
        {
            Initializer.Load();
        }
    }
}