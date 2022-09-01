namespace Flowers_ADC_Series
{
    using System;

    internal static class Program
    {
        public static void Loads()
        {
            OnGameLoad(new EventArgs());
        }

        private static void OnGameLoad(EventArgs Args)
        {
            Logic.InitAIO();
        }
    }
}