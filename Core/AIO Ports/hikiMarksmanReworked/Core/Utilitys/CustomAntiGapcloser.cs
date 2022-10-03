using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Color = System.Drawing.Color;

 namespace hikiMarksmanRework.Core.Utilitys
{
    public enum SpellType
    {
        Skillshot,
        Targeted
    }

    public class SpellData
    {
        public string ChampionName;
        public string SpellName;
        public SpellSlot Slot;
        public AntiGapcloser.GapcloserType SkillType;
        public int DangerLevel;

    }
    public static class AntiGapcloseSpell
    {

        public static List<SpellData> GapcloseableSpells = new List<SpellData>();

        static AntiGapcloseSpell()
        {

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Aatrox",
                    Slot = SpellSlot.Q,
                    SpellName = "aatroxq",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 1,

                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Akali",
                    Slot = SpellSlot.R,
                    SpellName = "akalishadowdance",
                    SkillType = AntiGapcloser.GapcloserType.Targeted,
                    DangerLevel = 5,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Alistar",
                    Slot = SpellSlot.W,
                    SpellName = "headbutt",
                    SkillType = AntiGapcloser.GapcloserType.Targeted,
                    DangerLevel = 5,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Corki",
                    Slot = SpellSlot.W,
                    SpellName = "carpetbomb",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                     DangerLevel = 1,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Diana",
                    Slot = SpellSlot.R,
                    SpellName = "dianateleport",
                    SkillType = AntiGapcloser.GapcloserType.Targeted,
                    DangerLevel = 4,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Ekko",
                    Slot = SpellSlot.E,
                    SpellName = "ekkoe",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 4,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Elise",
                    Slot = SpellSlot.Q,
                    SpellName = "elisespiderqcast",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 1,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Fiora",
                    Slot = SpellSlot.Q,
                    SpellName = "fioraq",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 1,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Fizz",
                    Slot = SpellSlot.Q,
                    SpellName = "fizzpiercingstrike",
                    SkillType = AntiGapcloser.GapcloserType.Targeted,
                    DangerLevel = 3,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Gnar",
                    Slot = SpellSlot.E,
                    SpellName = "gnare",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 1,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Gragas",
                    Slot = SpellSlot.E,
                    SpellName = "gragase",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 2,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Graves",
                    Slot = SpellSlot.E,
                    SpellName = "gravesmove",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 4,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Hecarim",
                    Slot = SpellSlot.R,
                    SpellName = "hecarimult",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 4,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Illaoi",
                    Slot = SpellSlot.W,
                    SpellName = "illaoiwattack",
                    SkillType = AntiGapcloser.GapcloserType.Targeted,
                    DangerLevel = 3,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Irelia",
                    Slot = SpellSlot.Q,
                    SpellName = "ireliagatotsu",
                    SkillType = AntiGapcloser.GapcloserType.Targeted,
                    DangerLevel = 5,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "JarvanIV",
                    Slot = SpellSlot.Q,
                    SpellName = "jarvanivdragonstrike",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 2,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Jax",
                    Slot = SpellSlot.Q,
                    SpellName = "jaxleapstrike",
                    SkillType = AntiGapcloser.GapcloserType.Targeted,
                    DangerLevel = 5,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Jayce",
                    Slot = SpellSlot.Q,
                    SpellName = "jaycetotheskies",
                    SkillType = AntiGapcloser.GapcloserType.Targeted,
                    DangerLevel = 3,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Khazix",
                    Slot = SpellSlot.E,
                    SpellName = "khazixe",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 3,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Khazix",
                    Slot = SpellSlot.E,
                    SpellName = "khazixelong",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 3,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "LeBlanc",
                    Slot = SpellSlot.W,
                    SpellName = "leblancslide",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 5,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "LeBlanc",
                    Slot = SpellSlot.R,
                    SpellName = "leblancslidem",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 5,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "LeeSin",
                    Slot = SpellSlot.Q,
                    SpellName = "blindmonkqtwo",
                    SkillType = AntiGapcloser.GapcloserType.Targeted,
                    DangerLevel = 3,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Lucian",
                    Slot = SpellSlot.E,
                    SpellName = "luciane",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 4,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Pantheon",
                    Slot = SpellSlot.W,
                    SpellName = "pantheon_leapbash",
                    SkillType = AntiGapcloser.GapcloserType.Targeted,
                    DangerLevel = 4,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Poppy",
                    Slot = SpellSlot.E,
                    SpellName = "poppyheroiccharge",
                    SkillType = AntiGapcloser.GapcloserType.Targeted,
                    DangerLevel = 5,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Renekton",
                    Slot = SpellSlot.E,
                    SpellName = "renektonsliceanddice",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 4,

                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Riven",
                    Slot = SpellSlot.Q,
                    SpellName = "riventricleave",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 4,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Riven",
                    Slot = SpellSlot.E,
                    SpellName = "rivenfeint",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 4,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Sejuani",
                    Slot = SpellSlot.Q,
                    SpellName = "sejuaniarcticassault",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 4,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Shen",
                    Slot = SpellSlot.E,
                    SpellName = "shenshadowdash",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 5,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Shyvana",
                    Slot = SpellSlot.R,
                    SpellName = "shyvanatransformcast",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 5,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Tristana",
                    Slot = SpellSlot.W,
                    SpellName = "rocketjump",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 4,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Tryndamere",
                    Slot = SpellSlot.E,
                    SpellName = "slashcast",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 2,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "XinZhao",
                    Slot = SpellSlot.E,
                    SpellName = "xenzhaosweep",
                    SkillType = AntiGapcloser.GapcloserType.Targeted,
                    DangerLevel = 4,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Yasuo",
                    Slot = SpellSlot.E,
                    SpellName = "yasuodashwrapper",
                    SkillType = AntiGapcloser.GapcloserType.Targeted,
                    DangerLevel = 4,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Zac",
                    Slot = SpellSlot.E,
                    SpellName = "zace",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 3,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Ziggs",
                    Slot = SpellSlot.W,
                    SpellName = "ziggswtoggle",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 4,
                });

            GapcloseableSpells.Add(
                new SpellData
                {
                    ChampionName = "Vi",
                    Slot = SpellSlot.Q,
                    SpellName = "ViQ",
                    SkillType = AntiGapcloser.GapcloserType.SkillShot,
                    DangerLevel = 5,
                });

        }
    }
}
