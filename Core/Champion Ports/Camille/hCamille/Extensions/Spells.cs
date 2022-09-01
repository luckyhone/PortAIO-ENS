using EnsoulSharp;
using EnsoulSharp.SDK;

namespace hCamille.Extensions
{
    public static class Spells
    {
        public static Spell Q { get; set; }
        public static Spell W { get; set; }
        public static Spell E { get; set; }
        public static Spell R { get; set; }


        public static void Initializer()
        {
            Q = new Spell(SpellSlot.Q, 325);
            W = new Spell(SpellSlot.W, 580);
            R = new Spell(SpellSlot.R, 475);
            E = new Spell(SpellSlot.E, 865);
            
            E.SetSkillshot(0.3f,30,500,false,SpellType.Line);
            W.SetSkillshot(0.195f,100,1750,false,SpellType.Cone);

        }
    }
}