using System;
using UnityEngine;

namespace O2un.Manager
{
    public static class AnnulusSampler
    {
        public static Vector3 Sample(Vector3 center, float minRadius, float maxRadius, System.Random rng)
        {
            double angle = rng.NextDouble() * 2.0 * Math.PI;
            float min2 = minRadius * minRadius;
            float max2 = maxRadius * maxRadius;
            float radius = Mathf.Sqrt(Mathf.Lerp(min2, max2, (float)rng.NextDouble()));

            float x = radius * (float)Math.Cos(angle);
            float z = radius * (float)Math.Sin(angle);
            return center + new Vector3(x, 0f, z);
        }
    }
}
