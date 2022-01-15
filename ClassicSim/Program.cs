using System;

namespace ClassicSim
{
    class Program
    {
        static void Main(string[] args)
        {
            // Number of times to run sim
            int iterations = 30000;
            bool basic = true;

            // How long is the sim in seconds (for not weights)
            int fightDuration = 120;

            MageSim(basic, iterations, fightDuration);
            //RogueSim(basic, iterations, fightDuration);
        }

        public static void MageSim(bool basic, int iterations, int fightDuration)
        {
            int[] durationArray = new int[] { 60, 120, 300};

            // Statse
            string name = "Remek";
            int intellect = 246;
            int spirit = 176;
            int spellPower = 254;
            int frostPower = 61;
            // 83% + 6% from talents for raid bosses
            // Cannot exceed 99%
            int spellHit = 89 + 2;
            int spellCrit = 4;
            int manaPerFive = 4;
            int rangedMin = 52;
            int rangedMax = 97;
            float rangedSwing = 1.4f;
            bool intBuff = true;
            bool spiritBuff = true;
            bool wildBuff = true;
            bool elementsBuff = true;
            bool arcanePower = true;
            bool manaOrb = false;
            bool manaChest = true;
            bool talismanTrinket = false;

            Mage Remek = new Mage(name, intellect, spirit, spellPower, frostPower, spellCrit, spellHit, manaPerFive,
                rangedMin, rangedMax, rangedSwing,
                intBuff, spiritBuff, wildBuff, elementsBuff, arcanePower, manaOrb, manaChest, talismanTrinket)
            {
                // Whether to display action output
                Logging = (iterations.Equals(1))
            };

            if (basic)
            {
                BasicSims.RunSim(iterations, fightDuration, Remek);
                //Remek.Intellect += 14 - 8;
                //Remek.Spirit += 7 - 3;
                //Remek.SpellPower += 23 - 29;
                //Remek.TalismanEquipped = true;
                //Remek.BaseCrit += 1 - 0;
                //Remek.HitChance += 0 - 0;
                //Remek.ManaPerFive += 4;
                //Remek.RangedMinDamage = 68;
                //Remek.RangedMaxDamage = 127;
                //Remek.RangedSwing = 1.8f;
                //BasicSims.RunSim(iterations, fightDuration, Remek);
                Console.ReadLine();
            }
            else
            {
                // Multiple fight durations in sequence
                int statStep = 5;
                int statRange = 50;
                StatWeightSims.MageSimWeights(Remek, iterations, statStep, statRange, durationArray);
            }
        }

        public static void RogueSim(bool basic, int iterations, int fightDuration)
        {
            string name = "Munroe";
            int agility = 210;
            int strength = 120;
            int attackPower = 160;
            // Hit improved 5% from talents
            int hitChance = 5 + 6;
            // Gets 5% crit from talents
            int critChance = 5 + 4;
            // 315 for raid bosses, 310 for dungeon bosses
            int targetDefenseSkill = 315;
            // 5 weapon skill from talents
            int weaponSkill = 300 + 5;
            int targetArmor = 3500;
            int mhMinDamage = 66;
            int mhMaxDamage = 124;
            float mhSwing = 2.7f;
            int ohMinDamage = 52;
            int ohMaxDamage = 97;
            float ohSwing = 1.8f;
            bool strengthOfEarthBuff = true;
            bool graceOfAirBuff = false;
            bool windfuryBuff = false;
            bool wildBuff = true;
            bool dotsAllowed = false;

            Rogue Munroe = new Rogue(name, agility, strength, attackPower, hitChance, critChance,
                weaponSkill, targetDefenseSkill, targetArmor, mhMinDamage, mhMaxDamage, mhSwing, ohMinDamage, ohMaxDamage, ohSwing,
                strengthOfEarthBuff, graceOfAirBuff, windfuryBuff, wildBuff, dotsAllowed)
            {
                Logging = iterations.Equals(1)
            };

            if (basic)
            {
                BasicSims.RunSim(iterations, fightDuration, Munroe);
                Munroe.WindfuryBuff = true;
                BasicSims.RunSim(iterations, fightDuration, Munroe);
                Console.ReadLine();
            }
            else
            {
                // Multiple fight durations in sequence
                int statStep = 5;
                int statRange = 50;
                int[] durationArray = new int[] {180};
                StatWeightSims.RogueSimWeights(Munroe, iterations, statStep, statRange, durationArray);
            }
        }
    }
}
