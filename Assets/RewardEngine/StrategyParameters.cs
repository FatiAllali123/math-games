using UnityEngine;
using System;
namespace com.glups.Reward
{
    public class StrategyParameters
    {
        // 6 ranks, and the minimum score associated with each rank
        public int[] theRankSteps = new int[] { 1, 50, 120, 200, 320, 360 };

        public int theFixedSchedule = 10;
        private int theMinVariableSchedule = 7;
        private int theMaxVariableSchedule = 13;

        public float theFixedIncentiveRatio = (float)0.2;

        internal virtual int computeVariableSchedule()
        {

            int schedule = theMinVariableSchedule + (int)Math.Floor(MathHelper.NextDouble * (theMaxVariableSchedule - theMinVariableSchedule));

            Debug.Log(">> Compute Variable Schedule: " + schedule);

            return schedule;
        }

        internal virtual float computeVariableIncentive(int errors)
        {
            return (float)(errors * MathHelper.NextDouble * theFixedIncentiveRatio * 2);
        }
    }

}
