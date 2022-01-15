using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassicSim
{
    abstract class Player
    {
        public float Time;
        public float TimeRemaining;
        public string Name;
        public bool IntellectBuff;
        public bool SpiritBuff;
        public bool WildBuff;
        public bool StrengthOfEarthBuff;
        public bool GraceOfAirBuff;
        public bool WindfuryBuff;
        public bool WindfuryProc;
        public bool ElementsBuff;
        public int TargetSunders;
        public int TargetArmor;
        public int Agility;
        public int Strength;
        public int Intellect;
        public int Spirit;
        public int AttackPower;
        public int FinalAttackPower;
        public int HitChance;
        public float BaseCrit;
        public float CritChance;
        public float CritMod;
        public int SpellPower;
        public int FrostPower;
        public int WeaponSkill;
        public int TargetDefenseSkill;
        public int MhMinDamage;
        public int MhMaxDamage;
        public float MhSwing;
        public float MhSwingRemaining;
        public int OhMinDamage;
        public int OhMaxDamage;
        public float OhSwing;
        public float OhSwingRemaining;
        public int RangedMinDamage;
        public int RangedMaxDamage;
        public float RangedSwing;
        public float ResourceTickTime;
        public int MaxResource;
        public float LastAction;
        public float CurrentResource;
        public int ManaPerFive;
        public float ManaPerFiveTickTime;
        public float BaseResourceGeneration;
        public float CombatResourceGeneration;
        public bool DotsAllowed;
        protected Random RNG;

        public bool Logging = false;

        public Player()
        {
            Time = 0;
            ResourceTickTime = 2;
            IntellectBuff = false;
            SpiritBuff = false;
            StrengthOfEarthBuff = false;
            GraceOfAirBuff = false;
            WindfuryBuff = false;
            WildBuff = false;
            ElementsBuff = false;
            LastAction = 0f;
            RNG = new Random();
            MaxResource = 0;
            CritMod = 0;
            BaseResourceGeneration = 0;
            CombatResourceGeneration = 0;
        }

        public abstract void Reset(float timeRemaining = 0);
        public abstract int NextAction();

        public void RefillResource()
        {
            CurrentResource = MaxResource;
        }

        public bool RollHit()
        {
            return RNG.Next(1, 101) <= HitChance;
        }

        public bool RollCrit(float tempCrit = 0)
        {
            return RNG.Next(1, 101) <= (CritChance + tempCrit);
        }

        public AttackResult RollHitAbility(int bonusHit = 0)
        {
            int missChance;
            if ((WeaponSkill - TargetDefenseSkill) <= 10)
            {
                missChance = (int)Math.Ceiling(7 + (TargetDefenseSkill - WeaponSkill) * .1) - bonusHit;
            }
            else
            {
                missChance = (int)Math.Ceiling(7 + (TargetDefenseSkill - WeaponSkill - 10) * .4) - bonusHit;
            }
            if (missChance < 0)
            {
                missChance = 0;
            }

            int roll = RNG.Next(1, 101);
            // 6.5% dodge baked into miss
            if (roll <= missChance + 6.5)
            {
                return AttackResult.Miss;
            }
            else if (roll - missChance - 6.5 <= CritChance)
            {
                return AttackResult.Crit;
            }
            else
            {
                return AttackResult.Hit;
            }
        }

        public DualWieldAttackResult RollHitDualWield(int bonusHit = 0)
        {
            int missChance;
            if ((WeaponSkill - TargetDefenseSkill) <= 10)
            {
                missChance = (int)Math.Ceiling(24 + (TargetDefenseSkill - WeaponSkill) * .1) - bonusHit;
            }
            else
            {
                missChance = (int)Math.Ceiling(24 + (TargetDefenseSkill - WeaponSkill - 10) * .4) - bonusHit;
            }
            if (missChance < 0)
            {
                missChance = 0;
            }

            // Glancing blows ignores weapon skill past your level * 5
            int glanceChance = 10 + 2 * (TargetDefenseSkill - (WeaponSkill > 300 ? 300 : WeaponSkill));
            // It does one roll and goes miss -> glancing -> crit -> defaults to hit

            int roll = RNG.Next(1, 101);

            // 6.5% dodge baked into miss
            if (roll <= missChance + 6.5)
            {
                return DualWieldAttackResult.Miss;
            }
            else if (roll - missChance - 6.5 <= glanceChance)
            {
                return DualWieldAttackResult.Glancing;
            }
            // -3% chance to crit
            else if (roll - missChance - glanceChance - 6.5 <= CritChance + 3)
            {
                return DualWieldAttackResult.Crit;
            }
            else
            {
                return DualWieldAttackResult.Hit;
            }
        }

        public void AddResource(float resource)
        {
            CurrentResource = (CurrentResource > MaxResource) ? MaxResource : CurrentResource + resource;
        }

        public void SubtractResource(float cost)
        {
            CurrentResource = (CurrentResource - cost < 0) ? 0 : CurrentResource - cost;
        }

        public double ArmorReduction()
        {
            double reduction = 1 - (TargetArmor - TargetSunders * 450) / ((TargetArmor - TargetSunders * 450) - 22167.5 + 467.5 * 60);
            if (reduction < 0.25)
            {
                return 0.25;
            }
            else if (reduction > 1)
            {
                return 1;
            }
            return reduction;
        }

        public enum AttackResult { Miss, Crit, Hit }
        public enum DualWieldAttackResult { Miss, Glancing, Crit, Hit }
    }
}
