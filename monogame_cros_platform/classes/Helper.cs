using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace monogame_cros_platform.classes
{
    static class Helper
    {
        static double radiantsPerDegree = Math.PI / 180;
        public static double FromDegreesToRadiants(double degree)
        {
            return radiantsPerDegree * degree;   
        }

        public static Vector2 direction(double angle)
        {
            return new Vector2((float)Math.Cos(Helper.FromDegreesToRadiants(angle)), (float)Math.Sin(Helper.FromDegreesToRadiants(angle)));
        }

        public static Vector2 between(Vector2 a, Vector2 b)
        {
            return (a + b) / 2;
        }

        public static float intensity(Vector2 a, Vector2 b)
        {
            float c1 = Vector2.Dot(a,b);
            float c2 = Vector2.Dot(b,b);
            return c1 / c2;
        }

        public static Vector2[] multiplyAll(Vector2[] v3, float c)
        {
            Vector2[] r = new Vector2[v3.Length];
            for (int i=0;i<v3.Length;i++)
            {
                Vector2 v = v3[i];
                r[i] = new Vector2(v.X, v.Y) * c;
            }
            return r;
        }

        public static Vector2[] addAll(Vector2[] v3, Vector2 c)
        {
            Vector2[] r = new Vector2[v3.Length];
            for (int i = 0; i < v3.Length; i++)
            {
                Vector2 v = v3[i];
                r[i] = new Vector2(v.X, v.Y) + c;
            }
            return r;
        }
    }


}
