using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using static OpenTK.Vector3;

namespace template
{
    public static class GlobalLib
    {
        public static float Epsilon = 0.00001f;
        public static int MaxRecursion = 10;
        public static int SamplesPerFrame = 5;
        public static int AA = 4;
        public static float FOV = 120;
        public static int Width = 512, Height = 512;

        public static Vector3 ReflectRay(Vector3 rayDirection, Vector3 normal)
        {
            return rayDirection - 2 * Dot(rayDirection, normal) * normal;
        }

        public static int VecToInt(Vector3 vector)
        {
            int R = vector.X > 1 ? 255 : (int)(vector.X * 255);
            int G = vector.Y > 1 ? 255 : (int)(vector.Y * 255);
            int B = vector.Z > 1 ? 255 : (int)(vector.Z * 255);
            return (R << 16) + (G << 8) + B;
        }
    }
}
