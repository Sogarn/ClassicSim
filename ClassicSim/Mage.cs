using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassicSim
{
    class Mage : Player
    {
        private int ChillStacks;
        private bool RubyCharge;
        private bool CitrineCharge;
        private bool JadeCharge;
        private float EvocateCD;
        private float RemainingEvocateDuration;
        private float GemCD;

        public Mage() : base()
        {

        }

        public Mage(string name, int intellect, int spirit, int spellPower, int frostPower, int spellCrit, int spellHit, 
            int rangedMinDamage, int rangedMaxdamage, float rangedSwing, 
            bool intBuff, bool spiritBuff, bool wildBuff, bool elementsBuff) : base()
        {
            Name = name;
            Intellect = intellect;
            Spirit = spirit;
            HitChance = spellHit;
            BaseCrit = spellCrit;
            CritMod = 2;
            SpellPower = spellPower;
            FrostPower = frostPower;

            RangedMinDamage = rangedMinDamage;
            RangedMaxDamage = rangedMaxdamage;
            RangedSwing = rangedSwing;
            if (intBuff)
            {
                Intellect += 31;
            }
            if (spiritBuff)
            {
                Spirit += 40;
            }
            if (wildBuff)
            {
                Intellect += 10;
                Spirit += 10;
            }

            Reset();
        }

        // Reset prepares Mage for next iteration
        public override void Reset(float bonusCrit = 0, float timeRemaining = 0)
        {
            Time = 0;
            TimeRemaining = timeRemaining;
            LastAction = 0;

            if (HitChance > 99)
            {
                HitChance = 99;
            }
            CalculateRegen();
            CalculateResourceAndCrit(bonusCrit);
            RefillResource();
            ResourceTickTime = 2;

            ChillStacks = 0;
            RubyCharge = true;
            CitrineCharge = true;
            JadeCharge = true;
            GemCD = 0;
            EvocateCD = 0;
            RemainingEvocateDuration = 0;
        }

        public int FrostBolt_10()
        {
            string result = "hits";

            int baseDamage = 440;
            int randomDamage = RNG.Next(0, 35);
            int bonusDamage = (int)((FrostPower + SpellPower) * 0.814);
            int damage = baseDamage + randomDamage + bonusDamage;
            // 6% bonus frost from talent and 8% from elements
            damage = (int)(damage * 1.06 * (ElementsBuff ? 1.08 : 1));

            float bonusCrit = ChillStacks * 2;

            if (ChillStacks < 5)
            {
                ChillStacks += 1;
            }

            if (RollHit())
            {
                if (RollCrit(bonusCrit))
                {
                    damage = 2 * damage;
                    result = "crits";
                }
            }
            else
            {
                damage = 0;
                result = "misses";
            }

            if (Logging)
            {
                Console.WriteLine(Math.Round(Time, 1) + ": Frostbolt 10 {0} for {1}", result, damage);
            }
            return damage;
        }

        public int Wand()
        {
            bool crit = RollCrit();
            string result = "hits";

            int damage = RNG.Next(RangedMinDamage, RangedMaxDamage + 1);
            if (crit)
            {
                damage = (int)(1.5 * damage);
                result = "crits";
            }

            if (Logging)
            {
                Console.WriteLine(Math.Round(Time, 1) + ": Wand {0} for {1}", result, damage);
            }

            return damage;
        }

        public void ManaRuby()
        {
            float result = 1000 + RNG.Next(0, 200);
            AddResource(result);
            RubyCharge = false;
            GemCD = 120;
            if (Logging)
            {
                Console.WriteLine(Math.Round(Time, 1) + ": Used Ruby for {0} mana", result);
            }
        }

        public void ManaCitrine()
        {
            float result = 775 + RNG.Next(0, 150);
            AddResource(result);
            CitrineCharge = false;
            GemCD = 120;
            if (Logging)
            {
                Console.WriteLine(Math.Round(Time, 1) + ": Used Citrine for {0} mana", result);
            }
        }

        public void ManaJade()
        {
            float result = 550 + RNG.Next(0, 100);
            AddResource(result);
            JadeCharge = false;
            GemCD = 120;
            if (Logging)
            {
                Console.WriteLine(Math.Round(Time, 1) + ": Used Jade for {0} mana", result);
            }
        }

        public void Evocate()
        {
            RemainingEvocateDuration = 8;
            EvocateCD = 8 * 60;
            if (Logging)
            {
                Console.WriteLine(Math.Round(Time, 1) + ": Used Evocate");
            }
        }
        float regenValue;
        public override int NextAction()
        {
            // Add mana
            while (ResourceTickTime <= 0)
            {
                // Evocate gives base resource regen and while active gives 15x mana multiplier
                if (LastAction >= 5 || RemainingEvocateDuration > 0)
                {
                    regenValue = BaseResourceGeneration * (RemainingEvocateDuration > 0 ? 15 : 1);
                }
                else
                {
                    regenValue = CombatResourceGeneration;
                }
                AddResource(regenValue);
                if (Logging)
                {
                    Console.WriteLine("Mana: {0} / {1} (+{2})", CurrentResource, MaxResource, regenValue);
                }
                ResourceTickTime += 2;
            }
            // If Evocate running, just generate mana
            if (RemainingEvocateDuration > 0)
            {
                LastAction += ResourceTickTime;
                UpdateTime(ResourceTickTime);
            }
            else
            {
                // Use mana gems
                if ((MaxResource - CurrentResource > 1500) && GemCD.Equals(0))
                {
                    if (RubyCharge)
                    {
                        ManaRuby();
                    }
                    else if (CitrineCharge)
                    {
                        ManaCitrine();
                    }
                    else if (JadeCharge)
                    {
                        ManaJade();
                    }
                }
                // Cast frostbolt
                if (CurrentResource >= 221)
                {
                    LastAction = 0;
                    UpdateTime(2.5f);
                    SubtractResource(221);
                    return FrostBolt_10();
                }
                // Evocate
                else if (EvocateCD.Equals(0))
                {
                    Evocate();
                    LastAction += 1.5f;
                    UpdateTime(1.5f);
                }
                // Wand if nothing else
                else
                {
                    LastAction += RangedSwing;
                    UpdateTime(RangedSwing);
                    return Wand();
                }
            }
            return 0;
        }

        public void UpdateTime(float time)
        {
            Time += time;
            TimeRemaining -= time;
            ResourceTickTime -= time;
            RemainingEvocateDuration = (RemainingEvocateDuration - time < 0) ? 0 : RemainingEvocateDuration - time;
            EvocateCD = (EvocateCD - time < 0) ? 0 : EvocateCD - time;
            GemCD = (GemCD - time < 0) ? 0 : GemCD - time;
        }

        public void CalculateRegen()
        {
            BaseResourceGeneration = (13 + (Spirit / 4)) / 2;
            CombatResourceGeneration = BaseResourceGeneration * 0.35f / 2;
        }

        // Bonus crit applies when changing int value between interations
        public void CalculateResourceAndCrit(float bonusCrit = 0)
        {
            // Bosses have -3% crit
            CritChance = (0.2f + Intellect / 59.5f) + BaseCrit + bonusCrit - 3;
            MaxResource = 933 + Intellect * 15;
        }

        public override string ToString()
        {
            string statLine = "Intellect: " + Intellect + " Spirit: " + Spirit + " Spellpower: " + SpellPower;
            return statLine;
        }

        public string ToCSV()
        {
            return Intellect + "," + Spirit + "," + SpellPower;
        }
    }
}
