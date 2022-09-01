namespace ADCPackage
{
    class Menu
    {
        public static EnsoulSharp.SDK.MenuUI.Menu Config =
            new EnsoulSharp.SDK.MenuUI.Menu("adcpackage","ADC Package" , true);

        public static void Initialize()
        {
            ItemManager.Initialize();

            Config.Attach();
        }
    }
}