using System;
using UnityEngine;
using NFP;

namespace BlockAlloc
{
    public static class Extension2
    {
        public static bool EqualTo(this Vector2 a, Vector2 b)
        {
            double dis;

            dis = Math.Sqrt(Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2));

            if (dis < 2.0E-4) { return true; }
            else { return false; }
        }
        public static float Dis(this Vector2 a, Vector2 b)
        {
            float dis;

            dis = (a.x - b.x)* (a.x - b.x) + (a.y - b.y)* (a.y - b.y);

            return dis;
        }
        public static double Dis(this VERTEX a, VERTEX b)
        {
            double dis;

            dis = (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);

            return dis;
        }
        public static double Dis(this VERTEX a, Vector2 b)
        {
            double dis;

            dis = (a.X - b.x) * (a.X - b.x) + (a.Y - b.y) * (a.Y - b.y);

            return dis;
        }
        public static Vector2 Move(this Vector2 a, float dx, float dy)
        {
            a.x += dx;
            a.y += dy;

            return new Vector2(a.x, a.y);
        }
    }
}