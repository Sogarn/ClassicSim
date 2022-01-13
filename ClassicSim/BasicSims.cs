using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassicSim
{
    class BasicSims
    {
        public static void RunSim(int iterations, int fightDuration, Player actor, int bonusCrit = 0)
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

            Console.WriteLine("Overall:\n Total damage: {0} \n DPS: {1}", totalDamage / iterations, totalDPS / iterations);
        }
    }
}
