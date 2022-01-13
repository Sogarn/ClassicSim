using System;
using System.Diagnostics;
using System.IO;

namespace ClassicSim
{
    class StatWeightSims
    {
        public static void SimWeights(Player actor, int iterations, int statStep, int statRange, int[] durationArray)
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
            int baseDPS;
            double progress;
            Stopwatch sw = new Stopwatch();

            string date = DateTime.Now.ToString("HHmm");
            using (StreamWriter writer = new StreamWriter(@"C:\Temp\" + actor.Name + "Stats_" + date + ".csv", true))
            {
                writer.WriteLine("Duration,Intellect,Spirit,Spellpower,CritChance,HitChance,WandDPS");
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
                        simWeight += Math.Round(RunSimWeights(iterations, duration, baseDPS, actor) / i, 3);
                        actor.Intellect -= i;
                    }
                    writer.Write(Math.Round(simWeight / (statRange / statStep), 3) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // Spirit check
                    for (int i = statStep; i <= statRange; i += statStep)
                    {
                        actor.Spirit += i;
                        simWeight += Math.Round(RunSimWeights(iterations, duration, baseDPS, actor) / i, 3);
                        actor.Spirit -= i;
                    }
                    writer.Write(Math.Round(simWeight / (statRange / statStep), 3) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // Spellpower check
                    for (int i = statStep; i <= statRange; i += statStep)
                    {
                        actor.FrostPower += i;
                        simWeight += Math.Round(RunSimWeights(iterations, duration, baseDPS, actor) / i, 3);
                        actor.FrostPower -= i;
                    }
                    writer.Write(Math.Round(simWeight / (statRange / statStep), 3) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // Crit check
                    for (int i = 1; i <= 5; i += 1)
                    {
                        actor.CritChance += i;
                        simWeight += Math.Round(RunSimWeights(iterations, duration, baseDPS, actor, i) / i, 3);
                        actor.CritChance -= i;
                    }
                    writer.Write(Math.Round(simWeight / (5 / 1), 3) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // Hit check
                    for (int i = 1; i <= 5; i += 1)
                    {
                        actor.HitChance += i;
                        simWeight += Math.Round(RunSimWeights(iterations, duration, baseDPS, actor) / i, 3);
                        actor.HitChance -= i;
                    }
                    writer.Write(Math.Round(simWeight / (5 / 1), 3) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    int originalMin = actor.RangedMinDamage;
                    int originalMax = actor.RangedMaxDamage;
                    // Wand damage
                    for (int i = 1; i <= 10; i += 1)
                    {
                        actor.RangedMinDamage += (int)Math.Floor(i * actor.RangedSwing);
                        actor.RangedMaxDamage += (int)Math.Ceiling(i * actor.RangedSwing);
                        simWeight += Math.Round(RunSimWeights(iterations, duration, baseDPS, actor) / i, 3);
                        actor.RangedMinDamage = originalMin;
                        actor.RangedMaxDamage = originalMax;
                    }
                    writer.Write(Math.Round(simWeight / (10 / 1), 3) + ",");
                    currentStep += duration;

                    writer.WriteLine();
                }
            }
            sw.Stop();
            Console.WriteLine("Complete!");
        }

        public static double RunSimWeights(int iterations, int fightDuration, int baseDPS, Player actor, int bonusCrit = 0)
        {
            int totalDamage = 0;
            int totalDPS = 0;

            int fightDamage;

            totalDamage = 0;
            totalDPS = 0;
            fightDamage = 0;
            for (int i = 0; i < iterations; i++)
            {
                actor.Reset(bonusCrit, fightDuration);
                fightDamage = 0;
                while (actor.Time < fightDuration)
                {
                    fightDamage += actor.NextAction();
                }
                totalDamage += fightDamage;
                totalDPS += fightDamage / fightDuration;
            }

            int result = (totalDPS / iterations - baseDPS);
            return result;
        }

        // For reference for stat weights
        public static int RunSimDamage(int iterations, int fightDuration, Player actor)
        {
            int totalDamage = 0;
            int totalDPS = 0;

            int fightDamage;

            totalDamage = 0;
            totalDPS = 0;
            fightDamage = 0;
            for (int i = 0; i < iterations; i++)
            {
                actor.Reset(0, fightDuration);
                fightDamage = 0;
                while (actor.Time < fightDuration)
                {
                    fightDamage += actor.NextAction();
                }
                totalDamage += fightDamage;
                totalDPS += fightDamage / fightDuration;
            }

            return totalDPS / iterations;
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
            totalSteps = totalSteps * 9;
            int currentStep = 0;
            double simWeight;
            int baseDPS;
            double progress;
            Stopwatch sw = new Stopwatch();

            string date = DateTime.Now.ToString("HHmm");
            using (StreamWriter writer = new StreamWriter(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\" + actor.Name + "Stats_" + date + ".csv", true))
            //using (StreamWriter writer = new StreamWriter(@"C:\Users\Filipe\Documents\Visual Studio 2017\Projects\ClassicSim\Results\" + actor.Name + "Stats_" + date + ".csv", true))
            {
                writer.WriteLine("Duration,Strength,Agility,AttackPower,CritChance,HitChance,MH_DPS,OH_DPS");
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
                        simWeight += Math.Round(RunSimWeights(iterations, duration, baseDPS, actor) / i, 3);
                        actor.Strength -= i;
                    }
                    writer.Write(Math.Round(simWeight / (statRange / statStep), 3) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // Agility check
                    for (int i = statStep; i <= statRange; i += statStep)
                    {
                        actor.Agility += i;
                        simWeight += Math.Round(RunSimWeights(iterations, duration, baseDPS, actor) / i, 3);
                        actor.Agility -= i;
                    }
                    writer.Write(Math.Round(simWeight / (statRange / statStep), 3) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // AttackPower check
                    for (int i = statStep; i <= statRange; i += statStep)
                    {
                        actor.AttackPower += i;
                        simWeight += Math.Round(RunSimWeights(iterations, duration, baseDPS, actor) / i, 3);
                        actor.AttackPower -= i;
                    }
                    writer.Write(Math.Round(simWeight / (statRange / statStep), 3) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));
                    
                    simWeight = 0;
                    // Crit check
                    for (int i = 1; i <= 5; i += 1)
                    {
                        actor.CritChance += i;
                        simWeight += Math.Round(RunSimWeights(iterations, duration, baseDPS, actor, i) / i, 3);
                        actor.CritChance -= i;
                    }
                    writer.Write(Math.Round(simWeight / (5 / 1), 3) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));
                    
                    simWeight = 0;
                    // Hit check
                    for (int i = 1; i <= 5; i += 1)
                    {
                        actor.HitChance += i;
                        simWeight += Math.Round(RunSimWeights(iterations, duration, baseDPS, actor) / i, 3);
                        actor.HitChance -= i;
                    }
                    writer.Write(Math.Round(simWeight / (5 / 1), 3) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // MH check
                    
                    int MHOriginalMin = actor.MhMinDamage;
                    int MHOriginalMax = actor.MhMaxDamage;
                    for (int i = 2; i <= 20; i += 2)
                    {
                        actor.MhMinDamage += (int)Math.Floor(i * actor.MhSwing);
                        actor.MhMaxDamage += (int)Math.Ceiling(i * actor.MhSwing);
                        simWeight += Math.Round(RunSimWeights(iterations, duration, baseDPS, actor) / i, 3);
                        actor.MhMinDamage = MHOriginalMin;
                        actor.MhMaxDamage = MHOriginalMax;
                    }
                    writer.Write(Math.Round(simWeight / (20 / 2), 3) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // OH check
                    int OHOriginalMin = actor.OhMinDamage;
                    int OHOriginalMax = actor.OhMaxDamage;
                    for (int i = 2; i <= 20; i += 2)
                    {
                        actor.OhMinDamage += (int)Math.Floor(i * actor.OhSwing);
                        actor.OhMaxDamage += (int)Math.Ceiling(i * actor.OhSwing);
                        simWeight += Math.Round(RunSimWeights(iterations, duration, baseDPS, actor) / i, 3);
                        actor.OhMinDamage = OHOriginalMin;
                        actor.OhMaxDamage = OHOriginalMax;
                    }
                    writer.Write(Math.Round(simWeight / (20 / 2), 3) + ",");
                    currentStep += duration;
                    
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));
                    
                    simWeight = 0;
                    // MH speed
                    for (float i = 0.1f; i <= 0.1f; i += 0.1f)
                    {
                        actor.MhMinDamage = (int)Math.Floor(MHOriginalMin * (actor.MhSwing + i) / actor.MhSwing);
                        actor.MhMaxDamage = (int)Math.Ceiling(MHOriginalMax * (actor.MhSwing + i) / actor.MhSwing);
                        actor.MhSwing += i;
                        simWeight += Math.Round(RunSimWeights(iterations, duration, baseDPS, actor) / i, 3);
                        actor.MhSwing -= i;
                        actor.MhMinDamage = MHOriginalMin;
                        actor.MhMaxDamage = MHOriginalMax;
                    }
                    writer.Write(Math.Round(simWeight / (0.1 / 0.1), 3) + ",");
                    currentStep += duration;
                    progress = Math.Round((double)currentStep / totalSteps, 3);
                    Console.WriteLine("Progress: {0}% Time Remaining: {1}s", 100 * progress, Math.Round(sw.Elapsed.TotalSeconds / progress - sw.Elapsed.TotalSeconds, 1));

                    simWeight = 0;
                    // OH speed
                    for (float i = 0.1f; i <= 0.1f; i += 0.1f)
                    {
                        actor.OhMinDamage = (int)Math.Floor(OHOriginalMin * (actor.OhSwing + i) / actor.OhSwing);
                        actor.OhMaxDamage = (int)Math.Ceiling(OHOriginalMax * (actor.OhSwing + i) / actor.OhSwing);
                        actor.OhSwing += i;
                        simWeight += Math.Round(RunSimWeights(iterations, duration, baseDPS, actor) / i, 3);
                        actor.OhSwing -= i;
                        actor.OhMinDamage = OHOriginalMin;
                        actor.OhMaxDamage = OHOriginalMax;
                    }
                    writer.Write(Math.Round(simWeight / (0.1 / 0.1), 3) + ",");
                    currentStep += duration;
                    
                    writer.WriteLine();
                }
            }
            sw.Stop();
            Console.WriteLine("Complete!");
        }
    }
}
