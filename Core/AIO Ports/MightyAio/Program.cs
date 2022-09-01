using System;
using EnsoulSharp;
using MightyAio.Champions;

namespace MightyAio
{
    internal static class Program
    {
        private static AIHeroClient _player;

        public static void Loads()
        {
            try
            {
                _player = ObjectManager.Player;
                switch (_player.CharacterName)
                {
                    case "Akali":
                        var Akali = new Akali();
                        break;
                    case "Fizz":
                        var fizz = new Fizz();
                        break;
                    case "Ezreal":
                        var ezreal = new Ezreal();
                        break;
                    case "Jinx":
                        var jinx = new Jinx();
                        break;
                    case "Lillia":
                        var load = new Lillia();
                        break;
                    case "Senna":
                        var senna = new Senna();
                        break;  
                    case "Lucian":
                        var Lucian = new Lucian();
                        break;
                    case "Zac":
                        var zac = new Zac();
                        break;
                    case "Chogath":
                        var chogath = new Chogath();
                        break;
                    case "Udyr":
                        var Udyr = new Udyr();
                        break; 
                    case "Yorick":
                        var yorick = new Yorick();
                        break;
                    case "Volibear":
                        //var Volibear = new Volibear();
                        break;
                    case "Shen":
                        var Shen = new Shen();
                        break;
                    case "Riven":
                        var Riven = new Riven();
                        break;
                    case "Skarner":
                        var Skarner = new Skarner();
                        break;
                    case "Yuumi":
                        var Yuumi = new Yuumi();
                        break;
                    case "Zoe":
                        var Zoe = new Zoe();
                        break;
                    
                   
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(@"Failed To load: " + error);
            }
        }
    }
}