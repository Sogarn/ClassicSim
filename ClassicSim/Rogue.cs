using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassicSim
{
    class Rogue : Player
    {
        private bool Stealth;
        private int GarroteTick;
        private int RemainingGarroteTicks;
        private float RemainingGarroteInterval;
        private int RuptureTick;
        private int RemainingRuptureTicks;
        private float RemainingRuptureInterval;
        private float VanishCD;
        private float BladeFlurryCD;
        private float RemainingBladeFlurry;
        private float AdrenalineRushCD;
        private float RemainingAdrenalineRush;
        private float RemainingSliceAndDice;
        private float OrcRacialCD;
        private float RemainingOrcRacial;
        private float OrcRacialBonus;
        private int MhPoisonCharges;
        private int OhPoisonCharges;
        private int ComboPoints;

        public Rogue(string name, int agility, int strength, int attackPower, int bonusHit, int critChance, int weaponSkill, int targetDefenseSkill, int targetArmor,
            int mhMinDamage, int mhMaxDamage, float mhSwing, int ohMinDamage, int ohMaxDamage, float ohSwing,
            bool strengthOfEarthBuff, bool graceOfAirBuff, bool windfuryBuff, bool wildBuff, bool dotsAllowed) : base()
        {
            Name = name;
            Agility = agility;
            Strength = strength;
            AttackPower = attackPower;
            HitChance = bonusHit;
            BaseCrit = critChance;
            CritMod = 2;
            WeaponSkill = weaponSkill;
            TargetDefenseSkill = targetDefenseSkill;
            TargetArmor = targetArmor;
            MhMinDamage = mhMinDamage;
            MhMaxDamage = mhMaxDamage;
            MhSwing = mhSwing;
            MhSwingRemaining = MhSwing;
            OhMinDamage = ohMinDamage;
            OhMaxDamage = ohMaxDamage;
            OhSwing = ohSwing;
            OhSwingRemaining = OhSwing;
            StrengthOfEarthBuff = strengthOfEarthBuff;
            if (StrengthOfEarthBuff)
            {
                Strength += 61;
            }
            GraceOfAirBuff = graceOfAirBuff;
            if (GraceOfAirBuff)
            {
                Agility += 67;
            }
            WindfuryBuff = windfuryBuff;
            WildBuff = wildBuff;
            if (WildBuff)
            {
                Strength += 12;
                Agility += 12;
            }
            DotsAllowed = dotsAllowed;
            MaxResource = 100;
            BaseResourceGeneration = 20;
            Reset();
        }

        public override void Reset(float timeRemaining = 0)
        {
            Time = 0;
            TimeRemaining = timeRemaining;
            OrcRacialCD = 0;
            RemainingOrcRacial = 0;
            Stealth = DotsAllowed;
            ComboPoints = 0;
            ResourceTickTime = 2;
            RemainingGarroteTicks = 0;
            RemainingGarroteInterval = 0;
            RemainingRuptureTicks = 0;
            RemainingRuptureInterval = 0;
            RemainingSliceAndDice = 0;
            RemainingBladeFlurry = 0;
            RemainingAdrenalineRush = 0;
            WindfuryProc = false;
            MhSwingRemaining = MhSwing;
            OhSwingRemaining = OhSwing;
            FinalAttackPower = Strength + Agility + AttackPower + 2 * 60 - 20;
            OrcRacialBonus = 111;
            // Bosses have -3% crit
            CritChance = BaseCrit + Agility / 29 - 3;
            if (CritChance < 0)
            {
                CritChance = 0;
            }
            CurrentResource = MaxResource;
            BladeFlurryCD = 0;
            AdrenalineRushCD = 0;
            TargetSunders = 0;
            MhPoisonCharges = 115;
            OhPoisonCharges = 115;
        }

        public int MhAttack()
        {
            int damage = RNG.Next(MhMinDamage, MhMaxDamage + 1) + (int)((FinalAttackPower + (WindfuryProc ? 315 : 0) + (RemainingOrcRacial > 0 ? OrcRacialBonus : 0)) * (MhSwing / 14));
            damage = (int)(damage * ArmorReduction());

            string result = "";

            switch (RollHitDualWield(HitChance))
            {
                case DualWieldAttackResult.Miss:
                    damage = 0;
                    result = "misses";
                    break;
                case DualWieldAttackResult.Glancing:
                    float glanceLowMult = (float)(1.3 - 0.05 * (TargetDefenseSkill - WeaponSkill));
                    if (glanceLowMult > 0.91)
                    {
                        glanceLowMult = 0.91f;
                    }
                    float glanceHighMult = (float)(1.2 - 0.03 * (TargetDefenseSkill - WeaponSkill));
                    if (glanceHighMult > 0.99)
                    {
                        glanceHighMult = 0.99f;
                    }
                    damage = (int)(damage * (glanceLowMult + (glanceHighMult - glanceLowMult) * RNG.NextDouble()));
                    result = "glances";
                    break;
                case DualWieldAttackResult.Crit:
                    damage = (int)(CritMod * damage);
                    result = "crits";
                    break;
                default:
                    result = "hits";
                    break;
            }
            if (Logging)
            {
                Console.WriteLine(Math.Round(Time, 1) + ": Main hand {0} for {1}", result, damage);
            }

            int poisonDamage = 0;
            // Windfury and poison are mutually exclusive
            if (damage > 0 && !WindfuryBuff)
            {
                // Do poison checks if damage > 0 (hit somehow)
                if (MhPoisonCharges > 0 && RNG.Next(1, 101) <= 20)
                {
                    MhPoisonCharges -= 1;
                    poisonDamage = 112 + RNG.Next(0, 37 + 1);
                    if (Logging)
                    {
                        Console.WriteLine(Math.Round(Time, 1) + ": Instant poison for {0}", poisonDamage);
                    }
                }
            }

            // Update windfury
            WindfuryProc = false;
            return damage + poisonDamage;
        }

        public int OhAttack()
        {
            int damage = (int)(0.75 * (RNG.Next(OhMinDamage, OhMaxDamage + 1) + (int)((FinalAttackPower + (RemainingOrcRacial > 0 ? OrcRacialBonus : 0)) * (OhSwing / 14))));
            damage = (int)(damage * ArmorReduction());

            string result = "";
            switch (RollHitDualWield(HitChance))
            {
                case DualWieldAttackResult.Miss:
                    damage = 0;
                    result = "misses";
                    break;
                case DualWieldAttackResult.Glancing:
                    float glanceLowMult = (float)(1.3 - 0.05 * (TargetDefenseSkill - WeaponSkill));
                    if (glanceLowMult > 0.91)
                    {
                        glanceLowMult = 0.91f;
                    }
                    float glanceHighMult = (float)(1.2 - 0.03 * (TargetDefenseSkill - WeaponSkill));
                    if (glanceHighMult > 0.99)
                    {
                        glanceHighMult = 0.99f;
                    }
                    damage = (int)(damage * (glanceLowMult + (glanceHighMult - glanceLowMult) * RNG.NextDouble()));
                    result = "glances";
                    break;
                case DualWieldAttackResult.Crit:
                    damage = (int)(CritMod * damage);
                    result = "crits";
                    break;
                default:
                    result = "hits";
                    break;
            }
            if (Logging)
            {
                Console.WriteLine(Math.Round(Time, 1) + ": Off hand {0} for {1}", result, damage);
            }

            int poisonDamage = 0;
            if (damage > 0)
            {
                // Do poison checks if damage > 0 (hit somehow)
                if (OhPoisonCharges > 0 && RNG.Next(1, 101) <= 20)
                {
                    OhPoisonCharges -= 1;
                    poisonDamage = 112 + RNG.Next(0, 37 + 1);
                    if (Logging)
                    {
                        Console.WriteLine(Math.Round(Time, 1) + ": Instant poison for {0}", poisonDamage);
                    }
                }
            }
            return damage + poisonDamage;
        }

        public void GenerateComboPoints(int increase)
        {
            ComboPoints = (ComboPoints + increase > 5 ? 5 : ComboPoints + increase);
        }

        public int SpendComboPoints()
        {
            int spent = ComboPoints;
            ComboPoints = 0;
            return spent;
        }

        public int SinisterStrike()
        {
            // aggression talent is 6% increase
            int damage = (int)(1.06 * (RNG.Next(MhMinDamage, MhMaxDamage + 1) + (int)((FinalAttackPower + (RemainingOrcRacial > 0 ? OrcRacialBonus : 0)) * (MhSwing / 14)) + 98));
            damage = (int)(damage * ArmorReduction());

            string result = "";
            switch (RollHitAbility(HitChance))
            {
                case AttackResult.Miss:
                    damage = 0;
                    SubtractResource(50 * .2f);
                    result = "misses";
                    break;
                case AttackResult.Crit:
                    damage = (int)((CritMod + 0.3) * damage);
                    result = "crits";
                    GenerateComboPoints(1);
                    SubtractResource(40);
                    break;
                default:
                    result = "hits";
                    GenerateComboPoints(1);
                    SubtractResource(40);
                    break;
            }
            if (Logging)
            {
                Console.WriteLine(Math.Round(Time, 1) + ": Sinister Strike {0} for {1}", result, damage);
            }
            return damage;
        }

        public void Garrote()
        {
            string result = "";

            switch (RollHitAbility(HitChance))
            {
                case AttackResult.Miss:
                    SubtractResource(50 * .2f);
                    result = "misses";
                    break;
                default:
                    GarroteTick = 71 + (int)(0.03 * FinalAttackPower);
                    Stealth = false;
                    RemainingGarroteTicks = 6;
                    RemainingGarroteInterval = 3;
                    GenerateComboPoints(1);
                    SubtractResource(50);
                    result = "hits";
                    break;
            }

            if (Logging)
            {
                Console.WriteLine(Math.Round(Time, 1) + ": Garrote {0}", result);
            }
        }

        public void SliceAndDice()
        {
            // 40% attack speed
            int investedComboPoints = SpendComboPoints();
            RemainingSliceAndDice = (int)(1.5 * (6 + (3 * investedComboPoints)));
            SubtractResource(25);
            //Ruthlessness
            if (RNG.Next(1, 101) <= 60)
            {
                GenerateComboPoints(1);
            }
            //Relentless Strikes
            if (RNG.Next(1, 101) <= 20 * investedComboPoints)
            {
                AddResource(25);
            }
            if (Logging)
            {
                Console.WriteLine(Math.Round(Time, 1) + ": Slice and Dice with {0} combo points", investedComboPoints);
            }
        }

        public void Rupture()
        {
            int investedComboPoints = SpendComboPoints();

            string result;
            // Build base value of rupture damage
            switch (investedComboPoints)
            {
                case (1):
                    RuptureTick = 176;
                    break;
                case (2):
                    RuptureTick = 255;
                    break;
                case (3):
                    RuptureTick = 348;
                    break;
                case (4):
                    RuptureTick = 455;
                    break;
                default:
                    RuptureTick = 576;
                    break;
            }

            switch (RollHitAbility(HitChance))
            {
                case AttackResult.Miss:
                    SubtractResource(25 * .2f);
                    GenerateComboPoints(investedComboPoints);
                    result = "misses";
                    break;
                default:
                    RemainingRuptureTicks = (8 + (2 * investedComboPoints)) / 2;
                    RuptureTick = (int)((RuptureTick + 0.24 * FinalAttackPower + (RemainingOrcRacial > 0 ? OrcRacialBonus : 0)) / RemainingRuptureTicks);
                    RemainingRuptureInterval = 2;

                    SubtractResource(25);
                    SpenderTalents(investedComboPoints);
                    result = "hits";
                    break;
            }

            if (Logging)
            {
                Console.WriteLine(Math.Round(Time, 1) + ": Rupture {0} with {1} combo points", result, investedComboPoints);
            }
        }

        public int Eviscerate()
        {
            int investedComboPoints = SpendComboPoints();
            int baseDamage;
            string result = "";

            // Calculate eviscerate damage
            switch (investedComboPoints)
            {
                // aggression talent is 6% increase
                case (1):
                    baseDamage = (int)(1.06 * (199 + RNG.Next(0, 96) + (investedComboPoints * 0.03 * (FinalAttackPower + (RemainingOrcRacial > 0 ? OrcRacialBonus : 0)))));
                    break;
                case (2):
                    baseDamage = (int)(1.06 * (350 + RNG.Next(0, 96) + (investedComboPoints * 0.03 * (FinalAttackPower + (RemainingOrcRacial > 0 ? OrcRacialBonus : 0)))));
                    break;
                case (3):
                    baseDamage = (int)(1.06 * (501 + RNG.Next(0, 96) + (investedComboPoints * 0.03 * (FinalAttackPower + (RemainingOrcRacial > 0 ? OrcRacialBonus : 0)))));
                    break;
                case (4):
                    baseDamage = (int)(1.06 * (652 + RNG.Next(0, 96) + (investedComboPoints * 0.03 * (FinalAttackPower + (RemainingOrcRacial > 0 ? OrcRacialBonus : 0)))));
                    break;
                default:
                    baseDamage = (int)(1.06 * (803 + RNG.Next(0, 96) + (investedComboPoints * 0.03 * (FinalAttackPower + (RemainingOrcRacial > 0 ? OrcRacialBonus : 0)))));
                    break;
            }

            switch (RollHitAbility(HitChance))
            {
                case AttackResult.Miss:
                    baseDamage = 0;
                    GenerateComboPoints(investedComboPoints);
                    SubtractResource(25 * .2f);
                    result = "misses";
                    break;
                case AttackResult.Crit:
                    baseDamage = (int)((CritMod) * baseDamage);
                    SubtractResource(25);
                    SpenderTalents(investedComboPoints);
                    result = "crits";
                    break;
                default:
                    SubtractResource(25);
                    SpenderTalents(investedComboPoints);
                    result = "hits";
                    break;
            }

            int finalDamage = (int)(baseDamage * ArmorReduction());

            if (Logging)
            {
                Console.WriteLine(Math.Round(Time, 1) + ": Eviscerate {0} with {1} combo points for {2}", result, investedComboPoints, finalDamage);
            }

            return finalDamage;
        }

        public void SpenderTalents(int comboPoints)
        {
            //Ruthlessness
            if (RNG.Next(1, 101) <= 60)
            {
                GenerateComboPoints(1);
            }
            //Relentless Strikes
            if (RNG.Next(1, 101) <= 20 * comboPoints)
            {
                AddResource(25);
            }
        }

        public void ReduceCDs(float time)
        {
            TimeRemaining -= time;
            ResourceTickTime -= time;
            MhSwingRemaining -= time * (1.2f * (RemainingBladeFlurry > 0 ? 1 : 0) + 1.4f * (RemainingSliceAndDice > 0 ? 1 : 0));
            OhSwingRemaining -= time * (1.2f * (RemainingBladeFlurry > 0 ? 1 : 0) + 1.4f * (RemainingSliceAndDice > 0 ? 1 : 0));
            RemainingGarroteInterval = (RemainingGarroteInterval - time < 0) ? 0 : RemainingGarroteInterval - time;
            RemainingRuptureInterval = (RemainingRuptureInterval - time < 0) ? 0 : RemainingRuptureInterval - time;
            RemainingAdrenalineRush = (RemainingAdrenalineRush - time < 0) ? 0 : RemainingAdrenalineRush - time;
            RemainingBladeFlurry = (RemainingBladeFlurry - time < 0) ? 0 : RemainingBladeFlurry - time;
            RemainingSliceAndDice = (RemainingSliceAndDice - time < 0) ? 0 : RemainingSliceAndDice - time;
            RemainingOrcRacial = (RemainingOrcRacial - time < 0) ? 0 : RemainingOrcRacial - time;
            BladeFlurryCD = (BladeFlurryCD - time < 0) ? 0 : BladeFlurryCD - time;
            AdrenalineRushCD = (AdrenalineRushCD - time < 0) ? 0 : AdrenalineRushCD - time;
        }

        public void UpdateTime(float time)
        {
            Time += time;
            ReduceCDs(time);
        }

        public override int NextAction()
        {
            int totalDamage = 0;
            while (ResourceTickTime <= 0)
            {
                AddResource(BaseResourceGeneration * ((RemainingAdrenalineRush > 0) ? 2 : 1));
                ResourceTickTime += 2;
            }
            // Check MH
            if (!Stealth && MhSwingRemaining <= 0)
            {
                totalDamage += MhAttack();
                // Handle windfury procs
                if (WindfuryBuff && RNG.Next(1,101) <= 20)
                {
                    if (Logging)
                    {
                        Console.WriteLine(Math.Round(Time, 1) + ": Windfury proc");
                    }
                    WindfuryProc = true;
                    totalDamage += MhAttack();
                }
                MhSwingRemaining += MhSwing;
                // Cheat and put sunders here
                if (TargetSunders < 5)
                {
                    TargetSunders += 1;
                }
            }
            // Check OH
            if (!Stealth && OhSwingRemaining <= 0)
            {
                totalDamage += OhAttack();
                OhSwingRemaining += OhSwing;
            }
            // Check garrote
            if (RemainingGarroteTicks > 0 && RemainingGarroteInterval <= 0)
            {
                totalDamage += GarroteTick;
                if (Logging)
                {
                    Console.WriteLine(Math.Round(Time, 1) + ": Garrote ticks for {0}", GarroteTick);
                }
                RemainingGarroteTicks -= 1;
                RemainingGarroteInterval += 3;
            }
            // Check rupture
            if (RemainingRuptureTicks > 0 && RemainingRuptureInterval <= 0)
            {
                totalDamage += RuptureTick;
                if (Logging)
                {
                    Console.WriteLine(Math.Round(Time, 1) + ": Rupture ticks for {0}", RuptureTick);
                }
                RemainingRuptureTicks -= 1;
                RemainingRuptureInterval += 3;
            }
            // Use orc racial on CD
            if (OrcRacialCD.Equals(0))
            {
                if (Logging)
                {
                    Console.WriteLine(Math.Round(Time, 1) + ": Blood Fury");
                }
                RemainingOrcRacial += 15;
                OrcRacialCD = 120;
            }
            // Use blade flurry on CD
            if (!Stealth && BladeFlurryCD.Equals(0))
            {
                if (Logging)
                {
                    Console.WriteLine(Math.Round(Time, 1) + ": Blade Flurry");
                }
                RemainingBladeFlurry = 15;
                BladeFlurryCD = 120;
                UpdateTime(1);
            }
            // If missing at least 40 energy use adrenaline rush
            if (!Stealth && MaxResource - CurrentResource > 60 && AdrenalineRushCD.Equals(0))
            {
                if (Logging)
                {
                    Console.WriteLine(Math.Round(Time, 1) + ": Adrenaline Rush");
                }
                RemainingAdrenalineRush = 15;
                AdrenalineRushCD = 300;
                UpdateTime(1);
            }
            // Prioritize garrote from opener if allowed
            if (DotsAllowed && Stealth && CurrentResource >= 50)
            {
                Garrote();
                UpdateTime(1);
            }
            // Immediately use slice and dice not up, otherwise use at 5
            else if (RemainingSliceAndDice.Equals(0) && CurrentResource >= 25 && ComboPoints >= 1 && TimeRemaining > 10)
            {
                SliceAndDice();
                UpdateTime(1);
            }
            else if (RemainingSliceAndDice < 5 && CurrentResource >= 25 && ComboPoints.Equals(5) && TimeRemaining > 25)
            {
                SliceAndDice();
                UpdateTime(1);
            } // Check if allowed to use dots
            else if (DotsAllowed && RemainingRuptureTicks.Equals(0) && CurrentResource >= 25 && ComboPoints.Equals(5))
            {
                Rupture();
                UpdateTime(1);
            } // If you cannot use dots, ignore rupture component
            else if ((RemainingRuptureTicks > 5 || !DotsAllowed) && CurrentResource >= 25 && ComboPoints.Equals(5))
            {
                Eviscerate();
                UpdateTime(1);
            } // Check if allowed to use dots and vanish is ready
            else if (DotsAllowed && VanishCD.Equals(0) && RemainingGarroteTicks.Equals(0))
            {
                if (CurrentResource >= 50)
                {
                    if (Logging)
                    {
                        Console.WriteLine(Math.Round(Time, 1) + ": Vanish");
                    }
                    VanishCD = 300;
                    UpdateTime(1);
                    Garrote();
                    UpdateTime(1);
                }
                else
                {
                    Wait();
                }
            }
            else if (CurrentResource >= 40)
            {
                SinisterStrike();
                UpdateTime(1);
            }
            else
            {
                Wait();
            }
            return totalDamage;
        }

        public void Wait()
        {
            UpdateTime(0.5f);
        }
    }
}