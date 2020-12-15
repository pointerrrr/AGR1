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

        public static (Vector3, Vector3) GetBoundingVolume(List<Primitive> primitives)
        {
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            float minZ = float.PositiveInfinity;
            float maxZ = float.NegativeInfinity;

            for (int i = 0; i < primitives.Count; i++)
            {
                (var bbMin, var bbMax) = primitives[i].BoundingBox;

                if (bbMin.X < minX)
                    minX = bbMin.X;
                if (bbMax.X > maxX)
                    maxX = bbMax.X;
                if (bbMin.Y < minY)
                    minY = bbMin.Y;
                if (bbMax.Y > maxY)
                    maxY = bbMax.Y;
                if (bbMin.Z < minZ)
                    minZ = bbMin.Z;
                if (bbMax.Z > maxZ)
                    maxZ = bbMax.Z;
            }

            return (new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
        }
    }

    
}
