using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassicSim
{
    class BasicSims
    {
        public static void RunSim(int iterations, int fightDuration, Player actor, bool textLog = false)
        {
            float totalDamage = 0;
            float totalDPS = 0;

            int fightDamage;

            totalDamage = 0;
            totalDPS = 0;
            fightDamage = 0;
            if (actor.Logging)
            {
                if (File.Exists(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\SimOutput.txt"))
                {
                    File.Delete(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\SimOutput.txt");
                }
            }
            for (int i = 0; i < iterations; i++)
            {
                actor.Reset(fightDuration);
                fightDamage = 0;
                while (actor.Time < fightDuration)
                {
                    fightDamage += actor.NextAction();
                }
                totalDamage += fightDamage;
                totalDPS += fightDamage / fightDuration;
            }
            if (actor.Logging)
            {
                using (StreamWriter writer = new StreamWriter(@"C:\Users\Sogarn\Source\Repos\ClassicSim\Results\SimOutput.txt", true))
                {
                    writer.WriteLine("Overall:\n Total damage: {0} \n DPS: {1}", totalDamage / iterations, totalDPS / iterations);
                }
            }

            Console.WriteLine("Overall:\n Total damage: {0} \n DPS: {1}", totalDamage / iterations, totalDPS / iterations);
        }
    }
}
