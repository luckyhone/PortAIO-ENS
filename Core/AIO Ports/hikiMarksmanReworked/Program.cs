using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using Color = System.Drawing.Color;

namespace hikiMarksmanRework
{
    class Program
    {
        public static void Game_OnGameLoad()
        {
            switch (ObjectManager.Player.CharacterName)
            {
                case "Graves":
                    new Champions.Graves();
                    break;
                case "Sivir":
                    new Champions.Sivir();
                    break;
                case "Lucian":
                    new Champions.Lucian();
                    break;
                case "Ezreal":
                    //new Champions.Ezreal();
                    break;
                case "Vayne":
                    new Champions.Vayne();
                    break;
                case "Draven":
                    new Champions.Draven();
                    break;
                case "Corki":
                    new Champions.Corki();
                    break;
            }
            return;
        }
    }
}