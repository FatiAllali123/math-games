using System;
namespace com.glups.Reward
{
    internal static class MathHelper
    {
        private static Random randomInstance = null;

        public static double NextDouble
        {
            get
            {
                if (randomInstance == null)
                    randomInstance = new Random();

                return randomInstance.NextDouble();
            }
        }
    }
}
