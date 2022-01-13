using System;

namespace ClassicSim
{
    class Program
    {
        static void Main(string[] args)
        {
            // Number of times to run sim
            int iterations = 20000;

            // How long is the sim in seconds (for not weights)
            int fightDuration = 180;

            MageSim(iterations, fightDuration);
            //RogueSim(iterations, fightDuration);
        }

        public static void MageSim(int iterations, int fightDuration)
        {
            // Stats
            string name = "Remek";
            int intellect = 229;
            int spirit = 187;
            int spellPower = 122;
            int frostPower = 91;
            // 83% + 6% from talents for raid bosses
            // 93% + 6% otherwise
            // Cannot exceed 99%
            int spellHit = 89 + 1;
            int spellCrit = 0;
            int rangedMin = 41;
            int rangedMax = 78;
            float rangedSwing = 1.6f;
            bool intBuff = true;
            bool spiritBuff = false;
            bool wildBuff = false;
            bool elementsBuff = false;

            Mage Remek = new Mage(name, intellect, spirit, spellPower, frostPower, spellCrit, spellHit,  
                rangedMin, rangedMax, rangedSwing,
                intBuff, spiritBuff, wildBuff, elementsBuff)
            {
                // Whether to display action output
                Logging = (iterations.Equals(1))
            };

            if (iterations.Equals(1))
            {
                BasicSims.RunSim(iterations, fightDuration, Remek, spellCrit);
                Console.ReadLine();
            }
            else
            {
                // Multiple fight durations in sequence
                int statStep = 5;
                int statRange = 50;
                int[] durationArray = new int[] { 60, 120, 180, 240, 300, 360 };
                StatWeightSims.SimWeights(Remek, iterations, statStep, statRange, durationArray);
            }
        }

        public static void RogueSim(int iterations, int fightDuration)
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
            bool strengthOfEarthBuff = false;
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

            if (iterations.Equals(1))
            {
                BasicSims.RunSim(iterations, fightDuration, Munroe, critChance);
                Console.ReadLine();
            }
            else
            {
                // Multiple fight durations in sequence
                int statStep = 5;
                int statRange = 50;
                int[] durationArray = new int[] { 180};
                StatWeightSims.SimWeights(Munroe, iterations, statStep, statRange, durationArray);
            }
        }
    }
}
