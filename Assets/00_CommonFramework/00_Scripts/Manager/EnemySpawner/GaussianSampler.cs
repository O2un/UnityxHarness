using System;

namespace O2un.Manager
{
    public static class GaussianSampler
    {
        public static float Sample(Random rng, float mean, float stdDev)
        {
            double u1 = 1.0 - rng.NextDouble();
            double u2 = 1.0 - rng.NextDouble();
            double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            return mean + stdDev * (float)z;
        }
    }
}
