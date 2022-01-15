using System;
using System.Diagnostics;
using System.IO;

namespace ClassicSim
{
    class StatWeightSims
    {
        public static void MageSimWeights(Player actor, int iterations, int statStep, int statRange, int[] durationArray)
        {
            // For progress output in console window
            int totalSteps = 0;
            foreach (int time in durationArray)
            {
                totalSteps += time;
            }
            // Number of different stats being iterated
            totalSteps = totalSteps * 6;
            int currentStep = 0;
            double simWeight;
            float baseDPS;
            double progress;
            Stopwatch sw = new Stopwatch();

            // For hit and crit
            int bonusWeightStep = 1;
            int bonusWeightRange = 5;

            // For weapon stats
            int weaponWeightStep = 1;
            int weaponWeightRange = 5;

            string date = DateTime.Now.ToString("HHmm");
            using (StreamWriter writer = new StreamWriter(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\" + actor.Name + "Stats_" + date + ".csv", true))
            //using (StreamWriter writer = new StreamWriter(@"C:\Users\Filipe\Documents\Visual Studio 2017\Projects\ClassicSim\Results\" + actor.Name + "Stats_" + date + ".csv", true))
            {
                writer.WriteLine("Duration,Intellect,Spirit,HitChance,CritChance,Spellpower,WandDPS");
                // Do all times in one batch
                foreach (int duration in durationArray)
                {
                    sw.Start();
                    // Write top line
                    writer.Write(duration + ",");

                    baseDPS = RunSimDamage(iterations, duration, actor);

                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));
                    
                    simWeight = 0;
                    // Int check
                    for (int i = statStep; i <= statRange; i += statStep)
                    {
                        actor.Intellect += i;
                        simWeight += RunSimWeights(iterations, duration, baseDPS, actor) / i;
                        actor.Intellect -= i;
                    }
                    writer.Write(Math.Round(simWeight / (statRange / statStep), 2) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // Spirit check
                    for (int i = statStep; i <= statRange; i += statStep)
                    {
                        actor.Spirit += i;
                        simWeight += RunSimWeights(iterations, duration, baseDPS, actor) / i;
                        actor.Spirit -= i;
                    }
                    writer.Write(Math.Round(simWeight / (statRange / statStep), 2) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // Hit check
                    for (int i = bonusWeightStep; i <= bonusWeightRange; i += bonusWeightStep)
                    {
                        actor.HitChance += i;
                        simWeight += RunSimWeights(iterations, duration, baseDPS, actor) / i;
                        actor.HitChance -= i;
                    }
                    writer.Write(Math.Round(simWeight / (bonusWeightRange / bonusWeightStep), 2) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // Crit check
                    for (int i = bonusWeightStep; i <= bonusWeightRange; i += bonusWeightStep)
                    {
                        actor.BaseCrit += i;
                        simWeight += RunSimWeights(iterations, duration, baseDPS, actor) / i;
                        actor.BaseCrit -= i;
                    }
                    writer.Write(Math.Round(simWeight / (bonusWeightRange / bonusWeightStep), 2) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // Spellpower check
                    for (int i = statStep; i <= statRange; i += statStep)
                    {
                        actor.FrostPower += i;
                        simWeight += RunSimWeights(iterations, duration, baseDPS, actor) / i;
                        actor.FrostPower -= i;
                    }
                    writer.Write(Math.Round(simWeight / (statRange / statStep), 2) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 1);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));
                    
                    simWeight = 0;
                    int originalMin = actor.RangedMinDamage;
                    int originalMax = actor.RangedMaxDamage;
                    // Wand damage
                    for (int i = weaponWeightStep; i <= weaponWeightRange; i += weaponWeightStep)
                    {
                        actor.RangedMinDamage += (int)Math.Floor(i * actor.RangedSwing);
                        actor.RangedMaxDamage += (int)Math.Ceiling(i * actor.RangedSwing);
                        simWeight += RunSimWeights(iterations, duration, baseDPS, actor) / i;
                        actor.RangedMinDamage = originalMin;
                        actor.RangedMaxDamage = originalMax;
                    }
                    writer.Write(Math.Round(simWeight / (weaponWeightRange / weaponWeightStep), 3) + ",");
                    currentStep += duration;

                    writer.WriteLine();
                }
            }
            sw.Stop();
            Console.WriteLine("Complete!");
        }

        public static float RunSimWeights(int iterations, int fightDuration, float baseDPS, Player actor)
        {
            float totalDamage = 0;

            float fightDamage = 0;

            for (int i = 0; i < iterations; i++)
            {
                actor.Reset(fightDuration);
                fightDamage = 0;
                while (actor.Time < fightDuration)
                {
                    fightDamage += actor.NextAction();
                }
                totalDamage += fightDamage;
            }

            float result = totalDamage / (iterations * fightDuration) - baseDPS;
            return result;
        }

        // For reference for stat weights
        public static float RunSimDamage(int iterations, int fightDuration, Player actor)
        {
            float totalDamage = 0;

            float fightDamage = 0;
            for (int i = 0; i < iterations; i++)
            {
                actor.Reset(fightDuration);
                fightDamage = 0;
                while (actor.Time < fightDuration)
                {
                    fightDamage += actor.NextAction();
                }
                totalDamage += fightDamage;
            }

            return totalDamage / (iterations * fightDuration);
        }

        public static void RogueSimWeights(Player actor, int iterations, int statStep, int statRange, int[] durationArray)
        {
            // For progress output in console window
            int totalSteps = 0;
            foreach (int time in durationArray)
            {
                totalSteps += time;
            }
            // Number of different stats being iterated
            totalSteps = totalSteps * 7;
            int currentStep = 0;
            double simWeight;
            float baseDPS;
            double progress;
            Stopwatch sw = new Stopwatch();

            // For hit and crit
            int bonusWeightStep = 1;
            int bonusWeightRange = 5;

            // For weapon stats
            int weaponWeightStep = 1;
            int weaponWeightRange = 10;

            // For weapon swing
            //float weaponSwingWeightStep = 0.1f;
            //float weaponSwingWeightRange = 0.1f;

            string date = DateTime.Now.ToString("HHmm");
            using (StreamWriter writer = new StreamWriter(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\" + actor.Name + "Stats_" + date + ".csv", true))
            //using (StreamWriter writer = new StreamWriter(@"C:\Users\Filipe\Documents\Visual Studio 2017\Projects\ClassicSim\Results\" + actor.Name + "Stats_" + date + ".csv", true))
            {
                writer.WriteLine("Duration,Strength,Agility,Hit,Crit,AttackPower,MH_DPS,OH_DPS");
                // Do all times in one batch
                foreach (int duration in durationArray)
                {
                    sw.Start();
                    // Write top line
                    writer.Write(duration + ",");

                    baseDPS = RunSimDamage(iterations, duration, actor);
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));
                    
                    simWeight = 0;
                    // Strength check
                    for (int i = statStep; i <= statRange; i += statStep)
                    {
                        actor.Strength += i;
                        simWeight += RunSimWeights(iterations, duration, baseDPS, actor) / i;
                        actor.Strength -= i;
                    }
                    writer.Write(Math.Round(simWeight / (statRange / statStep), 2) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // Agility check
                    for (int i = statStep; i <= statRange; i += statStep)
                    {
                        actor.Agility += i;
                        simWeight += RunSimWeights(iterations, duration, baseDPS, actor) / i;
                        actor.Agility -= i;
                    }
                    writer.Write(Math.Round(simWeight / (statRange / statStep), 2) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // Hit check
                    for (int i = bonusWeightStep; i <= bonusWeightRange; i += bonusWeightStep)
                    {
                        actor.HitChance += i;
                        simWeight += RunSimWeights(iterations, duration, baseDPS, actor) / i;
                        actor.HitChance -= i;
                    }
                    writer.Write(Math.Round(simWeight / (bonusWeightRange / bonusWeightStep), 2) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // Crit check
                    for (int i = bonusWeightStep; i <= bonusWeightRange; i += bonusWeightStep)
                    {
                        actor.BaseCrit += i;
                        simWeight += RunSimWeights(iterations, duration, baseDPS, actor) / i;
                        actor.BaseCrit -= i;
                    }
                    writer.Write(Math.Round(simWeight / (bonusWeightRange / bonusWeightStep), 3) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // AttackPower check
                    for (int i = statStep; i <= statRange; i += statStep)
                    {
                        actor.AttackPower += i;
                        simWeight += RunSimWeights(iterations, duration, baseDPS, actor) / i;
                        actor.AttackPower -= i;
                    }
                    writer.Write(Math.Round(simWeight / (statRange / statStep), 2) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // MH check
                    
                    int MHOriginalMin = actor.MhMinDamage;
                    int MHOriginalMax = actor.MhMaxDamage;
                    for (int i = weaponWeightStep; i <= weaponWeightRange; i += weaponWeightStep)
                    {
                        actor.MhMinDamage += (int)Math.Floor(i * actor.MhSwing);
                        actor.MhMaxDamage += (int)Math.Ceiling(i * actor.MhSwing);
                        simWeight += RunSimWeights(iterations, duration, baseDPS, actor) / i;
                        actor.MhMinDamage = MHOriginalMin;
                        actor.MhMaxDamage = MHOriginalMax;
                    }
                    writer.Write(Math.Round(simWeight / (weaponWeightRange / weaponWeightStep), 2) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // OH check
                    int OHOriginalMin = actor.OhMinDamage;
                    int OHOriginalMax = actor.OhMaxDamage;
                    for (int i = weaponWeightStep; i <= weaponWeightRange; i += weaponWeightStep)
                    {
                        actor.OhMinDamage += (int)Math.Floor(i * actor.OhSwing);
                        actor.OhMaxDamage += (int)Math.Ceiling(i * actor.OhSwing);
                        simWeight += RunSimWeights(iterations, duration, baseDPS, actor) / i;
                        actor.OhMinDamage = OHOriginalMin;
                        actor.OhMaxDamage = OHOriginalMax;
                    }
                    writer.Write(Math.Round(simWeight / (weaponWeightRange / weaponWeightStep), 2) + ",");
                    currentStep += duration;
                    /*
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // MH speed
                    for (float i = weaponSwingWeightStep; i <= weaponSwingWeightRange; i += weaponSwingWeightStep)
                    {
                        actor.MhMinDamage = (int)Math.Floor(MHOriginalMin * (actor.MhSwing + i) / actor.MhSwing);
                        actor.MhMaxDamage = (int)Math.Ceiling(MHOriginalMax * (actor.MhSwing + i) / actor.MhSwing);
                        actor.MhSwing += i;
                        simWeight += RunSimWeights(iterations, duration, baseDPS, actor) / i;
                        actor.MhSwing -= i;
                        actor.MhMinDamage = MHOriginalMin;
                        actor.MhMaxDamage = MHOriginalMax;
                    }
                    writer.Write(Math.Round(simWeight / (weaponSwingWeightRange / weaponSwingWeightStep), 3) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // OH speed
                    for (float i = weaponSwingWeightStep; i <= weaponSwingWeightRange; i += weaponSwingWeightStep)
                    {
                        actor.OhMinDamage = (int)Math.Floor(OHOriginalMin * (actor.OhSwing + i) / actor.OhSwing);
                        actor.OhMaxDamage = (int)Math.Ceiling(OHOriginalMax * (actor.OhSwing + i) / actor.OhSwing);
                        actor.OhSwing += i;
                        simWeight += RunSimWeights(iterations, duration, baseDPS, actor) / i;
                        actor.OhSwing -= i;
                        actor.OhMinDamage = OHOriginalMin;
                        actor.OhMaxDamage = OHOriginalMax;
                    }
                    writer.Write(Math.Round(simWeight / (weaponSwingWeightRange / weaponSwingWeightStep), 3) + ",");
                    currentStep += duration;
                    */
                    writer.WriteLine();
                }
            }
            sw.Stop();
            Console.WriteLine("Complete!");
        }
    }
}
