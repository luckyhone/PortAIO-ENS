using System;

namespace Slutty_Thresh
{
    internal class Program
    {
        public static void Loads()
        {
            try
            {
                SluttyThresh.OnLoad();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }
        }
    }
}