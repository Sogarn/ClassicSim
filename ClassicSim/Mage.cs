using System;
using System.Collections.Generic;
using System.IO;
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
        private float RemainingEvocateTime;
        private float GemCD;
        private bool ArcaneSpec;
        private float RemainingArcanePowerTime;
        private float ArcanePowerCD;
        private bool Clearcasting;
        private bool ManaOrb;
        private bool UsedOffhand;
        private bool OffhandReady;
        private float OffhandRemaining;
        private bool PresenceActive;
        private float PresenceCD; 
        public bool TalismanEquipped;
        private float TalismanCD;
        private float RemainingTalismanTime;
        private bool ManaChest;
        private float ManaChestCD;

        public Mage() : base()
        {

        }

        public Mage(string name, int intellect, int spirit, int spellPower, int frostPower, int spellCrit, int spellHit, int manaPerFive,
            int rangedMinDamage, int rangedMaxdamage, float rangedSwing,
            bool intBuff, bool spiritBuff, bool wildBuff, bool elementsBuff, bool arcaneSpec, bool manaOrb, bool manaChest, bool talismanTrinket) : base()
        {

            Name = name;
            Intellect = intellect;
            Spirit = spirit;
            HitChance = spellHit;
            ManaPerFive = manaPerFive;
            BaseCrit = spellCrit;
            CritMod = 2;
            SpellPower = spellPower;
            FrostPower = frostPower;
            ArcaneSpec = arcaneSpec;

            if (ArcaneSpec)
            {
                BaseCrit += 3;
            }

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
                Intellect += 12;
                Spirit += 12;
            }

            ManaOrb = manaOrb;
            ManaChest = manaChest;
            TalismanEquipped = talismanTrinket;
            Reset();
        }

        // Reset prepares Mage for next iteration
        public override void Reset(float timeRemaining = 0)
        {
            Time = 0;
            TimeRemaining = timeRemaining;
            LastAction = 0;

            if (HitChance > 99)
            {
                HitChance = 99;
            }
            CalculateRegen();
            CalculateResourceAndCrit();
            RefillResource();
            ResourceTickTime = 2;
            ManaPerFiveTickTime = 5;
            ChillStacks = 0;
            RubyCharge = true;
            CitrineCharge = true;
            JadeCharge = true;
            Clearcasting = false;
            GemCD = 0;
            RemainingEvocateTime = 0;
            EvocateCD = 0;
            RemainingArcanePowerTime = 0;
            ArcanePowerCD = 0;
            TalismanCD = 0;
            RemainingTalismanTime = 0;
            PresenceActive = false;
            PresenceCD = 0;
            if (ManaOrb)
            {
                // If ended fight with mana offhand equipped, fix stats
                if (!OffhandReady && !UsedOffhand)
                {
                    Intellect -= 3;
                    SpellPower += 7;
                }
                OffhandReady = true;
                UsedOffhand = false;
                OffhandRemaining = 0;
            }
            ManaChestCD = 0;
        }

        public int FrostBolt_10()
        {
            string result = "hits";

            int baseDamage = RNG.Next(440, 475 + 1);
            // Arcane gets 3% bonus spellpower
            int bonusDamage = (int)((FrostPower + SpellPower + (RemainingTalismanTime > 0 ? 175 : 0)) * 0.814);
            int damage = baseDamage + bonusDamage;
            // 6% bonus frost from talent and 8% from elements
            damage = (int)(damage * 1.06 * (ElementsBuff ? 1.08 : 1) * (ArcaneSpec ? 1.03 : 1));

            // Arcane power
            if (RemainingArcanePowerTime > 0)
            {
                damage = (int)(damage * 1.3);
            }

            if (ChillStacks < 5)
            {
                ChillStacks += 1;
            }

            if (RollHit())
            {
                if (RollCrit(ChillStacks * 2))
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
                using (StreamWriter writer = new StreamWriter(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\SimOutput.txt", true))
                {
                    writer.WriteLine(Math.Round(Time, 1) + ": Frostbolt 10 {0} for {1}", result, damage);
                }
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
                using (StreamWriter writer = new StreamWriter(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\SimOutput.txt", true))
                {
                    writer.WriteLine(Math.Round(Time, 1) + ": Wand {0} for {1}", result, damage);
                }
                Console.WriteLine(Math.Round(Time, 1) + ": Wand {0} for {1}", result, damage);
            }

            return damage;
        }

        #region Mana Gem functions
        public void ManaRuby()
        {
            float result = 1000 + RNG.Next(0, 200);
            AddResource(result);
            RubyCharge = false;
            GemCD = 120;
            if (Logging)
            {
                using (StreamWriter writer = new StreamWriter(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\SimOutput.txt", true))
                {
                    writer.WriteLine(Math.Round(Time, 1) + ": Used Ruby for {0} mana", result);
                }
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
                using (StreamWriter writer = new StreamWriter(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\SimOutput.txt", true))
                {
                    writer.WriteLine(Math.Round(Time, 1) + ": Used Citrine for {0} mana", result);
                }
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
                using (StreamWriter writer = new StreamWriter(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\SimOutput.txt", true))
                {
                    writer.WriteLine(Math.Round(Time, 1) + ": Used Jade for {0} mana", result);
                }
                Console.WriteLine(Math.Round(Time, 1) + ": Used Jade for {0} mana", result);
            }
        }
        #endregion

        public void Evocate()
        {
            RemainingEvocateTime = 8;
            EvocateCD = 8 * 60;
            if (Logging)
            {
                using (StreamWriter writer = new StreamWriter(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\SimOutput.txt", true))
                {
                    writer.WriteLine(Math.Round(Time, 1) + ": Used Evocate");
                }
                Console.WriteLine(Math.Round(Time, 1) + ": Used Evocate");
            }
        }

        float regenValue;
        public override int NextAction()
        {
            // MP5
            if (ManaPerFiveTickTime <= 0)
            {
                CurrentResource += ManaPerFive;
                ManaPerFiveTickTime += 5;
            }
            // Add mana
            while (ResourceTickTime <= 0)
            {
                // Evocate gives base resource regen and while active gives 15x mana multiplier
                if (LastAction >= 5 || RemainingEvocateTime > 0)
                {
                    regenValue = BaseResourceGeneration;
                    if (RemainingEvocateTime > 0)
                    {
                        regenValue *= 16;
                    }
                }
                else
                {
                    regenValue = CombatResourceGeneration;
                }
                AddResource(regenValue);
                if (Logging)
                {
                    using (StreamWriter writer = new StreamWriter(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\SimOutput.txt", true))
                    {
                        writer.WriteLine("Mana: {0} / {1} (+{2})", CurrentResource, MaxResource, regenValue);
                    }
                    Console.WriteLine("Mana: {0} / {1} (+{2})", CurrentResource, MaxResource, regenValue);
                }
                ResourceTickTime += 2;
            }

            // If you want to use robes of archmage
            if (ManaChest && ManaChestCD.Equals(0) && MaxResource - CurrentResource > 700)
            {
                int bonusMana = RNG.Next(375, 625 + 1);
                ManaChestCD = 300;
                AddResource(bonusMana);
                if (Logging)
                {
                    using (StreamWriter writer = new StreamWriter(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\SimOutput.txt", true))
                    {
                        writer.WriteLine(Math.Round(Time, 1) + ": Used chest for {0} mana", bonusMana);
                    }
                    Console.WriteLine(Math.Round(Time, 1) + ": Used chest for {0} mana", bonusMana);
                }
            }

            #region ManaOrb AI
            // If you want to use mana offhand
            if (ManaOrb)
            {
                // If offhand ready to use, have used evocate, and have less than 50% mana equip and the fight will last at least 35 seconds
                if (OffhandReady && EvocateCD > 0 && RemainingEvocateTime.Equals(0) && OffhandRemaining.Equals(0) && CurrentResource / MaxResource < 0.5 && TimeRemaining > 35)
                {
                    OffhandReady = false;
                    OffhandRemaining = 30;
                    Intellect += 3;
                    SpellPower -= 7;
                    CalculateResourceAndCrit();
                    if (Logging)
                    {
                        using (StreamWriter writer = new StreamWriter(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\SimOutput.txt", true))
                        {
                            writer.WriteLine(Math.Round(Time, 1) + ": Equipped offhand");
                        }
                        Console.WriteLine(Math.Round(Time, 1) + ": Equipped offhand");
                    }
                }
                // Use it
                if (!UsedOffhand && !OffhandReady && OffhandRemaining.Equals(0))
                {
                    UsedOffhand = true;
                    int manaGain = RNG.Next(400, 1200 + 1);
                    AddResource(manaGain);
                    Intellect -= 3;
                    SpellPower += 7;
                    UpdateTime(1.5f);
                    LastAction += 1.5f;
                    CalculateResourceAndCrit();
                    if (Logging)
                    {
                        using (StreamWriter writer = new StreamWriter(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\SimOutput.txt", true))
                        {
                            writer.WriteLine(Math.Round(Time, 1) + ": Used offhand for {0} mana", manaGain);
                        }
                        Console.WriteLine(Math.Round(Time, 1) + ": Used offhand for {0} mana", manaGain);
                    }
                }
            }
            #endregion

            // If Evocate running, just generate mana till have enough
            if (RemainingEvocateTime > 0)
            {
                // If we have enough mana just cancel evocate
                if (CurrentResource / 221 * 2.5 < TimeRemaining)
                {
                    LastAction += ResourceTickTime;
                    UpdateTime(ResourceTickTime);
                }
                else
                {
                    RemainingEvocateTime = 0;
                }
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
                // Use presence of mind if available and 3+ chill stacks
                if (ArcaneSpec && PresenceCD.Equals(0) && ChillStacks >= 3)
                {
                    PresenceActive = true;
                    PresenceCD = 180;
                    if (Logging)
                    {
                        using (StreamWriter writer = new StreamWriter(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\SimOutput.txt", true))
                        {
                            writer.WriteLine(Math.Round(Time, 1) + ": Used Presence of Mind");
                        }
                        Console.WriteLine(Math.Round(Time, 1) + ": Used Presence of Mind");
                    }
                }
                // Use Arcane Power if AP spec and check mana if evocate ready and 3+ winters chill
                if (ArcaneSpec && ArcanePowerCD.Equals(0) && (CurrentResource > 1500 || EvocateCD > 0) && ChillStacks >= 3)
                {
                    if (Logging)
                    {
                        using (StreamWriter writer = new StreamWriter(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\SimOutput.txt", true))
                        {
                            writer.WriteLine(Math.Round(Time, 1) + ": Used Arcane Power");
                        }
                        Console.WriteLine(Math.Round(Time, 1) + ": Used Arcane Power");
                    }
                    RemainingArcanePowerTime = 15;
                    ArcanePowerCD = 180;
                }
                // Use Talisman if ready and check mana if evocate ready and 3+ winters chill stacks
                if (TalismanEquipped && TalismanCD.Equals(0) && (CurrentResource > 1000 || EvocateCD > 0) && ChillStacks >= 3)
                {
                    if (Logging)
                    {
                        using (StreamWriter writer = new StreamWriter(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\SimOutput.txt", true))
                        {
                            writer.WriteLine(Math.Round(Time, 1) + ": Used Talisman of Ephemeral Power");
                        }
                        Console.WriteLine(Math.Round(Time, 1) + ": Used Talisman of Ephemeral Power");
                    }
                    RemainingTalismanTime = 15;
                    TalismanCD = 90;
                }
                // Cast frostbolt if clearcasting or have enough mana OR (not in 5 second rule or have enough mana to chaincast frostbolt to end or have at least 20% mana)
                if (Clearcasting || (CurrentResource >= (RemainingArcanePowerTime > 0 ? 221 * 1.3f : 221) && (LastAction < 5 || (CurrentResource >= TimeRemaining * 221 / 2.5) || CurrentResource >= MaxResource * 0.25)))
                {
                    LastAction = 0;
                    // If PoM active, it's 1 second instead of 2.5
                    if (PresenceActive)
                    {
                        UpdateTime(1);
                        PresenceActive = false;
                    }
                    else
                    {
                        UpdateTime(2.5f);
                    }
                    if (Clearcasting)
                    {
                        Clearcasting = false;
                    }
                    else
                    {
                        if (RemainingArcanePowerTime > 0)
                        {
                            SubtractResource(221 * 1.3f);
                        }
                        else
                        {
                            SubtractResource(221);
                        }
                    }
                    Clearcasting = RNG.Next(1, 101) <= 10;
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
            ManaPerFiveTickTime -= time;
            RemainingEvocateTime = (RemainingEvocateTime - time < 0) ? 0 : RemainingEvocateTime - time;
            EvocateCD = (EvocateCD - time < 0) ? 0 : EvocateCD - time;
            GemCD = (GemCD - time < 0) ? 0 : GemCD - time;
            if (ArcaneSpec)
            {
                PresenceCD = (PresenceCD - time < 0) ? 0 : PresenceCD - time;
                ArcanePowerCD = (ArcanePowerCD - time < 0) ? 0 : ArcanePowerCD - time;
                RemainingArcanePowerTime = (RemainingArcanePowerTime - time < 0) ? 0 : RemainingArcanePowerTime - time;
            }
            if (TalismanEquipped)
            {
                TalismanCD = (TalismanCD - time < 0) ? 0 : TalismanCD - time;
                RemainingTalismanTime = (RemainingTalismanTime - time < 0) ? 0 : RemainingTalismanTime - time;
            }
            if (ManaOrb)
            {
                OffhandRemaining = (OffhandRemaining - time < 0) ? 0 : OffhandRemaining - time;
            }
            if (ManaChest)
            {
                ManaChestCD = (ManaChestCD - time < 0) ? 0 : ManaChestCD - time;
            }
        }

        public void CalculateRegen()
        {
            BaseResourceGeneration = (12.5f + (Spirit / 4));
            CombatResourceGeneration = BaseResourceGeneration * (ArcaneSpec ? 0.45f : 0.35f);
        }

        // Bonus crit applies when changing int value between interations
        public void CalculateResourceAndCrit()
        {
            // Bosses have -3% crit
            CritChance = (0.2f + Intellect / 59.5f) + BaseCrit - 3;
            MaxResource = 933 + Intellect * 15;
            // Extra talents for arcane
            if (ArcaneSpec)
            {
                MaxResource = (int)(MaxResource * 1.1);
            }
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
